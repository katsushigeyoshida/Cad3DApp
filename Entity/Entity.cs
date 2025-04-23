using CoreLib;
using System.Globalization;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Cad3DApp
{
    /// <summary>
    /// 要素の種類
    /// </summary>
    public enum EntityId
    {
        Non, Link,
        Point, Line, Arc, Polyline, Polygon, Text,
        WireCube, Cube, Cylinder, Sphere, Cone,
        Extrusion, Blend, BlendPolyline, Revolution, Sweep
    }

    public abstract class Entity
    {
        public EntityId mID = EntityId.Non;
        public List<SurfaceData> mSurfaceDataList;              //  3D座標データ
        public List<Polyline3D> mVertexList;                    //  2D表示用3D座標データ
        public double mLineThickness = 1.0;                     //  線の太さ
        public int mLineType = 0;                               //  線種(0:solid 1:dash 2:center 3:phantom)
        public Brush mLineColor = Brushes.Black;                //  線の色
        public Brush mFaceColor = Brushes.Blue;                 //  面の色
        public Brush mPickColor = Brushes.Red;                  //  ピック時のカラー
        public bool mPick = false;                              //  ピック状態
        public double mDivAngle = Math.PI / 12;                 //  円弧分割角度

        public bool mDisp3D = true;                             //  3Dでの表示/非表示
        public bool mDisp2D = true;                             //  2Dでの表示/非表示
        public bool mEdgeDisp = true;                           //  端面表示の有無
        public bool mReverse = false;                           //  座標点の逆順
        public bool mEdgeReverse = false;                       //  端面座標点の逆順
        public bool mWireFrame = false;                         //  ワイヤーフレーム表示
        public int mGroup = 0;                                  //  グループ番号
        public bool mRemove = false;                            //  削除フラグ
        public int mLinkNo = -1;                                //  リンク先要素番号
        public byte[] mLayerBit;                                //  レイヤーBit
        public Box3D mArea;                                     //  要素領域
        public int mOperationNo = -1;                           //  操作位置

        public YLib ylib = new YLib();

        /// <summary>
        /// 2D 表示(ポリライン) 外形線の表示
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="face">表示 2D平面</param>
        public void draw2D(YWorldDraw draw, FACE3D face)
        {
            if (mPick)
                draw.mBrush = mPickColor;
            else
                draw.mBrush = mLineColor;
            draw.mThickness = mLineThickness;
            draw.mLineType = mLineType;
            draw.mPointSize = 3;
            draw.mPointType = 0;
            for (int i = 0; i < mVertexList.Count; i++) {
                //if (!mPick && mOutlineDisp) {
                //    draw.mBrush = mOutlineColors[i % mOutlineColors.Count];
                //    draw.mLineType = mOutlineType[i % mOutlineType.Count];
                //}
                PolylineD polyline = mVertexList[i].toPolylineD(face);
                draw.drawWPolyline(polyline);
            }
        }

        /// <summary>
        /// 2D表示の可否
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool is2DDraw(Layer layer)
        {
            if (!mRemove && mDisp2D &&
                (layer.bitAnd(mLayerBit) || layer.mLayerAll || layer.IsEmpty(mLayerBit)))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 3D表示の可否
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool is3DDraw(Layer layer)
        {
            if (!mRemove && mDisp3D &&
                (layer.bitAnd(mLayerBit) || layer.mLayerAll || layer.IsEmpty(mLayerBit)))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 2DのVertexListで要素ピックを判定
        /// </summary>
        /// <param name="b">ピック領域</param>
        /// <param name="face">表示面</param>
        /// <returns>ピックの可否</returns>
        public bool isPick(Box b, FACE3D face)
        {
            if (mRemove || mID == EntityId.Link || !mDisp2D || mArea == null) return false;
            Box area = mArea.toBox(face);
            if (area.outsideChk(b)) return false;
            if (b.insideChk(area)) return true;
            for (int i = 0; i < mVertexList.Count; i++) {
                List<PointD> plist = mVertexList[i].toPointD(face);
                if (0 < b.intersection(plist, false, true).Count || b.insideChk(plist))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 属性をコピーする
        /// </summary>
        /// <param name="entity">コピー元要素</param>
        public void copyProperty(Entity entity)
        {
            //mID = entity.mID;                           //  要素ID
            mLineThickness = entity.mLineThickness;     //  線の太さ
            mLineType = entity.mLineType;               //  線種(0:solid 1:dash 2:center 3:phantom)
            mLineColor = entity.mLineColor;             //  線の色
            mFaceColor = entity.mFaceColor;             //  面の色
            mPickColor = entity.mPickColor;             //  ピック時のカラー
            mPick = entity.mPick;                       //  ピック状態
            mDivAngle = entity.mDivAngle;               //  円弧分割角度
            mDisp3D = entity.mDisp3D;                   //  3Dでの表示/非表示
            mDisp2D = entity.mDisp2D;                   //  2Dでの表示/非表示
            mReverse = entity.mReverse;                 //  座標順反転
            mEdgeDisp = entity.mEdgeDisp;               //  端面表示
            mEdgeReverse = entity.mEdgeReverse;         //  端面座標順反転
            mGroup = entity.mGroup;                     //  グループ番号
            mRemove = entity.mRemove;                   //  削除フラグ
            mLinkNo = entity.mLinkNo;                   //  リンク先要素番号
            Array.Copy(entity.mLayerBit, mLayerBit, entity.mLayerBit.Length);   //  レイヤーBit
            mArea = entity.mArea;                       //  要素領域
            mOperationNo = entity.mOperationNo;         //  操作位置
        }

        /// <summary>
        /// 文字列リストから属性を設定
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public int setProperty(List<string[]> dataList, int sp)
        {
            int layerSize = 0;
            while (dataList[sp][0] != "PropertyEnd") {
                if (dataList[sp][0] != "DispLayerBit") {
                    for (int i = 0; i < dataList[sp].Length; i += 2) {
                        switch (dataList[sp][i]) {
                            case "LineColor":
                                mLineColor = ylib.getBrsh(dataList[sp][i + 1]);
                                break;
                            case "LineThickness":
                                mLineThickness = ylib.doubleParse(dataList[sp][i + 1]);
                                break;
                            case "LineType":
                                mLineType = ylib.intParse(dataList[sp][i + 1]);
                                break;
                            case "FaceColor":
                                mFaceColor = ylib.getBrsh(dataList[sp][i + 1]);
                                break;
                            case "DivideAngle":
                                mDivAngle = ylib.doubleParse(dataList[sp][i + 1]);
                                break;
                            case "Disp2D":
                                mDisp2D = ylib.boolParse(dataList[sp][i + 1]);
                                break;
                            case "Disp3D":
                                mDisp3D = ylib.boolParse(dataList[sp][i + 1]);
                                break;
                            case "EdgeDisp":
                                mEdgeDisp = ylib.boolParse(dataList[sp][i + 1]);
                                break;
                            case "Reverse":
                                mReverse = ylib.boolParse(dataList[sp][i + 1]);
                                break;
                            case "EdgeReverse" :
                                mEdgeReverse = ylib.boolParse(dataList[sp][i + 1]);
                                break;
                            case "Group":
                                mGroup = ylib.intParse(dataList[sp][i + 1]);
                                break;
                            case "LayerSize":
                                layerSize = ylib.intParse(dataList[sp][i + 1]);
                                break;
                        }
                    }
                } else if (dataList[sp][0] == "DispLayerBit") {
                    mLayerBit = new byte[dataList[sp].Length - 1];
                    for (int i = 1; i < dataList[sp].Length; i++)
                        mLayerBit[i - 1] = byte.Parse(dataList[sp][i], NumberStyles.HexNumber);
                }
                sp++;
            }
            return ++sp;
        }

        /// <summary>
        /// Mini3DCadデータのプロパティ取得
        /// </summary>
        /// <param name="list"></param>
        public void setElementProperty(string[] list)
        {
            if (list == null || list.Length == 0)
                return;
            int ival;
            double val;
            bool bval;
            for (int i = 0; i < list.Length; i++) {
                if (list[i] == "PrimitiveId") {
                    //mPrimitiveId = (PrimitiveId)Enum.Parse(typeof(PrimitiveId), list[++i]);
                } else if (list[i] == "LineColor") {
                    mLineColor = ylib.getBrsh(list[++i]);
                } else if (list[i] == "LineThickness") {
                    mLineThickness = double.TryParse(list[++i], out val) ? val : 1;
                } else if (list[i] == "LineType") {
                    mLineType = int.TryParse(list[++i], out ival) ? ival : 0;
                } else if (list[i] == "Close") {
                    mEdgeDisp = bool.TryParse(list[++i], out bval) ? bval : false;
                } else if (list[i] == "Reverse") {
                    mReverse = bool.TryParse(list[++i], out bval) ? bval : false;
                } else if (list[i] == "DivideAngle") {
                    mDivAngle = double.TryParse(list[++i], out val) ? val : Math.PI / 15;
                } else if (list[i] == "FaceColors") {
                    int count = int.TryParse(list[++i], out ival) ? ival : 0;
                    mFaceColor = ylib.getBrsh(list[++i]);
                }
            }
        }

        /// <summary>
        /// 属性を文字列リストに変換
        /// </summary>
        /// <returns></returns>
        public List<string[]> toPropertyList()
        {
            List<string[]> propertyList = new List<string[]>();
            propertyList.Add(new string[] { "ID", mID.ToString() });
            string[] buf = new string[] {
                "LineColor",        ylib.getBrushName(mLineColor),
                "LineThickness",    mLineThickness.ToString(),
                "LineType",         mLineType.ToString(),
                "FaceColor",        ylib.getBrushName(mFaceColor),
                "DivideAngle",      mDivAngle.ToString(),
                "Disp2D",           mDisp2D.ToString(),
                "Disp3D",           mDisp3D.ToString(),
                "EdgeDisp",         mEdgeDisp.ToString(),
                "Reverse",          mReverse.ToString(),
                "EdgeReverse",      mEdgeReverse.ToString(),
                "Group",            mGroup.ToString(),
                "LayerSize",        mLayerBit.Length.ToString(),
            };
            propertyList.Add(buf);
            List<string> layerbit = new List<string>() { "DispLayerBit" };
            for (int i = 0; i < mLayerBit.Length; i++) {
                layerbit.Add(mLayerBit[i].ToString("X2"));
            }
            propertyList.Add(layerbit.ToArray());
            propertyList.Add(new string[] { "PropertyEnd" });

            return propertyList;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public abstract Entity toCopy();

        /// <summary>
        /// 3D座標(Surface)リストの作成
        /// </summary>
        public abstract void createSurfaceData();

        /// <summary>
        /// 2D表示用座標データの作成
        /// </summary>
        public abstract void createVertexData();

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public abstract void translate(Point3D v);

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">操作面</param>
        public abstract void rotate(Point3D cp, double ang, PointD pickPos, FACE3D face);

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">操作面</param>
        public abstract void offset(Point3D sp, Point3D ep, PointD pickPos, FACE3D face);

        /// <summary>
        /// ミラー
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D操作面</param>
        public abstract void mirror(Point3D sp, Point3D ep, PointD pickPos, FACE3D face);

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        public abstract void trim(Point3D sp, Point3D ep, PointD pickPos, FACE3D face);

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="cp">拡大中心</param>
        /// <param name="scale">倍率</param>
        /// <param name="face">2D平面</param>
        public abstract void scale(Point3D cp, double scale, PointD pickPos, FACE3D face);

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="vec">移動ベクトル</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="arc">円弧変形</param>
        /// <param name="face">2D平面</param>
        public abstract void stretch(Point3D vec, bool arc, PointD pickPos, FACE3D face);

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="pos">分割位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割要素リスト</returns>
        public abstract List<Entity> divide(Point3D pos, FACE3D face);

        /// <summary>
        /// 要素同士の交点
        /// </summary>
        /// <param name="entity">要素</param>
        /// <param name="pos">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>交点座標</returns>
        public abstract Point3D intersection(Entity entity, PointD pos, FACE3D face);

        /// <summary>
        /// 3D座標点リストの取得
        /// </summary>
        /// <returns>座標点リスト</returns>
        public abstract List<Point3D> toPointList();

        /// <returns>文字列リスト</returns>
        /// <summary>
        /// 図形データを文字列から設定
        /// </summary>
        /// <param name="dataList">データ文字列リスト</param>
        public abstract void setDataText(List<string> dataList);

        /// <summary>
        /// 図形データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public abstract List<string> getDataText();

        /// <summary>
        /// データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public abstract List<string[]> toDataList();

        /// <summary>
        /// 文字列データを設定
        /// </summary>
        /// <param name="dataList">文字列データリスト</param>
        /// <param name="sp">データ位置</param>
        /// <returns>終了データ位置</returns>
        public abstract int setDataList(List<string[]> dataList, int sp);

        /// <summary>
        /// Mini3DCadの要素データの読込
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public abstract int setElementDataList(List<string[]> dataList, int sp);

        /// <summary>
        /// 2D表示データから交点を求める
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <param name="pos">指定点</param>
        /// <param name="face">2D平面</param>
        /// <returns>3D交点</returns>
        public Point3D intersection2(Entity entity, PointD pos, FACE3D face)
        {
            Polyline3D polyline0 = nearLineArc(pos, face);
            Polyline3D polyline1 = entity.nearLineArc(pos, face);
            return polyline0.intersection(polyline1, pos, face);
        }

        /// <summary>
        /// 2D表示データから指定点に近い線分(円弧)をポリラインで抽出
        /// </summary>
        /// <param name="pos">指定点</param>
        /// <param name="face">2D平面</param>
        /// <returns>ポリライン</returns>
        public Polyline3D nearLineArc(PointD pos, FACE3D face)
        {
            double dis = double.MaxValue;
            int n = -1;
            int ln = 0;
            for (int i = 0; i < mVertexList.Count; i++) {
                PolylineD polyline = mVertexList[i].toPolylineD(face);
                (int pn, PointD p) = polyline.nearCrossPos(pos);
                double l = pos.length(p);
                if (l < dis) {
                    dis = l;
                    n = i;
                    ln = pn;
                }
            }
            if (0 <= n) {
                PolylineD polyline = mVertexList[n].toPolylineD(face);
                return new Polyline3D(polyline.getLineArc(ln), face);
            }
            return null;
        }

        /// <summary>
        /// 2D表示データから分割点で最も近い座標
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <param name="divNo">分割数</param>
        /// <param name="face">2D平面</param>
        /// <returns>要素上の座標</returns>
        public Point3D nearPoint(PointD pos, int divNo, FACE3D face)
        {
            PointD np = new PointD();
            double dis = double.MaxValue;
            for (int i = 0;i < mVertexList.Count; i++) {
                PolylineD polyline = mVertexList[i].toPolylineD(face);
                PointD ip = polyline.nearPoint(pos, divNo);
                double l = ip.length(pos);
                if (dis > l) {
                    dis = l;
                    np = ip;
                }
            }
            return new Point3D(np, face);
        }

        /// <summary>
        /// 表示領域を設定
        /// </summary>
        public void setArea()
        {
            if (mVertexList != null && 0 < mVertexList.Count) {
                mArea = new Box3D(mVertexList[0].toPoint3D()[0]);
                for (int i = 0; i < mVertexList.Count;i++) {
                    Polyline3D polyline = new Polyline3D(mVertexList[i]);
                    List<Point3D> plist = polyline.toPoint3D(mDivAngle);
                    plist.ForEach(p => mArea.extension(p));
                }
            } else {
                mArea = null;
            }
        }

        /// <summary>
        /// 2D表示データを3Dワイヤーフレーム表示に変換
        /// </summary>
        /// <returns>サーフェスデータ</returns>
        public List<SurfaceData> toWireFrame()
        {
            return polyline2SurfaceData(mVertexList);
        }

        /// <summary>
        /// ポリラインデータをサーフェスデータに変換
        /// </summary>
        /// <param name="polylins">ポリライン</param>
        /// <returns>サーフェスデータ</returns>
        public List<SurfaceData> polyline2SurfaceData(List<Polyline3D> polylins)
        {
            List<SurfaceData> surfaceDatas = new List<SurfaceData>();
            for (int i = 0; i < mVertexList.Count; i++) {
                surfaceDatas.Add(new SurfaceData(mVertexList[i]));
            }
            return surfaceDatas;
        }
    }
}
