using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;
using Rhino;

namespace Crane.Components.Constraints
{
    public class MirrorSymmetryComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VertexMirrorConstraintComponent class.
        /// </summary>
        public MirrorSymmetryComponent()
          : base("Mirror Symmetry", "Mirror Symmetry",
              "Set mirror symmetric constraint.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.septenary; 
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
            pManager.AddGenericParameter("Constraint", "Constraint", "Mirror symmetric Constraint", GH_ParamAccess.item);
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

            var constraint = new MirrorSymmetry(cMesh, plane, tolerance);

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
                return Properties.Resource.icons_mirror_symmetry;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("32003720-c006-42e6-9446-1eccfb3a8c83"); }
        }


    }
}