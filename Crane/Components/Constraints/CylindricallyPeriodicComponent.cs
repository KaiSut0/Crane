using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

using Crane.Constraints;

namespace Crane.Components.Constraints
{
    public class CylindricallyPeriodicComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CylindricallyPeriodic class.
        /// </summary>
        public CylindricallyPeriodicComponent()
          : base("CylindricallyPeriodic", "CylindricallyPeriodic",
              "Cylindrically periodic constraint",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("From Vertex Id", "From Vertex Id", "From Vertex Id", GH_ParamAccess.item);
            pManager.AddIntegerParameter("To Vertex Id", "To Vertex Id", "To Vertex Id", GH_ParamAccess.item);
            pManager.AddIntegerParameter("S1 or S2", "S1 or S2", "Input S1:0, S2:1", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Periodic Constraint", "Periodic Constraint", "Periodic Constraint", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int fromId = 0;
            int toId = 1;
            int S1orS2 = 0;

            DA.GetData(0, ref fromId);
            DA.GetData(1, ref toId);
            DA.GetData(2, ref S1orS2);

            IndexPair VIdPair = new IndexPair(fromId, toId);

            CylindricallyPeriodic period = new CylindricallyPeriodic(VIdPair, S1orS2);

            DA.SetData(0, period);
            
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
            get { return new Guid("212037ad-457d-4e3f-a15d-d568a54a6c33"); }
        }
    }
}