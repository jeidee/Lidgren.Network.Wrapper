using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren;
using Lidgren.Network;
using Lidgren.Message;
namespace S2C
{
	public class Message
	{
		public const int Version = 100;

		public enum Flag
		{
			kFlagFail = 0,
			kFlagSuccess = 1,
		};

		public struct ResLogin
		{
			public Int16 ret;	
		}
		public struct NotifyLogout
		{
			public String logout_id;	
		}
		public struct NotifyLogin
		{
			public String new_id;	
		}
		public struct ResSend
		{
			public Int16 ret;	
			public String ret_message;	
			public String to_id;	
		}
		public struct NotifySend
		{
			public String from_id;	
			public String message;	
		}
		public struct ResSendAll
		{
			public Int16 ret;	
			public String ret_message;	
		}
		public struct NotifySendAll
		{
			public String from_id;	
			public String message;	
		}
	}
}
