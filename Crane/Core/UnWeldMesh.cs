using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino;


namespace Crane.Core
{
    public class UnWeldMesh
    {

        public Mesh Mesh { get; private set; }
        public Mesh UnWeldedMesh { get; private set; }

        private Mesh UnWeld(Mesh mesh, List<Line> unWeldEdges)
        {
            Mesh m = mesh.DuplicateMesh();
            var unWeldVertexIndicesList = SortUnWeldVertexIndicesList(m, unWeldEdges);
            foreach(var unWeldVertexIndices in unWeldVertexIndicesList)
            {
                for(int i = 0; i < unWeldVertexIndices.Count; i++)
                {
                    int vertId = unWeldVertexIndices[i];
                    if (i == 0)
                    {
                        //if (TopologyVertexIsOnBoundary(m, vertId))
                        //{
                        //    var pair = new IndexPair(vertId, unWeldVertexIndices[i + 1]);
                        //    UnWeldEdge(pair, false, ref m);
                        //}
                    }
                    else if (i == unWeldVertexIndices.Count - 1)
                    {
                        //if (TopologyVertexIsOnBoundary(m, vertId))
                        //{
                        //    var pair = new IndexPair(unWeldVertexIndices[i - 1], vertId);
                        //    UnWeldEdge(pair, true, ref m);
                        //}
                    }
                    else
                    {
                        var pairFrom = new IndexPair(unWeldVertexIndices[i - 1], vertId);
                        var pairTo = new IndexPair(vertId, unWeldVertexIndices[i + 1]);
                        UnWeldEdge(pairFrom, pairTo, ref m);
                    }
                }
            }
            return m;
        }

        private List<List<int>> SortUnWeldVertexIndicesList(Mesh mesh, List<Line> unWeldEdges)
        {
            //List<int> unWeldVertexIds = new List<int>();
            //foreach(var edge in unWeldEdges)
            //{
            //    unWeldVertexIds.Add(Utils.GetPointID(mesh, edge.From));
            //    unWeldVertexIds.Add(Utils.GetPointID(mesh, edge.To));
            //}
            //unWeldVertexIds
            //List<List<Line>> 
            throw new NotImplementedException();
        }

        private void UnWeldEdge(IndexPair pair, bool isEnd, ref Mesh mesh)
        {

            throw new NotImplementedException();
        }
        private void UnWeldEdge(IndexPair pairFrom, IndexPair pairTo, ref Mesh mesh)
        {
            int unWeldVertId = pairFrom.J;
            var edgeIdFrom = Util.GetTopologyEdgeIndex(mesh, pairFrom);
            var edgeIdTo = Util.GetTopologyEdgeIndex(mesh, pairTo);
            int[] faceIdsFrom = mesh.TopologyEdges.GetConnectedFaces(edgeIdFrom);
            int[] faceIdsTo = mesh.TopologyEdges.GetConnectedFaces(edgeIdTo);
            int faceIdFromRight = -1;
            int faceIdFromLeft = -1;
            int faceIdToRight = -1;
            int faceIdToLeft = -1;
            if(MeshFaceIsRight(mesh.Faces[faceIdsFrom[0]], pairFrom))
            {
                faceIdFromRight = faceIdsFrom[0];
                faceIdFromLeft = faceIdsFrom[1];
            }
            else
            {
                faceIdFromRight = faceIdsFrom[1];
                faceIdFromLeft = faceIdsFrom[0];
            }
            if(MeshFaceIsRight(mesh.Faces[faceIdsTo[0]], pairTo))
            {
                faceIdToRight = faceIdsTo[0];
                faceIdToLeft = faceIdsTo[1];
            }
            else
            {
                faceIdToRight = faceIdsTo[1];
                faceIdToLeft = faceIdsTo[0];
            }
            List<int> rightFaceIds = new List<int>();
            List<int> leftFaceIds = new List<int>();
            GetRightAndLeftFaceIds(mesh, faceIdFromRight, faceIdToRight, faceIdFromLeft, faceIdToLeft, mesh.TopologyVertices.ConnectedFaces(unWeldVertId).ToList(), out rightFaceIds, out leftFaceIds);
            mesh.Vertices.Add(mesh.Vertices[unWeldVertId]);
            int unWeldedVertId = mesh.Vertices.Count - 1;
            foreach(var leftFaceId in leftFaceIds)
            {
                var face = mesh.Faces[leftFaceId];
                int localId = GetFaceVertexLocalIndex(face, unWeldVertId);

                mesh.Faces.SetFace(leftFaceId, ReplaceFaceVertexId(face, localId, unWeldedVertId));
            }
        }
        private int GetFaceVertexLocalIndex(MeshFace face, int vertexGlobalId)
        {
            int localId = -1;
            if (face.A == vertexGlobalId) localId = 0;
            else if (face.B == vertexGlobalId) localId = 1;
            else if (face.C == vertexGlobalId) localId = 2;
            else
            {
                if (face.IsQuad)
                {
                    if (face.D == vertexGlobalId) localId = 3;
                }
            }
            return localId;
        }
        private MeshFace ReplaceFaceVertexId(MeshFace face, int vertexLocalId, int vertexGlobalId)
        {
            MeshFace replacedFace = new MeshFace();
            if (face.IsQuad)
            {
                if (vertexLocalId == 0) replacedFace = new MeshFace(vertexGlobalId, face.B, face.C, face.D);
                else if (vertexLocalId == 1) replacedFace = new MeshFace(face.A, vertexGlobalId, face.C, face.D);
                else if (vertexLocalId == 2) replacedFace = new MeshFace(face.A, face.B, vertexGlobalId, face.D);
                else if (vertexLocalId == 3) replacedFace = new MeshFace(face.A, face.B, face.C, vertexGlobalId);
            }
            else if (face.IsTriangle)
            {
                if (vertexLocalId == 0) replacedFace = new MeshFace(vertexGlobalId, face.B, face.C);
                else if (vertexLocalId == 1) replacedFace = new MeshFace(face.A, vertexGlobalId, face.C);
                else if (vertexLocalId == 2) replacedFace = new MeshFace(face.A, face.B, vertexGlobalId);
            }
            return replacedFace;
        }

        private bool TopologyVertexIsOnBoundary(Mesh mesh, int vertexId)
        {
            int[] edgeIndices = mesh.TopologyVertices.ConnectedEdges(vertexId);
            bool isOnBoundary = false;
            foreach(var edgeId in edgeIndices)
            {
                if (mesh.TopologyEdges.GetConnectedFaces(edgeId).Length == 1) isOnBoundary = true;
            }
            return isOnBoundary;
        }

        private bool MeshFaceIsRight(MeshFace face, IndexPair pair)
        {
            bool isRight = false;
            if (face.IsQuad)
            {
                int i = pair.I;
                int j = pair.J;
                int a = face.A;
                int b = face.B;
                int c = face.C;
                int d = face.D;
                if ((i == a & j == b) ||
                    (i == b & j == c) ||
                    (i == c & j == d) ||
                    (i == d & j == a))
                    isRight = true;
            }
            else if (face.IsTriangle)
            {
                int i = pair.I;
                int j = pair.J;
                int a = face.A;
                int b = face.B;
                int c = face.C;
                if ((i == a & j == b) ||
                    (i == b & j == c) ||
                    (i == c & j == a))
                    isRight = true;
            }
            return isRight;
        }
        private bool MeshFaceIsRight(Mesh mesh, int faceId, int[] faceIds)
        {
            int[] adjFaceIds = mesh.Faces.AdjacentFaces(faceId);
            bool isRight = false;
            foreach(var adjFaceId in adjFaceIds)
            {
                if (faceIds.Contains(adjFaceId)) isRight = true;
            }
            return isRight;
        }
        private void GetRightAndLeftFaceIds(Mesh mesh, int faceIdFromRight, int faceIdToRight, int faceIdFromLeft, int faceIdToLeft, List<int> faceIds, out List<int> rightFaceIds, out List<int> leftFaceIds)
        {
            leftFaceIds = new List<int>(faceIds.ToArray());
            leftFaceIds.Remove(faceIdFromRight);
            leftFaceIds.Remove(faceIdToRight);
            leftFaceIds.Remove(faceIdFromLeft);
            leftFaceIds.Remove(faceIdToLeft);
            rightFaceIds = new List<int>();
            rightFaceIds.Add(faceIdFromRight);
            int faceId = faceIdFromRight;
            int i = 0;
            while(faceId != faceIdToRight & i < 10)
            {
                int faceIdTmp = GetAdjacentFaceId(mesh, faceId, leftFaceIds.ToArray());
                rightFaceIds.Add(faceIdTmp);
                leftFaceIds.Remove(faceIdTmp);
                faceId = faceIdTmp;
                i++;
            }
        }
        private int GetAdjacentFaceId(Mesh mesh, int faceId, int[] faceIds)
        {
            int[] adjFaceIds = mesh.Faces.AdjacentFaces(faceId);

            foreach(var adjFaceId in adjFaceIds)
            {
                if (faceIds.Contains(adjFaceId)) return adjFaceId;
            }
            return -1;
        }
    }
}
