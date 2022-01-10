using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using Crane.Core;
using OpenCvSharp.CPlusPlus;
using Point3d = Rhino.Geometry.Point3d;

namespace Crane.Components.FabTools
{
    public class OffsetThickPanel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OffsetThickPanel class.
        /// </summary>
        public OffsetThickPanel()
          : base("Offset Thick Panel", "Offset Thick Panel",
              "Offset thick panel.",
              "Crane", "FabTools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 1e-3);
            pManager.AddPointParameter("Origin", "Origin", "Origin", GH_ParamAccess.item, new Point3d(0, 0, 0));
            pManager.AddNumberParameter("RotAng", "RotAng", "Rotation angle.", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Top", "Top", "Top face polylines.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Bottom", "Bottom", "Bottom face polylines.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            double thickness = 1.0;
            double tolerance = 1e-3;
            Point3d origin = Point3d.Origin;
            double rotAng = 0;
            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref thickness);
            DA.GetData(2, ref tolerance);

            FabMesh fabMesh = new FabMesh(cMesh, new Point2d(origin.X, origin.Y), rotAng, tolerance);
            DA.SetDataList(0, fabMesh.OffsetFace(thickness, true));
            DA.SetDataList(1, fabMesh.OffsetFace(thickness, false));
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
            get { return new Guid("6df57d52-9371-4c2f-a3ea-b36e5563fa15"); }
        }
    }
}