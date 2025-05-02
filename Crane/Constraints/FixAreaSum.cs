using Crane.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Crane.Constraints
{
    public class FixAreaSum : Constraint
    {
        public FixAreaSum(double goalArea, double stiffness)
        {
            this.goalArea = goalArea;
            this.stiffness = stiffness;
        }

        private readonly double stiffness;
        private readonly double goalArea;
        public override double[] Error(CMesh cMesh)
        {
            int numFace = cMesh.Mesh.Faces.Count;
            Point3d[] pts = cMesh.Mesh.Vertices.ToPoint3dArray();
            

            double area = 0;

            for (int fId = 0; fId < numFace; fId++)
            {
                
                var face = cMesh.Mesh.Faces[fId];
                int v0Id = face.A;
                int v1Id = face.B;
                int v2Id = face.C;
                Point3d v0 = pts[v0Id];
                Point3d v1 = pts[v1Id];
                Point3d v2 = pts[v2Id];
                Vector3d e1 = v1 - v0;
                Vector3d e2 = v2 - v0;
                area += 0.5 * (Vector3d.CrossProduct(e1, e2).Length);
            }

            return new double[] { stiffness * (area - goalArea) };
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = 1;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elems = new List<Tuple<int, int, double>>();

            int numFace = cMesh.Mesh.Faces.Count;
            
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray();

            for (int fId = 0; fId < numFace; fId++)
            {
                var face = cMesh.Mesh.Faces[fId];
                int v0Id = face.A;
                int v1Id = face.B;
                int v2Id = face.C;
                Point3d v0 = pts[v0Id];
                Point3d v1 = pts[v1Id];
                Point3d v2 = pts[v2Id];
                Vector3d e1 = v1 - v0;
                Vector3d e2 = v2 - v0;
                Vector3d n = Vector3d.CrossProduct(e1, e2);
                n.Unitize();
                Vector3d dAdv0 = stiffness * 0.5 * Vector3d.CrossProduct(e1 - e2, n);
                Vector3d dAdv1 = stiffness * 0.5 * Vector3d.CrossProduct(e2, n);
                Vector3d dAdv2 = stiffness * 0.5 * Vector3d.CrossProduct(-e1, n);
                for(int j = 0; j < 3; j++)
                {
                    elems.Add(new Tuple<int, int, double>(0, 3 * v0Id + j, dAdv0[j]));
                    elems.Add(new Tuple<int, int, double>(0, 3 * v1Id + j, dAdv1[j]));
                    elems.Add(new Tuple<int, int, double>(0, 3 * v2Id + j, dAdv2[j]));
                }
            }

            return new SparseMatrixBuilder(rows, columns, elems);
        }
    }
}
