﻿using System;
using System.Collections.Generic;
using Crane.Patterns.TessellationOnSurface;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crane.Components.Patterns
{
    public class HuffmanRectWeaveOnDoubleSurfaceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HuffmanRectWeaveOnDoubleSurfaceComponent class.
        /// </summary>
        public HuffmanRectWeaveOnDoubleSurfaceComponent()
          : base("Huffman Rect Weave On Double Surface", "Huffman Rect Weave On Double Surface",
              "Generate Huffman Rect Weave between two surfaces.",
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
            pManager.AddNumberParameter("SlideParam1", "SlideParam1", "The first slide parameter for Huffman Rect Weave",
                GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("SlideParam2", "SlideParam2", "The second slide parameter for Huffman Rect Weave",
                GH_ParamAccess.item, 0.05);
            pManager.AddNumberParameter("SlideParam3", "SlideParam3", "The third slide parameter for Huffman Rect Weave",
                GH_ParamAccess.item, 0.1);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
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
            double slide1 = 0.3;
            double slide2 = 0.05;
            double slide3 = 0.1;
            DA.GetData(0, ref s1);
            DA.GetData(1, ref s2);
            DA.GetData(2, ref u);
            DA.GetData(3, ref v);
            DA.GetData(4, ref slide1);
            DA.GetData(5, ref slide2);
            DA.GetData(6, ref slide3);
            DA.SetData(0,
                new HuffmanRectWeaveOnDoubleSurface(s1, s2, u, v, new HuffmanRectWeaveParam() { SlideParam1 = slide1, SlideParam2 = slide2, SlideParam3 = slide3}).Tessellation);

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
                return Properties.Resource.icons_haffman_extruded_boxes_srf_69;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("01112EDE-152D-4DF8-9B2E-182CFFC4EB95"); }
        }
    }
}