namespace StompClient
{
    [StompFrameType("DISCONNECT", StompFrameDirection.ClientToServer)]
    class StompDisconnectFrame : StompFrame
    {
        public StompDisconnectFrame()
        {

        }
    }
}
