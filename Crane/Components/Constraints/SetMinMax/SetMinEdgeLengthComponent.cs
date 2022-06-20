using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;

namespace Crane.Components.Constraints
{
    public class SetMinEdgeLengthComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetMinEdgeLengthComponent class.
        /// </summary>
        public SetMinEdgeLengthComponent()
          : base("Set Min Edge Length", "Set Min Edge Length",
              "Set minimum edge length. This enforce the edge length to be larger than the set length.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.senary; 
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Min Edge Length", "Min Edge Length", "Set minimum edge length.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Minimum edge length constraint.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double minEdgeLength = 0;
            DA.GetData(0, ref minEdgeLength);
            DA.SetData(0, new SetMinEdgeLength(minEdgeLength));
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
                return Properties.Resource.icons_set_min_edge_length;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6c961f19-a0de-4f86-8462-5e627c1b78be"); }
        }
    }
}