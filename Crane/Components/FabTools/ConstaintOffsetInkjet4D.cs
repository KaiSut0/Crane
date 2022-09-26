using System;
using System.Collections.Generic;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.FabTools
{
    public class ConstaintOffsetInkjet4D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ConstaintOffsetInkjet4D class.
        /// </summary>
        public ConstaintOffsetInkjet4D()
          : base("Constaint Offset Inkjet 4D", "Cons Ink 4D",
              "Constant offset inkjet 4d.",
              "Crane", "FabTools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ink Thickness", "Thickness", "Ink Thickness", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Offset Width", "Offset", "Offset width for folding lines.", GH_ParamAccess.item, 1);
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
            pManager.AddCurveParameter("Mid", "Mid", "Mid face polylines.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Dev", "Dev", "Development polylines.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fold", "Fold", "Folded state polylines.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Top Panel", "Top Panel", "Thick panels of the top faces.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Bottom Panel", "Fold Panel", "Thick panels of the bottom faces.", GH_ParamAccess.list);
            pManager.AddTransformParameter("Trans", "Trans", "Transformation Dev to Fold.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            double inkThickness = 0.1;
            double offset = 1;
            double tolerance = 1e-3;
            Point3d origin = Point3d.Origin;
            double rotAng = 0;
            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref inkThickness);
            DA.GetData(2, ref offset);
            DA.GetData(3, ref tolerance);
            DA.GetData(4, ref origin);
            DA.GetData(5, ref rotAng);

            FabMesh fabMesh = new FabMesh(cMesh, new Point2d(origin.X, origin.Y), rotAng, tolerance);
            var top = fabMesh.OffsetFaceInkjet4DConstant(offset, 0);
            var bottom = fabMesh.OffsetFaceInkjet4DConstant(offset, 2);
            var mid = fabMesh.OffsetFaceInkjet4DConstant(offset, 1);
            var devPolyline = fabMesh.GetFacePolylines(fabMesh.DevCMesh);
            var foldPolyline = fabMesh.GetFacePolylines(fabMesh.CMesh);
            var topPanel = fabMesh.ThickPanels(top, inkThickness);
            var bottomPanel = fabMesh.ThickPanels(bottom, -inkThickness);
            DA.SetDataList(0, top);
            DA.SetDataList(1, bottom);
            DA.SetDataList(2, mid);
            DA.SetDataList(3, devPolyline);
            DA.SetDataList(4, foldPolyline);
            DA.SetDataList(5, topPanel);
            DA.SetDataList(6, bottomPanel);
            DA.SetDataList(7, fabMesh.GetDev2FoldTransforms());

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
                return Properties.Resource.icons_inkjet_4d;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6CAF2978-E991-4836-BF13-B46098DC53A0"); }
        }
    }
}