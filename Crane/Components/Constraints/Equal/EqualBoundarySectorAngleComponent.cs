using System;
using System.Collections.Generic;
using Crane.Constraints;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Constraints.Equal
{
    public class EqualBoundarySectorAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EqualBoundarySectorAngleComponent class.
        /// </summary>
        public EqualBoundarySectorAngleComponent()
          : base("Equal Boundary Sector Angle", "Equal Boundary Sector Angle",
              "Set constraint to equal selected boundary sector angles.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("First Vertex Ids", "First Vertex Ids", "First vertex ids.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Second Vertex Ids", "Second Vertex Ids", "Second vertex ids.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Equal boundary sector angle constraint.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> primaryVertexIds = new List<int>();
            List<int> secondaryVertexIds = new List<int>();
            DA.GetDataList(0, primaryVertexIds);
            DA.GetDataList(1, secondaryVertexIds);
            DA.SetData(0, new EqualBoundarySectorAngle(primaryVertexIds.ToArray(), secondaryVertexIds.ToArray()));
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
                return Properties.Resource.icons_equal_boundary_sector_angle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1DC66640-E816-4B78-953D-2D7CAF70B0E2"); }
        }
    }
}