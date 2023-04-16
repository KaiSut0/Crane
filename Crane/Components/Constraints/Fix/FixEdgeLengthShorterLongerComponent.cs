using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Constraints.Fix
{
    public class FixEdgeLengthShorterLongerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixEdgeLengthShorterLongerComponent class.
        /// </summary>
        public FixEdgeLengthShorterLongerComponent()
          : base("Fix Edge Length Shorter Longer", "Fix Edge Length SL",
              "Fix edge length constraint with switching a stiffness when an edge length is shorter or longer than a goal length.",
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
            pManager.AddNumberParameter("Edge Length", "Edge Length", "The goal edge length.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Shorter Stiffness", "Shorter Stiffness", "Set stiffness when an edge length is shorter than a goal edge length", GH_ParamAccess.list);
            pManager.AddNumberParameter("Longer Stiffness", "Longer Stiffness", "Set stiffness when an edge length is longer than a goal edge length", GH_ParamAccess.list);
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
            List<double> shorterStiffnesses = new List<double>();
            List<double> longerStiffnesses = new List<double>();
            DA.GetData(0,ref cMesh);
            DA.GetDataList(1, lines);
            DA.GetDataList(2, edgeLengths);
            DA.GetDataList(3, shorterStiffnesses);
            DA.GetDataList(4, longerStiffnesses);
            if (edgeLengths.Count != lines.Count)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    edgeLengths.Add(edgeLengths[0]);
                }
            }
            if (shorterStiffnesses.Count != lines.Count)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    shorterStiffnesses.Add(shorterStiffnesses[0]);
                }
            }
            if (longerStiffnesses.Count != lines.Count)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    longerStiffnesses.Add(longerStiffnesses[0]);
                }
            }
           

            FixEdgeLength constraint = new FixEdgeLength(cMesh, lines.ToArray(), edgeLengths.ToArray(), longerStiffnesses.ToArray(), shorterStiffnesses.ToArray());
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
            get { return new Guid("141436BE-02F9-4579-AB15-BD73F1283F6F"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}