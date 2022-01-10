using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Util
{
    public class GetFoldAngles : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetFoldAngles class.
        /// </summary>
        public GetFoldAngles()
          : base("GetFoldAngles", "GetFoldAngles",
              "Get fold angles.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("FoldAngles", "FA", "Fold angles.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);

            CMesh cMesh = new CMesh(mesh);
            var foldAngles = cMesh.GetFoldAngles();

            DA.SetDataList(0, foldAngles);
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
            get { return new Guid("9ed33750-5d70-449c-bdc1-6e3d717d2f62"); }
        }
    }
}