using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel.Types.Transforms;
using Crane.Core;

namespace Crane.Constraints
{
    public class CylindricallyPeriodic : Constraint
    {
        public CylindricallyPeriodic(IndexPair VIdPair, int IsS1orS2)
        {
            this.VIdPair = VIdPair;
            this.IsS1orS2 = IsS1orS2;
        }
        #region Public Members
        public IndexPair VIdPair;
        public int IsS1orS2;
        public double RotationAngle;
        public double TranslateCoefficient;
        #endregion
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            if (IsS1orS2 == 0)
            {
                RotationAngle = cMesh.CylindricallyRotationAngles[0];
                TranslateCoefficient = cMesh.CylindricallyTranslationCoefficients[0];
            }
            else
            {
                RotationAngle = cMesh.CylindricallyRotationAngles[1];
                TranslateCoefficient = cMesh.CylindricallyTranslationCoefficients[1];
            }

            Transform R = Transform.Rotation(RotationAngle, cMesh.CylinderAxis, cMesh.CylinderOrigin);
            Transform K = new Transform(0);
            K.M01 = -cMesh.CylinderAxis.Z;
            K.M02 = cMesh.CylinderAxis.Y;
            K.M10 = cMesh.CylinderAxis.Z;
            K.M12 = -cMesh.CylinderAxis.X;
            K.M20 = -cMesh.CylinderAxis.Y;
            K.M21 = cMesh.CylinderAxis.X;
            Point3d xFrom = cMesh.Mesh.Vertices[VIdPair.I];
            Point3d xTo = cMesh.Mesh.Vertices[VIdPair.J];

            Transform Rtest = Util.Addition(Transform.Identity, Util.Addition(Util.Multiply(Math.Sin(RotationAngle), K), Util.Multiply(1.0 - Math.Cos(RotationAngle), K * K)));

            Transform dCdxFrom = R;
            //Transform dCdxTo = Utils.Multiply(-1, Transform.Identity);

            Point3d dCdtheta = Util.Addition(Util.Multiply(Math.Cos(RotationAngle), K), Util.Multiply(Math.Sin(RotationAngle), K*K)) * xFrom;
            Vector3d dCda = cMesh.CylinderAxis;

            int rows = 3;
            int columns = cMesh.DOF + 4;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            Tuple<int, int, double> dCdxFrom00 = Tuple.Create(0, 3 * VIdPair.I, dCdxFrom.M00);
            Tuple<int, int, double> dCdxFrom01 = Tuple.Create(0, 3 * VIdPair.I + 1, dCdxFrom.M01);
            Tuple<int, int, double> dCdxFrom02 = Tuple.Create(0, 3 * VIdPair.I + 2, dCdxFrom.M02);
            Tuple<int, int, double> dCdxFrom10 = Tuple.Create(1, 3 * VIdPair.I, dCdxFrom.M10);
            Tuple<int, int, double> dCdxFrom11 = Tuple.Create(1, 3 * VIdPair.I + 1, dCdxFrom.M11);
            Tuple<int, int, double> dCdxFrom12 = Tuple.Create(1, 3 * VIdPair.I + 2, dCdxFrom.M12);
            Tuple<int, int, double> dCdxFrom20 = Tuple.Create(2, 3 * VIdPair.I, dCdxFrom.M20);
            Tuple<int, int, double> dCdxFrom21 = Tuple.Create(2, 3 * VIdPair.I + 1, dCdxFrom.M21);
            Tuple<int, int, double> dCdxFrom22 = Tuple.Create(2, 3 * VIdPair.I + 2, dCdxFrom.M22);
            elements.Add(dCdxFrom00);
            elements.Add(dCdxFrom01);
            elements.Add(dCdxFrom02);
            elements.Add(dCdxFrom10);
            elements.Add(dCdxFrom11);
            elements.Add(dCdxFrom12);
            elements.Add(dCdxFrom20);
            elements.Add(dCdxFrom21);
            elements.Add(dCdxFrom22);

            Tuple<int, int, double> dCdxTo00 = Tuple.Create(0, 3 * VIdPair.J, -1.0);
            Tuple<int, int, double> dCdxTo11 = Tuple.Create(1, 3 * VIdPair.J + 1, -1.0);
            Tuple<int, int, double> dCdxTo22 = Tuple.Create(2, 3 * VIdPair.J + 2, -1.0);
            elements.Add(dCdxTo00);
            elements.Add(dCdxTo11);
            elements.Add(dCdxTo22);

            Tuple<int, int, double> dCdtheta0 = Tuple.Create(0, cMesh.DOF + IsS1orS2, dCdtheta.X);
            Tuple<int, int, double> dCdtheta1 = Tuple.Create(1, cMesh.DOF + IsS1orS2, dCdtheta.Y);
            Tuple<int, int, double> dCdtheta2 = Tuple.Create(2, cMesh.DOF + IsS1orS2, dCdtheta.Z);
            elements.Add(dCdtheta0);
            elements.Add(dCdtheta1);
            elements.Add(dCdtheta2);

            Tuple<int, int, double> dCda0 = Tuple.Create(0, cMesh.DOF + IsS1orS2 + 2, dCda.X);
            Tuple<int, int, double> dCda1 = Tuple.Create(1, cMesh.DOF + IsS1orS2 + 2, dCda.Y);
            Tuple<int, int, double> dCda2 = Tuple.Create(2, cMesh.DOF + IsS1orS2 + 2, dCda.Z);
            elements.Add(dCda0);
            elements.Add(dCda1);
            elements.Add(dCda2);

            return new SparseMatrixBuilder(rows, columns, elements);
        }
        public override double[] Error(CMesh cMesh)
        {
            if (IsS1orS2 == 0)
            {
                RotationAngle = cMesh.CylindricallyRotationAngles[0];
                TranslateCoefficient = cMesh.CylindricallyTranslationCoefficients[0];
            }
            else
            {
                RotationAngle = cMesh.CylindricallyRotationAngles[1];
                TranslateCoefficient = cMesh.CylindricallyTranslationCoefficients[1];
            }

            Transform R = Transform.Rotation(RotationAngle, cMesh.CylinderAxis, cMesh.CylinderOrigin);
            Point3d xFrom = cMesh.Mesh.Vertices[VIdPair.I];
            Point3d xTo = cMesh.Mesh.Vertices[VIdPair.J];
            Vector3d C = R * xFrom + TranslateCoefficient * cMesh.CylinderAxis - xTo;
            double[] error_ = new double[3];
            error_[0] = C[0];
            error_[1] = C[1];
            error_[2] = C[2];
            return error_;
        }
    }
}
