using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class EdgeLengthMirrorComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EdgeLengthMirrorConstraint class.
        /// </summary>
        public EdgeLengthMirrorComponent()
          : base("EdgeLengthMirrorConstraint", "EdgeMirrorConst",
              "Set edge length mirror symmetric constraint.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPlaneParameter("MirrorPlane","MirrorPlane","MirrorPlane",GH_ParamAccess.item);
            pManager.AddNumberParameter("tolerance", "tolerance", "tolerance", GH_ParamAccess.item, 1e-3);
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mirror Vertex Constraint", "Mirror Vertex Constraint", "Mirror Vertex Constraint", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Plane plane = new Plane();
            double tolerance = 1e-3;
            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref plane);
            DA.GetData(2, ref tolerance);

            var constraint = new MirrorSymmetricEdgeLength(cMesh, plane, tolerance);
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
            get { return new Guid("8e63886e-2902-4dfd-aca2-4b9524f6cbb7"); }
        }
    }
}