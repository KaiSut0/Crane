using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane;
using Crane.Core;

namespace Crane.Components.Outputs
{
    public class DeconstructRigidOrigami : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructRigidOrigami class.
        /// </summary>
        public DeconstructRigidOrigami()
          : base("Deconstruct RigidOrigami", "Deconstruct RigidOrigami",
              "Deconstruct RigidOrigami.",
              "Crane", "Outputs")
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
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RigidOrigami rigidOrigami = new RigidOrigami();
            if (!DA.GetData(0, ref rigidOrigami)) { return; }
            DA.SetData(0, rigidOrigami.CMesh);

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
            get { return new Guid("f6018c89-a1ab-4e5b-92b5-dd7b13814742"); }
        }
    }
}