using CoreLib;

namespace Cad3DApp
{
    /// <summary>
    /// ポリゴン要素
    /// </summary>
    public class PolygonEntity : Entity
    {
        public Polygon3D mPolygon;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤーサイズ</param>
        public PolygonEntity(int layersize)
        {
            mID = EntityId.Polygon;
            mPolygon = new Polygon3D();
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="layersize">レイヤサイズ</param>
        public PolygonEntity(Polygon3D polygon, int layersize)
        {
            mID = EntityId.Polygon;
            mPolygon = polygon;
            mPolygon.squeeze();
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            PolygonEntity polygon = new PolygonEntity(mPolygon.toCopy(), mLayerBit.Length * 8);
            polygon.copyProperty(this);
            return polygon;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            bool triangleDraw = true;
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mPolygon.holePlate2Quads(null, triangleDraw);
            surfaceData.mDrawType = triangleDraw ? DRAWTYPE.TRIANGLES : DRAWTYPE.QUADS;
            surfaceData.mFaceColor = mFaceColor;
            surfaceData.reverse(mReverse);
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<Polyline3D> {
                mPolygon.toPolyline3D(0, true, mDivAngle)
            };
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mPolygon.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mPolygon.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolygon.offset(sp, ep);
        }

        /// <summary>
        /// 座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mPolygon.mirror(new Line3D(sp, ep), face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
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
            mPolygon.scale(cp, scale);
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            mPolygon.stretch(vec, new Point3D(pickPos, face), arc);
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="pos">分割位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割要素リスト</returns>
        public override List<Entity> divide(Point3D pos, FACE3D face)
        {
            Polyline3D polyline = mPolygon.divide(pos.toPoint(face), face);
            List<Entity> entitys = new List<Entity>();
            PolylineEntity polylineEntity = new PolylineEntity(polyline.toCopy(), mLayerBit.Length * 8);
            polylineEntity.copyProperty(this);
            entitys.Add(polylineEntity);
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
                return mPolygon.intersection(line, pos, face);
            } else if (entity.mID == EntityId.Arc) {
                Arc3D arc = ((ArcEntity)entity).mArc;
                return mPolygon.intersection(arc, pos, face);
            } else if (entity.mID == EntityId.Polyline) {
                Polyline3D polyline = ((PolylineEntity)entity).mPolyline;
                return mPolygon.intersection(polyline, pos, face);
            } else if (entity.mID == EntityId.Polygon) {
                Polygon3D polygon = ((PolygonEntity)entity).mPolygon;
                return polygon.intersection(mPolygon, pos, face);
            }
            return null;
        }

        /// <summary>
        /// 3D座標点リストの取得
        /// </summary>
        /// <returns>座標点リスト</returns>
        public override List<Point3D> toPointList()
        {
            return mPolygon.toPoint3D(0, true);
        }

        /// <summary>
        /// 図形データを文字列から設定
        /// </summary>
        /// <param name="dataList">データ文字列リスト</param>
        public override void setDataText(List<string> dataList)
        {
            int n = 0;
            if (dataList == null || dataList[n++] != "PolygonData") return;
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
                mPolygon = new Polygon3D(plist);
        }

        /// <summary>
        /// 図形データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string> getDataText()
        {
            List<string> dataList = new List<string> {
                "PolygonData",
                "PointList"
            };
            List<Point3D> plist = mPolygon.toPoint3D();
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
                "PolygonData",
                "Cp", mPolygon.mPlane.mCp.x.ToString(), mPolygon.mPlane.mCp.y.ToString(), mPolygon.mPlane.mCp.z.ToString(),
                "U", mPolygon.mPlane.mU.x.ToString(), mPolygon.mPlane.mU.y.ToString(), mPolygon.mPlane.mU.z.ToString(),
                "V", mPolygon.mPlane.mV.x.ToString(), mPolygon.mPlane.mV.y.ToString(), mPolygon.mPlane.mV.z.ToString(),
                "Size", mPolygon.mPolygon.Count.ToString(),
            };
            dataList.Add(buf.ToArray());
            buf = new List<string>() { "PList" };
            for (int i = 0; i < mPolygon.mPolygon.Count; i++) {
                buf.Add(mPolygon.mPolygon[i].x.ToString());
                buf.Add(mPolygon.mPolygon[i].y.ToString());
                buf.Add(mPolygon.mPolygon[i].type.ToString());
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
                if (dataList[sp][0] == "PolygonData") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            mPolygon.mPlane.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            mPolygon.mPlane.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            mPolygon.mPlane.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            mPolygon.mPlane.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            mPolygon.mPlane.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            mPolygon.mPlane.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mPolygon.mPlane.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mPolygon.mPlane.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mPolygon.mPlane.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Size") {
                            size = ylib.intParse(dataList[sp][++i]);
                        }
                    }
                } else if (dataList[sp][0] == "PList") {
                    mPolygon.mPolygon = new List<PointD>();
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(dataList[sp][i]);
                        p.y = ylib.doubleParse(dataList[sp][++i]);
                        p.type = ylib.intParse(dataList[sp][++i]);
                        mPolygon.mPolygon.Add(p);
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
            string[] list = dataList[sp++];
            if (0 == list.Length || list[0] != "PolygonData")
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
                        mPolygon.mPlane.mCp = p;
                    } else if (list[i] == "U") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mPolygon.mPlane.mU = p;
                    } else if (list[i] == "V") {
                        Point3D p = new Point3D();
                        p.x = ylib.doubleParse(list[++i]);
                        p.y = ylib.doubleParse(list[++i]);
                        p.z = ylib.doubleParse(list[++i]);
                        mPolygon.mPlane.mV = p;
                    } else if (list[i] == "Size") {
                        count = ylib.intParse(list[++i]);
                    } else if (list[i] == "Multi") {
                        multi = ylib.boolParse(list[++i]);
                    } else if (ylib.IsNumberString(list[i])) {
                        PointD p = new PointD();
                        p.x = ylib.doubleParse(list[i]);
                        p.y = ylib.doubleParse(list[++i]);
                        if (multi)
                            p.type = int.TryParse(list[++i], out ival) ? ival : 0;
                        mPolygon.mPolygon.Add(p);
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Polygon setDataList {e.ToString()}");
            }
            return sp;
        }
    }
}
