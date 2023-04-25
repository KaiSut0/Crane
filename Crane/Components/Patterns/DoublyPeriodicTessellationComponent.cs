using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Patterns;

namespace Crane.Components.Patterns
{
    public class DoublyPeriodicTessellationComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DoublyPeriodicTessellationComponent class.
        /// </summary>
        public DoublyPeriodicTessellationComponent()
          : base("Doubly Periodic Tessellation", "Doubly Periodic Tessellation",
              "Create doubly periodic tessellation CMesh.",
              "Crane", "Pattern")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("CornerVertex", "CornerVertex", "CornerVertex", GH_ParamAccess.item);
            pManager.AddPointParameter("S1EdgeVertices", "S1EdgeVertices", "Edge vertices transformed by S1.", GH_ParamAccess.list);
            pManager.AddPointParameter("S2EdgeVertices", "S2EdgeVertices", "Edge vertices transformed by S2.", GH_ParamAccess.list);
            pManager.AddPointParameter("InnerVertices", "InnerVertices", "Inner vertices.", GH_ParamAccess.list);
            pManager.AddMeshFaceParameter("Faces", "Faces", "Facces", GH_ParamAccess.list);
            pManager.AddTransformParameter("S1", "S1", "S1 transform. This must be commutative to S2.", GH_ParamAccess.item);
            pManager.AddTransformParameter("S2", "S2", "S2 transform. This must be commutative to S1.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Copy", "Copy", "Copy or not to 2 x 2.", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddGenericParameter("Constraints", "Constraints", "Constraints", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cornerVertex = new Point3d();
            var s1EdgeVertices = new List<Point3d>();
            var s2EdgeVertices = new List<Point3d>();
            var innerVertices = new List<Point3d>();
            var faces = new List<MeshFace>();
            var s1 = new Transform();
            var s2 = new Transform();
            var copy = false;

            DA.GetData(0, ref cornerVertex);
            DA.GetDataList(1, s1EdgeVertices);
            DA.GetDataList(2, s2EdgeVertices);
            DA.GetDataList(3, innerVertices);
            DA.GetDataList(4, faces);
            DA.GetData(5, ref s1);
            DA.GetData(6, ref s2);
            DA.GetData(7, ref copy);

            var tessellation = new DoublyPeriodicTessellation(cornerVertex, s1EdgeVertices, s2EdgeVertices,
                innerVertices, faces, s1, s2, copy);

            DA.SetData(0, tessellation.CMesh);
            DA.SetDataList(1, tessellation.Constraints);

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
            get { return new Guid("92098d61-1c16-425d-ac71-a2aeab2e0cc7"); }
        }
    }
}