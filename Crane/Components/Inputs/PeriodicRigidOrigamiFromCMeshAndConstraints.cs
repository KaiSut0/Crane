using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Inputs
{
    public class PeriodicRigidOrigamiFromCMeshAndConstraints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RigidOrigamiFromCMeshAndConstraints class.
        /// </summary>
        public PeriodicRigidOrigamiFromCMeshAndConstraints()
          : base("PeriodicRigidOrigami", "PeriodicRigidOrigami",
              "Periodic Rigid Origami form the CMesh and constraints.",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "Input the CMesh instance.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Constraints", "Constraints", "Input the constraints.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Periodic Constriants", "Periodic Constraints", "Input the periodic constraints.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Axis", "Axis", "Input the cylinder axis.", GH_ParamAccess.item);
            pManager.AddPointParameter("Origin", "Origin", "Input the cylinder axis origin.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PeriodicRigidOrigami", "PeriodicRigidOrigami", "PeriodicRigidOrigami", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            List<Constraint> constraints = new List<Constraint>();
            List<CylindricallyPeriodic> periodicConstraints = new List<CylindricallyPeriodic>();
            Vector3d axis = new Vector3d(0, 0, 0);
            Point3d origin = new Point3d(0, 0, 0);
            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, constraints);
            DA.GetDataList(2, periodicConstraints);
            DA.GetData(3, ref axis);
            DA.GetData(4, ref origin);
            PeriodicRigidOrigami rigidOrigami = new PeriodicRigidOrigami(cMesh, constraints, periodicConstraints);
            rigidOrigami.CMesh.CylinderAxis = axis;
            rigidOrigami.CMesh.CylinderOrigin = origin;
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
            get { return new Guid("9b75d7e1-009d-4763-8159-45a16e8a560e"); }
        }
    }
}