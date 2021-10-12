using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;
using Rhino.UI;

namespace Crane.Misc
{
    public class Approx2dSymmetry
    {
        public Approx2dSymmetry(List<Point3d> pts, List<int> firstIds, List<int> secondIds)
        {
            numPts = firstIds.Count;
            firstPts = new Point2d[numPts];
            secondPts = new Point2d[numPts];
            for (int i = 0; i < numPts; i++)
            {
                var fPt = pts[firstIds[i]];
                var sPt = pts[secondIds[i]];
                var firstPt = new Point2d(fPt);
                var secondPt = new Point2d(sPt);
                firstPts[i] = firstPt;
                secondPts[i] = secondPt;
            }
            approxSymmmetry(8, 1e-12);
        }

        public Transform Symmetry2d { get; private set; }
        private Point2d[] firstPts;
        private Point2d[] secondPts;
        private int numPts;

        private double[,] A;
        private double[] r;
        private double[] b;

        private void approxSymmmetry(int iterationMax, double tolerance)
        {
            A = new double[2, 2];
            r = new double[2];
            b = new double[2];
            var err = error();
            var jac = jacobian();
            var diff = Vector<double>.Build.Dense(8);
            var zero = Vector<double>.Build.Dense(8);
            double residual = err.L2Norm();
            int iteration = 0;
            var speed = new List<double>();
            while (residual > tolerance && iteration < iterationMax)
            {
                diff = RigidOrigami.CGNRSolveForRectangleMatrix(jac, -err, zero, 1e-23, 8, ref speed);
                update(diff);
                err = error();
                jac = jacobian();
                residual = err.L2Norm();
                iteration++;
            }

            var symmetry2d = new Transform(); 
            symmetry2d[0, 0] = A[0,0];
            symmetry2d[0, 1] = A[0,1];
            symmetry2d[1, 0] = A[1,0];
            symmetry2d[1, 1] = A[1,1];
            symmetry2d[0, 3] = (1 - A[0, 0]) * r[0] + (-A[0, 1]) * r[1] + b[0];
            symmetry2d[1, 3] = (-A[1, 0]) * r[0] + (1 - A[1, 1]) * r[1] + b[1];
            symmetry2d[2, 2] = symmetry2d[3, 3] = 1;
            Symmetry2d = symmetry2d;
        }

        private void update(Vector<double> diff)
        {
            A[0, 0] += diff[0];
            A[0, 1] += diff[1];
            A[1, 0] += diff[2];
            A[1, 1] += diff[3];
            r[0] += diff[4];
            r[1] += diff[5];
            b[0] += diff[6];
            b[1] += diff[7];
        }

        private Vector<double> error()
        {
            var err = new double[2*numPts];
            for (int i = 0; i < numPts; i++)
            {
                err[2*i] = A[0, 0] * (firstPts[i].X - r[0]) + A[0, 1] * (firstPts[i].Y - r[1]) + r[0] + b[0] -secondPts[i].X;
                err[2*i+1] = A[1, 0] * (firstPts[i].X - r[0]) + A[1, 1] * (firstPts[i].Y - r[1]) + r[1] + b[1] -secondPts[i].Y;
            }
            return Vector<double>.Build.DenseOfArray(err)/numPts;
        }

        private Matrix<double> jacobian()
        {
            var jac = new double[2*numPts, 8];
            for (int i = 0; i < numPts; i++)
            {
                jac[2 * i, 0] = firstPts[i].X - r[0];
                jac[2 * i, 1] = firstPts[i].Y - r[1];
                jac[2 * i + 1, 2] = firstPts[i].X - r[0];
                jac[2 * i + 1, 3] = firstPts[i].Y - r[1];
                jac[2 * i, 4] = -A[0, 0] + 1;
                jac[2 * i, 5] = -A[0, 1];
                jac[2 * i + 1, 4] = -A[1, 0];
                jac[2 * i + 1, 5] = -A[1, 1] + 1;
                jac[2 * i, 6] = 1;
                jac[2 * i + 1, 7] = 1;
            }

            return Matrix<double>.Build.DenseOfArray(jac) / numPts;
        }
    }
}
