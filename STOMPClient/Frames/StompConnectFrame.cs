using System;

namespace StompClient
{
    /// <summary>
    ///     The same as a STOMP frame, but compatible with 1.1 and 1.0
    /// </summary>
    [StompFrameType("CONNECT", StompFrameDirection.ClientToServer)]
    class StompConnectFrame : StompFrame
    {
        [StompHeaderIdentifier("accept-version")]
        internal string AcceptedVersions = "1.0,1.1,1.2";

        [StompHeaderIdentifier("host")]
        internal string Hostname;

        [StompHeaderIdentifier("login", true)]
        internal string Username;

        [StompHeaderIdentifier("password", true)]
        internal string Password;

        [StompHeaderIdentifier("heart-beat", true)]
        internal string Heartbeat;

        internal StompConnectFrame(STOMPClient Client, Uri ConnectTo)
        {
            Hostname = ConnectTo.Host;
            Username = Client.Username;
            Password = Client.Password;

            Heartbeat = String.Format("{0}, {0}", Client.HeartbeatTimeout);
        }
    }
}
