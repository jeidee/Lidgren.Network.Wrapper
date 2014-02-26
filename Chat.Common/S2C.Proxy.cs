using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren;
using Lidgren.Network;
using Lidgren.Message;
namespace S2C
{
	public class Proxy : MessageProxy
	{
		public const int Version = 100;

		public bool ResLogin(NetConnection connection, Int16 ret)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)200);
			om.Write(ret);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool NotifyLogout(NetConnection connection, String logout_id)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)201);
			om.Write(logout_id);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool NotifyLogin(NetConnection connection, String new_id)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)202);
			om.Write(new_id);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool ResSend(NetConnection connection, Int16 ret, String ret_message, String to_id)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)203);
			om.Write(ret);
			om.Write(ret_message);
			om.Write(to_id);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool NotifySend(NetConnection connection, String from_id, String message)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)204);
			om.Write(from_id);
			om.Write(message);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool ResSendAll(NetConnection connection, Int16 ret, String ret_message)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)205);
			om.Write(ret);
			om.Write(ret_message);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool NotifySendAll(NetConnection connection, String from_id, String message)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)206);
			om.Write(from_id);
			om.Write(message);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
	}
}
