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
        protected Hashtable bluetoothStreams;

        public BluetoothStack()
        {
            this.status = BluetoothStatus.Down;
            this.bluetoothStreams = new Hashtable();
            
        }


        public static BluetoothStackTypes _Type
        {
            get
            {
                //Determine the type of Bluetooth Stack
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Bluetooth\\");
                RegistryKey rk2 = Registry.LocalMachine.OpenSubKey("Software\\WIDCOMM\\");
                if (rk2 != null)
                    type = BluetoothStackTypes.Widcomm;

                else if (rk != null)
                    type = BluetoothStackTypes.Microsoft;
                else
                {
                    rk = null;
                    rk = Registry.LocalMachine.OpenSubKey("Software\\WIDCOMM\\");
                    if (rk != null)
                        type = BluetoothStackTypes.Widcomm;
                    else
                        type = BluetoothStackTypes.Microsoft;
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
        public abstract BluetoothStream Connect(CircularBuffer buffer,CircularBuffer sbuffer, byte[] address, string pin);
        public abstract void Dispose();
    }
}
