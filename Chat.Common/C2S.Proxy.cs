using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren;
using Lidgren.Network;
using Lidgren.Message;
namespace C2S
{
	public class Proxy : MessageProxy
	{
		public const int Version = 100;

		public bool ReqLogin(NetConnection connection, String id)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)100);
			om.Write(id);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool ReqLogout(NetConnection connection, String id)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)101);
			om.Write(id);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool Heartbeat(NetConnection connection)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)102);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool ReqSend(NetConnection connection, String to_id, String message)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)103);
			om.Write(to_id);
			om.Write(message);
			NetSendResult result = peer.SendMessage(om, connection,
				NetDeliveryMethod.ReliableOrdered);
			if (result == NetSendResult.FailedNotConnected ||
				result == NetSendResult.Dropped)
				return false;

			peer.FlushSendQueue();
			return true;
		}
		public bool ReqSendAll(NetConnection connection, String message)
		{
			if (peer == null || connection == null) return false;
			if (peer.ConnectionsCount < 1 ||
				connection.Status != NetConnectionStatus.Connected)
				return false;
			NetOutgoingMessage om = peer.CreateMessage();
			om.Write((UInt32)104);
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
