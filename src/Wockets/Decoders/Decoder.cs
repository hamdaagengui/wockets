using System;
using System.Collections.Generic;
using System.Text;
using Wockets.Data;
using Wockets.Utils;
#if (PocketPC)
using Microsoft.ApplicationBlocks.MemoryMappedFile;
#endif

namespace Wockets.Decoders
{
    public abstract class Decoder: XMLSerializable
    {

        #region Serialization Constants
        public const string DECODER_ELEMENT = "DECODER";
        protected const string TYPE_ATTRIBUTE = "type";
        protected const string ID_ATTRIBUTE = "id";
        #endregion Serialization Constants

        
        //format in the MMF file will be as follows
        //full time stamp (4 bytes), X (2 bytes), Y (2 bytes), Z (2 bytes)
        

        private SensorData[] data;
        private SensorData[] response;
        private int size;                
        protected byte[] packet;
        protected int packetPosition;
        private int id;
        private int index;
        private static int IDCounter = 0;
        protected DecoderTypes type;
        protected int head = 0;
        protected int delIndex = 0;
        protected Response.ResponseHandler[] delegates= new Response.ResponseHandler[20];
        protected bool[] subscribed = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        protected bool cmd = false;

       
        
        
        public int MaxedoutSamples = 0;

        protected int TotalMaxedout5Minutes = 0;
        protected int TotalSamples5Minutes = 0;

        public int LastMaxedout5Minutes = 0;
        public int LastSamples5Minutes = 0;

        public long TotalMaxedOutSamples = 0;
        public long TotalSamples = 0;

        #if (PocketPC)
        protected MemoryMappedFileStream sdata = null;
        protected MemoryMappedFileStream shead = null;
        protected int sdataSize = 0;       
        public static uint _DUSize = sizeof(short) * 3 + sizeof(double);
        #endif


        public Decoder()
        {
        }
        public Decoder(int bufferSize,int packetSize)
        {
            this.data = new SensorData[bufferSize];
            this.response = new SensorData[10];
            this.size = 0;
            this.packet = new byte[packetSize];
            this.packetPosition = 0;
            this.id = 0;
            this.index = 0;
            this.head = 0;           
      
        }

        #region Access Properties

        public int _Head
        {
            get
            {
                return this.head;
            }
        }
        public DecoderTypes _Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }
        public int _ID
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
            }
        }
        public int _Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }

        public SensorData[] _Data
        {
            get
            {
                return this.data;
            }
        }

          public SensorData[] _Response
        {
            get
            {
                return this.response;
            }
        }
        #endregion Access Properties

        protected void FireEvent(Response.ResponseArgs e)
        {
            if (this.subscribed[(int)e._Response.Type])
                this.delegates[(int)e._Response.Type](this, e);
        }

        public void Subscribe(SensorDataType type, Response.ResponseHandler handler)
        {
            this.subscribed[(int)type] = true;
            this.delegates[(int)type] = handler;
        }

        public void Unsubscribe(SensorDataType type, Response.ResponseHandler handler)
        {
            this.subscribed[(int)type] = false;
        }

        public bool cmdMode
        {
            get
            {
                return this.cmd;
            }
            set
            {
                this.cmd = value;
            }
        }

        public abstract int Decode(int sensorID,byte[] data, int length);
        public abstract int Decode(int sensorID, CircularBuffer data, int start,int end);
        
        public bool Dispose()
        {
#if (PocketPC)
            if (this.sdata!=null)
                this.sdata.Close();
            if (this.shead!=null)
                this.shead.Close();
#endif
            return true;
        }

        //Serialization
        public abstract string ToXML();
        public abstract void FromXML(string xml);
    }
}

