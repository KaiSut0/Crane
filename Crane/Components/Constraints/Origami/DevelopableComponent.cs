using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;

namespace Crane.Components.Constraints
{
    public class DevelopableComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Developable class.
        /// </summary>
        public DevelopableComponent()
          : base("Developable", "Developable",
              "Set developable constraint.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.quinary; 
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
            pManager.AddGenericParameter("Constraint", "C", "Developable constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, new Developable());
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
                return Properties.Resource.icons_developable;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f0bd4523-a9d4-4f88-b80f-7a6cb5f6670a"); }
        }
    }
}