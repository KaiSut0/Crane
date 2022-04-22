using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Crane.Components.Constraints
{
    public class SetMaxSectorAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetMaxSectorAngleComponent class.
        /// </summary>
        public SetMaxSectorAngleComponent()
          : base("Set Min Sector Angle", "Set Min Sector Angle",
              "Set minimal sector angle. This enforce the sector angle to be larger than the set angle.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.senary; 
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
                return Properties.Resource.icons_set_min_sector_angle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c313598e-0284-42b1-97f2-093809f9d410"); }
        }
    }
}