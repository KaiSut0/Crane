using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crane.Misc
{
    public class MapMiuraParameter
    {
        public MapMiuraParameter(double xA, double yA, double xB, double yB, double t)
        {
            a = new List<double>();
            b = new List<double>();
            rhoA = new List<double>();
            rhoB = new List<double>();
            ComputeMapMiuraParameters(xA, yA, xB, yB, t);
        }

        public List<double> a { get; private set; }
        public List<double> b { get; private set; }
        public List<double> rhoA { get; private set; }
        public List<double> rhoB { get; private set; }

        public int NumberOfSolutions { get; private set; }

        private void ComputeMapMiuraParameters(double xA, double yA, double xB, double yB, double t)
        {
            double xyA = yA / xA;
            double xAB = xB / xA;
            double yAB = yB / yA;

            // Compute ab.
            var ab_sqrt_inner = (-1 + Math.Pow(xAB, 2)) * (-1 + Math.Pow(yAB, 2));
            if (ab_sqrt_inner < 0)
            {
                NumberOfSolutions = 0;
                return;
            }

            var ab_p = 
                ((xyA * (-1 + Math.Pow(xAB, 2) * Math.Pow(yAB, 2)) * (-1 + Math.Pow(Math.Tan(t / 2), 2))) / 
                (xAB * Math.Sqrt(ab_sqrt_inner) * (1 + Math.Pow(Math.Tan(t / 2), 2))));

            var ab_m = -ab_p;

            var ab = (ab_p > 0) ? ab_p : ab_m;

            // Compute rA.
            var rAB_sqrt_inner = ((-1 + Math.Pow(xAB, 2)) * (-1 + Math.Pow(xAB, 2) * Math.Pow(yAB, 2)));
            if (rAB_sqrt_inner < 0)
            {
                NumberOfSolutions = 0;
                return;
            }

            var rA_denominator = (-1 + Math.Pow(xAB, 2) + (2 + Math.Pow(xAB, 2) * (2 - 4 * Math.Pow(yAB, 2))) 
                                       * Math.Pow(Math.Tan(t / 2), 2) + (-1 + Math.Pow(xAB, 2)) * Math.Pow(Math.Tan(t / 2), 4));
            if (rA_denominator == 0)
            {
                NumberOfSolutions = 0;
                return;
            }

            var rA_term1 = 
                +1 
                - Math.Pow(xAB, 2) 
                + 6 * Math.Pow(Math.Tan(t / 2), 2) 
                - 2 * Math.Pow(xAB, 2) * Math.Pow(Math.Tan(t / 2), 2) 
                - 4 * Math.Pow(xAB, 2) * Math.Pow(yAB, 2) * Math.Pow(Math.Tan(t / 2), 2) 
                + Math.Pow(Math.Tan(t / 2), 4) 
                - Math.Pow(xAB, 2) * Math.Pow(Math.Tan(t / 2), 4);

            var rA_term2 = 4 * Math.Sqrt(rAB_sqrt_inner) * (Math.Tan(t / 2) + Math.Pow(Math.Tan(t / 2), 3));

            var rA_sqrt_inner_p = (rA_term1 + rA_term2) / rA_denominator;
            var rA_sqrt_inner_m = (rA_term1 - rA_term2) / rA_denominator;

            double rA1 = 0;
            double rA2 = 0;
            double rA_sqrt_inner = 0;

            if (rA_sqrt_inner_p < 0 && rA_sqrt_inner_m < 0)
            {
                NumberOfSolutions = 0;
                return;
            }

            if (rA_sqrt_inner_p > 0 && rA_sqrt_inner_m > 0)
            {
                NumberOfSolutions = 2;
                rA1 = 4 * Math.Atan(Math.Sqrt(rA_sqrt_inner_p));
                rA2 = 4 * Math.Atan(Math.Sqrt(rA_sqrt_inner_m));
            }

            else
            {
                NumberOfSolutions = 1;
                rA_sqrt_inner = (rA_sqrt_inner_p > 0) ? rA_sqrt_inner_p : rA_sqrt_inner_m;
                rA1 = Math.Atan(4 * Math.Sqrt(rA_sqrt_inner));
            }

            // Compute rB.
            var rB_denominator = (4 * Math.Pow(Math.Tan(t / 2), 2) +
                Math.Pow(yAB, 2) * (-1 - xAB + (-1 + xAB) * Math.Pow(Math.Tan(t / 2), 2)) * (1 
                - xAB + (1 + xAB) * Math.Pow(Math.Tan(t / 2), 2)));
            if (rB_denominator == 0)
            {
                NumberOfSolutions = 0;
                return;
            }


            var rB_sqrt_inner = (Math.Pow(yAB, 2) -
                                  Math.Pow(xAB, 2) * Math.Pow(yAB, 2) + 4 * Math.Pow(Math.Tan(t / 2), 2) +
                                  2 * Math.Pow(yAB, 2) * Math.Pow(Math.Tan(t / 2), 2) -
                                  6 * Math.Pow(xAB, 2) * Math.Pow(yAB, 2) * Math.Pow(Math.Tan(t / 2), 2) +
                                  Math.Pow(yAB, 2) * Math.Pow(Math.Tan(t / 2), 4) -
                                  Math.Pow(xAB, 2) * Math.Pow(yAB, 2) * Math.Pow(Math.Tan(t / 2), 4) -
                                  4 * yAB * Math.Sqrt(rAB_sqrt_inner) * (Math.Tan(t / 2) 
                                  + Math.Pow(Math.Tan(t / 2), 3))) / rB_denominator;

            if (rB_sqrt_inner < 0)
            {
                NumberOfSolutions = 0;
                return;
            }

            var rB = 4 * Math.Atan(Math.Sqrt(rB_sqrt_inner));

            if (rB > Math.PI)
            {
                rB = 2 * Math.PI - rB;
            }

            if (NumberOfSolutions >= 1)
            {
                var cosXiA = Math.Sqrt(Math.Pow(Math.Sin(t) * Math.Sin(rA1 / 2), 2) + Math.Pow(Math.Cos(t), 2));
                var cosZetaA = Math.Cos(t) / cosXiA;
                var a1 = xA / (2 * cosZetaA);
                var b1 = a1 * ab;
                var rhoA1 = rA1;
                if (rA1 > Math.PI)
                {
                    rhoA1 = 2 * Math.PI - rA1;
                }
                var rhoB1 = rB;
                a.Add(a1);
                b.Add(b1);
                rhoA.Add(rhoA1);
                rhoB.Add(rhoB1);
            }
        }


    }
}
