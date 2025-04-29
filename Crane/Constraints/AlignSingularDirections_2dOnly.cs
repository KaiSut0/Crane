using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace Crane.Constraints
{
    public class AlignSingularDirections_2dOnly: Constraint
    {
        public AlignSingularDirections_2dOnly() { }
        private CMesh cMeshOriginal;
        private double strength;

        public override double[] Error(CMesh cMesh)
        {

            int n = cMesh.FacePairs.Count;
            double[] error = new double[n];

            for (int i = 0; i < n; i++)
            {
                var pair = cMesh.FacePairs[i];
                int fI = pair.I;
                int fJ = pair.J;

                var pairOrig = cMeshOriginal.FacePairs[i];
                int FI = pairOrig.I;
                int FJ = pairOrig.J;

                var fI_ = cMesh.Mesh.Faces[fI];
                var fJ_ = cMesh.Mesh.Faces[fJ];

                var FI_ = cMeshOriginal.Mesh.Faces[FI];
                var FJ_ = cMeshOriginal.Mesh.Faces[FJ];


                Point3d x1I, x1J, x2I, x2J, x3I, x3J;

                x1I = cMesh.Mesh.Vertices[fI_[0]];
                x2I = cMesh.Mesh.Vertices[fI_[1]];
                x3I = cMesh.Mesh.Vertices[fI_[2]];

                x1J = cMesh.Mesh.Vertices[fJ_[0]];
                x2J = cMesh.Mesh.Vertices[fJ_[1]];
                x3J = cMesh.Mesh.Vertices[fJ_[2]];

                Point3d X1I, X1J, X2I, X2J, X3I, X3J;

                X1I = cMesh.Mesh.Vertices[FI_[0]];
                X2I = cMesh.Mesh.Vertices[FI_[1]];
                X3I = cMesh.Mesh.Vertices[FI_[2]];

                X1J = cMesh.Mesh.Vertices[FJ_[0]];
                X2J = cMesh.Mesh.Vertices[FJ_[1]];
                X3J = cMesh.Mesh.Vertices[FJ_[2]];


                Vector3d X12I = X2I - X1I;
                Vector3d X13I = X3I - X1I;
                Vector2d X12I_ = new Vector2d(X12I.Length, 0);
                double AngI = Vector3d.VectorAngle(X12I, X13I);
                Vector2d X13I_ = new Vector2d(X13I.Length * Math.Cos(AngI), X13I.Length * Math.Sin(AngI));

                Vector3d X12J = X2J - X1J;
                Vector3d X13J = X3J - X1J;
                Vector2d X12J_ = new Vector2d(X12J.Length, 0);
                double AngJ = Vector3d.VectorAngle(X12J, X13J);
                Vector2d X13J_ = new Vector2d(X13J.Length * Math.Cos(AngJ), X13J.Length * Math.Sin(AngJ));

                Vector3d x12I = x2I - x1I;
                Vector3d x13I = x3I - x1I;
                Vector2d x12I_ = new Vector2d(x12I.X, x12I.Y);
                Vector2d x13I_ = new Vector2d(x13I.X, x13I.Y);

                Vector3d x12J = x2J - x1J;
                Vector3d x13J = x3J - x1J;
                Vector2d x12J_ = new Vector2d(x12J.X, x12J.Y);
                Vector2d x13J_ = new Vector2d(x13J.X, x13J.Y);


                Matrix<double> xI = Matrix<double>.Build.Dense(2, 2);
                xI[0, 0] = x12I_.X; xI[0, 1] = x13I_.X;
                xI[1, 0] = x12I_.Y; xI[1, 1] = x13I_.Y;

                Matrix<double> XI = Matrix<double>.Build.Dense(2, 2);
                XI[0, 0] = X12I_.X; XI[0, 1] = X13I_.X;
                XI[1, 0] = X12I_.Y; XI[1, 1] = X13I_.Y;

                Matrix<double> AI = Matrix<double>.Build.Dense(2, 2);
                AI = xI * XI.Inverse();

                double thetaI = Math.Atan2(AI[1, 0] - AI[0, 1], AI[0, 0] + AI[1, 1]);

                Matrix<double> xJ = Matrix<double>.Build.Dense(2, 2);
                xJ[0, 0] = x12J_.X; xJ[0, 1] = x13J_.X;
                xJ[1, 0] = x12J_.Y; xJ[1, 1] = x13J_.Y;

                Matrix<double> XJ = Matrix<double>.Build.Dense(2, 2);
                XJ[0, 0] = X12J_.X; XJ[0, 1] = X13J_.X;
                XJ[1, 0] = X12J_.Y; XJ[1, 1] = X13J_.Y;

                Matrix<double> AJ = Matrix<double>.Build.Dense(2, 2);
                AJ = xJ * XJ.Inverse();

                double thetaJ = Math.Atan2(AJ[1, 0] - AJ[0, 1], AJ[0, 0] + AJ[1, 1]);
                
                error[i] = strength * (thetaI - thetaJ);

            }
            return error;
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            throw new NotImplementedException();
        }
    }
}
