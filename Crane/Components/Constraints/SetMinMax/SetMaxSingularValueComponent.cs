using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane.Core;
using Crane.Constraints;

namespace Crane.Components.Constraints.SetMinMax
{
    public class SetMaxSingularValueComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetMaxSingularValueComponent class.
        /// </summary>
        public SetMaxSingularValueComponent()
          : base("Set Max Singular Value", "Set Max SV",
              "Set maximum singular value constraint.",
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
            pManager.AddNumberParameter("Max SV", "Max SV", "Maximum singular value.", GH_ParamAccess.item, 1);
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
            double maxSV = 1;
            double strength = 1;

            DA.GetData(0, ref cMeshOrig);
            DA.GetData(1, ref maxSV);
            DA.GetData(2, ref strength);

            var constraint = new SetMaxSingularValue(cMeshOrig, strength, maxSV);

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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3EEF176B-CCA6-43A6-8923-FFD6080EFC6D"); }
        }
    }
}