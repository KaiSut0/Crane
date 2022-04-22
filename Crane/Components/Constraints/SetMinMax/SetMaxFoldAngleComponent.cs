using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;

namespace Crane.Components.Constraints
{
    public class SetMaxFoldAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetMaxFoldAngle class.
        /// </summary>
        public SetMaxFoldAngleComponent()
          : base("Set Max Fold Angle", "Set Max Fold Angle",
              "Set maximum fold angle. This enforce the fold angle to be smaller than the set angle.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.senary; 
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("MaxFoldAngle", "MaxFoldAngle", "Maximum fold angle.", GH_ParamAccess.item, Math.PI);
            pManager.AddBooleanParameter("MountainOn", "MountainOn",
                "Set maximum fold angle constraint to mountain fold angle.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ValleyOn", "ValleyOn",
                "Set maximum fold angle constraint to valley fold angle.", GH_ParamAccess.item, true);

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
            double maxFoldAngle = Math.PI;
            bool mountainOn = true;
            bool valleyOn = true;
            DA.GetData(0, ref maxFoldAngle);
            DA.GetData(1, ref mountainOn);
            DA.GetData(2, ref valleyOn);
            DA.SetData(0, new SetMaxFoldAngle(maxFoldAngle, mountainOn, valleyOn));
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
                return Properties.Resource.upper_fold_angle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a79503ed-0a37-4c0f-b9ef-1b252720889f"); }
        }
    }
}