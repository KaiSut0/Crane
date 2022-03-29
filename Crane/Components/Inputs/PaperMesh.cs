using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using OpenCvSharp;
using OpenCvSharp.Blob;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Crane
{
    public class PaperMeshComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PaperMeshComponent()
          : base("Paper Mesh", "Paper Mesh",
              "Generate flat mesh from scanned img",
              "Crane", "Inputs")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("filePath", "filePath", "filePath of img", GH_ParamAccess.item);
            pManager.AddIntegerParameter("size", "s", "size", GH_ParamAccess.item);
            pManager.AddIntegerParameter("maxNgon", "maxN", "maxNgon", GH_ParamAccess.item);
            pManager.AddNumberParameter("tolerance", "t", "tolerance to manually weld mesh\r\nt must be 0<t<1", GH_ParamAccess.item);

            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Vertices", "V", "Vertices", GH_ParamAccess.list);
            pManager.AddLineParameter("Mountain Crease", "M", "Moutain Creases", GH_ParamAccess.list);
            pManager.AddLineParameter("Vally Crease", "V", "Valley Creases", GH_ParamAccess.list);
            pManager.AddLineParameter("Boundary", "B", "Boundary", GH_ParamAccess.list);
            pManager.AddMeshParameter("mesh", "M", "mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)

        {
            string path = "";
            int size = 0;
            bool debug = false;
            int maxNgon = 3;
            double tol = -1;

            if (!DA.GetData(0, ref path)) { return; }
            if (!DA.GetData(1, ref size)) { return; }
            DA.GetData(2, ref maxNgon);
            DA.GetData(3, ref tol);

            IplImage img = new IplImage(path, LoadMode.Color);
            IplImage imgHSV = new IplImage(img.Size, BitDepth.U8, 3);

            Cv.CvtColor(img, imgHSV, ColorConversion.RgbToHsv);

            var channels = imgHSV.Split();
            IplImage hue = channels[0];



            IplImage Render = new IplImage(img.Size, BitDepth.U8, 3);

            //色抽出マスク用画像宣言
            IplImage imgB1 = new IplImage(img.Size, BitDepth.U8, 1);
            IplImage imgB2 = new IplImage(img.Size, BitDepth.U8, 1);
            IplImage imgR = new IplImage(img.Size, BitDepth.U8, 1);
            IplImage imgG = new IplImage(img.Size, BitDepth.U8, 1);
            IplImage imgB = new IplImage(img.Size, BitDepth.U8, 1);

            int RG = 30;
            int GB = 90;
            int BR = 150;
            int off = 1;

            int smin = 30;
            int bmin = 30;

            //色抽出用閾値作成
            CvScalar Bmin1 = new CvScalar(0, smin, bmin);
            CvScalar Bmax1 = new CvScalar(RG - off, 255, 255);

            CvScalar Bmin2 = new CvScalar(BR + off, smin, bmin);
            CvScalar Bmax2 = new CvScalar(180, 255, 255);

            CvScalar Gmin = new CvScalar(RG + off, smin, bmin);
            CvScalar Gmax = new CvScalar(GB - off, 255, 255);

            CvScalar Rmin = new CvScalar(GB + off, smin, bmin);
            CvScalar Rmax = new CvScalar(BR - off, 255, 255);

            //閾値を用いて色抽出
            Cv.InRangeS(imgHSV, Bmin1, Bmax1, imgB1);
            Cv.InRangeS(imgHSV, Bmin2, Bmax2, imgB2);
            Cv.Add(imgB1, imgB2, imgB);
            Cv.InRangeS(imgHSV, Gmin, Gmax, imgG);
            Cv.InRangeS(imgHSV, Rmin, Rmax, imgR);


            //Blobs化
            CvBlobs Rs = new CvBlobs(imgR);
            CvBlobs Gs = new CvBlobs(imgG);
            CvBlobs Bs = new CvBlobs(imgB);

            int minArea = img.Width * img.Height / 20000;
            int maxArea = img.Width * img.Height;
            Bs.FilterByArea(minArea, maxArea);
            Rs.FilterByArea(minArea, maxArea);
            Gs.FilterByArea(minArea, maxArea);


            //blobの配列化
            CvBlob[] Rblobs = new CvBlob[Rs.Count];
            CvBlob[] Bblobs = new CvBlob[Bs.Count];
            CvBlob[] Gblobs = new CvBlob[Gs.Count];
            Rs.Values.CopyTo(Rblobs, 0);
            Bs.Values.CopyTo(Bblobs, 0);
            Gs.Values.CopyTo(Gblobs, 0);


            if (!debug)
            {

                string deb = "";

                foreach (var bbbb in Rblobs)
                {
                    deb += bbbb.Area + "\r\n";
                }

                //BlobからLine化
                List<Line> Rlines = ExtractLinesFromBlobs(Rblobs);
                List<Line> Blines = ExtractLinesFromBlobs(Bblobs);
                List<Line> Glines = ExtractLinesFromBlobs(Gblobs);

                //scale
                double MinSize = Math.Min(img.Width, img.Height);
                double ScaleFactor = (double)size / MinSize;
                var scale = Transform.Scale(new Point3d(0, 0, 0), ScaleFactor);

                Network network = new Network();

                //ネットワークにLineを色ごとにラベル付きで入れる
                foreach (var l in Rlines)
                {
                    l.Transform(scale);
                    network.Add(l, 0);
                }
                foreach (var l in Blines)
                {
                    l.Transform(scale);
                    network.Add(l, 1);
                }
                foreach (var l in Glines)
                {
                    l.Transform(scale);
                    network.Add(l, 2);
                }




                double t = network.SearchWeldToleranceBinary(0, (double)size / 10, 0, 10);
                if (tol != -1)
                {
                    network.weld(tol * size);
                }
                else
                {
                    network.weld(t);
                }

                deb += "tolerance: " + t + "\r\n\r\n";



                //ウェルド後のエッジ抽出
                Rlines = network.ExtractLines(0);
                Blines = network.ExtractLines(1);
                Glines = network.ExtractLines(2);

                List<List<int>> faces = network.detectCycles(maxNgon);


                deb += "B: " + Bs.Count.ToString() + "\r\n";
                deb += "R: " + Rs.Count.ToString() + "\r\n";
                deb += "G: " + Gs.Count.ToString() + "\r\n";


                Mesh mesh = GenerateMesh(network.verts, faces);
                mesh.Normals.ComputeNormals();

                DA.SetDataList(0, network.verts);
                DA.SetDataList(1, Rlines);
                DA.SetDataList(2, Blines);
                DA.SetDataList(3, Glines);
                DA.SetData(4, mesh);
            }
            else
            {
                //赤レンダリング
                Rs.RenderBlobs(img, Render, RenderBlobsMode.Angle);
                Rs.RenderBlobs(img, Render, RenderBlobsMode.BoundingBox);
                Rs.RenderBlobs(img, Render, RenderBlobsMode.Centroid);

                //青レンダリング
                Bs.RenderBlobs(img, Render, RenderBlobsMode.Angle);
                Bs.RenderBlobs(img, Render, RenderBlobsMode.BoundingBox);
                Bs.RenderBlobs(img, Render, RenderBlobsMode.Centroid);

                //黒レンダリング
                Gs.RenderBlobs(img, Render, RenderBlobsMode.Angle);
                Gs.RenderBlobs(img, Render, RenderBlobsMode.BoundingBox);
                Gs.RenderBlobs(img, Render, RenderBlobsMode.Centroid);

                Cv.NamedWindow("test");
                IplImage Render2 = new IplImage(img.Size.Width / 4, img.Size.Height / 4, BitDepth.U8, 3);

                string deb = "";
                deb += "B: " + Bs.Count.ToString() + "\r\n";
                deb += "R: " + Rs.Count.ToString() + "\r\n";
                deb += "G: " + Gs.Count.ToString() + "\r\n";

                Cv.Resize(Render, Render2);
                Cv.ShowImage("test", Render2);

                Cv.WaitKey();

                Cv.DestroyWindow("test");
            }

            Cv.ReleaseImage(img);
            Cv.ReleaseImage(imgHSV);

        }

        protected Rectangle3d CVRect2Rect(CvRect cvr)
        {
            Rectangle3d rect = new Rectangle3d(Plane.WorldXY, new Point3d(cvr.Left, cvr.Bottom, 0), new Point3d(cvr.Right, cvr.Top, 0));
            return rect;
        }

        protected List<Line> ExtractLinesFromBlobs(CvBlob[] blobs)
        {
            List<Line> o = new List<Line>();

            foreach (CvBlob b in blobs)
            {
                b.CalcCentroid();

                Point3d cen = new Point3d(b.Centroid.X, b.Centroid.Y, 0);
                Vector3d dir = new Vector3d(Math.Cos(b.Angle()), Math.Sin(b.Angle()), 0);
                Line line = new Line(cen, dir, 10000);

                Curve bb = CVRect2Rect(b.Rect).ToNurbsCurve();

                var events = Rhino.Geometry.Intersect.Intersection.CurveLine(bb, line, 0.001, 0.001);

                Point3d start = new Point3d(events[0].PointB.X, -1 * events[0].PointB.Y, 0);
                Point3d end = new Point3d(events[1].PointB.X, -1 * events[1].PointB.Y, 0);

                o.Add(new Line(start, end));
            }

            return o;
        }

        protected Mesh GenerateMesh(List<Point3d> verts, List<List<int>> faceIndices)
        {
            Mesh mesh = new Mesh();
            mesh.Vertices.AddVertices(verts);

            List<MeshFace> faces = new List<MeshFace>();
            foreach (List<int> indices in faceIndices)
            {
                if (indices.Count == 3)
                {
                    faces.Add(new MeshFace(indices[0], indices[1], indices[2]));
                }
                else if (indices.Count == 4)
                {
                    faces.Add(new MeshFace(indices[0], indices[1], indices[2], indices[3]));
                }
            }

            mesh.Faces.AddFaces(faces);


            mesh.UnifyNormals();
            mesh.Normals.ComputeNormals();

            if (faces.Count == 0)
            {
                return null;
            }
            else
            {
                return mesh;
            }

        }

        public class Network
        {
            public List<Point3d> verts;
            public List<List<int>> edges;
            private List<List<int>> weldList;
            public List<List<int>> neighbors;
            public List<List<int>> connectedEdges;
            public List<bool> isBoundary;
            List<int> faceNumRemain;
            List<List<int>> nGons;

            public Network()
            {
                verts = new List<Point3d>();
                edges = new List<List<int>>();
                weldList = new List<List<int>>();
                neighbors = new List<List<int>>();
                connectedEdges = new List<List<int>>();
                isBoundary = new List<bool>();
                faceNumRemain = new List<int>();
                nGons = new List<List<int>>();
            }

            public void Add(Line l, int label)
            {
                Point3d a = l.From;
                Point3d b = l.To;
                verts.Add(a);
                verts.Add(b);
                edges.Add(new List<int>() { verts.Count - 2, verts.Count - 1, label });
            }

            public void weld(double t)
            {

                List<Point3d> prevVerts = new List<Point3d>(this.verts);
                List<Point3d> newVerts = new List<Point3d>();
                int weldCount = 0;

                //頂点ウェルド
                while (verts.Count != 0)
                {

                    List<Point3d> temp = new List<Point3d>();
                    List<int> weldIndices = new List<int>();

                    weldIndices.Add(weldCount);

                    temp.Add(verts[0]);
                    verts.RemoveAt(0);

                    //残り頂点回ループ
                    foreach (Point3d v in verts)
                    {
                        bool near = false;

                        //近傍判定
                        foreach (Point3d vv in temp)
                        {
                            if (v.DistanceTo(vv) < t)
                            {
                                near = true;
                            }

                        }
                        //近傍ならtempに追加、vertsから削除
                        if (near)
                        {
                            temp.Add(v);
                        }
                    }
                    foreach (Point3d i in temp)
                    {
                        verts.Remove(i);
                        weldIndices.Add(prevVerts.IndexOf(i));
                    }
                    Point3d newVert = new Point3d(0, 0, 0);
                    foreach (Point3d v in temp)
                    {
                        newVert += v;
                    }

                    newVert /= temp.Count;

                    newVerts.Add(newVert);

                    this.weldList.Add(weldIndices);
                    weldCount++;
                }
                this.verts = newVerts;

                //エッジ更新
                foreach (List<int> ind in weldList)
                {
                    for (int i = 1; i < ind.Count; i++)
                    {
                        foreach (List<int> edge in this.edges)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                if (edge[j] == ind[i])
                                {
                                    edge[j] = ind[0];
                                }
                            }
                        }
                    }
                }

                //接続エッジ情報生成
                this.generateConnectionInfo();
            }

            public bool checkWeldTolerance(double t)
            {
                List<Point3d> vertsTemp = new List<Point3d>(this.verts);

                bool completed = true;

                //頂点ウェルド
                while (vertsTemp.Count != 0)
                {

                    List<Point3d> temp = new List<Point3d>();

                    temp.Add(vertsTemp[0]);
                    vertsTemp.RemoveAt(0);

                    //残り頂点回ループ
                    foreach (Point3d v in vertsTemp)
                    {
                        bool near = false;

                        //近傍判定
                        foreach (Point3d vv in temp)
                        {
                            if (v.DistanceTo(vv) < t)
                            {
                                near = true;
                            }

                        }
                        //近傍ならtempに追加
                        if (near)
                        {
                            temp.Add(v);
                        }
                    }

                    if (temp.Count < 2)
                    {
                        completed = false;
                    }

                    foreach (Point3d i in temp)
                    {
                        vertsTemp.Remove(i);
                    }
                }

                return completed;

            }

            public double SearchWeldToleranceBinary(double left, double right, int iteration, int iterationMax)
            {


                double center = (left + right) / 2;
                bool c = checkWeldTolerance(center);
                if (iteration > iterationMax)
                {
                    if (c)
                    {
                        return center;
                    }
                    else
                    {
                        return right;
                    }

                }
                else
                {
                    if (!checkWeldTolerance(right))
                    {
                        return SearchWeldToleranceBinary(left, right * 0.8, iteration + 1, iterationMax);
                    }
                    else if (c)
                    {
                        return SearchWeldToleranceBinary(left, center, iteration + 1, iterationMax);
                    }
                    else
                    {
                        return SearchWeldToleranceBinary(center, right, iteration + 1, iterationMax);
                    }
                }

            }

            public List<List<int>> detectCycles(int maxNgon)
            {
                List<List<int>> ans = new List<List<int>>();

                //N角形の探索
                for (int N = 3; N < maxNgon + 1; N++)
                {
                    //残り頂点インデックスリスト
                    List<int> remainVerts = new List<int>();
                    for (int i = 0; i < this.verts.Count; i++)
                    {
                        if (faceNumRemain[i] != 0)
                        {
                            remainVerts.Add(i);
                        }
                    }

                    //探索ルーツ頂点が残っている限りループ
                    while (remainVerts.Count != 0)
                    {
                        List<int> root = new List<int>();
                        root.Add(remainVerts[0]);
                        List<List<int>> temp = unit(remainVerts, root, N);
                        ans.AddRange(temp);
                        foreach (var templist in temp)
                        {
                            foreach (int tempi in templist)
                            {
                                faceNumRemain[tempi] -= 1;
                            }
                        }

                        remainVerts.RemoveAt(0);
                    }
                }

                ans = RemoveDupFace(ans);

                return ans;
            }

            public void generateConnectionInfo()
            {
                //create neighbors
                this.neighbors.Clear();

                for (int i = 0; i < this.verts.Count; i++)
                {
                    this.neighbors.Add(new List<int>());
                }

                foreach (List<int> edge in this.edges)
                {
                    this.neighbors[edge[0]].Add(edge[1]);
                    this.neighbors[edge[1]].Add(edge[0]);
                }

                //generate connectedEdges, IsBoundary

                for (int i = 0; i < this.verts.Count; i++)
                {
                    this.connectedEdges.Add(new List<int>());
                    this.isBoundary.Add(false);
                }

                foreach (List<int> edge in this.edges)
                {
                    connectedEdges[edge[0]].Add(edge[2]);
                    connectedEdges[edge[1]].Add(edge[2]);
                    if (edge[2] == 2)
                    {
                        isBoundary[edge[0]] = true;
                        isBoundary[edge[1]] = true;
                    }
                }

                for (int i = 0; i < this.verts.Count; i++)
                {
                    if (isBoundary[i])
                    {
                        faceNumRemain.Add((neighbors[i].Count - 1) * 2);
                    }
                    else
                    {
                        faceNumRemain.Add(neighbors[i].Count * 2);
                    }

                }
            }

            public void clearAll()
            {
                this.verts.Clear();
                this.edges.Clear();
                this.weldList.Clear();
            }

            public List<Line> ExtractLines(int label)
            {
                List<Line> lines = new List<Line>();
                foreach (List<int> edge in this.edges)
                {
                    if (edge[2] == label)
                    {
                        lines.Add(new Line(this.verts[edge[0]], this.verts[edge[1]]));
                    }
                }
                return lines;
            }

            private List<List<int>> unit(List<int> remains, List<int> path, int max)
            {
                int length = path.Count;
                int now = path[length - 1];

                int start = path[0];

                if (length - 1 == 0)
                {
                    List<List<int>> ans = new List<List<int>>();
                    foreach (int next in this.neighbors[now])
                    {
                        if (remains.Contains(next))
                        {
                            List<int> temp = new List<int>(path);
                            temp.Add(next);
                            ans.AddRange(unit(remains, temp, max));
                        }
                    }
                    return ans;
                }
                else if (length - 1 < max)
                {
                    int prev = path[length - 2];
                    List<List<int>> ans = new List<List<int>>();
                    foreach (int next in this.neighbors[now])
                    {

                        if (next != prev && remains.Contains(next))
                        {
                            List<int> temp = new List<int>(path);
                            temp.Add(next);
                            ans.AddRange(unit(remains, temp, max));
                        }

                    }
                    return ans;
                }
                else
                {
                    List<List<int>> ans = new List<List<int>>();
                    if (start == now)
                    {
                        ans.Add(path.GetRange(0, length - 1));
                        return ans;
                    }
                    else
                    {
                        return ans;
                    }
                }

            }

            private bool ListEqual(List<int> a, List<int> b)
            {
                bool ans = true;


                if (a.Count == b.Count)
                {
                    foreach (int aa in a)
                    {
                        ans = ans && b.Contains(aa);
                    }
                }
                else if (a.Count < b.Count)
                {
                    foreach (int aa in a)
                    {
                        ans = ans && b.Contains(aa);
                    }
                }
                else
                {
                    ans = false;
                }

                return ans;
            }

            private List<List<int>> RemoveDupFace(List<List<int>> listlist)
            {
                List<List<int>> ans = new List<List<int>>();
                if (listlist.Count == 0)
                {
                    return ans;
                }
                else
                {
                    ans.Add(listlist[0]);
                    foreach (List<int> a in listlist)
                    {
                        if (!ContainsEqual(ans, a))
                        {
                            ans.Add(a);
                        }
                    }
                    return ans;
                }

            }

            private bool ContainsEqual(List<List<int>> listlist, List<int> list)
            {
                bool ans = false;
                foreach (List<int> l in listlist)
                {
                    ans = ans || ListEqual(l, list);
                }
                return ans;
            }

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resource.icons_papermesh;
                //return Crane.Properties.Resources.papermesh;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b80e8a5c-fb9b-4243-9bd3-28ce106b38e5"); }
        }
    }
}