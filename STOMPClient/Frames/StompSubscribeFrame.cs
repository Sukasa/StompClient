namespace StompClient
{
    /// <summary>
    ///     The Subscription frame type for subscribing to a feed on the server
    /// </summary>
    [StompFrameType("SUBSCRIBE", StompFrameDirection.ClientToServer)]
    class StompSubscribeFrame : StompFrame
    {
        [StompHeaderIdentifier("destination")]
        internal string _Destination;

        [StompHeaderIdentifier("id")]
        internal string _id;

        // We're just going to set client mode on all subscriptions, and properly ACK them all automagically
        [StompHeaderIdentifier("ack", true)]
        internal string _ack = "client-individual";

        /// <summary>
        ///     Which feed to subscribe to
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
        ///     A unique, client-generated Id for this particular subscription
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        ///     Creates a new Subscription frame with the specified Destination and Id
        /// </summary>
        /// <param name="Destination">
        ///     Which feed to subscribe to
        /// </param>
        /// <param name="Id">
        ///     A unique, client-generated Id for this particular subscription
        /// </param>
        public StompSubscribeFrame(string Destination, string Id)
        {
            _Destination = Destination;
            _id = Id;
        }
    }
}
