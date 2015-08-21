namespace StompClient
{
    /// <summary>
    ///     Ack frame for acknowledging the server.  Is handled automatically by the server for Messages
    /// </summary>
    [StompFrameType("ACK", StompFrameDirection.ClientToServer)]
    public class StompAckFrame : StompFrame
    {
        [StompHeaderIdentifier("id")]
        private string _id;

        /// <summary>
        ///     Creates a new Ack frame for the given message Id
        /// </summary>
        /// <param name="Id">
        ///     The Id of the message this frame is an Ack for
        /// </param>
        public StompAckFrame(string Id)
        {
            _id = Id;
        }
    }
}
