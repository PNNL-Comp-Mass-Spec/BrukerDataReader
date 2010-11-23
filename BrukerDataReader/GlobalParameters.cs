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
        public double CalA { get; set; }

        /// <summary>
        /// Calibration value B
        /// </summary>
        public double CalB { get; set; }

        /// <summary>
        /// The number of individual values in a scan. For FTMS, typically this is a power of 2.
        /// e.g.  if there are 8 values, this translates to 4 XY datapoints
        /// </summary>
        public int NumValuesInScan { get; set; }

        /// <summary>
        /// TODO: Define this.   SampleRate is key for figuring out m/z values
        /// </summary>
        public double SampleRate { get; set; }

        /// <summary>
        /// Minimum m/z value reported. Datareader will trim the low m/z values accordingly.
        /// </summary>
        public float MinMZ { get; set; }

        /// <summary>
        /// Maximum m/z value reported. Datareader will trim the high m/z values accordingly.
        /// </summary>
        public float MaxMZ { get; set; }

        public GlobalParameters()
        {
            setDefaults();
        }

        public GlobalParameters(double calA, double calB, double sampleRate, int numValuesInScan)
            : this()
        {
            this.CalA = calA;
            this.CalB = calB;
            this.SampleRate = sampleRate;
            this.NumValuesInScan = numValuesInScan;
        }

        private void setDefaults()
        {
            CalA = -1;
            CalB = -1;
            NumValuesInScan = 0;
            SampleRate = -1;

            MinMZ = 300;
            MaxMZ = 2000;
        }

    }
}
