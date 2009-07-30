using System;
using System.Collections.Generic;
using System.Text;
using Wockets.Utils.network.Bluetooth;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Net.Sockets;
using System.Threading;

namespace Wockets.Utils.network.Bluetooth.Microsoft
{
    public class MicrosoftBluetoothStream:BluetoothStream
    {

        private BluetoothClient client;
        private Socket socket;
        private NetworkStream nstream;
        private BluetoothAddress btAddress;


        public MicrosoftBluetoothStream(byte[] buffer, byte[] address, string pin): base(buffer,address,pin)
        {
            try
            {   
                btAddress = new BluetoothAddress(this.address);
                if (this.pin != null)                                   
                    BluetoothSecurity.SetPin(btAddress, this.pin);
                
            }
            catch (Exception e)
            {
                this.errorMessage = "MicrosoftBluetoothStream failed at the constructor";
                this.status = BluetoothStatus.Error;                
            }
        }

        public override bool Open()
        {
            try
            {                                
                client = new BluetoothClient();    
                if (processingThread != null)
                    processingThread.Abort();
                //client.SetPin(this.pin);
                client.Connect(btAddress, BluetoothService.SerialPort);
                socket = client.Client;               
                socket.Blocking = true;
                nstream = client.GetStream();

                //start the processing thread
                processingThread = new Thread(new ThreadStart(Process));
                processingThread.Start();
                this.status = BluetoothStatus.Connected;      
            }
            catch (Exception e)
            {
                
                this.errorMessage = "MicrosoftBluetoothStream failed at opening a connection to " + btAddress.ToString();
                this.status = BluetoothStatus.Disconnected;
                return false;
            }
            return true;
        }
        public override void Process()
        {
            int sendTimer = 0;
            byte[] sendByte = new byte[1];
            sendByte[0] = 0xff;


            while (this.status == BluetoothStatus.Connected)
            {
                int bytesReceived = 0;

                try
                {

                    if (sendTimer > 200)
                    {
                        if (socket.Send(sendByte, 1, SocketFlags.None) <= 0)
                        {
                            this.errorMessage = "MicrosoftBluetoothStream failed at Process(). Cannot send bytes to " + btAddress.ToString();
                            this.status = BluetoothStatus.Disconnected;
                        }
                        sendTimer = 0;
                        Thread.Sleep(50);
                    }
                    sendTimer++;

                    int availableBytes = socket.Available;
                    if (availableBytes > 0)
                    {        
                        bytesReceived = 0;
                        //if we will pass the end of buffer receive till the end then receive the rest
                        if ((tail + availableBytes) > this.buffer.Length)
                        {
                            bytesReceived = socket.Receive(this.buffer, tail, this.buffer.Length - tail, SocketFlags.None);
                            availableBytes -= bytesReceived;
                            tail = (tail + bytesReceived) % this.buffer.Length;
                        }
                        bytesReceived += socket.Receive(this.buffer, tail, availableBytes, SocketFlags.None);
                        tail = (tail + bytesReceived) % this.buffer.Length;
                    }

                    Thread.Sleep(30);

                    if (bytesReceived > 0)
                        disconnectionCounter = 0;
                    else
                    {
                        disconnectionCounter++;
                        if (disconnectionCounter > MAX_DISCONNECTION_COUNTER)
                        {
                            this.errorMessage = "MicrosoftBluetoothStream failed at Process(). Disconnection timeout to " + this._HexAddress;
                            this.status = BluetoothStatus.Disconnected;
                        }

                    }


                }
                catch (Exception e)
                {
                    this.errorMessage = "MicrosoftBluetoothStream failed at Process(). " + e.Message + " to " + btAddress.ToString();
                    this.status = BluetoothStatus.Disconnected;

                }

            }

        }
        public override bool Close()
        {
            if (this.status!=BluetoothStatus.Down)
                this.status = BluetoothStatus.Down;
            processingThread.Join();
            if (processingThread != null)
                processingThread.Abort();
            try
            {
                nstream.Close();
            }
            catch (Exception)
            {
            }
            try
            {
                socket.Close();
            }           
            catch (Exception)
            {
            }
            try
            {
                client.Close();
            }
            catch (Exception)
            {
            }
            processingThread = null;
            nstream = null;
            socket = null;
            client = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return true;
        }
    }
}
