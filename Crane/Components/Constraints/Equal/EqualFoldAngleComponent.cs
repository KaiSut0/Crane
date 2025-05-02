using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;

namespace Crane.Components.Constraints
{
    public class EqualFoldAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EqualFoldAngleComponent class.
        /// </summary>
        public EqualFoldAngleComponent()
          : base("Equal Fold Angle", "Equal Fold Angle",
              "Equal fold angle constraint.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddLineParameter("FirstLine", "SecondLine", "The first lines for detecting the edge.", GH_ParamAccess.list);
            pManager.AddLineParameter("SecondLine", "SecondLine", "The second lines for detecting the edge.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "Strength", GH_ParamAccess.list, new List<double> { 1 });
            pManager.AddNumberParameter("Tolerance", "Tolerance", "The tolerance for detecting an inner edge from the given line.", GH_ParamAccess.item, 1e-3);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Equal fold angle constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            List<Line> firstLines = new List<Line>();
            List<Line> secondLines = new List<Line>();
            List<double> strength = new List<double>();
            double tolerance = 1e-3;
            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, firstLines);
            DA.GetDataList(2, secondLines);
            DA.GetDataList(3, strength);
            DA.GetData(4, ref tolerance);
            if (firstLines.Count != secondLines.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of first lines and second lines should be the same.");
                return;
            }
            if (strength.Count != firstLines.Count)
            {
                for (int i = strength.Count; i < firstLines.Count; i++)
                {
                    strength.Add(strength[0]);
                }
            }

            EqualFoldAngle constraint = new EqualFoldAngle(cMesh, firstLines.ToArray(), secondLines.ToArray(), strength.ToArray());
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
                return Properties.Resource.icons_equal_fold_angle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4ab8e7f1-c05b-4702-91dd-04e1308b590e"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}