using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Crane.Constraints;

namespace Crane.Components.Constraints.Fix
{
    public class FixAreaSumComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixAreaSumComponent class.
        /// </summary>
        public FixAreaSumComponent()
          : base("Fix Area Sum", "Fix Area Sum",
              "Set fix sum of mesh area constraint.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Goal Area", "Goal Area", "Goal Area", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stiffness", "Stiffness", "Stiffness", GH_ParamAccess.item, 1);
            pManager[1].Optional = true;
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
            double goalArea = 0;
            double stiffness = 1;
            DA.GetData(0, ref goalArea);
            DA.GetData(1, ref stiffness);
            DA.SetData(0, new FixAreaSum(goalArea, stiffness));
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
            get { return new Guid("4FE7C04E-DB09-463A-8691-F5CF4C4058A0"); }
        }
    }
}