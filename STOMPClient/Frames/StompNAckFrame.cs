
namespace StompClient
{
    [StompFrameType("NACK", StompFrameDirection.ClientToServer)]
    class StompNAckFrame : StompFrame
    {
        [StompHeaderIdentifier("id")]
        private string _id;

        public StompNAckFrame(string Id)
        {
            _id = Id;
        }

    }
}
