using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren;
using Lidgren.Network;
using Lidgren.Message;
namespace S2C
{
	public class Stub : MessageStub
	{
		public const int Version = 100;

		public delegate void ResLoginDelegate(NetIncomingMessage im, S2C.Message.ResLogin data);
		public event ResLoginDelegate OnResLogin;
		[RpcStubAttribute(200)]
		public virtual S2C.Message.ResLogin ResLogin(NetIncomingMessage im)
		{
			Message.ResLogin data = new Message.ResLogin();
			data.ret = im.ReadInt16();
			if(OnResLogin != null) OnResLogin(im, data);

			return data;
		}
		public delegate void NotifyLogoutDelegate(NetIncomingMessage im, S2C.Message.NotifyLogout data);
		public event NotifyLogoutDelegate OnNotifyLogout;
		[RpcStubAttribute(201)]
		public virtual S2C.Message.NotifyLogout NotifyLogout(NetIncomingMessage im)
		{
			Message.NotifyLogout data = new Message.NotifyLogout();
			data.logout_id = im.ReadString();
			if(OnNotifyLogout != null) OnNotifyLogout(im, data);

			return data;
		}
		public delegate void NotifyLoginDelegate(NetIncomingMessage im, S2C.Message.NotifyLogin data);
		public event NotifyLoginDelegate OnNotifyLogin;
		[RpcStubAttribute(202)]
		public virtual S2C.Message.NotifyLogin NotifyLogin(NetIncomingMessage im)
		{
			Message.NotifyLogin data = new Message.NotifyLogin();
			data.new_id = im.ReadString();
			if(OnNotifyLogin != null) OnNotifyLogin(im, data);

			return data;
		}
		public delegate void ResSendDelegate(NetIncomingMessage im, S2C.Message.ResSend data);
		public event ResSendDelegate OnResSend;
		[RpcStubAttribute(203)]
		public virtual S2C.Message.ResSend ResSend(NetIncomingMessage im)
		{
			Message.ResSend data = new Message.ResSend();
			data.ret = im.ReadInt16();
			data.ret_message = im.ReadString();
			data.to_id = im.ReadString();
			if(OnResSend != null) OnResSend(im, data);

			return data;
		}
		public delegate void NotifySendDelegate(NetIncomingMessage im, S2C.Message.NotifySend data);
		public event NotifySendDelegate OnNotifySend;
		[RpcStubAttribute(204)]
		public virtual S2C.Message.NotifySend NotifySend(NetIncomingMessage im)
		{
			Message.NotifySend data = new Message.NotifySend();
			data.from_id = im.ReadString();
			data.message = im.ReadString();
			if(OnNotifySend != null) OnNotifySend(im, data);

			return data;
		}
		public delegate void ResSendAllDelegate(NetIncomingMessage im, S2C.Message.ResSendAll data);
		public event ResSendAllDelegate OnResSendAll;
		[RpcStubAttribute(205)]
		public virtual S2C.Message.ResSendAll ResSendAll(NetIncomingMessage im)
		{
			Message.ResSendAll data = new Message.ResSendAll();
			data.ret = im.ReadInt16();
			data.ret_message = im.ReadString();
			if(OnResSendAll != null) OnResSendAll(im, data);

			return data;
		}
		public delegate void NotifySendAllDelegate(NetIncomingMessage im, S2C.Message.NotifySendAll data);
		public event NotifySendAllDelegate OnNotifySendAll;
		[RpcStubAttribute(206)]
		public virtual S2C.Message.NotifySendAll NotifySendAll(NetIncomingMessage im)
		{
			Message.NotifySendAll data = new Message.NotifySendAll();
			data.from_id = im.ReadString();
			data.message = im.ReadString();
			if(OnNotifySendAll != null) OnNotifySendAll(im, data);

			return data;
		}
	}
}
