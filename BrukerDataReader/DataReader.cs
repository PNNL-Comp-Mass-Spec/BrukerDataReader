using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BrukerDataReader
{
    public class DataReader
    {

        string m_fileName = "";
        int m_numMSScans = -1;

        long m_lengthOfDataFileInBytes = 0;

        GlobalParameters m_parameters = null;

        public DataReader(string fileName)
        {
            this.Parameters = new GlobalParameters();

            bool fileExists = File.Exists(fileName);

            if (File.Exists(fileName))
            {
                m_fileName = fileName;
            }
            else
            {
                throw new FileNotFoundException("Dataset could not be opened. File not found.");
            }


            using (BinaryReader reader = new BinaryReader(File.Open(m_fileName, FileMode.Open)))
            {
                m_lengthOfDataFileInBytes = reader.BaseStream.Length;
            }



        }

       
        #region Properties

        public GlobalParameters Parameters { get; set; }

        public string FileName
        {
            get { return m_fileName; }
        }


        #endregion

        #region Public Methods


        public int GetNumMSScans()
        {

            //determine if the numMSScans was already stored or not. If not, open file and figure it out.
            bool numScansNotYetDetermined = (m_numMSScans == -1);

            if (numScansNotYetDetermined)
            {
                Check.Require(this.Parameters != null && this.Parameters.NumPointsInScan > 0, "Cannot determine number of MS Scans. Parameter for number of points in Scan has not been set.");

                using (BinaryReader reader = new BinaryReader(File.Open(m_fileName, FileMode.Open)))
                {
                    long fileLength = reader.BaseStream.Length;
                    long totalNumberOfValues = fileLength / sizeof(Int32);

                    m_numMSScans = (int)(totalNumberOfValues / Parameters.NumPointsInScan);
                }
            }
            return m_numMSScans;
        }


        public long GetLengthOfDataFileInBytes()
        {
            return m_lengthOfDataFileInBytes;


        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanNum">Zero-based scan number</param>
        public void GetMassSpectrum(int scanNum, ref float[] mzValues, ref float[]intensities)
        {
            Check.Require(Parameters != null, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan num is greater than number of scans in dataset.");


            float[] vals = new float[Parameters.NumPointsInScan];
            using (BinaryReader reader = new BinaryReader(File.Open(m_fileName, FileMode.Open)))
            {
                reader.BaseStream.Position = scanNum * Parameters.NumPointsInScan;
                for (int i = 0; i < Parameters.NumPointsInScan; i++)
                {
                    vals[i] = reader.ReadInt32();
                }
            }

            int lengthOfMZAndIntensityArray = Parameters.NumPointsInScan / 2;
            mzValues = new float[lengthOfMZAndIntensityArray];
            intensities = new float[lengthOfMZAndIntensityArray];

            DeconEngine.Utils.FourierTransform(ref vals);

            for (int i = 0; i < lengthOfMZAndIntensityArray; i++)
            {
                float mz = (float)getMZ(i);
                double intensity = Math.Sqrt(vals[2 * i + 1] * vals[2 * i + 1] + vals[2 * i] * vals[2 * i]);

                int indexForReverseInsertion = (lengthOfMZAndIntensityArray - i - 1);
                mzValues[indexForReverseInsertion] = mz;
                intensities[indexForReverseInsertion] = i;
            }


            


            //convert to m/z

            //zerofill

            //apodize

            //



        }

        private double getMZ(int i)
        {
            double freq = i * Parameters.SampleRate / Parameters.NumPointsInScan;

            double mass = 0;
            if (freq + Parameters.CalB != 0)
            {
                mass = Parameters.CalA / (freq + Parameters.CalB);
            }
            else if (freq - Parameters.CalB <= 0)
            {
                mass = Parameters.CalA;
            }
            else
            {
                mass = 0;
            }
            return mass;
        }


        public void GetMassSpectrum(int scanNum, ref float xvals, ref float yvals)
        {

        }


        #endregion

        #region Private Methods

        #endregion


    }
}
