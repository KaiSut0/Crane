using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Inputs
{
    public class ConstructCMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CMeshFromMVLinesWithDevelopment class.
        /// </summary>
        public ConstructCMesh()
          : base("Construct CMesh", "Construct CMesh",
              "Create the CMesh instance from a mesh and mountain and valley lines with development.",
              "Crane", "Inputs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddLineParameter("Mountain Crease Lines", "M", "Mountain Crease Lines List", GH_ParamAccess.list);
            pManager.AddLineParameter("Valley Crease Lines", "V", "Valley Crease Lines List", GH_ParamAccess.list);
            pManager.AddLineParameter("Triangulated Crease Lines", "T", "Flat Panel Diagonal Lines List", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Develop", "Develop", "Develop", GH_ParamAccess.item, false);
            pManager.AddPointParameter("DevelopmentOrigin", "DevOrigin", "Origin of the development", GH_ParamAccess.item);
            pManager.AddNumberParameter("DevelopmentRotation", "DevRotation", "Rotation angle for development", GH_ParamAccess.item, 0);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("Development", "Dev", "Development", GH_ParamAccess.item);
            pManager.AddGenericParameter("Constraint", "C", "Developable Constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            List<Line> m = new List<Line>();
            List<Line> v = new List<Line>();
            List<Line> t = new List<Line>();
            bool doDevelop = false;
            Point3d devOrigin = new Point3d();
            double devRotation = 0;

            if (!DA.GetData(0, ref mesh)) { return; }
            DA.GetDataList(1, m);
            DA.GetDataList(2, v);
            DA.GetDataList(3, t);
            DA.GetData(4, ref doDevelop);
            if (!DA.GetData(5, ref devOrigin))
            {
                if (doDevelop)
                {
                    BoundingBox bb = mesh.GetBoundingBox(true);
                    double offset = bb.Max.X - bb.Min.X;
                    devOrigin = new Point3d(bb.Max.X + offset, 0, 0);
                }
            }
            DA.GetData(6, ref devRotation);

            CMesh cMesh = new CMesh();

            if (doDevelop)
            {
                cMesh = new CMesh(mesh, m, v, t, new Point2d(devOrigin.X, devOrigin.Y), devRotation);
            }
            else
            {
                cMesh = new CMesh(mesh, m, v, t);
            } 
            
            DA.SetData(0, cMesh);
            if (doDevelop) DA.SetData(1, cMesh.Development());
            if (doDevelop) DA.SetData(2, new DevelopableWithDevelopment(cMesh, cMesh.DevelopmentEdgeIndexPairs.ToArray()));

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
            get { return new Guid("facbb5c3-d68b-4d17-815a-3dcc5f824395"); }
        }
    }
}