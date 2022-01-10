using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Util
{
    public class AutoAssignMVLines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AutoAssignMVLines class.
        /// </summary>
        public AutoAssignMVLines()
          : base("Auto Assign MV Lines", "Auto Assign MV Lines",
              "Automatically assign mountain and valley foldlines.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Mountain", "M", "Mountain foldlines.", GH_ParamAccess.list);
            pManager.AddLineParameter("Valley", "V", "Valley foldlines.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            DA.GetData(0, ref mesh);
            CMesh cmesh = new CMesh(mesh);
            var mv = cmesh.GetAutomaticallyAssignedMVLines();
            DA.SetDataList(0, mv.Item1);
            DA.SetDataList(1, mv.Item2);
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
                return Properties.Resource.auto_assign_mv;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fbfc1405-7bac-4abb-8569-827fb2163e0a"); }
        }
    }
}