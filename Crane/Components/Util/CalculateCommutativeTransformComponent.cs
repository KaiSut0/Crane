using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Data;
using Crane.Core;
using Crane.Misc;

namespace Crane.Components.Misc
{
    public class CalculateCommutativeTransformComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CalculateCommutativeTransformComponent class.
        /// </summary>
        public CalculateCommutativeTransformComponent()
          : base("Calculate Commutative Transform", "Comm Transform",
              "Calculate two commutative transforms.",
              "Crane", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("L1ini", "L1ini", "L1ini", GH_ParamAccess.list);
            pManager.AddNumberParameter("L2ini", "L2ini", "L2ini", GH_ParamAccess.list);
            pManager.AddNumberParameter("d1ini", "d1ini", "d1ini", GH_ParamAccess.list);
            pManager.AddNumberParameter("d2ini", "d2ini", "d2ini", GH_ParamAccess.list);
            pManager.AddBooleanParameter("detL1is1", "detL1is1", "detL1is1", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("detL2is1", "detL2is1", "detL2is1", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("DetL1Goal", "DetL1Goal", "DetL1Goal", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("DetL2Goal", "DetL2Goal", "DetL2Goal", GH_ParamAccess.item, 1);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTransformParameter("T1", "T1", "T1", GH_ParamAccess.item);
            pManager.AddTransformParameter("T2", "T2", "T2", GH_ParamAccess.item);
            pManager.AddNumberParameter("error", "error", "error", GH_ParamAccess.list);
            pManager.AddNumberParameter("DetL1", "DetL1", "DetL1", GH_ParamAccess.item);
            pManager.AddNumberParameter("DetL2", "DetL2", "DetL2", GH_ParamAccess.item);
            pManager.AddNumberParameter("rotAngle1", "rotAngle1", "rotAngle1", GH_ParamAccess.item);
            pManager.AddNumberParameter("rotAngle2", "rotAngle2", "rotAngle2", GH_ParamAccess.item);
            pManager.AddVectorParameter("rotVec1", "rotVec1", "rotVec1", GH_ParamAccess.item);
            pManager.AddVectorParameter("rotVec2", "rotVec2", "rotVec2", GH_ParamAccess.item);
            pManager.AddPlaneParameter("scalePlane1", "scalePlane1", "scalePlane1", GH_ParamAccess.item);
            pManager.AddPlaneParameter("scalePlane2", "scalePlane2", "scalePlane2", GH_ParamAccess.item);
            pManager.AddVectorParameter("scale1", "scale1", "scale1", GH_ParamAccess.item);
            pManager.AddVectorParameter("scale2", "scale2", "scale2", GH_ParamAccess.item);
            pManager.AddVectorParameter("translation1", "translation1", "translation1", GH_ParamAccess.item);
            pManager.AddVectorParameter("translation2", "translation2", "translation2", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var l1ini = new List<double>();
            var l2ini = new List<double>();
            var d1ini = new List<double>();
            var d2ini = new List<double>();
            var detL1is1 = false;
            var detL2is1 = false;
            double detL1Goal = 1;
            double detL2Goal = 1;


            DA.GetDataList(0, l1ini);
            DA.GetDataList(1, l2ini);
            DA.GetDataList(2, d1ini);
            DA.GetDataList(3, d2ini);
            DA.GetData(4, ref detL1is1);
            DA.GetData(5, ref detL2is1);
            DA.GetData(6, ref detL1Goal);
            DA.GetData(7, ref detL2Goal);

            var _l1ini = new double[3, 3];
            var _l2ini = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _l1ini[i, j] = l1ini[3 * i + j];
                    _l2ini[i, j] = l2ini[3 * i + j];
                }
            }

            var calComTrans = new CalculateCommutativeTransform(_l1ini, _l2ini, d1ini.ToArray(), d2ini.ToArray(), detL1is1, detL2is1, detL1Goal, detL2Goal);
            calComTrans.SolveCommutativeEquation(1000, 1e-12);
            DA.SetData(0, calComTrans.A1);
            DA.SetData(1, calComTrans.A2);
            DA.SetDataList(2, calComTrans.Error);
            DA.SetData(3, calComTrans.DetL1);
            DA.SetData(4, calComTrans.DetL2);

            double rotAngle1, rotAngle2;
            Vector3d rotVec1, rotVec2, scale1, scale2, trans1, trans2;
            Plane scalePlane1, scalePlane2;
            Core.Util.LogMap(calComTrans.A1, out rotAngle1, out rotVec1, out scalePlane1, out scale1, out trans1);
            Core.Util.LogMap(calComTrans.A2, out rotAngle2, out rotVec2, out scalePlane2, out scale2, out trans2);

            DA.SetData(5, rotAngle1);
            DA.SetData(6, rotAngle2);
            DA.SetData(7, rotVec1);
            DA.SetData(8, rotVec2);
            DA.SetData(9, scalePlane1);
            DA.SetData(10, scalePlane2);
            DA.SetData(11, scale1);
            DA.SetData(12, scale2);
            DA.SetData(13, trans1);
            DA.SetData(14, trans2);


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
                return Properties.Resource.icons_cal_commutative_trans;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("22f5c474-49cb-4228-be13-3a558a9dfcff"); }
        }
    }
}