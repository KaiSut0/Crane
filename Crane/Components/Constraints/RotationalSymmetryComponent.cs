using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class RotationalSymmetryComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RotationalSymmetricConstraintComponent class.
        /// </summary>
        public RotationalSymmetryComponent()
          : base("RotationalSymmetric", "RotationSymmetry",
              "Set vertex rotational symmetric constraint.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddLineParameter("RotationAxis","RotAxis","Rotation axis.",GH_ParamAccess.item);
            pManager.AddAngleParameter("RotationAngle","RotAng","Rotation angle.",GH_ParamAccess.item);
            pManager.AddNumberParameter("tolerance", "tolerance", "tolerance", GH_ParamAccess.item, 1e-3);
            pManager.AddIntegerParameter("FirstIndices", "FirstIndices", "Specify the index pairs for selecting constraint points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("SecondIndices", "SecondIndices", "Specify the index pairs for selecting constraint points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("FixIndices", "FixIndices", "Specify the index pairs for selecting constraint points", GH_ParamAccess.list);
            
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
            pManager.AddGenericParameter("RotationalConstraint","RotConst","Rotational symmetric vertex constraint.",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = new CMesh();
            Line line = new Line();
            double angle = Math.PI / 2;
            double tolerance = 1e-3;

            var firstIndices = new List<int>();
            var secondIndices = new List<int>();
            var fixIndices = new List<int>();
            bool hasFirstIndices = false;
            bool hasSecondIndices = false;
            bool hasFixIndices = false;
            bool isIndexMode = false;

            Constraint constraint = null;

            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref line);
            DA.GetData(2, ref angle);
            DA.GetData(3, ref tolerance);
            if (DA.GetDataList(4, firstIndices)) hasFirstIndices = true;
            if (DA.GetDataList(5, secondIndices)) hasSecondIndices = true;
            if (DA.GetDataList(6, fixIndices)) hasFixIndices = true;

            isIndexMode = (hasFirstIndices & hasSecondIndices) || hasFixIndices;
            if (isIndexMode)
            {
                Transform rotation = Transform.Rotation(angle, line.Direction, line.From);
                var indexPairs = Utils.MergeIndexPairs(firstIndices, secondIndices);
                constraint = new TransformSymmetry(cMesh, rotation, indexPairs, line, fixIndices);
            }
            else
            {
                constraint = new RotationalSymmetry(cMesh, line, angle, tolerance);
            }

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
            get { return new Guid("91f50e19-f724-4515-aa71-a78c6362ae1d"); }
        }
    }
}