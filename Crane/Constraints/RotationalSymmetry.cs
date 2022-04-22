using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class RotationalSymmetry : TransformSymmetryBase
    {
        public RotationalSymmetry(CMesh cMesh, Line rotationAxis, double rotationAngle, double tolerance)
        {
            List<IndexPair> indexPairs;
            List<int> fixedIndices;
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray().ToList();
            Transform rotation = Transform.Rotation(rotationAngle, rotationAxis.Direction, rotationAxis.From);
            Util.CreateTransformIndexPairs(pts, rotation, tolerance, out indexPairs, out fixedIndices);
            SetIndexPairs(indexPairs);
            SetTransform(rotation);
            if (fixedIndices.Count != 0)
            {
                var box = cMesh.Mesh.GetBoundingBox(true);
                double dist = (box.Max - box.Min).Length;
                var v = 100 * dist * rotationAxis.Direction;
                var axis = new Line(rotationAxis.From - v, rotationAxis.From + v);
                OnCurve constriant = new OnCurve(cMesh, axis.ToNurbsCurve(), fixedIndices.ToArray(), 1.0);
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
        //        Point3d pt1Rot = new Point3d(pt1);
        //        Point3d pt2Rot = new Point3d(pt2);
        //        Transform rot = Transform.Rotation(_rotationAngle, _rotationPlane.Normal, _rotationPlane.Origin);
        //        Transform rotT = Transform.Rotation(-_rotationAngle, _rotationPlane.Normal, _rotationPlane.Origin);
        //        pt1Rot.Transform(rot);
        //        pt2Rot.Transform(rotT);
        //        Vector3d dif1 = pt2 - pt1Rot;
        //        Vector3d dif2 = pt1 - pt2Rot;
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
        //        Point3d pt1Rot = new Point3d(pt1);
        //        Point3d pt2Rot = new Point3d(pt2);
        //        Transform rot = Transform.Rotation(_rotationAngle, _rotationPlane.Normal, _rotationPlane.Origin);
        //        Transform rotT = Transform.Rotation(-_rotationAngle, _rotationPlane.Normal, _rotationPlane.Origin);
        //        pt1Rot.Transform(rot);
        //        pt2Rot.Transform(rotT);
        //        Vector3d dif1 = pt2 - pt1Rot;
        //        Vector3d dif2 = pt1 - pt2Rot;

        //        Vector3d dif1Rot = new Vector3d(dif1);
        //        Vector3d dif2Rot = new Vector3d(dif2);

        //        dif1Rot.Transform(rot);
        //        dif2Rot.Transform(rotT);

        //        Vector3d drdx1 = (dif2 - dif1Rot) / (_edgeAverageLength * _edgeAverageLength);
        //        Vector3d drdx2 = (-dif2Rot + dif1) / (_edgeAverageLength * _edgeAverageLength);

        //        for (int j = 0; j < 3; j++)
        //        {
        //            elements.Add(new Tuple<int, int, double>(i, 3*id1+j, drdx1[j]));
        //            elements.Add(new Tuple<int, int, double>(i, 3*id2+j, drdx2[j]));
        //        }

        //    }
        //    return Matrix<double>.Build.SparseOfIndexed(rows, columns, elements);

        //}
    }
}
