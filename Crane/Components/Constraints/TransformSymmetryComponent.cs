using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class TransformSymmetryComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TranslationalSymmetryForDevelopmentComponent class.
        /// </summary>
        public TransformSymmetryComponent()
          : base("TransformSymmetry", "TrasformSymmetry",
              "Set transform symmetric constraint.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddTransformParameter("Transform", "Transform", "Transform", GH_ParamAccess.item);
            pManager.AddIntegerParameter("First Index", "First Index", "First index", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Second Index", "Second Index", "Second index", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Translational symmetry constraint.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Transform transform = new Transform();
            List<int> firstIndices = new List<int>();
            List<int> secondIndices = new List<int>();

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref transform);
            DA.GetDataList(2, firstIndices);
            DA.GetDataList(3, secondIndices);

            TransformSymmetry constraint =
                new TransformSymmetry(cMesh, transform, Utils.MergeIndexPairs(firstIndices, secondIndices));

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
            get { return new Guid("489492c7-bd2f-4da7-89a7-7555e1c1024d"); }
        }
    }
}