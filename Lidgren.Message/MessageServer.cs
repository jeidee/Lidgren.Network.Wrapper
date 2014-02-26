using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using Lidgren;

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


        public void Init(string app_identifier, int listen_port, int max_connections, int interval_ms)
        {
            this.app_identifier = app_identifier;
            this.interval_ms = interval_ms;

            NetPeerConfiguration config = new NetPeerConfiguration(this.app_identifier);
            config.MaximumConnections = max_connections;
            config.Port = listen_port;

            net_server = new NetServer(config);
            net_worker = new BackgroundWorker();
            net_worker.DoWork += new DoWorkEventHandler(net_worker_DoWork);
            net_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(net_worker_RunWorkerCompleted);
        }

        
        public void Start()
        {
            proxy_list.ForEach(e => { e.SetNetwork(net_server); }); 
            
            net_server.Start();
            net_worker.RunWorkerAsync();
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
        }


        public void Join()
        {
            while (!is_done)
            {
                Thread.Sleep(1);
            }
        }

        void net_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("{0} server start...", this.app_identifier);

            is_running = true;
            is_done = false;

            long last_tick = DateTime.Now.Ticks;
            while (is_running)
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

                if (interval_ms > 0 && OnTick != null)
                {
                    long diff_ms = (DateTime.Now.Ticks - last_tick) / TimeSpan.TicksPerMillisecond;
                    if (diff_ms >= interval_ms)
                    {
                        last_tick = DateTime.Now.Ticks;
                        try
                        {
                            OnTick();
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                Thread.Sleep(1);
            }

            Console.WriteLine("{0} server is stopping...", this.app_identifier);
        }

        
        void net_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("{0} server is stopped!", this.app_identifier);
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
