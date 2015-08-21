
namespace StompClient
{
    public class StompMessageEventArgs : StompFrameEventArgs
    {
        public bool SendNAck { get; set; }

        internal StompMessageEventArgs(StompFrame Frame) : base(Frame)
        {

        }
    }
}
