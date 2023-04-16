using System;
using System.Collections.Generic;
using System.ComponentModel;
using Crane.Core;
using Crane.Constraints;
using Grasshopper.Kernel;
using OpenCvSharp.CPlusPlus.Detail;
using Rhino.Geometry;

namespace Crane.Components.Constraints.Fix
{
    public class FixAreaComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixAreaComponent class.
        /// </summary>
        public FixAreaComponent()
          : base("Fix Area", "Fix Area",
              "Set fix area constraint.",
              "Crane", "Constraints")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "CMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Face Center", "Face Center", "Face centers to detect the face to fix its area.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Goal Area", "Goal Area", "Goal areas.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stiffness", "Stiffness", "Stiffness", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Constraint", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CMesh cMesh = null;
            List<Point3d> faceCenters = new List<Point3d>();
            List<double> goalAreas = new List<double>();
            List<double> stiffnesses = new List<double>();

            DA.GetData(0, ref cMesh);
            DA.GetDataList(1, faceCenters);
            DA.GetDataList(2, goalAreas);
            DA.GetDataList(3, stiffnesses);

            if(goalAreas.Count < faceCenters.Count)
            {
                for(int i = goalAreas.Count; i < faceCenters.Count; i++)
                {
                    goalAreas.Add(goalAreas[0]);
                }
            }
            if(stiffnesses.Count < faceCenters.Count)
            {
                for (int i = stiffnesses.Count; i < faceCenters.Count; i++)
                {
                    stiffnesses.Add(stiffnesses[0]);
                }
            }

            DA.SetData(0, new FixArea(cMesh, faceCenters, goalAreas, stiffnesses));
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
            get { return new Guid("85E00A56-AE66-4D56-AFC8-FBBA6F1A96A2"); }
        }
    }
}