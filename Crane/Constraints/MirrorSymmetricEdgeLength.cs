using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Grasshopper.GUI.Canvas;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class MirrorSymmetricEdgeLength : Constraint
    {
        public MirrorSymmetricEdgeLength(CMesh cMesh, Plane plane, double tolerance)
        {
            _indexPairs = Util.CreateMirrorEdgeIndexPairs(cMesh, plane, tolerance).ToArray();
            _plane = plane;
            edgeAverageLength = cMesh.EdgeLengthSquared.Average();
            n = _indexPairs.Length;
        }

        private readonly IndexPair[] _indexPairs;
        private readonly Plane _plane;
        private readonly double edgeAverageLength = 1.0;
        private readonly int n;

        public override double[] Error(CMesh cMesh)
        {
            double[] err = new double[n];
            Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < n; i++)
            {
                IndexPair idPair = _indexPairs[i];
                IndexPair ptIDPairI = cMesh.Mesh.TopologyEdges.GetTopologyVertices(idPair.I);
                IndexPair ptIDPairJ = cMesh.Mesh.TopologyEdges.GetTopologyVertices(idPair.J);

                Point3d ptII = verts[ptIDPairI.I];
                Point3d ptIJ = verts[ptIDPairI.J];
                Point3d ptJI = verts[ptIDPairJ.I];
                Point3d ptJJ = verts[ptIDPairJ.J];

                err[i] = (ptII.DistanceToSquared(ptIJ) - ptJI.DistanceToSquared(ptJJ)) / (2.0 * edgeAverageLength * edgeAverageLength);
            }

            return err;
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            int rows = n;
            int cols = 3 * verts.Length;

            for (int i = 0; i < n; i++)
            {
                IndexPair idPair = _indexPairs[i];
                IndexPair ptIDPairI = cMesh.Mesh.TopologyEdges.GetTopologyVertices(idPair.I);
                IndexPair ptIDPairJ = cMesh.Mesh.TopologyEdges.GetTopologyVertices(idPair.J);

                Point3d ptII = verts[ptIDPairI.I];
                Point3d ptIJ = verts[ptIDPairI.J];
                Point3d ptJI = verts[ptIDPairJ.I];
                Point3d ptJJ = verts[ptIDPairJ.J];

                Vector3d vI = ptII - ptIJ;
                Vector3d vJ = ptJI - ptJJ;

                for (int j = 0; j < 3; j++)
                {
                    double var0 = vI[j] / (edgeAverageLength * edgeAverageLength);
                    int rID0 = i;
                    int cID0 = 3 * ptIDPairI.I + j;
                    elements.Add(new Tuple<int, int, double>(rID0, cID0, var0));

                    double var1 = -vI[j] / (edgeAverageLength * edgeAverageLength);
                    int rID1 = i;
                    int cID1 = 3 * ptIDPairI.J + j;
                    elements.Add(new Tuple<int, int, double>(rID1, cID1, var1));

                    double var2 = -vJ[j] / (edgeAverageLength * edgeAverageLength);
                    int rID2 = i;
                    int cID2 = 3 * ptIDPairJ.I + j;
                    elements.Add(new Tuple<int, int, double>(rID2, cID2, var2));

                    double var3 = vJ[j] / (edgeAverageLength * edgeAverageLength);
                    int rID3 = i;
                    int cID3 = 3 * ptIDPairJ.J + j;
                    elements.Add(new Tuple<int, int, double>(rID3, cID3, var3));
                }
            }

            return new SparseMatrixBuilder(rows, cols, elements);
        }
    }
}
