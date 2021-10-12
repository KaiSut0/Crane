using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crane.Core;
using Rhino;
using Rhino.Geometry;

namespace Crane.Patterns
{
    public class TrancatedStarOrigamize
    {
        public CMesh CMesh { get; private set; }
        public Mesh Mesh { get; private set; }
        public List<Point3d> BaseVertices { get; private set; }
        public List<Point3d> TopVertices { get; private set; }
        public List<Line> MountainLines { get; private set; }
        public List<Line> ValleyLines { get; private set; }
        public List<Line> TriangulatedLines { get; private set; }

        public TrancatedStarOrigamize(Mesh mesh, double shrinkRatio, double offsetRatio, double trancateRatio, double rotation)
        {
            CreateMesh(mesh, shrinkRatio, offsetRatio, trancateRatio, rotation);
        }
        private void CreateMesh(Mesh mesh, double shrinkRatio, double offsetRatio, double truncatedRatio, double rotation)
        {
            Mesh star = new Mesh();
            MountainLines = new List<Line>();
            ValleyLines = new List<Line>();
            TriangulatedLines = new List<Line>();

            // Add offseted original vertices
            Point3d[] origVerts = mesh.Vertices.ToPoint3dArray();
            Point3d[] offsetVerts = new Point3d[origVerts.Length];
            List<Point3d> topVerts = new List<Point3d>();

            int vertexIdCount = 0;

            // create <edgeFromVertexId, edgeToVertexId> to top vertex id map
            Dictionary<Tuple<int, int>, int> edgeVertexId2TopVertexId = new Dictionary<Tuple<int, int>, int>();

            for (int i = 0; i < origVerts.Length; i++)
            {
                var isNaked = mesh.GetNakedEdgePointStatus()[i];
                var normal = new Vector3d(mesh.Normals[i]);
                double averageLength = 0;
                mesh.TopologyVertices.SortEdges();
                var tEdges = mesh.TopologyVertices.ConnectedEdges(i);
                foreach (var tEdge in tEdges)
                {
                    averageLength += offsetRatio * mesh.TopologyEdges.EdgeLine(tEdge).Length / (double)(tEdges.Length);
                }
                offsetVerts[i] = origVerts[i] - averageLength * normal;

                int edgeCount = 0;
                foreach (var tEdge in tEdges)
                {
                    var endVertIds = mesh.TopologyEdges.GetTopologyVertices(tEdge);
                    int toVertId = endVertIds.I;
                    if (endVertIds.I == i)
                        toVertId = endVertIds.J;
                    edgeVertexId2TopVertexId.Add(new Tuple<int, int>(i, toVertId), vertexIdCount + offsetVerts.Length);
                    Vector3d topMoveVec = mesh.Vertices[toVertId] - mesh.Vertices[i];
                    topMoveVec.Transform(Transform.Rotation(rotation, -normal, Point3d.Origin));
                    var topVert = offsetVerts[i] + truncatedRatio * topMoveVec;
                    topVerts.Add(topVert);
                    if (isNaked)
                    {
                        if (edgeCount != 0 && edgeCount != (tEdges.Length - 1))
                        {
                            TriangulatedLines.Add(new Line(offsetVerts[i], topVerts[vertexIdCount]));
                        }
                    }
                    else
                    {
                        TriangulatedLines.Add(new Line(offsetVerts[i], topVerts[vertexIdCount]));
                    }
                    if (edgeCount != (tEdges.Length - 1) && edgeCount != 0)
                    {
                        star.Faces.AddFace(i, vertexIdCount + offsetVerts.Length - 1, vertexIdCount + offsetVerts.Length);
                        ValleyLines.Add(new Line(topVerts[vertexIdCount], topVerts[vertexIdCount - 1]));
                        TriangulatedLines.Add(new Line(offsetVerts[i], topVerts[vertexIdCount]));
                    }
                    else if (edgeCount == (tEdges.Length - 1))
                    {
                        star.Faces.AddFace(i, vertexIdCount + offsetVerts.Length - 1, vertexIdCount + offsetVerts.Length);
                        ValleyLines.Add(new Line(topVerts[vertexIdCount], topVerts[vertexIdCount - 1]));
                        if (!isNaked)
                        {
                            star.Faces.AddFace(i, vertexIdCount + offsetVerts.Length, vertexIdCount + offsetVerts.Length - tEdges.Length + 1);
                            ValleyLines.Add(new Line(topVerts[vertexIdCount - tEdges.Length + 1], topVerts[vertexIdCount]));
                        }
                    }
                    vertexIdCount++;
                    edgeCount++;
                }
            }
            //TopVertices = offsetVerts.ToList();
            //star.Vertices.AddVertices(offsetVerts);
            TopVertices = new List<Point3d>();
            TopVertices.AddRange(offsetVerts);
            TopVertices.AddRange(topVerts);
            star.Vertices.AddVertices(offsetVerts);
            star.Vertices.AddVertices(topVerts);
            
            //int vertID = origVerts.Length;
            int vertID = topVerts.Count + offsetVerts.Length;

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
                {
                    star.Faces.AddFace(fVertIDs[0], fVertIDs[1], fVertIDs[2]);
                    MountainLines.Add(new Line(
                        shrinkedVerts[fVertIDs[0] - topVerts.Count - offsetVerts.Length],
                        shrinkedVerts[fVertIDs[1] - topVerts.Count - offsetVerts.Length]));
                    MountainLines.Add(new Line(
                        shrinkedVerts[fVertIDs[1] - topVerts.Count - offsetVerts.Length],
                        shrinkedVerts[fVertIDs[2] - topVerts.Count - offsetVerts.Length]));
                    MountainLines.Add(new Line(
                        shrinkedVerts[fVertIDs[2] - topVerts.Count - offsetVerts.Length],
                        shrinkedVerts[fVertIDs[0] - topVerts.Count - offsetVerts.Length]));
                }
                else
                {
                    star.Faces.AddFace(fVertIDs[0], fVertIDs[1], fVertIDs[2], fVertIDs[3]);
                    MountainLines.Add(new Line(
                        shrinkedVerts[fVertIDs[0] - topVerts.Count - offsetVerts.Length],
                        shrinkedVerts[fVertIDs[1] - topVerts.Count - offsetVerts.Length]));
                    MountainLines.Add(new Line(
                        shrinkedVerts[fVertIDs[1] - topVerts.Count - offsetVerts.Length],
                        shrinkedVerts[fVertIDs[2] - topVerts.Count - offsetVerts.Length]));
                    MountainLines.Add(new Line(
                        shrinkedVerts[fVertIDs[2] - topVerts.Count - offsetVerts.Length],
                        shrinkedVerts[fVertIDs[3] - topVerts.Count - offsetVerts.Length]));
                    MountainLines.Add(new Line(
                        shrinkedVerts[fVertIDs[3] - topVerts.Count - offsetVerts.Length],
                        shrinkedVerts[fVertIDs[0] - topVerts.Count - offsetVerts.Length]));
                }
            }
            star.Vertices.AddVertices(shrinkedVerts);
            BaseVertices = shrinkedVerts;

            Dictionary<Tuple<int, int>, bool> wasEdgeVisited = new Dictionary<Tuple<int, int>, bool>();
            // create inserted mesh
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                bool isNaked = mesh.GetNakedEdgePointStatus()[i];
                mesh.TopologyVertices.SortEdges(i);
                var tEdges = mesh.TopologyVertices.ConnectedEdges(i);
                int edgeCount = 0;
                foreach (var tEdge in tEdges)
                {
                    var endIDs = mesh.TopologyEdges.GetTopologyVertices(tEdge);
                    var to = endIDs.I;
                    if (to == i) to = endIDs.J;

                    bool visited = false;
                    if (wasEdgeVisited.ContainsKey(new Tuple<int, int>(to, i)))
                    {
                        visited = true;
                    }
                    else
                    {
                        wasEdgeVisited[new Tuple<int, int>(i, to)] = true;
                    }

                    int top = edgeVertexId2TopVertexId[new Tuple<int, int>(i, to)];
                    int topNext = top + 1;

                    var faces = mesh.TopologyEdges.GetConnectedFaces(tEdge);
                    if (faces.Length == 2)
                    {
                        var facePair = SortFacePair(mesh, faces, i, to);
                        var faceLeftVertexIndexPairI = GetFaceLeftVertexIndexPair(mesh.Faces[facePair.I], i);
                        var faceLeftVertexIndexPairJ = GetFaceLeftVertexIndexPair(mesh.Faces[facePair.J], i);
                        int rFrom = vertexMap[new Tuple<int, int>(facePair.I, faceLeftVertexIndexPairI.I)];
                        int rTo = vertexMap[new Tuple<int, int>(facePair.I, faceLeftVertexIndexPairI.J)];
                        int l = vertexMap[new Tuple<int, int>(facePair.J, faceLeftVertexIndexPairJ.I)];
                        if (edgeCount == 0)
                        {
                            star.Faces.AddFace(top, top + tEdges.Length - 1, rTo);
                            ValleyLines.Add(new Line(star.Vertices[top], star.Vertices[rTo]));
                            ValleyLines.Add(new Line(star.Vertices[top + tEdges.Length - 1], star.Vertices[rTo]));

                        }
                        else
                        {
                            star.Faces.AddFace(top, top - 1, rTo);
                            ValleyLines.Add(new Line(star.Vertices[top], star.Vertices[rTo]));
                            ValleyLines.Add(new Line(star.Vertices[top - 1], star.Vertices[rTo]));

                        }
                        //star.Faces.AddFace(top, rFrom, rTo);
                        star.Faces.AddFace(top, rTo, l);
                        MountainLines.Add(new Line(star.Vertices[top], star.Vertices[l]));
                        //if (edgeCount == 1 && isNaked)
                        //    star.Faces.AddFace(top - 1, rFrom, top);
                        if (edgeCount == 0)
                        {
                            star.Faces.AddFace(top + tEdges.Length - 1, rFrom, rTo);
                        }
                        else
                            star.Faces.AddFace(top - 1, rFrom, rTo);
                        //star.Faces.AddFace(top, l, topNext);
                        //if (edgeCount == tEdges.Length - 1) ;
                        //    star.Faces.AddFace(top, l, topNext - tEdges.Length);
                        if (edgeCount == tEdges.Length - 2 && isNaked)
                        {
                            star.Faces.AddFace(top, l, topNext);
                        }

                        if (!visited)
                        {
                            MountainLines.Add(new Line(star.Vertices[rTo], star.Vertices[l]));
                        }
                    }

                    edgeCount++;
                }
            }
            star.Compact();
            star.FaceNormals.ComputeFaceNormals();
            star.Normals.ComputeNormals();
            var splited = star.SplitDisjointPieces();
            foreach (var s in splited)
            {
                if (s.Vertices.Count > 10)
                    Mesh = s;
            }
            CMesh = new CMesh(Mesh, MountainLines, ValleyLines, TriangulatedLines);
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
