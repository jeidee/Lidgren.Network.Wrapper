using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren;
using Lidgren.Network;
using Lidgren.Message;
namespace C2S
{
	public class Message
	{
		public const int Version = 100;

		public enum Flag
		{
			kFlagFail = 0,
			kFlagSuccess = 1,
		};

		public struct ReqLogin
		{
			public String id;	
		}
		public struct ReqLogout
		{
			public String id;	
		}
		public struct Heartbeat
		{
		}
		public struct ReqSend
		{
			public String to_id;	
			public String message;	
		}
		public struct ReqSendAll
		{
			public String message;	
		}
	}
}
