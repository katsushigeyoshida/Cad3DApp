using CoreLib;

namespace Cad3DApp
{
    /// <summary>
    /// 線分要素
    /// </summary>
    public class LineEntity : Entity
    {
        public Line3D mLine;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤーサイズ</param>
        public LineEntity(int layersize)
        {
            mID = EntityId.Line;
            mLine = new Line3D();
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="line">線分</param>
        /// <param name="layersize">レイヤーサイズ</param>
        public LineEntity(Line3D line, int layersize)
        {
            mID = EntityId.Line;
            mLine = line;
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            LineEntity line = new LineEntity(mLine.toCopy(), mLayerBit.Length * 8);
            line.copyProperty(this);
            return line;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mLine.toPoint3D();
            surfaceData.mDrawType = DRAWTYPE.LINE_STRIP;
            surfaceData.mFaceColor = mFaceColor;
            mSurfaceDataList.Add(surfaceData);
        }

        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public override void createVertexData()
        {
            mVertexList = new List<Polyline3D> {
                new Polyline3D(mLine)
            };
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mLine.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mLine.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Point3D v = ep - sp;
            mLine.offset(v);
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
            mLine = l.mirror(mLine, face);
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
            mLine.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mLine.scale(cp, scale);
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
            mLine.stretch(vec, pos);
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="pos">分割位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割要素リスト</returns>
        public override List<Entity> divide(Point3D pos, FACE3D face)
        {
            List<Line3D> lineList = mLine.divide(pos.toPoint(face), face);
            List<Entity> entitys = new List<Entity>();
            foreach (Line3D line in lineList) {
                LineEntity lineEntity = new LineEntity(line.toCopy(), mLayerBit.Length * 8);
                lineEntity.copyProperty(this);
                entitys.Add(lineEntity);
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
                return line.intersection(mLine, face);
            } else if (entity.mID == EntityId.Arc) {
                Arc3D arc = ((ArcEntity)entity).mArc;
                return arc.intersection(mLine, pos, face);
            } else if (entity.mID == EntityId.Polyline) {
                Polyline3D polyline = ((PolylineEntity)entity).mPolyline;
                return polyline.intersection(mLine, pos, face);
            } else if (entity.mID == EntityId.Polygon) {
                Polygon3D polygon = ((PolygonEntity)entity).mPolygon;
                return polygon.intersection(mLine, pos, face);
            }
            return null;
        }

        /// <summary>
        /// 3D座標点リストの取得
        /// </summary>
        /// <returns>座標点リスト</returns>
        public override List<Point3D> toPointList()
        {
            return mLine.toPoint3D();
        }


        /// <summary>
        /// 図形データを文字列から設定
        /// </summary>
        /// <param name="dataList">データ文字列リスト</param>
        public override void setDataText(List<string> dataList)
        {
            int n = 0;
            if (dataList == null || dataList[n++] != "LineData") return;
            while (n < dataList.Count && dataList[n] != "DataEnd") {
                string[] buf = dataList[n++].Split(',');
                int i = 0;
                if (buf[i].Trim() == "始点") {
                    mLine.mSp.x = ylib.doubleParse(buf[++i]);
                    mLine.mSp.y = ylib.doubleParse(buf[++i]);
                    mLine.mSp.z = ylib.doubleParse(buf[++i]);
                } else if (buf[i].Trim() == "終点") {
                    Point3D p = new Point3D();
                    p.x = ylib.doubleParse(buf[++i]);
                    p.y = ylib.doubleParse(buf[++i]);
                    p.z = ylib.doubleParse(buf[++i]);
                    mLine.mV = p - mLine.mSp;
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
                "LineData",
                $"始点, {mLine.mSp.x.ToString()}, {mLine.mSp.y.ToString()}, {mLine.mSp.z.ToString()},",
                $"終点, {mLine.endPoint().x.ToString()}, {mLine.endPoint().y.ToString()}, {mLine.endPoint().z.ToString()}",
                "DataEnd"
            };
            return dataList;
        }

        /// <summary>
        /// データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public override List<string[]> toDataList()
        {
            List<string[]> dataList = new List<string[]>();
            dataList.Add(new string[] {
                "LineData",
                "Sp", mLine.mSp.x.ToString(), mLine.mSp.y.ToString(), mLine.mSp.z.ToString(),
                "V", mLine.mV.x.ToString(), mLine.mV.y.ToString(), mLine.mV.z.ToString(),
            });
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
            while (dataList[sp][0] != "DataEnd") {
                if (dataList[sp][0] == "LineData") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Sp") {
                            mLine.mSp.x = ylib.doubleParse(dataList[sp][++i]);
                            mLine.mSp.y = ylib.doubleParse(dataList[sp][++i]);
                            mLine.mSp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mLine.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mLine.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mLine.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        }
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
            if (0 == list.Length || list[0] != "LineData")
                return sp;
            try {
                double val;
                for (int i = 1; i < list.Length; i++) {
                    if (list[i] == "Sp") {
                        mLine.mSp.x = ylib.doubleParse(list[++i]);
                        mLine.mSp.y = ylib.doubleParse(list[++i]);
                        mLine.mSp.z = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "Ep") {
                        Point3D ep = new Point3D();
                        ep.x = ylib.doubleParse(list[++i]);
                        ep.y = ylib.doubleParse(list[++i]);
                        ep.z = ylib.doubleParse(list[++i]);
                        mLine.mV = ep - mLine.mSp;
                    } else if (list[i] == "V") {
                        mLine.mV.x = ylib.doubleParse(list[++i]);
                        mLine.mV.y = ylib.doubleParse(list[++i]);
                        mLine.mV.z = ylib.doubleParse(list[++i]);
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Line setDataList {e.ToString()}");
            }
            return sp;
        }
    }
}
