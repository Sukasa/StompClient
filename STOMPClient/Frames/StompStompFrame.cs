using System;

namespace StompClient
{
    [StompFrameType("STOMP", StompFrameDirection.ClientToServer)]
    internal class StompStompFrame : StompFrame
    {
        [StompHeaderIdentifier("accept-version")]
        internal string AcceptedVersions = "1.2";

        [StompHeaderIdentifier("host")]
        internal string Hostname;

        [StompHeaderIdentifier("login", true)]
        internal string Username;

        [StompHeaderIdentifier("password", true)]
        internal string Password;

        [StompHeaderIdentifier("heart-beat", true)]
        internal string Heartbeat;

        internal StompStompFrame(StompClient Client, Uri ConnectTo)
        {
            Hostname = ConnectTo.Host;
            Username = Client.Username;
            Password = Client.Password;

            Heartbeat = String.Format("{0}, {0}", Client.HeartbeatTimeout);
        }
    }
}
