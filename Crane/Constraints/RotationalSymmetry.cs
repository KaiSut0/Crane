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
    }
}
