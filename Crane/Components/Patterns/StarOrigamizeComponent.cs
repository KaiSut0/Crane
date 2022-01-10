using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Patterns;

namespace Crane.Components.Patterns
{
    public class StarOrigamizeComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the StarOrigamize class.
        /// </summary>
        public StarOrigamizeComponent()
          : base("Star Origamize", "Star Origamize",
              "Origamize the input mesh to generalized Resch pattern by inserting star tuck.",
              "Crane", "Pattern")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("ShrinkRatio", "ShrinkRatio", "ShrinkRatio", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("OffsetRatio", "OffsetRatio", "OffsetRatio", GH_ParamAccess.item, 1.0);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("OrigamizedMesh", "OrigamizedMesh", "Origamized mesh.", GH_ParamAccess.item);
            pManager.AddPointParameter("BaseVertices", "BaseVertices", "BaseVertices", GH_ParamAccess.list);
            pManager.AddPointParameter("TopVertices", "TopVertices", "TopVertices", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            double shrinkRatio = 0.3;
            double offsetRatio = 1.0;

            DA.GetData(0, ref mesh);
            DA.GetData(1, ref shrinkRatio);
            DA.GetData(2, ref offsetRatio);

            var starOrigamize = new StarOrigamize(mesh, shrinkRatio, offsetRatio);

            DA.SetData(0, starOrigamize.Mesh);
            DA.SetDataList(1, starOrigamize.BaseVertices);
            DA.SetDataList(2, starOrigamize.TopVertices);
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
            get { return new Guid("3f76a541-5e61-4dd3-bfb5-8a0601a9be0d"); }
        }
    }
}