namespace StompClient
{
    [StompFrameType("BEGIN", StompFrameDirection.ClientToServer)]
    class StompBeginFrame : StompFrame
    {
        [StompHeaderIdentifier("transaction")]
        internal new string _TransactionId;

        public new string TransactionId
        {
            get
            {
                return _TransactionId;
            }
            set
            {
                _TransactionId = value;
            }
        }

        public StompBeginFrame(string TransactionId)
        {
            _TransactionId = TransactionId;
        }
    }
}
