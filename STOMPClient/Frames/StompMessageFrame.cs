namespace StompClient
{
    /// <summary>
    ///     STOMP Message frame that contains data sent from other nodes to this node via the server
    /// </summary>
    [StompFrameType("MESSAGE", StompFrameDirection.ServerToClient)]
    class StompMessageFrame : StompBodiedFrame
    {
        [StompHeaderIdentifier("subscription")]
        internal string _Subscription = null;

        [StompHeaderIdentifier("destination")]
        internal string _Destination = null;

        [StompHeaderIdentifier("message-id")]
        internal string _MessageId = null;

        /// <summary>
        ///     The Id of the subscription that this message was received as a part of
        /// </summary>
        public string SubscriptionId
        {
            get
            {
                return _Subscription;
            }
        }

        /// <summary>
        ///     The destination address set from the sending client
        /// </summary>
        public string Destination
        {
            get
            {
                return _Destination;
            }
        }

        /// <summary>
        ///     A unique Id for the message, used for Ack/NAck packets
        /// </summary>
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
