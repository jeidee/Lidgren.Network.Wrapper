using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren;
using Lidgren.Network;
using Lidgren.Message;
namespace C2S
{
	public class Stub : MessageStub
	{
		public const int Version = 100;

		public delegate void ReqLoginDelegate(NetIncomingMessage im, C2S.Message.ReqLogin data);
		public event ReqLoginDelegate OnReqLogin;
		[RpcStubAttribute(100)]
		public virtual C2S.Message.ReqLogin ReqLogin(NetIncomingMessage im)
		{
			Message.ReqLogin data = new Message.ReqLogin();
			data.id = im.ReadString();
			if(OnReqLogin != null) OnReqLogin(im, data);

			return data;
		}
		public delegate void ReqLogoutDelegate(NetIncomingMessage im, C2S.Message.ReqLogout data);
		public event ReqLogoutDelegate OnReqLogout;
		[RpcStubAttribute(101)]
		public virtual C2S.Message.ReqLogout ReqLogout(NetIncomingMessage im)
		{
			Message.ReqLogout data = new Message.ReqLogout();
			data.id = im.ReadString();
			if(OnReqLogout != null) OnReqLogout(im, data);

			return data;
		}
		public delegate void HeartbeatDelegate(NetIncomingMessage im, C2S.Message.Heartbeat data);
		public event HeartbeatDelegate OnHeartbeat;
		[RpcStubAttribute(102)]
		public virtual C2S.Message.Heartbeat Heartbeat(NetIncomingMessage im)
		{
			Message.Heartbeat data = new Message.Heartbeat();
			if(OnHeartbeat != null) OnHeartbeat(im, data);

			return data;
		}
		public delegate void ReqSendDelegate(NetIncomingMessage im, C2S.Message.ReqSend data);
		public event ReqSendDelegate OnReqSend;
		[RpcStubAttribute(103)]
		public virtual C2S.Message.ReqSend ReqSend(NetIncomingMessage im)
		{
			Message.ReqSend data = new Message.ReqSend();
			data.to_id = im.ReadString();
			data.message = im.ReadString();
			if(OnReqSend != null) OnReqSend(im, data);

			return data;
		}
		public delegate void ReqSendAllDelegate(NetIncomingMessage im, C2S.Message.ReqSendAll data);
		public event ReqSendAllDelegate OnReqSendAll;
		[RpcStubAttribute(104)]
		public virtual C2S.Message.ReqSendAll ReqSendAll(NetIncomingMessage im)
		{
			Message.ReqSendAll data = new Message.ReqSendAll();
			data.message = im.ReadString();
			if(OnReqSendAll != null) OnReqSendAll(im, data);

			return data;
		}
	}
}
