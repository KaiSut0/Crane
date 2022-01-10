using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane;
using Crane.Core;

namespace Crane.Components.Inputs
{
    public class RigidOrigamiFromCMeshAndConstraints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RigidOrigamiFromCMeshAndConstraints class.
        /// </summary>
        public RigidOrigamiFromCMeshAndConstraints()
          : base("Construct Rigid Origami", "Construct Rigid Origami",
              "Rigid Origami form the CMesh and constraints.",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "Input the CMesh instance.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Constraints", "Constraints", "Input the constraints", GH_ParamAccess.list);
            pManager[1].Optional = true;
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
            CMesh cMesh = new CMesh();
            List<Constraint> constraints = new List<Constraint>();
            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, constraints);
            RigidOrigami rigidOrigami = new RigidOrigami(cMesh, constraints);
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
            get { return new Guid("297bbb77-8be3-400d-9dfe-1b4303b73678"); }
        }
    }
}