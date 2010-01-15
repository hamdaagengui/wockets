using System;
using System.Xml;
using System.Collections;
using System.IO;
using Wockets.Data.Accelerometers;
using Wockets.Sensors;
using Wockets.Decoders;
using Wockets.Receivers;
using Wockets.Utils;
#if (PocketPC)
using Microsoft.ApplicationBlocks.MemoryMappedFile;
#endif

namespace Wockets.Sensors.Accelerometers
{
    public abstract class Accelerometer : Sensor
    {

        #region Serialization Constants

        protected const string CALIBRATION_ELEMENT = "CALIBRATION";
        protected const string RANGE_ELEMENT = "RANGE";


        protected const string MAX_ATTRIBUTE = "max";
        protected const string MIN_ATTRIBUTE = "min";
        protected const string X1G_ATTRIBUTE = "x1g";
        protected const string XN1G_ATTRIBUTE = "xn1g";
        protected const string Y1G_ATTRIBUTE = "y1g";
        protected const string YN1G_ATTRIBUTE = "yn1g";
        protected const string Z1G_ATTRIBUTE = "z1g";
        protected const string ZN1G_ATTRIBUTE = "zn1g";

        protected const string XSTD_ATTRIBUTE = "xstd";
        protected const string YSTD_ATTRIBUTE = "ystd";
        protected const string ZSTD_ATTRIBUTE = "zstd";
        #endregion Serialization Constants


        #region calibration data
        private double x1g;
        private double xn1g;
        private double y1g;
        private double yn1g;
        private double z1g;
        private double zn1g;
        private double xstd;
        private double ystd;
        private double zstd;
        private double min;
        private double max;
        #endregion calibration data

        #region IO storage variables
        private int presentHour = -1;
        private string dayPath = "";
        private ByteWriter bw = null;
        private string currentDataFile = "";
        private const string FILE_TYPE_PREFIX = "WocketsAccelBytes";
        private const string FILE_EXT = "PLFormat";
        private static int TIMESTAMP_AFTER_SAMPLES = 200;

        private int timeSaveCount = TIMESTAMP_AFTER_SAMPLES;
        private double aUnixTime = 0;
        private double lastUnixTime = 0;
        private bool isForceTimestampSave = true;
        private int flushTimer = 0;        
        public static int MAX_FLUSH_TIME = 2000;

        #endregion IO storage variables

#if (PocketPC)
        private MemoryMappedFileStream sdata = null;
        private MemoryMappedFileStream shead = null;
#endif
        private int sdataSize = 0;
        byte[] head = new byte[4];
        public Accelerometer(SensorClasses sensorclass):base(SensorTypes.ACCEL,sensorclass)
        {

        }



        public double _Min
        {
            get
            {
                return this.min;
            }

            set
            {
                this.min = value;
            }
        }

        public double _Max
        {
            get
            {
                return this.max;
            }

            set
            {
                this.max = value;
            }
        }
        public double _XSTD
        {
            get
            {
                return this.xstd;
            }
            set
            {
                this.xstd = value;
            }
        }

        public double _YSTD
        {
            get
            {
                return this.ystd;
            }
            set
            {
                this.ystd = value;
            }
        }

        public double _ZSTD
        {
            get
            {
                return this.zstd;
            }
            set
            {
                this.zstd = value;
            }
        }


        public double _X1G
        {
            get
            {
                return this.x1g;
            }
            set
            {
                this.x1g = value;
            }
        }

        public double _XN1G
        {
            get
            {
                return this.xn1g;
            }
            set
            {
                this.xn1g = value;
            }
        }


        public double _Y1G
        {
            get
            {
                return this.y1g;
            }
            set
            {
                this.y1g = value;
            }
        }

        public double _YN1G
        {
            get
            {
                return this.yn1g;
            }
            set
            {
                this.yn1g = value;
            }
        }


        public double _Z1G
        {
            get
            {
                return this.z1g;
            }
            set
            {
                this.z1g = value;
            }
        }

        public double _ZN1G
        {
            get
            {
                return this.zn1g;
            }
            set
            {
                this.zn1g = value;
            }
        }

        #region File Storage Methods
        /// <summary>
        /// Determine and create the directory where the raw data is saved in 1-hour chunks. 
        /// </summary>
        private void DetermineFilePath()
        {
            if (presentHour != DateTime.Now.Hour)
            {
                if (bw != null)
                    bw.CloseFile();
                presentHour = DateTime.Now.Hour;
                // Need to create a new directory and switch the file name
                dayPath = DirectoryStructure.DayDirectoryToUse(this._RootStorageDirectory);

                // Make sure hour directory exists 
                currentDataFile =  dayPath + "\\" + presentHour + "\\";
                if (!System.IO.Directory.Exists(currentDataFile))
                    System.IO.Directory.CreateDirectory(currentDataFile);

                currentDataFile = currentDataFile + FILE_TYPE_PREFIX + "." +
                               DirectoryStructure.GetDate() + "." + this._ID + "." + FILE_EXT;

                bw = new ByteWriter(currentDataFile, true);
                bw.OpenFile();

                // Ensure that the first data point in the new file will start
                // with the full, rather than differential, timecode info. 
                isForceTimestampSave = true;
            }

        }

        private int diffMS = 0;
        private byte diffMSByte = 0;

        private void WriteTimeDiff(double aUnixTime, double lastUnixTime, bool isForceTimeCodeSave)
        {

            diffMS = (int)(aUnixTime - lastUnixTime);

            // Save a full timestamp if forced
            // or time is > than 255 ms
            if (isForceTimeCodeSave || (diffMS > 254))
            {
                //if (diffMS >= 254)
                //    Console.WriteLine("Warning; Max on MS diff: " + diffMS);
                diffMSByte = (byte)255;
                bw.WriteByte(diffMSByte);
                WriteTimeStampPLFormat(aUnixTime, bw);
            }
            else // diff MS in range and no forced timestamp save
            {
                diffMSByte = (byte)diffMS;
                bw.WriteByte(diffMSByte);
            }

        }

        private byte[] retBytes = new byte[6];
        private void WriteTimeStampPLFormat(double unixTime, ByteWriter byteWriter)
        {
            WocketsTimer.GetUnixTimeBytes(unixTime, retBytes);
            byteWriter.WriteBytes(retBytes, 6);
        }
        #endregion File Storage Methods
        int tail=0;
        double tailUnixTimestamp=0;

        public override void Dispose()
        {

#if (PocketPC)
            if (sdata != null)
                sdata.Close();
            if (shead != null)
                shead.Close();
#endif
            if (bw != null)
            {
                bw.Flush();
                bw.CloseFile();
            }

            if (br != null)            
                br.CloseFile();
            
        }

       
        private int lastHead = 0;
        AccelerationData prevdata = null;
        AccelerationData data = new WocketsAccelerationData();
        byte[] timestamp = new byte[sizeof(double)];
        byte[] acc = new byte[sizeof(short)];
        public override void Save()
        {
#if (PocketPC)           
            if (_Saving)
            {
                if (sdata == null)
                {
                    this.sdataSize = (int)Decoder._DUSize * Wockets.Decoders.Accelerometers.WocketsDecoder.BUFFER_SIZE;
                    sdata = new MemoryMappedFileStream("\\Temp\\wocket" + this._ID + ".dat", "wocket" + this._ID, (uint)this.sdataSize, MemoryProtection.PageReadWrite);
                    shead = new MemoryMappedFileStream("\\Temp\\whead" + this._ID + ".dat", "whead" + this._ID, sizeof(int), MemoryProtection.PageReadWrite);

                    sdata.MapViewToProcessMemory(0, this.sdataSize);
                    shead.MapViewToProcessMemory(0, sizeof(int));
                    shead.Read(head, 0, 4);
                    int lastHead = BitConverter.ToInt32(head, 0);
                    tail = lastHead;
                    shead.Seek(0, System.IO.SeekOrigin.Begin);
                    sdata.Seek((lastHead * (sizeof(double) + 3 * sizeof(short))), System.IO.SeekOrigin.Begin);
                }

                flushTimer++;
                if (flushTimer >= MAX_FLUSH_TIME)
                {
                    bw.Flush();
                    bw.CloseFile();
                    bw = new ByteWriter(currentDataFile, false);
                    bw.OpenFile(false);
                    flushTimer = 0;
                }

                shead.Read(head, 0, 4);
                int currentHead = BitConverter.ToInt32(head, 0);
                shead.Seek(0, System.IO.SeekOrigin.Begin);
               
               /*if ((currentHead==0) || (((tail <= currentHead) && ((currentHead - tail) < (this._Decoder._Data.Length / 6))) ||
                    ((tail>currentHead) && ((this._Decoder._Data.Length -tail +currentHead)<(this._Decoder._Data.Length / 6)))))
                    return;
                else //save up to where the head points
                    currentHead--;
               
                 */              

                DetermineFilePath();
                

 
                //AccelerationData data = ((AccelerationData)this._Decoder._Data[tail]);
                while (tail != currentHead)// && (data.UnixTimeStamp > 0))
                {


                    sdata.Read(timestamp, 0, sizeof(double));
                    data.UnixTimeStamp = BitConverter.ToDouble(timestamp, 0);
                    sdata.Read(acc, 0, sizeof(short));
                    data.X = BitConverter.ToInt16(acc, 0);
                    sdata.Read(acc, 0, sizeof(short));
                    data.Y = BitConverter.ToInt16(acc, 0);
                    sdata.Read(acc, 0, sizeof(short));
                    data.Z = BitConverter.ToInt16(acc, 0);

                    aUnixTime = data.UnixTimeStamp;

                    if (aUnixTime < lastUnixTime)
                    {
                        string s = "Data overwritten without saving Accelerometer.cs Save " + this._ID + " " + aUnixTime + " " + lastUnixTime;
                        Logger.Error(s);
                        break;
                    }
        

                    // Roughly once per second save full timestamp, no matter what
                    if (isForceTimestampSave || (timeSaveCount == TIMESTAMP_AFTER_SAMPLES))
                    {
                        WriteTimeDiff(aUnixTime, lastUnixTime, true); // Force save
                        timeSaveCount = 0;
                    }
                    else
                    {
                        WriteTimeDiff(aUnixTime, lastUnixTime, false);
                        timeSaveCount++;
                    }

                    isForceTimestampSave = false;

                    // Save Raw Bytes                        
                    if (bw != null)
                        for (int j = 0; j < data.NumRawBytes; j++)
                            bw.WriteByte(data.RawBytes[j]);

                    lastUnixTime = aUnixTime;

                    this.tailUnixTimestamp = aUnixTime;
                    prevdata = data;
                    if (tail >= this._Decoder._Data.Length - 1)
                    {
                        tail = 0;
                        sdata.Seek(0, System.IO.SeekOrigin.Begin);
                    }
                    else
                        tail++;
                    this.SavedPackets++;
                    //data = ((AccelerationData)this._Decoder._Data[tail]);


                }                    
            }
#endif
        }

        ArrayList someBinaryFiles = new ArrayList();
        private ByteReader br;
        private double dTimeStamp = 0;
        private void GenerateBinaryFileList()
        {

            if (Directory.Exists(this._RootStorageDirectory) == false)
                return;

            string[] subdirectories = Directory.GetDirectories(this._RootStorageDirectory);
            foreach (string subdirectory in subdirectories)
            {
                for (int i = 0; i < 30; i++)
                {
                    if (Directory.Exists(subdirectory + "\\" + i))
                    {
                        string[] matchingFiles = Directory.GetFiles(subdirectory + "\\" + i, FILE_TYPE_PREFIX + "*" + this._ID + "." + FILE_EXT);
                        for (int j = 0; (j < matchingFiles.Length); j++)
                            someBinaryFiles.Add(matchingFiles[j]);
                    }
                }
            }
        }


        private bool SetupNextFiles(int index)
        {
            dTimeStamp = 0;


            if (br != null)
                br.CloseFile();


            br = null;

            string f = ((string)someBinaryFiles[index]);

            if (f != "")
            {
                br = new ByteReader(f);
                br.OpenFile();
                Console.WriteLine("Opening file for read: " + f);
            }

            if (br == null)
                return false;
            else
                return true;
        }

        private bool loading = false;
        private int fileIndex = 0;
        private byte[] b6 = new byte[6];
        private byte[] b = new byte[1];
        //Populates the decoder with the data read from the binary files
        public override bool Load()
        {
            //double lastUnixTime = aLastUnixTime; 
            //Generate the list of files to go through for this sensor
            if (loading == false)
            {
                GenerateBinaryFileList();
                if (someBinaryFiles.Count < 1)
                    return false;
                    //throw new Exception("Error: no PLFormat files for sensor " + this._ID);
                SetupNextFiles(0);
                loading = true;
            }

            while (true)
            {
                try
                {
                    #region Read Timestamp
                    if (!(br.ReadByte(b)))
                        throw new Exception("Error: reading first byte in PLFormat file");

                    //read a complete timestamp
                    if (b[0] == ((int)255))
                    {
                        if (!(br.ReadBytes(b6)))
                            throw new Exception("Error: reading full timestamp in PLFormat file");

                        lastUnixTime = WocketsTimer.DecodeUnixTimeCodeBytesFixed(b6);
                    }
                    else //read a differential timestamp          
                        lastUnixTime += (int)b[0];

                    #endregion Read Timestamp

                    DateTime dt = new DateTime();
                    WocketsTimer.GetDateTime((long)lastUnixTime, out dt);

                    int numRawBytes = 0;
                    if (this._Decoder._Type == DecoderTypes.Wockets)
                        numRawBytes = Data.Accelerometers.WocketsAccelerationData.NUM_RAW_BYTES;
                    else if (this._Decoder._Type == DecoderTypes.HTCDiamondTouch)
                        numRawBytes = Data.Accelerometers.HTCDiamondTouchAccelerationData.NUM_RAW_BYTES;

                    byte[] tempByte = new byte[numRawBytes];
                    if (!(br.ReadBytes(tempByte)))
                        throw new Exception("Error: reading data in PLFormat file");

       
                    int lastDecodedIndex = 0;
                    //Successfully decoded a packet
                    if (this._Decoder.Decode(this._ID, tempByte, tempByte.Length) == 1)
                    {

                        if (this._Decoder._Head == 0)
                            lastDecodedIndex = this._Decoder._Data.Length - 1;
                        else
                            lastDecodedIndex = this._Decoder._Head - 1;                        
                        this._Decoder._Data[lastDecodedIndex].UnixTimeStamp = lastUnixTime;                       
                        break;
                    }
                    else //failed to decode a packet... check if this ever happens
                        return false;
                }
                catch (Exception e)
                {
                    //if the data ended return false
                    if (!((++fileIndex < someBinaryFiles.Count) && SetupNextFiles(fileIndex)))
                        return false;
                }
            }

            return true;
        }
        protected string ToXML(string innerXML)
        {
            innerXML += "<" + CALIBRATION_ELEMENT + " " + X1G_ATTRIBUTE +
                "=\"" + this.x1g.ToString("0.00") + "\" " + XN1G_ATTRIBUTE + "=\"" + this.xn1g.ToString("0.00") +
                "\" " + Y1G_ATTRIBUTE + "=\"" + this.y1g.ToString("0.00") + "\" " +
                YN1G_ATTRIBUTE + "=\"" + this.yn1g.ToString("0.00") + "\" " +
                Z1G_ATTRIBUTE + "=\"" + this.z1g.ToString("0.00") + "\" " +
                ZN1G_ATTRIBUTE + "=\"" + this.zn1g.ToString("0.00") + "\" " +
                XSTD_ATTRIBUTE + "=\"" + this.xstd.ToString("0.00") + "\" " +
                YSTD_ATTRIBUTE + "=\"" + this.ystd.ToString("0.00") + "\" " +
                ZSTD_ATTRIBUTE + "=\"" + this.zstd.ToString("0.00") + "\" />\n";
            innerXML += "<" + RANGE_ELEMENT + " " + MAX_ATTRIBUTE +
                "=\"" + this.max.ToString("0.00") + "\" " + MIN_ATTRIBUTE + "=\"" + this.min.ToString("0.00") + "\" />\n";
            return base.ToXML(innerXML);
        }


        public override void FromXML(string xml)
        {
            base.FromXML(xml);
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNode iNode = dom.DocumentElement;
            if (iNode.Name == SENSOR_ELEMENT)
            {
                foreach (XmlNode jNode in iNode.ChildNodes)
                {
                    foreach (XmlAttribute jAttribute in jNode.Attributes)
                    {                     
                        if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == X1G_ATTRIBUTE))
                            this.x1g = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == XN1G_ATTRIBUTE))
                            this.xn1g = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == Y1G_ATTRIBUTE))
                            this.y1g = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == YN1G_ATTRIBUTE))
                            this.yn1g = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == Z1G_ATTRIBUTE))
                            this.z1g = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == ZN1G_ATTRIBUTE))
                            this.zn1g = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == XSTD_ATTRIBUTE))
                            this.xstd = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == YSTD_ATTRIBUTE))
                            this.ystd = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == CALIBRATION_ELEMENT) && (jAttribute.Name == ZSTD_ATTRIBUTE))
                            this.zstd = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == RANGE_ELEMENT) && (jAttribute.Name == MIN_ATTRIBUTE))
                            this._Min = Convert.ToDouble(jAttribute.Value);
                        else if ((jNode.Name == RANGE_ELEMENT) && (jAttribute.Name == MAX_ATTRIBUTE))
                            this._Max = Convert.ToDouble(jAttribute.Value);

                    }
                }
            }
        }

    }
}

