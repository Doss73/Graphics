using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphs
{
    public class Lagrange
    {
        private double l(int index, double[] X, double x)
        {
            double l = 1;
            for (int i = 0; i < X.Length; i++)
            {
                if (i != index)
                {
                    l *= (x - X[i]) / (X[index] - X[i]);
                }
            }
            return l;
        }

        public double GetValue(double[] X, double[] Y, double x)
        {
            double y = 0;
            for (int i = 0; i < X.Length; i++)
            {
                y += Y[i] * l(i, X, x);
            }

            return y;
        }
    }
}
