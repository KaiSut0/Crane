using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class TransformSymmetry : TransformSymmetryBase
    {

        public TransformSymmetry(CMesh cMesh, Transform transform, List<IndexPair> indexPairs)
        {
            SetTransform(cMesh, transform, indexPairs);
        }

        public TransformSymmetry(CMesh cMesh, Transform transform, List<IndexPair> indexPairs, Line rotationAxis, List<int> fixedIndices)
        {
            SetTransform(cMesh, transform, indexPairs);
            if(fixedIndices.Count!=0)
                SetLineFixedPointConstraint(cMesh, rotationAxis, fixedIndices);
        }
        public TransformSymmetry(CMesh cMesh, Transform transform, List<IndexPair> indexPairs, Plane mirrorPlane, List<int> fixedIndices)
        {
            SetTransform(cMesh, transform, indexPairs);
            if(fixedIndices.Count!=0)
                SetPlaneFixedPointConstraint(cMesh, mirrorPlane, fixedIndices);
        }
        private void SetTransform(CMesh cMesh, Transform transform, List<IndexPair> indexPairs)
        {
            //var pts = cMesh.Mesh.Vertices.ToPoint3dArray().ToList();
            SetIndexPairs(indexPairs);
            SetTransform(transform);
            this._edgeAverageLength = cMesh.EdgeLengthSquared.Select(l => Math.Sqrt(l)).Average();
        }
        private void SetLineFixedPointConstraint(CMesh cMesh, Line rotationAxis, List<int> fixedIndices)
        {
            var box = cMesh.Mesh.GetBoundingBox(true);
            double dist = (box.Max - box.Min).Length;
            var v = 100 * dist * rotationAxis.Direction;
            var axis = new Line(rotationAxis.From - v, rotationAxis.From + v);
            OnCurve constriant = new OnCurve(cMesh, axis.ToNurbsCurve(), fixedIndices.ToArray(), 1.0);
            SetFixedPointConstraint(constriant);
        }
        private void SetPlaneFixedPointConstraint(CMesh cMesh, Plane mirrorPlane, List<int> fixedIndices)
        {
            OnPlane constrant = new OnPlane(cMesh, mirrorPlane, fixedIndices.ToArray(), 1.0);
            SetFixedPointConstraint(constrant);
        }
    }
}
