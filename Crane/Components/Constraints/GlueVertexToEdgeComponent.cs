using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class GlueVertexToEdgeComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GlueVertexToEdgeComponent class.
        /// </summary>
        public GlueVertexToEdgeComponent()
          : base("Glue Vertex To Edge", "Glue Vertex To Edge",
              "Set glue the vertex to the edge constraint.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Vertex", "Vertex", "Vertex glue to edge.", GH_ParamAccess.item);
            pManager.AddLineParameter("Edge", "Edge", "Edge glue to edge.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance to detect the vertex and edge indices of the mesh.", GH_ParamAccess.item, 1e-3);

            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Glue vertex to edge constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            var vertex = new Point3d();
            var edge = new Line();
            double threshold = 1e-3;
            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref vertex);
            DA.GetData(2, ref edge);
            DA.GetData(3, ref threshold);

            var constraint = new GlueVertexToEdge(cMesh, vertex, edge, threshold);

            DA.SetData(0, constraint);
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
            get { return new Guid("c40d765f-9a7c-4c0f-9dbb-6d69ec656d53"); }
        }
    }
}