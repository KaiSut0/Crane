using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class OnPlaneComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OnPlaneConstraintComponent class.
        /// </summary>
        public OnPlaneComponent()
          : base("On Plane", "On Plane",
              "Set the constraint to restrict the selected point onto the goal plane.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Plane", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "Point", "Point to restrict onto the plane.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("PointID", "PointID", "You can also use id to select the point to restrict onto the plane. If this is not empty, this is used instead of the point input.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "Strength", GH_ParamAccess.item, 1.0);
            pManager[3].Optional = true;
            pManager[4].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "On plane constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Plane plane = new Plane();
            List<Point3d> pts = new List<Point3d>();
            List<int> ptIDs = new List<int>();
            double strength = 1.0;

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref plane);
            DA.GetDataList(2, pts);
            bool usePtIDs = DA.GetDataList(3, ptIDs);
            DA.GetData(4, ref strength);

            if (!usePtIDs)
            {
                foreach (var pt in pts)
                {
                    ptIDs.Add(Core.Util.GetPointID(cMesh.Mesh, pt));
                }
            }

            var constraint = new OnPlane(cMesh, plane, ptIDs.ToArray(), strength);
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
                return Properties.Resource.icons_on_plane;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a5971d14-d3f3-42e1-8234-42e1ec1ae9f9"); }
        }
    }
}