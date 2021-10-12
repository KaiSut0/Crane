using System;
using System.Collections.Generic;
using System.Linq;
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
        public Mesh Mesh { get; set; }
        public Mesh InitialMesh { get; set; }
        public Vector<double> MeshVerticesVector { get; set; }
        public Vector<double> ConfigulationVector { get; set; }
        public List<double> EdgeLengthSquared { get; set; }
        public double[] CylindricallyRotationAngles { get; set; }
        public double[] CylindricallyTranslationCoefficients { get; set; }
        public Vector3d CylinderAxis;
        public Point3d CylinderOrigin;
        public int DOF { get; set; }
        public int NumberOfVertices { get; set; }
        public int NumberOfEdges { get; set; }
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
        #endregion

        #region Private Member
        public List<Char> edgeInfo;
        public MeshFaceList orig_faces;
        public List<Char> inner_edge_assignment;
        public List<double> foldang;

        public List<int> innerVertexIds;

        public List<IndexPair> inner_edges;
        public List<IndexPair> boundary_edges;
        public List<int> inner_boundary_edges;
        public List<IndexPair> triangulated_edges;
        public List<IndexPair> mountain_edges;
        public List<IndexPair> valley_edges;

        public List<IndexPair> face_pairs;
        public List<IndexPair> triangulated_face_pairs;
        public List<IndexPair> mountain_face_pairs;
        public List<IndexPair> valley_face_pairs;

        public List<Tuple<double, double>> face_height_pairs;
        public List<Tuple<double, double>> triangulated_face_height_pairs;
        public List<Tuple<double, double>> mountain_face_height_pairs;
        public List<Tuple<double, double>> valley_face_height_pairs;

        public List<double> length_of_diagonal_edges;
        public List<double> length_of_triangulated_diagonal_edges;
        public List<double> length_of_mountain_diagonal_edges;
        public List<double> length_of_valley_diagonal_edges;
        public List<double> Initial_edges_length;


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

            this.edgeInfo = new List<char>(cMesh.edgeInfo.ToArray());
            this.orig_faces = this.InitialMesh.Faces;
            this.inner_edge_assignment = new List<char>(cMesh.inner_edge_assignment.ToArray());
            this.foldang = new List<double>(cMesh.foldang);

            this.innerVertexIds = new List<int>(cMesh.innerVertexIds.ToArray());

            this.inner_edges = Utils.CloneIndexPairs(cMesh.inner_edges).ToList();
            this.boundary_edges = Utils.CloneIndexPairs(cMesh.boundary_edges).ToList();
            this.inner_boundary_edges = new List<int>(cMesh.inner_boundary_edges.ToArray());
            this.triangulated_edges = Utils.CloneIndexPairs(cMesh.triangulated_edges).ToList();
            this.mountain_edges = Utils.CloneIndexPairs(cMesh.mountain_edges).ToList();
            this.valley_edges = Utils.CloneIndexPairs(cMesh.valley_edges).ToList();

            this.face_pairs = Utils.CloneIndexPairs(cMesh.face_pairs).ToList();
            this.triangulated_face_pairs = Utils.CloneIndexPairs(cMesh.triangulated_face_pairs).ToList();
            this.mountain_face_pairs = Utils.CloneIndexPairs(cMesh.mountain_face_pairs).ToList();
            this.valley_face_pairs = Utils.CloneIndexPairs(cMesh.valley_face_pairs).ToList();

            this.face_height_pairs = Utils.CloneTuples(cMesh.face_height_pairs).ToList();
            this.triangulated_face_height_pairs = Utils.CloneTuples(cMesh.triangulated_face_height_pairs).ToList();
            this.mountain_face_height_pairs = Utils.CloneTuples(cMesh.mountain_face_height_pairs).ToList();
            this.valley_face_height_pairs = Utils.CloneTuples(cMesh.valley_face_height_pairs).ToList();
            
            this.length_of_diagonal_edges = new List<double>(cMesh.length_of_diagonal_edges.ToArray());
            this.length_of_triangulated_diagonal_edges = new List<double>(cMesh.length_of_triangulated_diagonal_edges.ToArray());
            this.length_of_mountain_diagonal_edges = new List<double>(cMesh.length_of_mountain_diagonal_edges.ToArray());
            this.length_of_valley_diagonal_edges = new List<double>(cMesh.length_of_valley_diagonal_edges.ToArray());

            this.ConnectedTopologyVerticesList = cMesh.ConnectedTopologyVerticesList;
            this.ConnectedTopologyEdgesList = cMesh.ConnectedTopologyEdgesList;
            //this.Initial_edges_length = new List<double>(cMesh.Initial_edges_length.ToArray());

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

        private void SetCMeshFromMVT(Mesh mesh, List<Line> M, List<Line> V, List<Line> T)
        {
            this.Mesh = mesh.DuplicateMesh();
            this.InitialMesh = mesh.DuplicateMesh();
            this.DOF = mesh.Vertices.Count * 3;
            this.NumberOfVertices = mesh.Vertices.Count;
            this.NumberOfEdges = mesh.TopologyEdges.Count;
            this.Mesh.FaceNormals.ComputeFaceNormals();
            this.orig_faces = mesh.Faces;
            var tri = this.InsertTriangulate();

            //this.Mesh.TopologyVertices.SortEdges();
            SetConnectedTopologyVertices();
            var edges = this.Mesh.TopologyEdges;
            var verts = this.Mesh.TopologyVertices;

            SetInnerVertexIds();

            this.edgeInfo = new List<Char>();
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges.IsNgonInterior(i))
                {
                    edgeInfo.Add('T');
                }
                else
                {
                    edgeInfo.Add('U');
                }
            }

            foreach (Line m in M)
            {
                Point3d a = m.From;
                Point3d b = m.To;

                int found = 0;
                int vertsCount = verts.Count;
                int j = 0;
                int fromIndex = -1;
                int toIndex = -1;
                while (found != 2 && j < vertsCount)
                {
                    Point3d p = verts[j];

                    if (p.DistanceTo(a) < 0.001)
                    {
                        fromIndex = j;
                        found++;
                    }
                    else if (p.DistanceTo(b) < 0.001)
                    {
                        toIndex = j;
                        found++;
                    }
                    j++;
                }
                int ind = edges.GetEdgeIndex(fromIndex, toIndex);
                if (ind != -1)
                {
                    edgeInfo[ind] = 'M';
                }
            }
            foreach (Line v in V)
            {
                Point3d a = v.From;
                Point3d b = v.To;

                int found = 0;
                int vertsCount = verts.Count;
                int j = 0;
                int fromIndex = -1;
                int toIndex = -1;
                while (found != 2 && j < vertsCount)
                {
                    Point3d p = verts[j];

                    if (p.DistanceTo(a) < 0.001)
                    {
                        fromIndex = j;
                        found++;
                    }
                    else if (p.DistanceTo(b) < 0.001)
                    {
                        toIndex = j;
                        found++;
                    }
                    j++;
                }
                int ind = edges.GetEdgeIndex(fromIndex, toIndex);
                if (ind != -1)
                {
                    edgeInfo[ind] = 'V';
                }
            }
            if (tri.Count != 0)
            {
                foreach (List<int> v in tri)
                {
                    int ind = edges.GetEdgeIndex(v[0], v[1]);
                    if (ind != -1)
                    {
                        this.edgeInfo[ind] = 'T';
                    }
                }
            }

            foreach (Line t in T)
            {
                Point3d a = t.From;
                Point3d b = t.To;

                int found = 0;
                int vertsCount = verts.Count;
                int j = 0;
                int fromIndex = -1;
                int toIndex = -1;
                while (found != 2 && j < vertsCount)
                {
                    Point3d p = verts[j];

                    if (p.DistanceTo(a) < 0.001)
                    {
                        fromIndex = j;
                        found++;
                    }
                    else if (p.DistanceTo(b) < 0.001)
                    {
                        toIndex = j;
                        found++;
                    }
                    j++;
                }
                int ind = edges.GetEdgeIndex(fromIndex, toIndex);
                if (ind != -1)
                {
                    edgeInfo[ind] = 'T';
                }
            }

            List<bool> naked = new List<bool>(mesh.GetNakedEdgePointStatus());

            for (int i = 0; i < this.Mesh.TopologyEdges.Count; i++)
            {
                if (edges.GetConnectedFaces(i).Count() == 1)
                {

                    this.edgeInfo[i] = 'B';
                }
            }
            SetMeshFundamentalInfo();
            SetTriangulatedFacePairBasicInfo();
            SetMountainFacePairBasicInfo();
            SetValleyFacePairBasicInfo();
            this.inner_edge_assignment = GetInnerEdgeAssignment();
            this.SetMeshVerticesVector();
            this.SetConfigulationVector();
            this.SetPeriodicParameters();
            ComputeInitialProperties();
            HasDevelopment = false;
        }

        private void SetCMeshFromMVTWithDevelopment(Mesh mesh, List<Line> M, List<Line> V, List<Line> T, Point2d developmentOrigin, double developmentRotation = 0)
        {
            CMesh cMesh = new CMesh(mesh, M, V, T);
            var dev = new DevelopMesh(cMesh.Mesh, developmentOrigin, developmentRotation);
            Mesh joinedMesh = Utils.JoinMesh(dev.Mesh, dev.DevelopedMesh);
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
            var edges = this.inner_edges;
            var verts = this.Mesh.Vertices;
            var faces = this.face_pairs;

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
            for (int i = 0; i < this.edgeInfo.Count; i++)
            {
                if (edgeInfo[i] != 'B')
                {
                    inner_edge_info.Add(edgeInfo[i]);
                }
            }
            return inner_edge_info;
        }
        public List<Char> GetTrianglatedEdgeAssignment()
        {
            List<Char> trianglated_edge_info = new List<Char>();
            for (int i = 0; i < this.edgeInfo.Count; i++)
            {
                if (edgeInfo[i] == 'T')
                {
                    trianglated_edge_info.Add(edgeInfo[i]);
                }
            }
            return trianglated_edge_info;
        }
        public void SetMeshFundamentalInfo()
        {
            this.inner_edges = new List<IndexPair>();
            this.boundary_edges = new List<IndexPair>();
            this.inner_boundary_edges = new List<int>();
            this.face_pairs = new List<IndexPair>();
            this.foldang = new List<double>();
            this.triangulated_edges = new List<IndexPair>();
            this.triangulated_face_pairs = new List<IndexPair>();
            this.mountain_edges = new List<IndexPair>();
            this.mountain_face_pairs = new List<IndexPair>();
            this.valley_edges = new List<IndexPair>();
            this.valley_face_pairs = new List<IndexPair>();
            List<int> inner_edges_id = new List<int>();
            List<int> boundary_edges_id = new List<int>();

            for (int e = 0; e < this.Mesh.TopologyEdges.Count; e++)
            {
                if (this.Mesh.TopologyEdges.GetConnectedFaces(e).Count() > 1)
                {
                    IndexPair inner_edge = new IndexPair();
                    inner_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    inner_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.inner_edges.Add(inner_edge);
                    inner_edges_id.Add(e);

                    IndexPair face_pair = GetFacePair(inner_edge, e);
                    this.face_pairs.Add(face_pair);
                }
                if (this.Mesh.TopologyEdges.GetConnectedFaces(e).Count() == 1)
                {
                    IndexPair boundary_edge = new IndexPair();
                    boundary_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    boundary_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.boundary_edges.Add(boundary_edge);
                    boundary_edges_id.Add(e);
                }
                if (this.edgeInfo[e] == 'T')
                {
                    IndexPair triangulated_edge = new IndexPair();
                    triangulated_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    triangulated_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.triangulated_edges.Add(triangulated_edge);

                    IndexPair triangulated_face_pair = GetFacePair(triangulated_edge, e);
                    this.triangulated_face_pairs.Add(triangulated_face_pair);
                }
                if (this.edgeInfo[e] == 'M')
                {
                    IndexPair mountain_edge = new IndexPair();
                    mountain_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    mountain_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.mountain_edges.Add(mountain_edge);

                    IndexPair mountain_face_pair = GetFacePair(mountain_edge, e);
                    this.mountain_face_pairs.Add(mountain_face_pair);
                }
                if (this.edgeInfo[e] == 'V')
                {
                    IndexPair valley_edge = new IndexPair();
                    valley_edge.I = this.Mesh.TopologyEdges.GetTopologyVertices(e).I;
                    valley_edge.J = this.Mesh.TopologyEdges.GetTopologyVertices(e).J;
                    this.valley_edges.Add(valley_edge);

                    IndexPair valley_face_pair = GetFacePair(valley_edge, e);
                    this.valley_face_pairs.Add(valley_face_pair);
                }
            }
            this.inner_boundary_edges.AddRange(inner_edges_id);
            this.inner_boundary_edges.AddRange(boundary_edges_id);
            this.foldang = GetFoldAngles();
        }
        public void SetFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.inner_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.inner_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.face_pairs[e_ind];
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
            this.face_height_pairs = face_heignt_pairs;
            this.length_of_diagonal_edges = edge_length_between_face_pairs;
        }
        public void SetTriangulatedFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.triangulated_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.triangulated_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.triangulated_face_pairs[e_ind];
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
            this.triangulated_face_height_pairs = face_heignt_pairs;
            this.length_of_triangulated_diagonal_edges = edge_length_between_face_pairs;
        }
        public void SetMountainFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.mountain_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.mountain_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.mountain_face_pairs[e_ind];
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
            this.mountain_face_height_pairs = face_heignt_pairs;
            this.length_of_mountain_diagonal_edges = edge_length_between_face_pairs;
        }
        public void SetValleyFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.valley_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.valley_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.valley_face_pairs[e_ind];
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
            this.valley_face_height_pairs = face_heignt_pairs;
            this.length_of_valley_diagonal_edges = edge_length_between_face_pairs;
        }
        public void SetInnerVertexIds()
        {
            var isNaked = new List<bool>(Mesh.GetNakedEdgePointStatus());
            innerVertexIds = new List<int>();
            for (int i = 0; i < isNaked.Count; i++)
            {
                if (!isNaked[i])
                {
                    innerVertexIds.Add(i);
                }
            }
        }
        public int GetInnerVertexId(Point3d vert, double threshold)
        {
            int id = -1;
            foreach(int i in innerVertexIds)
            {
                var pt = Mesh.Vertices[i];
                if (vert.DistanceTo(new Point3d(pt)) < threshold)
                    id = i;
            }
            return id;
        }

        public Tuple<List<Line>, List<Line>> GetAutomaticallyAssignedMVLines()
        {
            var verts = Mesh.Vertices.ToPoint3dArray();
            var foldang = GetFoldAngles();
            var m = new List<Line>();
            var v = new List<Line>();
            for (int i = 0; i < foldang.Count; i++)
            {
                var edge = inner_edges[i];
                var ang = foldang[i];
                if (inner_edge_assignment[i] != 'T')
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
            foreach (var pair in mountain_edges)
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
            foreach (var pair in valley_edges)
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
            foreach (var pair in triangulated_edges)
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
                var connectedTopologyVertices = new List<int>();
                connectedTopologyVertices.AddRange(Mesh.TopologyVertices.ConnectedTopologyVertices(i));
                ConnectedTopologyVerticesList.Add(connectedTopologyVertices);

                var connectedTopologyEdges = new List<int>();
                foreach(int j in connectedTopologyVertices)
                {
                    int edgeId = Utils.GetTopologyEdgeIndex(Mesh, new IndexPair(i, j));
                    connectedTopologyEdges.Add(edgeId);
                }

                ConnectedTopologyEdgesList.Add(connectedTopologyEdges);
            }
        }
        public int GetInnerEdgeIndex(Line line, double threshold)
        {
            int id = -1;
            for(int i = 0; i < inner_edges.Count; i++)
            {
                var pair = inner_edges[i];
                var pt1 = Mesh.Vertices[pair.I];
                var pt2 = Mesh.Vertices[pair.J];
                if (Utils.PointsMatchLineEnds(line, pt1, pt2, threshold)) id = i;
            }
            return id;
        }
        public int GetEdgeIndex(Line line, double threshold)
        {
            int id = -1;
            for(int i = 0; i < Mesh.TopologyEdges.Count; i++)
            {
                var pair = Mesh.TopologyEdges.GetTopologyVertices(i);
                var pt1 = Mesh.Vertices[pair.I];
                var pt2 = Mesh.Vertices[pair.J];
                if (Utils.PointsMatchLineEnds(line, pt1, pt2, threshold)) id = i;
            }
            return id;
        }

        public void UpdateMesh(Vector<double> ptsVector)
        {
            MeshVerticesVector = ptsVector;
            UpdateMesh();
        }
        public void UpdateMesh()
        {
            this.Mesh.Vertices.Destroy();
            if (IsPeriodic)
            {
                for (int i = 0; i < NumberOfVertices; i++)
                {
                    this.Mesh.Vertices.Add(this.ConfigulationVector[3 * i], this.ConfigulationVector[3 * i + 1], this.ConfigulationVector[3 * i + 2]);
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
                    this.Mesh.Vertices.Add(this.MeshVerticesVector[3 * i], this.MeshVerticesVector[3 * i + 1], this.MeshVerticesVector[3 * i + 2]);
                }
            }
            this.Mesh.Compact();
            this.Mesh.FaceNormals.ComputeFaceNormals();
            this.Mesh.Normals.ComputeNormals();
        }
        public void UpdateMesh(Point3d[] verts)
        {
            this.Mesh.Vertices.Destroy();
            for (int i = 0; i < NumberOfVertices; i++)
            {
                this.Mesh.Vertices.Add(verts[i]);
            }

            Mesh.Compact();
            Mesh.FaceNormals.ComputeFaceNormals();
            Mesh.Normals.ComputeNormals();
        }
        public void UpdateEdgeLengthSquared()
        {
            this.EdgeLengthSquared = new List<double>();
            for(int i = 0; i < this.Mesh.TopologyEdges.Count; i++)
            {
                IndexPair indexPair = this.Mesh.TopologyEdges.GetTopologyVertices(i);
                Point3d from = this.Mesh.Vertices[indexPair.I];
                Point3d to = this.Mesh.Vertices[indexPair.J];
                this.EdgeLengthSquared.Add(from.DistanceToSquared(to));
            }
        }
        public void UpdateFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.inner_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.inner_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.face_pairs[e_ind];
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
            this.face_height_pairs = face_heignt_pairs;
            this.length_of_diagonal_edges = edge_length_between_face_pairs;
        }
        private void UpdateTriangulatedFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.triangulated_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.triangulated_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.triangulated_face_pairs[e_ind];
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
            this.triangulated_face_height_pairs = face_heignt_pairs;
            this.length_of_triangulated_diagonal_edges = edge_length_between_face_pairs;
        }
        private void UpdateMountainFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.mountain_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.mountain_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.mountain_face_pairs[e_ind];
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
            this.mountain_face_height_pairs = face_heignt_pairs;
            this.length_of_mountain_diagonal_edges = edge_length_between_face_pairs;
        }
        private void UpdateValleyFacePairBasicInfo()
        {
            Mesh m = this.Mesh;

            List<Tuple<double, double>> face_heignt_pairs = new List<Tuple<double, double>>();
            List<double> edge_length_between_face_pairs = new List<double>();

            m.FaceNormals.ComputeFaceNormals();

            MeshVertexList vert = m.Vertices;

            for (int e_ind = 0; e_ind < this.valley_edges.Count; e_ind++)
            {
                // Register indices
                IndexPair edge_ind = this.valley_edges[e_ind];
                int u = edge_ind.I;
                int v = edge_ind.J;
                IndexPair face_ind = this.valley_face_pairs[e_ind];
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
            this.valley_face_height_pairs = face_heignt_pairs;
            this.length_of_valley_diagonal_edges = edge_length_between_face_pairs;
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
    }
}
