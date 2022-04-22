using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Patterns
{
    public class TuckOrigamize : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TuckOrigamize class.
        /// </summary>
        public TuckOrigamize()
          : base("Tuck Origamize", "Tuck Origamize",
              "Create Tuck origamized mesh.",
              "Crane", "Pattern")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Original mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "Scale", "Face shrink scale.", GH_ParamAccess.item, 0.8);
            pManager.AddNumberParameter("Angle", "Angle", "Face rotation angle.", GH_ParamAccess.item, Math.PI/10);
            pManager.AddNumberParameter("Offset", "Offset", "Tuck offset.", GH_ParamAccess.item, 0.3);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Tuck", "Tuck", "Tucked mesh.", GH_ParamAccess.item);
            pManager.AddPointParameter("Glue Vertex", "Glue Vertex", "Glue vertex", GH_ParamAccess.list);
            pManager.AddLineParameter("Glue Edge", "Glue Edge", "Glue edges.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            double scale = 0.8;
            double angle = Math.PI / 10;
            double offset = 0.3;
            DA.GetData(0, ref mesh);
            DA.GetData(1, ref scale);
            DA.GetData(2, ref angle);
            DA.GetData(3, ref offset);
            var tuck = new Crane.Patterns.TuckOrigamize(mesh, scale, angle, offset);
            DA.SetData(0, tuck.Mesh);
            DA.SetDataList(1, tuck.GlueVertexList);
            DA.SetDataList(2, tuck.GlueEdgeList);
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
            get { return new Guid("390C4BF6-69CB-4A0C-BBE5-1CE161A258D8"); }
        }
    }
}