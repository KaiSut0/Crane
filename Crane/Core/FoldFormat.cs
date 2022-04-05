using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rhino;
using Rhino.Geometry;

namespace Crane.Core
{
    public class FoldFormat
    {
        [JsonPropertyName("file_classes")]
        public string[] FileClasses { get; set; }
        [JsonPropertyName("file_creator")]
        public string FileCreator { get; set; }
        [JsonPropertyName("file_spec")]
        public double FileSpec { get; set; }
        [JsonPropertyName("frame_classes")]
        public string[] FrameClasses { get; set; }
        [JsonPropertyName("frame_unit")]
        public string FrameUnit { get; set; }
        [JsonPropertyName("frame_title")]
        public string FrameTitle { get; set; }
        [JsonPropertyName("frame_attributes")]
        public string[] FrameAttributes { get; set; }
        [JsonPropertyName("vertices_coords")]
        public double[][] VerticesCoords { get; set; }
        [JsonPropertyName("edges_assignment")]
        public string[] EdgesAssignment { get; set; } 
        [JsonPropertyName("edges_foldAngle")]
        public double?[] EdgesFoldAngle { get; set; }
        [JsonPropertyName("edges_vertices")]
        public int[][] EdgesVertices { get; set; }
        [JsonPropertyName("faces_vertices")]
        public int[][] FacesVertices { get; set; }

        public FoldFormat() { }
        public FoldFormat(CMesh cMesh, bool mVFullFold)
        {
            FileClasses = new string[] { "singleModel" };
            FileCreator = "Crane";
            FileSpec = 1.1;
            FrameClasses = new string[] { "foldedForm" };
            FrameUnit = ParseRhinoModelUnitSystem(RhinoDoc.ActiveDoc.ModelUnitSystem);
            VerticesCoords = new double[cMesh.Mesh.Vertices.Count][];
            for (int i = 0; i < cMesh.Mesh.Vertices.Count; i++)
            {
                var pt = cMesh.Mesh.Vertices.Point3dAt(i);
                double[] coord = new double[3];
                for (int j = 0; j < 3; j++)
                {
                    coord[j] = pt[j];
                }

                VerticesCoords[i] = coord;
            }

            EdgesVertices = new int[cMesh.Mesh.TopologyEdges.Count][];
            for (int i = 0; i < cMesh.Mesh.TopologyEdges.Count; i++)
            {
                var e = cMesh.Mesh.TopologyEdges.GetTopologyVertices(i);
                EdgesVertices[i] = new int[] { e.I, e.J };
            }

            EdgesAssignment = new string[cMesh.EdgeInfo.Count];
            for (int i = 0; i < cMesh.EdgeInfo.Count; i++)
            {
                if (cMesh.EdgeInfo[i] == 'T')
                {
                    EdgesAssignment[i] = "F";
                }
                else
                {
                    EdgesAssignment[i] = cMesh.EdgeInfo[i].ToString();
                }
            }

            EdgesFoldAngle = new double?[cMesh.EdgeInfo.Count];
            var foldAngles = cMesh.GetFoldAngles();
            int id = 0;
            for (int i = 0; i < cMesh.EdgeInfo.Count; i++)
            {
                if (cMesh.EdgeInfo[i] != 'B')
                {
                    EdgesFoldAngle[i] = 180 * foldAngles[id] / Math.PI;
                    id++;
                }
                else
                {
                    EdgesFoldAngle[i] = null;
                }
            }

            if (mVFullFold)
            {
                for (int i = 0; i < cMesh.EdgeInfo.Count; i++)
                {
                    if (cMesh.EdgeInfo[i] == 'M')
                    {
                        EdgesFoldAngle[i] = -180;
                    }
                    else if (cMesh.EdgeInfo[i] == 'V')
                    {
                        EdgesFoldAngle[i] = 180;
                    }
                }
            }

            FacesVertices = new int[cMesh.Mesh.Faces.Count][];
            for (int i = 0; i < cMesh.Mesh.Faces.Count; i++)
            {
                var face = cMesh.Mesh.Faces[i];
                if (face.IsTriangle)
                {
                    FacesVertices[i] = new int[] { face.A, face.B, face.C };
                }
                else if (face.IsQuad)
                {
                    FacesVertices[i] = new int[] { face.A, face.B, face.C, face.D };
                }
            }

        }
        public CMesh ToCMesh()
        {
            Mesh mesh = new Mesh();
            mesh.Vertices.UseDoublePrecisionVertices = true;
            for (int i = 0; i < VerticesCoords.Length; i++)
            {
                mesh.Vertices.Add(Utils.ToPoint3d(VerticesCoords[i]));
            }

            for (int i = 0; i < FacesVertices.Length; i++)
            {
                mesh.Faces.AddFace(Utils.ToMeshFace(FacesVertices[i]));
            }

            var mountain = new List<Line>();
            var valley = new List<Line>();
            var triangle = new List<Line>();
            for (int i = 0; i < EdgesAssignment.Length; i++)
            {
                var assign = EdgesAssignment[i];
                if (assign == "M")
                {
                    var e = EdgesVertices[i];
                    mountain.Add(new Line(mesh.Vertices[e[0]], mesh.Vertices[e[1]]));
                }

                if (assign == "V")
                {
                    var e = EdgesVertices[i];
                    valley.Add(new Line(mesh.Vertices[e[0]], mesh.Vertices[e[1]]));
                }

                if (assign == "F")
                {
                    var e = EdgesVertices[i];
                    triangle.Add(new Line(mesh.Vertices[e[0]], mesh.Vertices[e[1]]));
                }
            }

            mesh.Normals.ComputeNormals();
            mesh.FaceNormals.ComputeFaceNormals();
            return new CMesh(mesh, mountain, valley, triangle);
        }

        public static FoldFormat ReadFoldFormat(string path)
        {
            string jsonString = File.ReadAllText(path);
            FoldFormat foldFormat = JsonSerializer.Deserialize<FoldFormat>(jsonString);
            return foldFormat;
        }
        private string ParseRhinoModelUnitSystem(Rhino.UnitSystem unitSystem)
        {
            string unit = "m";
            if (unitSystem == UnitSystem.Meters) unit = "m";
            else if (unitSystem == UnitSystem.Centimeters) unit = "cm";
            else if (unitSystem == UnitSystem.Millimeters) unit = "mm";
            return unit;
        }
    }
}
