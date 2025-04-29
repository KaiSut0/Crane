using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Crane.Core;
using Crane.Constraints;

namespace Crane.Components.Constraints.SetMinMax
{
    public class SetMinSingularValueComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetMinSingularValueComponent class.
        /// </summary>
        public SetMinSingularValueComponent()
          : base("Set Min Singular Value", "Set Min Singular Value",
              "Set minimum singular value constraint.",
              "Crane", "Constraints")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.senary; 


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Min SV", "Min SV", "Minimum singular value.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Strength", "Strength", "Strength", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Set minimum singular value constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMeshOrig = null;
            double minSV = 1;
            double strength = 1;

            DA.GetData(0, ref cMeshOrig);
            DA.GetData(1, ref minSV);
            DA.GetData(2, ref strength);

            var constraint = new SetMinSingularValue(cMeshOrig, strength, minSV);

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
            get { return new Guid("421BACEA-D2FD-48DA-BEAF-C77E76C7ACD3"); }
        }
    }
}