# STOMPClient

An easy to use, albeit not *entirely* STOMP-compliant STOMP client.
Includes various useful features such as inbuilt serialization of user types and classes designed to make custom extensions of the protocol as easy as defining the types
and calling a single function - once.

# Use

Using STOMPClient is easy:

1. Download, add to your solution, and add it to your project references.
2. Create a StompClient.STOMPClient object
3. Attach listeners to the events that are relevant to you
4. Set your properties and call Connect()

The library will take care of negotiating the connection, processing heartbeats, serialization and deserialization, and all other 'boilerplate' work.

# Known Issues

1. The client isn't fully STOMP 1.2 compliant, as it does not support binary blobs in frame bodies
2. Some useful events are missing, such as ClientDisconnected
3. Sending a message still requires manual creation of a StompSendFrame object, and should be folded into a convenience function
4. No Disconnect function

# Additional Classes

STOMPClient also includes a utility RingBuffer class that allows writing, reading, and limited seeking through the buffer.