using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;
using Rhino;

namespace Crane.Components.Constraints
{
    public class TranslationalSymmetryComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TranslationalSymmetryComponent class.
        /// </summary>
        public TranslationalSymmetryComponent()
          : base("Translation Symmetry", "Translation Symmetry",
              "Set translational symmetric constraint.",
              "Crane", "Const-Symmetry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddVectorParameter("Translation", "Translation", "Translation vector.", GH_ParamAccess.item);
            pManager.AddVectorParameter("GoalTranslation", "GoalTranslation", "Goal translation.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 1e-3);
            pManager.AddBooleanParameter("DevelopSymmetryOn", "OnDevelopSymmetryOn", "Development symmetry on.", GH_ParamAccess.item, false);
            pManager.AddVectorParameter("DevelopTranslation", "DevelopTranslation", "Develop translation", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraints", "Constraints", "Translational symmetry constraint.",
                GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Vector3d translation = new Vector3d();
            Vector3d goalTranslation = new Vector3d();
            double tolerance = 1e-3;
            bool devSymOn = false;
            Vector3d devTranslation = new Vector3d();

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref translation);
            DA.GetData(2, ref goalTranslation);
            DA.GetData(3, ref tolerance);
            DA.GetData(4, ref devSymOn);
            DA.GetData(5, ref devTranslation);

            var constraints = new List<Constraint>();

            var constraint = new TranslationalSymmetry(cMesh, translation, goalTranslation, tolerance);
            constraints.Add(constraint);
            if (devSymOn)
            {
                List<IndexPair> devIndexPair =
                    Utils.OffsetIndexPairs(constraint.indexPairs, cMesh.NumberOfVertices / 2);
                var devConstraint = new TranslationalSymmetry(cMesh, devTranslation, devIndexPair);
                constraints.Add(devConstraint);
            }

            DA.SetDataList(0, constraints);

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
                return Properties.Resource.icons_translation_symmetry;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e29cabb2-a762-4cf1-9f0b-9321d3606dbb"); }
        }
    }
}