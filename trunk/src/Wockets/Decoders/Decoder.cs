using System;
using System.Collections.Generic;
using System.Text;
using Wockets.Data;
using Wockets.Utils;

namespace Wockets.Decoders
{
    public abstract class Decoder: XMLSerializable
    {

        #region Serialization Constants
        public const string DECODER_ELEMENT = "DECODER";
        protected const string TYPE_ATTRIBUTE = "type";
        protected const string ID_ATTRIBUTE = "id";
        #endregion Serialization Constants


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
            this.id = IDCounter++;
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

        public abstract int Decode(int sensorID,byte[] data, int length);
        public abstract int Decode(int sensorID, byte[] data, int head,int tail,double samplespacing,double lasttimestamp);
        //public abstract bool IsValid(SensorData data);

        //Serialization
        public abstract string ToXML();
        public abstract void FromXML(string xml);
    }
}