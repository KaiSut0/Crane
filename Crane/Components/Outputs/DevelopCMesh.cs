using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;

namespace Crane.Components.Outputs
{
    public class DevelopCMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DevelopCMesh class.
        /// </summary>
        public DevelopCMesh()
          : base("Develop CMesh", "Dev CMesh",
              "Develop CMesh if possible",
              "Crane", "Outputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Origin", "Origin", "Development origin.", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddNumberParameter("Rotation Angle", "Rot Ang", "Development rotation angle.", GH_ParamAccess.item,
                0);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Development", "Dev", "Development", GH_ParamAccess.item);
            pManager.AddLineParameter("Mountain", "M", "Mountain edges.", GH_ParamAccess.list);
            pManager.AddLineParameter("Valley", "V", "Valley edges.", GH_ParamAccess.list);
            pManager.AddLineParameter("Boundary", "B", "Boundary edges.", GH_ParamAccess.list);
            pManager.AddLineParameter("Triangulated", "T", "Triangulated edges.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cmesh = new CMesh();
            Point3d origin = Point3d.Origin;
            double rot = 0;

            DA.GetData(0, ref cmesh);
            DA.GetData(1, ref origin);
            DA.GetData(2, ref rot);

            var dev = cmesh.GetDevelopment(origin, rot);
            var m = cmesh.GetLines(dev, cmesh.mountain_edges);
            var v = cmesh.GetLines(dev, cmesh.valley_edges);
            var b = cmesh.GetLines(dev, cmesh.boundary_edges);
            var t = cmesh.GetLines(dev, cmesh.triangulated_edges);

            DA.SetData(0, dev);
            DA.SetDataList(1, m);
            DA.SetDataList(2, v);
            DA.SetDataList(3, b);
            DA.SetDataList(4, t);
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
                return Properties.Resource.develop_mesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c7be94de-ca1a-438d-b0fc-2ccc4a08a347"); }
        }
    }
}