using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;
using Crane.Constraints;

namespace Crane.Components.Constraints
{
    public class GlueVerticesComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GlueVerticesComponent class.
        /// </summary>
        public GlueVerticesComponent()
          : base("Glue Vertices", "Glue Vertices",
              "Set glue the vertices constraint.",
              "Crane", "Const-Glue")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("GluePt1", "GluePt1", "GluePt1", GH_ParamAccess.item);
            pManager.AddPointParameter("GluePt2", "GluePt2", "GluePt2", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 1e-3);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Glue Vertices Constraint", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Point3d pt1 = new Point3d();
            Point3d pt2 = new Point3d();
            double tolerance = 1e-3;

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref pt1);
            DA.GetData(2, ref pt2);
            DA.GetData(3, ref tolerance);

            var constraint = new GlueVertices(cMesh, pt1, pt2, tolerance);

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
                return Properties.Resource.icons_glue_vertices;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5808c4f7-91d4-465e-9cd4-08ed96e32e3e"); }
        }
    }
}