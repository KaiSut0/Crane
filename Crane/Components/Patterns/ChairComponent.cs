using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Core;
using Crane.Patterns;

namespace Crane.Components.Patterns
{
    public class ChairComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Chair class.
        /// </summary>
        public ChairComponent()
          : base("Chair", "Chair",
              "Create chair pattern template",
              "Crane", "Pattern")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Height", "Height", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Panel Size", "Panel Size", "", GH_ParamAccess.item);
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
            CMesh cMesh = new CMesh();
            List<Constraint> consts = new List<Constraint>();

            double height = 10;
            double panelSize = 10;

            DA.GetData(0, ref height);
            DA.GetData(1, ref panelSize);

            Chair chair = new Chair(height, panelSize);

            DA.SetData(0, chair.CMesh);
            DA.SetDataList(1, chair.Constraints);
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
                return Properties.Resource.icons_chair;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("607ed827-658c-4749-bf32-ca9afa0b289a"); }
        }
    }
}