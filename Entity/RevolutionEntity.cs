using CoreLib;

namespace Cad3DApp
{
    /// <summary>
    /// 回転体
    /// </summary>
    public class RevolutionEntity : Entity
    {
        public Line3D mCenterLine;
        public Polyline3D mOutLine;
        public double mSa = 0;
        public double mEa = Math.PI * 2;
        public bool mLoop = true;
        public double mMinDivCount = 4;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤーサイズ</param>
        public RevolutionEntity(int layersize)
        {
            mID = EntityId.Revolution;
            mLayerBit = new byte[layersize / 8];
            mCenterLine = new Line3D();
            mOutLine = new Polyline3D();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="centerLine">中心線</param>
        /// <param name="polyline">外形線</param>
        /// <param name="close">閉領域</param>
        /// <param name="layersize">レイヤーサイズ</param>
        public RevolutionEntity(Line3D centerLine, Polyline3D polyline, bool close, int layersize)
        {
            mID = EntityId.Revolution;
            mLayerBit = new byte[layersize / 8];
            mCenterLine = centerLine.toCopy();
            mOutLine = polyline.toCopy();
            mLoop = close;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="centerLine">中心線</param>
        /// <param name="polyline">外形線</param>
        /// <param name="sa">始角</param>
        /// <param name="ea">終角</param>
        /// <param name="close">閉領域</param>
        /// <param name="layersize">レイヤーサイズ</param>
        public RevolutionEntity(Line3D centerLine, Polyline3D polyline, double sa, double ea, bool close, int layersize)
        {
            mID = EntityId.Revolution;
            mLayerBit = new byte[layersize / 8];
            mCenterLine = centerLine.toCopy();
            mOutLine = polyline.toCopy();
            mSa = sa;
            mEa = ea;
            mLoop = close;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            RevolutionEntity revolution = new RevolutionEntity(mCenterLine, mOutLine, mLoop, mLayerBit.Length * 8);
            revolution.mSa = mSa;
            revolution.mEa = mEa;
            revolution.copyProperty(this);
            return revolution;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            bool triangleDraw = true;
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData;
            //  回転座標作成
            List<List<Point3D>> outLines;
            outLines = getCenterLineRotate(mCenterLine, mOutLine.toPoint3D(mDivAngle), mDivAngle);
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
            //  端面表示
            if (mEdgeDisp && (mEa - mSa < Math.PI * 2)) {
                Polygon3D polygon0 = new Polygon3D(outLines[0]);
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = polygon0.holePlate2Quads(null, triangleDraw);
                surfaceData.mDrawType = triangleDraw ? DRAWTYPE.TRIANGLES : DRAWTYPE.QUADS;
                surfaceData.mFaceColor = mFaceColor;
                surfaceData.reverse(mEdgeReverse);
                mSurfaceDataList.Add(surfaceData);
                Polygon3D polygon1 = new Polygon3D(outLines[^1]);
                surfaceData = new SurfaceData();
                surfaceData.mVertexList = polygon1.holePlate2Quads(null, triangleDraw);
                surfaceData.mDrawType = triangleDraw ? DRAWTYPE.TRIANGLES : DRAWTYPE.QUADS;
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
            mVertexList = new List<Polyline3D> ();
            List<List<Point3D>> outLines;
            double divideAngle = mDivAngle < (Math.PI / 6) ? mDivAngle * 2 : mDivAngle;
            outLines = getCenterLineRotate(mCenterLine, mOutLine.toPoint3D(mDivAngle), divideAngle);
            for (int i = 0; i < outLines.Count; i++)
                mVertexList.Add(new Polyline3D(outLines[i]));
            for (int i = 0; i < outLines[0].Count; i++) {
                List<Point3D> plist = new List<Point3D>();
                for (int j = 0; j < outLines.Count; j++) {
                    plist.Add(outLines[j][i]);
                }
                mVertexList.Add(new Polyline3D(plist));
            }
        }

        /// <summary>
        /// 回転体の外形線作成
        /// </summary>
        /// <param name="centerline">中心線</param>
        /// <param name="outline">外形線</param>
        /// <param name="divideAngle">分割角度</param>
        /// <returns></returns>
        private List<List<Point3D>> getCenterLineRotate(Line3D centerline, List<Point3D> outline, double divideAngle)
        {
            List<List<Point3D>> outLines = new List<List<Point3D>>();
            Point3D cp = centerline.mSp;
            Point3D cv = cp.vector(centerline.endPoint());    //  中心線ベクトル
            cp.inverse();
            outline.ForEach(p => p.add(cp));
            cp.inverse();
            double ang = mSa;
            double dang = divideAngle;
            if ((mEa - mSa) / dang < mMinDivCount) dang = (mEa - mSa) / mMinDivCount;
            while ((ang - dang) < mEa) {
                if (mEa < ang)
                    ang = mEa;
                List<Point3D> plist = outline.ConvertAll(p => p.toCopy());
                plist.ForEach(p => p.rotate(cv, ang));
                plist.ForEach(p => p.add(cp));
                outLines.Add(plist);
                ang += dang;
            }
            return outLines;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mCenterLine.translate(v);
            mOutLine.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mCenterLine.rotate(cp, ang, face);
            mOutLine.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mCenterLine.offset(sp, ep);
            mOutLine.offset(sp, ep);
        }

        /// <summary>
        /// 線分の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mCenterLine.mirror(sp, ep);
            mOutLine.mirror(sp, ep);
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
            mCenterLine.scale(cp, scale);
            mOutLine.scale(cp, scale);
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
            if (dataList == null || dataList[n++] != "中心線") return;
            while (n < dataList.Count && dataList[n] != "DataEnd") {
                string[] buf = dataList[n++].Split(',');
                int i = 0;
                if (buf[i].Trim() == "始点") {
                    mCenterLine.mSp.x = ylib.doubleParse(buf[++i]);
                    mCenterLine.mSp.y = ylib.doubleParse(buf[++i]);
                    mCenterLine.mSp.z = ylib.doubleParse(buf[++i]);
                } else if (buf[i].Trim() == "終点") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(buf[++i]);
                    p.y = ylib.doubleParse(buf[++i]);
                    p.z = ylib.doubleParse(buf[++i]);
                    mCenterLine.mV = p - mCenterLine.mSp;
                } else if (buf[i] == "外形線") {
                    continue;
                } else if (buf[i] == "始角") {
                    mSa = ylib.D2R(ylib.doubleParse(buf[++i]));
                } else if (buf[i] == "終角") {
                    mEa = ylib.D2R(ylib.doubleParse(buf[++i]));
                } else if (buf[i] == "ループ") {
                    mLoop = ylib.boolParse(buf[++i]);
                } else if (buf[i] == "座標リスト") {
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
                mOutLine = new Polyline3D(plist);
        }

        /// <summary>
        /// 図形データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string> getDataText()
        {
            List<string> dataList = new List<string> {
                "中心線",
                $"始点, {mCenterLine.mSp.x.ToString()}, {mCenterLine.mSp.y.ToString()}, {mCenterLine.mSp.z.ToString()},",
                $"終点, {mCenterLine.endPoint().x.ToString()}, {mCenterLine.endPoint().y.ToString()}, {mCenterLine.endPoint().z.ToString()}",
            };
            dataList.AddRange(new List<string> {
                "外形線",
                $"始角, {ylib.R2D(mSa).ToString()}",
                $"終角, {ylib.R2D(mEa).ToString()}",
                $"ループ, {mLoop.ToString()}",
                "座標リスト"
            });
            List<Point3D> plist = mOutLine.toPoint3D();
            for (int i = 0; i < plist.Count; i++) {
                string buf = $"{i}, {plist[i].x},{plist[i].y},{plist[i].z},{plist[i].type}";
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
            dataList.Add(new string[] {
                "CenterLineData",
                "Sp", mCenterLine.mSp.x.ToString(), mCenterLine.mSp.y.ToString(), mCenterLine.mSp.z.ToString(),
                "V", mCenterLine.mV.x.ToString(), mCenterLine.mV.y.ToString(), mCenterLine.mV.z.ToString(),
            });
            List<string> buf = new List<string>() {
                "OutlineData",
                "Cp", mOutLine.mPlane.mCp.x.ToString(), mOutLine.mPlane.mCp.y.ToString(), mOutLine.mPlane.mCp.z.ToString(),
                "U", mOutLine.mPlane.mU.x.ToString(), mOutLine.mPlane.mU.y.ToString(), mOutLine.mPlane.mU.z.ToString(),
                "V", mOutLine.mPlane.mV.x.ToString(), mOutLine.mPlane.mV.y.ToString(), mOutLine.mPlane.mV.z.ToString(),
                "Sa", mSa.ToString(),
                "Ea", mEa.ToString(),
                "Loop", mLoop.ToString(),
                "Size", mOutLine.mPolyline.Count.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() { "PList" };
            for (int i = 0; i < mOutLine.mPolyline.Count; i++) {
                buf.Add(mOutLine.mPolyline[i].x.ToString());
                buf.Add(mOutLine.mPolyline[i].y.ToString());
                buf.Add(mOutLine.mPolyline[i].type.ToString());
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
                if (dataList[sp][0] == "CenterLineData") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Sp") {
                            mCenterLine.mSp.x = ylib.doubleParse(dataList[sp][++i]);
                            mCenterLine.mSp.y = ylib.doubleParse(dataList[sp][++i]);
                            mCenterLine.mSp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mCenterLine.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mCenterLine.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mCenterLine.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "OutlineData") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            mOutLine.mPlane.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine.mPlane.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine.mPlane.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            mOutLine.mPlane.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine.mPlane.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine.mPlane.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mOutLine.mPlane.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine.mPlane.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mOutLine.mPlane.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Sa") {
                            mSa = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Ea") {
                            mEa = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Loop") {
                            mLoop = ylib.boolParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Size") {
                            size = ylib.intParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "PList") {
                    mOutLine.mPolyline = new List<PointD>();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(dataList[sp][i]);
                        p.y = ylib.doubleParse(dataList[sp][++i]);
                        p.type = ylib.intParse(dataList[sp][++i]);
                        mOutLine.mPolyline.Add(p);
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
            if (0 == list.Length || list[0] != "RevolutionData")
                return sp;
            try {
                mCenterLine = new Line3D();
                mOutLine = new Polyline3D();
                int ival;
                double val;
                bool bval;
                int i = 1;
                int count = 0;
                bool multi = false;
                while (i < list.Length) {
                    if (list[i] == "StartAngle") {
                        mSa = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "EndAngle") {
                        mEa = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "Close") {
                        mEdgeDisp = ylib.boolParse(list[++i]);
                    } else if (list[i] == "CenterLineSp") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mCenterLine.mSp = p;
                    } else if (list[i] == "CenterLineV") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mCenterLine.mV = p;
                    } else if (list[i] == "OutLineCp") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine.mPlane.mCp = p;
                    } else if (list[i] == "OutLineU") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine.mPlane.mU = p;
                    } else if (list[i] == "OutLineV") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mOutLine.mPlane.mV = p;
                    } else if (list[i] == "OutLineSize") {
                        count = ylib.intParse(list[++i]);
                    } else if (list[i] == "Multi") {
                        multi = ylib.boolParse(list[++i]);
                    } else if (list[i] == "OutLine") {
                        for (int j = 0; j < count; j++) {
                            PointD p = new PointD();
                            p.x = ylib.doubleParse(list[++i]);
                            p.y = ylib.doubleParse(list[++i]);
                            if (multi)
                                p.type = ylib.intParse(list[++i]);
                            mOutLine.mPolyline.Add(p);
                        }
                    }
                    i++;
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Revolution setDataList {e.ToString()}");
            }
            return ++sp;
        }
    }
}
