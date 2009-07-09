using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Net.Sockets;
using System.IO.Ports;
using HousenCS.SerialIO;
using System.Runtime.InteropServices;
using System.Threading;
using Wockets.Utils;
using System.Net;

#if (PocketPC)
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Ports;
#endif

namespace Wockets.Receivers
{
    public sealed class RFCOMMReceiver : SerialReceiver, Radio_CMD
    {
        #region Serialization Constants
        private const string RFCOMM_TYPE = "RFCOMM";
        private const string MACADDRESS_ATTRIBUTE = "MacAddress";
        private const string PIN_ATTRIBUTE = "PIN";
        private const string TSNIFF_ATTRIBUTE = "TSniff";
        #endregion Serialization Constants
        //RFCOMM Configuration
        private const bool USE_PARITY = false;
        private const bool USE_STOP_BIT = true;
        private const int BAUD_RATE = 57600;
        private const int BUFFER_SIZE = 4096;
        private const int PORT_NUMBER = 9;
        private const int MAXIMUM_SAMPLING_RATE = 70;

        //RFCOMM Specific Objects
#if (PocketPC)
        private BluetoothStream bluetoothStream;
#endif
        private const int MAC_SIZE = 6;
        private string address;
        private byte[] address_bytes;
        private string pin;
        private int sniffTime = 0;
        private bool sniffMode;

        public RFCOMMReceiver()
        {
            this.type = ReceiverTypes.RFCOMM;
        }
        /*
        public RFCOMMReceiver(string address,string pin)
            : base(BUFFER_SIZE, PORT_NUMBER, BAUD_RATE, USE_PARITY, USE_STOP_BIT,MAXIMUM_SAMPLING_RATE)
        {            
            this.address = address;
            this.address_bytes = new byte[MAC_SIZE];
            for (int i = 0; (i < MAC_SIZE); i++)
                this.address_bytes[i] = (byte)(System.Int32.Parse(address.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier) & 0xff);
            this.pin = pin;
        }
         */
        #region Access Properties
        public byte[] _AddressBytes
        {
            get
            {
                return this.address_bytes;
            }
        }
        public string _Address
        {
            get
            {
                return this.address;
            }
        }
        public string _PIN
        {
            get
            {
                return this.pin;
            }
        }

        public int _TSNIFF
        {
            get
            {
                return this.sniffTime;
            }

            set
            {
                this.sniffTime = value;
            }
        }
        #endregion Access Properties

        public override bool Initialize()
        {

            //instead setup the bluetooth connection over here
            // instatiate a BT End Point
            // Create a socket
            // Connect
            // Maintain the socket
            // Close the network stream + socket to do the clean up

            try
            {
#if (PocketPC)
                this.bluetoothStream = BluetoothStream.OpenConnection(this.address_bytes, this.pin);
#endif
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

#if (PocketPC)
        public ArrayList BatchTimestamps
        {
            get
            {
                return this.bluetoothStream.BatchTimestamps;
            }
            set
            {
                this.bluetoothStream.BatchTimestamps = value;
            }
        }


        public double _LastTimestamps
        {
            get
            {
                return this.bluetoothStream.LastTimestamp;
            }
            set
            {
                this.bluetoothStream.LastTimestamp = value;
            }
        }
        public byte[] ReceiverBuffer
        {
            get
            {
                if (this.bluetoothStream.IsDisposed)
                    throw new ObjectDisposedException("BluetoothStream");
                return this.bluetoothStream.ReceiverBuffer;
            }
        }


        public int _Head
        {
            get
            {
                if (this.bluetoothStream.IsDisposed)
                    throw new ObjectDisposedException("BluetoothStream");
                return this.bluetoothStream.Head;
            }
            set
            {
                this.bluetoothStream.Head = value;
            }
        }

        public int _Tail
        {
            get
            {
                if (this.bluetoothStream.IsDisposed)
                    throw new ObjectDisposedException("BluetoothStream");
                return this.bluetoothStream.Tail;
            }
            set
            {
                this.bluetoothStream.Tail = value;
            }
        }
        public ArrayList BatchBytes
        {
            get
            {
                return this.bluetoothStream.BatchBytes;
            }
            set
            {
                this.bluetoothStream.BatchBytes = value;
            }
        }
#endif
        public override int Read()
        {
#if (PocketPC)
            return this.bluetoothStream.Read(this._Buffer, 0, this._Buffer.Length);
#else
            return 0;
#endif
        }

        public override void Write(byte[] data, int length)
        {
#if (PocketPC)
            this.bluetoothStream.Write(data, 0, length);
#endif
        }
        public override bool Dispose()
        {
            try
            {
#if (PocketPC)
                this.bluetoothStream.Close();
#endif
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region Radio Commands
#if (PocketPC)
        private void EnterCMD()
        {
            byte[] cmd = new byte[3];
            for (int i = 0; (i < 3); i++)
                cmd[i] = (byte)36;
            this.bluetoothStream.Write(cmd, 0, 3);
        }

        private void ExitCMD()
        {
            byte[] cmd = new byte[3];
            for (int i = 0; (i < 3); i++)
                cmd[i] = (byte)'-';
            this.bluetoothStream.Write(cmd, 0, 3);
        }


#endif

        public void Reset()
        {
#if (PocketPC)
            byte[] cmd = new byte[4];
            cmd[0] = (byte)'R';
            cmd[1] = (byte)',';
            cmd[2] = (byte)'1';
            cmd[3] = (byte)13;
            this.bluetoothStream.Write(cmd, 0, 4);
#endif
        }
        public bool LowPower
        {
            get
            {
                return this.sniffMode;
            }

            set
            {
#if (PocketPC)
                if (value != this.sniffMode)
                {
                    if (value)
                    {
                        this.EnterCMD();
                        Thread.Sleep(100);
                        byte[] cmd = new byte[8];
                        cmd[0] = (byte)'S';
                        cmd[1] = (byte)'W';
                        cmd[2] = (byte)',';
                        cmd[3] = (byte)'0';
                        cmd[4] = (byte)'6';
                        cmd[5] = (byte)'4';
                        cmd[6] = (byte)'0';
                        cmd[7] = (byte)13;
                        this.bluetoothStream.Write(cmd, 0, 8);
                        Thread.Sleep(100);
                        this.Reset();
                    }
                    else
                    {
                        this.EnterCMD();
                        Thread.Sleep(100);
                        byte[] cmd = new byte[8];
                        cmd[0] = (byte)'S';
                        cmd[1] = (byte)'W';
                        cmd[2] = (byte)',';
                        cmd[3] = (byte)'0';
                        cmd[4] = (byte)'0';
                        cmd[5] = (byte)'0';
                        cmd[6] = (byte)'0';
                        cmd[7] = (byte)13;
                        this.bluetoothStream.Write(cmd, 0, 8);
                        Thread.Sleep(100);
                        this.Reset();

                    }
                }
#endif
            }
        }
        #endregion Radio Commands


        #region Serialization Methods
        public override string ToXML()
        {
            string xml = "<" + RFCOMMReceiver.RECEIVER_ELEMENT + " ";
            xml += RFCOMMReceiver.ID_ATTRIBUTE + "=\"" + this._ID + "\" ";
            xml += RFCOMMReceiver.TYPE_ATTRIBUTE + "=\"" + RFCOMMReceiver.RFCOMM_TYPE + "\" ";
            xml += RFCOMMReceiver.MACADDRESS_ATTRIBUTE + "=\"" + this.address + "\" ";
            xml += RFCOMMReceiver.PIN_ATTRIBUTE + "=\"" + this.pin + "\" ";
            xml += RFCOMMReceiver.TSNIFF_ATTRIBUTE + "=\"" + this.sniffTime + "\" ";
            xml += RFCOMMReceiver.PORT_NUMBER_ATTRIBUTE + "=\"" + this._PortNumber + "\" ";
            xml += RFCOMMReceiver.PARITY_ATTRIBUTE + "=\"" + this._Parity + "\" ";
            xml += RFCOMMReceiver.STOPBIT_ATTRIBUTE + "=\"" + this._StopBit + "\" ";
            xml += RFCOMMReceiver.BAUD_RATE_ATTRIBUTE + "=\"" + this._BaudRate + "\" ";
            xml += RFCOMMReceiver.BUFFERSIZE_ATTRIBUTE + "=\"" + this._Buffer.Length + "\" ";
            xml += RFCOMMReceiver.MAX_SR_ATTRIBUTE + "=\"" + this._MaximumSamplingRate + "\" ";
            xml += "/>";
            return xml;
        }
        public override void FromXML(string xml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNode xNode = dom.DocumentElement;

            if ((xNode.Name == RFCOMMReceiver.RECEIVER_ELEMENT))
            {
                foreach (XmlAttribute xAttribute in xNode.Attributes)
                {

                    if ((xAttribute.Name == RFCOMMReceiver.TYPE_ATTRIBUTE) && (xAttribute.Value != RFCOMMReceiver.RFCOMM_TYPE))
                        throw new Exception("XML Parsing error - RFCOMM receiver parsing a receiver of a different type " + xAttribute.Value);
                    else if (xAttribute.Name == RFCOMMReceiver.MACADDRESS_ATTRIBUTE)
                    {
                        this.address = xAttribute.Value;
                        this.address_bytes = new byte[MAC_SIZE];
                        for (int i = 0; (i < MAC_SIZE); i++)
                            this.address_bytes[i] = (byte)(System.Int32.Parse(address.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier) & 0xff);
                    }
                    else if (xAttribute.Name == RFCOMMReceiver.PIN_ATTRIBUTE)
                        this.pin = xAttribute.Value;
                    else if (xAttribute.Name == RFCOMMReceiver.PORT_NUMBER_ATTRIBUTE)
                        this._PortNumber = Convert.ToInt32(xAttribute.Value);
                    else if (xAttribute.Name == RFCOMMReceiver.TSNIFF_ATTRIBUTE)
                        this._TSNIFF = Convert.ToInt32(xAttribute.Value);
                    else if (xAttribute.Name == RFCOMMReceiver.PARITY_ATTRIBUTE)
                    {
                        if (xAttribute.Value == "true")
                            this._Parity = true;
                        else
                            this._Parity = false;
                    }
                    else if (xAttribute.Name == RFCOMMReceiver.STOPBIT_ATTRIBUTE)
                    {
                        if (xAttribute.Value == "true")
                            this._StopBit = true;
                        else
                            this._StopBit = false;
                    }
                    else if (xAttribute.Name == RFCOMMReceiver.BAUD_RATE_ATTRIBUTE)
                        this._BaudRate = Convert.ToInt32(xAttribute.Value);
                    else if (xAttribute.Name == RFCOMMReceiver.BUFFERSIZE_ATTRIBUTE)
                        this._Buffer = new byte[Convert.ToInt32(xAttribute.Value)];
                    else if (xAttribute.Name == RFCOMMReceiver.MAX_SR_ATTRIBUTE)
                        this._MaximumSamplingRate = Convert.ToInt32(xAttribute.Value);
                    else if (xAttribute.Name == RFCOMMReceiver.ID_ATTRIBUTE)
                        this._ID = Convert.ToInt32(xAttribute.Value);

                }
            }
        }
        #endregion Serialization Functions
    }


#if (PocketPC)
    internal class BluetoothStream //: IDisposable
    {
        private static bool usingWidcomm;
        //all instances of BluetoothStream lock on this object
        private static object lockObject;
        private const int DEFAULT_READ_TIMEOUT = 100;//100 ms
        private const int DEFAULT_WRITE_TIMEOUT = 100;
        private const int MAX_TIMEOUTS = 10;//combined with 50 ms sleep time between reads, this amounts to .5 seconds with no data at all, very unlikely to happen normally

        private const int DEFAULT_BUFFER_SIZE = 8000;
        private static Predicate<DateTime> oldEnoughPredicate = new Predicate<DateTime>(isNewEnough);
        private static TimeSpan timeoutExceptionsOldnessCutoff = TimeSpan.FromSeconds(1);
        private static List<BluetoothStream> openStreams = new List<BluetoothStream>();
        //private static Thread readingThread = new Thread(new ThreadStart(readingLoop));
        private static Dictionary<BluetoothStream, int> timeouts = new Dictionary<BluetoothStream, int>();

        private Thread readingThread;

        #region MS_Stack_variables
        private BluetoothClient btClient;
        //private NetworkStream ms_stream;
        private Socket btSocket;
        private SerialPort sport;
        private bool disposed = false;
        private byte[] localBuffer;
        //this is the buffer used to read asynchonously from the socket. When
        //the asynchronous read returns, this is copied into the localBuffer.
        private byte[] singleReadBuffer;
        private int head = 0;
        private int tail = 0;
        //signal from the asynchronous reading functions to the synchronous (external)
        //reading functions that the socket is dead and the stream should throw
        //an exception
        private bool socketDead = false;
        private bool timeOut = false;
        #endregion

        #region Widcomm_Stack_variables
        private string comPortName;
        private SerialPort comPort;
        private SerialPortController comPort2;
        #endregion


        /// <summary>
        /// Synchronization Barrier for Fairness
        /// </summary>

        private static Barrier barrier = null;
        static BluetoothStream()
        {
            usingWidcomm = BluetoothRadio.PrimaryRadio == null;
            lockObject = new object();
            //if (barrier == null)
            //  barrier = new Barrier(0);
        }

        private BluetoothStream()
        {

        }

        ~BluetoothStream()
        {
            Dispose();
        }

        int prevData = 0;
        //private TextWriter ttw = null;
        IntPtr cthread;

        private static int iii = 0;
        NetworkStream n;
        public static void Read_Callback(IAsyncResult ar)
        {

            //BluetoothStream so = ( BluetoothStream)ar.AsyncState;
            //so.bytesReceived= so.btSocket.EndReceive(ar);
            //so.receiving = false;
        }

        //public int bytesReceived = 0;
        //public bool receiving = false;
        byte[] xxx = new byte[1];
        double sendTimer = 0;

        //Use a counter to avoid calling the timer function
        private int disconnectionCounter = 0;
        private const int MAX_DISCONNECTION_COUNTER = 200; //approximately consider disconnected if 10 sec passes with no data

        ArrayList batchTimestamps;
        ArrayList batchBytes;
        //int unprocessedReceiveCount;

        public bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }

        public ArrayList BatchTimestamps
        {
            get
            {
                return this.batchTimestamps;
            }

            set
            {
                this.batchTimestamps = value;
            }
        }

        public ArrayList BatchBytes
        {
            get
            {
                return this.batchBytes;
            }

            set
            {
                this.batchBytes = value;
            }
        }


        public int Head
        {
            get
            {
                return this.head;
            }

            set
            {
                this.head = value;
            }
        }
        public int Tail
        {
            get
            {
                return this.tail;
            }

            set
            {
                this.tail = value;
            }
        }

        public byte[] ReceiverBuffer
        {
            get
            {
                return this.singleReadBuffer;
            }
            set
            {
                this.singleReadBuffer = value;
            }
        }
        private double lastTimestamp=0;
        public double LastTimestamp
        {
            get
            {
                return this.lastTimestamp;
            }
            set
            {
                this.lastTimestamp = value;
            }
        }
        private void readingFunction()
        {
            //double prevTime = 0;
            //double currentTime=0;
            byte[] buffer = new byte[100];

            //double nodataTimer = WocketsTimer.GetUnixTime();
            int sendTimer = 0;
            byte[] sendByte = new byte[1];
            sendByte[0] = 0xff;

            //hack for battery
            int batteryTimer = 0;
            byte[] batteryByte = new byte[1];
            batteryByte[0] = 0xA0;


            n = btClient.GetStream();
            //localBuffer = new byte[DEFAULT_BUFFER_SIZE];
            singleReadBuffer = new byte[DEFAULT_BUFFER_SIZE];

            //TextWriter tttw = new StreamWriter("samples"+(iii++)+".csv");



            batchTimestamps = new ArrayList();
            batchBytes = new ArrayList();

            while (!disposed)
            {
                if (usingWidcomm)
                {
                    //TODO FIXME
                }
                else
                {
                    if (!btClient.Connected)
                        return;

                    int bytesReceived = 0;
                    bool readHappened = false;


                    try
                    {

                        readHappened = true;

                        try
                        {


                            if (sendTimer > 200)
                            {

                                if (btSocket.Send(sendByte, 1, SocketFlags.None) <= 0)
                                    throw new Exception("send: socket timed out");
                                sendTimer = 0;
                                Thread.Sleep(50);

                            }
                            sendTimer++;

                            /*
                            if (batteryTimer > 2000)
                            {

                                btSocket.Send(batteryByte, 1, SocketFlags.None);
                                batteryTimer = 0;
                                Thread.Sleep(50);

                            }
                            batteryTimer++;
                             */

                            int availableBytes = btSocket.Available;
                            if (availableBytes > 0)
                            {
                                int currentHead = head;
                                this.lastTimestamp = WocketsTimer.GetUnixTime();
                                bytesReceived = 0;
                                //if we will pass the end of buffer receive till the end then receive the rest
                                if ((tail + availableBytes) > singleReadBuffer.Length)
                                {
                                    bytesReceived = btSocket.Receive(singleReadBuffer, tail, singleReadBuffer.Length - tail, SocketFlags.None);
                                    availableBytes -= bytesReceived;
                                    tail = (tail + bytesReceived) % DEFAULT_BUFFER_SIZE;
                                }
                                bytesReceived += btSocket.Receive(singleReadBuffer, tail, availableBytes, SocketFlags.None);
                                //batchTimestamps.Add(currentTime);
                                //batchBytes.Add(bytesReceived);


                                //int origTail = tail;
                                tail = (tail + bytesReceived) % DEFAULT_BUFFER_SIZE;
                                //if (((origTail < head) && (tail >= head)) ||
                                 //     (origTail > head) && (tail < origTail) && (tail >= head))
                                  //  throw new Exception("Overflow");

                                
                            }

                            Thread.Sleep(30);

                            if (bytesReceived > 0)
                                disconnectionCounter = 0;
                            else
                            {
                                disconnectionCounter++;
                                if (disconnectionCounter > MAX_DISCONNECTION_COUNTER)
                                    throw new Exception("socket timed out");
                            }


                        }
                        catch (Exception e)
                        {
                            socketDead = true;
                            if (e.Message.Equals("socket timed out")) this.timeOut = true;
                            else this.timeOut = false;
                            Dispose();
                            throw new Exception(e.Message);
                        }


                    }
                    catch (Exception e)
                    {

                        return;
                    }

                }

            }



        }

        // Bluetooth Parameters
        private static InTheHand.Net.BluetoothAddress blt_address;
        private static BluetoothClient blt;
        private static BluetoothEndPoint blt_endPoint;
        private static int prevPort = 1;
        public static string prepareCOMport(byte[] addr, string pin)
        {
            if (!usingWidcomm)
            {
                BluetoothRadio.PrimaryRadio.Mode = RadioMode.Connectable;
                byte[] reverseAddr = new byte[addr.Length];
                for (int ii = 0; ii < addr.Length; ii++)
                {
                    reverseAddr[reverseAddr.Length - 1 - ii] = addr[ii];
                }
                blt_address = new BluetoothAddress(reverseAddr);

                if (pin != null)
                    BluetoothSecurity.SetPin(blt_address, pin);

                blt_endPoint = new BluetoothEndPoint((BluetoothAddress)blt_address, BluetoothService.SerialPort);
                BluetoothSerialPort newPort = BluetoothSerialPort.CreateClient(blt_endPoint);
                /*BluetoothSerialPort newPort =null;
                
                for (int j = prevPort; (j < 100); j++)
                {
                    try
                    {
                        newPort = BluetoothSerialPort.CreateClient("COM"+j, blt_endPoint);
                        prevPort=j;
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }*/

                if (newPort != null)
                    return newPort.PortName;
                else
                    throw new Exception("Got a null pointer from the Microsoft code");



            }
            else
            {
                IntPtr stringPtr = prepareCOMportWidcomm(addr);
                if (stringPtr != IntPtr.Zero)
                    return Marshal.PtrToStringUni(stringPtr);
                else
                    throw new Exception("Got a null pointer from the WIDCOMM code");
            }

        }



        /// <summary>
        /// Opens a Bluetooth connection with the specified address and returns
        /// a BluetoothStream object which can be used to communicate over that
        /// connection
        /// </summary>
        /// <param name="addr">The MAC address of the remote bluetooth device. 
        /// It <b>MUST</b> be in most-significant-byte first
        /// order (i.e. the bluetooth address 00:f1:ad:34:3d:f3 would be
        /// { 0x00, 0xf1, ...} and NOT {0xf3, 0x3d, ...})</param>
        /// <param name="pin">An optional pin for the bluetooth device</param>
        /// <returns></returns>
        public static BluetoothStream OpenConnection(byte[] addr, string pin)
        {
            BluetoothStream newStream = new BluetoothStream();

            try
            {

                if (usingWidcomm)
                {
                    bool canStart = initializeWidcommBluetooth();
                    if (!canStart)
                        throw new Exception("Couldn't instantiate the Widcomm object in C++");
                    IntPtr stringPtr = prepareCOMportWidcomm(addr);
                    if (stringPtr != IntPtr.Zero)
                        newStream.comPortName = Marshal.PtrToStringUni(stringPtr);
                    else
                        throw new Exception("Got a null pointer from the WIDCOMM code");

                    //now open the port
                    newStream.comPort = new SerialPort(newStream.comPortName);

                    newStream.comPort.Open();
                }
                else
                {

                    if (newStream.readingThread != null)
                        newStream.readingThread.Abort();
                    newStream.btClient = new BluetoothClient();
                    byte[] reverseAddr = new byte[addr.Length];
                    for (int ii = 0; ii < addr.Length; ii++)
                    {
                        reverseAddr[reverseAddr.Length - 1 - ii] = addr[ii];
                    }


                    newStream.localBuffer = new byte[DEFAULT_BUFFER_SIZE];
                    newStream.singleReadBuffer = new byte[DEFAULT_BUFFER_SIZE];
                    //lock (lockObject)
                    // {
                    BluetoothRadio.PrimaryRadio.Mode = RadioMode.Connectable;
                    BluetoothAddress bt_addr = new BluetoothAddress(reverseAddr);
                    if (pin != null)
                        BluetoothSecurity.SetPin(bt_addr, pin);


                    newStream.btClient.Connect(bt_addr, BluetoothService.SerialPort);
                    newStream.btSocket = newStream.btClient.Client;
                    newStream.btSocket.Blocking = true;

                    //}


                }


                newStream.readingThread = new Thread(new ThreadStart(newStream.readingFunction));
                //newStream.readingThread.Priority = ThreadPriority.Highest;
                newStream.readingThread.Start();
                /* lock (lockObject)
                 {
                     if (barrier != null)                    
                         barrier.NumSynchronizedThreads = barrier.NumSynchronizedThreads + 1;                    
                 }*/

            }
            catch (Exception e)
            {
                newStream.disposed = true;
                throw;
            }
            return newStream;
        }



        public int Read(byte[] destination, int offset, int length)
        {
            if (disposed)
                if (this.timeOut) throw new ObjectDisposedException("timed out");
                else throw new ObjectDisposedException("disconnected");


            if (usingWidcomm)
            {
                return comPort.Read(destination, offset, length);
            }
            else
            {
                if (socketDead)
                {
                    Dispose();
                    if (this.timeOut) throw new Exception("timed out");
                    else throw new Exception("disconnected");
                }

                if (tail == head)
                    return 0;

                lock (this)
                {


                    int bytesCopied;
                    for (bytesCopied = 0; head != tail && bytesCopied < length; head = (head + 1) % DEFAULT_BUFFER_SIZE)
                    {
                        destination[bytesCopied + offset] = localBuffer[head];
                        bytesCopied++;
                    }
                    return bytesCopied;
                    //return btSocket.Receive(destination, offset, length, SocketFlags.None);//ms_stream.Read(destination, offset, length);
                }
            }


        }

        public void Write(byte[] buffer, int offset, int length)
        {
            if (disposed)
                throw new ObjectDisposedException("BluetoothStream");
            try
            {
                if (usingWidcomm)
                {
                    comPort.Write(buffer, offset, length);
                }
                else
                {
                    //lock (lockObject)
                    btSocket.Send(buffer, offset, length, SocketFlags.None);//ms_stream.Write(buffer, offset, length);
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Close()
        {
            //n.Close();
            Dispose();
            //ttw.Flush();
            // ttw.Close();
        }

        private static bool isNewEnough(DateTime timestamp)
        {
            return DateTime.Now.Subtract(timestamp) < timeoutExceptionsOldnessCutoff;
        }

        //[DllImport("nk.dll")]
        //public static extern IntPtr GetCurrentThread();

        //[DllImport("coredll.dll", EntryPoint = "CeSetThreadPriority", SetLastError = true)]
        //public static extern bool CeSetThreadPriority(IntPtr hThread, int nPriority);


        //[DllImport("coredll.dll", EntryPoint = "CeGetThreadPriority", SetLastError = true)]
        //public static extern int CeGetThreadPriority(IntPtr hThread); 


        [DllImport("WidcommWrapper.dll", CharSet = CharSet.Auto, EntryPoint = "?prepareCOMport@@YAPA_WQAE@Z")]
        private static extern IntPtr prepareCOMportWidcomm(byte[] addr);

        [DllImport("WidcommWrapper.dll", CharSet = CharSet.Auto, EntryPoint = "?instantiateBluetoothClient@@YAHXZ")]
        private static extern bool initializeWidcommBluetooth();

        [DllImport("WidcommWrapper.dll", CharSet = CharSet.Auto, EntryPoint = "?destroyBluetoothClient@@YAXXZ")]
        private static extern void destroyWidcommBluetooth();

        [DllImport("WidcommWrapper.dll", CharSet = CharSet.Auto, EntryPoint = "?setPin@@YAHQAEPA_W@Z")]
        private static extern bool setPinWidcomm(byte[] addr, String pin);

        #region IDisposable Members

        public void Dispose()
        {
            lock (this)
            {
                if (disposed)
                    return;
                disposed = true;
            }

            //readingThread.Join();
            //readingThread.Abort();

            if (usingWidcomm)
            {
                //TODO FIXME
            }
            else
            {


                n.Close();
                btSocket.Close();
                btClient.Close();

                //ms_stream = null;
                btSocket = null;
                btClient = null;
                n = null;


                //BluetoothRadio.PrimaryRadio.Mode = RadioMode.Connectable;

            }
            //readingThread.Abort();
        }

        #endregion
    }
#endif
}