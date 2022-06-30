using System;
using System.Collections.Generic;
using Crane.Patterns.TessellationOnSurface;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Patterns
{
    public class MiuraFoldingOnDoubleSurfaceComponent2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MiuraFoldingOnDoubleSurfaceComponent2 class.
        /// </summary>
        public MiuraFoldingOnDoubleSurfaceComponent2()
          : base("Miura Folding On Double Surface 2", "Miura Folding On Double Surface 2",
              "Generate Miura-folding between two surfaces.",
              "Crane", "Pattern")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("S1", "S1", "The first surface.", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("S2", "S2", "The second surface.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "U", "The U division count.", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("V", "V", "The V division count.", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("SlideParam", "SlideParam", "Slide parameter for Miura-Ori",
                GH_ParamAccess.item, 0.25);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface s1 = null;
            Surface s2 = null;
            int u = 1;
            int v = 1;
            double slide = 0.25;
            DA.GetData(0, ref s1);
            DA.GetData(1, ref s2);
            DA.GetData(2, ref u);
            DA.GetData(3, ref v);
            DA.GetData(4, ref slide);
            DA.SetData(0,
                new MiuraFoldingOnDoubleSurface(s1, s2, u, v, new MiuraFoldingParam() { SlideParam = slide }).Tessellation);

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
            get { return new Guid("CE6AEC5A-FCA0-41AF-A3A9-DCB951339972"); }
        }
    }
}