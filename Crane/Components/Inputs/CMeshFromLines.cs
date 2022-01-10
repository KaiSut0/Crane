using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Inputs
{
    public class CMeshFromLines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CMeshFromLines class.
        /// </summary>
        public CMeshFromLines()
          : base("CMesh From Lines", "CMesh From Lines",
              "CMesh from line topologies.",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "Lines", "Lines to construct mesh topology.", GH_ParamAccess.list);
            pManager.AddLineParameter("Mountain Lines", "M", "Mountain Lines", GH_ParamAccess.list);
            pManager.AddLineParameter("Valley Lines", "V", "Valley Lines", GH_ParamAccess.list);
            pManager.AddLineParameter("Triangulated Lines", "T", "Triangulated Lines", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Face Valence", "Face Valence", "Face valence.", GH_ParamAccess.item, 7);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 0.001);
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
            List<Line> lines = new List<Line>();
            List<Line> m = new List<Line>();
            List<Line> v = new List<Line>();
            List<Line> t = new List<Line>();
            int fv = 7;
            double tol = 0.001;
            DA.GetDataList(0, lines);
            DA.GetDataList(1, m);
            DA.GetDataList(2, v);
            DA.GetDataList(3, t);
            DA.GetData(4, ref fv);
            DA.GetData(5, ref tol);
            CMesh cMesh = new CMesh(lines, m, v, t, fv, tol);
            DA.SetData(0, cMesh);

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
                return Properties.Resource.cmesh_from_lines;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("df054f84-2cc6-4c13-8b0e-626f31a4cd65"); }
        }
    }
}