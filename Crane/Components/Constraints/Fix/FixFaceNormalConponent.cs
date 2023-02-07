using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Constraints.Fix
{
    public class FixFaceNormalConponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixFaceNormalConponent class.
        /// </summary>
        public FixFaceNormalConponent()
          : base("Fix Face Normal", "Fix Face Normal",
              "Fix face normal component.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Face Center", "Face Center", "Face center to select.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Goal Normal", "Goal Normal", "Goal normal.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "Strength", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Edge length fixing constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            List<Point3d> fc = new List<Point3d>();
            List<Vector3d> gn = new List<Vector3d>();
            List<double> s = new List<double>();
            DA.GetData(0,ref cMesh);
            DA.GetDataList(1, fc);
            DA.GetDataList(2, gn);
            DA.GetDataList(3, s);

            for (int i = 0; i < gn.Count; i++)
            {
                gn[i] = gn[i] / gn[i].Length;
            }

            var constraint = new AlignNormal(cMesh, fc, gn, s);

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
            get { return new Guid("58FC0D33-93D2-454D-982A-8F9068FE0024"); }
        }


        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}