namespace StompClient
{
    [StompFrameType("ACK", StompFrameDirection.ClientToServer)]
    public class StompAckFrame : StompFrame
    {
        [StompHeaderIdentifier("id")]
        private string _id;

        public StompAckFrame(string Id)
        {
            _id = Id;
        }
    }
}
