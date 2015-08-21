namespace StompClient
{
    [StompFrameType("ERROR", StompFrameDirection.ServerToClient)]
    public class StompErrorFrame : StompBodiedFrame
    {
        [StompHeaderIdentifier("message")]
        internal string _errorMessage = null;

        public string ErrorMessage { get { return _errorMessage; } }
    }
}
