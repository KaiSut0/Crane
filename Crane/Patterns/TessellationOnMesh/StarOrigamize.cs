using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class StarOrigamize
    {
        public Mesh Mesh { get; private set; }
        public List<Point3d> BaseVertices { get; private set; }
        public List<Point3d> TopVertices { get; private set; }

        public StarOrigamize(Mesh mesh, double shrinkRatio, double offsetRatio)
        {
            CreateMesh(mesh, shrinkRatio, offsetRatio);
        }
        private void CreateMesh(Mesh mesh, double shrinkRatio, double offsetRatio)
        {
            Mesh star = new Mesh();

            // Add offseted original vertices
            Point3d[] origVerts = mesh.Vertices.ToPoint3dArray();
            Point3d[] offsetVerts = new Point3d[origVerts.Length];
            for (int i = 0; i < origVerts.Length; i++)
            {
                var normal = new Vector3d(mesh.Normals[i]);
                double averageLength = 0;
                var tEdges = mesh.TopologyVertices.ConnectedEdges(i);
                foreach (var tEdge in tEdges)
                {
                    averageLength += offsetRatio * mesh.TopologyEdges.EdgeLine(tEdge).Length / (double)(tEdges.Length);
                }
                offsetVerts[i] = origVerts[i] + averageLength * normal;
            }
            TopVertices = offsetVerts.ToList();
            star.Vertices.AddVertices(offsetVerts);

            int vertID = origVerts.Length;

            // create <faceID, origVertID> to newVertID map
            Dictionary<Tuple<int, int>, int> vertexMap = new Dictionary<Tuple<int, int>, int>();
            List<Point3d> shrinkedVerts = new List<Point3d>();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var face = mesh.Faces[i];
                int nFace = 3;
                if (face.IsQuad) nFace = 4;
                int[] fVertIDs = new int[nFace];
                var offsets = new Vector3d[nFace];
                var faceCenter = mesh.Faces.GetFaceCenter(i);
                for (int j = 0; j < nFace; j++)
                {
                    var fVert = origVerts[face[j]];
                    var offset = shrinkRatio * (faceCenter - fVert);
                    shrinkedVerts.Add(fVert + offset);
                    vertexMap[new Tuple<int, int>(i, j)] = vertID;
                    fVertIDs[j] = vertID;
                    vertID++;
                }
                if (face.IsTriangle)
                    star.Faces.AddFace(fVertIDs[0], fVertIDs[1], fVertIDs[2]);
                else
                    star.Faces.AddFace(fVertIDs[0], fVertIDs[1], fVertIDs[2], fVertIDs[3]);
            }
            star.Vertices.AddVertices(shrinkedVerts);
            BaseVertices = shrinkedVerts;

            // create inserted mesh
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                mesh.TopologyVertices.SortEdges(i);
                var tEdges = mesh.TopologyVertices.ConnectedEdges(i);
                foreach (var tEdge in tEdges)
                {
                    var endIDs = mesh.TopologyEdges.GetTopologyVertices(tEdge);
                    var to = endIDs.I;
                    if (to == i) to = endIDs.J;

                    var faces = mesh.TopologyEdges.GetConnectedFaces(tEdge);
                    if (faces.Length == 2)
                    {
                        var facePair = SortFacePair(mesh, faces, i, to);
                        var faceLeftVertexIndexPairI = GetFaceLeftVertexIndexPair(mesh.Faces[facePair.I], i);
                        var faceLeftVertexIndexPairJ = GetFaceLeftVertexIndexPair(mesh.Faces[facePair.J], i);
                        int rFrom = vertexMap[new Tuple<int, int>(facePair.I, faceLeftVertexIndexPairI.I)];
                        int rTo = vertexMap[new Tuple<int, int>(facePair.I, faceLeftVertexIndexPairI.J)];
                        int l = vertexMap[new Tuple<int, int>(facePair.J, faceLeftVertexIndexPairJ.I)];
                        star.Faces.AddFace(i, rFrom, rTo);
                        star.Faces.AddFace(i, rTo, l);
                    }
                }
            }
            star.Compact();
            star.FaceNormals.ComputeFaceNormals();
            star.Normals.ComputeNormals();
            Mesh = star;
        }

        private IndexPair GetFaceLeftVertexIndexPair(MeshFace face, int fromVID)
        {
            int I = 0;
            int J = 1;
            int nFace = 3;
            if (face.IsQuad) nFace = 4;
            for (int i = 0; i < nFace; i++)
            {
                if (face[i] == fromVID) I = i;
            }

            if (I != 0)
            {
                J = I - 1;
            }
            else
            {
                J = nFace - 1;
            }

            return new IndexPair(I, J);
        }

        private IndexPair SortFacePair(Mesh mesh, int[] facePair, int fromVID, int toVID)
        {
            int faceI = facePair[0];
            int faceJ = facePair[1];

            int i = faceI;
            int j = faceJ;

            if (IsCounterclockwise(mesh.Faces[faceI], fromVID, toVID))
            {
                i = faceJ;
                j = faceI;
            }

            return new IndexPair(i, j);
        }

        private bool IsCounterclockwise(MeshFace face, int fromVID, int toVID)
        {
            bool isCounterclockwise = false;
            int nFace = 3;
            if (face.IsQuad) nFace = 4;
            int fFromVID = 0;
            int fToVID = 0;
            for (int i = 0; i < nFace; i++)
            {
                if (face[i] == fromVID) fFromVID = i;
                if (face[i] == toVID) fToVID = i;
            }

            if (fFromVID + 1 == fToVID)
            {
                isCounterclockwise = true;
            }

            if (face.IsTriangle)
                if (fFromVID == 2 & fToVID == 0)
                    isCounterclockwise = true;
            if (face.IsQuad)
                if (fFromVID == 3 & fToVID == 0)
                    isCounterclockwise = true;
            return isCounterclockwise;
        }
    }
}
