using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace Crane.Core
{
    public class CMesh
    {
        #region Properties
        public Mesh Mesh { get; private set; }
        public Mesh InitialMesh { get; private set; }
        public PointCloud VerticesCloud { get; private set; }
        public Point3d[] Vertices { get; private set; }
        public Vector3d[] FaceNormals { get; private set; }
        public double VertexSearchTolerance { get; private set; }
        public double AverageEdgeLength { get; private set; }
        public Vector<double> MeshVerticesVector { get; private set; }
        public Vector<double> ConfigulationVector { get; set; }
        public List<double> EdgeLengthSquared { get; set; }
        public double[] CylindricallyRotationAngles { get; set; }
        public double[] CylindricallyTranslationCoefficients { get; set; }
        public Vector3d CylinderAxis { get; set; }
        public Point3d CylinderOrigin { get; set; }
        public int DOF { get; set; }
        public int NumberOfVertices { get; private set; }
        public int NumberOfEdges { get; private set; }
        public double WholeScale { get; set; }
        public bool IsPeriodic { get; set; }
        public bool HasDevelopment { get; private set; } = false;
        public int NumberOfDevelopmentVertices { get; set; }
        public int NumberOfDevelopmentFaces { get; set; }
        public int NumberOfFoldingVertices { get; set; }
        public int NumberOfFoldingFaces { get; set; }
        public List<IndexPair> DevelopmentEdgeIndexPairs { get; private set; }
        public List<IndexPair> DevelopmentVertexIndexPairs { get; private set; }
        public List<List<int>> ConnectedTopologyVerticesList { get; private set; }
        public List<List<int>> ConnectedTopologyEdgesList { get; private set; }
        public List<List<int>> HoleLoopVertexIdsList { get; private set; }
        public List<int> BoundaryLoopVertexIds { get; private set; }
        public double[] FoldingSpeed { get; private set; }
        public List<Char> EdgeInfo { get; set; }
        public MeshFaceList OriginalFaces { get; set; }
        public List<Char> InnerEdgeAssignment { get; set; }
        public List<int> InnerVertexIds { get; set; }
        public List<IndexPair> InnerEdges { get; set; }
        public List<IndexPair> BoundaryEdges { get; set; }
        public List<int> InnerBoundaryEdges { get; set; }
        public List<IndexPair> TriangulatedEdges { get; set; }
        public List<IndexPair> MountainEdges { get; set; }
        public List<IndexPair> ValleyEdges { get; set; }
        public List<IndexPair> FacePairs { get; set; }
        public List<IndexPair> TriangulatedFacePairs { get; set; }
        public List<IndexPair> MountainFacePairs { get; set; }
        public List<IndexPair> ValleyFacePairs { get; set; }
        public List<Tuple<double, double>> FaceHeightPairs { get; set; }
        public List<Tuple<double, double>> TriangulatedFaceHeightPairs { get; set; } 
        public List<Tuple<double, double>> MountainFaceHeightPairs { get; set; }
        public List<Tuple<double, double>> ValleyFaceHeightPairs { get; set; }
        public List<double> LengthOfDiagonalEdges { get; set; }
        public List<double> LengthOfTriangulatedDiagonalEdges { get; set; }
        public List<double> LengthOfMountainDiagonalEdges { get; set; }
        public List<double> LengthOfValleyDiagonalEdges { get; set; }
        public List<double> InitialEdgesLength { get; set; }
        public Dictionary<int, int> EdgeIds2InnerEdgeIds { get; private set; }


#endregion

        public CMesh() { }
        public CMesh(Mesh mesh)
        {
            SetCMeshFromMVT(mesh, new List<Line>(), new List<Line>(), new List<Line>());
        }
        public CMesh(CMesh cMesh)
        {
            this.Mesh = cMesh.Mesh.DuplicateMesh();
            this.InitialMesh = cMesh.InitialMesh.DuplicateMesh();
            UpdateVerticesCloud();
            this.MeshVerticesVector = Vector<double>.Build.DenseOfVector(cMesh.MeshVerticesVector);
            this.ConfigulationVector = Vector<double>.Build.DenseOfVector(cMesh.ConfigulationVector);
            this.EdgeLengthSquared = new List<double>(cMesh.EdgeLengthSquared.ToArray());
            this.CylindricallyRotationAngles = cMesh.CylindricallyRotationAngles.Clone() as double[];
            this.CylindricallyTranslationCoefficients = cMesh.CylindricallyTranslationCoefficients.Clone() as double[];
            this.CylinderAxis = new Vector3d(cMesh.CylinderAxis);
            this.CylinderOrigin = new Point3d(cMesh.CylinderOrigin);
            this.DOF = cMesh.DOF;
            this.NumberOfVertices = cMesh.NumberOfVertices;
            this.WholeScale = cMesh.WholeScale;
            this.IsPeriodic = cMesh.IsPeriodic;
            this.HasDevelopment = cMesh.HasDevelopment;
            this.NumberOfDevelopmentVertices = cMesh.NumberOfDevelopmentVertices;
            this.NumberOfDevelopmentFaces = cMesh.NumberOfDevelopmentFaces;
            this.NumberOfFoldingVertices = cMesh.NumberOfFoldingVertices;
            this.NumberOfFoldingFaces = cMesh.NumberOfFoldingFaces;
            this.AverageEdgeLength = cMesh.AverageEdgeLength;

            this.FoldingSpeed = cMesh.FoldingSpeed;

            this.EdgeInfo = new List<char>(cMesh.EdgeInfo.ToArray());
            this.OriginalFaces = this.InitialMesh.Faces;
            this.InnerEdgeAssignment = new List<char>(cMesh.InnerEdgeAssignment.ToArray());

            this.InnerVertexIds = new List<int>(cMesh.InnerVertexIds.ToArray());

            this.InnerEdges = Util.CloneIndexPairs(cMesh.InnerEdges).ToList();
            this.BoundaryEdges = Util.CloneIndexPairs(cMesh.BoundaryEdges).ToList();
            this.InnerBoundaryEdges = new List<int>(cMesh.InnerBoundaryEdges.ToArray());
            this.TriangulatedEdges = Util.CloneIndexPairs(cMesh.TriangulatedEdges).ToList();
            this.MountainEdges = Util.CloneIndexPairs(cMesh.MountainEdges).ToList();
            this.ValleyEdges = Util.CloneIndexPairs(cMesh.ValleyEdges).ToList();

            this.FacePairs = Util.CloneIndexPairs(cMesh.FacePairs).ToList();
            this.TriangulatedFacePairs = Util.CloneIndexPairs(cMesh.TriangulatedFacePairs).ToList();
            this.MountainFacePairs = Util.CloneIndexPairs(cMesh.MountainFacePairs).ToList();
            this.ValleyFacePairs = Util.CloneIndexPairs(cMesh.ValleyFacePairs).ToList();

            this.FaceHeightPairs = Util.CloneTuples(cMesh.FaceHeightPairs).ToList();
            this.TriangulatedFaceHeightPairs = Util.CloneTuples(cMesh.TriangulatedFaceHeightPairs).ToList();
            this.MountainFaceHeightPairs = Util.CloneTuples(cMesh.MountainFaceHeightPairs).ToList();
            this.ValleyFaceHeightPairs = Util.CloneTuples(cMesh.ValleyFaceHeightPairs).ToList();
            
            this.LengthOfDiagonalEdges = new List<double>(cMesh.LengthOfDiagonalEdges.ToArray());
            this.LengthOfTriangulatedDiagonalEdges = new List<double>(cMesh.LengthOfTriangulatedDiagonalEdges.ToArray());
            this.LengthOfMountainDiagonalEdges = new List<double>(cMesh.LengthOfMountainDiagonalEdges.ToArray());
            this.LengthOfValleyDiagonalEdges = new List<double>(cMesh.LengthOfValleyDiagonalEdges.ToArray());

            this.ConnectedTopologyVerticesList = cMesh.ConnectedTopologyVerticesList;
            this.ConnectedTopologyEdgesList = cMesh.ConnectedTopologyEdgesList;

            this.HoleLoopVertexIdsList = cMesh.HoleLoopVertexIdsList;
            this.BoundaryLoopVertexIds = cMesh.BoundaryLoopVertexIds;
            //this.InitialEdgesLength = new List<double>(cMesh.InitialEdgesLength.ToArray());

        }
        public CMesh(Mesh mesh, List<Line> M, List<Line> V)
        {
            SetCMeshFromMVT(mesh, M, V, new List<Line>());
        }
        public CMesh(Mesh mesh, List<Line> M, List<Line> V, Point2d developmentOrigin, double developmentRotaiton = 0)
        {
            SetCMeshFromMVTWithDevelopment(mesh, M, V, new List<Line>(), developmentOrigin, developmentRotaiton);
        }
        public CMesh(Mesh mesh, List<Line> M, List<Line> V, List<Line> T)
        {
            SetCMeshFromMVT(mesh, M, V, T);
        }
        public CMesh(Mesh mesh, List<Line> M, List<Line> V, List<Line> T, Point2d developmentOrigin, double developmentRotation = 0)
        {
            SetCMeshFromMVTWithDevelopment(mesh, M, V, T, developmentOrigin, developmentRotation);
        }
        public CMesh(Mesh mesh, List<IndexPair> M_indexPairs, List<IndexPair> V_indexPairs) : this(mesh, PairsToLines(mesh, M_indexPairs), PairsToLines(mesh, V_indexPairs))
        {
        }

        public CMesh(Mesh mesh, List<IndexPair> M_indexPairs, List<IndexPair> V_indexPairs,
            List<IndexPair> T_indexPairs) : this(mesh, PairsToLines(mesh, M_indexPairs),
            PairsToLines(mesh, V_indexPairs), PairsToLines(mesh, T_indexPairs))
        { }

        public CMesh(List<Line> lines, List<Line> M, List<Line> V, List<Line> T, int maxFaceValence, double tolerance)
        {
            Mesh mesh = Mesh.CreateFromLines(lines.Select(line => line.ToNurbsCurve()).ToArray(), maxFaceValence, tolerance);
            mesh.Normals.ComputeNormals();
            mesh.FaceNormals.ComputeFaceNormals();
            SetCMeshFromMVT(mesh, M, V, T);
        }
        private void SetCMeshFromMVT(Mesh mesh, List<Line> M, List<Line> V, List<Line> T)
        {
            this.Mesh = mesh.DuplicateMesh();
            this.InitialMesh = mesh.DuplicateMesh();
            this.DOF = mesh.Vertices.Count * 3;
            this.Mesh.FaceNormals.ComputeFaceNormals();
            this.OriginalFaces = mesh.Faces;
            var tri = this.InsertTriangulate();
            UpdateVerticesCloud();
            UpdateFaceNormals();

            //this.Mesh.TopologyVertices.SortEdges();
            SetConnectedTopologyVertices();
            this.NumberOfVertices = mesh.Vertices.Count;
            this.NumberOfEdges = mesh.TopologyEdges.Count;
            var edges = this.Mesh.TopologyEdges;

            SetInnerVertexIds();

            this.EdgeInfo = new List<Char>();
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges.IsNgonInterior(i))
                {
                    EdgeInfo.Add('T');
                }
                else
                {
                    EdgeInfo.Add('U');
                }
            }
            foreach (Line m in M)
            {
                int fromIndex = VerticesCloud.ClosestPoint(m.From);
                int toIndex = VerticesCloud.ClosestPoint(m.To);
                int ind = edges.GetEdgeIndex(fromIndex, toIndex);
                if (ind != -1)
                {
                    EdgeInfo[ind] = 'M';
                }
            }
            foreach (Line v in V)
            {
                int fromIndex = VerticesCloud.ClosestPoint(v.From);
                int toIndex = VerticesCloud.ClosestPoint(v.To);
                int ind = edges.GetEdgeIndex(fromIndex, toIndex);
                if (ind != -1)
                {
                    EdgeInfo[ind] = 'V';
                }
            }
            if (tri.Count != 0)
            {
                foreach (List<int> v in tri)
                {
                    int ind = edges.GetEdgeIndex(v[0], v[1]);
                    if (ind != -1)
                    {
                        this.EdgeInfo[ind] = 'T';
                    }
                }
            }
            foreach (Line t in T)
            {
                int fromIndex = VerticesCloud.ClosestPoint(t.From);
                int toIndex = VerticesCloud.ClosestPoint(t.To);
                int ind = edges.GetEdgeIndex(fromIndex, toIndex);
                if (ind != -1)
                {
                    EdgeInfo[ind] = 'T';
                }
            }

            List<bool> naked = new List<bool>(mesh.GetNakedEdgePointStatus());

            for (int i = 0; i < this.Mesh.TopologyEdges.Count; i++)
            {
                if (edges.GetConnectedFaces(i).Count() == 1)
                {

                    this.EdgeInfo[i] = 'B';
                }
            }
            SetMeshFundamentalInfo();
            SetTriangulatedFacePairBasicInfo();
            SetMountainFacePairBasicInfo();
            SetValleyFacePairBasicInfo();
            this.InnerEdgeAssignment = GetInnerEdgeAssignment();
            this.SetMeshVerticesVector();
            this.SetConfigulationVector();
            this.SetPeriodicParameters();
            if(mesh.GetNakedEdges()!=null) SetNakedLoopInfo();
            ComputeInitialProperties();
            SetNgon();
            SetFoldingSpeed(new double[this.InnerEdges.Count]);
            HasDevelopment = false;
        }
        private void SetCMeshFromMVTWithDevelopment(Mesh mesh, List<Line> M, List<Line> V, List<Line> T, Point2d developmentOrigin, double developmentRotation = 0)
        {
            CMesh cMesh = new CMesh(mesh, M, V, T);
            var dev = new DevelopMesh(cMesh.Mesh, developmentOrigin, developmentRotation);
            Mesh joinedMesh = Util.JoinMesh(dev.Mesh, dev.DevelopedMesh);
            SetCMeshFromMVT(joinedMesh, M, V, cMesh.GetTraiangulatedEdgeLines());
            NumberOfDevelopmentFaces = Mesh.Faces.Count/2;
            NumberOfDevelopmentVertices = Mesh.Vertices.Count/2;
            NumberOfFoldingFaces = Mesh.Faces.Count/2;
            NumberOfFoldingVertices = Mesh.Vertices.Count/2;
            DevelopmentEdgeIndexPairs = dev.GetEdgeIndexPairs().ToList();
            DevelopmentVertexIndexPairs = dev.GetVertexIndexPairs().ToList();
            HasDevelopment = true;
        }
        private static List<Line> PairsToLines(Mesh mesh, List<List<int>> Pairs)
        {
            var verts = mesh.Vertices;

            List<Line> M = new List<Line>();
            foreach (var m in Pairs)
            {
                M.Add(new Line(verts[m[0]], verts[m[1]]));
            }

            return M;

        }
        private static List<Line> PairsToLines(Mesh mesh, List<IndexPair> pairs)
        {
            var verts = mesh.Vertices;
            List<Line> M = new List<Line>();
            foreach(var m in pairs)
            {
                M.Add(new Line(verts[m.I], verts[m.J]));
            }
            return M;
        }
        private List<List<int>> InsertTriangulate()
        {
            var faces = this.Mesh.Faces;
            var verts = this.Mesh.Vertices;

            List<List<int>> ans = new List<List<int>>();

            List<MeshFace> newFaces = new List<MeshFace>();

            for (int i = 0; i < faces.Count; i++)
            {
                MeshFace face = faces[i];
                if (face[2] != face[3])
                {
                    double d1 = verts[face[0]].DistanceTo(verts[face[2]]);
                    double d2 = verts[face[1]].DistanceTo(verts[face[3]]);
                    if (d1 >= d2)
                    {
                        newFaces.Add(new MeshFace(face[1], face[2], face[0]));
                        newFaces.Add(new MeshFace(face[3], face[0], face[2]));
                        List<int> temp = new List<int>();
                        temp.Add(face[0]);
                        temp.Add(face[2]);
                        ans.Add(temp);
                    }
                    else
                    {
                        newFaces.Add(new MeshFace(face[0], face[1], face[3]));
                        newFaces.Add(new MeshFace(face[2], face[3], face[1]));
                        List<int> temp = new List<int>();
                        temp.Add(face[1]);
                        temp.Add(face[3]);
                        ans.Add(temp);
                    }
                }
                else
                {
                    newFaces.Add(new MeshFace(face[0], face[1], face[2]));
                }
            }

            this.Mesh.Faces.Destroy();
            this.Mesh.Faces.AddFaces(newFaces);

            return ans;
        }
        public List<double> GetFoldAngles()
        {
            var edges = this.InnerEdges;
            var verts = this.Mesh.Vertices;
            var faces = this.FacePairs;

            List<double> foldang = new List<double>();

            this.Mesh.FaceNormals.ComputeFaceNormals();

            for (int e = 0; e < edges.Count; e++)
            {
                double foldang_e = 0;
                int u = edges[e].I;
                int v = edges[e].J;
                int p = faces[e].I;
                int q = faces[e].J;
                Vector3d normal_i = this.Mesh.FaceNormals[p];
                Vector3d normal_j = this.Mesh.FaceNormals[q];
                /// cos(foldang_e) = n_i * n_j
                double cos_foldang_e = normal_i * normal_j;
                /// sin(foldang_e) = n_i × n_j
                Vector3d vec_e = verts[u] - verts[v];
                vec_e.Unitize();
                double sin_foldang_e = Vector3d.CrossProduct(normal_i, normal_j) * vec_e;
                if (sin_foldang_e >= 0)
                {
                    if (cos_foldang_e >= 1.0)
                    {
                        foldang_e = 0;
                    }
                    else if (cos_foldang_e <= -1.0)
                    {
                        foldang_e = Math.PI;
                    }
                    else
                    {
                        foldang_e = Math.Acos(cos_foldang_e);
                    }
                }
                else
                {
                    if (cos_foldang_e >= 1.0)
                    {
                        foldang_e = 0;
                    }
                    else if (cos_foldang_e <= -1.0)
                    {
                        foldang_e = -Math.PI;
                    }
                    else
                    {
                        foldang_e = -Math.Acos(cos_foldang_e);
                    }
                }
                foldang.Add(foldang_e);
            }
            return foldang;
        }
        public List<Char> GetInnerEdgeAssignment()
        {
            List<Char> inner_edge_info = new List<Char>();
            for (int i = 0; i < this.EdgeInfo.Count; i++)
            {
                if (EdgeInfo[i] != 'B')
                {
                    inner_edge_info.Add(EdgeInfo[i]);
                }
            }
            return inner_edge_info;
        }
        public List<Char> GetTrianglatedEdgeAssignment()
        {
            List<Char> trianglated_edge_info = new List<Char>();
            for (int i = 0; i < this.EdgeInfo.Count; i++)
            {
                if (EdgeInfo[i] == 'T')
                {
                    trianglated_edge_info.Add(EdgeInfo[i]);
                }
            }
            return trianglated_edge_info;
        }
        private void SetMeshFundamentalInfo()
        {
            InnerEdges = new List<IndexPair>();
            BoundaryEdges = new List<IndexPair>();
            InnerBoundaryEdges = new List<int>();
            FacePairs = new List<IndexPair>();
            TriangulatedEdges = new List<IndexPair>();
            TriangulatedFacePairs = new List<IndexPair>();
            MountainEdges = new List<IndexPair>();
            MountainFacePairs = new List<IndexPair>();
            ValleyEdges = new List<IndexPair>();
            ValleyFacePairs = new List<IndexPair>();
            List<int> inner_edges_id = new List<int>();
            List<int> boundary_edges_id = new List<int>();
            EdgeIds2InnerEdgeIds = new Dictionary<int, int>();

            int innerEdgeId = 0;

            for (int e = 0; e < this.Mesh.TopologyEdges.Count; e++)
            {
                if (this.Mesh.TopologyEdges.GetConnectedFaces(e).Count() > 1)
                {
                    EdgeIds2InnerEdgeIds[e] = innerEdgeId;
                    IndexPair inner_edge = new IndexPair();
                    inner_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    inner_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.InnerEdges.Add(inner_edge);
                    inner_edges_id.Add(e);

                    IndexPair face_pair = GetFacePair(inner_edge, e);
                    this.FacePairs.Add(face_pair);
                    innerEdgeId++;
                }
                if (this.Mesh.TopologyEdges.GetConnectedFaces(e).Count() == 1)
                {
                    IndexPair boundary_edge = new IndexPair();
                    boundary_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    boundary_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.BoundaryEdges.Add(boundary_edge);
                    boundary_edges_id.Add(e);
                }
                if (this.EdgeInfo[e] == 'T')
                {
                    IndexPair triangulated_edge = new IndexPair();
                    triangulated_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    triangulated_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.TriangulatedEdges.Add(triangulated_edge);

                    IndexPair triangulated_face_pair = GetFacePair(triangulated_edge, e);
                    this.TriangulatedFacePairs.Add(triangulated_face_pair);
                }
                if (this.EdgeInfo[e] == 'M')
                {
                    IndexPair mountain_edge = new IndexPair();
                    mountain_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    mountain_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.MountainEdges.Add(mountain_edge);

                    IndexPair mountain_face_pair = GetFacePair(mountain_edge, e);
                    this.MountainFacePairs.Add(mountain_face_pair);
                }
                if (this.EdgeInfo[e] == 'V')
                {
                    IndexPair valley_edge = new IndexPair();
                    valley_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    valley_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.ValleyEdges.Add(valley_edge);

                    IndexPair valley_face_pair = GetFacePair(valley_edge, e);
                    this.ValleyFacePairs.Add(valley_face_pair);
                }
            }
            this.InnerBoundaryEdges.AddRange(inner_edges_id);
            this.InnerBoundaryEdges.AddRange(boundary_edges_id);
        }
        public void SetFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.InnerEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.InnerEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.FacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.FaceHeightPairs = face_heignt_pairs;
            this.LengthOfDiagonalEdges = edge_length_between_face_pairs;
        }
        public void SetTriangulatedFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.TriangulatedEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.TriangulatedEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.TriangulatedFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.TriangulatedFaceHeightPairs = face_heignt_pairs;
            this.LengthOfTriangulatedDiagonalEdges = edge_length_between_face_pairs;
        }
        public void SetMountainFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.MountainEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.MountainEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.MountainFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.MountainFaceHeightPairs = face_heignt_pairs;
            this.LengthOfMountainDiagonalEdges = edge_length_between_face_pairs;
        }
        public void SetValleyFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.ValleyEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.ValleyEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.ValleyFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.ValleyFaceHeightPairs = face_heignt_pairs;
            this.LengthOfValleyDiagonalEdges = edge_length_between_face_pairs;
        }
        public void SetInnerVertexIds()
        {
            var isNaked = new List<bool>(Mesh.GetNakedEdgePointStatus());
            InnerVertexIds = new List<int>();
            for (int i = 0; i < isNaked.Count; i++)
            {
                if (!isNaked[i])
                {
                    InnerVertexIds.Add(i);
                }
            }
        }

        private void SetNakedLoopInfo()
        {
            var nakedPolylines = Mesh.GetNakedEdges();
            var nakedSegmentCounts = nakedPolylines.Select(p => p.SegmentCount).ToArray();
            int boundarySegmentCount = nakedSegmentCounts.Max();
            int boundaryId = 0;
            for (int i = 0; i < nakedSegmentCounts.Count(); i++)
            {
                if (nakedSegmentCounts[i] == boundarySegmentCount)
                {
                    boundaryId = i;
                    break;
                }
            }

            List<Polyline> holePolylines = new List<Polyline>();
            Polyline boundaryPolyline = nakedPolylines[boundaryId];
            for (int i = 0; i < nakedPolylines.Length; i++)
            {
                if (i != boundaryId)
                {
                    holePolylines.Add(nakedPolylines[i]);
                }
            }

            HoleLoopVertexIdsList = new List<List<int>>();
            BoundaryLoopVertexIds = new List<int>();

            foreach (var holePolyline in holePolylines)
            {
                var holeLoopVertexIds = new List<int>();
                foreach (var pt in holePolyline)
                {
                    holeLoopVertexIds.Add(VerticesCloud.ClosestPoint(pt));
                }

                int v0 = holeLoopVertexIds[0];
                int v1 = holeLoopVertexIds[1];
                var f0 = Mesh.TopologyVertices.ConnectedFaces(v0);
                var f1 = Mesh.TopologyVertices.ConnectedFaces(v1);
                var fId = f0.Intersect(f1).First();

                if (!Util.AlignFaceOrientation(Mesh.Faces[fId], v0, v1))
                {
                    holeLoopVertexIds.Reverse();
                }

                HoleLoopVertexIdsList.Add(holeLoopVertexIds);
            }

            foreach (var pt in boundaryPolyline)
            {
                BoundaryLoopVertexIds.Add(VerticesCloud.ClosestPoint(pt));
            }

            int bv0 = BoundaryLoopVertexIds[0];
            int bv1 = BoundaryLoopVertexIds[1];
            var bf0 = Mesh.TopologyVertices.ConnectedFaces(bv0);
            var bf1 = Mesh.TopologyVertices.ConnectedFaces(bv1);
            var bfId = bf0.Intersect(bf1).First();

            if (!Util.AlignFaceOrientation(Mesh.Faces[bfId], bv0, bv1))
            {
                BoundaryLoopVertexIds.Reverse();
            }
        }
        public int GetInnerVertexId(Point3d vert, double threshold)
        {
            int id = -1;
            foreach(int i in InnerVertexIds)
            {
                var pt = Mesh.Vertices[i];
                if (vert.DistanceTo(new Point3d(pt)) < threshold)
                    id = i;
            }
            return id;
        }

        public int GetVertexId(Point3d vert)
        {
            int id = VerticesCloud.ClosestPoint(vert);
            return id;
        }

        public int GetFaceIdFrom3Pts(Point3d pt1, Point3d pt2, Point3d pt3)
        {
            int id1 = GetVertexId(pt1);
            int id2 = GetVertexId(pt2);
            int id3 = GetVertexId(pt3);
            int[] fpt1 = Mesh.TopologyVertices.ConnectedFaces(id1);
            int[] fpt2 = Mesh.TopologyVertices.ConnectedFaces(id2);
            int[] fpt3 = Mesh.TopologyVertices.ConnectedFaces(id3);
            var fpt12 = fpt1.Intersect(fpt2);
            var fpt123 = fpt12.Intersect(fpt3);
            if (fpt123.Count() == 1) return fpt123.First();
            else throw new Exception("Face id Could not be found.");
        }

        public int GetFaceIdFrom3PtIds(int id1, int id2, int id3)
        {
            int[] fpt1 = Mesh.TopologyVertices.ConnectedFaces(id1);
            int[] fpt2 = Mesh.TopologyVertices.ConnectedFaces(id2);
            int[] fpt3 = Mesh.TopologyVertices.ConnectedFaces(id3);
            var fpt12 = fpt1.Intersect(fpt2);
            var fpt123 = fpt12.Intersect(fpt3);
            if (fpt123.Count() == 1) return fpt123.First();
            else throw new Exception("Face id Could not be found.");
        }

        public IndexPair GetOtherVertexIdPair(int faceId, int vertId)
        {
            var tri = Mesh.Faces[faceId];
            int order = -1;
            for(int i = 0; i < 3; i++)
            {
                if (tri[i] == vertId)
                {
                    order = i;
                }
            }

            if (order == 0)
            {
                return new IndexPair(tri[2], tri[1]);
            }
            else if (order == 1)
            {
                return new IndexPair(tri[0], tri[2]);
            }
            else if (order == 2)
            {
                return new IndexPair(tri[1], tri[0]);
            }
            else
            {
                throw new Exception("Given the face id and the vertex id are invalid.");
            }
        }

        public Vector3d[] ComputeDerivativeOfSectorAngle(int fId, int v1Id, int v2Id, int v3Id)
        {
            var pts = Mesh.Vertices.ToPoint3dArray();
            var v1 = pts[v1Id];
            var v2 = pts[v2Id];
            var v3 = pts[v3Id];
            var sec = Util.ComputeAngleFrom3Pts(v1, v2, v3);
            var n = ComputeFaceNormal(fId);
            var v12 = v1 - v2;
            var v32 = v3 - v2;
            var v12SDist = v12.SquareLength;
            var v32SDist = v32.SquareLength;
            var nv12 = Vector3d.CrossProduct(n, v12) / v12SDist;
            var nv32 = Vector3d.CrossProduct(n, v32) / v32SDist;
            var DSecDv1 = nv12;
            var DSecDv2 = -nv12 + nv32;
            var DSecDv3 = -nv32;
            return new Vector3d[] { DSecDv1, DSecDv2, DSecDv3 };
        }

        public Vector3d ComputeFaceNormal(int fId)
        {
            var f = Mesh.Faces[fId];
            var v1 = Vertices[f.A];
            var v2 = Vertices[f.B];
            var v3 = Vertices[f.C];
            var v12 = v2 - v1;
            var v13 = v3 - v1;
            var n = Vector3d.CrossProduct(v12, v13);
            n.Unitize();
            return n;
        }

        public Vector3d[] ComputeDerivativeOfEdgeLength(int eId)
        {
            var vPair = Mesh.TopologyEdges.GetTopologyVertices(eId);
            var v1Id = vPair.I;
            var v2Id = vPair.J;
            var v1 = Mesh.Vertices.Point3dAt(v1Id);
            var v2 = Mesh.Vertices.Point3dAt(v2Id);
            var e = v2 - v1;
            var elen = e.Length;
            return new Vector3d[] { -e / elen, e / elen };
        }
        public Tuple<List<Line>, List<Line>> GetAutomaticallyAssignedMVLines()
        {
            var verts = Mesh.Vertices.ToPoint3dArray();
            var foldang = GetFoldAngles();
            var m = new List<Line>();
            var v = new List<Line>();
            for (int i = 0; i < foldang.Count; i++)
            {
                var edge = InnerEdges[i];
                var ang = foldang[i];
                if (InnerEdgeAssignment[i] != 'T')
                {
                    if (ang < 0)
                    {
                        m.Add(new Line(verts[edge.I], verts[edge.J]));
                    }
                    else if (ang > 0)
                    {
                        v.Add(new Line(verts[edge.I], verts[edge.J]));
                    }
                }
            }
            return new Tuple<List<Line>, List<Line>>(m, v);
        }
        public List<Line> GetMountainEdgeLines()
        {
            List<Line> lines = new List<Line>();
            var varts = Mesh.Vertices.ToPoint3dArray();
            foreach (var pair in MountainEdges)
            {
                var ptI = varts[pair.I];
                var ptJ = varts[pair.J];
                lines.Add(new Line(ptI, ptJ));
            }

            return lines;
        }
        public List<Line> GetValleyEdgeLines()
        {
            List<Line> lines = new List<Line>();
            var varts = Mesh.Vertices.ToPoint3dArray();
            foreach (var pair in ValleyEdges)
            {
                var ptI = varts[pair.I];
                var ptJ = varts[pair.J];
                lines.Add(new Line(ptI, ptJ));
            }

            return lines;
        }

        public List<Line> GetInnerEdgeLines()
        {
            List<Line> lines = new List<Line>();
            var varts = Mesh.Vertices.ToPoint3dArray();
            foreach (var pair in InnerEdges)
            {
                var ptI = varts[pair.I];
                var ptJ = varts[pair.J];
                lines.Add(new Line(ptI, ptJ));
            }

            return lines;
        }
        public List<Line> GetTraiangulatedEdgeLines()
        {
            List<Line> lines = new List<Line>();
            var varts = Mesh.Vertices.ToPoint3dArray();
            foreach (var pair in TriangulatedEdges)
            {
                var ptI = varts[pair.I];
                var ptJ = varts[pair.J];
                lines.Add(new Line(ptI, ptJ));
            }

            return lines;
        }
        public Mesh FoldingMesh()
        {
            Mesh foldingMesh = new Mesh();
            var verts = Mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < NumberOfFoldingVertices; i++)
            {
                foldingMesh.Vertices.Add(verts[i]);
            }
            for (int i = 0; i < NumberOfFoldingFaces; i++)
            {
                var face = Mesh.Faces[i];
                if (face.IsTriangle) foldingMesh.Faces.AddFace(face.A, face.B, face.C);
                else foldingMesh.Faces.AddFace(face.A, face.B, face.C, face.D);
            }

            foldingMesh.FaceNormals.ComputeFaceNormals();
            foldingMesh.Normals.ComputeNormals();

            return foldingMesh;
        }
        public Mesh Development()
        {
            if (!HasDevelopment) return null;
            Mesh development = new Mesh();
            var verts = Mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < NumberOfDevelopmentVertices; i++)
            {
                development.Vertices.Add(verts[i + NumberOfFoldingVertices]);
            }

            for (int i = 0; i < NumberOfDevelopmentFaces; i++)
            {
                var face = Mesh.Faces[i];
                if (face.IsTriangle) development.Faces.AddFace(face.A, face.B, face.C);
                else development.Faces.AddFace(face.A, face.B, face.C, face.D);
            }

            development.FaceNormals.ComputeFaceNormals();
            development.Normals.ComputeNormals();
            return development;
        }

        public Mesh GetDevelopment(Point3d origin, double rotationAngle)
        {
            var dev = new DevelopMesh(Mesh, new Point2d(origin.X, origin.Y), rotationAngle);
            return dev.DevelopedMesh;
        }
        public List<Line> GetLines(Mesh mesh, List<IndexPair> edges)
        {
            var lines = new List<Line>();
            var verts = mesh.Vertices.ToPoint3dArray();
            foreach (var edge in edges)
            {
                lines.Add(new Line(verts[edge.I], verts[edge.J]));
            }
            return lines;
        }
        private Boolean SortFacePair(MeshFace f, IndexPair e)
        {
            IndexPair f1 = new IndexPair(f.A, f.B);
            IndexPair f2 = new IndexPair(f.B, f.C);
            IndexPair f3 = new IndexPair(f.C, f.A);
            Boolean Bool = false;
            Boolean b1 = (f1.I == e.I) & (f1.J == e.J);
            Boolean b2 = (f2.I == e.I) & (f2.J == e.J);
            Boolean b3 = (f3.I == e.I) & (f3.J == e.J);
            Bool = b1 | b2 | b3;

            return Bool;
        }
        private IndexPair GetFacePair(IndexPair edge_pair, int e)
        {
            int f_ind_0 = this.Mesh.TopologyEdges.GetConnectedFaces(e)[0];
            int f_ind_1 = this.Mesh.TopologyEdges.GetConnectedFaces(e)[1];
            MeshFace f = this.Mesh.Faces[f_ind_0];
            IndexPair face_pair;
            if (SortFacePair(f, edge_pair))
            {
                face_pair = new IndexPair(f_ind_0, f_ind_1);
            }
            else
            {
                face_pair = new IndexPair(f_ind_1, f_ind_0);
            }

            return face_pair;

        }
        private void SetConnectedTopologyVertices()
        {
            Mesh.TopologyVertices.SortEdges();
            ConnectedTopologyVerticesList = new List<List<int>>();
            ConnectedTopologyEdgesList = new List<List<int>>();
            for (int i = 0; i < Mesh.Vertices.Count; i++)
            {
                var conVerts = Mesh.TopologyVertices.ConnectedTopologyVertices(i);
                 ConnectedTopologyVerticesList.Add(conVerts.ToList());

                var connectedTopologyEdges = new List<int>();
                foreach(int j in conVerts)
                {
                    int edgeId = Util.GetTopologyEdgeIndex(Mesh, new IndexPair(i, j));
                    connectedTopologyEdges.Add(edgeId);
                }

                ConnectedTopologyEdgesList.Add(connectedTopologyEdges);
            }
        }
        public int GetInnerEdgeIndex(Line line)
        {
            int from = VerticesCloud.ClosestPoint(line.From);
            int to = VerticesCloud.ClosestPoint(line.To);
            int id = Mesh.TopologyEdges.GetEdgeIndex(from, to);
            if (EdgeIds2InnerEdgeIds.ContainsKey(id))
            {
                return EdgeIds2InnerEdgeIds[id];
            }
            else
            {
                return -1;
            }
        }
        public int GetEdgeIndex(Line line, double threshold)
        {
            int id = -1;
            for(int i = 0; i < Mesh.TopologyEdges.Count; i++)
            {
                var pair = Mesh.TopologyEdges.GetTopologyVertices(i);
                var pt1 = Mesh.Vertices[pair.I];
                var pt2 = Mesh.Vertices[pair.J];
                if (Util.PointsMatchLineEnds(line, pt1, pt2, threshold)) id = i;
            }
            return id;
        }

        public int GetEdgeIndex(Line line)
        {
            int id = -1;
            int from = VerticesCloud.ClosestPoint(line.From);
            int to = VerticesCloud.ClosestPoint(line.To);
            id = Mesh.TopologyEdges.GetEdgeIndex(from, to);
            return id;
        }
        public void UpdateMesh(Vector<double> ptsVector)
        {
            MeshVerticesVector = ptsVector;
            UpdateMesh();
        }
        public void UpdateMesh()
        {
            Mesh.Vertices.Destroy();
            this.Mesh.Vertices.UseDoublePrecisionVertices = true;
            if (IsPeriodic)
            {
                for (int i = 0; i < NumberOfVertices; i++)
                {
                    Mesh.Vertices.Add(MeshVerticesVector[3 * i], MeshVerticesVector[3 * i + 1],
                        MeshVerticesVector[3 * i + 2]);
                    //this.Mesh.Vertices.SetVertex(i, MeshVerticesVector[3 * i], MeshVerticesVector[3 * i + 1],
                    //    MeshVerticesVector[3 * i + 2]);
                    Vertices[i] = new Point3d(MeshVerticesVector[3 * i], MeshVerticesVector[3 * i + 1], MeshVerticesVector[3 * i + 2]);
                }
                for (int i = 0; i < 2; i++)
                {
                    this.CylindricallyRotationAngles[i] = this.ConfigulationVector[DOF + i];
                    this.CylindricallyTranslationCoefficients[i] = this.ConfigulationVector[DOF + i + 2];
                }
            }
            else
            {
                for (int i = 0; i < NumberOfVertices; i++)
                {
                    Mesh.Vertices.Add(MeshVerticesVector[3 * i], MeshVerticesVector[3 * i + 1],
                        MeshVerticesVector[3 * i + 2]);
                    //this.Mesh.Vertices.SetVertex(i, MeshVerticesVector[3 * i], MeshVerticesVector[3 * i + 1],
                    //    MeshVerticesVector[3 * i + 2]);
                    Vertices[i] = new Point3d(MeshVerticesVector[3 * i], MeshVerticesVector[3 * i + 1], MeshVerticesVector[3 * i + 2]);
                }
            }
            UpdateFaceNormals();
            this.Mesh.FaceNormals.ComputeFaceNormals(); 
            this.Mesh.Normals.ComputeNormals();
        }
        public void UpdateMesh(Point3d[] verts)
        {
            Mesh.Vertices.Destroy();
            Mesh.Vertices.UseDoublePrecisionVertices = true;
            for (int i = 0; i < NumberOfVertices; i++)
            {
                Mesh.Vertices.Add(verts[i]);
                //Mesh.Vertices.SetVertex(i,verts[i]);
            }   
            Vertices = verts;
            Mesh.FaceNormals.ComputeFaceNormals();
            Mesh.Normals.ComputeNormals();
        }
        public void UpdateEdgeLengthSquared()
        {
            this.EdgeLengthSquared = new List<double>();
            for(int i = 0; i < this.Mesh.TopologyEdges.Count; i++)
            {
                IndexPair indexPair = this.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d from = Vertices[indexPair.I];
                Point3d to = Vertices[indexPair.J];
                this.EdgeLengthSquared.Add(from.DistanceToSquared(to));
            }

            AverageEdgeLength = EdgeLengthSquared.Select(e => Math.Sqrt(e)).Average();
        }
        public void UpdateFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.InnerEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.InnerEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.FacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.FaceHeightPairs = face_heignt_pairs;
            this.LengthOfDiagonalEdges = edge_length_between_face_pairs;
        }
        private void UpdateTriangulatedFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.TriangulatedEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.TriangulatedEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.TriangulatedFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.TriangulatedFaceHeightPairs = face_heignt_pairs;
            this.LengthOfTriangulatedDiagonalEdges = edge_length_between_face_pairs;
        }
        private void UpdateMountainFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.MountainEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.MountainEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.MountainFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.MountainFaceHeightPairs = face_heignt_pairs;
            this.LengthOfMountainDiagonalEdges = edge_length_between_face_pairs;
        }
        private void UpdateValleyFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.ValleyEdges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.ValleyEdges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.ValleyFacePairs[e_ind];
                int P = face_ind.I;
                int Q = face_ind.J;

                MeshFace face_P = m.Faces[P];
                MeshFace face_Q = m.Faces[Q];
                int p = 0;
                int q = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!edge_ind.Contains(face_P[i]))
                    {
                        p = face_P[i];
                    }
                    if (!edge_ind.Contains(face_Q[i]))
                    {
                        q = face_Q[i];
                    }
                }
                /// Compute h_P & cot_Pu
                Vector3d vec_up = vert[p] - vert[u];
                Vector3d vec_uv = vert[v] - vert[u];
                double sin_Pu = (Vector3d.CrossProduct(vec_up, vec_uv) / (vec_up.Length * vec_uv.Length)).Length;
                double len_up = (vec_up - vec_uv).Length;
                double h_P = len_up * sin_Pu;
                /// Compute h_Q & cot_Qu
                Vector3d vec_uq = vert[q] - vert[u];
                double sin_Qu = (Vector3d.CrossProduct(vec_uq, vec_uv) / (vec_uq.Length * vec_uv.Length)).Length;
                double len_uq = (vec_uq - vec_uv).Length;
                double h_Q = len_uq * sin_Qu;
                // Compute len_uv
                double len_uv = vec_uv.Length;
                // Set Tuple<h_P, h_Q>
                Tuple<double, double> face_height_pair = new Tuple<double, double>(h_P, h_Q);
                face_heignt_pairs.Add(face_height_pair);
                edge_length_between_face_pairs.Add(len_uv);
            }
            this.ValleyFaceHeightPairs = face_heignt_pairs;
            this.LengthOfValleyDiagonalEdges = edge_length_between_face_pairs;
        }

        private void UpdateFaceNormals()
        {
            FaceNormals = new Vector3d[Mesh.Faces.Count];
            for (int i = 0; i < Mesh.Faces.Count; i++)
            {
                MeshFace f = Mesh.Faces[i];
                Point3d p1 = Mesh.Vertices.Point3dAt(f.A);
                Point3d p2 = Mesh.Vertices.Point3dAt(f.B);
                Point3d p3 = Mesh.Vertices.Point3dAt(f.C);
                Vector3d v12 = p2 - p1;
                Vector3d v13 = p3 - p1;
                Vector3d n = Vector3d.CrossProduct(v12, v13);
                n.Unitize();
                FaceNormals[i] = n;
            }
        }
        public void UpdateVerticesCloud()
        {
            VerticesCloud = new PointCloud(Mesh.Vertices.ToPoint3dArray());
            Vertices = Mesh.Vertices.ToPoint3dArray();
        }
        public void SetFoldingSpeed(double[] foldingSpeed)
        {
            FoldingSpeed = foldingSpeed.ToArray();
        }
        private void SetNgon()
        {
            Dictionary<int, int> faceId2NgonId = new Dictionary<int, int>();

            int nowNgonCount = 0;
            foreach (var tri in TriangulatedFacePairs)
            {
                int i = tri.I;
                int j = tri.J;
                if (faceId2NgonId.ContainsKey(i))
                {
                    faceId2NgonId[j] = faceId2NgonId[i];
                }
                else
                {
                    if (faceId2NgonId.ContainsKey(j))
                    {
                        faceId2NgonId[i] = faceId2NgonId[j];
                    }
                    else
                    {
                        faceId2NgonId[i] = faceId2NgonId[j] = nowNgonCount;
                        nowNgonCount++;
                    }
                }
            }

            for (int i = 0; i < Mesh.Faces.Count; i++)
            {
                if (!faceId2NgonId.ContainsKey(i))
                {
                    faceId2NgonId[i] = nowNgonCount;
                    nowNgonCount++;
                }
            }

            List<int>[] faceIdLists = new List<int>[nowNgonCount];
            List<int>[] vertexIdLists = new List<int>[nowNgonCount];
            for (int i = 0; i < nowNgonCount; i++)
            {
                faceIdLists[i] = new List<int>();
                vertexIdLists[i] = new List<int>();
            }
            for (int i = 0; i < Mesh.Faces.Count; i++)
            {
                var ngonId = faceId2NgonId[i];
                faceIdLists[ngonId].Add(i);
            }

            for (int i = 0; i < nowNgonCount; i++)
            {
                var faceIdList = faceIdLists[i];
                foreach (var fId in faceIdList)
                {
                    MeshFace face = Mesh.Faces[fId];
                    int n = face.IsTriangle ? 3 : 4;
                    for (int j = 0; j < n; j++)
                    {
                        if (!vertexIdLists[i].Contains(face[j]))
                        {
                            vertexIdLists[i].Add(face[j]);
                        }
                    }
                }
            }
            Mesh.Ngons.Clear();
            for (int i = 0; i < nowNgonCount; i++)
            {
                Mesh.Ngons.AddNgon(MeshNgon.Create(vertexIdLists[i], faceIdLists[i]));
            }

        }
        public void ComputeInitialProperties()
        {
            SetMeshVerticesVector();
            UpdateEdgeLengthSquared();
            UpdateFacePairBasicInfo();
            UpdateTriangulatedFacePairBasicInfo();
            UpdateMountainFacePairBasicInfo();
            UpdateValleyFacePairBasicInfo();
            SetWholeScale();
        }
        public void UpdateProperties()
        {
            UpdateMesh();
            SetWholeScale();
        }
        public void SetWholeScale()
        {
            BoundingBox box = new BoundingBox(this.Mesh.Vertices.ToPoint3dArray());
            this.WholeScale = box.Max.DistanceTo(box.Min);
        }
        public void SetMeshVerticesVector()
        {
            MeshVerticesVector = DenseVector.Build.Dense(DOF);
            for(int i = 0; i < NumberOfVertices; i++)
            {
                MeshVerticesVector[3 * i + 0] = Mesh.Vertices[i].X;
                MeshVerticesVector[3 * i + 1] = Mesh.Vertices[i].Y;
                MeshVerticesVector[3 * i + 2] = Mesh.Vertices[i].Z;
            }
        }
        public void SetConfigulationVector()
        {
            ConfigulationVector = DenseVector.Build.Dense(DOF + 4);
            for(int i = 0; i < NumberOfVertices; i++)
            {
                ConfigulationVector[3 * i + 0] = Mesh.Vertices[i].X;
                ConfigulationVector[3 * i + 1] = Mesh.Vertices[i].Y;
                ConfigulationVector[3 * i + 2] = Mesh.Vertices[i].Z;
            }
            for(int i = 0; i < 2; i++)
            {
                ConfigulationVector[DOF + 0] = 0.1;
                ConfigulationVector[DOF + 1] = 0.1;
                ConfigulationVector[DOF + 2] = 0.1;
                ConfigulationVector[DOF + 3] = 0.1;
            }
        }
        public void SetPeriodicParameters()
        {
            this.CylindricallyRotationAngles = new double[2] {0.1, 0.1 };
            this.CylindricallyTranslationCoefficients = new double[2] { 0.1, 0.1 };
        }
        public void ResetMesh()
        {
            Mesh = InitialMesh.DuplicateMesh();
            SetMeshVerticesVector();
            UpdateEdgeLengthSquared();
            SetWholeScale();
        }


        public string ToFoldFormat(bool mVFullFold)
        {
            var foldFormat = new FoldFormat(this, mVFullFold);
            string ff = JsonSerializer.Serialize(foldFormat, new JsonSerializerOptions{WriteIndented = true});

            return ff;
        }
    }
}
