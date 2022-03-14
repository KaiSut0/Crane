using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Constraints;
using Crane.Core;

namespace Crane.Components.Constraints
{
    public class FixSectorAngleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixSectorAngleComponent class.
        /// </summary>
        public FixSectorAngleComponent()
          : base("Fix Sector Angle", "Fix Sector Angle",
              "Set the constraint to fix the selected sector angle to the goal angle.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Left Pt", "Left Pt", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Center Pt", "Center Pt", "Centor Pt", GH_ParamAccess.list);
            pManager.AddPointParameter("Right Pt", "Right Pt", "Right Pt", GH_ParamAccess.list);
            pManager.AddNumberParameter("Sector Angle", "Secotr Angle", "The fixing sector angles.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stiffness", "Stiffness", "Stiffness", GH_ParamAccess.list, 1);
            pManager[5].Optional = true;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Fold angle fixing constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            List<Point3d> left = new List<Point3d>();
            List<Point3d> center = new List<Point3d>();
            List<Point3d> right = new List<Point3d>();
            List<double> sectorAngles = new List<double>();
            List<double> stiffness = new List<double>();
            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, left);
            DA.GetDataList(2, center);
            DA.GetDataList(3, right);
            DA.GetDataList(4, sectorAngles);
            DA.GetDataList(5, stiffness);
            if (sectorAngles.Count != left.Count)
            {
                for (int i = sectorAngles.Count; i < left.Count; i++)
                {
                    sectorAngles.Add(sectorAngles[0]);
                }
            }
            if (stiffness.Count != left.Count)
            {
                for (int i = stiffness.Count; i < left.Count; i++)
                {
                    stiffness.Add(stiffness[0]);
                }
            }

            FixSectorAngle constraint = new FixSectorAngle(cMesh, center.ToArray(), left.ToArray(), right.ToArray(),
                sectorAngles.ToArray(), stiffness.ToArray());

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
            get { return new Guid("de3b8b89-f523-4e3a-b289-ef2356729315"); }
        }
    }
}