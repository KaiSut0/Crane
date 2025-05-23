﻿using System;
using System.Collections.Generic;
using Crane.Patterns.TessellationOnSurface;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Patterns
{
    public class HuffmanStarsTrianglesOnDoubleSurfaceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public HuffmanStarsTrianglesOnDoubleSurfaceComponent()
          : base("Huffman Stars-Triangles On Double Surface", "Huffman Stars-Triangles On Double Surface",
              "Generate Huffman Stars-Triangles between two surfaces.",
              "Crane", "Pattern")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("S1", "S1", "The first surface.", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("S2", "S2", "The second surface.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "U", "The U division count.", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("V", "V", "The V division count.", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("SlideParam1", "SlideParam1", "The first slide parameter for Huffman Stars-Triangles",
                GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("SlideParam2", "SlideParam2", "The second slide parameter for Huffman Stars-Triangles",
                GH_ParamAccess.item, 0.25);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
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
            double slide1 = 0.1;
            double slide2 = 0.25;
            DA.GetData(0, ref s1);
            DA.GetData(1, ref s2);
            DA.GetData(2, ref u);
            DA.GetData(3, ref v);
            DA.GetData(4, ref slide1);
            DA.GetData(5, ref slide2);
            DA.SetData(0,
                new HuffmanStarsTrianglesOnDoubleSurface(s1, s2, u, v, new HuffmanStarsTrianglesParam() { SlideParam1 = slide1, SlideParam2 = slide2}).Tessellation);

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
                return Properties.Resource.icons_haffman_star_triangle_srf;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("59530D27-9D64-466A-A7F7-E75849A92EE1"); }
        }
    }
}