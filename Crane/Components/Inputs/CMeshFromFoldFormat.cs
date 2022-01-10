using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Inputs
{
    public class CMeshFromFoldFormat : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CMeshFromFoldFormat class.
        /// </summary>
        public CMeshFromFoldFormat()
          : base("CMesh From Fold Format", "CMesh From Fold Format",
              "Create CMesh form fold format.",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "Path to fold format file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            DA.GetData(0, ref path);
            CMesh cMesh = FoldFormat.ReadFoldFormat(path).ToCMesh();
            DA.SetData(0, cMesh);
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
            get { return new Guid("0af375b2-25a7-4012-8873-4e4dc74746ba"); }
        }
    }
}