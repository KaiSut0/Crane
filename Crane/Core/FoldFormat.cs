using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Rhino;

namespace Crane.Core
{
    public class FoldFormat
    {
        [JsonPropertyName("edges_assignment")]
        public string[] EdgesAssignment { get; set; } 
        [JsonPropertyName("edges_foldAngle")]
        public double[] EdgesFoldAngle { get; set; }
        [JsonPropertyName("edges_vertices")]
        public int[][] EdgesVertices { get; set; }
        [JsonPropertyName("faces_vertices")]
        public int[][] FacesVertices { get; set; }
        [JsonPropertyName("file_classes")]
        public string[] FileClasses { get; set; }
        [JsonPropertyName("file_creator")]
        public string FileCreator { get; set; }
        [JsonPropertyName("file_spec")]
        public int FileSpec { get; set; }
        [JsonPropertyName("frame_classes")]
        public string[] FrameClasses { get; set; }
        [JsonPropertyName("frame_unit")]
        public string FrameUnit { get; set; }
        [JsonPropertyName("vertices_coords")]
        public double[][] VerticesCoords { get; set; }

        public FoldFormat(CMesh cMesh)
        {
            FileClasses = new string[] { "singleModel" };
            FileCreator = "Crane";
            FileSpec = 1;
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

            EdgesAssignment = new string[cMesh.edgeInfo.Count];
            for (int i = 0; i < cMesh.edgeInfo.Count; i++)
            {
                EdgesAssignment[i] = cMesh.edgeInfo[i].ToString();
            }

            EdgesFoldAngle = new double[cMesh.edgeInfo.Count];
            var foldAngles = cMesh.GetFoldAngles();
            int id = 0;
            for (int i = 0; i < cMesh.edgeInfo.Count; i++)
            {
                if (cMesh.edgeInfo[i] != 'B')
                {
                    EdgesFoldAngle[i] = 180 * foldAngles[id] / Math.PI;
                    id++;
                }
                else
                {
                    EdgesFoldAngle[i] = 0;
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
