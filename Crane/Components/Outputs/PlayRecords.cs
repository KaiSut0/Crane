using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Outputs
{
    public class PlayRecords : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlayRecords class.
        /// </summary>
        public PlayRecords()
          : base("Play Records", "Play Records",
              "Play the recorded rigid origami motion and form finding processs.",
              "Crane", "Outputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rigid Origami", "RO", "Rigid origami.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Frame Number", "Frame", "The frame to play.", GH_ParamAccess.item);
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
            int frame = 0;
            DA.GetData(0, ref rigidOrigami);
            DA.GetData(1, ref frame);

            rigidOrigami = new RigidOrigami(rigidOrigami);

            if (frame < rigidOrigami.RecordedMeshPoints.Count)
                rigidOrigami.CMesh.UpdateMesh(rigidOrigami.RecordedMeshPoints[frame]);
            else
                rigidOrigami.CMesh.UpdateMesh(rigidOrigami.RecordedMeshPoints[rigidOrigami.RecordedMeshPoints.Count - 1]);

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
            get { return new Guid("14379720-87da-45ce-9755-f6239c6ffc47"); }
        }
    }
}