using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class OnPointComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OnPointComponent class.
        /// </summary>
        public OnPointComponent()
          : base("On Point", "On Point",
              "Set the constraint to restrict the selected points onto the goal point.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("GoalPoint", "GoalPoint", "Goal point", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "Points", "Point to restrict onto the mesh.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("PointID", "PointID", "You can also use id to select the point to restrict onto the point. If this is not empty, this is used instead of the point input.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "Strength", GH_ParamAccess.item, 1.0);
            pManager[3].Optional = true;
            pManager[4].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "On point constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Point3d gpt = new Point3d();
            List<Point3d> pts = new List<Point3d>();
            List<int> ptIDs = new List<int>();
            double strength = 1.0;

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref gpt);
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

            var constraint = new OnPoint(cMesh, gpt, ptIDs.ToArray(), strength);
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
            get { return new Guid("5bb8ea7e-36b3-4ca1-9a65-ad37caef5a6c"); }
        }
    }
}