using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane.Core;
using Crane.Constraints;

namespace Crane.Components.Constraints.Fix
{
    public class FixSVDirectionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixSVDirectionComponent class.
        /// </summary>
        public FixSVDirectionComponent()
          : base("Fix SV Direction", "Fix SV Dir",
              "Fix singular value direction of deformation tensor.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh that is object to simulation.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Original CMesh", "Orig CMesh", "Original shape of CMesh that is reference of deformation.", GH_ParamAccess.item);
            pManager.AddPointParameter("Face Points", "Face Points", "Add face center points for applying constraint whose mesh is simulated.", GH_ParamAccess.list);
            pManager.AddPointParameter("Original Face Points", "Orig Face Points", "Add face center points for applying constraint whose mesh is compared to simulation mesh.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Goal Angle", "Goal Angle", "Goal angle of the singular value direction.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "Strength of this constraint.", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Fix singular value direction constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            CMesh cMeshOrig = null;
            List<Point3d> facePoints = new List<Point3d>();
            List<Point3d> FacePoints = new List<Point3d>();
            List<double> goalAngles = new List<double>();
            double strength = 1;


            DA.GetData(0, ref cMesh);
            DA.GetData(1, ref cMeshOrig);
            DA.GetDataList(2, facePoints);
            DA.GetDataList(3, FacePoints);
            DA.GetDataList(4, goalAngles);
            DA.GetData(5, ref strength);

            if (goalAngles.Count != facePoints.Count)
            {
                double goalAngle = goalAngles[0];
                goalAngles = new List<double>();
                for(int i = 0; i < facePoints.Count; i++)
                {
                    goalAngles.Add(goalAngle);
                }
            }

            var constraint = new FixSVDirection(cMesh, cMeshOrig, facePoints.ToArray(), FacePoints.ToArray(), goalAngles.ToArray(), strength);

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
            get { return new Guid("7A5806B2-8440-47A8-A48C-A199ACF81784"); }
        }
    }
}