﻿using System;
using System.Collections.Generic;
using System.IO;

namespace BrukerDataReader
{
    // TODO: add apodization ability
    // TODO: remove all dependence on DeconEngine (FFT, apodization, etc).

    public class DataReader : IDisposable
    {
        // Ignore Spelling: acqu, acqus, apodization, Bruker, fid, ser

        private int _numMSScans = -1;
        private int _lastScanOpened;
        private BinaryReader _reader;
        private long _previousStartPosition;
        private long _bytesAdvanced;

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
                FileName = fileName;
            }
            else
            {
                throw new FileNotFoundException("Dataset could not be opened. File not found.");
            }

            // Assure that the file can be opened
            using (new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
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

        public GlobalParameters Parameters { get; set; }

        public string FileName { get; }

        /// <summary>
        /// Load the parameters from an acqus file or apexAcquisition.method file
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
            Parameters = new GlobalParameters { ML1 = calA, ML2 = calB, SampleRate = sampleRate, NumValuesInScan = numValuesInScan };
        }

        public void SetParameters(GlobalParameters gp)
        {
            Parameters = gp;
        }

        public int GetNumMSScans()
        {
            // If numMSScans was already stored, then return it
            if (_numMSScans != -1)
            {
                return _numMSScans;
            }

            // Determine the number of scans using the file length

            Check.Require(Parameters?.NumValuesInScan > 0, "Cannot determine number of MS Scans. Parameter for number of points in Scan has not been set.");

            using (var reader = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var fileLength = reader.BaseStream.Length;
                var totalNumberOfValues = fileLength / sizeof(int);

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
        // ReSharper disable once UnusedMember.Global
        public void GetMassSpectrumUsingSupposedlyFasterBinaryReader(int scanNum, out float[] mzValues, out float[] intensities)
        {
            Check.Require(Parameters != null, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan number is greater than number of scans in dataset.");

            if (_reader == null)
            {
                _reader = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }

            if (Parameters == null)
                throw new Exception("Parameters is null in GetMassSpectrumUsingSupposedlyFasterBinaryReader");

            var vals = new double[Parameters.NumValuesInScan];
            var diffBetweenCurrentAndPreviousScan = scanNum - _lastScanOpened;

            var byteOffset = diffBetweenCurrentAndPreviousScan * (long)Parameters.NumValuesInScan * sizeof(int) - _bytesAdvanced;

            if (byteOffset != 0)
            {
                _reader.BaseStream.Seek(byteOffset, SeekOrigin.Current);
            }

            _previousStartPosition = _reader.BaseStream.Position;
            for (var i = 0; i < Parameters.NumValuesInScan; i++)
            {
                vals[i] = _reader.ReadInt32();
            }
            _bytesAdvanced = _reader.BaseStream.Position - _previousStartPosition;

            var lengthOfMZAndIntensityArray = Parameters.NumValuesInScan / 2;
            var mzValuesFullRange = new float[lengthOfMZAndIntensityArray];
            var intensitiesFullRange = new float[lengthOfMZAndIntensityArray];

            _fourierTransform.RealFourierTransform(ref vals);

            for (var i = 0; i < lengthOfMZAndIntensityArray; i++)
            {
                var mz = (float)GetMZ(i);
                var intensity = (float)Math.Sqrt(vals[2 * i + 1] * vals[2 * i + 1] + vals[2 * i] * vals[2 * i]);

                var indexForReverseInsertion = lengthOfMZAndIntensityArray - i - 1;
                mzValuesFullRange[indexForReverseInsertion] = mz;
                intensitiesFullRange[indexForReverseInsertion] = intensity;
            }

            // Trim off m/z values according to parameters
            var indexOfLowMZ = GetIndexForMZ(Parameters.MinMZFilter, lengthOfMZAndIntensityArray);
            var indexOfHighMZ = GetIndexForMZ(Parameters.MaxMZFilter, lengthOfMZAndIntensityArray);

            mzValues = new float[indexOfHighMZ - indexOfLowMZ];
            intensities = new float[indexOfHighMZ - indexOfLowMZ];

            for (var i = indexOfLowMZ; i < indexOfHighMZ; i++)
            {
                mzValues[i - indexOfLowMZ] = mzValuesFullRange[i];
                intensities[i - indexOfLowMZ] = intensitiesFullRange[i];
            }

            _lastScanOpened = scanNum;
        }

        /// <summary>
        /// Gets the mass spectrum. Main difference with 'GetMassSpectrumUsingSupposedlyFasterBinaryReader' is that a new BinaryReader is created
        /// every time here. This is advantageous in terms of making sure the file is opened and closed properly.
        /// Unit tests show this to be about 3 to 10% slower. Presently (Nov 2010), since there isn't much speed gain, I favor this one.
        /// </summary>
        /// <param name="scanNum">Zero-based scan number</param>
        /// <param name="mzValues">m/z values are returned here</param>
        /// <param name="intensities">intensity values are returned here</param>
        public void GetMassSpectrum(int scanNum, out float[] mzValues, out float[] intensities)
        {
            var scanNums = new[] { scanNum };

            GetMassSpectrum(scanNums, out mzValues, out intensities);
        }

        public void GetMassSpectrum(int scanNum, float minMZ, float maxMZ, out float[] mzValues, out float[] intensities)
        {
            Check.Require(Parameters?.ML1 > -1, "Cannot get mass spectrum. Need to first set Parameters.");
            Check.Require(maxMZ >= minMZ, "Cannot get mass spectrum. MinMZ is greater than MaxMZ - that's impossible.");

            if (Parameters == null)
                throw new Exception("Parameters is null in GetMassSpectrum");

            Parameters.MinMZFilter = minMZ;
            Parameters.MaxMZFilter = maxMZ;

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
            Check.Require(Parameters != null && Math.Abs(Parameters.ML1 - (-1)) > float.Epsilon, "Cannot get mass spectrum. Need to first set Parameters.");
            if (Parameters == null)
                throw new Exception("Parameters is null in GetMassSpectrum");

            ValidateScanNums(scanNumsToBeSummed);
            //Check.Require(scanNum < GetNumMSScans(), "Cannot get mass spectrum. Requested scan number is greater than number of scans in dataset.");

            var scanDataList = new List<double[]>();

            using (var reader = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                foreach (var scanNum in scanNumsToBeSummed)
                {
                    var vals = new double[Parameters.NumValuesInScan];

                    var bytePosition = scanNum * (long)Parameters.NumValuesInScan * sizeof(int);

                    reader.BaseStream.Seek(bytePosition, SeekOrigin.Begin);
                    for (var i = 0; i < Parameters.NumValuesInScan; i++)
                    {
                        vals[i] = reader.ReadInt32();
                    }

                    scanDataList.Add(vals);
                }

                reader.Close();
            }

            var lengthOfMZAndIntensityArray = Parameters.NumValuesInScan / 2;
            var mzValuesFullRange = new float[lengthOfMZAndIntensityArray];
            var intensitiesFullRange = new float[lengthOfMZAndIntensityArray];

            for (var i = 0; i < scanDataList.Count; i++)
            {
                var vals = scanDataList[i];
                 _fourierTransform.RealFourierTransform(ref vals);

                for (var j = 0; j < lengthOfMZAndIntensityArray; j++)
                {
                    var indexForReverseInsertion = lengthOfMZAndIntensityArray - j - 1;

                    var firstTimeThrough = i == 0;
                    if (firstTimeThrough)
                    {
                        var mz = (float)GetMZ(j);
                        mzValuesFullRange[indexForReverseInsertion] = mz;
                    }

                    var intensity = (float)Math.Sqrt(vals[2 * j + 1] * vals[2 * j + 1] + vals[2 * j] * vals[2 * j]);
                    intensitiesFullRange[indexForReverseInsertion] += intensity;    //sum the intensities
                }
            }

            // Trim off m/z values according to parameters
            var indexOfLowMZ = GetIndexForMZ(Parameters.MinMZFilter, lengthOfMZAndIntensityArray);
            var indexOfHighMZ = GetIndexForMZ(Parameters.MaxMZFilter, lengthOfMZAndIntensityArray);

            mzValues = new float[indexOfHighMZ - indexOfLowMZ];
            intensities = new float[indexOfHighMZ - indexOfLowMZ];

            for (var i = indexOfLowMZ; i < indexOfHighMZ; i++)
            {
                mzValues[i - indexOfLowMZ] = mzValuesFullRange[i];
                intensities[i - indexOfLowMZ] = intensitiesFullRange[i];
            }
        }

        // ReSharper disable once UnusedMember.Global
        public void GetMassSpectrum(int[] scansNumsToBeSummed, float minMZ, float maxMZ, out float[] mzValues, out float[] intensities)
        {
            Check.Require(maxMZ >= minMZ, "Cannot get mass spectrum. MinMZ is greater than MaxMZ - that's impossible.");

            Parameters.MinMZFilter = minMZ;
            Parameters.MaxMZFilter = maxMZ;

            GetMassSpectrum(scansNumsToBeSummed, out mzValues, out intensities);
        }

        private void ValidateScanNums(IEnumerable<int> scanNumsToBeSummed)
        {
            foreach (var scanNum in scanNumsToBeSummed)
            {
                Check.Require(scanNum < GetNumMSScans(),"Cannot get mass spectrum. Requested scan number (" + scanNum + ") is greater than number of scans in dataset. Note that the first scan is scan 0");
            }
        }

        private int GetIndexForMZ(float targetMZ, int arrayLength)
        {
            var index = (int)(Parameters.NumValuesInScan / Parameters.SampleRate * (Parameters.ML1 / targetMZ - Parameters.ML2));
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

        private double GetMZ(int i)
        {
            var freq = i * Parameters.SampleRate / Parameters.NumValuesInScan;

            if (Math.Abs(freq + Parameters.ML2) > 0)
            {
                return Parameters.ML1 / (freq + Parameters.ML2);
            }

            if (freq - Parameters.ML2 <= 0)
            {
                return Parameters.ML1;
            }

            return 0;
        }

        public void Dispose()
        {
            try
            {
                if (_reader != null)
                {
                    using (var br = _reader)
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
    }
}
