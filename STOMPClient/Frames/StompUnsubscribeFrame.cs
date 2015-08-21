namespace StompClient
{
    [StompFrameType("UNSUBSCRIBE", StompFrameDirection.ClientToServer)]
    class StompUnsubscribeFrame : StompFrame
    {
        [StompHeaderIdentifier("id")]
        internal string _id;

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

        public StompUnsubscribeFrame(string Id)
        {
            _id = Id;
        }
    }
}
