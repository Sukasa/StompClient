namespace StompClient
{
    [StompFrameType("RECEIPT", StompFrameDirection.ServerToClient)]
    class StompReceiptFrame : StompFrame
    {
        [StompHeaderIdentifier("receipt-id")]
        internal string _ReceiptId = null;

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
