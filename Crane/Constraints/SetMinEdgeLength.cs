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
    public class SetMinEdgeLength : Constraint
    {
        public SetMinEdgeLength(double minEdgeLength)
        {
            this.minEdgeLength = minEdgeLength;
        }
        private double minEdgeLength;
        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = 0;
            int columns = cMesh.Mesh.Vertices.Count * 3;
            List<Tuple<int, int, double>> elements = new List<Tuple<int, int, double>>();

            for (int i = 0; i < cMesh.Mesh.TopologyEdges.Count; i++)
            {
                IndexPair ind = cMesh.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d a = cMesh.Vertices[ind.I];
                Point3d b = cMesh.Vertices[ind.J];

                double length = a.DistanceTo(b);
                double goalLength = minEdgeLength + 0.1;
                if (length < minEdgeLength)
                {
                    
                    for (int j = 0; j < 3; j++)
                    {
                        double var1 = (a[j] - b[j]) / (goalLength * goalLength);
                        int rind1 = rows;
                        int cind1 = 3 * ind.I + j;
                        Tuple<int, int, double> element1 = Tuple.Create(rind1, cind1, var1);

                        double var2 = (b[j] - a[j]) / (goalLength * goalLength);
                        int rind2 = rows;
                        int cind2 = 3 * ind.J + j;
                        Tuple<int, int, double> element2 = Tuple.Create(rind2, cind2, var2);

                        elements.Add(element1);
                        elements.Add(element2);
                    }

                    rows++;
                }

            }

            return new SparseMatrixBuilder(rows, columns, elements);
        }

        public override double[] Error(CMesh cMesh)
        {
            int n = cMesh.Mesh.TopologyEdges.Count;
            List<double> error = new List<double>();
            for (int i = 0; i < n; i++)
            {
                IndexPair ind = cMesh.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d a = cMesh.Vertices[ind.I];
                Point3d b = cMesh.Vertices[ind.J];
                double length = a.DistanceTo(b);
                double goalEdgeLength = minEdgeLength + 0.1;
                if (length < minEdgeLength)
                {
                    error.Add((a.DistanceToSquared(b) / (goalEdgeLength * goalEdgeLength) - 1) / 2);
                }
            }

            return error.ToArray();
        }
    }
}
