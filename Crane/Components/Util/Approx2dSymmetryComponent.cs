using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Misc;

namespace Crane.Components.Misc
{
    public class Approx2dSymmetryComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Approx2dSymmetryComponent class.
        /// </summary>
        public Approx2dSymmetryComponent()
          : base("Approx 2d Symmetry", "Approx 2d Symmetry",
              "Approximate 2d symmetry from 2d point pairs.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("2dPts", "2dPts", "2dPts", GH_ParamAccess.list);
            pManager.AddIntegerParameter("1stIds", "1stIds", "1stIds", GH_ParamAccess.list);
            pManager.AddIntegerParameter("2ndIds", "2ndIds", "2ndIds", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTransformParameter("2dSym", "2dSym", "2dSym", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pts = new List<Point3d>();
            var fIds = new List<int>();
            var sIds = new List<int>();

            DA.GetDataList(0, pts);
            DA.GetDataList(1, fIds);
            DA.GetDataList(2, sIds);

            var ap2dSym = new Approx2dSymmetry(pts, fIds, sIds);

            DA.SetData(0, ap2dSym.Symmetry2d);

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
                return Properties.Resource.icons_approx_2d_symmetry;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("15d7616e-8bcd-4e4d-b3c7-37dde515423d"); }
        }
    }
}