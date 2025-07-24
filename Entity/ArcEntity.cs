using CoreLib;

namespace Cad3DApp
{
    /// <summary>
    /// 円弧要素
    /// </summary>
    public class ArcEntity : Entity
    {
        public Arc3D mArc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layersize">レイヤーサイズ</param>
        public ArcEntity(int layersize)
        {
            mID = EntityId.Arc;
            mArc = new Arc3D();
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="arc">円弧</param>
        /// <param name="layersize">レイヤーサイズ</param>
        public ArcEntity(Arc3D arc, int layersize)
        {
            mID = EntityId.Arc;
            mArc = arc;
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns>Entity</returns>
        public override Entity toCopy()
        {
            ArcEntity arc = new ArcEntity(mArc.toCopy(), mLayerBit.Length * 8);
            arc.copyProperty(this);
            return arc;
        }

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public override void createSurfaceData()
        {
            mSurfaceDataList = new List<SurfaceData>();
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mArc.toPoint3D(mDivAngle);
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
                new Polyline3D(mArc, 0)
            };
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public override void translate(Point3D v)
        {
            mArc.translate(v);
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">表示面</param>
        public override void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face)
        {
            mArc.rotate(cp, ang, face);
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public override void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mArc.offset(sp, ep);
        }

        /// <summary>
        /// 円弧の座標を反転する
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="outline">3Dデータと外形線の作成</param>
        public override void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            Line3D l = new Line3D(sp, ep);
            mArc.mirror(l, face);
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public override void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face)
        {
            mArc.trim(sp, ep);
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public override void scale(Point3D cp, double scale, PointD pickPos, FACE3D face)
        {
            mArc.mPlane.mCp.scale(cp, scale);
            mArc.mR *= scale;
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">2D平面</param>
        public override void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face)
        {
            mArc.stretch(vec, new Point3D(pickPos, face));
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="pos">分割位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割要素リスト</returns>
        public override List<Entity> divide(Point3D pos, FACE3D face)
        {
            List<Arc3D> arcList = mArc.divide(pos.toPoint(face), face);
            List<Entity> entitys = new List<Entity>();
            foreach (Arc3D arc in arcList) {
                ArcEntity arcEntity = new ArcEntity(arc.toCopy(), mLayerBit.Length * 8);
                arcEntity.copyProperty(this);
                entitys.Add(arcEntity);
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
                return mArc.intersection(line, pos, face);
            } else if (entity.mID == EntityId.Arc) {
                Arc3D arc = ((ArcEntity)entity).mArc;
                return arc.intersection(mArc, pos, face);
            } else if (entity.mID == EntityId.Polyline) {
                Polyline3D polyline = ((PolylineEntity)entity).mPolyline;
                return polyline.intersection(mArc, pos, face);
            } else if (entity.mID == EntityId.Polygon) {
                Polygon3D polygon = ((PolygonEntity)entity).mPolygon;
                return polygon.intersection(mArc, pos, face);
            }
            return null;
        }

        /// <summary>
        /// 3D座標点リストの取得
        /// </summary>
        /// <returns>座標点リスト</returns>
        public override List<Point3D> toPointList()
        {
            return mArc.toPoint3D();
        }


        /// <returns>文字列リスト</returns>
        /// <summary>
        /// 図形データを文字列から設定
        /// </summary>
        /// <param name="dataList">データ文字列リスト</param>
        public override void setDataText(List<string> dataList)
        {
            int n = 0;
            if (dataList == null || dataList[n++] != "ArcData") return;
            while (n < dataList.Count && dataList[n] != "DataEnd") {
                string[] buf = dataList[n++].Split(',');
                int i = 0;
                if (buf[i].Trim() == "中心") {
                    mArc.mPlane.mCp.x = ylib.doubleParse(buf[++i]);
                    mArc.mPlane.mCp.y = ylib.doubleParse(buf[++i]);
                    mArc.mPlane.mCp.z = ylib.doubleParse(buf[++i]);
                } else if (buf[i].Trim() == "半径") {
                    mArc.mR = ylib.doubleParse(buf[++i]);
                } else if (buf[i].Trim() == "始角") {
                    mArc.mSa = ylib.D2R(ylib.doubleParse(buf[++i]));
                } else if (buf[i].Trim() == "終角") {
                    mArc.mEa = ylib.D2R(ylib.doubleParse(buf[++i]));
                } else if (buf[i].Trim() == "面U") {
                    mArc.mPlane.mU.x = ylib.doubleParse(buf[++i]);
                    mArc.mPlane.mU.y = ylib.doubleParse(buf[++i]);
                    mArc.mPlane.mU.z = ylib.doubleParse(buf[++i]);
                } else if (buf[i].Trim() == "面V") {
                    mArc.mPlane.mV.x = ylib.doubleParse(buf[++i]);
                    mArc.mPlane.mV.y = ylib.doubleParse(buf[++i]);
                    mArc.mPlane.mV.z = ylib.doubleParse(buf[++i]);
                }
            }
        }

        /// <summary>
        /// 図形データを文字列リストに変換
        /// </summary>
        public override List<string> getDataText()
        {
            Point3D sp = mArc.startPosition();
            Point3D mp = mArc.midPosition();
            Point3D ep = mArc.endPosition();
            List<string> dataList = new List<string> {
                "ArcData",
                $"中心, {mArc.mPlane.mCp.x.ToString()}, {mArc.mPlane.mCp.y.ToString()}, {mArc.mPlane.mCp.z.ToString()},",
                $"半径, {mArc.mR}",
                $"始角, {ylib.R2D(mArc.mSa)}",
                $"終角, {ylib.R2D(mArc.mEa)}",
                $"面U,  {mArc.mPlane.mU.x.ToString()}.{mArc.mPlane.mU.y.ToString()}, {mArc.mPlane.mU.z.ToString()},",
                $"面V,  {mArc.mPlane.mV.x.ToString()}.{mArc.mPlane.mV.y.ToString()}, {mArc.mPlane.mV.z.ToString()},",
                $"参考",
                $"始点, {sp.x.ToString()}.{sp.y.ToString()}, {sp.z.ToString()},",
                $"中点, {mp.x.ToString()}.{mp.y.ToString()}, {mp.z.ToString()},",
                $"終点, {ep.x.ToString()}.{ep.y.ToString()}, {ep.z.ToString()},",
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
                "ArcData",
                "Cp", mArc.mPlane.mCp.x.ToString(), mArc.mPlane.mCp.y.ToString(), mArc.mPlane.mCp.z.ToString(),
                "R", mArc.mR.ToString(),
                "U", mArc.mPlane.mU.x.ToString(), mArc.mPlane.mU.y.ToString(), mArc.mPlane.mU.z.ToString(),
                "V", mArc.mPlane.mV.x.ToString(), mArc.mPlane.mV.y.ToString(), mArc.mPlane.mV.z.ToString(),
                "Sa", mArc.mSa.ToString(),
                "Ea", mArc.mEa.ToString()
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
                if (dataList[sp][0] == "ArcData") {
                    for (int i = 1; i < dataList[sp].Length; i++) {
                        if (dataList[sp][i] == "Cp") {
                            mArc.mPlane.mCp.x = ylib.doubleParse(dataList[sp][++i]);
                            mArc.mPlane.mCp.y = ylib.doubleParse(dataList[sp][++i]);
                            mArc.mPlane.mCp.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "R") {
                            mArc.mR = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "U") {
                            mArc.mPlane.mU.x = ylib.doubleParse(dataList[sp][++i]);
                            mArc.mPlane.mU.y = ylib.doubleParse(dataList[sp][++i]);
                            mArc.mPlane.mU.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "V") {
                            mArc.mPlane.mV.x = ylib.doubleParse(dataList[sp][++i]);
                            mArc.mPlane.mV.y = ylib.doubleParse(dataList[sp][++i]);
                            mArc.mPlane.mV.z = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Sa") {
                            mArc.mSa = ylib.doubleParse(dataList[sp][++i]);
                        } else if (dataList[sp][i] == "Ea") {
                            mArc.mEa = ylib.doubleParse(dataList[sp][++i]);
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
            if (0 == list.Length || list[0] != "ArcData")
                return sp;
            try {
                double val;
                for (int i = 1; i < list.Length; i++) {
                    if (list[i] == "Cp") {
                        mArc.mPlane.mCp.x = ylib.doubleParse(list[++i]);
                        mArc.mPlane.mCp.y = ylib.doubleParse(list[++i]);
                        mArc.mPlane.mCp.z = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "R") {
                        mArc.mR = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "U") {
                        mArc.mPlane.mU.x = ylib.doubleParse(list[++i]);
                        mArc.mPlane.mU.y = ylib.doubleParse(list[++i]);
                        mArc.mPlane.mU.z = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "V") {
                        mArc.mPlane.mV.x = ylib.doubleParse(list[++i]);
                        mArc.mPlane.mV.y = ylib.doubleParse(list[++i]);
                        mArc.mPlane.mV.z = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "Sa") {
                        mArc.mSa = ylib.doubleParse(list[++i]);
                    } else if (list[i] == "Ea") {
                        mArc.mEa = ylib.doubleParse(list[++i]);
                    }
                }
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"Arc setDataList {e.ToString()}");
            }

            return sp;
        }

    }
}
