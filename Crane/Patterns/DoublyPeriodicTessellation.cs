using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Crane.Constraints;
using Crane.Core;
using QuickGraph;
using Rhino;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class DoublyPeriodicTessellation : Pattern
    {
        public DoublyPeriodicTessellation(Point3d cornerVertex, List<Point3d> edgeVerticesS1,
            List<Point3d> edgeVerticess2, List<Point3d> innerVertices,
            List<MeshFace> faces, Transform s1, Transform s2, bool is2x2)
        {
            var pts = new List<Point3d>();
            List<Point3d> s1PtsFrom = new List<Point3d>();
            List<Point3d> s2PtsFrom = new List<Point3d>();
            List<Point3d> s1PtsTo = new List<Point3d>();
            List<Point3d> s2PtsTo = new List<Point3d>();

            {
                var v = cornerVertex;

                var v1 = new Point3d(v);
                v1.Transform(s1);

                var v2 = new Point3d(v);
                v2.Transform(s2);

                var v12 = new Point3d(v);
                v12.Transform(s1);
                v12.Transform(s2);

                pts.AddRange(new[] {v, v1, v2, v12});

                s1PtsFrom.AddRange(new[] {v, v2});
                s1PtsTo.AddRange(new[] {v1, v12});

                s2PtsFrom.AddRange(new[] {v, v1});
                s2PtsTo.AddRange(new[] {v2, v12});
            }   

            foreach (Point3d v in edgeVerticesS1)
            {
                var v1 = new Point3d(v);
                v1.Transform(s1);

                pts.AddRange(new[] { v, v1 });
                s1PtsFrom.Add(v);
                s1PtsTo.Add(v1);
            }

            foreach (Point3d v in edgeVerticess2)
            {
                var v1 = new Point3d(v);
                v1.Transform(s2);

                pts.AddRange(new[] {v, v1});
                s2PtsFrom.Add(v);
                s2PtsTo.Add(v1);
                
            }

            foreach (Point3d p in innerVertices)
            {
                pts.Add(p);
            }

            Mesh mesh = new Mesh();
            mesh.Vertices.AddVertices(pts);
            mesh.Faces.AddFaces(faces);
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();

            if (is2x2)
            {
                var copiedMesh = new Mesh();
                var s1Mesh = mesh.DuplicateMesh();
                var s2Mesh = mesh.DuplicateMesh();
                var s12Mesh = mesh.DuplicateMesh();
                s1Mesh.Transform(s1);
                s2Mesh.Transform(s2);
                s12Mesh.Transform(s1);
                s12Mesh.Transform(s2);
                copiedMesh.Append(mesh);
                copiedMesh.Append(s1Mesh);
                copiedMesh.Append(s2Mesh);
                copiedMesh.Append(s12Mesh);
                copiedMesh.Weld(0);
                copiedMesh.Vertices.CombineIdentical(true, true);

                mesh = copiedMesh;

                {
                    var v = cornerVertex;
                    var vS1 = new Point3d(v);
                    vS1.Transform(s1);

                    var vS1S1 = new Point3d(v);
                    vS1S1.Transform(s1);
                    vS1S1.Transform(s1);

                    var vS2 = new Point3d(v);
                    vS2.Transform(s2);

                    var vS2S2 = new Point3d(v);
                    vS2S2.Transform(s2);
                    vS2S2.Transform(s2);

                    var vS1S2 = new Point3d(v);
                    vS1S2.Transform(s1);
                    vS1S2.Transform(s2);

                    var vS1S2S1 = new Point3d(v);
                    vS1S2S1.Transform(s1);
                    vS1S2S1.Transform(s2);
                    vS1S2S1.Transform(s1);

                    var vS1S2S2 = new Point3d(v);
                    vS1S2S2.Transform(s1);
                    vS1S2S2.Transform(s2);
                    vS1S2S2.Transform(s2);

                    var vS1S2S1S2 = new Point3d(v);
                    vS1S2S1S2.Transform(s1);
                    vS1S2S1S2.Transform(s2);
                    vS1S2S1S2.Transform(s1);
                    vS1S2S1S2.Transform(s2);


                    s1PtsFrom.AddRange(new [] { vS1, vS1S2, vS2S2, vS1S2S2 });
                    s1PtsTo.AddRange(new[] { vS1S1, vS1S2S1, vS1S2S2, vS1S2S1S2 });

                    s2PtsFrom.AddRange(new [] {vS2, vS1S2, vS1S1, vS1S2S1 });
                    s2PtsTo.AddRange(new [] {vS2S2, vS1S2S2, vS1S2S1, vS1S2S1S2 });
                }

                foreach (Point3d v in edgeVerticesS1)
                {
                    var vS1 = new Point3d(v);
                    vS1.Transform(s1);

                    var vS1S1 = new Point3d(v);
                    vS1S1.Transform(s1);
                    vS1S1.Transform(s1);

                    var vS2 = new Point3d(v);
                    vS2.Transform(s2);

                    var vS2S2 = new Point3d(v);
                    vS2S2.Transform(s2);
                    vS2S2.Transform(s2);

                    var vS1S2 = new Point3d(v);
                    vS1S2.Transform(s1);
                    vS1S2.Transform(s2);

                    var vS1S2S1 = new Point3d(v);
                    vS1S2S1.Transform(s1);
                    vS1S2S1.Transform(s2);
                    vS1S2S1.Transform(s1);

                    s1PtsFrom.AddRange(new [] { vS1, vS2, vS1S2 });
                    s1PtsTo.AddRange(new[] { vS1S1, vS1S2, vS1S2S1 });

                    s2PtsFrom.AddRange(new [] {v, vS1, vS1S1});
                    s2PtsTo.AddRange(new[] {vS2, vS1S2, vS1S2S1});

                }

                foreach (Point3d v in edgeVerticess2)
                {
                    var vS1 = new Point3d(v);
                    vS1.Transform(s1);

                    var vS2 = new Point3d(v);
                    vS2.Transform(s2);

                    var vS2S2 = new Point3d(v);
                    vS2S2.Transform(s2);
                    vS2S2.Transform(s2);

                    var vS1S2 = new Point3d(v);
                    vS1S2.Transform(s1);
                    vS1S2.Transform(s2);

                    var vS1S2S2 = new Point3d(v);
                    vS1S2S2.Transform(s1);
                    vS1S2S2.Transform(s2);
                    vS1S2S2.Transform(s2);

                    s1PtsFrom.AddRange(new[] { v, vS2, vS2S2 });
                    s1PtsTo.AddRange(new[] {vS1, vS1S2, vS1S2S2});

                    s2PtsFrom.AddRange(new [] {vS1, vS2, vS1S2 });
                    s2PtsTo.AddRange(new [] {vS1S2, vS2S2, vS1S2S2 });


                }

                foreach (Point3d v in innerVertices)
                {
                    var vS1 = new Point3d(v);
                    vS1.Transform(s1);

                    var vS2 = new Point3d(v);
                    vS2.Transform(s2);

                    var vS1S2 = new Point3d(v);
                    vS1S2.Transform(s1);
                    vS1S2.Transform(s2);

                    s1PtsFrom.AddRange(new [] {v, vS2});
                    s1PtsTo.AddRange(new[] {vS1, vS1S2});

                    s2PtsFrom.AddRange(new [] {v, vS1});
                    s2PtsTo.AddRange(new[] {vS2, vS1S2});
                }

            }

            var ptCloud = new PointCloud(mesh.Vertices.ToPoint3dArray());
            var s1PtsIdPairs = new List<IndexPair>();
            var s2PtsIdPairs = new List<IndexPair>();
            for (int i = 0; i < s1PtsFrom.Count; i++)
            {
                int fromId = ptCloud.ClosestPoint(s1PtsFrom[i]);
                int toId = ptCloud.ClosestPoint(s1PtsTo[i]);
                s1PtsIdPairs.Add(new IndexPair(fromId, toId));
            }
            for (int i = 0; i < s2PtsFrom.Count; i++)
            {
                int fromId = ptCloud.ClosestPoint(s2PtsFrom[i]);
                int toId = ptCloud.ClosestPoint(s2PtsTo[i]);
                s2PtsIdPairs.Add(new IndexPair(fromId, toId));
            }

            CMesh cmesh = new CMesh(mesh);

            var constraints = new List<Constraint>();

            var s1Sym = new TransformSymmetry(cmesh, s1, s1PtsIdPairs);
            var s2Sym = new TransformSymmetry(cmesh, s2, s2PtsIdPairs);

            constraints.AddRange(new [] {s1Sym, s2Sym});

            CMesh = cmesh;
            Constraints = constraints;
        }

        protected override void SetCMesh()
        {
            throw new NotImplementedException();
        }

        protected override void SetConstraints()
        {
            throw new NotImplementedException();
        }

    }
}
