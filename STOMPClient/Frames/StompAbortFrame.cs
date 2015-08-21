namespace StompClient
{
    /// <summary>
    ///     Abort frame used to cancel a transaction in progress
    /// </summary>
    [StompFrameType("ABORT", StompFrameDirection.ClientToServer)]
    public class StompAbortFrame : StompFrame
    {
        [StompHeaderIdentifier("transaction")]
        internal new string _TransactionId;

        /// <summary>
        ///     The transaction Id of the transaction to cancel
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
        ///     Creates a new StompAbortFrame with the required parameters set
        /// </summary>
        /// <param name="TransactionId">
        ///     The transaction Id of the transaction to cancel  
        /// </param>
        public StompAbortFrame(string TransactionId)
        {
            _TransactionId = TransactionId;
        }
    }
}
