using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane;
using Crane.Core;

namespace Crane.Components.Outputs
{
    public class DeconstructCMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructCMesh class.
        /// </summary>
        public DeconstructCMesh()
          : base("Deconstruct CMesh", "Deconstruct CMesh",
              "Deconstruct the CMesh into the mesh and folding lines.",
              "Crane", "Outputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddLineParameter("Mountain", "M", "Mountain Crease Lines", GH_ParamAccess.list);
            pManager.AddLineParameter("Valley", "V", "Valley Crease Lines", GH_ParamAccess.list);
            pManager.AddLineParameter("Boundary", "B", "Boundary Edges", GH_ParamAccess.list);
            pManager.AddLineParameter("Unassigned", "U", "Unassigned Crease Lines", GH_ParamAccess.list);
            pManager.AddLineParameter("Triangulate", "T", "Triangulate Edges", GH_ParamAccess.list);
            pManager.AddLineParameter("Edges", "E", "Edges", GH_ParamAccess.list);
            pManager.AddMeshParameter("Folding", "F", "Folding mesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("Development", "D", "Development", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cmesh = new CMesh();

            if (!DA.GetData(0, ref cmesh)) { return; }

            Mesh mesh = ReconstructQuadMesh(cmesh);
            var edges = cmesh.Mesh.TopologyEdges;

            List<Line> m = new List<Line>();
            List<Line> v = new List<Line>();
            List<Line> b = new List<Line>();
            List<Line> u = new List<Line>();
            List<Line> t = new List<Line>();
            List<Line> e = new List<Line>();


            for (int i = 0; i < edges.Count; i++)
            {
                int info = cmesh.edgeInfo[i];

                if (info == 'M')
                {
                    m.Add(edges.EdgeLine(i));
                }
                else if (info == 'V')
                {
                    v.Add(edges.EdgeLine(i));
                }
                else if (info == 'B')
                {
                    b.Add(edges.EdgeLine(i));
                }
                else if (info == 'U')
                {
                    u.Add(edges.EdgeLine(i));
                }
                else if (info == 'T')
                {
                    t.Add(edges.EdgeLine(i));
                }

                e.Add(edges.EdgeLine(i));
            }

            DA.SetData(0, mesh);
            DA.SetDataList(1, m);
            DA.SetDataList(2, v);
            DA.SetDataList(3, b);
            DA.SetDataList(4, u);
            DA.SetDataList(5, t);
            DA.SetDataList(6, e);
            DA.SetData(7, cmesh.FoldingMesh());
            DA.SetData(8, cmesh.Development());
        }
        private Mesh ReconstructQuadMesh(CMesh cm)
        {
            var faces = cm.orig_faces;
            Mesh mesh = cm.Mesh.DuplicateMesh();
            mesh.Faces.Destroy();
            mesh.Faces.AddFaces(faces);
            return mesh;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e50bde60-7f16-481b-a234-a05c6a79901e"); }
        }
    }
}