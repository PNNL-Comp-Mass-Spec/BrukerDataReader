using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrukerDataReader.UnitTests
{
    public class TestUtilities
    {


        public static void DisplayXYValues(float[] xvalues, float[] yvalues)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < xvalues.Length; i++)
            {
                sb.Append(xvalues[i]);
                sb.Append('\t');
                sb.Append(yvalues[i]);
                sb.Append(Environment.NewLine);
            }
            Console.Write(sb.ToString());
        }



    }
}
