using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Crane.Components.Constraints.Equal
{
    public class EqualSectorAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EqualSectorAngleComponent class.
        /// </summary>
        public EqualSectorAngleComponent()
          : base("Equal Sector Angle", "Equal Sector Angle",
              "Equal sector angle constraint",
              "Crane", "Constraints")
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
                return Properties.Resource.icons_equal_sector_angle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e0065ae9-0cce-4e08-aef0-bb6f60aac264"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}