using CoreLib;

namespace Cad3DApp
{
    public class BlendEntity : Entity
    {
        public List<Polyline3D> mPolylines;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤーサイズ</param>
        public BlendEntity(int layersize)
        {
            mID = EntityId.Blend;
            mPolylines = new List<Polyline3D>();
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polylines">ポリライン</param>
        /// <param name="layersize">レイヤーサイズ</param>
        public BlendEntity(List<Polyline3D> polylines, int layersize)
        {
            mID = EntityId.Blend;
            mPolylines = polylines.ConvertAll(p => p.toCopy());
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygons">ポリゴン</param>
        /// <param name="layersize">レイヤーサイズ</param>
        public BlendEntity(List<Polygon3D> polygons, int layersize)
        {
            mID = EntityId.Blend;
            mPolylines = polygons.ConvertAll(p => p.toPolyline3D());
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            BlendEntity blend = new BlendEntity(mLayerBit.Length * 8);
            blend.copyProperty(this);
            foreach (Polyline3D polyline in mPolylines)
                blend.mPolylines.Add(polyline.toCopy());
            return blend;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            bool reverse = mReverse;
            mSurfaceDataList = new List<SurfaceData>();
            for (int i = 1; i < mPolylines.Count; i++) {
                SurfaceData surfaceData = new SurfaceData();
                surfaceData.mVertexList = createSurfaceData(mPolylines[i - 1].toPoint3D(), mPolylines[i].toPoint3D());
                surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
                surfaceData.mFaceColor = mFaceColor;
                surfaceData.reverse(reverse);
                mSurfaceDataList.Add(surfaceData);
            }
            if (mEdgeDisp) {
                Point3D normal = mPolylines[0].toPoint3D(0) - mPolylines.Last().toPoint3D(0);
                Polygon3D polygon0 = new Polygon3D(mPolylines[0]);
                SurfaceData surfaceData0 = createEdgeFaceData(polygon0);
                Point3D v0 = surfaceData0.mVertexList[0].getNormal(surfaceData0.mVertexList[1], surfaceData0.mVertexList[2]);
                reverse = (Math.PI / 2) > normal.angle(v0) ? mEdgeReverse : !mEdgeReverse;
                surfaceData0.reverse(reverse);
                mSurfaceDataList.Add(surfaceData0);
                SurfaceData surfaceData1 = createEdgeFaceData(new Polygon3D(mPolylines.Last()));
                surfaceData1.reverse(!reverse);
                mSurfaceDataList.Add(surfaceData1);
            }
        }

        /// <summary>
        /// ブレンドの端面を作成
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        private SurfaceData createEdgeFaceData(Polygon3D polygon)
        {
            bool triangleDraw = true;
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = polygon.holePlate2Quads(null, triangleDraw);
            surfaceData.mDrawType = triangleDraw ? DRAWTYPE.TRIANGLES : DRAWTYPE.QUADS;
            surfaceData.mFaceColor = mFaceColor;
            return surfaceData;
        }


        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<Polyline3D>();
            for (int i = 0; i < mPolylines.Count; i++)
                mVertexList.Add(mPolylines[i]);
            for (int i = 1; i < mPolylines.Count; i++) {
                List<Point3D> plist = createSurfaceData(mPolylines[i - 1].toPoint3D(), mPolylines[i].toPoint3D(), 1);
                for (int j = 0; j < plist.Count; j += 2) {
                    List<Point3D> line = new List<Point3D>() {
                        plist[j], plist[j + 1]
                    };
                    mVertexList.Add(new Polyline3D(line));
                }
            }
        }

        /// <summary>
        /// ポリライン同士のブレンドによる座標点リストの作成
        /// </summary>
        /// <param name="plist1">ポリライン</param>
        /// <param name="plist2">ポリライン</param>
        /// <param name="st">開始位置</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> createSurfaceData(List<Point3D> plist1, List<Point3D> plist2, int st = 0)
        {
            Line3D line1, line2;
            Arc3D arc1, arc2;
            List<Point3D> plist = new List<Point3D>() { plist1[0], plist2[0] };
            for (int i = 0, j = 0; i < plist1.Count - 1 && j < plist2.Count - 1; i++, j++) {
                if (i < plist1.Count - 2 && plist1[i + 1].type == 1 && i < plist1.Count - 2) {
                    line1 = null;
                    arc1 = new Arc3D(plist1[i], plist1[i + 1], plist1[i + 2]);
                    i++;
                } else {
                    line1 = new Line3D(plist1[i], plist1[i + 1]);
                    arc1 = null;
                }
                if (j < plist2.Count - 2 && plist2[j + 1].type == 1 && j < plist2.Count - 2) {
                    line2 = null;
                    arc2 = new Arc3D(plist2[j], plist2[j + 1], plist2[j + 2]);
                    j++;
                } else {
                    line2 = new Line3D(plist2[j], plist2[j + 1]);
                    arc2 = null;
                }
                if (line1 != null && line2 != null) {
                    plist.AddRange(createVertexData(line1, line2));
                } else if (line1 != null && arc2 != null) {
                    plist.AddRange(createVertexData(line1, arc2, false, st));
                } else if (arc1 != null && line2 != null) {
                    plist.AddRange(createVertexData(line2, arc1, false, st));
                } else if (arc1 != null && arc2 != null) {
                    plist.AddRange(createVertexData(arc1, arc2));
                }
            }
            return plist;
        }

        /// <summary>
        /// 線分同士のブレンド(QUADS_STRP)
        /// </summary>
        /// <param name="line1">線分</param>
        /// <param name="line2">線分</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> createVertexData(Line3D line1, Line3D line2)
        {
            List<Point3D> plist = new List<Point3D>() { line1.endPoint(), line2.endPoint() };
            return plist;
        }

        /// <summary>
        /// 線分と円弧のブレンド(QUADS_STRP)
        /// </summary>
        /// <param name="line">線分</param>
        /// <param name="arc">円弧</param>
        /// <param name="reverse">反転</param>
        /// <param name="st">開始位置</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> createVertexData(Line3D line, Arc3D arc, bool reverse = false, int st = 0)
        {
            List<Point3D> arcPlist = arc.toPoint3D(mDivAngle);
            if (!arc.mCcw) arcPlist.Reverse();
            List<Point3D> linePlist = line.toPoint3D(arcPlist.Count - 1);
            List<Point3D> plist = new List<Point3D>();
            for (int i = st; i < arcPlist.Count; i++) {
                if (reverse) {
                    plist.Add(arcPlist[i]);
                    plist.Add(linePlist[i]);
                } else {
                    plist.Add(linePlist[i]);
                    plist.Add(arcPlist[i]);
                }
            }
            return plist;
        }

        /// <summary>
        /// 円同士のブレンド(QUADS_STRP)
        /// </summary>
        /// <param name="arc1">円弧</param>
        /// <param name="arc2">円弧</param>
        /// <returns>座標点リスト</returns>
        private List<Point3D> createVertexData(Arc3D arc1, Arc3D arc2)
        {
            int n = 1;
            if (arc1.mOpenAngle < arc2.mOpenAngle)
                n = (int)(arc2.mOpenAngle / mDivAngle) + 1;
            else
                n = (int)(arc1.mOpenAngle / mDivAngle) + 1;
            List<Point3D> arc1Plist = arc1.toPoint3D(n);
            if (!arc1.mCcw) arc1Plist.Reverse();
            List<Point3D> arc2Plist = arc2.toPoint3D(n);
            List<Point3D> plist = new List<Point3D>();
            for (int i = 1; i < arc1Plist.Count && i < arc2Plist.Count; i++) {
                plist.Add(arc1Plist[i]);
                plist.Add(arc2Plist[i]);
            }
            return plist;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            foreach (var polyline in mPolylines)
                polyline.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            foreach (var polyline in mPolylines)
                polyline.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            foreach (var polyline in mPolylines)
                polyline.offset(sp, ep);
        }

        /// <summary>
        /// 線分の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            foreach (var polyline in mPolylines)
                polyline.mirror(l, face);
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
            foreach (var polyline in mPolylines)
                polyline.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
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
            if (dataList == null || dataList[n++] != "BlendData") return;
            List<Point3D> plist = new List<Point3D>();
            Polyline3D polyline;
            mPolylines.Clear();
            while (n < dataList.Count && dataList[n] != "DataEnd") {
                string[] buf = dataList[n++].Split(',');
                int i = 0;
                if (buf[i] == "PolylineData") {
                    int np = ylib.intParse(buf[++i]);
                    int count = ylib.intParse(buf[++i]);
                    plist = new List<Point3D>();
                } else if (buf[i] == "PolylineEnd") {
                    polyline = new Polyline3D(plist);
                    mPolylines.Add(polyline);
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
                "BlendData",
            };
            for (int i = 0; i < mPolylines.Count; i++) {
                dataList.Add($"PolylineData,{i},{mPolylines[i].mPolyline.Count}");
                List<Point3D> plist = mPolylines[i].toPoint3D();
                for (int j = 0; j < plist.Count; j++) {
                    string buf = $"{j}, {plist[j].x},{plist[j].y},{plist[j].z},{plist[j].type}";
                    dataList.Add(buf);
                }
                dataList.Add("PolylineEnd");
            }
            dataList.Add("DataEnd");
            return dataList;
        }

        /// <summary>
        /// 要素固有データの文字配列に変換(saveFile)
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public override List<string[]> toDataList()
        {
            List<string[]> dataList = new List<string[]>() { new string[] { "Blend", } };
            foreach (var polyline in mPolylines)
                dataList.AddRange(toPolylineDataList(polyline));
            dataList.Add(new string[] { "DataEnd" });
            return dataList;
        }

        /// <summary>
        /// ポリラインデータを文字列化
        /// </summary>
        /// <param name="polyline">ポリライン</param>
        /// <returns>文字列配列リスト<returns>
        private List<string[]> toPolylineDataList(Polyline3D polyline)
        {
            List<string[]> dataList = new List<string[]>();
            List<string> buf = new List<string>() {
                "PolylineData",
                "Cp", polyline.mCp.x.ToString(), polyline.mCp.y.ToString(), polyline.mCp.z.ToString(),
                "U", polyline.mU.x.ToString(), polyline.mU.y.ToString(), polyline.mU.z.ToString(),
                "V", polyline.mV.x.ToString(), polyline.mV.y.ToString(), polyline.mV.z.ToString(),
                "Size", polyline.mPolyline.Count.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() { "PList" };
            for (int i = 0; i < polyline.mPolyline.Count; i++) {
                buf.Add(polyline.mPolyline[i].x.ToString());
                buf.Add(polyline.mPolyline[i].y.ToString());
                buf.Add(polyline.mPolyline[i].type.ToString());
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
            mPolylines = new List<Polyline3D>();
            Polyline3D polyline = new Polyline3D();
            while (dataList[sp][0] != "DataEnd") {
                if (dataList[sp][0] == "PolylineData") {
                    polyline = new Polyline3D();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            polyline.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            polyline.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            polyline.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            polyline.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            polyline.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            polyline.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            polyline.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            polyline.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            polyline.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Size") {
                            int size = ylib.intParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "PList") {
                    polyline.mPolyline = new List<PointD>();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(dataList[sp][i]);
                        p.y = ylib.doubleParse(dataList[sp][++i]);
                        p.type = ylib.intParse(dataList[sp][++i]);
                        polyline.mPolyline.Add(p);
                    }
                    mPolylines.Add(polyline);
                }
                sp++;
            }
            return ++sp;
        }

        /// <summary>
        /// Mini3DCadの要素データの読込(import)
        /// </summary>
        /// <param name="dataList">文字列データリスト</param>
        /// <param name="sp">データ位置</param>
        /// <returns>終了データ位置</returns>
        public override int setElementDataList(List<string[]> dataList, int sp)
        {
            mPolylines = new List<Polyline3D>();
            try {
                while (sp < dataList.Count) {
                    string[] list = dataList[sp];
                    if (0 == list.Length)
                        break;
                    if (list[0] == "BlendPolylineData1") {
                        mPolylines.Add(getPolylineDataList(list));
                    } else if (list[0] == "BlendPolylineData2") {
                        mPolylines.Add(getPolylineDataList(list));
                    } else if (list[0] == "BlendData1") {
                        mPolylines.Add(getPolylineDataList(list, true));
                    } else if (list[0] == "BlendData2") {
                        mPolylines.Add(getPolylineDataList(list, true));
                    } else
                        break;
                    sp++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Blend setDataList {e.ToString()}");
            }
            return sp;
        }

        /// <summary>
        /// 文字データからポリラインデータの取得
        /// </summary>
        /// <param name="list">文字配列</param>
        /// <returns>ポリライン</returns>
        private Polyline3D getPolylineDataList(string[] list, bool polygon = false)
        {
            Polyline3D polyline = new Polyline3D();
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
                    polyline.mCp = p;
                } else if (list[i] == "U") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polyline.mU = p;
                } else if (list[i] == "V") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(list[++i]);
                    p.y = ylib.doubleParse(list[++i]);
                    p.z = ylib.doubleParse(list[++i]);
                    polyline.mV = p;
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
                    polyline.mPolyline.Add(p);
                }
                i++;
            }
            if (polygon)
                polyline.mPolyline.Add(polyline.mPolyline[0]);
            return polyline;
        }
    }
}
