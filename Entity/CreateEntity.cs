using CoreLib;

namespace Cad3DApp
{
    /// <summary>
    /// 要素作成クラス
    /// </summary>
    public class CreateEntity
    {

        public GlobalData mGlobal = new GlobalData();
        private double mEps = 1E-8;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="global">グロバルデータ</param>
        public CreateEntity(GlobalData global)
        {
            mGlobal = global;
        }

        /// <summary>
        /// 要素データ(文字列)リストから要素データを取得
        /// </summary>
        /// <param name="id">要素ID</param>
        /// <param name="dataList">データリスト</param>
        /// <param name="sp">開始位置</param>
        /// <returns>(次位置,要素データ)</returns>
        public (int sp, Entity? entity) dataList2Entity(string id, List<string[]> dataList, int sp)
        {
            switch (id) {
                case "Line":
                    LineEntity line = new LineEntity(mGlobal.mLayerSize);
                    sp = line.setProperty(dataList, sp);
                    sp = line.setDataList(dataList, sp);
                    return (sp, line);
                case "Arc":
                    ArcEntity arc = new ArcEntity(mGlobal.mLayerSize);
                    sp = arc.setProperty(dataList, sp);
                    sp = arc.setDataList(dataList, sp);
                    return (sp, arc);
                case "Polyline":
                    PolylineEntity polyline = new PolylineEntity(mGlobal.mLayerSize);
                    sp = polyline.setProperty(dataList, sp);
                    sp = polyline.setDataList(dataList, sp);
                    return (sp, polyline);
                case "Polygon":
                    PolygonEntity polygon = new PolygonEntity(mGlobal.mLayerSize);
                    sp = polygon.setProperty(dataList, sp);
                    sp = polygon.setDataList(dataList, sp);
                    return (sp, polygon);
                case "Extrusion":
                    ExtrusionEntity extrusion = new ExtrusionEntity(mGlobal.mLayerSize);
                    sp = extrusion.setProperty(dataList, sp);
                    sp = extrusion.setDataList(dataList, sp);
                    return (sp, extrusion);
                case "Blend":
                    BlendEntity blend = new BlendEntity(mGlobal.mLayerSize);
                    sp = blend.setProperty(dataList, sp);
                    sp = blend.setDataList(dataList, sp);
                    return (sp, blend);
                case "Revolution":
                    RevolutionEntity revolution = new RevolutionEntity(mGlobal.mLayerSize);
                    sp = revolution.setProperty(dataList, sp);
                    sp = revolution.setDataList(dataList, sp);
                    return (sp, revolution);
                case "Sweep":
                    SweepEntity sweep = new SweepEntity(mGlobal.mLayerSize);
                    sp = sweep.setProperty(dataList, sp);
                    sp = sweep.setDataList(dataList, sp);
                    return (sp, sweep);
                default:
                    break;
            }
            return (sp, null);
        }

        /// <summary>
        /// Min3DCadデータの取込み
        /// </summary>
        /// <param name="dataList">要素データリスト(文字列)</param>
        /// <param name="sp">要素位置</param>
        /// <returns>要素データ</returns>
        public (int sp, Entity? entity) getMini3DCadData(string[] buf, List<string[]> dataList, int sp)
        {
            switch (buf[1]) {
                case "Line":
                    LineEntity line = new LineEntity(mGlobal.mLayerSize);
                    line.setElementProperty(buf);
                    sp = line.setElementDataList(dataList, sp);
                    return (sp, line);
                case "Arc":
                    ArcEntity arc = new ArcEntity(mGlobal.mLayerSize);
                    arc.setElementProperty(buf);
                    sp = arc.setElementDataList(dataList, sp);
                    return (sp, arc);
                case "Polyline":
                    PolylineEntity polyline = new PolylineEntity(mGlobal.mLayerSize);
                    polyline.setElementProperty(buf);
                    sp = polyline.setElementDataList(dataList, sp);
                    return (sp, polyline);
                case "Polygon":
                    PolygonEntity polygon = new PolygonEntity(mGlobal.mLayerSize);
                    polygon.setElementProperty(buf);
                    sp = polygon.setElementDataList(dataList, sp);
                    return (sp, polygon);
                case "Extrusion":
                    ExtrusionEntity extrusion = new ExtrusionEntity(mGlobal.mLayerSize);
                    extrusion.setElementProperty(buf);
                    sp = extrusion.setElementDataList(dataList, sp);
                    return (sp, extrusion);
                case "Blend":
                    BlendEntity blend = new BlendEntity(mGlobal.mLayerSize);
                    blend.setElementProperty(buf);
                    sp = blend.setElementDataList(dataList, sp);
                    return (sp, blend);
                case "BlendPolyline":
                    BlendEntity blendPolyline = new BlendEntity(mGlobal.mLayerSize);
                    blendPolyline.setElementProperty(buf);
                    sp = blendPolyline.setElementDataList(dataList, sp);
                    return (sp, blendPolyline);
                case "Revolution":
                    RevolutionEntity revolution = new RevolutionEntity(mGlobal.mLayerSize);
                    revolution.setElementProperty(buf);
                    sp = revolution.setElementDataList(dataList, sp);
                    return (sp, revolution);
                case "Sweep":
                    SweepEntity sweep = new SweepEntity(mGlobal.mLayerSize);
                    sweep.setElementProperty(buf);
                    sp = sweep.setElementDataList(dataList, sp);
                    return (sp, sweep);
                default:
                    break;
            }
            return (sp, null);
        }

        /// <summary>
        /// CadAppデータの取込み
        /// </summary>
        /// <param name="property">要素属性データ(文字列)</param>
        /// <param name="data">要素データ(文字列)</param>
        /// <param name="face">2D平面</param>
        /// <returns>要素データ</returns>
        public Entity getCadAppData(string[] property, string[] data, FACE3D face)
        {
            Entity entity;
            switch (property[0]) {
                case "Line":
                    PointD ps = new PointD(ylib.doubleParse(data[0]), ylib.doubleParse(data[1]));
                    PointD pe = new PointD(ylib.doubleParse(data[2]), ylib.doubleParse(data[3]));
                    Line3D line = new Line3D(new Point3D(ps, face), new Point3D(pe, face));
                    entity = new LineEntity(line, mGlobal.mLayerSize);
                    break;
                case "Arc":
                    ArcD arc = new ArcD();
                    arc.mCp = new PointD(ylib.doubleParse(data[3]), ylib.doubleParse(data[4]));
                    arc.mR = ylib.doubleParse(data[5]);
                    arc.mSa = ylib.doubleParse(data[6]);
                    arc.mEa = ylib.doubleParse(data[7]);
                    Arc3D arc3 = new Arc3D(arc, face);
                    entity = new ArcEntity(arc3, mGlobal.mLayerSize);
                    break;
                case "Polyline":
                    PolylineD polyline = new PolylineD();
                    for (int i = 0; i < data.Length - 1; i += 2) {
                        PointD p = new PointD(ylib.doubleParse(data[i]), ylib.doubleParse(data[i + 1]));
                        polyline.Add(p);
                    }
                    polyline.squeeze();
                    Polyline3D polyline3 = new Polyline3D(polyline, face);
                    entity = new PolylineEntity(polyline3, mGlobal.mLayerSize);
                    break;
                case "Polygon":
                    PolygonD polygon = new PolygonD();
                    for (int i = 3; i < data.Length - 1; i += 2) {
                        PointD p = new PointD(ylib.doubleParse(data[i]), ylib.doubleParse(data[i + 1]));
                        polygon.Add(p);
                    }
                    polygon.squeeze();
                    Polygon3D polygon3 = new Polygon3D(polygon, face);
                    entity = new PolygonEntity(polygon3, mGlobal.mLayerSize);
                    break;
                default:
                    return null;
            }
            entity.mLineColor = ylib.getBrsh(property[1].Trim());
            entity.mFaceColor = ylib.getBrsh(property[1].Trim());
            entity.mLineThickness = ylib.doubleParse(property[2].Trim());
            entity.mLineType = ylib.intParse(property[3].Trim());

            return entity;
        }



        /// <summary>
        /// 線分要素の作成
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>線分要素</returns>
        public Entity createLine(Point3D sp, Point3D ep, bool surface = false)
        {
            return createLine(new Line3D(sp, ep), surface);
        }

        /// <summary>
        /// 線分要素の作成
        /// </summary>
        /// <param name="pline">線分</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>線分要素</returns>
        public Entity createLine(Line3D pline, bool surface = false)
        {
            LineEntity line = new LineEntity(pline, mGlobal.mLayerSize);
            line.mLineThickness = mGlobal.mLineThickness;
            line.mLineType      = mGlobal.mLineType;
            line.mLineColor     = mGlobal.mEntityBrush;
            line.mFaceColor     = mGlobal.mEntityBrush;
            if (surface)
                line.createSurfaceData();
            line.createVertexData();
            return line;
        }

        /// <summary>
        /// 円弧要素の作成
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="p">円周上の点</param>
        /// <param name="sa">開始角</param>
        /// <param name="ea">終了角</param>
        /// <param name="face">作成面</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>円弧要素</returns>
        public Entity createArc(Point3D cp, Point3D p, double sa = 0, double ea = Math.PI * 2, FACE3D face = FACE3D.FRONT, bool surface = false)
        {
            double r = cp.length(p);
            return createArc(new Arc3D(cp, r, sa, ea, face), surface);
        }

        /// <summary>
        /// 円弧要素の作成
        /// </summary>
        /// <param name="cp">中心</param>
        /// <param name="r">半径</param>
        /// <param name="sa">開始角</param>
        /// <param name="ea">終了角</param>
        /// <param name="face">作成面</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>円弧要素</returns>
        public Entity createArc(Point3D cp, double r, double sa = 0, double ea = Math.PI * 2, FACE3D face = FACE3D.FRONT, bool surface = false)
        {
            return createArc(new Arc3D(cp, r, sa, ea, face), surface);
        }

        /// <summary>
        /// 円弧要素の作成(3点円弧)
        /// </summary>
        /// <param name="p0">始点</param>
        /// <param name="p1">中点</param>
        /// <param name="p2">終点</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>円弧要素</returns>
        public Entity createArc(Point3D p0, Point3D p1, Point3D p2, bool surface = false)
        {
            if (p0.length(p1) < mEps || p0.length(p2) < mEps || p1.length(p2) < mEps)
                return null;
            else
                return createArc(new Arc3D(p0, p1, p2), surface);
        }

        /// <summary>
        /// 円弧要素の作成
        /// </summary>
        /// <param name="parc">円弧</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>円弧要素</returns>
        public Entity createArc(Arc3D parc, bool surface = false)
        {
            ArcEntity arc = new ArcEntity(parc, mGlobal.mLayerSize);
            arc.mLineThickness = mGlobal.mLineThickness;
            arc.mLineType   = mGlobal.mLineType;
            arc.mLineColor = mGlobal.mEntityBrush;
            arc.mFaceColor = mGlobal.mEntityBrush;
            if (surface)
                arc.createSurfaceData();
            arc.createVertexData();
            return arc;
        }

        /// <summary>
        /// ポリライン要素の作成
        /// </summary>
        /// <param name="plist">2D座標リスト</param>
        /// <param name="face">作成面</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリライン要素</returns>
        public Entity createPolyline(List<PointD> plist, FACE3D face, bool surface = false)
        {
            return createPolyline(new Polyline3D(plist, face), surface);
        }

        /// <summary>
        /// ポリライン要素の作成
        /// </summary>
        /// <param name="plist">3D座標リスト</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリライン要素</returns>
        public Entity createPolyline(List<Point3D> plist, bool surface = false)
        {
            return createPolyline(new Polyline3D(plist), surface);
        }

        /// <summary>
        /// ポリライン要素の作成
        /// </summary>
        /// <param name="plist">3D座標リスト</param>
        /// <param name="face">作成面</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリライン要素</returns>
        public Entity createPolyline(List<Point3D> plist, FACE3D face, bool surface = false)
        {
            return createPolyline(new Polyline3D(plist, face), surface);
        }

        /// <summary>
        /// ポリライン要素の作成
        /// </summary>
        /// <param name="ppolyline">ポリライン</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリライン要素</returns>
        public Entity createPolyline(Polyline3D ppolyline, bool surface = false)
        {
            PolylineEntity polyline = new PolylineEntity(ppolyline, mGlobal.mLayerSize);
            polyline.mLineThickness = mGlobal.mLineThickness;
            polyline.mLineType  = mGlobal.mLineType;
            polyline.mLineColor = mGlobal.mEntityBrush;
            polyline.mFaceColor = mGlobal.mEntityBrush;
            if (surface)
                polyline.createSurfaceData();
            polyline.createVertexData();
            return polyline;
        }

        /// <summary>
        /// ポリゴン要素の作成
        /// </summary>
        /// <param name="plist">2D座標リスト</param>
        /// <param name="face">作成面</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリゴン要素</returns>
        public Entity createPolygon(List<PointD> plist, FACE3D face, bool surface = false)
        {
            return createPolygon(new Polygon3D(plist, face), surface);
        }

        /// <summary>
        /// ポリゴン要素の作成
        /// </summary>
        /// <param name="plist">3D座標リスト</param>
        /// <param name="face">作成面</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリゴン要素</returns>
        public Entity createPolygon(List<Point3D> plist, FACE3D face, bool surface = false)
        {
            return createPolygon(new Polygon3D(plist, face), surface);
        }

        /// <summary>
        /// ポリゴン要素の作成
        /// </summary>
        /// <param name="plist">3D座標リスト</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリゴン要素</returns>
        public Entity createPolygon(List<Point3D> plist, bool surface = false)
        {
            return createPolygon(new Polygon3D(plist), surface);
        }

        /// <summary>
        /// 矩形領域(ポリゴン)の作成
        /// </summary>
        /// <param name="sp">端点</param>
        /// <param name="ep">端点</param>
        /// <param name="face">作成面</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリゴン要素</returns>
        public Entity createRect(Point3D sp, Point3D ep, FACE3D face, bool surface = false)
        {
            Box3D box3 = new Box3D(sp, ep);
            List<PointD> plist = box3.toBox(face).ToPointList();
            plist.Reverse();
            return createPolygon(plist, face, surface);
        }

        /// <summary>
        /// ポリゴン要素の作成
        /// </summary>
        /// <param name="ppolygon">ポリゴン</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>ポリゴン要素</returns>
        public Entity createPolygon(Polygon3D ppolygon, bool surface = false)
        {
            PolygonEntity polygon = new PolygonEntity(ppolygon, mGlobal.mLayerSize);
            polygon.mLineThickness = mGlobal.mLineThickness;
            polygon.mLineType   = mGlobal.mLineType;
            polygon.mLineColor  = mGlobal.mEntityBrush;
            polygon.mFaceColor  = mGlobal.mEntityBrush;
            if (surface)
                polygon.createSurfaceData();
            polygon.createVertexData();
            return polygon;
        }
    }
}
