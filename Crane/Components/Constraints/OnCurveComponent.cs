using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class OnCurveComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OnCurve class.
        /// </summary>
        public OnCurveComponent()
          : base("OnCurve", "OnCurve",
              "Set the constraint to restrict the selected point onto the goal curve.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curve", "Curve", "Curve", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "Point", "Point to restrict onto the curve.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("PointID", "PointID", "You can also use id to select the point to restrict onto the curve. If this is not empty, this is used instead of the point input.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "Strength", GH_ParamAccess.item, 1.0);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "On curve constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Curve curve = null;
            List<Point3d> pts = new List<Point3d>();
            List<int> ptIDs = new List<int>();
            double strength = 1.0;

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref curve);
            DA.GetDataList(2, pts);
            bool usePtIDs = DA.GetDataList(3, ptIDs);
            DA.GetData(4, ref strength);

            if (!usePtIDs)
            {
                foreach (var pt in pts)
                {
                    ptIDs.Add(Core.Utils.GetPointID(cMesh.Mesh, pt));
                }
            }

            var constraint = new OnCurve(cMesh, curve, ptIDs.ToArray(), strength);
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
            get { return new Guid("1d0246e4-5ecc-482d-893f-64942eef985b"); }
        }
    }
}