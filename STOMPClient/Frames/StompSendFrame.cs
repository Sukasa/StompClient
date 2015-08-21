namespace StompClient
{
    [StompFrameType("SEND", StompFrameDirection.ClientToServer)]
    class StompSendFrame : StompBodiedFrame
    {
        [StompHeaderIdentifier("destination")]
        internal string _Destination;

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

        public StompSendFrame(string Destination)
        {
            _Destination = Destination;
        }

        public StompSendFrame(string Destination, string Body)
        {
            _Destination = Destination;
            PacketBody = Body;
        }
    }
}
