using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Util
{
    public class DevelopCMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DevelopCMesh class.
        /// </summary>
        public DevelopCMesh()
          : base("DevelopCMesh", "DevelopCMesh",
              "Develop crane mesh.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CM", "CMesh to develop.", GH_ParamAccess.item);
            pManager.AddVectorParameter("DevelopOrigin", "DevOrigin", "Development origin", GH_ParamAccess.item);
            pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DevelopCMesh", "DCM", "Developed crane mesh.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Mesh devMesh = new Mesh();
            Vector3d devOrigin = new Vector3d();
            DA.GetData(0, ref cMesh);
            if (!DA.GetData(1, ref devOrigin))
            {
                BoundingBox bb = cMesh.Mesh.GetBoundingBox(true);
                double offset = bb.Max.X - bb.Min.X;
                devOrigin = new Vector3d(bb.Max.X + offset, 0, 0);
            }

            var dev = new Core.DevelopMesh(cMesh.Mesh, new Point2d(devOrigin.X, devOrigin.Y));
            devMesh = dev.DevelopedMesh;
            CMesh devCMesh = new CMesh(devMesh, cMesh.mountain_edges, cMesh.valley_edges, cMesh.triangulated_edges);
            DA.SetData(0, devCMesh);
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
            get { return new Guid("fb7721c4-4649-4ba6-ac94-f1f127d4acf0"); }
        }
    }
}