using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Message;
using Lidgren.Network;
using System.Threading;

namespace Chat
{
    class ConnectedUser
    {
        public NetConnection connection;
        public string id;
        public Int64 last_heartbeat_ticks;

        public void Disconnect(string goodbye)
        {
            if (connection == null)
            {
                return;
            }

            connection.Disconnect(goodbye);
        }
    }

    class ChatServer
    {
        const int kTimeoutConnection = 1000;
        const int kServerTickInterval = 1000;

        MessageServer net_server = new MessageServer();

        C2S.Stub c2s_stub = new C2S.Stub();
        S2C.Proxy s2c_proxy = new S2C.Proxy();

        Dictionary<NetConnection, ConnectedUser> client_list = new Dictionary<NetConnection, ConnectedUser>();
        Dictionary<string, ConnectedUser> idx_id_for_client_list = new Dictionary<string, ConnectedUser>();

        public void Init(string app_identifier, int server_port, int max_client)
        {
            net_server.AttachStub(c2s_stub);
            net_server.AttachProxy(s2c_proxy);

            net_server.Init(app_identifier, server_port, max_client, kServerTickInterval);
            net_server.OnTick += new Action(net_server_OnTick);

            c2s_stub.OnReqLogin += new C2S.Stub.ReqLoginDelegate(c2s_stub_OnReqLogin);
            c2s_stub.OnReqLogout += new C2S.Stub.ReqLogoutDelegate(c2s_stub_OnReqLogout);
            c2s_stub.OnReqSend += new C2S.Stub.ReqSendDelegate(c2s_stub_OnReqSend);
            c2s_stub.OnHeartbeat += new C2S.Stub.HeartbeatDelegate(c2s_stub_OnHeartbeat);
            c2s_stub.OnReqSendAll += new C2S.Stub.ReqSendAllDelegate(c2s_stub_OnReqSendAll);
        }

        void net_server_OnTick()
        {
            List<NetConnection> remove_list = new List<NetConnection>();

            foreach (var client in client_list.Values)
            {
                long diff_ms = (DateTime.Now.Ticks - client.last_heartbeat_ticks) / TimeSpan.TicksPerMillisecond;
                if (diff_ms >= kTimeoutConnection)
                {
                    remove_list.Add(client.connection);
                    idx_id_for_client_list.Remove(client.id);
                }
            }

            foreach (var connection in remove_list)
            {
                client_list.Remove(connection);
            }
        }

        void c2s_stub_OnReqSendAll(NetIncomingMessage im, C2S.Message.ReqSendAll data)
        {
            ConnectedUser user = client_list[im.SenderConnection];
            if (user == null)
            {
                im.SenderConnection.Disconnect("You are not logged in.");
                return;
            }

            foreach (var client in client_list.Values)
            {
                s2c_proxy.NotifySendAll(client.connection, user.id, data.message);
            }

            s2c_proxy.ResSendAll(user.connection, (short)S2C.Message.Flag.kFlagSuccess, "Success");

            Console.WriteLine("{0} sent a message[{1}] to all from {2}.", user.id, data.message, im.SenderEndPoint.ToString());                        
        }

        void c2s_stub_OnHeartbeat(NetIncomingMessage im, C2S.Message.Heartbeat data)
        {
            ConnectedUser user = client_list[im.SenderConnection];
            if (user == null)
            {
                im.SenderConnection.Disconnect("You are not logged in.");
                return;
            }
            user.last_heartbeat_ticks = DateTime.Now.Ticks;
        }

        void c2s_stub_OnReqSend(NetIncomingMessage im, C2S.Message.ReqSend data)
        {
            ConnectedUser user = client_list[im.SenderConnection];
            if (user == null)
            {
                im.SenderConnection.Disconnect("You are not logged in.");
                return;
            }

            ConnectedUser to_user = idx_id_for_client_list[data.to_id];
            if (to_user == null)
            {
                s2c_proxy.ResSend(user.connection, (short)S2C.Message.Flag.kFlagFail, "Can't find user.", data.to_id);
                return;
            }

            s2c_proxy.NotifySend(to_user.connection, user.id, data.message);
            s2c_proxy.ResSend(user.connection, (short)S2C.Message.Flag.kFlagSuccess, "Success", data.to_id);

            //Console.WriteLine("{0} sent a message[{1}] to {2} from {3}.", user.id, data.message, data.to_id, im.SenderEndPoint.ToString());
        }

        void c2s_stub_OnReqLogout(NetIncomingMessage im, C2S.Message.ReqLogout data)
        {
            ConnectedUser user = client_list[im.SenderConnection];
            if (user == null)
            {
                user = idx_id_for_client_list[data.id];
            }
            if (user == null)
            {
                im.SenderConnection.Disconnect("You are not logged in.");
                return;
            }

            client_list.Remove(im.SenderConnection);
            idx_id_for_client_list.Remove(data.id);

            if (user.connection != null)
            {
                s2c_proxy.NotifyLogout(user.connection, user.id);
            }

            foreach (var client in client_list.Values)
            {
                s2c_proxy.NotifyLogout(client.connection, user.id);
            }

            Console.WriteLine("{0} logout from {1}.", user.id, im.SenderEndPoint.ToString());
        }

        void c2s_stub_OnReqLogin(NetIncomingMessage im, C2S.Message.ReqLogin data)
        {
            if (client_list.ContainsKey(im.SenderConnection))
            {
                Console.WriteLine("{0} has already logged in.", data.id);
                return;
                //client_list[im.SenderConnection].Disconnect("Try to login from another site. You will be disconnected from server.");
            }
            if (idx_id_for_client_list.ContainsKey(data.id))
            {
                Console.WriteLine("{0} has already logged in from {1}.", data.id, idx_id_for_client_list[data.id].connection.RemoteEndPoint.ToString());
                return;
                //client_list[im.SenderConnection].Disconnect("Try to login from another site. You will be disconnected from server.");
            }
            ConnectedUser user = new ConnectedUser();
            user.connection = im.SenderConnection;
            user.id = data.id;
            user.last_heartbeat_ticks = DateTime.Now.Ticks;

            client_list[im.SenderConnection] = user;
            idx_id_for_client_list[data.id] = user;

            s2c_proxy.ResLogin(im.SenderConnection, (short)S2C.Message.Flag.kFlagSuccess);

            foreach (var client in client_list.Values)
            {
                if (client.connection == im.SenderConnection) continue;
                s2c_proxy.NotifyLogin(client.connection, data.id);
            }

            Console.WriteLine("{0} login from {1}", data.id, im.SenderEndPoint.ToString());
        }

        public void Start()
        {
            net_server.Start();
            Thread.Sleep(10);
        }

        public void Stop()
        {
            net_server.Stop();
            net_server.Join();
        }
    }
}
