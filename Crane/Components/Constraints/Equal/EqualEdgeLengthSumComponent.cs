using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Constraints.Equal
{
    public class EqualEdgeLengthSumComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EqualEdgeLengthSumComponent class.
        /// </summary>
        public EqualEdgeLengthSumComponent()
          : base("Equal Edge Length Sum", "Equal Edge Length Sum",
              "Set equal edge length sum constraint.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddLineParameter("First Edges", "First Edges", "First edges to sum of length.",
                GH_ParamAccess.list);
            pManager.AddLineParameter("Second Edges", "Second Edges", "Second edges to sum of length.",
                GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Equal edge length sum constraint.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            List<Line> firstLines = new List<Line>();
            List<Line> secondLines = new List<Line>();
            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, firstLines);
            DA.GetDataList(2, secondLines);
            DA.SetData(0, new EqualEdgeLengthSum(cMesh, firstLines.ToArray(), secondLines.ToArray()));
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
                return Properties.Resource.icons_equal_edge_length_55;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("DB0DD1BC-9D77-4CCF-84BF-4D357803A5E5"); }
        }
    }
}