using System;
using System.Collections.Generic;
using Crane.Constraints;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Constraints.Fix
{
    public class FixBoundarySectorAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixBoundarySectorAngleComponent class.
        /// </summary>
        public FixBoundarySectorAngleComponent()
          : base("Fix Boundary Sector Angle", "Fix Boundary Sector Angle",
              "Set fix boundary sector angle constraint.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Vertex Ids", "Vertex Ids", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Goal Angles", "Goal Angles", "", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Fix boundary sector angle constraint",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> vertexIds = new List<int>();
            List<double> goalAngles = new List<double>();
            DA.GetDataList(0, vertexIds);
            DA.GetDataList(1, goalAngles);
            DA.SetData(0, new FixBoundarySectorAngle(vertexIds.ToArray(), goalAngles.ToArray()));
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
                return Properties.Resource.icons_fix_boundary_sector_angle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4159163A-C35E-4546-9614-7EA8164449C0"); }
        }
    }
}