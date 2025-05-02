using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class TuckOrigamize
    {
        public TuckOrigamize(Mesh originalMesh, double scale, double rotAngle1, double rotAngle2, double offset)
        {
            CreateMesh(originalMesh, scale, rotAngle1, rotAngle2, offset);
        }
        public Mesh Mesh { get; private set; }
        public Dictionary<int, int[]> VertexToTopFaceVertices { get; private set; }
        public Dictionary<int, int[]> VertexToBottomFaceVertices { get; private set; }
        public Dictionary<int, int[]> EdgeToLeftFaceVertices { get; private set; }
        public Dictionary<int, int[]> EdgeToRightFaceVertices { get; private set; }
        public Dictionary<int, int[]> FaceToVertices { get; private set; }
        public List<Line> MountainLines { get; private set; }
        public List<Line> ValleyLines { get; private set; }
        public List<Point3d> GlueVertexList { get; private set; }
        public List<Line> GlueEdgeList { get; private set; }
        private void CreateMesh(Mesh origMesh, double scale, double rotAngle1, double rotAngle2, double offset)
        {
            // Initialize TuckedMesh properties.
            Mesh mesh = new Mesh();
            VertexToTopFaceVertices = new Dictionary<int, int[]>();
            VertexToBottomFaceVertices = new Dictionary<int, int[]>();
            EdgeToLeftFaceVertices = new Dictionary<int, int[]>();
            EdgeToRightFaceVertices = new Dictionary<int, int[]>();
            FaceToVertices = new Dictionary<int, int[]>();
            MountainLines = new List<Line>();
            ValleyLines = new List<Line>();
            GlueVertexList = new List<Point3d>();
            GlueEdgeList = new List<Line>();

            // Initialize origMesh properties.
            origMesh.Normals.ComputeNormals();
            origMesh.FaceNormals.ComputeFaceNormals();
            origMesh.TopologyVertices.SortEdges();

            // Construct TuckedMesh vertices.
            int vIndex = 0;
            int origVCount = origMesh.Vertices.Count;
            for (int i = 0; i < origVCount; i++)
            {
                var pt = origMesh.Vertices.Point3dAt(i);
                var faces = Util.GetSortedFaceIndices(origMesh, i);
                var vertexNormal = origMesh.Normals[i];
                var topFaceVertices = new List<int>();
                var bottomFaceVertices = new List<int>();
                for (int j = 0; j < faces.Length; j++)
                {
                    var f = faces[j];
                    var faceCenter = origMesh.Faces.GetFaceCenter(f);
                    var faceNormal = origMesh.FaceNormals[f];
                    var tpt = 1.0 * pt;
                    tpt.Transform(Transform.Scale(faceCenter, scale));
                    tpt.Transform(Transform.Rotation(rotAngle1, faceNormal, faceCenter));
                    mesh.Vertices.Add(tpt);
                    topFaceVertices.Add(vIndex);
                    vIndex++;
                    var bpt = 1.0 * tpt;
                    bpt.Transform(Transform.Rotation(-2*Math.PI/faces.Length - rotAngle2, vertexNormal, pt));
                    bpt -= (float)offset * vertexNormal;
                    mesh.Vertices.Add(bpt);
                    bottomFaceVertices.Add(vIndex);
                    vIndex++;

                    int faceVertexCount = 3;
                    if (origMesh.Faces[f].IsQuad) faceVertexCount = 4;
                    if (!FaceToVertices.ContainsKey(f))
                    {
                        FaceToVertices[f] = new int[faceVertexCount];
                    }
                    for (int k = 0; k < faceVertexCount; k++)
                    {
                        if (i == origMesh.Faces[f][k])
                        {
                            FaceToVertices[f][k] = topFaceVertices[j];
                        }
                    }

                }
                VertexToTopFaceVertices[i] = topFaceVertices.ToArray();
                VertexToBottomFaceVertices[i] = bottomFaceVertices.ToArray();

                if (!Util.IsInnerVertex(origMesh, i)) continue;
                // Fill mesh face.
                Util.FillMeshPolygonHole(bottomFaceVertices.ToArray(), ref mesh);
                for (int j = 0; j < faces.Length; j++)
                {
                    mesh.Faces.AddFace(topFaceVertices[j], bottomFaceVertices[j],
                        bottomFaceVertices[(j + 1) % faces.Length]);
                }
            }

            // Construct EdgeToFaceVertices.
            int origECount = origMesh.TopologyEdges.Count;
            for (int i = 0; i < origECount; i++)
            {
                if (!Util.IsInnerEdge(origMesh, i)) continue;
                var pair = origMesh.TopologyEdges.GetTopologyVertices(i);
                int I = pair.I;
                int J = pair.J;
                int eLI = GetEdgeLocalIndex(origMesh, i, I);
                int eLJ = GetEdgeLocalIndex(origMesh, i, J);
                var tfvI = VertexToTopFaceVertices[I];
                var tfvJ = VertexToTopFaceVertices[J];
                var bfvI = VertexToBottomFaceVertices[I];
                var bfvJ = VertexToBottomFaceVertices[J];
                int bI = bfvI[eLI];
                int bJ = bfvJ[eLJ];
                int tlI = tfvI[eLI];
                int trI = tfvI[(eLI + tfvI.Length - 1) % tfvI.Length];
                int tlJ = tfvJ[eLJ];
                int trJ = tfvJ[(eLJ + tfvJ.Length - 1) % tfvJ.Length];
                int[] leftVertexIndices = new int[] { tlI, bI, bJ, trJ };
                int[] rightVertexIndices = new int[] { tlJ, bJ, bI, trI };
                EdgeToLeftFaceVertices[i] = leftVertexIndices;
                EdgeToRightFaceVertices[i] = rightVertexIndices;
                Util.FillMeshPolygonHole(leftVertexIndices, ref mesh);
                Util.FillMeshPolygonHole(rightVertexIndices, ref mesh);
                GlueVertexList.Add(mesh.Vertices.Point3dAt(tlI));
                GlueVertexList.Add(mesh.Vertices.Point3dAt(tlJ));
                GlueEdgeList.Add(new Line(mesh.Vertices.Point3dAt(trI), mesh.Vertices.Point3dAt(tlJ)));
                GlueEdgeList.Add(new Line(mesh.Vertices.Point3dAt(trJ), mesh.Vertices.Point3dAt(tlI)));
                MountainLines.Add(new Line(mesh.Vertices[bI], mesh.Vertices[tlI]));
                MountainLines.Add(new Line(mesh.Vertices[bJ], mesh.Vertices[tlJ]));
                ValleyLines.Add(new Line(mesh.Vertices[bI], mesh.Vertices[bJ]));

            }

            for (int i = 0; i < origMesh.Faces.Count; i++)
            {
                var faces = FaceToVertices[i];
                Util.FillMeshPolygonHole(faces, ref mesh);
            }

            mesh.Compact();
            mesh.Normals.ComputeNormals();
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.UnifyNormals();
            Mesh = mesh;
        }

        private int GetEdgeLocalIndex(Mesh mesh, int edgeIndex, int vertexIndex)
        {
            int[] edgeIndices = mesh.TopologyVertices.ConnectedEdges(vertexIndex);
            int localIndex = 0;
            for (int i = 0; i < edgeIndices.Length; i++)
            {
                if (edgeIndex == edgeIndices[i]) localIndex = i;
            }

            return localIndex;
        }
    }
}
