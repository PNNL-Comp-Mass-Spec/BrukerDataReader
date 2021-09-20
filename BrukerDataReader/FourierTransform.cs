using System;

namespace BrukerDataReader
{
    public class FourierTransform
    {
        public int RealFourierTransform(ref double[] data)
        {
            // int iSign = 1;
            var n = data.Length;

            int i, i1, i2, i3, i4, n2p3;
            double c1 = 0.5, c2, hir, h1i, h2r, h2i;
            double wpr, wpi, wi, wr, theta, wtemp;
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

            n = n / 2;
            theta = 3.141592653589793 / (double)n;
            if (isign == 1)
            {
                c2 = -0.5f;
                PerformFourierTransform(n, ref data, 1);
            }
            else
            {
                c2 = 0.5f;
                theta = -theta;
            }
            wtemp = Math.Sin(0.5 * theta);
            wpr = -2.0 * wtemp * wtemp;
            wpi = Math.Sin(theta);
            wr = 1.0 + wpr;
            wi = wpi;
            n2p3 = 2 * n + 3;
            for (i = 2; i <= n / 2; i++)
            {
                i4 = 1 + (i3 = n2p3 - (i2 = 1 + (i1 = i + i - 1)));
                hir = c1 * (data[i1 - 1] + data[i3 - 1]);
                h1i = c1 * (data[i2 - 1] - data[i4 - 1]);
                h2r = -c2 * (data[i2 - 1] + data[i4 - 1]);
                h2i = c2 * (data[i1 - 1] - data[i3 - 1]);
                data[i1 - 1] = (hir + wr * h2r - wi * h2i);
                data[i2 - 1] = (h1i + wr * h2i + wi * h2r);
                data[i3 - 1] = (hir - wr * h2r + wi * h2i);
                data[i4 - 1] = (-h1i + wr * h2i + wi * h2r);
                wr = (wtemp = wr) * wpr - wi * wpi + wr;
                wi = wi * wpr + wtemp * wpi + wi;
            }
            if (isign == 1)
            {
                data[0] = (hir = data[0]) + data[1];
                data[1] = hir - data[1];
                //		for(i=0;i<(n*2);i++) data[i] /= (n);  // GAA 50-30-00
            }
            else
            {
            }

            //if (iSign == 1)
            //{
            data[0] = (hir = data[0]) + data[1];
            data[1] = hir - data[1];
            //		for(i=0;i<(n*2);i++) data[i] /= (n);  // GAA 50-30-00
            //}

            return 0;
        }

        private void PerformFourierTransform(int nn, ref double[] data, int isign)
        {
            long m;
            long i;
            double wr, wpr, wpi, wi, theta;

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

            long mmax = 2;
            while (n > mmax)
            {
                var istep = 2 * mmax;
                theta = 6.28318530717959 / (isign * mmax);
                var wtemp = Math.Sin(0.5 * theta);
                wpr = -2.0 * wtemp * wtemp;
                wpi = Math.Sin(theta);
                wr = 1.0;
                wi = 0.0;
                for (m = 1; m < mmax; m += 2)
                {
                    for (i = m; i <= n; i += istep)
                    {
                        j = i + mmax;
                        var jm1 = j - 1;
                        var im1 = i - 1;
                        var tempr = (wr * data[jm1] - wi * data[j]);
                        var tempi = (wr * data[j] + wi * data[jm1]);
                        data[jm1] = (data[im1] - tempr);
                        data[j] = (data[i] - tempi);
                        data[im1] += tempr;
                        data[i] += tempi;
                    }
                    wr = (wtemp = wr) * wpr - wi * wpi + wr;
                    wi = wi * wpr + wtemp * wpi + wi;
                }
                mmax = istep;
            }
        }

        private void SwapValuesInArray(ref double[] data, long i, long j)
        {
            var tempVal = data[j];
            data[j] = data[i];
            data[i] = tempVal;
        }

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
