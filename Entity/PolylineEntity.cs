using CoreLib;

namespace Cad3DApp
{
    /// <summary>
    /// ポリライン要素
    /// </summary>
    public class PolylineEntity : Entity
    {
        public Polyline3D mPolyline;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤサイズ</param>
        public PolylineEntity(int layersize)
        {
            mID = EntityId.Polyline;
            mPolyline = new Polyline3D();
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polyline">ポリライン</param>
        /// <param name="layersize">レイヤサイズ</param>
        public PolylineEntity(Polyline3D polyline, int layersize)
        {
            mID = EntityId.Polyline;
            mPolyline = polyline;
            mPolyline.squeeze();
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            PolylineEntity polyline = new PolylineEntity(mPolyline.toCopy(), mLayerBit.Length * 8);
            polyline.copyProperty(this);
            return polyline;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mPolyline.toPoint3D(mDivAngle);
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mFaceColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<Polyline3D> { mPolyline };
            //mVertexList = new List<List<Point3D>>();
            //mVertexList.Add(mPolyline.toPoint3D());
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mPolyline.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mPolyline.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolyline.offset(sp, ep);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolyline.mirror(new Line3D(sp, ep), face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolyline.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mPolyline.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            mPolyline.stretch(vec, new Point3D(pickPos, face), arc);
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="pos">分割位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割要素リスト</returns>
        public override List<Entity> divide(Point3D pos, FACE3D face)
        {
            List<Polyline3D> polylineList = mPolyline.divide(pos.toPoint(face), face);
            List<Entity> entitys = new List<Entity>();
            foreach (Polyline3D polyline in polylineList) {
                PolylineEntity polylineEntity = new PolylineEntity(polyline.toCopy(), mLayerBit.Length * 8);
                polylineEntity.copyProperty(this);
                entitys.Add(polylineEntity);
            }
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
            if (entity.mID == EntityId.Line) {
                Line3D line = ((LineEntity)entity).mLine;
                return mPolyline.intersection(line, pos, face);
            } else if (entity.mID == EntityId.Arc) {
                Arc3D arc = ((ArcEntity)entity).mArc;
                return mPolyline.intersection(arc, pos, face);
            } else if (entity.mID == EntityId.Polyline) {
                Polyline3D polyline = ((PolylineEntity)entity).mPolyline;
                return polyline.intersection(mPolyline, pos, face);
            } else if (entity.mID == EntityId.Polygon) {
                Polygon3D polygon = ((PolygonEntity)entity).mPolygon;
                return polygon.intersection(mPolyline, pos, face);
            }
            return null;
        }

        /// <summary>
        /// 3D座標点リストの取得
        /// </summary>
        /// <returns>座標点リスト</returns>
        public override List<Point3D> toPointList()
        {
            return mPolyline.toPoint3D();
        }


        /// <summary>
        /// 図形データを文字列から設定
        /// </summary>
        /// <param name="dataList">データ文字列リスト</param>
        public override void setDataText(List<string> dataList)
        {
            int n = 0;
            if (dataList == null || dataList[n++] != "PolylineData") return;
            List<Point3D> plist = new List<Point3D>();
            while (n < dataList.Count && dataList[n] != "DataEnd") {
                string[] buf = dataList[n++].Split(',');
                int i = 0;
                if (buf[i] == "PointList") {
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
                mPolyline = new Polyline3D(plist);
        }

        /// <summary>
        /// 図形データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string> getDataText()
        {
            List<string> dataList = new List<string> {
                "PolylineData",
                "PointList"
            };
            List<Point3D> plist = mPolyline.toPoint3D();
            for (int i = 0; i < plist.Count; i++) {
                string buf = $"{i}, {plist[i].x},{plist[i].y},{plist[i].z},{plist[i].type}";
                dataList.Add(buf);
            }
            dataList.Add("DataEnd");
            return dataList;
        }

        /// <summary>
        /// データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string[]> toDataList()
        {
            List<string[]> dataList = new List<string[]>();
            List<string> buf = new List<string>() {
                "PolylineData",
                "Cp", mPolyline.mCp.x.ToString(), mPolyline.mCp.y.ToString(), mPolyline.mCp.z.ToString(),
                "U", mPolyline.mU.x.ToString(), mPolyline.mU.y.ToString(), mPolyline.mU.z.ToString(),
                "V", mPolyline.mV.x.ToString(), mPolyline.mV.y.ToString(), mPolyline.mV.z.ToString(),
                "Size", mPolyline.mPolyline.Count.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() { "PList" };
            for (int i = 0; i < mPolyline.mPolyline.Count; i++) {
                buf.Add(mPolyline.mPolyline[i].x.ToString());
                buf.Add(mPolyline.mPolyline[i].y.ToString());
                buf.Add(mPolyline.mPolyline[i].type.ToString());
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
                if (dataList[sp][0] == "PolylineData") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            mPolyline.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            mPolyline.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            mPolyline.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            mPolyline.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            mPolyline.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            mPolyline.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mPolyline.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mPolyline.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mPolyline.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Size") {
                            size = ylib.intParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "PList") {
                    mPolyline.mPolyline = new List<PointD>();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(dataList[sp][i]);
                        p.y = ylib.doubleParse(dataList[sp][++i]);
                        p.type = ylib.intParse(dataList[sp][++i]);
                        mPolyline.mPolyline.Add(p);
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
            if (0 == list.Length || list[0] != "PolylineData")
                return sp;
            try {
                int ival;
                double val;
                int count;
                bool multi = false;
                bool bval;
                for (int i = 1; i < list.Length; i++) {
                    if (list[i] == "Cp") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mPolyline.mCp = p;
                    } else if (list[i] == "U") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mPolyline.mU = p;
                    } else if (list[i] == "V") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mPolyline.mV = p;
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
                        mPolyline.mPolyline.Add(p);
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Polyline setDataList {e.ToString()}");
            }
            return ++sp;
        }
    }
}
