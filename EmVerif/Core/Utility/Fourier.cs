using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmVerif.Core.Utility
{
    public class FourierTransform
    {
        public static FourierTransform Instance = new FourierTransform();

        public void Fft(Complex[] compSrc, out Complex[] compDst, double id = 1.0)
        {
            // データ個数
            int n = compSrc.Length;

            compDst = new Complex[n];

            int i, i0, i1, j, k, arg;
            double th;

            compSrc.CopyTo(compDst, 0);

            Complex comp;
            Complex compTh;

            int ns = n / 2;
            double radTemp = 2.0 * Math.PI / (double)n;

            while (ns >= 1)
            {
                arg = 0;

                for (j = 0; j < n; j += 2 * ns)
                {
                    k = n / 4;

                    th = -id * radTemp * arg;
                    compTh = Complex.FromPolarCoordinates(1.0, th);

                    for (i0 = j; i0 < j + ns; i0++)
                    {
                        i1 = i0 + ns;

                        comp = compDst[i1] * compTh;

                        compDst[i1] = compDst[i0] - comp;
                        compDst[i0] = compDst[i0] + comp;
                    }

                    while (k <= arg)
                    {
                        arg -= k;
                        k /= 2;

                        if (k == 0) break;
                    }

                    arg += k;
                }
                ns /= 2;
            }

            // 逆変換のとき
            if (id < 0)
            {
                for (i = 0; i < n; i++)
                {
                    compDst[i] /= (double)n;
                }
            }

            j = 1;
            for (i = 1; i < n; i++)
            {
                if (i <= j)
                {
                    comp = compDst[i - 1];
                    compDst[i - 1] = compDst[j - 1];
                    compDst[j - 1] = comp;
                }
                k = n / 2;

                while (k < j)
                {
                    j -= k;
                    k /= 2;
                }
                j += k;
            }
        }

        public void Hamming(Complex[] data, Boolean normalize = false)
        {
            int dataCount = data.Length;

            var radTemp = 2.0 * Math.PI / (double)dataCount;

            if (normalize)
            {
                for (int i = 0; i < dataCount; i++)
                {
                    data[i] *= (1 - (0.46 / 0.54) * Math.Cos(radTemp * (double)i));
                }
            }
            else
            {
                for (int i = 0; i < dataCount; i++)
                {
                    data[i] *= (0.54 - 0.46 * Math.Cos(radTemp * (double)i));
                }
            }
        }

        public void Hanning(Complex[] data, Boolean normalize = false)
        {
            int dataCount = data.Length;

            var radTemp = 2.0 * Math.PI / (double)dataCount;

            if (normalize)
            {
                for (int i = 0; i < dataCount; i++)
                {
                    data[i] *= (1.0 * (1.0 - Math.Cos(radTemp * (double)i)));
                }
            }
            else
            {
                for (int i = 0; i < dataCount; i++)
                {
                    data[i] *= (0.5 * (1.0 - Math.Cos(radTemp * (double)i)));
                }
            }
        }

        public void Blackman(Complex[] data, Boolean normalize = false)
        {
            int dataCount = data.Length;

            var radTemp = 2.0 * Math.PI / (double)dataCount;

            if (normalize)
            {
                for (int i = 0; i < dataCount; i++)
                {
                    data[i] *= (1.0 - (0.5 / 0.42) * Math.Cos(radTemp * (double)i) + (0.08 / 0.42) * Math.Cos(2.0 * radTemp * (double)i));
                }
            }
            else
            {
                for (int i = 0; i < dataCount; i++)
                {
                    data[i] *= (0.42 - 0.5 * Math.Cos(radTemp * (double)i) + 0.08 * Math.Cos(2.0 * radTemp * (double)i));
                }
            }
        }
    }
}
