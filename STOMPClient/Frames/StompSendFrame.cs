namespace StompClient
{
    /// <summary>
    ///     Send frame used to send text data to a given destination on the server
    /// </summary>
    [StompFrameType("SEND", StompFrameDirection.ClientToServer)]
    public class StompSendFrame : StompBodiedFrame
    {
        [StompHeaderIdentifier("destination")]
        internal string _Destination;

        /// <summary>
        ///     The destination address on the server to send to
        /// </summary>
        public string Destination
        {
            get
            {
                return _Destination;
            }
            set
            {
                _Destination = value;
            }
        }

        /// <summary>
        ///     Creates a Send frame set to the given destination, with a blank body
        /// </summary>
        /// <param name="Destination">
        ///     The destination address on the server to send to
        /// </param>
        public StompSendFrame(string Destination)
        {
            _Destination = Destination;
            _PacketBody = string.Empty;
        }
        /// <summary>
        ///     Creates a Send frame set to the given destination, with the given body
        /// </summary>
        /// <param name="Destination">
        ///     The destination address on the server to send to
        /// </param>
        /// <param name="Body">
        ///     The text to add to the body of the Send frame
        /// </param>
        public StompSendFrame(string Destination, string Body)
        {
            _Destination = Destination;
            _PacketBody = Body;
        }
    }
}
