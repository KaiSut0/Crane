using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Crane.Core;
using Crane.Constraints;

namespace Crane.Components.Constraints.Equal
{
    public class EqualSVDirectionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EqualSVDirectionComponent class.
        /// </summary>
        public EqualSVDirectionComponent()
          : base("Equal SV Direction", "Equal SV Dir",
              "Equal Singular Value Direction for each adjacent faces.",
              "Crane", "Constraints")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "Input original shape.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Strength", "Strength", "Strength of this constraint.", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Equal singular value direction constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMeshOrig = null;
            double strength = 1;

            DA.GetData(0, ref cMeshOrig);
            DA.GetData(1, ref strength);

            var constraint = new EqualSingularValueDirection(cMeshOrig, strength);

            DA.SetData(0, constraint);
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
            get { return new Guid("1216B7D3-C93A-47CD-A7EE-9EE845FF4630"); }
        }
    }
}