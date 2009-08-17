using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Microsoft.Win32; 

namespace Wockets.Utils.network.Bluetooth
{
    public abstract class BluetoothStack
    {
        protected BluetoothStatus status;
        private static BluetoothStackTypes type;
        //protected BluetoothStreamList bluetoothStreams;
        protected Hashtable bluetoothStreams;

        public BluetoothStack()
        {
            this.status = BluetoothStatus.Down;
            //this.bluetoothStreams = new BluetoothStreamList();
            this.bluetoothStreams = new Hashtable();
            
        }


        public static BluetoothStackTypes _Type
        {
            get
            {
                //Determine the type of Bluetooth Stack
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\WIDCOMM\\");
                if (rk != null)
                    type = BluetoothStackTypes.Widcomm;
                else
                {
                    rk = null;
                    rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Bluetooth\\");
                    if (rk != null)
                        type = BluetoothStackTypes.Microsoft;
                    else
                        type = BluetoothStackTypes.Unknown;
                }
                return type;
            }
        }
        public abstract BluetoothStatus _Status
        {
            get;
        }


        public abstract bool Initialize();
        public abstract Hashtable Search();
        public abstract BluetoothStream Connect(byte[] buffer,CircularBuffer sbuffer, byte[] address, string pin);
        public abstract void Dispose();
    }
}
