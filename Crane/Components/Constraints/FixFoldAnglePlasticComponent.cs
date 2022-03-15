using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class FixFoldAnglePlasticComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixFoldingAnglePlasticComponent class.
        /// </summary>
        public FixFoldAnglePlasticComponent()
          : base("Fix Fold Angle Plastic", "Fix Fold Angle Plastic",
              "Set the constraint to fix the selected fold angle to the goal angle with plastic moment.",
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
            pManager.AddNumberParameter("Plastic Moment", "Plastic Moment", "Plastic moment", GH_ParamAccess.list);
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
            List<double> plasticMoments = new List<double>();
            DA.GetData(0,ref cMesh);
            DA.GetDataList(1, lines);
            DA.GetDataList(2, foldAngles);
            DA.GetDataList(3, strengths);
            DA.GetDataList(4, plasticMoments);
            if (foldAngles.Count != lines.Count)
            {
                for (int i = foldAngles.Count; i < lines.Count; i++)
                {
                    foldAngles.Add(foldAngles[0]);
                }
            }

            PlasticMoment constraint = new PlasticMoment(cMesh, lines.ToArray(), foldAngles.ToArray(),
                strengths.ToArray(), plasticMoments.ToArray());
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
            get { return new Guid("85386fa1-f672-436b-8145-cb05bce746f3"); }
        }
    }
}