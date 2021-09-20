using System;
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
        /// e.g.  if there are 8 values, this translates to 4 XY data points
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
        public float MinMZFilter { get; set; }

        /// <summary>
        /// Last maximum m/z value specified when calling GetMassSpectrum()
        /// </summary>
        public float MaxMZFilter { get; set; }

        public GlobalParameters()
        {
            setDefaults();
        }

        // ReSharper disable once UnusedMember.Global
        public GlobalParameters(double ml1, double ml2, double sampleRate, int numValuesInScan)
            : this()
        {
            ML1 = ml1;
            ML2 = ml2;
            SampleRate = sampleRate;
            NumValuesInScan = numValuesInScan;
        }

        // ReSharper disable once UnusedMember.Global
        public void Display()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("ML1 =    {0:F3}{1}", ML1, Environment.NewLine);
            sb.AppendFormat("ML2 =    {0:F3}{1}", ML2, Environment.NewLine);
            sb.AppendFormat("SW_h =   {0:F3}{1}", SampleRate, Environment.NewLine);
            sb.AppendFormat("TD =     {0}{1}", NumValuesInScan, Environment.NewLine);
            sb.AppendFormat("MZ_min = {0:F3}{1}", AcquiredMZMinimum, Environment.NewLine);
            sb.AppendFormat("MZ_max = {0:F3}{1}", AcquiredMZMaximum, Environment.NewLine);

            Console.WriteLine(sb.ToString());
        }

        private void setDefaults()
        {
            ML1 = -1;
            ML2 = -1;
            NumValuesInScan = 0;
            SampleRate = -1;

            MinMZFilter = 300;
            MaxMZFilter = 2000;
        }
    }
}
