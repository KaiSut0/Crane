using System;
using System.Collections.Generic;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane.Constraints;

namespace Crane.Components.Constraints.Fix
{
    public class FixSingularValueComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixSingularValueComponent class.
        /// </summary>
        public FixSingularValueComponent()
          : base("Fix Singular Value", "Fix Singular Value",
              "Fix singular values compared to input original CMesh.",
              "Crane", "Constraints")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh that is object to simulation.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Original CMesh", "Orig CMesh", "Original shape of CMesh that is reference of deformation.", GH_ParamAccess.item);
            pManager.AddPointParameter("Face Points", "Face Points", "Add face center points for applying constraint whose mesh is simulated.", GH_ParamAccess.list);
            pManager.AddPointParameter("Original Face Points", "Orig Face Points", "Add face center points for applying constraint whose mesh is compared to simulation mesh.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Sigma1", "Sigma1", "Goal singular value of deformation gradient tensor. sigma1 > sigma2.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Sigma2", "Sigma2", "Goal singular value of deformation gradient tensor. sigma1 > sigma2.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Mode", "Mode", "If mode == 0, sigma1 and sigma2 are fixed. If mode == 1, sigma1 is fixed. If mode==2, sigma2 is fixed.", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Strength", "Strength", "Strength of this constraint.", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddGenericParameter("Constraint", "Constraint", "Singular value fixing constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            CMesh cMeshOrig = null;

            List<Point3d> pts = new List<Point3d>();
            List<Point3d> ptsOrig = new List<Point3d>();
            List<double> s1 = new List<double>();
            List<double> s2 = new List<double>();
            int mode = 0;
            double strength = 1;

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref cMeshOrig);
            DA.GetDataList(2, pts);
            DA.GetDataList(3, ptsOrig);
            DA.GetDataList(4, s1);
            DA.GetDataList(5, s2);
            DA.GetData(6, ref mode);
            DA.GetData(7, ref strength);

            FixSingularValue constraint = new FixSingularValue(cMesh, cMeshOrig, pts.ToArray(), ptsOrig.ToArray(), s1.ToArray(), s2.ToArray(), mode, strength);

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
            get { return new Guid("FC6C90FC-0689-4948-852C-478333621607"); }
        }
    }
}