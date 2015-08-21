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
    public class StompClient
    {
        public delegate void StompMessageEvent(object sender, StompMessageEventArgs e);
        public delegate void StompFrameEvent(object sender, StompFrameEventArgs e);
        public delegate void StompErrorEvent(object sender, StompErrorEventArgs e);
        public delegate void StompReceiptEvent(object sender, StompFrameEventArgs e);

        private Dictionary<string, Type> _FrameTypeMapping = new Dictionary<string,Type>();
        private int _Heartbeat = 0;
        private int _HeartbeatTxInterval = 0;
        private int _HeartbeatRxInterval = 0;
        private int _HeartbeatTxIntervalTimeout = 0;
        private int _HeartbeatRxIntervalTimeout = 0;
        private float _ConnectionVersion = 0.0f;
        private Thread _PollThread;
        private TcpClient _Client;

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
        public float ConnectionVersion { get { return _ConnectionVersion;  } }

        /// <summary>
        ///     Creates a new Stomp client and connects to the destination server with default settings
        /// </summary>
        /// <param name="ServerAddress">
        ///     The URI of the Stomp server to connect to 
        /// </param>
        /// <remarks>
        ///     Constructs a Stomp client and opens a connection to the address supplied.  Will attempt to use protocol v1.2 initially, with the base protocol only.
        /// </remarks>
        public StompClient(Uri ServerAddress)
            : this()
        {

        }

        /// <summary>
        ///     Creates a new Stomp client
        /// </summary>
        /// <remarks>
        ///     Constructs a Stomp client with default settings, but does not connect to a server
        /// </remarks>
        public StompClient()
        {
            UseStompPacket = false;
            HeartbeatTimeout = 0;
            RxBufferSize = 16384;

            Assembly.GetExecutingAssembly().GetTypes()
                                           .Where(x => typeof(StompFrame).IsAssignableFrom(x) && !x.IsAbstract)
                                           .Select<Type, Tuple<String, Type>>(x => new Tuple<String, Type>(x.GetCustomAttribute<StompFrameType>()._FrameType, x))
                                           .ForEach(x => _FrameTypeMapping[x.Item1] = x.Item2);

            CreateClient();
        }

        /// <summary>
        ///     Imports all loaded StompFrame-derived classes into the client for automatic deserialization
        /// </summary>
        public void ImportProtocolExtensions()
        {
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                           .Where(x => typeof(StompFrame).IsAssignableFrom(x) && !x.IsAbstract)
                                           .Select<Type, Tuple<String, Type>>(x => new Tuple<String, Type>(x.GetCustomAttribute<StompFrameType>()._FrameType, x))
                                           .ForEach(x => _FrameTypeMapping[x.Item1] = x.Item2);
        }

        /// <summary>
        ///     Connects to the listed server, disconnecting if currently connected.
        /// </summary>
        /// <param name="ServerAddress"></param>
        public void Connect(Uri ServerAddress)
        {
            if (_Client.Connected)
            {
                _Client.Close();
                CreateClient();
            }
            _Client.Connect(Dns.GetHostAddresses(ServerAddress.Host)[0], ServerAddress.Port);

            if (!_Client.Connected)
                throw new InvalidOperationException("Not connected to server");

            if (UseStompPacket)
            {
                SendFrame(new StompStompFrame(this, ServerAddress));
            }
            else
            {
                SendFrame(new StompConnectFrame(this, ServerAddress));
            }

            _PollThread = new Thread(Run);
            _PollThread.Start();
        }

        /// <summary>
        ///     Serializes and transmits a frame to the server
        /// </summary>
        /// <param name="Frame">
        ///     The <seealso cref="StompFrame"/> to send to the server 
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if
        ///     <list type="number">
        ///         <item>You attempt to serialize a frame class that has no <seealso cref="StompFrameType"/> attribute</item>
        ///         <item>You attempt to send a frame that is marked as server to client only</item>
        ///         <item>You fail to fill in a mandatory frame parameter</item>
        ///     </list>
        /// </exception>
        public void SendFrame(StompFrame Frame)
        {
            StompFrameType SFT = Frame.GetType().GetCustomAttribute<StompFrameType>();

            if (SFT == null)
                throw new InvalidOperationException("Attempt to serialize frame without frame type attribute");

            if (SFT._Direction == StompFrameDirection.ServerToClient)
                throw new InvalidOperationException("Attempt to send server frame from client");
            
            string FrameData = Frame.Serialize();
            byte[] Data = Encoding.UTF8.GetBytes(FrameData);

            lock (_Client)
            {
                _Client.GetStream().Write(Data, 0, Data.Length);
                _Client.GetStream().WriteByte(0);
                _Client.GetStream().Flush();
            }

            _HeartbeatTxIntervalTimeout = _HeartbeatTxInterval;
        }



        private void HandleMessageFrame(StompMessageFrame Frame)
        {
            StompMessageEventArgs e = new StompMessageEventArgs(Frame);

            try
            {
                MessageReceived(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            StompMessageFrame MF = (StompMessageFrame)Frame;
            if (e.SendNAck)
            {
                SendFrame(new StompNAckFrame(MF.MessageId));
            }
            else
            {
                SendFrame(new StompAckFrame(MF.MessageId));
            }
        }

        private void DispatchFrame(StompFrame Frame)
        {
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
                finally
                {

                }
            }
            else if (Frame is StompErrorFrame)
            {
                try
                {
                    ServerError(this, new StompErrorEventArgs((StompErrorFrame)Frame));
                }
                finally
                {

                }
            }
            else if (Frame is StompConnectedFrame)
            {
                // Handle connection configuration
                StompConnectedFrame SCF = (StompConnectedFrame)Frame;

                int[] Timings = SCF._Heartbeat.Split(',').Select<string, int>(x => int.Parse(x)).ToArray();

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
                finally
                {

                }
            }
        }

        private void Run()
        {
            byte[] RxData = new byte[512];
            NetworkStream Stream = _Client.GetStream();
            StompRingBuffer<byte> Buffer = new StompRingBuffer<byte>(RxBufferSize);

            while (_Client.Connected)
            {
                if (_Heartbeat > 0 && ConnectionVersion > 0.0f)
                {
                    int SleepAmt = Math.Min(_HeartbeatTxIntervalTimeout, 15);

                    if (_HeartbeatRxInterval > 0)
                    {
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

                    if (_HeartbeatTxInterval > 0)
                    {
                        _HeartbeatTxIntervalTimeout -= SleepAmt;
                        if (_HeartbeatTxIntervalTimeout < 0 && _HeartbeatTxInterval > 0)
                            _Client.GetStream().WriteByte((byte)'\n');
                    }

                    Thread.Sleep(SleepAmt);
                }
                else
                {
                    Thread.Sleep(15);
                }

                // Read in as much data from the stream as we can into the ring buffer
                while (_Client.Available > 0 && Buffer.Available > 0)
                {
                    _HeartbeatRxIntervalTimeout = (int)(_HeartbeatRxInterval * 1.5); // +50% forgiveness for heartbeat loss
                    int AmtRead = Stream.Read(RxData, 0, Buffer.Available);
                    Buffer.Write(RxData, AmtRead);
                }

                // Advance through any heartbeats rx'd or frame separators
                while (Buffer.Peek() == '\r' || Buffer.Peek() == '\n' || Buffer.Peek() == '\0')
                    Buffer.Read(1);

                // See if we have rx'd a packet separator
                // DOES NOT SUPPORT BINARY BLOBS
                int PacketLength = Buffer.DistanceTo(0);

                if (PacketLength > 0)
                {
                    String Packet = Encoding.UTF8.GetString(Buffer.Read(PacketLength));

                    StompFrame Frame = StompFrame.Build(Packet, _FrameTypeMapping);

                    DispatchFrame(Frame);


                    Buffer.Read(1);
                }
            }


        }

        private void CreateClient()
        {
            if (_PollThread != null && _PollThread.ThreadState == ThreadState.Running)
                _PollThread.Abort();

            _Client = new TcpClient();
        }

    }
}
