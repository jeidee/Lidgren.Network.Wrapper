using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Message;
using System.Threading;

namespace Chat
{
    class ChatClient
    {
        const int kHeartbeatInterval = 500;

        MessageClient net_client = new MessageClient();
        C2S.Proxy c2s_proxy = new C2S.Proxy();
        S2C.Stub s2c_stub = new S2C.Stub();
        bool is_login = false;
        string id = "";

        Timer heartbeat_timer;

        public bool IsLogin()
        {
            return is_login;
        }

        public void Init(string app_identifier)
        {
            net_client.AttachStub(s2c_stub);
            net_client.AttachProxy(c2s_proxy);

            net_client.Init(app_identifier, false, false);

            heartbeat_timer = new Timer(e => {

                if (is_login && net_client != null && net_client.connection != null)
                {
                    //Console.WriteLine("Send Heartbeat...[ThreadID:{0}]", Thread.CurrentThread.ManagedThreadId);
                    c2s_proxy.Heartbeat(net_client.connection);
                }

            }, null, 0, kHeartbeatInterval);

            s2c_stub.OnResLogin += new S2C.Stub.ResLoginDelegate(s2c_stub_OnResLogin);
            s2c_stub.OnResSend += new S2C.Stub.ResSendDelegate(s2c_stub_OnResSend);
            s2c_stub.OnResSendAll += new S2C.Stub.ResSendAllDelegate(s2c_stub_OnResSendAll);
            s2c_stub.OnNotifyLogin += new S2C.Stub.NotifyLoginDelegate(s2c_stub_OnNotifyLogin);
            s2c_stub.OnNotifyLogout += new S2C.Stub.NotifyLogoutDelegate(s2c_stub_OnNotifyLogout);
            s2c_stub.OnNotifySend += new S2C.Stub.NotifySendDelegate(s2c_stub_OnNotifySend);
            s2c_stub.OnNotifySendAll += new S2C.Stub.NotifySendAllDelegate(s2c_stub_OnNotifySendAll);
        }

        void s2c_stub_OnNotifySendAll(Lidgren.Network.NetIncomingMessage im, S2C.Message.NotifySendAll data)
        {
            Console.WriteLine("OnNotifySendAll...[ThreadID:{0}]", Thread.CurrentThread.ManagedThreadId);

            if (data.from_id == id)
                return;

            Console.WriteLine("[{0}] {1}", data.from_id, data.message);
        }

        void s2c_stub_OnResSendAll(Lidgren.Network.NetIncomingMessage im, S2C.Message.ResSendAll data)
        {
            if (data.ret == (short)S2C.Message.Flag.kFlagFail)
            {
                Console.WriteLine("Sending message to all was failed. Result = [{0}:{1}].", data.ret, data.ret_message);
            }
        }

        void s2c_stub_OnResSend(Lidgren.Network.NetIncomingMessage im, S2C.Message.ResSend data)
        {
            if (data.ret == (short)S2C.Message.Flag.kFlagFail)
            {
                Console.WriteLine("Sending message to {0} was failed. Result = [{1}:{2}].", data.to_id, data.ret, data.ret_message);
            }
        }

        void s2c_stub_OnNotifySend(Lidgren.Network.NetIncomingMessage im, S2C.Message.NotifySend data)
        {
            if (data.from_id == id)
                return;
            Console.WriteLine("[{0}] {1}", data.from_id, data.message);
        }

        void s2c_stub_OnNotifyLogout(Lidgren.Network.NetIncomingMessage im, S2C.Message.NotifyLogout data)
        {
            Console.WriteLine("{0} leaved out.", data.logout_id);
            if (data.logout_id == this.id)
            {
                Close();
            }            
        }

        void s2c_stub_OnNotifyLogin(Lidgren.Network.NetIncomingMessage im, S2C.Message.NotifyLogin data)
        {
            Console.WriteLine("OnNotifyLogin...[ThreadID:{0}]", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("{0} entered in.", data.new_id);
        }

        void s2c_stub_OnResLogin(Lidgren.Network.NetIncomingMessage im, S2C.Message.ResLogin data)
        {
            Console.WriteLine("OnResLogin...[ThreadID:{0}]", Thread.CurrentThread.ManagedThreadId);

            is_login = data.ret == (short)S2C.Message.Flag.kFlagSuccess ? true : false;

            if (is_login)
            {
                Console.WriteLine("You entered in.", data.ret);
            }
            else
            {
                Console.WriteLine("You can't entered in.", data.ret);
            }
        }

        public void Connect(string server_ip, int server_port)
        {
            net_client.Connect(server_ip, server_port);
            Thread.Sleep(1000);
        }

        public void Close()
        {
            is_login = false;
            net_client.Stop();
            net_client.Join();
        }

        public void Login(string id)
        {
            Console.WriteLine("Post Login...[ThreadID:{0}]", Thread.CurrentThread.ManagedThreadId);

            SynchronizationContext.Current.Post(e =>
            {
                Console.WriteLine("Exec Login...[ThreadID:{0}]", Thread.CurrentThread.ManagedThreadId);

                if (is_login)
                {
                    Console.WriteLine("You are already logged in.");
                    return;
                }

                this.id = id;
                List<int> ages = new List<int>();
                if (!c2s_proxy.ReqLogin(net_client.connection, id))
                {
                    Console.WriteLine("Request was refused. Check your connections.");
                }
            
            }, this);
        }

        public void Logout()
        {
            if (!is_login)
            {
                Console.WriteLine("You should log in first.");
                return;
            }

            if (!c2s_proxy.ReqLogout(net_client.connection, id))
            {
                Console.WriteLine("Request was refused. Check your connections.");
            }

            //Close();
        }

        public void Send(string to_id, string message)
        {
            if (!is_login)
            {
                Console.WriteLine("You should log in first.");
                return;
            }

            if (!c2s_proxy.ReqSend(net_client.connection, to_id, message))
            {
                Console.WriteLine("Request was refused. Check your connections.");
            }
        }

        public void SendToAll(string message)
        {
            Console.WriteLine("SendToAll...[ThreadID:{0}]", Thread.CurrentThread.ManagedThreadId);

            if (!is_login)
            {
                Console.WriteLine("You should log in first.");
                return;
            }

            if(!c2s_proxy.ReqSendAll(net_client.connection, message))
            {
                Console.WriteLine("Request was refused. Check your connections.");
            }
        }

    }
}
