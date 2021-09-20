using System;
using System.Text;

namespace BrukerDataReader.UnitTests
{
    public static class TestUtilities
    {
        public static void DisplayXYValues(
            float[] xValues,
            float[] yValues,
            int numPointsToShow = 0,
            float mzStart = float.MinValue,
            float mzEnd = float.MaxValue)
        {
            var sb = new StringBuilder();

            var actualNumPointsToShow = xValues.Length;
            if (numPointsToShow > 0)
                actualNumPointsToShow = numPointsToShow;

            for (var i = 0; i < actualNumPointsToShow; i++)
            {
                if (xValues[i] >= mzStart && xValues[i] <= mzEnd)
                {
                    sb.Append(xValues[i]);
                    sb.Append('\t');
                    sb.Append(yValues[i]);
                    sb.Append(Environment.NewLine);
                }
            }
            Console.Write(sb.ToString());
        }
    }
}
