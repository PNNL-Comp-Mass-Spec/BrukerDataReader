using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrukerDataReader
{

    public class GlobalParameters
    {
        /// <summary>
        /// Calibration value A
        /// </summary>
        /// <remarks>Previously CalA</remarks>
        public double ML1 { get; set; }

        /// <summary>
        /// Calibration value B
        /// </summary>
        /// <remarks>Previously CalB</remarks>
        public double ML2 { get; set; }

        /// <summary>
        /// The number of individual values in a scan. For FTMS, typically this is a power of 2.
        /// e.g.  if there are 8 values, this translates to 4 XY datapoints
        /// </summary>
        /// <remarks>TD</remarks>
        public int NumValuesInScan { get; set; }

        /// <summary>
        /// Sampling rate; not sure of the units.  A key for figuring out m/z values
        /// </summary>
        /// <remarks>SW_h * 2</remarks>
        public double SampleRate { get; set; }

        /// <summary>
        /// Minimum m/z value for the acquired data
        /// </summary>
        public double AcquiredMZMinimum { get; set; }

        /// <summary>
        /// Maximum m/z value for the acquired data
        /// </summary>
        public double AcquiredMZMaximum { get; set; }

        /// <summary>
        /// Last minimum m/z value specified when calling GetMassSpectrum()
        /// </summary>
        public float MinMZfilter { get; set; }

        /// <summary>
        /// Last maximum m/z value specified when calling GetMassSpectrum()
        /// </summary>
        public float MaxMZfilter { get; set; }
    
        public GlobalParameters()
        {
            setDefaults();
        }

        public GlobalParameters(double ml1, double ml2, double sampleRate, int numValuesInScan)
            : this()
        {
            this.ML1 = ml1;
            this.ML2 = ml2;
            this.SampleRate = sampleRate;
            this.NumValuesInScan = numValuesInScan;
        }

        public void Display()
        {
            var sb = new StringBuilder();
            sb.Append("ML1 =    " + ML1.ToString("0.000") + Environment.NewLine);
            sb.Append("ML2 =    " + ML2.ToString("0.000") + Environment.NewLine);
            sb.Append("SW_h =   " + SampleRate.ToString("0.000") + Environment.NewLine);
            sb.Append("TD =     " + NumValuesInScan + Environment.NewLine);
            sb.Append("MZ_min = " + AcquiredMZMinimum.ToString("0.000") + Environment.NewLine);
            sb.Append("MZ_max = " + AcquiredMZMaximum.ToString("0.000") + Environment.NewLine);
        }

        private void setDefaults()
        {
            ML1 = -1;
            ML2 = -1;
            NumValuesInScan = 0;
            SampleRate = -1;

            MinMZfilter = 300;
            MaxMZfilter = 2000;
        }

    }
}
