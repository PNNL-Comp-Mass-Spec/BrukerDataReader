using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrukerDataReader
{
    public class GlobalParameters
    {

        public double CalA { get; set; }
        public double CalB { get; set; }
        public int NumPointsInScan { get; set; }
        public double SampleRate { get; set; }

        public GlobalParameters()
        {
            CalA = -1;
            CalB = -1;
            NumPointsInScan = 0;
            SampleRate = -1;
        }

    }
}
