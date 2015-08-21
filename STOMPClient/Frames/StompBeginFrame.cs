namespace StompClient
{
    /// <summary>
    ///     Frame sent to the server to initiate a transaction under a specific transaction Id
    /// </summary>
    [StompFrameType("BEGIN", StompFrameDirection.ClientToServer)]
    class StompBeginFrame : StompFrame
    {
        [StompHeaderIdentifier("transaction")]
        internal new string _TransactionId;

        /// <summary>
        ///     The id to assign the new transaction
        /// </summary>
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

        /// <summary>
        ///     Creates a new Begin frame with the specified transaction id to use
        /// </summary>
        /// <param name="TransactionId">
        ///     The id to assign the new transaction
        /// </param>
        public StompBeginFrame(string TransactionId)
        {
            _TransactionId = TransactionId;
        }
    }
}
