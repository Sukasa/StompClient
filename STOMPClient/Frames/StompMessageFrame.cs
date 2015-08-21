using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StompClient
{
    [StompFrameType("MESSAGE", StompFrameDirection.ServerToClient)]
    class StompMessageFrame : StompBodiedFrame
    {
        [StompHeaderIdentifier("subscription")]
        internal string _Subscription = null;

        [StompHeaderIdentifier("destination")]
        internal string _Destination = null;

        [StompHeaderIdentifier("message-id")]
        internal string _MessageId = null;

        public string SubscriptionId
        {
            get
            {
                return _Subscription;
            }
        }

        public string Destination
        {
            get
            {
                return _Destination;
            }
        }

        public string MessageId
        {
            get
            {
                return _MessageId;
            }
        }

        internal StompMessageFrame() { }
    }
}
