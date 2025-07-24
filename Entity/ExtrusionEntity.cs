using CoreLib;

namespace Cad3DApp
{
    public class ExtrusionEntity : Entity
    {
        public List<Polygon3D> mPolygons;
        public Point3D mVector;
        public bool mClose = true;          //  閉領域 Polyline(false)とPolygon(true)

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤサイズ</param>
        public ExtrusionEntity(int layersize)
        {
            mID = EntityId.Extrusion;
            mLayerBit = new byte[layersize / 8];
            mPolygons = new List<Polygon3D>();
            mVector = new Point3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygons">ポリゴン</param>
        /// <param name="vector">押出ベクトル</param>
        /// <param name="layersize">レイヤサイズ</param>
        public ExtrusionEntity(List<Polygon3D> polygons, Point3D vector, int layersize)
        {
            mID = EntityId.Extrusion;
            mLayerBit = new byte[layersize / 8];
            mPolygons = polygons;
            mVector = vector;
            mClose = true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polylines">ポリライン</param>
        /// <param name="vector">押出ベクトル</param>
        /// <param name="layersize">レイヤサイズ</param>
        public ExtrusionEntity(List<Polyline3D> polylines, Point3D vector, int layersize)
        {
            mID = EntityId.Extrusion;
            mLayerBit = new byte[layersize / 8];
            mPolygons = new List<Polygon3D>();
            foreach (Polyline3D polyline in polylines) 
                mPolygons.Add(new Polygon3D(polyline));
            mVector = vector;
            mClose = false;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            ExtrusionEntity extrusion = new ExtrusionEntity(mLayerBit.Length * 8);
            extrusion.copyProperty(this);
            foreach (Polygon3D polygon in mPolygons)
                extrusion.mPolygons.Add(polygon.toCopy());
            extrusion.mVector = mVector.toCopy();
            extrusion.mClose = mClose;
            return extrusion;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = createSideSurface(mPolygons, mVector);
            if (mEdgeDisp)
                mSurfaceDataList.AddRange(createSurface(mPolygons, mVector));
        }

        /// <summary>
        /// 端面のサーフェースデータの作成
        /// </summary>
        /// <param name="polygons">ポリゴン</param>
        /// <param name="vector">押出ベクトル</param>
        /// <returns>サーフェースデータ</returns>
        private List<SurfaceData> createSurface(List<Polygon3D> polygons, Point3D vector)
        {
            bool triangleDraw = true;
            List<SurfaceData> surfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            Polygon3D polygon = polygons[0];
            List<Polygon3D> innerPolygon = new List<Polygon3D>();
            innerPolygon = polygons.Skip(1).ToList();
            //  上面
            surfaceData.mVertexList = polygon.holePlate2Quads(innerPolygon, triangleDraw);
            bool reverse = surfaceWise(surfaceData.mVertexList, vector);
            reverse = mEdgeReverse ? !reverse : reverse;
            surfaceData.mDrawType = triangleDraw ? DRAWTYPE.TRIANGLES : DRAWTYPE.QUADS;
            surfaceData.mFaceColor = mFaceColor;
            surfaceData.reverse(reverse);
            surfaceDataList.Add(surfaceData);
            //  下面
            surfaceData = new SurfaceData();
            surfaceData.mVertexList = surfaceDataList[surfaceDataList.Count - 1].mVertexList.ConvertAll(p => p.toCopy());
            surfaceData.mVertexList.ForEach(p => p.translate(vector));
            reverse = surfaceWise(surfaceData.mVertexList, vector);
            reverse = mEdgeReverse ? !reverse : reverse;
            surfaceData.mDrawType = triangleDraw ? DRAWTYPE.TRIANGLES : DRAWTYPE.QUADS;
            surfaceData.mFaceColor = mFaceColor;
            surfaceData.reverse(!reverse);
            surfaceDataList.Add(surfaceData);
            return surfaceDataList;
        }


        /// <summary>
        /// 複数ポリゴンの3D側面表示
        /// </summary>
        /// <param name="polygons">ポリゴンリスト</param>
        /// <returns>サーフェースデータ</returns>
        private List<SurfaceData> createSideSurface(List<Polygon3D> polygons, Point3D vector)
        {
            List<SurfaceData> surfaceDataList = new List<SurfaceData>();
            for (int i = 0; i < polygons.Count; i++) {
                SurfaceData surfaceData = new SurfaceData();
                if (mClose) {
                    surfaceData.mVertexList = polygons[i].sideFace2Quads(vector);
                } else {
                    Polyline3D polyline = new Polyline3D(polygons[i], false);
                    surfaceData.mVertexList = polyline.sideFace2Quads(vector);
                }
                surfaceData.mDrawType = DRAWTYPE.QUADS;
                surfaceData.mFaceColor = mFaceColor;
                surfaceData.reverse(!mReverse);
                surfaceDataList.Add(surfaceData);
            }
            return surfaceDataList;
        }

        /// <summary>
        /// 面の向きが押出方向と同じかを求める
        /// </summary>
        /// <param name="vertex">座標リスト</param>
        /// <param name="vec">押出方向</param>
        /// <returns>押出方向に対する面の向き</returns>
        private bool surfaceWise(List<Point3D> vertex, Point3D vec)
        {
            int n = 1;
            List<Point3D> plist = new List<Point3D>() { vertex[0] };
            if (plist[0].length(vertex[n]) > 1e-6)
                plist.Add(vertex[n]);
            else
                plist.Add(vertex[++n]);
            if (plist[1].length(vertex[++n]) > 1e-6)
                plist.Add(vertex[n]);
            else
                plist.Add(vertex[++n]);
            Point3D normal = vertex[0].getNormal(vertex[1], vertex[2]);
            return (Math.PI / 2) > normal.angle(vec);
        }


        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<Polyline3D> ();
            for (int i = 0; i < mPolygons.Count; i++) {
                mVertexList.Add (mPolygons[i].toPolyline3D(0, mClose));
                Polyline3D polyline = mPolygons[i].toPolyline3D(0, mClose);
                polyline.translate(mVector);
                mVertexList.Add(polyline);
                mVertexList.AddRange(createSideLineData(mPolygons[i], mVector));
            }
        }

        /// <summary>
        /// 側面の線分作成
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="vec">押出ベクトル</param>
        /// <returns>ポリラインリスト</returns>
        private List<Polyline3D> createSideLineData(Polygon3D polygon, Point3D vec)
        {
            List<Polyline3D> polylineList = new List<Polyline3D> ();
            List<Point3D> plist = polygon.toPoint3D(mDivAngle);
            for (int i = 0; i < plist.Count; i++) {
                Point3D p1 = plist[i].toCopy();
                p1.type = 0;
                Point3D p2 = p1.toCopy();
                p2.type = 0;
                p2.add(vec);
                List<Point3D> sideLine = new List<Point3D>() { p1, p2 };
                polylineList.Add(new Polyline3D(sideLine));
            }
            return polylineList;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            foreach (var polygon in mPolygons)
                polygon.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mVector.rotate(new Point3D(), ang, face);
            foreach (var polygon in mPolygons)
                polygon.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            foreach (var polygon in mPolygons)
                polygon.offset(sp, ep);
        }

        /// <summary>
        /// 線分の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mVector = new Line3D(new Point3D(), ep - sp).mirror(mVector, face);
            Line3D l = new Line3D(sp, ep);
            foreach (var polygon in mPolygons)
                polygon.mirror(l, face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mVector.length(mVector.length() * scale);
            foreach (var polygon in mPolygons)
                polygon.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            Point3D pos = new Point3D(pickPos, face);
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="pos">分割位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割要素リスト</returns>
        public override List<Entity> divide(Point3D pos, FACE3D face)
        {
            List<Entity> entitys = new List<Entity>();
            return entitys;
        }

        /// <summary>
        /// 要素同士の交点
        /// </summary>
        /// <param name="entity">対象要素</param>
        /// <param name="pos">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>交点座標</returns>
        public override Point3D intersection(Entity entity, PointD pos, FACE3D face)
        {
            return null;
        }

        /// <summary>
        /// 3D座標点リストの取得
        /// </summary>
        /// <returns>座標点リスト</returns>
        public override List<Point3D> toPointList()
        {
            return null;
        }

        /// <summary>
        /// 図形データを文字列から設定
        /// </summary>
        /// <param name="dataList">データ文字列リスト</param>
        public override void setDataText(List<string> dataList)
        {
            int n = 0;
            if (dataList == null || dataList[n++] != "ExtrusionData") return;
            List<Point3D> plist = new List<Point3D>();
            Polygon3D polygon;
            mPolygons.Clear();
            while (n < dataList.Count && dataList[n] != "DataEnd") {
                string[] buf = dataList[n++].Split(',');
                int i = 0;
                if (buf[i] == "押出ベクトル") {
                    mVector.x = ylib.doubleParse(buf[++i]);
                    mVector.y = ylib.doubleParse(buf[++i]);
                    mVector.z = ylib.doubleParse(buf[++i]);
                } else if (buf[i] == "閉領域") {
                    mClose = ylib.boolParse(buf[++i]);
                } else if (buf[i] == "PolygonData") {
                    int np = ylib.intParse(buf[++i]);
                    int count = ylib.intParse(buf[++i]);
                    plist = new List<Point3D>();
                } else if (buf[i] == "PolygonEnd") {
                    polygon = new Polygon3D(plist);
                    mPolygons.Add(polygon);
                } else if (0 <= ylib.intParse(buf[i], -1)) {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(buf[++i]);
                    p.y = ylib.doubleParse(buf[++i]);
                    p.z = ylib.doubleParse(buf[++i]);
                    p.type = ylib.intParse(buf[++i]);
                    plist.Add(p);
                }
            }
        }

        /// <summary>
        /// 図形データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string> getDataText()
        {
            List<string> dataList = new List<string> {
                "ExtrusionData",
                $"押出ベクトル,{mVector.x},{mVector.y},{mVector.z}",
                $"閉領域,{mClose}"
            };
            for (int i = 0; i < mPolygons.Count; i++) {
                dataList.Add($"PolygonData,{i},{mPolygons[i].mPolygon.Count}");
                List<Point3D> plist = mPolygons[i].toPoint3D();
                for (int j = 0; j < plist.Count; j++) {
                    string buf = $"{j}, {plist[j].x},{plist[j].y},{plist[j].z},{plist[j].type}";
                    dataList.Add(buf);
                }
                dataList.Add("PolygonEnd");
            }
            dataList.Add("DataEnd");
            return dataList;
        }

        /// <summary>
        /// データを文字列リストに変換(saveFile)
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string[]> toDataList()
        {
            List<string[]> dataList = new List<string[]>();
            dataList.Add(new string[] {
                "ExtrusionData",
            });
            dataList.Add(new string[] {
                "Vector", mVector.x.ToString(), mVector.y.ToString(), mVector.z.ToString(),
            });
            dataList.Add(new string[] { "Loop", mClose.ToString(), });
            foreach (var polygon in mPolygons)
                dataList.AddRange(toDataList(polygon));
            dataList.Add(new string[] { "DataEnd" });
            return dataList;
        }

        /// <summary>
        /// ポリゴンデータを文字列化
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <returns>文字列リスト</returns>
        private List<string[]> toDataList(Polygon3D polygon)
        {
            List<string[]> dataList = new List<string[]>();
            List<string> buf = new List<string>() {
                "PolygonData",
                "Cp", polygon.mPlane.mCp.x.ToString(), polygon.mPlane.mCp.y.ToString(), polygon.mPlane.mCp.z.ToString(),
                "U", polygon.mPlane.mU.x.ToString(), polygon.mPlane.mU.y.ToString(), polygon.mPlane.mU.z.ToString(),
                "V", polygon.mPlane.mV.x.ToString(), polygon.mPlane.mV.y.ToString(), polygon.mPlane.mV.z.ToString(),
                "Size", polygon.mPolygon.Count.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() { "PList" };
            for (int i = 0; i < polygon.mPolygon.Count; i++) {
                buf.Add(polygon.mPolygon[i].x.ToString());
                buf.Add(polygon.mPolygon[i].y.ToString());
                buf.Add(polygon.mPolygon[i].type.ToString());
            }
            dataList.Add(buf.ToArray());
            return dataList;
        }

        /// <summary>
        /// 文字列データを設定(loadFile)
        /// </summary>
        /// <param name="dataList">文字列データリスト</param>
        /// <param name="sp">データ位置</param>
        /// <returns>終了データ位置</returns>
        public override int setDataList(List<string[]> dataList, int sp)
        {
            mPolygons = new List<Polygon3D>();
            Polygon3D polygon = new Polygon3D();
            while (dataList[sp][0] != "DataEnd") {
                if (dataList[sp][0] == "Vector") {
                    mVector.x = ylib.doubleParse(dataList[sp][1]);
                    mVector.y = ylib.doubleParse(dataList[sp][2]);
                    mVector.z = ylib.doubleParse(dataList[sp][3]);
                } else if (dataList[sp][0] == "Loop") {
                    mClose = ylib.boolParse(dataList[sp][1]);
                } else if (dataList[sp][0] == "PolygonData") {
                    polygon = new Polygon3D();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            polygon.mPlane.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            polygon.mPlane.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            polygon.mPlane.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            polygon.mPlane.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            polygon.mPlane.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            polygon.mPlane.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            polygon.mPlane.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            polygon.mPlane.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            polygon.mPlane.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Size") {
                            int size = ylib.intParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "PList") {
                    polygon.mPolygon = new List<PointD>();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(dataList[sp][i]);
                        p.y = ylib.doubleParse(dataList[sp][++i]);
                        p.type = ylib.intParse(dataList[sp][++i]);
                        polygon.mPolygon.Add(p);
                    }
                    mPolygons.Add(polygon);
                }
                sp++;
            }
            return ++sp;
        }

        /// <summary>
        /// Mini3DCadの要素データの読込
        /// </summary>
        /// <param name="dataList">文字列データリスト</param>
        /// <param name="sp">データ位置</param>
        /// <returns>終了データ位置</returns>
        public override int setElementDataList(List<string[]> dataList, int sp)
        {
            try {
                while (sp < dataList.Count) {
                    string[] list = dataList[sp];
                    if (0 == list.Length || list[0].IndexOf("ExtrusionData") < 0)
                        break;
                    string np = list[0].Substring("ExtrusionData".Length);
                    if (np.Length == 0) {
                        Polygon3D polygon = setDataListBase(list);
                        if (0 < polygon.mPolygon.Count)
                            mPolygons.Add(polygon);
                    } else {
                        mPolygons.Add(getPolygonDataList(list));
                    }
                    sp++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Extrusion setDataList {e.ToString()}");
            }
            return sp;
        }

        /// <summary>
        /// 押出の外形データを要素化
        /// </summary>
        /// <param name="list">文字列データリスト</param>
        /// <returns>ポリゴン</returns>
        private Polygon3D setDataListBase(string[] list)
        {
            Polygon3D polygon = new Polygon3D();
            int ival;
            double val;
            bool bval;
            int i = 1;
            int count;
            bool multi = false;
            while (i < list.Length) {
                if (list[i] == "Vector") {
                    mVector.x = ylib.doubleParse(list[++i]);
                    mVector.y = ylib.doubleParse(list[++i]);
                    mVector.z = ylib.doubleParse(list[++i]);
                } else if (list[i] == "Close") {
                    mEdgeDisp = bool.TryParse(list[++i], out bval) ? bval : true;
                } else if (list[i] == "Loop") {
                    mClose = bool.TryParse(list[++i], out bval) ? bval : true;
                } else if (list[i] == "Cp") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polygon.mPlane.mCp = p;
                } else if (list[i] == "U") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polygon.mPlane.mU = p;
                } else if (list[i] == "V") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polygon.mPlane.mV = p;
                } else if (list[i] == "Size") {
                    count = ylib.intParse(list[++i]);
                } else if (list[i] == "Multi") {
                    multi = ylib.boolParse(list[++i]);
                } else if (ylib.IsNumberString(list[i])) {
                    PointD p = new PointD();
                    p.x = ylib.doubleParse(list[i]);
                    p.y = ylib.doubleParse(list[++i]);
                    if (multi)
                        p.type = ylib.intParse(list[++i]);
                    polygon.mPolygon.Add(p);
                }
                i++;
            }
            return polygon;
        }

        /// <summary>
        /// 押出の内部データを要素化
        /// </summary>
        /// <param name="list">文字列データリスト</param>
        /// <returns>ポリゴン</returns>
        private Polygon3D getPolygonDataList(string[] list)
        {
            Polygon3D polygon = new Polygon3D();
            int ival;
            double val;
            bool bval;
            int i = 1;
            int count;
            bool multi = false;
            while (i < list.Length) {
                if (list[i] == "Cp") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polygon.mPlane.mCp = p;
                } else if (list[i] == "U") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polygon.mPlane.mU = p;
                } else if (list[i] == "V") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polygon.mPlane.mV = p;
                } else if (list[i] == "Size") {
                    count = ylib.intParse(list[++i]);
                } else if (list[i] == "Multi") {
                    multi = ylib.boolParse(list[++i]);
                } else {
                    PointD p = new PointD();
                    p.x = ylib.doubleParse(list[i]);
                    p.y = ylib.doubleParse(list[++i]);
                    if (multi)
                        p.type = ylib.intParse(list[++i]);
                    polygon.mPolygon.Add(p);
                }
                i++;
            }
            return polygon;
        }
    }
}
