using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Patterns;

namespace Crane.Components.Patterns
{
    public class ChickenWireOnSurfaceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ChickenWireOnSurfaceComponent class.
        /// </summary>
        public ChickenWireOnSurfaceComponent()
          : base("ChickenWireOnSurface", "ChickenWireOnSurf",
              "Create chiken-wire tessellation on a surface.",
              "Crane", "Pattern")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("S", "S", "The surface.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "U", "The U division count.", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("V", "V", "The V division count.", GH_ParamAccess.item, 5);
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddMeshParameter("Mesh", "M", "Chicken wire tessellation.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface S = null;
            int U = 4;
            int V = 4;
            DA.GetData(0, ref S);
            DA.GetData(1, ref U);
            DA.GetData(2, ref V);
            var mesh = new ChickenWireOnSurface(S, S, U, V);
            DA.SetData(0, mesh.Tessellation);

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
            get { return new Guid("a10a36dc-922d-4d66-8745-28931d0b7af0"); }
        }
    }
}