using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Crane.Constraints;
using Crane.Core;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class Chair : Pattern
    {
        public Chair(double height, double panelSize)
        {

            _height = height;
            _panelSize = panelSize;

            SetCMesh();
            SetConstraints();
        }

        private double _height;
        private double _panelSize;
        protected override void SetCMesh()
        {
            Mesh mesh = CreateMesh(_height, _panelSize);
            Point3d[] vs = mesh.Vertices.ToPoint3dArray();

            List<Line> M = new List<Line>();
            List<Line> V = new List<Line>();

            M.Add(new Line(vs[0], vs[6]));
            M.Add(new Line(vs[1], vs[7]));
            M.Add(new Line(vs[6], vs[8]));
            M.Add(new Line(vs[6], vs[9]));
            M.Add(new Line(vs[7], vs[8]));
            M.Add(new Line(vs[7], vs[9]));
            V.Add(new Line(vs[6], vs[7]));

            CMesh cMesh = new CMesh(mesh, M, V);
            CMesh = cMesh;
        }

        protected override void SetConstraints()
        {
            List<Constraint> consts = new List<Constraint>();
            Plane plane = new Plane(new Point3d(0, 0, _height), Vector3d.ZAxis);
            consts.Add(new Developable());
            consts.Add(new MirrorSymmetry(this.CMesh, Plane.WorldYZ, 1e-3));
            consts.Add(new MirrorSymmetricEdgeLength(this.CMesh, Plane.WorldYZ, 1e-3));
            consts.Add(new OnPlane(this.CMesh, plane, new int[]{6, 7, 8}, 1.0));
            consts.Add(new OnPlane(this.CMesh, Plane.WorldXY, new[] { 0, 1, 2, 3, 4, 5 }, 1.0));
            consts.Add(new GlueVertices(this.CMesh, 2, 3));
            consts.Add(new GlueVertices(this.CMesh, 4, 5));

            Constraints = consts;
        }

        private Mesh CreateMesh(double height, double panelSize)
        {
            var eps = 1e-15;
            Point3d p1 = new Point3d(-panelSize / 2, 0, 0);
            Point3d p2 = new Point3d(panelSize / 2, 0, 0);
            Point3d p3 = new Point3d(-eps, -Math.Sqrt(3) * panelSize / 2, 0);
            Point3d p4 = new Point3d(eps, -Math.Sqrt(3) * panelSize / 2, 0);
            Point3d p5 = new Point3d(-eps, panelSize / 2, 0);
            Point3d p6 = new Point3d(eps, panelSize / 2, 0);
            Vector3d h = new Vector3d(0, 0, height);
            Point3d p7 = p1 + h;
            Point3d p8 = p2 + h;
            Point3d p9 = p3 + h;
            Point3d p10 = p5 + 2 * h;

            Mesh mesh = new Mesh();
            mesh.Vertices.AddVertices(new Point3d[]{p1, p2, p3, p4, p5, p6, p7, p8, p9, p10});
            mesh.Faces.AddFace(0, 2, 8, 6);
            mesh.Faces.AddFace(7, 8, 3, 1);
            mesh.Faces.AddFace(4, 0, 6, 9);
            mesh.Faces.AddFace(9, 7, 1, 5);
            mesh.Faces.AddFace(6, 8, 7);
            mesh.Faces.AddFace(9, 6, 7);
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();

            return mesh;
        }
    }
}
