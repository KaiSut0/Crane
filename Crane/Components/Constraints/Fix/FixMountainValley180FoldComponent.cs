﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Crane.Constraints;

namespace Crane.Components.Constraints.Fix
{
    public class FixMountainValley180FoldComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FixMountainValley180Fold class.
        /// </summary>
        public FixMountainValley180FoldComponent()
          : base("Fix Mountain Valley 180 Fold", "Fix 180 Fold",
              "Fix fold angles 180 degree for mountain and valley.",
              "Crane", "Constraints")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "180 Fold angle fixing constraint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            DA.SetData(0, new MountainValley180Fold());

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
                return Properties.Resource.icons_fix_face_normal_65;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8BACC235-A99E-4FF7-8DDD-05AEFC086C15"); }
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

    }
}