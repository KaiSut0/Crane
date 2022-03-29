using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Crane.Components.Util
{
    public class MapDev2Fold : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MapDev2Fold class.
        /// </summary>
        public MapDev2Fold()
          : base("Map Dev2Fold", "Map Dev2Fold",
              "Map development panels to folded state.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Panel", "Panel", "Panel", GH_ParamAccess.list);
            pManager.AddMeshParameter("Dev", "Dev", "Dev", GH_ParamAccess.item);
            pManager.AddMeshParameter("Fold", "Fold", "Fold", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Panel", "Panel", "Panel", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryBase> geos = new List<GeometryBase>();
            Mesh dev = new Mesh();
            Mesh fold = new Mesh();
            DA.GetDataList(0, geos);
            DA.GetData(1, ref dev);
            DA.GetData(2, ref fold);
            for (int i = 0; i < dev.Ngons.Count; i++)
            {
                var fd = dev.Faces[(int)dev.Ngons[i].FaceIndexList()[0]];
                var ff = fold.Faces[(int)fold.Ngons[i].FaceIndexList()[0]];
                var pd0 = dev.Vertices[fd.A];
                var pd1 = dev.Vertices[fd.B];
                var pd2 = dev.Vertices[fd.C];
                var pf0 = fold.Vertices[ff.A];
                var pf1 = fold.Vertices[ff.B];
                var pf2 = fold.Vertices[ff.C];

                Plane plnd = new Plane(pd0, pd1, pd2);
                Plane plnf = new Plane(pf0, pf1, pf2);
                geos[i].Transform(Transform.PlaneToPlane(plnd, plnf));
            }

            DA.SetDataList(0, geos);

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
            get { return new Guid("93073aa4-20d8-46e0-8a70-e01aab005274"); }
        }
    }
}