using System;
using System.Collections.Generic;
using System.Linq;
using Crane.Core;
using MathNet.Numerics.LinearAlgebra;
using Rhino;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class DevelopableWithDevelopment : Constraint
    {
        public DevelopableWithDevelopment(CMesh cMesh, IndexPair[] edgeIndexPairs)
        {
            this.edgeIndexPairs = edgeIndexPairs;
            edgeAverageLength = 0;
            for(int i = 0; i < cMesh.Mesh.TopologyEdges.Count; i++)
            {
                edgeAverageLength += cMesh.Mesh.TopologyEdges.EdgeLine(i).Length;
            }

            edgeAverageLength /= cMesh.Mesh.TopologyEdges.Count;
            numEdgePairs = edgeIndexPairs.Length;
            numDevMeshVertices = cMesh.NumberOfVertices / 2;
        }

        private IndexPair[] edgeIndexPairs;
        private readonly double edgeAverageLength = 1.0;
        private readonly int numEdgePairs;
        private readonly int numDevMeshVertices;

        public override Vector<double> Error(CMesh cMesh)
        {
            double[] err = new double[numEdgePairs + numDevMeshVertices];
            //double[] err = new double[numEdgePairs];
            Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < numEdgePairs; i++)
            {
                IndexPair idPair = edgeIndexPairs[i];
                IndexPair ptIDPairI = cMesh.Mesh.TopologyEdges.GetTopologyVertices(idPair.I);
                IndexPair ptIDPairJ = cMesh.Mesh.TopologyEdges.GetTopologyVertices(idPair.J);

                Point3d ptII = verts[ptIDPairI.I];
                Point3d ptIJ = verts[ptIDPairI.J];
                Point3d ptJI = verts[ptIDPairJ.I];
                Point3d ptJJ = verts[ptIDPairJ.J];

                err[i] = (ptII.DistanceToSquared(ptIJ) - ptJI.DistanceToSquared(ptJJ)) / (2.0 * edgeAverageLength * edgeAverageLength);

            }

            for (int i = numDevMeshVertices; i < 2 * numDevMeshVertices; i++)
            {
                Point3d pt = verts[i];
                err[numEdgePairs + i - numDevMeshVertices] = 0.5 * pt.Z * pt.Z;
            }

            return Vector<double>.Build.DenseOfArray(err);
        }

        public override Matrix<double> Jacobian(CMesh cMesh)
        {
            Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();
            int rows = numEdgePairs + numDevMeshVertices;
            //int rows = numEdgePairs;
            int cols = 3 * verts.Length;

            for (int i = 0; i < numEdgePairs; i++)
            {
                IndexPair idPair = edgeIndexPairs[i];
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

            for (int i = numDevMeshVertices; i < 2 * numDevMeshVertices; i++)
            {
                Point3d pt = verts[i];
                double var = pt.Z;
                int rID = numEdgePairs + i - numDevMeshVertices;
                int cID = 3 * i + 2;
                elements.Add(new Tuple<int, int, double>(rID, cID, var));
            }

            return Matrix<double>.Build.SparseOfIndexed(rows, cols, elements);
        }
    }
}