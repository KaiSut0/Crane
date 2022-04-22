using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class MirrorSymmetry : TransformSymmetryBase
    {
        public MirrorSymmetry(CMesh cMesh, Plane mirrorPlane, double tolerance)        
        {
            List<IndexPair> indexPairs;
            List<int> fixedIndices;
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray().ToList();
            Transform transform = Transform.Mirror(mirrorPlane);
            Util.CreateTransformIndexPairs(pts, transform, tolerance, out indexPairs, out fixedIndices);
            SetIndexPairs(indexPairs);
            SetTransform(transform);
            if (fixedIndices.Count != 0)
            {
                OnPlane constriant = new OnPlane(cMesh, mirrorPlane, fixedIndices.ToArray(), 1.0);
                SetFixedPointConstraint(constriant);
            }

        }

        //public override Vector<double> Error(CMesh cMesh)
        //{
        //    double[] error = new double[_n];
        //    for(int i = 0; i < _n; i++)
        //    {
        //        IndexPair pair = indexPairs[i];
        //        int id1 = pair.I;
        //        int id2 = pair.J;
        //        Point3d pt1 = (Point3d) cMesh.Mesh.Vertices[id1];
        //        Point3d pt2 = (Point3d) cMesh.Mesh.Vertices[id2];
        //        Point3d pt2Mir = new Point3d(pt2);
        //        Point3d pt1Mir = new Point3d(pt1);
        //        Transform mir = Transform.Mirror(_plane);
        //        pt2Mir.Transform(mir);
        //        pt1Mir.Transform(mir);

              
        //        Vector3d dif1 = pt1 - pt2Mir;
        //        Vector3d dif2 = pt2 - pt1Mir;

        //        error[i] = 0.5 * (dif1 * dif1 + dif2 * dif2) / (_edgeAverageLength * _edgeAverageLength);

        //    }
        //    return Vector<double>.Build.DenseOfArray(error);
        //}

        //public override Matrix<double> Jacobian(CMesh cMesh)
        //{
        //    int rows = _n;
        //    int columns = cMesh.Mesh.Vertices.Count * 3;
        //    List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

        //    for (int i = 0; i < _n; i++)
        //    {
        //        IndexPair pair = indexPairs[i];
        //        int id1 = pair.I;
        //        int id2 = pair.J;
        //        Point3d pt1 = (Point3d) cMesh.Mesh.Vertices[id1];
        //        Point3d pt2 = (Point3d) cMesh.Mesh.Vertices[id2];
        //        Point3d pt2Mir = new Point3d(pt2);
        //        Point3d pt1Mir = new Point3d(pt1);
        //        Transform mir = Transform.Mirror(_plane);
        //        pt2Mir.Transform(mir);
        //        pt1Mir.Transform(mir);

              
        //        Vector3d dif1 = pt1 - pt2Mir;
        //        Vector3d dif2 = pt2 - pt1Mir;

        //        Vector<double> dif1Vec = Vector<double>.Build.DenseOfArray(new double[] {dif1.X, dif1.Y, dif1.Z});
        //        Vector<double> dif2Vec = Vector<double>.Build.DenseOfArray(new double[] {dif2.X, dif2.Y, dif2.Z});

        //        Vector3d n = _plane.Normal;
        //        Vector<double> normal = Vector<double>.Build.DenseOfArray(new double[] {n.X, n.Y, n.Z});
        //        Matrix<double> nn = normal.OuterProduct(normal);
        //        Matrix<double> I = Matrix<double>.Build.DenseIdentity(3);

        //        Vector<double> drdx1 = (2 * nn - I) * dif2Vec + dif1Vec;
        //        Vector<double> drdx2 = (2 * nn - I) * dif1Vec + dif2Vec;

        //        drdx1 /= _edgeAverageLength * _edgeAverageLength;
        //        drdx2 /= _edgeAverageLength * _edgeAverageLength;

        //        for (int j = 0; j < 3; j++)
        //        {
        //            elements.Add(new Tuple<int, int, double>(i, 3 * id1 + j, drdx1[j]));
        //            elements.Add(new Tuple<int, int, double>(i, 3 * id2 + j, drdx2[j]));

        //        }

        //        //IndexPair pair = indexPairs[i];
        //        //int id1 = pair.I;
        //        //int id2 = pair.J;
        //        //Vector3d normal = _plane.Normal;

        //        //elements.Add(new Tuple<int, int, double>(3 * i, 3 * id1, 1));
        //        //elements.Add(new Tuple<int, int, double>(3 * i + 1, 3 * id1 + 1, 1));
        //        //elements.Add(new Tuple<int, int, double>(3 * i + 2, 3 * id1 + 2, 1));

        //        //Vector3d drdx2 = 2 * normal.X * normal - Vector3d.XAxis;
        //        //Vector3d drdy2 = 2 * normal.Y * normal - Vector3d.YAxis;
        //        //Vector3d drdz2 = 2 * normal.Z * normal - Vector3d.ZAxis;

        //        //for (int j = 0; j < 3; j++)
        //        //{
        //        //    elements.Add(new Tuple<int, int, double>(3 * i, 3 * id2 + j, drdx2[j]));
        //        //    elements.Add(new Tuple<int, int, double>(3 * i + 1, 3 * id2 + j, drdy2[j]));
        //        //    elements.Add(new Tuple<int, int, double>(3 * i + 2, 3 * id2 + j, drdz2[j]));
        //        //}

        //    }
        //    return Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);

    }

}
