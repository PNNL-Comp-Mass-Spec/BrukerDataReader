
using System;
using System.Collections.Generic;
using System.IO;

namespace BrukerDataReader
{
    

    //TODO:  add apodization ability
    //TODO:  remove all dependence on DeconEngine  (FFT, apodization, etc).

    public class DataReader : IDisposable
    {

        readonly string _fileName = "";
        int _numMSScans = -1;
        int _lastScanOpened;
        BinaryReader _reader;
        Int64 _previousStartPosition;
        Int64 _bytesAdvanced;
        readonly Int64 _lengthOfDataFileInBytes;


        private readonly FourierTransform _fourierTransform = new FourierTransform();

        /// <summary>
        /// Constructor for the DataReader class
        /// </summary>
        /// <param name="fileName">Refers to the binary file containing the mass spectra data. For Bruker data, 
        /// this is a 'ser' or a 'fid' file</param>
        /// <param name="settingsFilePath">Path to the acqus or apexAcquisition.method file that should be used for reading parameters</param>
        public DataReader(string fileName, string settingsFilePath = "")
        {
            
            if (File.Exists(fileName))
            {
                _fileName = fileName;
            }
            else
            {
                throw new FileNotFoundException("Dataset could not be opened. File not found.");
            }

            using (var reader = new BinaryReader(File.Open(_fileName, FileMode.Open)))
            {
                _lengthOfDataFileInBytes = reader.BaseStream.Length;
            }

            if (string.IsNullOrEmpty(settingsFilePath))
            {
                Parameters = new GlobalParameters();
            } 
            else
            {
                LoadParameters(settingsFilePath);
            }

        }


        #region Properties

        public GlobalParameters Parameters { get; set; }

        public string FileName
        {
            get { return _fileName; }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Load the paramters from an acqus file or apexAcquisition.method file
        /// </summary>
        /// <param name="settingsFilePath"></param>
        public void LoadParameters(string settingsFilePath)
        {
            var fiSettingsFile = new FileInfo(settingsFilePath);

            if (!fiSettingsFile.Exists)
                throw new FileNotFoundException("Settings file not found");

            var filenameLower = fiSettingsFile.Name.ToLower();
            var reader = new BrukerSettingsFileReader();

            switch (filenameLower)
            {
                case "acqu":
                case "acqus":
                    Parameters = reader.LoadApexAcqusParameters(fiSettingsFile);
                    break;
                case "apexacquisition.method":
                    Parameters = reader.LoadApexAcqParameters(fiSettingsFile);
                    break;
                default:
                    throw new Exception("Unrecognized settings file (" + fiSettingsFile.Name + "); should be acqus or apexAcquisition.method");
            }

        }

       
        public void SetParameters(double calA, double calB, double sampleRate, int numValuesInScan)
        {  
            this.Parameters = new GlobalParameters { ML1 = calA, ML2 = calB, SampleRate = sampleRate, NumValuesInScan = numValuesInScan };
        }

        public void SetParameters(GlobalParameters gp)
        {
            this.Parameters = gp;
        }

        public int GetNumMSScans()
        {

            // If numMSScans was already stored, then return it
            if (_numMSScans != -1)
            {
                return _numMSScans;
            }

            // Determine the number of scans using the file length

            Check.Require(this.Parameters != null && this.Parameters.NumValuesInScan > 0, "Cannot determine number of MS Scans. Parameter for number of points in Scan has not been set.");

            using (var reader = new BinaryReader(File.Open(_fileName, FileMode.Open)))
            {
                Int64 fileLength = reader.BaseStream.Length;
                var totalNumberOfValues = fileLength / sizeof(Int32);

                if (Parameters != null) _numMSScans = (int)(totalNumberOfValues / Parameters.NumValuesInScan);
            }
            return _numMSScans;
        }

        /// <summary>
        /// Gets the mass spectrum.  Opens the BinaryReader and doesn't close it. Then finds the correct scan
        /// by using a relative position within the reader.  It turns out to be only ~3-4% faster. 
        /// </summary>
        /// <param name="scanNum">Zero-based scan number</param>
        /// <param name="mzValues">array of m/z values</param>
        /// <param name="intensities">Array of intensity values</param>
        public void GetMassSpectrumUsingSupposedlyFasterBinaryReader(int scanNum, out float[] mzValues, out float[] intensities)
        {            

            Check.Require(Parameters != null, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan num is greater than number of scans in dataset.");

            if (_reader == null)
            {
                _reader = new BinaryReader(File.Open(_fileName, FileMode.Open));
            }

            var vals = new double[Parameters.NumValuesInScan];
            int diffBetweenCurrentAndPreviousScan = scanNum - _lastScanOpened;

            Int64 byteOffset = (Int64)diffBetweenCurrentAndPreviousScan * (Int64)Parameters.NumValuesInScan * (Int64)sizeof(Int32) - _bytesAdvanced;

            if (byteOffset != 0)
            {
                _reader.BaseStream.Seek(byteOffset, SeekOrigin.Current);

            }

            _previousStartPosition = _reader.BaseStream.Position;
            for (int i = 0; i < Parameters.NumValuesInScan; i++)
            {
                vals[i] = _reader.ReadInt32();
            }
            _bytesAdvanced = _reader.BaseStream.Position - _previousStartPosition;

            int lengthOfMZAndIntensityArray = Parameters.NumValuesInScan / 2;
            var mzValuesFullRange = new float[lengthOfMZAndIntensityArray];
            var intensitiesFullRange = new float[lengthOfMZAndIntensityArray];

            _fourierTransform.RealFourierTransform(ref vals);

            for (int i = 0; i < lengthOfMZAndIntensityArray; i++)
            {
                var mz = (float)getMZ(i);
                var intensity = (float)(Math.Sqrt(vals[2 * i + 1] * vals[2 * i + 1] + vals[2 * i] * vals[2 * i]));

                int indexForReverseInsertion = (lengthOfMZAndIntensityArray - i - 1);
                mzValuesFullRange[indexForReverseInsertion] = mz;
                intensitiesFullRange[indexForReverseInsertion] = intensity;
            }

            // Trim off m/z values according to parameters
            var indexOfLowMZ = getIndexForMZ(Parameters.MinMZfilter, lengthOfMZAndIntensityArray);
            var indexOfHighMZ = getIndexForMZ(Parameters.MaxMZfilter, lengthOfMZAndIntensityArray);

            mzValues = new float[indexOfHighMZ - indexOfLowMZ];
            intensities = new float[indexOfHighMZ - indexOfLowMZ];

            for (int i = indexOfLowMZ; i < indexOfHighMZ; i++)
            {
                mzValues[i - indexOfLowMZ] = mzValuesFullRange[i];
                intensities[i - indexOfLowMZ] = intensitiesFullRange[i];
            }

            _lastScanOpened = scanNum;

        }

        /// <summary>
        /// Gets the mass spectrum. Main difference with 'GetMassSpectrumUsingSupposedlyFasterBinaryReader' is that a new BinaryReader is created
        /// everytime here. This is advantageous in terms of making sure the file is opened and closed properly.
        /// Unit tests show this to be about 3 to 10% slower. Presently (Nov 2010), since there isn't much speed gain, I favor this one.
        /// </summary>
        /// <param name="scanNum">Zero-based scan number</param>
        /// <param name="mzValues">m/z values are returned here</param>
        /// <param name="intensities">intensity values are returned here</param>
        public void GetMassSpectrum(int scanNum, out float[] mzValues, out float[] intensities)
        {
            var scanNums = new int[] { scanNum };

            GetMassSpectrum(scanNums, out mzValues, out intensities);         
        }


        public void GetMassSpectrum(int scanNum, float minMZ, float maxMZ, out float[] mzValues, out float[] intensities)
        {
            Check.Require(Parameters != null && Parameters.ML1 > -1, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(maxMZ >= minMZ, "Cannot get mass spectrum. MinMZ is greater than MaxMZ - that's impossible.");

            Parameters.MinMZfilter = minMZ;
            Parameters.MaxMZfilter = maxMZ;

            GetMassSpectrum(scanNum, out mzValues, out intensities);
        }


        /// <summary>
        /// Gets the summed mass spectrum.
        /// </summary>
        /// <param name="scanNumsToBeSummed"></param>
        /// <param name="mzValues"></param>
        /// <param name="intensities"></param>
        public void GetMassSpectrum(int[] scanNumsToBeSummed, out float[] mzValues, out float[] intensities)
        {
            Check.Require(Parameters != null && Parameters.ML1 != -1, "Cannot get mass spectrum. Need to first set Parameters.");

            validateScanNums(scanNumsToBeSummed);
            //Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan num is greater than number of scans in dataset.");
      
            var scanDataList = new List<double[]>();

            using (var reader = new BinaryReader(File.Open(_fileName, FileMode.Open)))
            {

                foreach (var scanNum in scanNumsToBeSummed)
                {
                    var vals = new double[Parameters.NumValuesInScan];

                    Int64 bytePosition = (Int64)scanNum * (Int64)Parameters.NumValuesInScan * (Int64)sizeof(Int32);

                    reader.BaseStream.Seek(bytePosition, SeekOrigin.Begin);
                    for (int i = 0; i < Parameters.NumValuesInScan; i++)
                    {
                        vals[i] = reader.ReadInt32();
                    }

                    scanDataList.Add(vals);

                }

                reader.Close();

            }


            int lengthOfMZAndIntensityArray = Parameters.NumValuesInScan / 2;
            var mzValuesFullRange = new float[lengthOfMZAndIntensityArray];
            var intensitiesFullRange = new float[lengthOfMZAndIntensityArray];


            for (int i = 0; i < scanDataList.Count; i++)
            {
                double[] vals = scanDataList[i];
                 _fourierTransform.RealFourierTransform(ref vals);

                for (int j = 0; j < lengthOfMZAndIntensityArray; j++)
                {

                    int indexForReverseInsertion = (lengthOfMZAndIntensityArray - j - 1);

                    bool firstTimeThrough = (i == 0);
                    if (firstTimeThrough)
                    {
                        var mz = (float)getMZ(j);
                        mzValuesFullRange[indexForReverseInsertion] = mz;
                    }

                    var intensity = (float)(Math.Sqrt(vals[2 * j + 1] * vals[2 * j + 1] + vals[2 * j] * vals[2 * j]));
                    intensitiesFullRange[indexForReverseInsertion] += intensity;    //sum the intensities
                }

            }

            // Trim off m/z values according to parameters
            var indexOfLowMZ = getIndexForMZ(Parameters.MinMZfilter, lengthOfMZAndIntensityArray);
            var indexOfHighMZ = getIndexForMZ(Parameters.MaxMZfilter, lengthOfMZAndIntensityArray);

            mzValues = new float[indexOfHighMZ - indexOfLowMZ];
            intensities = new float[indexOfHighMZ - indexOfLowMZ];

            for (int i = indexOfLowMZ; i < indexOfHighMZ; i++)
            {
                mzValues[i - indexOfLowMZ] = mzValuesFullRange[i];
                intensities[i - indexOfLowMZ] = intensitiesFullRange[i];
            }

        }


        public void GetMassSpectrum(int[] scansNumsToBeSummed, float minMZ, float maxMZ, out float[] mzValues, out float[] intensities)
        {
            Check.Require(maxMZ >= minMZ, "Cannot get mass spectrum. MinMZ is greater than MaxMZ - that's impossible.");

            Parameters.MinMZfilter = minMZ;
            Parameters.MaxMZfilter = maxMZ;

            GetMassSpectrum(scansNumsToBeSummed, out mzValues, out intensities);

        }


        private void validateScanNums(IEnumerable<int> scanNumsToBeSummed)
        {
            foreach (var scanNum in scanNumsToBeSummed)
            {
                Check.Require(scanNum < GetNumMSScans(),"Cannot get mass spectrum. Requested scan num (" + scanNum + ") is greater than number of scans in dataset. Note that the first scan is scan 0");                
            }
        }


        #endregion

        #region Private Methods

        private int getIndexForMZ(float targetMZ, int arrayLength)
        {
            var index = (int)((Parameters.NumValuesInScan / Parameters.SampleRate) * (Parameters.ML1 / targetMZ - Parameters.ML2));
            index = arrayLength - index;

            if (index < 0)
            {
                return 0;
            }
            
            if (index > arrayLength - 1)
            {
                return arrayLength - 1;
            }
            
            return index;
        }

        private double getMZ(int i)
        {
            double freq = i * Parameters.SampleRate / Parameters.NumValuesInScan;

            double mass;
            if (Math.Abs(freq + Parameters.ML2) > 0)
            {
                mass = Parameters.ML1 / (freq + Parameters.ML2);
            }
            else if (freq - Parameters.ML2 <= 0)
            {
                mass = Parameters.ML1;
            }
            else
            {
                mass = 0;
            }
            return mass;
        }

        internal Int64 GetLengthOfDataFileInBytes()
        {
            return _lengthOfDataFileInBytes;
        }


        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            try
            {

                if (_reader != null)
                {
                    using (BinaryReader br = _reader)
                    {
                        br.Close();
                    }
                }

            }
            catch (Exception)
            {

                Console.WriteLine("BrukerDataReader had problems closing the binary reader. Note this.");
            }

        }

        #endregion
    }
}
