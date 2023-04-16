using Crane.Core;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crane.Constraints
{
    public class FixArea : Constraint
    {
        public FixArea(CMesh cMesh, List<Point3d> faceCenters, List<double> goalAreas, List<double> stiffnesses)
        {
            numGoalAreas = goalAreas.Count;
            this.goalAreas = goalAreas.ToArray();
            faceIds = new int[numGoalAreas];
            for(int i = 0; i < numGoalAreas; i++) faceIds[i] = cMesh.GetFaceIdFromFaceCenter(faceCenters[i]);
            this.stiffnesses = stiffnesses.ToArray();
        }

        private readonly int[] faceIds;
        private readonly double[] goalAreas;
        private readonly int numGoalAreas;
        private readonly double[] stiffnesses;
        public override double[] Error(CMesh cMesh)
        {
            double[] err = new double[numGoalAreas];
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < numGoalAreas; i++)
            {
                var fId = faceIds[i];
                var face = cMesh.Mesh.Faces[fId];
                int v0Id = face.A;
                int v1Id = face.B;
                int v2Id = face.C;
                Point3d v0 = pts[v0Id];
                Point3d v1 = pts[v1Id];
                Point3d v2 = pts[v2Id];
                Vector3d e1 = v1 - v0;
                Vector3d e2 = v2 - v0;
                double area = 0.5 * (Vector3d.CrossProduct(e1, e2).Length);
                double goalArea = goalAreas[i];
                double stiffness = stiffnesses[i];
                err[i] = stiffness * (area - goalArea);
            }
            return err;
        }

        public override SparseMatrixBuilder Jacobian(CMesh cMesh)
        {
            int rows = numGoalAreas;
            int columns = cMesh.DOF;
            List<Tuple<int, int, double>> elems = new List<Tuple<int, int, double>>();
            
            var pts = cMesh.Mesh.Vertices.ToPoint3dArray();

            for (int i = 0; i < numGoalAreas; i++)
            {
                var fId = faceIds[i];
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
                double stiffness = stiffnesses[i];
                Vector3d dAdv0 = stiffness * 0.5 * Vector3d.CrossProduct(e1 - e2, n);
                Vector3d dAdv1 = stiffness * 0.5 * Vector3d.CrossProduct(e2, n);
                Vector3d dAdv2 = stiffness * 0.5 * Vector3d.CrossProduct(-e1, n);
                for(int j = 0; j < 3; j++)
                {
                    elems.Add(new Tuple<int, int, double>(i, 3 * v0Id + j, dAdv0[j]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * v1Id + j, dAdv1[j]));
                    elems.Add(new Tuple<int, int, double>(i, 3 * v2Id + j, dAdv2[j]));
                }
            }

            return new SparseMatrixBuilder(rows, columns, elems);
        }
    }
}
