using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Patterns
{
    public class YoshimuraFoldingComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the YoshimuraFoldingComponent class.
        /// </summary>
        public YoshimuraFoldingComponent()
          : base("Yoshimura Fold", "Yoshimura",
              "Create a Yoshimura folding crease pattern.",
              "Crane", "Pattern")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("X", "X", "X", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Y", "Y", "Y", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("A", "A", "A sector angle.", GH_ParamAccess.item, 2 * Math.PI / 3);
            pManager.AddIntegerParameter("NX", "NX", "Number of X units.", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("NY", "NY", "Number of Y units.", GH_ParamAccess.item, 10);
            pManager[0].Optional = true;
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
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddLineParameter("M", "M", "Mountain creases.", GH_ParamAccess.list);
            pManager.AddLineParameter("V", "V", "Valley creases.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("E0296666-33B4-47D6-A9A1-3496DB03A703"); }
        }
    }
}