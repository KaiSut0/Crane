using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class TranslationalSymmetry : TransformSymmetryBase
    {
        public TranslationalSymmetry(CMesh cMesh, Vector3d transVector, double tolerance)
        {
            List<IndexPair> indexPairs;
            List<int> fixedIndices;
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray().ToList();
            Transform transform = Transform.Translation(transVector);
            Utils.CreateTransformIndexPairs(pts, transform, tolerance, out indexPairs, out fixedIndices);
            SetIndexPairs(indexPairs);
            SetTransform(transform);
        }

        public TranslationalSymmetry(CMesh cMesh, Vector3d transVector, Vector3d goalTransVector, double tolerance)
        {
            List<IndexPair> indexPairs;
            List<int> fixedIndices;
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray().ToList();
            Transform transform = Transform.Translation(transVector);
            Utils.CreateTransformIndexPairs(pts, transform, tolerance, out indexPairs, out fixedIndices);
            SetIndexPairs(indexPairs);
            Transform goalTransform = Transform.Translation(goalTransVector);
            SetTransform(goalTransform);
        }

        public TranslationalSymmetry(CMesh cMesh, Vector3d transVector, List<IndexPair> indexPairs)
        {
            Transform transform = Transform.Translation(transVector);
            SetIndexPairs(indexPairs);
            SetTransform(transform);
        }

    }
}
