using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Crane.Misc;

namespace Crane.Components.Misc
{
    public class ScaleRotationComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ScaleRotationComponent class.
        /// </summary>
        public ScaleRotationComponent()
          : base("Scale Rotation", "Scale Rotation",
              "Compute scale and rotation transform.",
              "Crane", "Misc")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Factor", "S", "Scale factor", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Angle", "A", "Rotation angle", GH_ParamAccess.item, Math.PI/2);
            pManager.AddVectorParameter("Axis", "Ax", "Rotation axis.", GH_ParamAccess.item, Vector3d.ZAxis);
            pManager.AddPointParameter("Origin", "O", "Scale and rotation origin.", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddNumberParameter("t", "t", "Continuous parameter.", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTransformParameter("ScaleRotation", "SR", "Scale and rotation transform.", GH_ParamAccess.item);
            pManager.AddTransformParameter("Scale", "S", "Scale transform.", GH_ParamAccess.item);
            pManager.AddTransformParameter("Rotation", "R", "Rotation transform.", GH_ParamAccess.item);
            pManager.AddTransformParameter("Derivative", "D", "Derivative of scale rotation transform.",
                GH_ParamAccess.item);
            pManager.AddTransformParameter("Parametrized", "P", "Parameterized scale rotation transform about t.",
                GH_ParamAccess.item);
            pManager.AddTransformParameter("PDerivative", "PD",
                "Derivative of parametrized scale rotation transform about t.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double f = 1;
            double a = Math.PI / 2;
            var ax = Vector3d.ZAxis;
            var o = Point3d.Origin;
            double t = 1;

            DA.GetData("Factor", ref f);
            DA.GetData("Angle", ref a);
            DA.GetData("Axis", ref ax);
            DA.GetData("Origin", ref o);
            DA.GetData("t", ref t);

            var sr = new ScaleRotation(f, a, ax, o);

            DA.SetData("ScaleRotation", sr.Transform);
            DA.SetData("Scale", sr.Scale);
            DA.SetData("Rotation", sr.Rotation);
            DA.SetData("Derivative", sr.Derivative(0));
            DA.SetData("Parametrized", sr.Parametrized(t));
            DA.SetData("PDerivative", sr.Derivative(t));

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
            get { return new Guid("50fcac77-7566-4f5a-bcd9-98a8179813af"); }
        }
    }
}