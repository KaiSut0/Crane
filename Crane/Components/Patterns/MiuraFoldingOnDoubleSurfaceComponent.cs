﻿using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Patterns;

namespace Crane.Components.Patterns
{
    public class MiuraFoldingOnDoubleSurfaceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MiuraFoldingOnDoubleSurface class.
        /// </summary>
        public MiuraFoldingOnDoubleSurfaceComponent()
          : base("Miura Folding On Double Surface", "Miura Folding On Surf",
              "Create Miura folding tessellation between double surfaces.",
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
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Miura-folding tessellation.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface S1 = null;
            Surface S2 = null;
            int U = 4;
            int V = 4;
            DA.GetData(0, ref S1);
            DA.GetData(1, ref S2);
            DA.GetData(2, ref U);
            DA.GetData(3, ref V);
            var miura = new MiuraFoldingOnDoubleSurface(S1, S2, U, V);
            DA.SetData(0, miura.Tessellation);

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
                return Properties.Resource.icons_miura_srf;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6dd48879-42f8-4f7a-a5d3-102b07eeaf30"); }
        }
    }
}