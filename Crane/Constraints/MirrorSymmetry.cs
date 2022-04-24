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
    }

}
