using System;
using System.Collections.Generic;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public abstract class TransformSymmetryBase : Constraint
    {

        public List<IndexPair> indexPairs;
        protected double _edgeAverageLength = 1.0;
        private Transform trans;
        private Transform inv;
        private Transform transLinear;
        private Vector3d transTranslation;
        private Transform invLinear;
        private Vector3d invTranslation;
        private bool hasFixedPints;
        private Constraint fixedPointConstraint;
        private Matrix<double> I_3 = Matrix<double>.Build.DenseDiagonal(3, 1);

        protected void SetIndexPairs(List<IndexPair> indexPairs)
        {
            this.indexPairs = indexPairs;
            hasFixedPints = false;
        }
        protected void SetFixedPointConstraint(Constraint fixedPointConstraint)
        {
            this.fixedPointConstraint = fixedPointConstraint;
            hasFixedPints = true;
        }
        protected void SetTransform(Transform transform)
        {
            trans = transform.Clone();
            trans.TryGetInverse(out inv);
            trans.DecomposeAffine(out transLinear, out transTranslation);
            //inv.DecomposeAffine(out invLinear, out invTranslation);
            transLinear.TryGetInverse(out invLinear);
            invTranslation = -transTranslation;
        }


        public override Vector<double> Error(CMesh cMesh)
        {
            List<double> error = new List<double>();
            for (int i = 0; i < indexPairs.Count; i++)
            {
                IndexPair pair = indexPairs[i];
                int id1 = pair.I;
                int id2 = pair.J;
                Point3d pt1 = (Point3d)cMesh.Mesh.Vertices[id1];
                Point3d pt2 = (Point3d)cMesh.Mesh.Vertices[id2];
                Point3d pt2Trans = inv * pt2;
                Point3d pt1Trans = trans * pt1;

                Vector3d dif1 = (pt1 - pt2Trans) / _edgeAverageLength;
                Vector3d dif2 = (pt2 - pt1Trans) / _edgeAverageLength;

                for (int j = 0; j < 3; j++) error.Add(dif1[j]);
                for (int j = 0; j < 3; j++) error.Add(dif2[j]);
                // error.Add(0.5 * (dif1 * dif1 + dif2 * dif2));// / (_edgeAverageLength * _edgeAverageLength));
            }

            if (hasFixedPints)
            {
                error.AddRange(fixedPointConstraint.Error(cMesh).ToArray());
            }
            return Vector<double>.Build.DenseOfArray(error.ToArray());
        }
        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            int rows = 6 * indexPairs.Count;
            int columns = cMesh.Mesh.Vertices.Count * 3;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            

            for (int i = 0; i < indexPairs.Count; i++)
            {
                IndexPair pair = indexPairs[i];
                int id1 = pair.I;
                int id2 = pair.J;
                //Point3d pt1 = (Point3d)cMesh.Mesh.Vertices[id1];
                //Point3d pt2 = (Point3d)cMesh.Mesh.Vertices[id2];
                //Point3d pt2TransInv = new Point3d(pt2);
                //Point3d pt1Trans = new Point3d(pt1);
                //pt2TransInv.Transform(inv);
                //pt1Trans.Transform(trans);

                //Vector3d dif1 = pt1 - pt2TransInv;
                //Vector3d dif2 = pt2 - pt1Trans;

                //double denominator = _edgeAverageLength * _edgeAverageLength;

                //Vector3d dr1dx1 = dif1 / denominator;
                //Vector3d dif1Tmp = new Vector3d(dif1);
                //dif1Tmp.Transform(invLinear);
                //Vector3d dr1dx2 = -dif1Tmp / denominator;

                //Vector3d dif2Tmp = new Vector3d(dif2);
                //dif2Tmp.Transform(transLinear);
                //Vector3d dr2dx1 = -dif2Tmp / denominator;
                //Vector3d dr2dx2 = dif2 / denominator;

                //Vector3d drdx1 = dr1dx1 + dr2dx1;
                //Vector3d drdx2 = dr1dx2 + dr2dx2;

                // d/dx1(x1 - (Ax2 + b)) = I
                // d/dx2(x1 - (Ax2 + b)) = 

                for (int j = 0; j < 3; j++)
                {
                    elements.Add(new Tuple<int, int, double>(6 * i + j,     3 * id1 + j, 1.0 / _edgeAverageLength));
                    elements.Add(new Tuple<int, int, double>(6 * i + j + 3, 3 * id2 + j, 1.0 / _edgeAverageLength));

                    for(int k = 0; k < 3; k++)
                    {
                        elements.Add(new Tuple<int, int, double>(6 * i + j,     3 * id2 + k, -  inv[j, k] / _edgeAverageLength));
                        elements.Add(new Tuple<int, int, double>(6 * i + j + 3, 3 * id1 + k, - trans[j, k] / _edgeAverageLength));
                    }
                }


                //for (int j = 0; j < 3; j++)
                //{
                //    elements.Add(new Tuple<int, int, double>(i, 3 * id1 + j, drdx1[j]));
                //    elements.Add(new Tuple<int, int, double>(i, 3 * id2 + j, drdx2[j]));
                //}
            }
            var mat = Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);
            if (hasFixedPints) mat = mat.Stack(fixedPointConstraint.Jacobian(cMesh));
            return mat;
        }


    }
}
