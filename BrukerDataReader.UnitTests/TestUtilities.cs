using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrukerDataReader.UnitTests
{
    public class TestUtilities
    {


        public static void DisplayXYValues(float[] xvalues, float[] yvalues, int numPointsToShow = 0, float mzStart = float.MinValue, float mzEnd = float.MaxValue)
        {
            var sb = new StringBuilder();

            int actualNumPointsToShow = xvalues.Length;
            if (numPointsToShow > 0)
                actualNumPointsToShow = numPointsToShow;

            for (int i = 0; i < actualNumPointsToShow; i++)
            {
                if (xvalues[i] >= mzStart && xvalues[i] <= mzEnd)
                {
                    sb.Append(xvalues[i]);
                    sb.Append('\t');
                    sb.Append(yvalues[i]);
                    sb.Append(Environment.NewLine);
                }
            }
            Console.Write(sb.ToString());
        }



    }
}
