
namespace StompClient
{
    public class StompErrorEventArgs : StompFrameEventArgs
    {
        private string _ErrorMessage;

        public string ErrorMessage { get { return _ErrorMessage;  } }

        public StompErrorEventArgs(StompErrorFrame Frame)
            : base(Frame)
        {
            _ErrorMessage = Frame._errorMessage;
        }
    }
}
