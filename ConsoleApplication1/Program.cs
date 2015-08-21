using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StompClient;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            STOMPClient Client = new STOMPClient();

            NextTest("Frame Serialize, Deserialize, Serialize");

            StompSendFrame Frame = new StompSendFrame("MyDestination", "This is the packet body here");

            Frame.Receipt = "ReceiptIdHere";
            Frame.AdditionalHeaders.Add(new KeyValuePair<string, string>("my-header", "MyValue"));

            string Output = Client.GetSerializedPacket(Frame);

            Console.WriteLine(Output);

            StompFrame Rebuilt = Client.GetBuiltFrame(Output);

            Console.WriteLine("");
            Console.WriteLine("------");
            Console.WriteLine("");

            Output = Client.GetSerializedPacket(Rebuilt);
            Console.WriteLine(Output);


            Console.WriteLine("");
            Console.WriteLine("");
            NextTest("RingBuffer Data Length");

            StompRingBuffer<int> Buffer = new StompRingBuffer<int>(128);

            int[] Data = new int[64];
            for (int i = 0; i < Data.Length; i++)
                Data[i] = i + 1;

            Buffer.Write(Data);

            Console.WriteLine(string.Format("Available Write before read: {0}", Buffer.AvailableWrite));

            int[] Data2 = Buffer.Read(Buffer.AvailableRead);
            Console.WriteLine(string.Format("Available Write after read: {0}", Buffer.AvailableWrite));
            Console.WriteLine(string.Format("Lengths: {0} and {1}", Data.Length, Data2.Length));

            NextTest("Data Comparison");

            for (int i = 0; i < Math.Min(Data.Length, Data2.Length); i++)
                Console.WriteLine(string.Format("{0} {1}", Data[i], Data2[i]));

            NextTest("Seek Back 32");

            Console.WriteLine(string.Format("Seek Ptr is {0}", Buffer.Seek(-32)));
            Console.WriteLine(string.Format("Available is {0}", Buffer.AvailableRead));
            Data2 = Buffer.Read(Buffer.AvailableRead);
            Console.WriteLine(string.Format("Read out {0}", Data2.Length));
            Console.WriteLine(string.Format("Seek Ptr is {0}", Buffer.Seek(0)));

            NextTest("Old Data");

            
            foreach (int D in Data2)
                Console.WriteLine(string.Format("{0} ", D));

            NextTest("Write and Read");

            Buffer.Write(Data, 32);
            Buffer.Seek(-32);
            Data2 = Buffer.Read(Buffer.AvailableRead);
            Console.WriteLine(string.Format("Write 32; Read out {0}", Data2.Length));

            NextTest("Data");

            foreach (int D in Data2)
                Console.WriteLine(string.Format("{0} ", D));

            NextTest("Seek Forward");

            Console.WriteLine(string.Format("Available Read is {0}, should be 0", Buffer.AvailableRead));

            Console.WriteLine(string.Format("Write {0}", Data.Length));
            Buffer.Write(Data);

            Console.WriteLine(string.Format("Available Read is {0}", Buffer.AvailableRead));
            Buffer.Seek(10);
            Console.WriteLine(string.Format("Seek; Ptr is {0}", Buffer.Seek(0)));
            Console.WriteLine(string.Format("Available Read is {0}", Buffer.AvailableRead));

            Data2 = Buffer.Read(Buffer.AvailableRead);
            Console.WriteLine(string.Format("Read out {0}", Data2.Length, Data.Length));
            Console.WriteLine(string.Format("Available Read is {0}", Buffer.AvailableRead));

            Console.WriteLine(string.Format("Seek Ptr is {0}", Buffer.Seek(0)));

            NextTest("Data");

            foreach (int D in Data2)
                Console.WriteLine(string.Format("{0} ", D));

            Console.ReadLine();
        }

        public static void NextTest(string TestName)
        {
            Console.WriteLine("");
            Console.WriteLine(TestName);
            Console.ReadLine();
            Console.Clear();
        }
    }
}
