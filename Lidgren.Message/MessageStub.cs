using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Reflection;

namespace Lidgren.Message
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RpcStubAttribute : Attribute
    {
        UInt32 message_id;
        public UInt32 MessageID
        {
            get
            {
                return message_id;           
            }
        }

        public RpcStubAttribute(UInt32 message_id)
        {
            this.message_id = message_id;
        }
    }


    public struct RpcStubInfo
    {
        public MessageStub StubInstance;
        public MethodInfo StubMethodInfo;

        public void Call(params object[] param)
        {
            StubMethodInfo.Invoke(StubInstance, param);
        }
    }


    public class MessageStub
    {
        public bool AttachRpcStub(Dictionary<UInt32, RpcStubInfo> message_handlers)
        {
            MemberInfo member = this.GetType();
            foreach (MethodInfo method in this.GetType().GetMethods())
            {
                RpcStubAttribute attr = GetRpcStubAttribute(method);
                if(attr != null)
                {
                    if (message_handlers.ContainsKey(attr.MessageID))
                    {
                        return false;
                    }
                    else
                    {
                        message_handlers[attr.MessageID] = new RpcStubInfo() { StubInstance = this, StubMethodInfo = method };
                    }
                }
            }
            return true;
        }


        RpcStubAttribute GetRpcStubAttribute(MemberInfo member)
        {
            foreach (object attribute in member.GetCustomAttributes(true))
            {
                if (attribute is RpcStubAttribute)
                {
                    return (attribute as RpcStubAttribute);
                }
            }
            return null;
        }
    }
}
