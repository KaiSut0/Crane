using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class FixEdgeLengthComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixEdgeLengthComponent class.
        /// </summary>
        public FixEdgeLengthComponent()
          : base("Fix Edge Length", "Fix Edge Length",
              "Fix edge length component.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddLineParameter("Line", "Line", "The lines for detecting the edge to fix the fold angle.", GH_ParamAccess.list);
            pManager.AddNumberParameter("EdgeLength", "EdgeLength", "The edge length.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "The tolerance for detecting the edge from given line.", GH_ParamAccess.item, 1e-3);
            pManager[3].Optional = true;

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
            List<Line> lines = new List<Line>();
            List<double> edgeLengths = new List<double>();
            double tolerance = 1e-3;
            DA.GetData(0,ref cMesh);
            DA.GetDataList(1, lines);
            DA.GetDataList(2, edgeLengths);
            DA.GetData(3, ref tolerance);
            if (edgeLengths.Count != lines.Count)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    edgeLengths.Add(edgeLengths[0]);
                }
            }

            FixEdgeLength constraint = new FixEdgeLength(cMesh, lines.ToArray(), edgeLengths.ToArray(), tolerance);
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
            get { return new Guid("a3bc72a6-6f4c-4fe5-be59-b8670cf24864"); }
        }
    }
}