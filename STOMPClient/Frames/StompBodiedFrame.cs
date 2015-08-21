using System.Text;

namespace StompClient
{
    /// <summary>
    ///     The base class for all frames that include a frame body
    /// </summary>
    public abstract class StompBodiedFrame : StompFrame
    {
        internal byte[] _PacketData;

        [StompHeaderIdentifier("content-type")]
        internal string _ContentType;

        [StompHeaderIdentifier("content-length", true)]
        internal string _ContentLength;

        /// <summary>
        ///     The body of the frame, in text form
        /// </summary>
        public string BodyText
        {
            get
            {
                return Encoding.UTF8.GetString(_PacketData);
            }
            set
            {
                _PacketData = Encoding.UTF8.GetBytes(value);
                
                if (ContentType == null || !ContentType.StartsWith("text"))
                    ContentType = "text/plain";

                ContentLengthBytes = Encoding.UTF8.GetByteCount(value);
            }
        }

        /// <summary>
        ///     The body of the frame, in binary form
        /// </summary>
        public byte[] BodyData
        {
            get
            {
                return _PacketData;
            }
            set
            {
                _PacketData = value;
                ContentLengthBytes = _PacketData.Length;
            }
        }

        /// <summary>
        ///     The MIME content-type of the data in the body
        /// </summary>
        public string ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                _ContentType = value;
            }
        }

        /// <summary>
        ///     The length, in bytes, of the content.  -1 if there is no body attached to the frame
        /// </summary>
        public int ContentLengthBytes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ContentLength))
                    return _PacketData != null ? _PacketData.Length : -1;
                return int.Parse(_ContentLength);
            }
            set
            {
                _ContentLength = value.ToString();
            }
        }
    }
}
