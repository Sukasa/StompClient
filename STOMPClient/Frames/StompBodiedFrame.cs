namespace StompClient
{
    public class StompBodiedFrame : StompFrame
    {
        public string PacketBody;

        [StompHeaderIdentifier("content-type")]
        public string ContentType;

        [StompHeaderIdentifier("content-length", true)]
        internal string _ContentLength;

        public int ContentLength
        {
            get
            {
                return int.Parse(_ContentLength);
            }
            set
            {
                _ContentLength = value.ToString();
            }
        }
    }
}
