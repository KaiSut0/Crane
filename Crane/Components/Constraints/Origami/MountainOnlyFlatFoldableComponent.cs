using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;

namespace Crane.Components.Constraints
{
    public class MountainOnlyFlatFoldableComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MountainOnlyFlatFoldableComponent class.
        /// </summary>
        public MountainOnlyFlatFoldableComponent()
          : base("Mountain Only Flat-Foldable", "Mountain Only Flat-Foldable",
              "Mountain only flat-foldable constraint.",
              "Crane", "Const-Origami")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Constraint", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, new MountainOnlyFlatFoldable());
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
                return Properties.Resource.icons_mountain_only_flat_foldable;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ebbd9518-2019-4eec-8a3d-db06550d5cec"); }
        }
    }
}