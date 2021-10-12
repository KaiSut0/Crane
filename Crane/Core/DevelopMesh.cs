using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using Rhino;

namespace Crane.Core
{
    public class DevelopMesh
    {
        public DevelopMesh(Mesh mesh, Point2d developmentOrigin, double developmentRotation = 0)
        {
            Mesh = mesh.DuplicateMesh();
            Mesh.FaceNormals.ComputeFaceNormals();
            Develop(developmentOrigin, developmentRotation);
            MeshWithDevelop = Utils.JoinMesh(Mesh, DevelopedMesh);
        }
        public Mesh Mesh { get; private set; }
        public Mesh DevelopedMesh { get; private set; }
        public Mesh MeshWithDevelop { get; private set; }

        private UndirectedGraph<int, SEdge<int>> faceGraph;
        private UndirectedGraph<int, SEdge<int>> faceTree;

        public IndexPair[] GetEdgeIndexPairs()
        {
            int numEdges = Mesh.TopologyEdges.Count;
            int numVertices = Mesh.TopologyVertices.Count;
            IndexPair[] edgeIndexPairs = new IndexPair[numEdges];
            for (int i = 0; i < numEdges; i++)
            {
                var ePair = Mesh.TopologyEdges.GetTopologyVertices(i);
                int vidI = ePair.I;
                int vidJ = ePair.J;
                int vidIdev = vidI + numVertices;
                int vidJdev = vidJ + numVertices;
                int edgeId = Utils.GetTopologyEdgeIndex(MeshWithDevelop, new IndexPair(vidI, vidJ));
                int edgeIdDev = Utils.GetTopologyEdgeIndex(MeshWithDevelop, new IndexPair(vidIdev, vidJdev));
                edgeIndexPairs[i] = new IndexPair(edgeId, edgeIdDev);
            }

            return edgeIndexPairs;
        }

        public IndexPair[] GetVertexIndexPairs()
        {
            int numVertices = Mesh.Vertices.Count;
            IndexPair[] vertexIndexPairs = new IndexPair[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                vertexIndexPairs[i] = new IndexPair(i, i + numVertices);
            }

            return vertexIndexPairs;
        }

        private void Develop(Point2d developmentOrigin, double developmentRotation = 0)
        {
            SetFaceGraph();
            Point3d[] devPts = new Point3d[Mesh.Vertices.Count];
            List<int> faceIDs = new List<int>();

            var bfs = new UndirectedBreadthFirstSearchAlgorithm<int, SEdge<int>>(faceTree);

            
            DevelopInitialFace(Mesh, 0, ref devPts, developmentOrigin, developmentRotation);
            faceIDs.Add(0);


            bfs.DiscoverVertex += fi =>
            {
                var fei = Mesh.TopologyEdges.GetEdgesForFace(fi);
                var adfs = faceTree.AdjacentVertices(fi);
                foreach (var fj in adfs)
                {
                    if (!faceIDs.Contains(fj))
                    {
                        var fej = Mesh.TopologyEdges.GetEdgesForFace(fj);
                        int e = fei.Intersect(fej).First();
                        var pair = Mesh.TopologyEdges.GetTopologyVertices(e);
                        var edgePair = GetFaceEdgeIndexPair(Mesh.Faces[fj], pair);
                        DevelopFace(Mesh, fj, edgePair, ref devPts);
                        faceIDs.Add(fj);
                    }
                }
            };
            bfs.Compute(0);

            SetDevelopedMesh(devPts);
        }

        private void SetDevelopedMesh(Point3d[] pts)
        {
            DevelopedMesh = Mesh.DuplicateMesh();
            DevelopedMesh.Vertices.Destroy();
            DevelopedMesh.Vertices.AddVertices(pts);
            DevelopedMesh.Compact();
            DevelopedMesh.FaceNormals.ComputeFaceNormals();
            DevelopedMesh.Normals.ComputeNormals();
        }
        
        private void SetFaceGraph()
        {
            faceGraph = new UndirectedGraph<int, SEdge<int>>();
            for (int i = 0; i < Mesh.Faces.Count; i++)
                faceGraph.AddVertex(i);
            for (int i = 0; i < Mesh.Faces.Count; i++)
            {
                var afIDs = Mesh.Faces.AdjacentFaces(i);
                foreach (var id in afIDs)
                {
                    faceGraph.AddEdge(new SEdge<int>(i, id));
                }
            }

            var mst = faceGraph.MinimumSpanningTreePrim(e => 1.0);
            faceTree = new UndirectedGraph<int, SEdge<int>>();
            for (int i = 0; i < Mesh.Faces.Count; i++)
                faceTree.AddVertex(i);
            faceTree.AddEdgeRange(mst);
        }

        private void DevelopInitialFace(Mesh mesh, int faceID, ref Point3d[] devPts, Point2d developmentOrigin, double developmentRotation = 0)
        {
            //var bb = mesh.GetBoundingBox(Plane.WorldXY);
            
            var verts = mesh.Vertices.ToPoint3dArray();
            MeshFace face = mesh.Faces[faceID];
            int ptNum = 0;
            if (face.IsQuad) ptNum = 4;
            else ptNum = 3;
            var faceNormal = new Vector3d(mesh.FaceNormals[faceID]);
            var faceEdge = verts[face.B] - verts[face.A];
            faceEdge.Unitize();
            var faceBinormal = Vector3d.CrossProduct(faceNormal, faceEdge);
            var facePlane = new Plane(verts[face.A], faceEdge, faceBinormal);
            var xAxis = Vector3d.XAxis;
            var yAxis = Vector3d.YAxis;
            xAxis.Rotate(developmentRotation, Vector3d.ZAxis);
            yAxis.Rotate(developmentRotation, Vector3d.ZAxis);
            var devPlane = new Plane(new Point3d(developmentOrigin.X, developmentOrigin.Y, 0), xAxis, yAxis);
            //double offset = (bb.Max.X - bb.Min.X);
            //Point3d planeOrigin = new Point3d(bb.Max.X + offset, 0, 0);
            var p2p = Transform.PlaneToPlane(facePlane, devPlane);
            for (int i = 0; i < ptNum; i++)
            {
                var pt = verts[face[i]];
                pt.Transform(p2p);
                devPts[face[i]] = pt;
            }
        }

        private void DevelopFace(Mesh mesh, int faceID, IndexPair idPair, ref Point3d[] devPts)
        {
            var verts = mesh.Vertices.ToPoint3dArray();
            MeshFace face = mesh.Faces[faceID];
            int ptNum = 0;
            if (face.IsQuad) ptNum = 4;
            else ptNum = 3;
            var faceNormal = new Vector3d(mesh.FaceNormals[faceID]);
            var faceEdge = verts[idPair.J] - verts[idPair.I];
            faceEdge.Unitize();
            var devEdge = devPts[idPair.J] - devPts[idPair.I];
            devEdge.Unitize();
            var faceBinormal = Vector3d.CrossProduct(faceNormal, faceEdge);
            var devBinormal = Vector3d.CrossProduct(Vector3d.ZAxis, devEdge);
            var facePlane = new Plane(verts[idPair.I], faceEdge, faceBinormal);
            var devPlane = new Plane(devPts[idPair.I], devEdge, devBinormal);

            var p2p = Transform.PlaneToPlane(facePlane, devPlane);
            for (int i = 0; i < ptNum; i++)
            {
                var pt = verts[face[i]];
                pt.Transform(p2p);
                devPts[face[i]] = pt;
            }
        }

        private IndexPair GetFaceEdgeIndexPair(MeshFace face, IndexPair pair)
        {
            int i = pair.I;
            int j = pair.J;
            IndexPair edgePair = new IndexPair();

            if (face.IsQuad)
            {
                int[] vIDs = new int[] {face.A, face.B, face.C, face.D};
                int iID = Array.IndexOf(vIDs, i);
                int jID = Array.IndexOf(vIDs, j);
                if (iID == 0 & jID == 3)
                {
                    edgePair.I = j;
                    edgePair.J = i;
                }
                else if (iID == 3 & jID == 0)
                {
                    edgePair.I = i;
                    edgePair.J = j;
                }
                else if (iID > jID)
                {
                    edgePair.I = j;
                    edgePair.J = i;
                }
                else if (jID > iID)
                {
                    edgePair.I = i;
                    edgePair.J = j;
                }
            }
            else
            {
                int[] vIDs = new int[] {face.A, face.B, face.C};
                int iID = Array.IndexOf(vIDs, i);
                int jID = Array.IndexOf(vIDs, j);
                if (iID == 0 & jID == 2)
                {
                    edgePair.I = j;
                    edgePair.J = i;
                }
                else if (iID == 2 & jID == 0)
                {
                    edgePair.I = i;
                    edgePair.J = j;
                }
                else if (iID > jID)
                {
                    edgePair.I = j;
                    edgePair.J = i;
                }
                else if (jID > iID)
                {
                    edgePair.I = i;
                    edgePair.J = j;
                }

            }

            return edgePair;
        }

        private Mesh UnweldMesh(Mesh mesh, Line[] unweldLines)
        {
            throw new NotImplementedException();
        }
    }

    
}
