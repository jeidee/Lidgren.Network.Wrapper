using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using Lidgren;
using ManagedHelpers.Threads;

namespace Lidgren.Message
{
    public class MessageServer
    {
        string app_identifier;
        NetServer net_server;
        BackgroundWorker net_worker;
        bool is_running = false;
        bool is_done = false;
        Dictionary<UInt32, RpcStubInfo> message_handlers = new Dictionary<UInt32, RpcStubInfo>();
        List<MessageStub> stub_list = new List<MessageStub>();
        List<MessageProxy> proxy_list = new List<MessageProxy>();

        int interval_ms = 0;
        public event Action OnTick;
        Timer tick_timer = null;

        public MessageServer()
        {
        }


        public void AttachStub(MessageStub stub)
        {
            if (stub.AttachRpcStub(message_handlers))
            {
                stub_list.Add(stub);
            }
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


        public void Init(string app_identifier, int listen_port, int max_connections, int interval_ms, bool use_multi_thread)
        {
            this.app_identifier = app_identifier;
            this.interval_ms = interval_ms;

            NetPeerConfiguration config = new NetPeerConfiguration(this.app_identifier);
            config.MaximumConnections = max_connections;
            config.Port = listen_port;

            net_server = new NetServer(config);

            if (SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(new STASynchronizationContext());
            }

            if (use_multi_thread == false && SynchronizationContext.Current != null)
            {
                net_server.RegisterReceivedCallback(new SendOrPostCallback(GotMessage));
            }
            else
            {
                net_worker = new BackgroundWorker();
                net_worker.DoWork += new DoWorkEventHandler(net_worker_DoWork);
                net_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(net_worker_RunWorkerCompleted);
            }

            tick_timer = new Timer(e =>
            {
                try
                {
                    if (OnTick != null && use_multi_thread && SynchronizationContext.Current != null)
                    {
                        SynchronizationContext.Current.Post(x =>
                        {
                            OnTick();
                        }, null);

                    }
                    else
                    {
                        OnTick();
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Tick timer exception = {0}", ex.Message);
                }
            });
        }

        
        public void Start()
        {
            proxy_list.ForEach(e => { e.SetNetwork(net_server); });

            Console.WriteLine("{0} server start...", this.app_identifier);

            net_server.Start();
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
            net_server.Shutdown("bye");
            Console.WriteLine("{0} server is stopped!", this.app_identifier);
        }


        void GotMessage(object peer)
        {
            try
            {
                NetIncomingMessage im;
                while ((im = net_server.ReadMessage()) != null)
                {
                    if (im.MessageType == NetIncomingMessageType.Data)
                        ProcessMessage(im);
                    else
                    {
                        string text = "[SERVER]" + im.ReadString();
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

            if (!message_handlers.ContainsKey(message_id))
            {
                Console.WriteLine("[SERVER]Unknown message id {0}", message_id);
                return;
            }

            message_handlers[message_id].Call(im);
        }
    }
}
