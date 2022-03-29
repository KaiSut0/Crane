using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Inputs
{
    public class CMeshFromMVTLines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CMeshFromMVTLines class.
        /// </summary>
        public CMeshFromMVTLines()
          : base("CMesh from MVT Lines", "CMesh from MVT",
              "Create the CMesh instance from a mesh and mountain, valley, and triangulated lines.",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddLineParameter("Mountain Crease Lines", "M", "Mountain Crease Lines List", GH_ParamAccess.list);
            pManager.AddLineParameter("Valley Crease Lines", "V", "Valley Crease Lines List", GH_ParamAccess.list);
            pManager.AddLineParameter("Triangulated Lines", "T", "Triangulated Lines List", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            List<Line> m = new List<Line>();
            List<Line> v = new List<Line>();
            List<Line> t = new List<Line>();

            if (!DA.GetData(0, ref mesh)) { return; }
            DA.GetDataList(1, m);
            DA.GetDataList(2, v);
            DA.GetDataList(3, t);

            CMesh cmesh = new CMesh(mesh, m, v, t);

            DA.SetData(0, cmesh);

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
                return Properties.Resource.icons_cmesh_mvt;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("35fa14c2-0bac-460c-a8a1-3335b1f16200"); }
        }
    }
}