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
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Original mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "Scale", "Face shrink scale.", GH_ParamAccess.item, 0.8);
            pManager.AddNumberParameter("Angle1", "Angle1", "Face rotation angle.", GH_ParamAccess.item, Math.PI/10);
            pManager.AddNumberParameter("Angle2", "Angle2", "Tuck rotation angle.", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Offset", "Offset", "Tuck offset.", GH_ParamAccess.item, 0.3);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Tuck", "Tuck", "Tucked mesh.", GH_ParamAccess.item);
            pManager.AddPointParameter("Glue Vertex", "Glue Vertex", "Glue vertex", GH_ParamAccess.list);
            pManager.AddLineParameter("Glue Edge", "Glue Edge", "Glue edges.", GH_ParamAccess.list);
            pManager.AddLineParameter("Mountain", "Mountain", "Mountain edges.", GH_ParamAccess.list);
            pManager.AddLineParameter("Valley", "Valley", "Valley edges.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            double scale = 0.8;
            double angle1 = Math.PI / 10;
            double angle2 = 0;
            double offset = 0.3;
            DA.GetData(0, ref mesh);
            DA.GetData(1, ref scale);
            DA.GetData(2, ref angle1);
            DA.GetData(3, ref angle2);
            DA.GetData(4, ref offset);
            var tuck = new Crane.Patterns.TuckOrigamize(mesh, scale, angle1, angle2, offset);
            DA.SetData(0, tuck.Mesh);
            DA.SetDataList(1, tuck.GlueVertexList);
            DA.SetDataList(2, tuck.GlueEdgeList);
            DA.SetDataList(3, tuck.MountainLines);
            DA.SetDataList(4, tuck.ValleyLines);
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
                return Properties.Resource.icons_chicken_srf_59;
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