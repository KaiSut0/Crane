using System;
using System.Collections.Generic;
using Crane.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Inputs
{
    public class CMeshFromMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CMeshFromMesh class.
        /// </summary>
        public CMeshFromMesh()
          : base("CMesh from Mesh", "CMesh from Mesh",
              "Create the CMesh instance from the mesh",
              "Crane", "Inputs")
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
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            if (!DA.GetData(0, ref mesh)) { return; }

            CMesh cmesh = new CMesh(mesh);

            DA.SetData(0, cmesh);
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
                return Properties.Resource.cmesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f0764ced-96f9-42ea-92b3-8e835be077ee"); }
        }
    }
}