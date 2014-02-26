using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Lidgren.Message
{
    public class MessageProxy
    {
        protected NetPeer peer;

        public void SetNetwork(NetPeer peer)
        {
            this.peer = peer;
        }
    }
}
