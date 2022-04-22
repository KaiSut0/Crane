using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class EqualEdgeLengthComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EqualEdgeLengthComponent class.
        /// </summary>
        public EqualEdgeLengthComponent()
          : base("Equal Edge Length", "Equal Edge Length",
              "Equal edge length constraint.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddLineParameter("FirstLine", "FirstLine", "The first lines for detecting the edge.", GH_ParamAccess.list);
            pManager.AddLineParameter("SecondLine", "SecondLine", "The second lines for detecting the edge.", GH_ParamAccess.list);
            pManager.AddNumberParameter("LengthRatio", "LengthRatio", "The length ratio between first and second edges.", GH_ParamAccess.list, 1.0);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Equal edge length constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            List<Line> firstEdges = new List<Line>();
            List<Line> secondEdges = new List<Line>();
            List<double> lengthRatios = new List<double>();

            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, firstEdges);
            DA.GetDataList(2, secondEdges);
            DA.GetDataList(3, lengthRatios);

            var constraint = new EqualEdgeLength(cMesh, firstEdges.ToArray(), secondEdges.ToArray(),
                lengthRatios.ToArray());

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
                return Properties.Resource.icons_equal_edge_length;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a44b8e0b-62a1-4c96-aacd-3ecb9747ee9d"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}