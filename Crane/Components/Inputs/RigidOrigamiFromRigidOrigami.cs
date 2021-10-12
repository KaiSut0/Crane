using System;
using System.Collections.Generic;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Inputs
{
    public class RigidOrigamiFromRigidOrigami : GH_Component
    {
        public RigidOrigami rigidOrigami;
        /// <summary>
        /// Initializes a new instance of the RigidOrigami class.
        /// </summary>
        public RigidOrigamiFromRigidOrigami()
          : base("RigidOrigami", "RigidOrigami",
              "Rigid Origami",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RigidOrigami", "RigidOrigami", "RigidOrigami", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RigidOrigami", "RigidOrigami", "RigidOrigami", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData(0, ref rigidOrigami);
            DA.SetData(0, rigidOrigami);
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
            get { return new Guid("0fc8460e-997c-4943-aa3c-695a4f66b855"); }
        }
    }
}