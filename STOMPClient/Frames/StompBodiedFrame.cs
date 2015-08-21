namespace StompClient
{
    /// <summary>
    ///     The base class for all frames that include a frame body
    /// </summary>
    public abstract class StompBodiedFrame : StompFrame
    {
        internal string _PacketBody;

        [StompHeaderIdentifier("content-type")]
        internal string _ContentType;

        [StompHeaderIdentifier("content-length", true)]
        internal string _ContentLength;

        /// <summary>
        ///     The body of the packet
        /// </summary>
        public string Body
        {
            get
            {
                return _PacketBody;
            }
            set
            {
                _PacketBody = value;
            }
        }

        /// <summary>
        ///     The MIME content-type of the data in the body
        /// </summary>
        public string ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                _ContentType = value;
            }
        }

        /// <summary>
        ///     The length, in bytes, of the content.  -1 if there is no body attached to the frame
        /// </summary>
        public int ContentLength
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ContentLength))
                    return _PacketBody != null ? _PacketBody.Length : -1;
                return int.Parse(_ContentLength);
            }
            set
            {
                _ContentLength = value.ToString();
            }
        }
    }
}
