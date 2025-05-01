using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Eto.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;

using Rhino;
using Rhino.Geometry;

using MathNet.Numerics.LinearAlgebra;
using Crane.Core;


// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Crane.Components.Solver
{
    public class CraneInteractiveSolver : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RigidOrigami rigidOrigami;
        public bool SolverOn { get; set; } = false;
        public bool IsStart { get; set; } = true;
        public bool IsReset { get; set; } = false;
        public bool IsRedo { get; set; } = false;
        public bool IsUndo { get; set; } = false;
        public bool IsGrabMode { get; set; } = false;
        public bool IsMouseDown { get; set; } = false;
        public bool IsRestart { get; set; } = false;
        public bool isOn = false;
        Rhino.UI.MouseCallback myMouse;
        public System.Windows.Forms.Timer timer;
        public double residual = 0;

        public CraneInteractiveSolver()
          : base("Crane Interactive Solver", "Solver",
              "Interactive solver for a rigid folding simulation and a form finding.",
              "Crane", "Solver")
        {
            rigidOrigami = new RigidOrigami();
            SolverOn = false;
            IsStart = true;
            IsReset = false;
            IsRedo = false;
            IsUndo = false;
            IsGrabMode = false;
            IsMouseDown = false;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 30;
            
            timer.Tick += new EventHandler(tick);
            IsMouseDown = false;
            IsStart = true;
            residual = 0;
        }

        public override void CreateAttributes()
        {
            m_attributes = new Attributes_Custom(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CMesh", "CMesh", "Input the CMesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Constraints", "Constraints", "Input the constraints", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fold Speed", "Fold Speed", "Input the parameter about folding speed between 0.0 - 1.0. Default is 0.3.", GH_ParamAccess.item, 0.3);
            pManager.AddIntegerParameter("NR Iteration", "NR Iteration", "Each iteration of Newton method.", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("CGNR Iteration", "CGNR Iteration", "Each iteration of CGNR method for solving linear equation.", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Threshold", "Threshold", "Threshold", GH_ParamAccess.item, 1e-13);
            
            pManager[1].Optional = true;
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
            pManager.AddGenericParameter("CMesh", "CMesh", "Output the CMesh", GH_ParamAccess.item);
            pManager.AddGenericParameter("RigidOrigami", "RigidOrigami", "Output the RigidOrigami", GH_ParamAccess.item);
            pManager.AddNumberParameter("Residual", "Residual", "Residual", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "Massage", "Message", GH_ParamAccess.item);
            pManager.AddNumberParameter("NRTime", "NRTime", "NRTime", GH_ParamAccess.list);
            pManager.AddPointParameter("GrabPts", "GrabPts", "GrabPts", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.UseBest();
            //bool useMKL = MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.TryUseNativeMKL();
            //MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.UseManaged();
            //var provider = MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.Provider;
            bool useMKL = true;
            string message = "";
            message += useMKL.ToString();
            CMesh cMesh = new CMesh();
            List<Constraint> constraints = new List<Constraint>();
            double foldSpeed = 1.0;
            int nrIteration = 3;
            int cgnrIteration = 100;
            double threshold = 1e-13;
            List<int> grabIds = new List<int>();

            if(!DA.GetData(0, ref cMesh)) { return; }
            DA.GetDataList(1, constraints);
            DA.GetData(2, ref foldSpeed);
            DA.GetData(3, ref nrIteration);
            DA.GetData(4, ref cgnrIteration);
            DA.GetData(5, ref threshold);


            RigidOrigami rigidOrigamiSI = new RigidOrigami(cMesh, constraints);


            foldSpeed /= 50;

            if (this.IsStart)
            {
                rigidOrigami = new RigidOrigami(rigidOrigamiSI); 
                IsStart = false;
                this.timer.Start();
            }
            if (this.IsReset)
            {
                rigidOrigamiSI.SaveModes(rigidOrigami.IsRigidMode, rigidOrigami.IsPanelFlatMode, rigidOrigami.IsFoldBlockMode, rigidOrigami.IsConstraintMode);
                rigidOrigami = new RigidOrigami(rigidOrigamiSI);
            }

            if (this.IsRestart)
            {
                isOn = true;
                timer.Start();
                IsRestart = false;
            }
            if (SolverOn)
            {
                if (!this.isOn)
                {
                    this.timer.Start();
                    this.isOn = true;
                }
                this.isOn = true;

                bool isfold = rigidOrigami.Fold || rigidOrigami.UnFold;
                
                bool isgrab = this.IsGrabMode & (Keyboard.Modifiers == Keys.Alt);
                double resi = rigidOrigami.ComputeResidual();
                bool iscompute = ((residual > threshold) || isfold || isgrab);

                if(IsUndo)
                {
                    if(rigidOrigami.NowRecordedIndexPosition > 0)
                    {
                        rigidOrigami.NowRecordedIndexPosition -= 1;
                        rigidOrigami.CMesh.UpdateMesh(rigidOrigami.RecordedMeshPoints[rigidOrigami.NowRecordedIndexPosition]);
                    }
                }
                if (IsRedo)
                {
                    if(rigidOrigami.NowRecordedIndexPosition < rigidOrigami.RecordedMeshPoints.Count - 1)
                    {
                        rigidOrigami.NowRecordedIndexPosition += 1;
                        rigidOrigami.CMesh.UpdateMesh(rigidOrigami.RecordedMeshPoints[rigidOrigami.NowRecordedIndexPosition]);
                    }
                        
                }

                if (resi < 1e+5)
                {
                    rigidOrigami.Constraints = constraints;

                    Vector<double> moveVector = Vector<double>.Build.Sparse(rigidOrigami.CMesh.DOF);
                    if (isfold)
                    {
                        moveVector += rigidOrigami.ComputeFoldMotion(foldSpeed, cgnrIteration);
                    }
                    if (isgrab)
                    {
                        myMouse = new MyMouseCallback();
                        myMouse.Enabled = true;
                        if (Mouse.Buttons == MouseButtons.Primary)
                        {
                            moveVector += this.GetGrabMoveVector(out grabIds);
                        }
                        else
                        {
                            IsMouseDown = false;
                        }
                    }
                    else
                    {
                        myMouse = new MyMouseCallbackOff();
                        myMouse.Enabled = true;
                    }
                    if(moveVector.L2Norm() > 1e-5 || rigidOrigami.ComputeResidual() > threshold)
                    {
                        if (rigidOrigami.IsRigidMode && iscompute)
                        {
                            residual = rigidOrigami.NRSolve(-moveVector, threshold, nrIteration, cgnrIteration);
                        }
                        else if (rigidOrigami.Constraints.Count != 0 && rigidOrigami.IsConstraintMode || iscompute)
                        {
                            residual = rigidOrigami.NRSolve(-moveVector, threshold, nrIteration, cgnrIteration);
                        }
                        else
                        {
                            rigidOrigami.CMesh.UpdateMesh(rigidOrigami.CMesh.MeshVerticesVector + moveVector);
                            rigidOrigami.CMesh.UpdateProperties();
                        }
                        if (!rigidOrigami.IsRigidMode)
                            rigidOrigami.CMesh.UpdateEdgeLengthSquared();

                    }
                    else
                    {
                        if (rigidOrigami.ComputeResidual() < threshold & !IsGrabMode & !isfold)
                        {
                            isOn = false;
                            timer.Stop();
                        }
                    }
                }


            }
            else
            {
                this.isOn = false;
                this.timer.Stop();
            }


            DA.SetData(0, rigidOrigami.CMesh);
            DA.SetData(1, rigidOrigami);
            DA.SetData(2, residual);
            DA.SetData(3, message);
            DA.SetDataList(4, rigidOrigami.NRComputationSpeeds);
            DA.SetDataList(5, grabIds.Select(id => rigidOrigami.CMesh.Mesh.Vertices[id]));

        }

        public class MyMouseCallback : Rhino.UI.MouseCallback
        {
            protected override void OnMouseDown(Rhino.UI.MouseCallbackEventArgs e)
            {
                e.Cancel = true;
                base.OnMouseDown(e);
            }
        }
        public class MyMouseCallbackOff : Rhino.UI.MouseCallback
        {
            protected override void OnMouseDown(Rhino.UI.MouseCallbackEventArgs e)
            {
                e.Cancel = false;
                base.OnMouseDown(e);
            }
        }
        private void tick(object myObject, EventArgs myeventArgs)
        {
            if (this.isOn && !(this.Locked))
            {
                ExpireSolution(true);
            }
        }
        private Vector<double> GetGrabMoveVector(out List<int> grabIds)
        {
            double radius = rigidOrigami.CMesh.WholeScale / 10;
            List<Vector3d> grabForces = new List<Vector3d>();
            var doc = RhinoDoc.ActiveDoc;
            var pt = Rhino.UI.MouseCursor.Location;
            var screenPt = new System.Drawing.Point((int)pt.X, (int)pt.Y);
            var clientPt = doc.Views.ActiveView.ScreenToClient(screenPt);
            var l = doc.Views.ActiveView.ActiveViewport.ClientToWorld(clientPt);
            var dir = doc.Views.ActiveView.ActiveViewport.CameraDirection;
            Plane pl = new Plane(new Point3d(0, 0, 0), dir);
            double t;
            Rhino.Geometry.Intersect.Intersection.LinePlane(l, pl, out t);

            if (!this.IsMouseDown)
            {
                rigidOrigami.MoveVertexIndices = new List<int>();
                double distanceMin = 1e+10;
                for (int i = 0; i < rigidOrigami.CMesh.NumberOfVertices; i++)
                {
                    double distance = l.DistanceTo(rigidOrigami.CMesh.Mesh.Vertices[i], false);
                    if (distance < radius)
                    {
                        rigidOrigami.MoveVertexIndices.Add(i);
                        if (distance < distanceMin)
                        {
                            rigidOrigami.MoveVertexIndex = i;
                            distanceMin = distance;
                        }
                    }
                }
                //クリックされたときは取り合えずゼロベクトル
                foreach (int vertexIndex in rigidOrigami.MoveVertexIndices)
                {
                    grabForces.Add(new Vector3d(0, 0, 0));
                }
                this.IsMouseDown = true;
            }
            else
            {
                //ドラッグされてる間は、(カーソルからのレイ)と頂点を使って入力ベクトル生成
                Point3d mouse = l.ClosestPoint(rigidOrigami.CMesh.Mesh.Vertices[rigidOrigami.MoveVertexIndex], false);
                Vector3d dragVector = mouse - rigidOrigami.CMesh.Mesh.Vertices[rigidOrigami.MoveVertexIndex];
                foreach(int vertexIndex in rigidOrigami.MoveVertexIndices)
                {
                    grabForces.Add(dragVector);
                }
            }

            rigidOrigami.MoveVertexIndices = new List<int>(new int[] { rigidOrigami.MoveVertexIndex });
            grabIds = rigidOrigami.MoveVertexIndices;
            return rigidOrigami.ComputeGrabMotion(rigidOrigami.MoveVertexIndices, grabForces);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resource.icons_interactive_solver;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9ccad0dd-e7a5-4208-a2ff-ac6169f803f3"); }
        }
    }

    public class Attributes_Custom : GH_ComponentAttributes
    {
        public Attributes_Custom(GH_Component owner) : base(owner) { }
        protected override void Layout()
        {
            base.Layout();

            // コンポーネントのサイズを取得し、ボタン分の長さをプラス(+220)する
            Rectangle recAll = GH_Convert.ToRectangle(Bounds);
            int buttonHeight = 176;
            recAll.Height += buttonHeight;


            // 22の高さの長方形を配置する

            Rectangle recSolverOn = recAll;
            recSolverOn.Y = recAll.Bottom - buttonHeight;
            recSolverOn.Height = 22;
            recSolverOn.Width = recSolverOn.Width / 2;
            recSolverOn.Inflate(-2, -2);
            Rectangle recReset = recSolverOn;
            recReset.Location = new System.Drawing.Point(recSolverOn.X + recSolverOn.Width + 4, recSolverOn.Location.Y);
            buttonHeight -= 22;

            Rectangle recUndo = recAll;
            recUndo.Y = recAll.Bottom - buttonHeight;
            recUndo.Height = 22;
            recUndo.Width = recUndo.Width / 2;
            recUndo.Inflate(-2, -2);
            Rectangle recRedo = recUndo;
            recRedo.Location = new System.Drawing.Point(recRedo.X + recRedo.Width + 4, recRedo.Location.Y);
            buttonHeight -= 22;


            Rectangle recFold = recAll;
            recFold.Y = recFold.Bottom - buttonHeight;
            recFold.Height = 22;
            recFold.Width = recFold.Width / 2;
            recFold.Inflate(-2, -2);
            Rectangle recUnFold = recFold;
            recUnFold.Location = new System.Drawing.Point(recUnFold.Location.X + recUnFold.Width + 4, recUnFold.Location.Y);
            buttonHeight -= 22;




            Rectangle recRigidEdgeMode = recAll;
            recRigidEdgeMode.Y = recAll.Bottom - buttonHeight;
            recRigidEdgeMode.Height = 22;
            recRigidEdgeMode.Inflate(-2, -2);
            buttonHeight -= 22;

            Rectangle recFlatPanelMode = recAll;
            recFlatPanelMode.Y = recAll.Bottom - buttonHeight;
            recFlatPanelMode.Height = 22;
            recFlatPanelMode.Inflate(-2, -2);
            buttonHeight -= 22;

            Rectangle recFoldBlockMode = recAll;
            recFoldBlockMode.Y = recAll.Bottom - buttonHeight;
            recFoldBlockMode.Height = 22;
            recFoldBlockMode.Inflate(-2, -2);
            buttonHeight -= 22;

            Rectangle recGrabMode = recAll;
            recGrabMode.Y = recAll.Bottom - buttonHeight;
            recGrabMode.Height = 22;
            recGrabMode.Inflate(-2, -2);
            buttonHeight -= 22;

            Rectangle recConstraintMode = recAll;
            recConstraintMode.Y = recAll.Bottom - buttonHeight;
            recConstraintMode.Height = 22;
            recConstraintMode.Inflate(-2, -2);
            //buttonHeight -= 22;

            //Rectangle recRecordMode = recAll;
            //recRecordMode.Y = recAll.Bottom - buttonHeight;
            //recRecordMode.Height = 22;
            //recRecordMode.Inflate(-2, -2);




            Bounds = recAll;
            ButtonBounds = recFold;
            ButtonBounds2 = recUnFold;
            ButtonBounds3 = recReset;
            ButtonBounds4 = recSolverOn;
            ButtonBounds5 = recRigidEdgeMode;
            ButtonBounds6 = recFlatPanelMode;
            ButtonBounds7 = recFoldBlockMode;
            ButtonBounds8 = recGrabMode;
            ButtonBounds9 = recConstraintMode;
            //ButtonBounds10 = recRecordMode;
            ButtonBounds11 = recRedo;
            ButtonBounds12 = recUndo;
        }
        private Rectangle ButtonBounds { get; set; } // Fold
        private Rectangle ButtonBounds2 { get; set; } // UnFold
        private Rectangle ButtonBounds3 { get; set; } // Reset
        private Rectangle ButtonBounds4 { get; set; } // Solver On/Off
        private Rectangle ButtonBounds5 { get; set; } // Rigid Edge Mode On/Off
        private Rectangle ButtonBounds6 { get; set; } // Flat Panel Mode On/Off
        private Rectangle ButtonBounds7 { get; set; } // 180 Fold Blocking Mode On/Off
        private Rectangle ButtonBounds8 { get; set; } // Grab Mode On/Off
        private Rectangle ButtonBounds9 { get; set; } // Constraint Mode On/Off
        //private Rectangle ButtonBounds10 { get; set; } // Record Mode On/Off
        private Rectangle ButtonBounds11 { get; set; } // Redo
        private Rectangle ButtonBounds12 { get; set; } // Undo


        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            CraneInteractiveSolver comp = this.Owner as CraneInteractiveSolver;
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Fold", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds2, ButtonBounds2, GH_Palette.Black, "Unfold", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds3, ButtonBounds3, GH_Palette.Black, "Reset", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds4, ButtonBounds4, GH_Palette.Black, "Solver: Off", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds5, ButtonBounds5, GH_Palette.Black, "Rigid Edge Mode: Off", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds6, ButtonBounds6, GH_Palette.Black, "Flat Panel Mode: Off", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds7, ButtonBounds7, GH_Palette.Black, "180 Fold Blocking Mode: Off", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds8, ButtonBounds8, GH_Palette.Black, "Grab Mode: Off", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds9, ButtonBounds9, GH_Palette.Black, "Constraint Mode: Off", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            //if (channel == GH_CanvasChannel.Objects)
            //{
            //    GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds10, ButtonBounds10, GH_Palette.Black, "Record Mode: Off", 2, 0);
            //    button.Render(graphics, Selected, Owner.Locked, false);
            //    button.Dispose();
            //}
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds11, ButtonBounds11, GH_Palette.Black, "Redo", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds12, ButtonBounds12, GH_Palette.Black, "Undo", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }


            if (comp.rigidOrigami.Fold & !comp.rigidOrigami.UnFold)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Folding", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (!comp.rigidOrigami.Fold & comp.rigidOrigami.UnFold)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds2, ButtonBounds2, GH_Palette.Black, "UnFolding", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.IsReset)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds3, ButtonBounds3, GH_Palette.Black, "Resetting", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.SolverOn)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds4, ButtonBounds4, GH_Palette.Black, "Solver: On", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.rigidOrigami.IsRigidMode)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds5, ButtonBounds5, GH_Palette.Black, "Rigid Edge Mode: On", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.rigidOrigami.IsPanelFlatMode)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds6, ButtonBounds6, GH_Palette.Black, "Flat Panel Mode: On", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.rigidOrigami.IsFoldBlockMode)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds7, ButtonBounds7, GH_Palette.Black, "180 Fold Blocking Mode: On", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.IsGrabMode)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds8, ButtonBounds8, GH_Palette.Black, "Grab Mode: On", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.rigidOrigami.IsConstraintMode)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds9, ButtonBounds9, GH_Palette.Black, "Constraint Mode: On", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            //if (comp.rigidOrigami.IsRecordMode)
            //{
            //    GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds10, ButtonBounds10, GH_Palette.Black, "Record Mode: On", 2, 0);
            //    button.Render(graphics, Selected, Owner.Locked, false);
            //    button.Dispose();
            //}
            if (comp.IsRedo)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds11, ButtonBounds11, GH_Palette.Black, "Redoing", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
            if (comp.IsUndo)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds12, ButtonBounds12, GH_Palette.Black, "Undoing", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }



        }

        /// <summary>
        /// マウスをクリックしたときのイベントハンドラ
        /// </summary>
        
        

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // ButtonBoundsを押したときのイベント
            if (Mouse.Buttons == MouseButtons.Primary)
            {
                CraneInteractiveSolver comp = this.Owner as CraneInteractiveSolver;
                RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    comp.rigidOrigami.Fold = true;
                    if (!comp.isOn)
                    {
                        comp.IsRestart = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec2 = ButtonBounds2;
                if (rec2.Contains(e.CanvasLocation))
                {
                    comp.rigidOrigami.UnFold = true;
                    if (!comp.isOn)
                    {
                        comp.IsRestart = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec3 = ButtonBounds3;
                if (rec3.Contains(e.CanvasLocation))
                {
                    comp.IsReset = true;
                    comp.ExpireSolution(true);

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec4 = ButtonBounds4;
                if (rec4.Contains(e.CanvasLocation))
                {
                    if (comp.SolverOn)
                    {
                        comp.SolverOn = false;
                        comp.ExpireSolution(true);
                    }
                    else if (!comp.SolverOn)
                    {
                        comp.SolverOn = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec5 = ButtonBounds5;
                if (rec5.Contains(e.CanvasLocation))
                {
                    if (comp.rigidOrigami.IsRigidMode)
                    {
                        comp.rigidOrigami.IsRigidMode = false;
                        comp.ExpireSolution(true);
                    }
                    else if (!comp.rigidOrigami.IsRigidMode)
                    {
                        comp.rigidOrigami.IsRigidMode = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec6 = ButtonBounds6;
                if (rec6.Contains(e.CanvasLocation))
                {
                    if (comp.rigidOrigami.IsPanelFlatMode)
                    {
                        comp.rigidOrigami.IsPanelFlatMode = false;
                        comp.ExpireSolution(true);
                    }
                    else if (!comp.rigidOrigami.IsPanelFlatMode)
                    {
                        comp.rigidOrigami.IsPanelFlatMode = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec7 = ButtonBounds7;
                if (rec7.Contains(e.CanvasLocation))
                {
                    if (comp.rigidOrigami.IsFoldBlockMode)
                    {
                        comp.rigidOrigami.IsFoldBlockMode = false;
                        comp.ExpireSolution(true);
                    }
                    else if (!comp.rigidOrigami.IsFoldBlockMode)
                    {
                        comp.rigidOrigami.IsFoldBlockMode = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec8 = ButtonBounds8;
                if (rec8.Contains(e.CanvasLocation))
                {
                    if (comp.IsGrabMode)
                    {
                        comp.IsGrabMode = false;
                        comp.ExpireSolution(true);
                    }
                    else if (!comp.IsGrabMode)
                    {
                        comp.IsGrabMode = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                RectangleF rec9 = ButtonBounds9;
                if (rec9.Contains(e.CanvasLocation))
                {
                    if (comp.rigidOrigami.IsConstraintMode)
                    {
                        comp.rigidOrigami.IsConstraintMode = false;
                        comp.ExpireSolution(true);
                    }
                    else if (!comp.rigidOrigami.IsConstraintMode)
                    {
                        comp.rigidOrigami.IsConstraintMode = true;
                        comp.ExpireSolution(true);
                    }

                    return GH_ObjectResponse.Handled;
                }

                //RectangleF rec10 = ButtonBounds10;
                //if (rec10.Contains(e.CanvasLocation))
                //{
                //    if (comp.rigidOrigami.IsRecordMode)
                //    {
                //        comp.rigidOrigami.IsRecordMode = false;
                //        comp.ExpireSolution(true);
                //    }
                //    else if (!comp.rigidOrigami.IsRecordMode)
                //    {
                //        comp.rigidOrigami.IsRecordMode = true;
                //        comp.ExpireSolution(true);
                //    }

                //    return GH_ObjectResponse.Handled;
                //}

                RectangleF rec11 = ButtonBounds11;
                if (rec11.Contains(e.CanvasLocation))
                {
                    comp.IsRedo = true;
                    comp.ExpireSolution(true);
                }
                RectangleF rec12 = ButtonBounds12;
                if (rec12.Contains(e.CanvasLocation))
                {
                    comp.IsUndo = true;
                    comp.ExpireSolution(true);
                }



            }



            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            CraneInteractiveSolver comp = this.Owner as CraneInteractiveSolver;
            RectangleF rec = ButtonBounds;
            if (rec.Contains(e.CanvasLocation))
            {
                comp.rigidOrigami.Fold = false;
                return GH_ObjectResponse.Handled;
            }
            RectangleF rec2 = ButtonBounds2;
            if (rec2.Contains(e.CanvasLocation))
            {
                comp.rigidOrigami.UnFold = false;
                return GH_ObjectResponse.Handled;
            }
            RectangleF rec3 = ButtonBounds3;
            if (rec3.Contains(e.CanvasLocation))
            {
                comp.IsReset = false;
                comp.ExpireSolution(true);
                return GH_ObjectResponse.Handled;
            }
            RectangleF rec11 = ButtonBounds11;
            if (rec11.Contains(e.CanvasLocation))
            {
                comp.IsRedo = false;
                comp.ExpireSolution(true);
                return GH_ObjectResponse.Handled;
            }
            RectangleF rec12 = ButtonBounds12;
            if (rec12.Contains(e.CanvasLocation))
            {
                comp.IsUndo = false;
                comp.ExpireSolution(true);
                return GH_ObjectResponse.Handled;
            }


            return base.RespondToMouseUp(sender, e);
        }
    }
}
