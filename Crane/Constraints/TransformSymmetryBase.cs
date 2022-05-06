using System;
using System.Collections.Generic;
using System.Linq;
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
            transLinear.TryGetInverse(out invLinear);
            invTranslation = -transTranslation;
        }


        public override double[] Error(CMesh cMesh)
        {
            List<double> error = new List<double>();
            for (int i = 0; i < indexPairs.Count; i++)
            {
                IndexPair pair = indexPairs[i];
                int id1 = pair.I;
                int id2 = pair.J;
                Point3d pt1 = cMesh.Vertices[id1];
                Point3d pt2 = cMesh.Vertices[id2];
                Point3d pt2Trans = inv * pt2;
                Point3d pt1Trans = trans * pt1;

                Vector3d dif1 = (pt1 - pt2Trans) / _edgeAverageLength;
                Vector3d dif2 = (pt2 - pt1Trans) / _edgeAverageLength;

                for (int j = 0; j < 3; j++) error.Add(dif1[j]);
                for (int j = 0; j < 3; j++) error.Add(dif2[j]);
            }

            if (hasFixedPints)
            {
                error.AddRange(fixedPointConstraint.Error(cMesh).ToArray());
            }

            return error.ToArray();
        }
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = 6 * indexPairs.Count;
            int columns = cMesh.Mesh.Vertices.Count * 3;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            for (int i = 0; i < indexPairs.Count; i++)
            {
                IndexPair pair = indexPairs[i];
                int id1 = pair.I;
                int id2 = pair.J;

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
            }

            var builder = new SparseMatrixBuilder(rows, columns, elements);
            if (hasFixedPints) builder.Append(fixedPointConstraint.Jacobian(cMesh));
            return builder;
        }


    }
}
