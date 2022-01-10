using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class FixFoldAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixFoldAngleComponent class.
        /// </summary>
        public FixFoldAngleComponent()
          : base("FixFoldAngle", "FixFoldAngle",
              "Set the constraint to fix the selected fold angle to the goal angle.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddLineParameter("Line", "Line", "The lines for detecting the inner edge to fix the fold angle.", GH_ParamAccess.list);
            pManager.AddNumberParameter("FoldAngle", "FoldAngle", "The fixing fold angles.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "Strength", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "The tolerance for detecting the inner edge from given line.", GH_ParamAccess.item, 1e-3);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Fold angle fixing constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            List<Line> lines = new List<Line>();
            List<double> foldAngles = new List<double>();
            List<double> strengths = new List<double>();
            double tolerance = 1e-3;
            DA.GetData(0,ref cMesh);
            DA.GetDataList(1, lines);
            DA.GetDataList(2, foldAngles);
            DA.GetDataList(3, strengths);
            DA.GetData(4, ref tolerance);
            if (foldAngles.Count != lines.Count)
            {
                for (int i = foldAngles.Count; i < lines.Count; i++)
                {
                    foldAngles.Add(foldAngles[0]);
                }
            }

            FixFoldAngle constraint = new FixFoldAngle(cMesh, lines.ToArray(), foldAngles.ToArray(), strengths.ToArray(), tolerance);
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
            get { return new Guid("2f2aa817-490c-407a-b2a9-c7b7724e884d"); }
        }
    }
}