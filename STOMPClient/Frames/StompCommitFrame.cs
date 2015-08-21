namespace StompClient
{
    [StompFrameType("COMMIT", StompFrameDirection.ClientToServer)]
    class StompCommitFrame : StompFrame
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

        public StompCommitFrame(string TransactionId)
        {
            _TransactionId = TransactionId;
        }
    }
}
