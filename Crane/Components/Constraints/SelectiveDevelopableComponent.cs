using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class SelectiveDevelopableComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SelectiveDevelopableComponent class.
        /// </summary>
        public SelectiveDevelopableComponent()
          : base("Selective Developable", "Selective Developable",
              "Selective developable component.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("InnerVertices", "InnerVertices", "Inner vertices to develop.",
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 1e-3);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Selective developable constraint.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            var innerVertices = new List<Point3d>();
            double tolerance = 1e-3;
            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, innerVertices);
            DA.GetData(2, ref tolerance);

            var constraint = new SelectiveDevelopable(cMesh, innerVertices.ToArray(), tolerance);

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
        public override Guid ComponentGuid
        {
            get { return new Guid("5775a364-e40a-418b-9adf-0471bde151e2"); }
        }
    }
}