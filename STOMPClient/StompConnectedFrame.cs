using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StompClient
{
    [StompFrameType("CONNECTED", StompFrameDirection.ServerToClient)]
    class StompConnectedFrame : StompFrame
    {
        [StompHeaderIdentifier("heartbeat", true)]
        internal string _Heartbeat = "0,0";

        [StompHeaderIdentifier("session", true)]
        internal string _SessionId = "No Session";

        [StompHeaderIdentifier("server", true)]
        internal string _ServerInfo = "Unknown/1.0";

        [StompHeaderIdentifier("version")]
        internal string _Version = "1.0";



        internal StompConnectedFrame()
        {

        }
    }
}
