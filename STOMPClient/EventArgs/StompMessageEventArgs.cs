
namespace StompClient
{
    /// <summary>
    ///     EventArgs class for Stomp message events
    /// </summary>
    public class StompMessageEventArgs : StompFrameEventArgs
    {
        /// <summary>
        ///     Whether to send a Negative Ack (NAck) instead of a normal Ack in reply
        /// </summary>
        public bool SendNAck { get; set; }

        internal StompMessageEventArgs(StompFrame Frame) : base(Frame)
        {
            SendNAck = false;
        }
    }
}
