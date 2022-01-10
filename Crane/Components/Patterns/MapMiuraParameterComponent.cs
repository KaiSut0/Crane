using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Misc;

namespace Crane.Components.Patterns
{
    public class MapMiuraParameterComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MapMiuraParameterComponent class.
        /// </summary>
        public MapMiuraParameterComponent()
          : base("MapMiuraParameter", "MapMiuraParameter",
              "Compute Miura-ori parameters from 2 rectangle length.",
              "Crane", "Pattern")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("xA", "xA", "xA", GH_ParamAccess.item);
            pManager.AddNumberParameter("yA", "yA", "yA", GH_ParamAccess.item);
            pManager.AddNumberParameter("xB", "xB", "xB", GH_ParamAccess.item);
            pManager.AddNumberParameter("yB", "yB", "yB", GH_ParamAccess.item);
            pManager.AddNumberParameter("Theta", "t", "Sector angle of Miura parallelogram.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("a", "a", "Horizontal length of Miura parallelogram.", GH_ParamAccess.list);
            pManager.AddNumberParameter("b", "b", "Oblique length of Miura parallelogram.", GH_ParamAccess.list);
            pManager.AddNumberParameter("rhoA", "rhoA", "A fold angle of Miura-ori A state.", GH_ParamAccess.list);
            pManager.AddNumberParameter("rhoB", "rhoB", "A fold angle of Miura-ori B state.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double xA = 0;
            double yA = 0;
            double xB = 0;
            double yB = 0;
            double t = 0;
            DA.GetData("xA", ref xA);
            DA.GetData("yA", ref yA);
            DA.GetData("xB", ref xB);
            DA.GetData("yB", ref yB);
            DA.GetData("Theta", ref t);

            var mapMiura = new MapMiuraParameter(xA, yA, xB, yB, t);
            DA.SetDataList("a", mapMiura.a);
            DA.SetDataList("b", mapMiura.b);
            DA.SetDataList("rhoA", mapMiura.rhoA);
            DA.SetDataList("rhoB", mapMiura.rhoB);
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
            get { return new Guid("17a0dd11-bff5-4df6-95c6-0fc33ce0ebac"); }
        }
    }
}