using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.ComponentModel;
using System.Threading;
using Lidgren;
using ManagedHelpers.Threads;

namespace Lidgren.Message
{
    public class MessageClient
    {
        string app_identifier;
        NetClient net_client;
        BackgroundWorker net_worker;
        bool is_running = false;
        bool is_done = false;
        Dictionary<UInt32, RpcStubInfo> stub_handlers = new Dictionary<UInt32, RpcStubInfo>();
        List<MessageStub> stub_list = new List<MessageStub>();
        List<MessageProxy> proxy_list = new List<MessageProxy>();
        public NetConnection connection { get; private set; }

        public MessageClient()
        {
        }


        public bool AttachStub(MessageStub stub)
        {
            if (stub.AttachRpcStub(stub_handlers))
            {
                stub_list.Add(stub);
                return true;
            }

            return false;
        }


        public bool AttachProxy(MessageProxy proxy)
        {
            if (proxy_list.Find(e => e == proxy) == null)
            {
                proxy_list.Add(proxy);
                return true;
            }

            return false;
        }


        public void Init(string app_identifier, bool auth_flush_send_queue, bool use_multi_thread)
        {
            this.app_identifier = app_identifier;

            NetPeerConfiguration config = new NetPeerConfiguration(this.app_identifier);
            config.AutoFlushSendQueue = auth_flush_send_queue;

            net_client = new NetClient(config);

            if (SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(new STASynchronizationContext());
            }

            if (use_multi_thread == false && SynchronizationContext.Current != null)
            {
                net_client.RegisterReceivedCallback(new SendOrPostCallback(GotMessage));
            }
            else
            {
                net_worker = new BackgroundWorker();
                net_worker.DoWork += new DoWorkEventHandler(net_worker_DoWork);
                net_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(net_worker_RunWorkerCompleted);
            }
        }


        public void Connect(string host, int port)
        {
            proxy_list.ForEach(e => { e.SetNetwork(net_client); });

            Console.WriteLine("{0} client start...", this.app_identifier);

            net_client.Start();
            connection = net_client.Connect(host, port);
            if (net_worker != null)
            {
                net_worker.RunWorkerAsync();
            }
        }


        public void Stop()
        {
            if (!is_running)
            {
                is_done = true;
            }
            else
            {
                is_running = false;
            }
            if (net_worker != null)
            {
                net_worker.CancelAsync();
            }
        }


        public void Join()
        {
            while (!is_done)
            {
                Thread.Sleep(1);
            }
            net_client.Shutdown("bye");
            Console.WriteLine("{0} client is stopped!", this.app_identifier);
        }


        void GotMessage(object peer)
        {
            try
            {
                NetIncomingMessage im;
                while ((im = net_client.ReadMessage()) != null)
                {
                    if (im.MessageType == NetIncomingMessageType.Data)
                        ProcessMessage(im);
                    else
                    {
                        string text = "[CLIENT]" + im.ReadString();
                        Console.WriteLine(text);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        void net_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            is_running = true;
            is_done = false;
            while (is_running)
            {
                GotMessage(null);
                Thread.Sleep(1);
            }
            Console.WriteLine("{0} server is stopping...", this.app_identifier);
        }


        void net_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            is_done = true;
        }


        void ProcessMessage(NetIncomingMessage im)
        {
            UInt32 message_id = im.ReadUInt32();

            if (!stub_handlers.ContainsKey(message_id))
            {
                Console.WriteLine("[CLIENT]Unknown message id {0}", message_id);
                return;
            }

            stub_handlers[message_id].Call(im);
        }
    }
}
