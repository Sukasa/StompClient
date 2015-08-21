using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace StompClient
{
    /// <summary>
    ///  A basic STOMP protocol client
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The StompClient class encapsulates a mostly-compliant Stomp 1.2 client (1.0, 1.1 compatible), with the exception that it does not support binary payloads at this time.
    ///     </para>
    ///     
    ///     <para>
    ///         The class provides basic connection and subscription functionality, and includes provisions for custom extensions
    ///         to the protocol including internal serialization that supports custom types derived from <seealso cref="StompFrame"/>.
    ///     </para>
    /// </remarks>
    public class STOMPClient
    {
        public delegate void StompMessageEvent(object sender, StompMessageEventArgs e);
        public delegate void StompFrameEvent(object sender, StompFrameEventArgs e);
        public delegate void StompErrorEvent(object sender, StompErrorEventArgs e);
        public delegate void StompReceiptEvent(object sender, StompFrameEventArgs e);

        private Dictionary<string, Type> _FrameTypeMapping = new Dictionary<string, Type>();
        private int _Heartbeat = 0;
        private int _HeartbeatTxInterval = 0;
        private int _HeartbeatRxInterval = 0;
        private int _HeartbeatTxIntervalTimeout = 0;
        private int _HeartbeatRxIntervalTimeout = 0;
        private float _ConnectionVersion = 0.0f;
        private Thread _PollThread;
        private TcpClient _Client;
        private Dictionary<string, string> _Subscriptions = new Dictionary<string, string>();
        private int _NumSubscriptions;

        /// <summary>
        ///     Fires when a Message frame is received
        /// </summary>
        /// <remarks>
        ///     The MessageReceived event fires any time a Message frame is received, and allows you to determine whether to reply with an Ack or NAck frame. 
        /// </remarks>
        public event StompMessageEvent MessageReceived;

        /// <summary>
        ///     Fires when a frame is received
        /// </summary>
        /// <remarks>
        ///     The FrameReceived event fires any time a frame is received from the server that is not a server error, receipt frame, or internal
        /// </remarks>
        public event StompFrameEvent FrameReceived;

        /// <summary>
        ///     Fires if the server returns an error frame and disconnects
        /// </summary>
        /// <remarks>
        ///     The ServerError event fires if the Stomp server returns an Error frame.  This event signifies the impending closure of the connection
        /// </remarks>
        public event StompErrorEvent ServerError;

        /// <summary>
        ///     Fires when a Receipt frame is received
        /// </summary>
        /// <remarks>
        ///     The FrameReceipt event fires any time the server returns a Receipt frame in response to a requested receipt for a frame the client has sent
        /// </remarks>
        public event StompReceiptEvent FrameReceipt;

        /// <summary>
        ///     The desired heartbeat timeout
        /// </summary>
        /// <remarks>
        ///     Due to how the Stomp protocol works, this is the <i>fastest</i> possible rate at which heartbeats will be exchanged.  Additionally, heartbeats may be exchanged in each direction at different rates.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if you attempt to change the Heartbeat settings after a connection has been established 
        /// </exception>
        public int HeartbeatTimeout
        {
            get
            {
                return _Heartbeat;
            }
            set
            {
                if (Connected)
                    throw new InvalidOperationException("Cannot change heartbeat once connected to server");
                _Heartbeat = value;
            }
        }

        /// <summary>
        ///     The connection state of the underlying <seealso cref="TcpClient"/>
        /// </summary>
        public bool Connected
        {
            get
            {
                return (_Client != null && _Client.Connected);
            }
        }

        /// <summary>
        ///     Whether or not to use the version 1.2 STOMP packet instead of CONNECT.  Only works for v1.2-compliant Stomp servers
        /// </summary>
        public bool UseStompPacket { get; set; }

        /// <summary>
        /// The username used when connecting to the Stomp server
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password used when connecting to the Stomp server
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     The size of the internal ring buffer used to receive frames.
        /// </summary>
        /// <remarks>
        ///     Gets or sets the size of the internal ring buffer used to receive frames.  If your connection fails unexpectedly or you expect to handle large amounts of data in a single frame, consider increasing this
        ///  </remarks>
        public int RxBufferSize { get; set; }

        /// <summary>
        ///     Which version of the Stomp protocol is in use
        /// </summary>
        public float ConnectionVersion { get { return _ConnectionVersion; } }

        /// <summary>
        ///     Creates a new Stomp client and connects to the destination server with default settings
        /// </summary>
        /// <param name="ServerAddress">
        ///     The URI of the Stomp server to connect to 
        /// </param>
        /// <remarks>
        ///     Constructs a Stomp client and opens a connection to the address supplied.  Will attempt to use protocol v1.2 initially, with the base protocol only.
        /// </remarks>
        public STOMPClient(Uri ServerAddress)
            : this()
        {
            Connect(ServerAddress);
        }

        /// <summary>
        ///     Creates a new Stomp client
        /// </summary>
        /// <remarks>
        ///     Constructs a Stomp client with default settings, but does not connect to a server
        /// </remarks>
        public STOMPClient()
        {
            // Set up default values
            UseStompPacket = false;
            HeartbeatTimeout = 0;
            RxBufferSize = 16384;

            // Load the base types in this library
            Assembly.GetExecutingAssembly().GetTypes()
                                           .Where(x => typeof(StompFrame).IsAssignableFrom(x) && !x.IsAbstract)
                                           .Select(x => new Tuple<String, Type>(x.GetCustomAttribute<StompFrameType>()._FrameType, x))
                                           .ForEach(x => _FrameTypeMapping[x.Item1] = x.Item2);
            // Init the client
            CreateClient();
        }

        /// <summary>
        ///     Imports all loaded StompFrame-derived classes into the client for automatic deserialization
        /// </summary>
        public void ImportProtocolExtensions()
        {
            // Reload ALL frame types in the current appdomain - i.e. "import" all types from code using this library
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                           .Where(x => typeof(StompFrame).IsAssignableFrom(x) && !x.IsAbstract)
                                           .Select(x => new Tuple<String, Type>(x.GetCustomAttribute<StompFrameType>()._FrameType, x))
                                           .ForEach(x => _FrameTypeMapping[x.Item1] = x.Item2);
        }

        /// <summary>
        ///     Connects to the listed server, disconnecting if currently connected.
        /// </summary>
        /// <param name="ServerAddress"></param>
        public void Connect(Uri ServerAddress)
        {
            // If we're already connected, close that connection and killit hard
            if (_Client.Connected)
            {
                _Client.Close();
                CreateClient();
            }

            // Now connect to the new stomp server
            _Client.Connect(Dns.GetHostAddresses(ServerAddress.Host)[0], ServerAddress.Port);

            if (!_Client.Connected)
                throw new InvalidOperationException("Failed to connect to server");

            if (UseStompPacket)
            {
                SendFrame(new StompStompFrame(this, ServerAddress));
            }
            else
            {
                SendFrame(new StompConnectFrame(this, ServerAddress));
            }

            // Start the polling thread to handle heartbeats, etc
            _PollThread = new Thread(Run);
            _PollThread.Start();
        }

        /// <summary>
        ///     Serializes and transmits a frame to the server
        /// </summary>
        /// <param name="Frame">
        ///     The <seealso cref="StompFrame"/> to send to the server 
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown if
        ///     <list type="number">
        ///         <item>You attempt to serialize a frame class that has no <seealso cref="StompFrameType"/> attribute</item>
        ///         <item>You attempt to send a frame that is marked as server to client only</item>
        ///         <item>You fail to fill in a mandatory frame parameter</item>
        ///     </list>
        /// </exception>
        public void SendFrame(StompFrame Frame)
        {
            // Get some metadata about the frame that we'll need
            StompFrameType SFT = Frame.GetType().GetCustomAttribute<StompFrameType>();

            // Validate frame before doing anything else
            if (SFT == null)
                throw new ArgumentException("Attempt to serialize frame without frame type attribute", "Frame");

            if (SFT._Direction == StompFrameDirection.ServerToClient)
                throw new ArgumentException("Attempt to send server frame from client", "Frame");

            // Serialize the frame and convert to byte array
            string FrameData = Frame.Serialize();
            byte[] Data = Encoding.UTF8.GetBytes(FrameData);

            // Now send the frame
            lock (_Client)
            {
                _Client.GetStream().Write(Data, 0, Data.Length);
                _Client.GetStream().WriteByte(0);
                _HeartbeatTxIntervalTimeout = _HeartbeatTxInterval;
            }

        }

        // ---

        [Obsolete("This is a debug function.  Don't use it.")]
        public string GetSerializedPacket(StompFrame Frame)
        {
            return Frame.Serialize();
        }

        [Obsolete("This is a debug function.  Don't use it.")]
        public StompFrame GetBuiltFrame(byte[] Packet)
        {
            return StompFrame.Build(Packet, _FrameTypeMapping);
        }

        // ---

        /// <summary>
        ///     Subscribe to a feed, getting the Subscription Id assigned to that feed
        /// </summary>
        /// <param name="Destination">
        ///     What destination to subscribe to
        /// </param>
        /// <returns>
        ///     A unique Id for this subscription
        /// </returns>
        public string Subscribe(string Destination)
        {
            if (_Subscriptions.ContainsKey(Destination))
                return _Subscriptions[Destination];

            Random RNG = new Random(_NumSubscriptions++);
            string Id;

            do
            {
                Id = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", RNG.Next('a', 'Z'), RNG.Next('a', 'Z'), RNG.Next('a', 'Z'), RNG.Next('a', 'Z'),
                                                               RNG.Next('a', 'Z'), RNG.Next('a', 'Z'), RNG.Next('a', 'Z'), RNG.Next('a', 'Z'));
            } while (_Subscriptions.ContainsValue(Id));

            StompSubscribeFrame Frame = new StompSubscribeFrame(Destination, Id);
            SendFrame(Frame);
            _Subscriptions[Destination] = Id;

            return Id;
        }

        /// <summary>
        ///     Unsubscribe from a subscription based on the subscription Id
        /// </summary>
        /// <param name="SubscriptionId">
        ///     The subscription Id originall returned by <seealso cref="Subscribe"/>
        /// </param>
        public void Unsubscribe(string SubscriptionId)
        {
            if (!_Subscriptions.ContainsValue(SubscriptionId))
                throw new ArgumentException("Not subscribed to a feed with this Id", "SubscriptionId");

            KeyValuePair<String, string> Subscription = _Subscriptions.First(x => x.Value == SubscriptionId);
            StompUnsubscribeFrame Frame = new StompUnsubscribeFrame(Subscription.Value);
            _Subscriptions.Remove(Subscription.Key);
        }

        private void HandleMessageFrame(StompMessageFrame Frame)
        {
            StompMessageEventArgs e = new StompMessageEventArgs(Frame);

            // First, raise the event - catch it if the client code throws an exception and spit to console but don't crash
            try
            {
                MessageReceived(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // Now reply with either NAck or Ack depending on what was set on the EventArgs
            if (e.SendNAck)
            {
                SendFrame(new StompNAckFrame(Frame.MessageId));
            }
            else
            {
                SendFrame(new StompAckFrame(Frame.MessageId));
            }
        }

        private void DispatchFrame(StompFrame Frame)
        {
            // Dispatch the frame to the correct handler function or event

            if (Frame is StompMessageFrame)
            {
                HandleMessageFrame((StompMessageFrame)Frame);
            }
            else if (Frame is StompReceiptFrame)
            {
                try
                {
                    FrameReceipt(this, new StompFrameEventArgs(Frame));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else if (Frame is StompErrorFrame)
            {
                try
                {
                    ServerError(this, new StompErrorEventArgs((StompErrorFrame)Frame));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else if (Frame is StompConnectedFrame)
            {
                // Handle connection configuration
                StompConnectedFrame SCF = (StompConnectedFrame)Frame;

                int[] Timings = SCF._Heartbeat.Split(',').Select(x => int.Parse(x)).ToArray();

                _HeartbeatRxInterval = Math.Max(_Heartbeat, Timings[0]);
                _HeartbeatTxInterval = Math.Max(_Heartbeat, Timings[1]);

                _ConnectionVersion = float.Parse(SCF._Version);
            }
            else
            {
                try
                {
                    FrameReceived(this, new StompFrameEventArgs(Frame));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private void Run()
        {
            byte[] RxData = new byte[512]; // Rx buffer used for transferring data from the stream to the ring buffer
            NetworkStream Stream = _Client.GetStream();
            StompRingBuffer<byte> Buffer = new StompRingBuffer<byte>(RxBufferSize);

            // Run while the client is connected, polling for data rx'd and dispatching frames as necessary
            // Also handle heartbeats and heartbeat disconnect
            while (_Client.Connected)
            {
                // If we should be heartbeating, and we're connected...
                if (_Heartbeat > 0 && ConnectionVersion > 0.0f)
                {
                    // Sleep up to 15ms, or less if we need to heartbeat sooner
                    int SleepAmt = Math.Min(_HeartbeatTxIntervalTimeout, 15);

                    // If we expect to recieve heartbeats...
                    if (_HeartbeatRxInterval > 0)
                    {
                        // ...check that we've recieved data within the rx interval.  If not, disconnect.
                        _HeartbeatRxIntervalTimeout -= SleepAmt;
                        if (_HeartbeatRxIntervalTimeout < 0)
                        {
                            lock (_Client)
                            {
                                _Client.Close();
                            }
                            return;
                        }
                    }

                    // If we need to send heartbeats...
                    if (_HeartbeatTxInterval > 0)
                    {
                        // ... send one if it's been too long since our last transmission
                        _HeartbeatTxIntervalTimeout -= SleepAmt;
                        if (_HeartbeatTxIntervalTimeout < 0 && _HeartbeatTxInterval > 0)
                        {
                            _Client.GetStream().WriteByte((byte)'\n');
                            _HeartbeatTxIntervalTimeout = _HeartbeatTxInterval;
                        }
                    }

                    Thread.Sleep(SleepAmt);
                }
                else // Otherwise just do a standard sleep
                {
                    Thread.Sleep(15);
                }

                // Read in as much data from the stream as we can into the ring buffer
                while (_Client.Available > 0 && Buffer.AvailableWrite > 0)
                {
                    // We've received data, so reset the heartbeat rx timeout
                    _HeartbeatRxIntervalTimeout = (int)(_HeartbeatRxInterval * 1.5); // +50% forgiveness for heartbeat loss

                    // Now read in from the networkstream to the ring buffer
                    int AmtRead = Stream.Read(RxData, 0, Buffer.AvailableWrite);
                    Buffer.Write(RxData, AmtRead);
                }

                // Advance through any heartbeats rx'd or frame separators
                while (Buffer.Peek() == '\r' || Buffer.Peek() == '\n' || Buffer.Peek() == '\0')
                    Buffer.Read(1);

                // Now try to build + dispatch the packet
                if (!TryBuildPacket(Buffer) && Buffer.AvailableWrite == 0)
                    throw new InvalidOperationException("Ran out of receive ringbuffer space in STOMPClient");

            }
        }

        /// <summary>
        ///     Tries to build a packet from the given ringbuffer
        /// </summary>
        /// <param name="Buffer">
        ///     The Ringbuffer to build a packet from
        /// </param>
        /// <returns>
        ///     Whether it was able to build a packet or not
        /// </returns>
        private bool TryBuildPacket(StompRingBuffer<byte> Buffer)
        {
            // See if we have rx'd a packet separator or a \0 in a binary frame body
            int PacketLength = Buffer.DistanceTo(0);

            // We have, so what did we find?
            if (PacketLength > 0)
            {
                // This is a really messy block of code.

                // The goal is that it tries to determine whether it has a full packet or needs to wait for more data
                // before building the packet and dispatching it

                byte[] Data = Buffer.Peek(PacketLength);
                string Header = Encoding.UTF8.GetString(Data);
                string[] HeaderCheck = Header.Split('\n');
                int ContentLength = 0;
                bool HasContentLength = false;
                bool IsFullHeader = false;

                // First, we look to see if our "packet" has a content-length header.  Since we scanned out to a null (\0) byte, we're guaranteed to at least have the headers
                // of whatever packet we're examining

                for (int i = 0; i < HeaderCheck.Length; i++)
                {
                    // We found a content-length header?  Flag it and store how large in bytes the content should be
                    if (HeaderCheck[i].StartsWith("content-length:"))
                    {
                        HasContentLength = true;
                        ContentLength = int.Parse(HeaderCheck[i].Substring(15));
                    }
                    if (HeaderCheck[i] == "" || HeaderCheck[i] == "\r")
                    {
                        IsFullHeader = true;
                        break;
                    }
                }

                if (!IsFullHeader)
                    return false;
                
                StompFrame Frame = null;
                
                if (HasContentLength)
                {
                    // We have a content-length header.  We need to find the start of the frame body, in bytes,
                    // and then make sure we have (ContentLength) bytes available after that

                    // Look for the end of the headers, either 1.0/1.1 or 1.2 (\r\n)-friendly
                    int EndOfHeaders = Header.IndexOf("\r\n\r\n") + 4;
                    if (EndOfHeaders == -3) // (-1) + 4
                        EndOfHeaders = Header.IndexOf("\n\n") + 2;

                    // Get the byte length of the header
                    int Offset = Encoding.UTF8.GetByteCount(Header.Substring(0, EndOfHeaders));

                    // Now see if we have that many bytes available in the ring buffer (realistically, we should except for obscene frame sizes)
                    if (Offset + ContentLength <= Buffer.AvailableRead)
                    {
                        // If we do, peek the exact packet length we want and assemble
                        Frame = StompFrame.Build(Buffer.Peek(Offset + ContentLength), _FrameTypeMapping);
                        Buffer.Seek(Offset + ContentLength);
                        DispatchFrame(Frame);

                        return true;
                    }
                }
                else // No content-length.  We're guaranteed to be a text packet without any overshoot; no special treatment needed
                {
                    Frame = StompFrame.Build(Data, _FrameTypeMapping);
                    Buffer.Seek(PacketLength);
                    DispatchFrame(Frame);

                    return true;
                }
            }

            return false;
        }

        private void CreateClient()
        {
            // Kill the poll thread hard if it's still going (it really shouldn't be, but...)
            if (_PollThread != null && _PollThread.ThreadState == ThreadState.Running)
                _PollThread.Abort();

            // Create a new TcpClient
            _Client = new TcpClient();
        }

    }
}
