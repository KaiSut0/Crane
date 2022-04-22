using System;
using System.Collections.Generic;
using Crane.Constraints;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Constraints.Fix
{
    public class FixBoundarySectorAngleSumComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixBoundarySectorAngleSum class.
        /// </summary>
        public FixBoundarySectorAngleSumComponent()
          : base("Fix Boundary Sector Angle Sum", "Fix Boundary Sector Angle Sum",
              "Set fix boundary sector angle sum constraint.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Vertex Ids", "Vertex Ids", "Vertex Ids", GH_ParamAccess.list);
            pManager.AddNumberParameter("Goal Angle", "Goal Angle", "Goal angle", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Fix boundary sector angle sum constraint.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> vertexIds = new List<int>();
            double goalAngle = Math.PI;
            DA.GetDataList(0, vertexIds);
            DA.GetData(1, ref goalAngle);
            DA.SetData(0, new FixBoundarySectorAngleSum(vertexIds.ToArray(), goalAngle));
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
            get { return new Guid("D45B16E7-7CFE-466F-995C-53B4A32AFF0F"); }
        }
    }
}