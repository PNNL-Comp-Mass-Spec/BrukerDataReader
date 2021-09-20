using System;

namespace BrukerDataReader
{
    public class FourierTransform
    {
        public int RealFourierTransform(ref double[] data)
        {
            // int iSign = 1;
            var n = data.Length;

            int i;
            const double c1 = 0.5;
            double hir;

            n /= 2;
            var theta = 3.141592653589793 / n;

            //if (iSign == 1)
            //{
            const double c2 = -0.5f;
            PerformFourierTransform(n, ref data, 1);
            //}
            //else
            //{
            //    c2 = 0.5f;
            //    theta = -theta;
            //}

            var wTemp = Math.Sin(0.5 * theta);
            var wpr = -2.0 * wTemp * wTemp;
            var wpi = Math.Sin(theta);
            var wr = 1.0 + wpr;
            var wi = wpi;
            var n2p3 = 2 * n + 3;

            for (i = 2; i <= n / 2; i++)
            {
                int i1;
                int i2;
                int i3;
                var i4 = 1 + (i3 = n2p3 - (i2 = 1 + (i1 = i + i - 1)));
                hir = c1 * (data[i1 - 1] + data[i3 - 1]);
                var h1i = c1 * (data[i2 - 1] - data[i4 - 1]);
                var h2r = -c2 * (data[i2 - 1] + data[i4 - 1]);
                var h2i = c2 * (data[i1 - 1] - data[i3 - 1]);
                data[i1 - 1] = hir + wr * h2r - wi * h2i;
                data[i2 - 1] = h1i + wr * h2i + wi * h2r;
                data[i3 - 1] = hir - wr * h2r + wi * h2i;
                data[i4 - 1] = -h1i + wr * h2i + wi * h2r;
                wr = (wTemp = wr) * wpr - wi * wpi + wr;
                wi = wi * wpr + wTemp * wpi + wi;
            }

            //if (iSign == 1)
            //{
            data[0] = (hir = data[0]) + data[1];
            data[1] = hir - data[1];
            //		for(i=0;i<(n*2);i++) data[i] /= (n);  // GAA 50-30-00
            //}

            return 0;
        }

        private void PerformFourierTransform(int nn, ref double[] data, int iSign)
        {
            long m;
            long i;

            long n = nn << 1;
            long j = 1;

            for (i = 1; i < n; i += 2)
            {
                if (j > i)
                {
                    SwapValuesInArray(ref data, i - 1, j - 1);
                    SwapValuesInArray(ref data, i, j);
                }
                m = n >> 1;
                while (m >= 2 && j > m)
                {
                    j -= m;
                    m >>= 1;
                }
                j += m;
            }

            long mMax = 2;
            while (n > mMax)
            {
                var iStep = 2 * mMax;
                var theta = 6.28318530717959 / (iSign * mMax);
                var wTemp = Math.Sin(0.5 * theta);
                var wpr = -2.0 * wTemp * wTemp;
                var wpi = Math.Sin(theta);
                var wr = 1.0;
                var wi = 0.0;

                for (m = 1; m < mMax; m += 2)
                {
                    for (i = m; i <= n; i += iStep)
                    {
                        j = i + mMax;
                        var jm1 = j - 1;
                        var im1 = i - 1;
                        var tempR = wr * data[jm1] - wi * data[j];
                        var tempI = wr * data[j] + wi * data[jm1];
                        data[jm1] = data[im1] - tempR;
                        data[j] = data[i] - tempI;
                        data[im1] += tempR;
                        data[i] += tempI;
                    }
                    wr = (wTemp = wr) * wpr - wi * wpi + wr;
                    wi = wi * wpr + wTemp * wpi + wi;
                }
                mMax = iStep;
            }
        }

        private void SwapValuesInArray(ref double[] data, long i, long j)
        {
            var tempVal = data[j];
            data[j] = data[i];
            data[i] = tempVal;
        }
    }
}
