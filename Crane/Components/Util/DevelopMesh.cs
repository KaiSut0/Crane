using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Crane.Components.Util
{
    public class DevelopMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DevelopMesh class.
        /// </summary>
        public DevelopMesh()
          : base("Develop Mesh", "Develop Mesh",
              "Develop mesh.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to develop.", GH_ParamAccess.item);
            pManager.AddVectorParameter("DevelopOrigin", "DevOrigin", "Development origin", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("DevelopMesh", "DM", "Developed mesh.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            Mesh devMesh = new Mesh();
            Vector3d devOrigin = new Vector3d();
            DA.GetData(0, ref mesh);
            if (!DA.GetData(1, ref devOrigin))
            {
                BoundingBox bb = mesh.GetBoundingBox(true);
                double offset = bb.Max.X - bb.Min.X;
                devOrigin = new Vector3d(bb.Max.X + offset, 0, 0);
            }
            var dev = new Core.DevelopMesh(mesh, new Point2d(devOrigin.X, devOrigin.Y));
            devMesh = dev.DevelopedMesh;
            DA.SetData(0, devMesh);
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
            get { return new Guid("2070e612-597f-4ed7-8821-b4548c5d9c88"); }
        }
    }
}