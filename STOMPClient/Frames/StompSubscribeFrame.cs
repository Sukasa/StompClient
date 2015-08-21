namespace StompClient
{
    [StompFrameType("SUBSCRIBE", StompFrameDirection.ClientToServer)]
    class StompSubscribeFrame : StompFrame
    {
        [StompHeaderIdentifier("destination")]
        internal string _Destination;

        [StompHeaderIdentifier("id")]
        internal string _id;

        // We're just going to set client mode on all subscriptions, and properly ACK them all automagically
        [StompHeaderIdentifier("ack", true)]
        internal string _ack = "client";

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

        public StompSubscribeFrame(string Destination, string Id)
        {
            _Destination = Destination;
            _id = Id;
        }
    }
}
