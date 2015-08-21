namespace StompClient
{
    [StompFrameType("ABORT", StompFrameDirection.ClientToServer)]
    class StompAbortFrame : StompFrame
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

        public StompAbortFrame(string TransactionId)
        {
            _TransactionId = TransactionId;
        }
    }
}
