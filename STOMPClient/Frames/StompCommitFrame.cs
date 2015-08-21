namespace StompClient
{
    /// <summary>
    ///     Commits a transaction previously starting
    /// </summary>
    [StompFrameType("COMMIT", StompFrameDirection.ClientToServer)]
    class StompCommitFrame : StompFrame
    {
        [StompHeaderIdentifier("transaction")]
        internal new string _TransactionId;

        /// <summary>
        ///     The transaction Id of the transaction to commit
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
        ///     Creates a new Commit frame with the given transaction id
        /// </summary>
        /// <param name="TransactionId">
        ///     The transaction Id of the transaction to commit
        /// </param>
        public StompCommitFrame(string TransactionId)
        {
            _TransactionId = TransactionId;
        }
    }
}
