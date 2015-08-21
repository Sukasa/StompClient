namespace StompClient
{
    /// <summary>
    ///     The Receipt frame sent from the server in response to a receipt-id header sent in a previous packet from this client
    /// </summary>
    [StompFrameType("RECEIPT", StompFrameDirection.ServerToClient)]
    public class StompReceiptFrame : StompFrame
    {
        [StompHeaderIdentifier("receipt-id")]
        internal string _ReceiptId = null;

        /// <summary>
        ///     The receipt Id sent back from the server
        /// </summary>
        public string ReceiptId
        {
            get
            {
                return _ReceiptId;
            }
        }

        internal StompReceiptFrame() { }
    }
}
