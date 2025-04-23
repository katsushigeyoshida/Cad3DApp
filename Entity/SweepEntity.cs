using CoreLib;

namespace Cad3DApp
{
    public class SweepEntity : Entity
    {
        public Polyline3D mOutLine1;
        public Polyline3D mOutLine2;
        public double mSa = 0;
        public double mEa = Math.PI * 2;
        public bool mLoop = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤーサイズ(bit数)</param>
        public SweepEntity(int layersize)
        {
            mID = EntityId.Sweep;
            mLayerBit = new byte[layersize / 8];
            mOutLine1 = new Polyline3D();
            mOutLine2 = new Polyline3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <param name="close">閉領域</param>
        /// <param name="layersize">レイヤーサイズ</param>
        public SweepEntity(Polyline3D outline1, Polyline3D outline2, bool close, int layersize)
        {
            mID = EntityId.Sweep;
            mLayerBit = new byte[layersize / 8];
            mOutLine1 = outline1.toCopy();
            mOutLine2 = outline2.toCopy();
            mLoop = close;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            SweepEntity sweep = new SweepEntity(mLayerBit.Length * 8);
            sweep.copyProperty(this);
            sweep.mOutLine1 = mOutLine1.toCopy();
            sweep.mOutLine2 = mOutLine2.toCopy();
            sweep.mSa = mSa;
            sweep.mEa = mEa;
            sweep.mLoop = mLoop;
            return sweep;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            //  回転座標作成
            List<List<Point3D>> outLines;
            if (!directChk(mOutLine1, mOutLine2))
                mOutLine2.mPolyline.Reverse();
            outLines = rotateOutlines(mOutLine1, mOutLine2, mDivAngle);
            //  Surfaceの作成
            for (int i = 0; i < outLines.Count - 1; i++) {
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = new List<Point3D>();
                for (int j = 0; j < outLines[i].Count; j++) {
                    surfaceData.mVertexList.Add(outLines[i + 1][j]);
                    surfaceData.mVertexList.Add(outLines[i][j]);
                }
                surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
                surfaceData.mFaceColor = mFaceColor;
                surfaceData.reverse(mReverse);
                mSurfaceDataList.Add(surfaceData);
            }
            if (mLoop) {
                //  筒状にする場合
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = new List<Point3D>();
                for (int j = 0; j < outLines[^1].Count; j++) {
                    surfaceData.mVertexList.Add(outLines[0][j]);
                    surfaceData.mVertexList.Add(outLines[^1][j]);
                }
                surfaceData.mDrawType = DRAWTYPE.QUAD_STRIP;
                surfaceData.mFaceColor = mFaceColor;
                surfaceData.reverse(mReverse);
                mSurfaceDataList.Add(surfaceData);
            }
            if (mEdgeDisp) {
                //  端面表示
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = new List<Point3D>();
                Line3D line1 = new Line3D(outLines[0][0], outLines[(int)(outLines.Count / 2)][0]);
                surfaceData.mVertexList.Add(line1.centerPoint());
                for (int i = 0; i < outLines.Count; i++) {
                    surfaceData.mVertexList.Add(outLines[i][0]);
                }
                surfaceData.mDrawType = DRAWTYPE.TRIANGLE_FAN;
                surfaceData.mFaceColor = mFaceColor;
                surfaceData.reverse(mEdgeReverse);
                mSurfaceDataList.Add(surfaceData);
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = new List<Point3D>();
                Line3D line2 = new Line3D(outLines[0][^1], outLines[(int)(outLines.Count / 2)][^1]);
                surfaceData.mVertexList.Add(line2.centerPoint());
                for (int i = outLines.Count - 1; 0 <= i ; i--) {
                    surfaceData.mVertexList.Add(outLines[i][^1]);
                }
                surfaceData.mDrawType = DRAWTYPE.TRIANGLE_FAN;
                surfaceData.mFaceColor = mFaceColor;
                surfaceData.reverse(mEdgeReverse);
                mSurfaceDataList.Add(surfaceData);
            }
        }

        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<Polyline3D>();
            List<List<Point3D>> outLines;
            if (!directChk(mOutLine1, mOutLine2))
                mOutLine2.mPolyline.Reverse();
            double divideAngle = mDivAngle < (Math.PI / 6) ? mDivAngle * 2 : mDivAngle;
            outLines = rotateOutlines(mOutLine1, mOutLine2, divideAngle);
            outLines.ForEach(p => mVertexList.Add(new Polyline3D(p, false)));
            for (int j = 0; j < outLines[0].Count; j++) {
                List<Point3D> plist = new List<Point3D>();
                for (int i = 0; i < outLines.Count; i++) {
                    plist.Add(outLines[i][j].toCopy());
                }
                mVertexList.Add(new Polyline3D(plist));
            }
        }

        /// <summary>
        /// 外形線同士の方向チェック
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <param name="face">作成2D平面</param>
        /// <returns></returns>
        private bool directChk(Polyline3D outline1, Polyline3D outline2)
        {
            Point3D sp1 = outline1.toFirstPoint3D();
            Point3D ep1 = outline1.toLastPoint3D();
            Point3D sp2 = outline2.toFirstPoint3D();
            Point3D ep2 = outline2.toLastPoint3D();
            Line3D l1 = new Line3D(sp1, sp2);
            Line3D l2 = new Line3D(ep1, ep2);
            Point3D ip = l1.intersection(l2);
            if (ip == null)
                return true;
            if (l1.onPoint(ip))
                return false;
            else
                return true;
        }

        /// <summary>
        /// 回転外形線の作成
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <returns>回転外形線リスト</returns>
        private List<List<Point3D>> rotateOutlines(Polyline3D outline1, Polyline3D outline2, double divideAngle)
        {
            List<List<Point3D>> outLines = new List<List<Point3D>>();
            //  中心線リスト
            (List<Line3D> centerlines, List<Line3D> outlines) = getCenterlines(outline1, outline2);
            double ang = mSa;
            double dang = divideAngle;
            while (ang < mEa) {
                List<Point3D> plist = new List<Point3D>();
                for (int i = 0; i < centerlines.Count; i++) {
                    Point3D cp = centerlines[i].mSp;
                    Point3D cv = centerlines[i].mV;
                    Point3D sp = outlines[i].mSp.toCopy();
                    Point3D ep = outlines[i].endPoint();
                    sp.sub(cp);
                    ep.sub(cp);
                    sp.rotate(cv, ang);
                    ep.rotate(cv, ang);
                    sp.add(cp);
                    ep.add(cp);
                    plist.Add(sp);
                    plist.Add(ep);
                }
                ang += dang;
                outLines.Add(plist);
            }
            return outLines;
        }

        /// <summary>
        /// 中心線リストの作成
        /// </summary>
        /// <param name="outline1">外形線1</param>
        /// <param name="outline2">外形線2</param>
        /// <returns>中心線リスト、外形線リスト</returns>
        private (List<Line3D> centerlines, List<Line3D> outlines) getCenterlines(Polyline3D outline1, Polyline3D outline2)
        {
            List<Line3D> centerlines = new List<Line3D>();
            List<Line3D> outlines = new List<Line3D>();
            int lineCount = Math.Min(outline1.mPolyline.Count, outline2.mPolyline.Count);
            for (int i = 0; i < lineCount - 1; i++) {
                Line3D l1 = new Line3D(outline1.toPoint3D(i), outline1.toPoint3D(i + 1));
                Line3D l2 = new Line3D(outline2.toPoint3D(i), outline2.toPoint3D(i + 1));
                (Line3D centerline, Line3D outline) = getCenterline(l1, l2);
                centerlines.Add(centerline);
                outlines.Add(outline);
            }
            return (centerlines, outlines);
        }

        /// <summary>
        /// ２線の中心線を求める
        /// </summary>
        /// <param name="l1">外形線分1</param>
        /// <param name="l2">外形線分2</param>
        /// <returns>中心線</returns>
        private (Line3D centerline, Line3D outline) getCenterline(Line3D l1, Line3D l2)
        {
            Point3D cp1, sp, cp2, ep;
            if (l1.mV.angle(l2.mV) < Math.PI / 180) {
                (cp1, sp) = getStartCenter(l1, l2);
                l1.reverse();
                l2.reverse();
                (cp2, ep) = getStartCenter(l1, l2);
                l1.reverse();
                l2.reverse();
            } else {
                Line3D cl1 = new Line3D(l1.mSp, l2.mSp);
                cp1 = cl1.centerPoint();
                sp = l1.mSp.toCopy();
                Line3D cl2 = new Line3D(l1.endPoint(), l2.endPoint());
                cp2 = cl2.centerPoint();
                ep = l1.endPoint();
            }
            return (new Line3D(cp1, cp2), new Line3D(sp, ep));
        }

        /// <summary>
        /// 外形線の中心を求める
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        private (Point3D cp, Point3D sp) getStartCenter(Line3D l1, Line3D l2)
        {
            Point3D ip1 = l1.intersection(l2.mSp);
            Point3D ip2 = l2.intersection(ip1);
            if (!l1.onPoint(ip1) || !l2.onPoint(ip2)) {
                ip2 = l2.intersection(l1.mSp);
                ip1 = l1.intersection(ip2);
            }
            Line3D l = new Line3D(ip1, ip2);
            Point3D cp = l.centerPoint();
            Point3D sp = ip1;
            return (cp, sp);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mOutLine1.translate(v);
            mOutLine2.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mOutLine1.rotate(cp, ang, face);
            mOutLine2.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mOutLine1.offset(sp, ep);
            mOutLine2.offset(sp, ep);
        }

        /// <summary>
        /// 線分の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mOutLine1.mirror(new Line3D(sp, ep), face);
            mOutLine2.mirror(new Line3D(sp, ep), face);
            YLib.Swap(ref mOutLine1, ref mOutLine2);
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
            mOutLine1.scale(cp, scale);
            mOutLine2.scale(cp, scale);
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
            List<Point3D> plist = new List<Point3D>();
            if (dataList == null || dataList[n++] != "掃引") return;
            while (n < dataList.Count && dataList[n] != "DataEnd") {
                string[] buf = dataList[n++].Split(',');
                int i = 0;
                if (buf[i] == "始角") {
                    mSa = ylib.D2R(ylib.doubleParse(buf[++i]));
                } else if (buf[i] == "終角") {
                    mEa = ylib.D2R(ylib.doubleParse(buf[++i]));
                } else if (buf[i] == "ループ") {
                    mLoop = ylib.boolParse(buf[++i]);
                } else if (buf[i] == "ポリライン1座標リスト") {
                    continue;
                } else if (buf[i] == "ポリライン2座標リスト") {
                    if (0 < plist.Count)
                        mOutLine1 = new Polyline3D(plist);
                    plist = new List<Point3D>();
                    continue;
                } else if (0 <= ylib.intParse(buf[i], -1)) {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(buf[++i]);
                    p.y = ylib.doubleParse(buf[++i]);
                    p.z = ylib.doubleParse(buf[++i]);
                    p.type = ylib.intParse(buf[++i]);
                    plist.Add(p);
                }
            }
            if (0 < plist.Count)
                mOutLine2 = new Polyline3D(plist);
        }

        /// <summary>
        /// 図形データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string> getDataText()
        {
            List<string> dataList = new List<string> {
                "掃引",
                $"始角, {ylib.R2D(mSa).ToString()}",
                $"終角, {ylib.R2D(mEa).ToString()}",
                $"ループ, {mLoop.ToString()}",
                "ポリライン1座標リスト"
            };
            List<Point3D> plist1 = mOutLine1.toPoint3D();
            for (int i = 0; i < plist1.Count; i++) {
                string buf = $"{i}, {plist1[i].x},{plist1[i].y},{plist1[i].z},{plist1[i].type}";
                dataList.Add(buf);
            }
            dataList.Add("ポリライン2座標リスト");
            List<Point3D> plist2 = mOutLine2.toPoint3D();
            for (int i = 0; i < plist2.Count; i++) {
                string buf = $"{i}, {plist2[i].x},{plist2[i].y},{plist2[i].z},{plist2[i].type}";
                dataList.Add(buf);
            }
            dataList.Add("DataEnd");
            return dataList;
        }

        /// <summary>
        /// 要素固有データの文字配列に変換(ファイル保存用)
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public override List<string[]> toDataList()
        {
            List<string[]> dataList = new List<string[]>();
            List<string> buf = new List<string>() {
                "SweepData",
                "Sa", mSa.ToString(),
                "Ea", mEa.ToString(),
                "Loop", mLoop.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() {
                "Outline1Data",
                "Cp", mOutLine1.mCp.x.ToString(), mOutLine1.mCp.y.ToString(), mOutLine1.mCp.z.ToString(),
                "U", mOutLine1.mU.x.ToString(), mOutLine1.mU.y.ToString(), mOutLine1.mU.z.ToString(),
                "V", mOutLine1.mV.x.ToString(), mOutLine1.mV.y.ToString(), mOutLine1.mV.z.ToString(),
                "Size", mOutLine1.mPolyline.Count.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() { "PList1" };
            for (int i = 0; i < mOutLine1.mPolyline.Count; i++) {
                buf.Add(mOutLine1.mPolyline[i].x.ToString());
                buf.Add(mOutLine1.mPolyline[i].y.ToString());
                buf.Add(mOutLine1.mPolyline[i].type.ToString());
            }
            dataList.Add(buf.ToArray());
            buf = new List<string>() {
                "Outline2Data",
                "Cp", mOutLine2.mCp.x.ToString(), mOutLine2.mCp.y.ToString(), mOutLine2.mCp.z.ToString(),
                "U", mOutLine2.mU.x.ToString(), mOutLine2.mU.y.ToString(), mOutLine2.mU.z.ToString(),
                "V", mOutLine2.mV.x.ToString(), mOutLine2.mV.y.ToString(), mOutLine2.mV.z.ToString(),
                "Size", mOutLine2.mPolyline.Count.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() { "PList2" };
            for (int i = 0; i < mOutLine2.mPolyline.Count; i++) {
                buf.Add(mOutLine2.mPolyline[i].x.ToString());
                buf.Add(mOutLine2.mPolyline[i].y.ToString());
                buf.Add(mOutLine2.mPolyline[i].type.ToString());
            }
            dataList.Add(buf.ToArray());
            dataList.Add(new string[] { "DataEnd" });

            return dataList;
        }

        /// <summary>
        /// 文字列データを設定
        /// </summary>
        /// <param name="dataList">文字列データリスト</param>
        /// <param name="sp">データ位置</param>
        /// <returns>終了データ位置</returns>
        public override int setDataList(List<string[]> dataList, int sp)
        {
            int size = 0;
            while (dataList[sp][0] != "DataEnd") {
                if (dataList[sp][0] == "SweepData") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Sa") {
                            mSa = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Ea") {
                            mEa = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Loop") {
                            mLoop = ylib.boolParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "Outline1Data") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            mOutLine1.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine1.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine1.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            mOutLine1.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine1.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine1.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mOutLine1.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine1.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine1.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Size") {
                            size = ylib.intParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "PList1") {
                    mOutLine1.mPolyline = new List<PointD>();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(dataList[sp][i]);
                        p.y = ylib.doubleParse(dataList[sp][++i]);
                        p.type = ylib.intParse(dataList[sp][++i]);
                        mOutLine1.mPolyline.Add(p);
                    }
                } else if (dataList[sp][0] == "Outline2Data") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            mOutLine2.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine2.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine2.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            mOutLine2.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine2.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine2.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mOutLine2.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine2.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine2.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Size") {
                            size = ylib.intParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "PList2") {
                    mOutLine2.mPolyline = new List<PointD>();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(dataList[sp][i]);
                        p.y = ylib.doubleParse(dataList[sp][++i]);
                        p.type = ylib.intParse(dataList[sp][++i]);
                        mOutLine2.mPolyline.Add(p);
                    }
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
            string[] list = dataList[sp];
            if (0 == list.Length || list[0] != "SweepData")
                return sp;
            try {
                mOutLine1 = new Polyline3D();
                mOutLine2 = new Polyline3D();
                int ival;
                double val;
                bool bval;
                int i = 1;
                int count = 0;
                bool multi1 = false;
                bool multi2 = false;
                while (i < list.Length) {
                    if (list[i] == "StartAngle") {
                        mSa = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "EndAngle") {
                        mEa = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "Close") {
                        mEdgeDisp = ylib.boolParse(list[++i]);
                    } else if (list[i] == "OutLine1Cp") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine1.mCp = p;
                    } else if (list[i] == "OutLine1U") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine1.mU = p;
                    } else if (list[i] == "OutLine1V") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine1.mV = p;
                    } else if (list[i] == "OutLine1Size") {
                        count = ylib.intParse(list[++i]);
                    } else if (list[i] == "Multi1") {
                        multi1 = ylib.boolParse(list[++i]);
                    } else if (list[i] == "OutLine1") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = ylib.doubleParse(list[++i]);
                            p.y = ylib.doubleParse(list[++i]);
                            if (multi1)
                                p.type = ylib.intParse(list[++i]);
                            mOutLine1.mPolyline.Add(p);
                        }
                    } else if (list[i] == "OutLine2Cp") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine2.mCp = p;
                    } else if (list[i] == "OutLine2U") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine2.mU = p;
                    } else if (list[i] == "OutLine2V") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine2.mV = p;
                    } else if (list[i] == "OutLine2Size") {
                        count = ylib.intParse(list[++i]);
                    } else if (list[i] == "Multi2") {
                        multi2 = ylib.boolParse(list[++i]);
                    } else if (list[i] == "OutLine2") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = ylib.doubleParse(list[++i]);
                            p.y = ylib.doubleParse(list[++i]);
                            if (multi2)
                                p.type = ylib.intParse(list[++i]);
                            mOutLine2.mPolyline.Add(p);
                        }
                    }
                    i++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Sweep setDataList {e.ToString()}");
            }
            return ++sp;
        }
    }
}
