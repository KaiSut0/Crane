﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MathNet.Numerics;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using Rhino;


namespace Crane.Core
{
    public class Util
    {
        public Util()
        {
        }

        public static Vector<double> Vector3d2VectorDouble(Vector3d vec3d)
        {
            Vector<double> vecD = new DenseVector(3);
            vecD[0] = vec3d.X;
            vecD[1] = vec3d.Y;
            vecD[2] = vec3d.Z;
            return vecD;
        }

        public static Vector<double> Point3d2VectorDouble(Point3d pt3d)
        {
            Vector<double> vecD = new DenseVector(3);
            vecD[0] = pt3d.X;
            vecD[1] = pt3d.Y;
            vecD[2] = pt3d.Z;
            return vecD;
        }

        public static Transform Multiply(double scale, Transform transform)
        {
            Transform result = new Transform(0);
            result.M00 = scale * transform.M00;
            result.M01 = scale * transform.M01;
            result.M02 = scale * transform.M02;
            result.M10 = scale * transform.M10;
            result.M11 = scale * transform.M11;
            result.M12 = scale * transform.M12;
            result.M20 = scale * transform.M20;
            result.M21 = scale * transform.M21;
            result.M22 = scale * transform.M22;
            return result;
        }

        public static Transform Addition(Transform t1, Transform t2)
        {
            Transform result = new Transform(0);
            result.M00 = t1.M00 + t2.M00;
            result.M01 = t1.M01 + t2.M01;
            result.M02 = t1.M02 + t2.M02;
            result.M10 = t1.M10 + t2.M10;
            result.M11 = t1.M11 + t2.M11;
            result.M12 = t1.M12 + t2.M12;
            result.M20 = t1.M20 + t2.M20;
            result.M21 = t1.M21 + t2.M21;
            result.M22 = t1.M22 + t2.M22;
            return result;
        }

        private static List<int> CreateIDList(int idNum)
        {
            var idList = new List<int>();
            for (int i = 0; i < idNum; i++)
            {
                idList.Add(i);
            }

            return idList;
        }

        private static bool IsMirrored(Point3d pt1, Point3d pt2, Plane pln, double tolerance)
        {
            var mirror = Transform.Mirror(pln);
            Point3d pt1Mirrored = new Point3d(pt1);
            pt1Mirrored.Transform(mirror);
            var isMirrored = false;
            if (pt2.DistanceTo(pt1Mirrored) < tolerance) isMirrored = true;
            return isMirrored;
        }

        private static bool IsRotated(Point3d pt1, Point3d pt2, Plane pln, double angle, double tolerance)
        {
            var rot = Transform.Rotation(angle, pln.Normal, pln.Origin);
            Point3d pt1Rot = new Point3d(pt1);
            pt1Rot.Transform(rot);
            var isRot = false;
            if (pt2.DistanceTo(pt1Rot) < tolerance) isRot = true;
            return isRot;
        }

        private static bool IsTransformed(Point3d pt1, Point3d pt2, Transform trans, double tolerance)
        {
            Point3d pt1Trans = new Point3d(pt1);
            pt1Trans.Transform(trans);
            var isTrans = false;
            if (pt2.DistanceTo(pt1Trans) < tolerance) isTrans = true;
            return isTrans;
        }
        private static bool HasMirrored(List<Point3d> edgeLocations, int searchID, List<int> searchIDs, Plane pln,
            double tolerance, out int id)
        {
            bool hasMirrored = false;
            id = -1;
            foreach (var sID in searchIDs)
            {
                if (sID != searchID)
                {
                    bool isMirrored = IsMirrored(edgeLocations[searchID], edgeLocations[sID], pln, tolerance);
                    if (isMirrored)
                    {
                        hasMirrored = true;
                        id = sID;
                    }
                }
            }

            return hasMirrored;
        }

        private static bool HasRotated(List<Point3d> pts, int searchID, List<int> searchIDs, Plane pln, double angle,
            double tolerance, out int id)
        {
            bool hasRot = false;
            id = -1;
            foreach (var sID in searchIDs)
            {
                if (sID != searchID)
                {
                    bool isRot = IsRotated(pts[searchID], pts[sID], pln, angle, tolerance);
                    if (isRot)
                    {
                        hasRot = true;
                        id = sID;
                    }
                }
            }

            return hasRot;
        }

        private static bool HasTransformed(Point3d[] pts, int searchId, List<int> searchIds, Transform trans,
            double tolerance, out int id)
        {
            bool hasTrans = false;
            id = -1;
            foreach (var sId in searchIds)
            {
                bool isTrans = IsTransformed(pts[searchId], pts[sId], trans, tolerance);
                if (isTrans)
                {
                    hasTrans = true;
                    id = sId;
                }
            }

            return hasTrans;
        }

        public static List<IndexPair> CreateMirrorIndexPairs(List<Point3d> pts, Plane plane, double tolerance)
        {
            List<IndexPair> idPairs = new List<IndexPair>();
            List<int> IDList = CreateIDList(pts.Count);
            int i = 0;
            while (i < pts.Count && IDList.Count > 0)
            {
                int id = IDList[0];
                int pairID = -1;
                bool hasMirrored = HasMirrored(pts, id, IDList, plane, tolerance, out pairID);
                if (hasMirrored)
                {
                    IDList.Remove(pairID);
                    IndexPair pair = new IndexPair(id, pairID);
                    idPairs.Add(pair);
                }

                IDList.RemoveAt(0);
                i++;
            }

            return idPairs;
        }

        public static List<IndexPair> CreateRotateIndexPairs(List<Point3d> pts, Plane plane, double angle,
            double tolerance)
        {
            List<IndexPair> idPairs = new List<IndexPair>();
            List<int> IDList = CreateIDList(pts.Count);
            int i = 0;
            while (i < pts.Count && IDList.Count > 0)
            {
                int id = IDList[0];
                int pairID = -1;
                bool hasRot = HasRotated(pts, id, IDList, plane, angle, tolerance, out pairID);
                if (hasRot)
                {
                    IDList.Remove(pairID);
                    IndexPair pair = new IndexPair(id, pairID);
                    idPairs.Add(pair);
                }

                IDList.RemoveAt(0);
                i++;
            }

            return idPairs;

        }

        public static void CreateTransformIndexPairs(Point3d[] pts, Transform trans, double tolearance,
            out List<IndexPair> transIndexPair, out List<int> fixedIndices)
        {
            transIndexPair = new List<IndexPair>();
            fixedIndices = new List<int>();
            List<int> IDList = CreateIDList(pts.Length);
            int i = 0;
            while (i < pts.Length && IDList.Count > 0)
            {
                int id = IDList[i];
                bool hasTrans = HasTransformed(pts, id, IDList, trans, tolearance, out var pairID);
                if (hasTrans)
                {
                    if (id == pairID) fixedIndices.Add(id);
                    else
                    {
                        var pair = new IndexPair(id, pairID);
                        transIndexPair.Add(pair);
                    }
                }
                i++;
            }

        }

        public static void SplitIndexPairs(List<IndexPair> indexPairs, out List<int> firstIndices, out List<int> secondIndices)
        {
            firstIndices = new List<int>();
            secondIndices = new List<int>();
            foreach (var pair in indexPairs)
            {
                firstIndices.Add(pair.I);
                secondIndices.Add(pair.J);
            }
        }

        public static List<IndexPair> MergeIndexPairs(List<int> firstIndices, List<int> secondIndices)
        {
            if (firstIndices.Count != secondIndices.Count) return null;
            var indexPairs = new List<IndexPair>();
            for(int i = 0; i < firstIndices.Count; i++)
            {
                int f = firstIndices[i];
                int s = secondIndices[i];
                indexPairs.Add(new IndexPair(f, s));
            }

            return indexPairs;
        }

        public static List<IndexPair> CreateMirrorEdgeIndexPairs(CMesh cMesh, Plane plane, double tolerance)
        {
            int n = cMesh.Mesh.TopologyEdges.Count;
            var topEdges = cMesh.Mesh.TopologyEdges;

            List<Point3d> pts = new List<Point3d>();
            Point3d[] verts = cMesh.Mesh.Vertices.ToPoint3dArray();

            for (int i = 0; i < n; i++)
            {
                var pair = topEdges.GetTopologyVertices(i);
                Point3d a = verts[pair.I];
                Point3d b = verts[pair.J];
                Point3d edgeMid = (a + b) / 2.0;
                pts.Add(edgeMid);
            }

            return CreateMirrorIndexPairs(pts, plane, tolerance);
        }

        public static IndexPair[] CreateDevelopEdgeIndexPairs(Mesh withoutDev, Mesh withDev)
        {
            int numEdges = withoutDev.TopologyEdges.Count;
            int numVertices = withoutDev.TopologyVertices.Count;
            IndexPair[] edgeIndexPairs = new IndexPair[numEdges];
            for (int i = 0; i < numEdges; i++)
            {
                var ePair = withoutDev.TopologyEdges.GetTopologyVertices(i);
                int vidI = ePair.I;
                int vidJ = ePair.J;
                int vidIdev = vidI + numVertices;
                int vidJdev = vidJ + numVertices;
                int edgeId = GetTopologyEdgeIndex(withDev, new IndexPair(vidI, vidJ));
                int edgeIdDev = GetTopologyEdgeIndex(withDev, new IndexPair(vidIdev, vidJdev));
                edgeIndexPairs[i] = new IndexPair(edgeId, edgeIdDev);
            }

            return edgeIndexPairs;

        }

        public static List<IndexPair> OffsetIndexPairs(List<IndexPair> indexPairs, int offset)
        {
            List<IndexPair> offsetIndexPairs = new List<IndexPair>();
            foreach(var indexPair in indexPairs)
            {
                int i = indexPair.I + offset;
                int j = indexPair.J + offset;

                offsetIndexPairs.Add(new IndexPair(i, j));
            }

            return offsetIndexPairs;
        }
        public static int GetPointID(Mesh mesh, Point3d pt)
        {
            var pcld = new PointCloud(mesh.Vertices.ToPoint3dArray());
            return pcld.ClosestPoint(pt);
        }

        public static bool PointsMatchLineEnds(Line line, Point3d pt1, Point3d pt2, double threshold)
        {
            bool match = false;
            var dist1From = line.From.DistanceTo(pt1);
            var dist2From = line.From.DistanceTo(pt2);
            var dist1To = line.To.DistanceTo(pt1);
            var dist2To = line.To.DistanceTo(pt2);
            if (dist1From + dist2To < threshold) match = true;
            if (dist1To + dist2From < threshold) match = true;
            return match;
        }

        public static int GetTopologyEdgeIndex(Mesh mesh, IndexPair topVertIndexPair)
        {
            int[] conEdgeIdsI = mesh.TopologyVertices.ConnectedEdges(topVertIndexPair.I);
            int[] conEdgeIdsJ = mesh.TopologyVertices.ConnectedEdges(topVertIndexPair.J);
            var intersectEdges = conEdgeIdsI.Intersect(conEdgeIdsJ);
            int edgeid = -1;
            if (intersectEdges.Count() == 1)
            {
                edgeid = intersectEdges.First();
            }
            return edgeid;
        }

        public static Mesh JoinMesh(Mesh mesh, Mesh devMesh)
        {
            Mesh joinedMesh = new Mesh();
            int numVerts = mesh.Vertices.Count;
            joinedMesh.Vertices.AddVertices(mesh.Vertices.ToPoint3dArray());
            joinedMesh.Vertices.AddVertices(devMesh.Vertices.ToPoint3dArray());
            joinedMesh.Faces.AddFaces(mesh.Faces);
            foreach (var face in devMesh.Faces)
            {
                if (face.IsQuad)
                {
                    int A = face.A + numVerts;
                    int B = face.B + numVerts;
                    int C = face.C + numVerts;
                    int D = face.D + numVerts;
                    joinedMesh.Faces.AddFace(new MeshFace(A, B, C, D));
                }
                else
                {
                    int A = face.A + numVerts;
                    int B = face.B + numVerts;
                    int C = face.C + numVerts;
                    joinedMesh.Faces.AddFace(new MeshFace(A, B, C));
                }
            }
            joinedMesh.FaceNormals.ComputeFaceNormals();
            joinedMesh.Normals.ComputeNormals();
            return joinedMesh;
        }
        public static int NumberOfUpperThan(double[] dArray, double threshold)
        {
            int count = 0;
            foreach (var d in dArray)
            {
                if (Math.Abs(d) > threshold) count++;
            }

            return count;
        }


        public static int SvdRank(double[] s)
        {
            double tolerance = 1e-6;
            return s.Count(t => Math.Abs(t) > tolerance);
        }

        public static IEnumerable<IndexPair> CloneIndexPairs(IEnumerable<IndexPair> origIndexPairs)
        {
            return origIndexPairs.Select(pair => new IndexPair(pair.I, pair.J));
        }

        public static IEnumerable<Tuple<double, double>> CloneTuples(IEnumerable<Tuple<double, double>> tuples)
        {
            return tuples.Select(tuple => new Tuple<double, double>(tuple.Item1, tuple.Item2));
        }

        public static void LogMap(Transform T1, out double angle, out Vector3d axis, out Plane scalePlane,
            out Vector3d scale, out Vector3d translation)
        {
            Transform rotation;
            Transform orthogonal;
            T1.DecomposeAffine(out translation, out rotation, out orthogonal, out scale);
            angle = Math.Acos((rotation[0, 0] + rotation[1, 1] + rotation[2, 2] - 1) / 2);
            if (angle != 0)
            {
                axis = 1.0 / (2 * Math.Sin(angle))
                       * (new Vector3d(
                           rotation[2, 1] - rotation[1, 2],
                           rotation[0, 2] - rotation[2, 0],
                           rotation[1, 0] - rotation[0, 1]));
            }
            else axis = new Vector3d();

            scalePlane = new Plane(Point3d.Origin, 
                new Vector3d(orthogonal[0, 0], orthogonal[0, 1], orthogonal[0, 2]),
                new Vector3d(orthogonal[1, 0], orthogonal[1, 1], orthogonal[1, 2]));
        }

        public static Point3d ToPoint3d(double[] coord)
        {
            return new Point3d(coord[0], coord[1], coord[2]);
        }

        public static MeshFace ToMeshFace(int[] face)
        {
            if (face.Length == 3)
            {
                return new MeshFace(face[0], face[1], face[2]);
            }
            else if (face.Length == 4)
            {
                return new MeshFace(face[0], face[1], face[2], face[3]);
            }
            else
            {
                throw new Exception("The number of indices is not 3 or 4.");
            }

        }

        public static double ComputeAngleFrom3Pts(Point3d left, Point3d center, Point3d right)
        {
            return Vector3d.VectorAngle(left - center, right - center);
        }

        public static bool AlignFaceOrientation(MeshFace face, int vertId1, int vertId2)
        {
            int fv1 = 0;
            int fv2 = 0;
            if (face.IsTriangle)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (face[i] == vertId1)
                    {
                        fv1 = i;
                    }

                    if (face[i] == vertId2)
                    {
                        fv2 = i;
                    }
                }

                if (fv2 == fv1 + 1 || fv2 == fv1 - 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (face[i] == vertId1)
                    {
                        fv1 = i;
                    }

                    if (face[i] == vertId2)
                    {
                        fv2 = i;
                    }
                }

                if (fv2 == fv1 + 1 || fv2 == fv1 - 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public static int GetFaceIndexFromEdgeIndexPair(Mesh mesh, int[] faces, IndexPair edgeIndexPair)
        {
            int[] topFaceI = mesh.TopologyEdges.GetConnectedFaces(edgeIndexPair.I);
            int[] topFaceJ = mesh.TopologyEdges.GetConnectedFaces(edgeIndexPair.J);
            return topFaceI.Intersect(topFaceJ).First();
        }

        public static int[] GetSortedFaceIndices(Mesh mesh, int vertexIndex)
        {
            mesh.TopologyVertices.SortEdges();
            var edges = mesh.TopologyVertices.ConnectedEdges(vertexIndex);
            var tfaces = mesh.TopologyVertices.ConnectedFaces(vertexIndex);
            var shiftEdges = new List<int>();
            var sortedFaces = new List<int>();
            int loopNum = edges.Length;
            if (!IsInnerVertex(mesh, vertexIndex)) loopNum -= 1;
            for (int i = 0; i < loopNum; i++)
            {
                shiftEdges.Add(edges[(i + 1) % edges.Length]);
                sortedFaces.Add(GetFaceIndexFromEdgeIndexPair(mesh, tfaces, new IndexPair(edges[i], shiftEdges[i])));
            }
            return sortedFaces.ToArray();
        }

        public static void FillMeshPolygonHole(int[] polygonVertexIndices, ref Mesh mesh)
        {
            int n = polygonVertexIndices.Length;
            int numFace = mesh.Faces.Count;
            var faceIndexList = new List<int>();
            for (int i = 1; i < n - 1; i++)
            {
                mesh.Faces.AddFace(polygonVertexIndices[0], polygonVertexIndices[i], polygonVertexIndices[i + 1]);
                faceIndexList.Add((i - 1) + numFace);
            }
            var ngon = MeshNgon.Create(polygonVertexIndices, faceIndexList);
            mesh.Ngons.AddNgon(ngon);
        }

        public static bool IsInnerVertex(Mesh mesh, int vertexIndex)
        {
            bool isInnerVertex = true;
            int[] connectedEdgeIndices = mesh.TopologyVertices.ConnectedEdges(vertexIndex);
            foreach (var ei in connectedEdgeIndices)
            {
                if (mesh.TopologyEdges.GetConnectedFaces(ei).Length == 1) isInnerVertex = false;
            }
            return isInnerVertex;
        }

        public static bool IsInnerEdge(Mesh mesh, int edgeIndex)
        {
            bool isInnerEdge = true;
            if (mesh.TopologyEdges.GetConnectedFaces(edgeIndex).Length == 1) isInnerEdge = false;
            return isInnerEdge;
        }

        public static Vector3d MatrixVector3dMulltiplication(Matrix<double> mat, Vector3d vec)
        {
            var resvec = mat * Vector<double>.Build.Dense(new double[] { vec.X, vec.Y, vec.Z });
            return new Vector3d(resvec[0], resvec[1], resvec[2]);
        }

        public static Matrix<double> OutorProduct(Vector3d a, Vector3d b)
        {
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { a.X * b.X, a.X * b.Y, a.X * b.Z },
                { a.Y * b.X, a.Y * b.Y, a.Y * b.Z },
                { a.Z * b.X, a.Z * b.Y, a.Z * b.Z },
            });
        }
    }
}
