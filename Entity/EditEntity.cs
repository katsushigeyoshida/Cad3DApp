using CoreLib;

namespace Cad3DApp
{
    /// <summary>
    /// 要素編集クラス
    /// </summary>
    public class EditEntity
    {
        public GlobalData mGlobal = new GlobalData();
        public List<Entity> mEntityList;

        private CreateEntity mCreateEntity;
        private double mEps = 1E-8;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entityList">要素リスト</param>
        public EditEntity(GlobalData global, List<Entity> entityList)
        {
            mGlobal = global;
            mEntityList = entityList;
            mCreateEntity = new CreateEntity(global);
        }

        /// <summary>
        /// 要素の移動
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> translate(List<PickData> pickEntity, Point3D sp, Point3D ep, bool surface)
        {
            List<Entity> entityList = new List<Entity>();
            Point3D v = ep - sp;
            foreach (var pick in pickEntity) {
                Entity entity = mEntityList[pick.mEntityNo].toCopy();
                entity.translate(v);
                entity.createVertexData();
                if (surface)
                    entity.createSurfaceData();
                entity.mLinkNo = -1;
                entityList.Add(entity);
            }
            return entityList;
        }

        /// <summary>
        /// 要素の回転(3点指定)
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="cp">回転中心</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> rotate(List<PickData> pickEntity, Point3D cp, Point3D sp, Point3D ep, FACE3D face, bool surface)
        {
            double ang = cp.toPoint(face).angle2(sp.toPoint(face), ep.toPoint(face));
            return rotate(pickEntity, cp, ang, face, surface);
        }

        /// <summary>
        /// 要素の回転(角度指定)
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">2D平面</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> rotate(List<PickData> pickEntity, Point3D cp, double ang, FACE3D face, bool surface)
        {
            List<Entity> entityList = new List<Entity>();
            foreach (var pick in pickEntity) {
                Entity entity = mEntityList[pick.mEntityNo].toCopy();
                entity.rotate(cp, -ang, pick.mPos, face);
                entity.createVertexData();
                if (surface)
                    entity.createSurfaceData();
                entity.mLinkNo = -1;
                entityList.Add(entity);
            }
            return entityList;
        }

        /// <summary>
        /// 要素のオフセット
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> offset(List<PickData> pickEntity, Point3D sp, Point3D ep, FACE3D face, bool surface)
        {
            List<Entity> entityList = new List<Entity>();
            if (0 < ep.length(sp)) {
                foreach (var pick in pickEntity) {
                    Entity entity = mEntityList[pick.mEntityNo].toCopy();
                    entity.offset(sp, ep, pick.mPos, face);
                    entity.createVertexData();
                    if (surface)
                        entity.createSurfaceData();
                    entity.mLinkNo = -1;
                    entityList.Add(entity);
                }
            }
            return entityList;
        }

        /// <summary>
        /// 要素のミラー
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> mirror(List<PickData> pickEntity, Point3D sp, Point3D ep, FACE3D face, bool surface)
        {
            List<Entity> entityList = new List<Entity>();
            if (0 < ep.length(sp)) {
                foreach (var pick in pickEntity) {
                    Entity entity = mEntityList[pick.mEntityNo].toCopy();
                    entity.mirror(sp, ep, pick.mPos, face);
                    entity.createVertexData();
                    if (surface)
                        entity.createSurfaceData();
                    entity.mLinkNo = -1;
                    entityList.Add(entity);
                }
            }
            return entityList;
        }

        /// <summary>
        /// 要素のトリム
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> trim(List<PickData> pickEntity, Point3D sp, Point3D ep, FACE3D face, bool surface)
        {
            List<Entity> entityList = new List<Entity>();
            if (0 < ep.length(sp)) {
                foreach (var pick in pickEntity) {
                    Entity entity = mEntityList[pick.mEntityNo].toCopy();
                    entity.trim(sp, ep, pick.mPos, face);
                    entity.createVertexData();
                    if (surface)
                        entity.createSurfaceData();
                    entity.mLinkNo = -1;
                    entityList.Add(entity);
                }
            }
            return entityList;
        }

        /// <summary>
        /// 要素の拡大縮小
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="cp">拡大中心</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> scale(List<PickData> pickEntity, Point3D cp, Point3D sp, Point3D ep, FACE3D face, bool surface)
        {
            List<Entity> entityList = new List<Entity>();
                double scale = cp.length(ep) / cp.length(sp);
                    foreach (var pick in pickEntity) {
                        Entity entity = mEntityList[pick.mEntityNo].toCopy();
                        entity.scale(sp, scale, pick.mPos, face);
                        entity.createVertexData();
                if (surface)
                    entity.createSurfaceData();
                entity.mLinkNo = -1;
                entityList.Add(entity);
            }
            return entityList;
        }

        /// <summary>
        /// 要素のストレッチ
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="face">2D平面</param>
        /// <param name="arc">円弧ストレッチ</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>編集要素リスト</returns>
        public List<Entity> stretch(List<PickData> pickEntity, Point3D sp, Point3D ep, bool arc, FACE3D face, bool surface)
        {
            List<Entity> entityList = new List<Entity>();
            Point3D v = ep - sp;
            if (v.length() < mEps)
                return entityList;
            foreach (var pick in pickEntity) {
                Entity entity = mEntityList[pick.mEntityNo].toCopy();
                entity.stretch(v, arc, pick.mPos, face);
                entity.createVertexData();
                if (surface)
                    entity.createSurfaceData();
                entity.mLinkNo = -1;
                entityList.Add(entity);
            }
            return entityList;
        }

        /// <summary>
        /// 要素分割
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="pos">分割位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>分割要素リスト</returns>
        public List<Entity> divide(List<PickData> pickEntity, Point3D pos, FACE3D face)
        {
            List<Entity> entityList = new List<Entity>();
            foreach (var pick in pickEntity) {
                Entity entity = mEntityList[pick.mEntityNo].toCopy();
                entityList.AddRange(entity.divide(pos, face));
            }
            foreach (Entity ent in entityList) {
                ent.createVertexData();
                ent.createSurfaceData();
                ent.mLinkNo = -1;
            }
            return entityList;
        }

        /// <summary>
        /// フィレット作成(線分と線分、ポリライン、ポリゴンのみ)
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="r">フィレット半径</param>
        /// <param name="face">2D平面</param>
        /// <returns>要素リスト</returns>
        public List<Entity> fillet(List<PickData> pickEntity, double r, FACE3D face)
        {
            List<Entity> entityList = new List<Entity>();
            if (pickEntity.Count == 2) {
                if (mEntityList[pickEntity[0].mEntityNo].mID == EntityId.Line &&
                    mEntityList[pickEntity[1].mEntityNo].mID == EntityId.Line) {
                    LineEntity line0 = (LineEntity)mEntityList[pickEntity[0].mEntityNo];
                    LineEntity line1 = (LineEntity)mEntityList[pickEntity[1].mEntityNo];
                    entityList = filletLineLine(line0, line1, r, pickEntity[0].mPos, pickEntity[1].mPos, face);
                }
            } else if (pickEntity.Count == 1) {
                if (mEntityList[pickEntity[0].mEntityNo].mID == EntityId.Polyline) {
                    PolylineEntity polyline = (PolylineEntity)mEntityList[pickEntity[0].mEntityNo];
                    entityList = filletPolyline(polyline, r, pickEntity[0].mPos, face);
                } else if (mEntityList[pickEntity[0].mEntityNo].mID == EntityId.Polygon) {
                    PolygonEntity polygon = (PolygonEntity)mEntityList[pickEntity[0].mEntityNo];
                    entityList = filletPolygon(polygon, r, pickEntity[0].mPos, face);
                }
            }

            foreach (Entity ent in entityList) {
                ent.createVertexData();
                ent.createSurfaceData();
                ent.mLinkNo = -1;
            }
            return entityList;
        }

        /// <summary>
        /// 線分同士のフィレット作成
        /// </summary>
        /// <param name="ent0">線分要素</param>
        /// <param name="ent1">線分要素</param>
        /// <param name="r">フィレット半径</param>
        /// <param name="pos0">ピック位置</param>
        /// <param name="pos1">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>要素リスト</returns>
        private List<Entity> filletLineLine(LineEntity ent0, LineEntity ent1, double r, PointD pos0, PointD pos1, FACE3D face)
        {
            List<Entity> entityList = new List<Entity>();
            LineD line0 = ent0.mLine.toLineD(face);
            LineD line1 = ent1.mLine.toLineD(face);
            if (r == 0) {
                PointD ip = line0.intersection(line1);
                line0.trimOn(ip, pos0);
                line1.trimOn(ip, pos1);
            } else if (0 < r) {
                ArcD arc = new ArcD(r, line0, pos0, line1, pos1);
                if (arc.mCp != null && !arc.mCp.isNaN()) {
                    ArcEntity arcEnt = new ArcEntity(new Arc3D(arc, face), ent0.mLayerBit.Length * 8);
                    arcEnt.copyProperty(ent0);
                    entityList.Add(arcEnt);
                    PointD p0 = arc.startPoint();
                    PointD p1 = arc.endPoint();
                    if (line0.onPointEx(p0)) {
                        line0.trimOn(p0, pos0);
                        line1.trimOn(p1, pos1);
                    } else {
                        line0.trimOn(p1, pos0);
                        line1.trimOn(p0, pos1);
                    }
                } else
                    return entityList;
            } else
                return entityList;
            LineEntity lineEnt0 = (LineEntity)ent0.toCopy();
            lineEnt0.mLine = new Line3D(line0, face);
            entityList.Add(lineEnt0);
            LineEntity lineEnt1 = (LineEntity)ent1.toCopy();
            lineEnt1.mLine = new Line3D(line1, face);
            entityList.Add(lineEnt1);

            return entityList;
        }

        /// <summary>
        /// ポリライン頂点のフィレット作成
        /// </summary>
        /// <param name="ent">ポリライン要素</param>
        /// <param name="r">フィレット半径</param>
        /// <param name="pos">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>要素リスト</returns>
        private List<Entity> filletPolyline(PolylineEntity ent, double r, PointD pos, FACE3D face)
        {
            List<Entity> entityList = new List<Entity>();
            PolylineD polyline = ent.mPolyline.toPolylineD(face);
            if (0 < r) {
                polyline.fillet(r, pos);
                PolylineEntity polylineEnt = (PolylineEntity)ent.toCopy();
                polylineEnt.mPolyline = new Polyline3D(polyline, face);
                entityList.Add(polylineEnt);
            }
            return entityList;
        }

        /// <summary>
        /// ポリゴン頂点のフィレット作成
        /// </summary>
        /// <param name="ent">ポリゴン要素</param>
        /// <param name="r">フィレット半径</param>
        /// <param name="pos">ピック位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>要素リスト</returns>
        private List<Entity> filletPolygon(PolygonEntity ent, double r, PointD pos, FACE3D face)
        {
            List<Entity> entityList = new List<Entity>();
            PolygonD polygon = ent.mPolygon.toPolygonD(face);
            if (0 < r) {
                polygon.fillet(r, pos);
                PolygonEntity polygonEnt = (PolygonEntity)ent.toCopy();
                polygonEnt.mPolygon = new Polygon3D(polygon, face);
                entityList.Add(polygonEnt);
            }
            return entityList;
        }

        /// <summary>
        /// 要素の接続
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="face">2D平面</param>
        /// <returns>要素リスト</returns>
        public List<Entity> connect(List<PickData> pickEntity, FACE3D face)
        {
            List<Entity> entityList = new List<Entity>();
            if (pickEntity.Count == 1) {
                //  1要素時ポリゴンに変換
                entityList.AddRange(connect1(mEntityList[pickEntity[0].mEntityNo]));
            } else if (pickEntity.Count == 2 && pickEntity[0].mEntityNo != pickEntity[1].mEntityNo) {
                //  ２要素時ピック位置で要素同士を接続しポリラインへんかん
                entityList.AddRange(connect2(mEntityList[pickEntity[0].mEntityNo],
                    new Point3D(pickEntity[0].mPos, face),
                    mEntityList[pickEntity[1].mEntityNo],
                    new Point3D(pickEntity[1].mPos, face)));
            } else if (2 < pickEntity.Count) {
                //  3要素以上複数の要素を接続しポリラインに変換
                entityList.AddRange(connect3(pickEntity));
            }
            //  サーフェスデータと2D表示テータの作成
            foreach (Entity ent in entityList) {
                ent.createVertexData();
                ent.createSurfaceData();
                ent.mLinkNo = -1;
            }
            return entityList;
        }

        /// <summary>
        /// 要素(１要素時)をポリゴンに変換(Arc/Plylineのみ)
        /// </summary>
        /// <param name="entity">指定要素</param>
        /// <returns>ポリゴン要素リスト</returns>
        private List<Entity> connect1(Entity entity)
        {
            List<Entity> entityList = new List<Entity>();
            Polygon3D polygon = new Polygon3D();
            if (entity.mID == EntityId.Arc) {
                ArcEntity arcEnt = (ArcEntity)entity;
                polygon = new Polygon3D(arcEnt.mArc);
            } else if (entity.mID == EntityId.Polyline) {
                PolylineEntity polyline = (PolylineEntity)entity;
                polygon = new Polygon3D(polyline.mPolyline);
            } else 
                return entityList;
            PolygonEntity polygonEnt = new PolygonEntity(polygon, mGlobal.mLayerSize);
            polygonEnt.copyProperty(entity);
            entityList.Add(polygonEnt);
            return entityList;
        }

        /// <summary>
        /// 2要素を接続しポリラインに変換(ピック位置に近い端点同士を接続)
        /// </summary>
        /// <param name="entity0">要素0</param>
        /// <param name="pos0">ピック位置0</param>
        /// <param name="entity1">要素1</param>
        /// <param name="pos1">ピック位置1</param>
        /// <returns>ポリライン要素リスト</returns>
        private List<Entity> connect2(Entity entity0, Point3D pos0, Entity entity1, Point3D pos1)
        {
            List<Entity> entityList = new List<Entity>();
            Polyline3D polyline0, polyline1;
            if (entity0.mID == EntityId.Line || entity0.mID == EntityId.Arc ||
                entity0.mID == EntityId.Polyline || entity0.mID == EntityId.Polygon) {
                polyline0 = new Polyline3D(entity0.toPointList());
            } else
                return null;
            if (entity1.mID == EntityId.Line || entity1.mID == EntityId.Arc ||
                entity1.mID == EntityId.Polyline || entity1.mID == EntityId.Polygon) {
                polyline1 = new Polyline3D(entity1.toPointList());
            } else
                return null;
            polyline0.connect(pos0, polyline1, pos1);
            PolylineEntity polylineEnt = new PolylineEntity(polyline0 , mGlobal.mLayerSize);
            polylineEnt.copyProperty(entity0);
            entityList.Add(polylineEnt);

            return entityList;
        }

        /// <summary>
        /// 3要素以上を接続してポリラインに変換
        /// </summary>
        /// <param name="pickEntity">ピック要素リスト</param>
        /// <returns>ポリライン要素リスト</returns>
        private List<Entity> connect3(List<PickData> pickEntity)
        {
            List<Entity> entityList = new List<Entity>();
            Polyline3D polyline0, polyline1;
            Entity entity = mEntityList[pickEntity[0].mEntityNo];
            if (entity.mID == EntityId.Line || entity.mID == EntityId.Arc ||
                entity.mID == EntityId.Polyline || entity.mID == EntityId.Polygon) {
                polyline0 = new Polyline3D(entity.toPointList());
            } else
                return entityList;
            for (int i = 1; i < pickEntity.Count; i++) {
                entity = mEntityList[pickEntity[i].mEntityNo];
                if (entity.mID == EntityId.Line || entity.mID == EntityId.Arc ||
                    entity.mID == EntityId.Polyline || entity.mID == EntityId.Polygon) {
                    polyline1 = new Polyline3D(entity.toPointList());
                    polyline0.connect(polyline1);
                } else
                    return entityList;

            }
            PolylineEntity polylineEnt = new PolylineEntity(polyline0, mGlobal.mLayerSize);
            polylineEnt.copyProperty(entity);
            entityList.Add(polylineEnt);
            return entityList;
        }

        /// <summary>
        /// ポリラインまたはポリゴンを線分と円弧に分解する
        /// </summary>
        /// <param name="pickEntity">ピック要素リスト</param>
        /// <returns>要素リスト</returns>
        public List<Entity> disassemble(List<PickData> pickEntity)
        {
            List<Entity> entityList = new List<Entity>();
            Arc3D arc;
            ArcEntity arcEnt;
            for (int i = 0; i < pickEntity.Count; i++) {
                Entity entity = mEntityList[pickEntity[i].mEntityNo];
                if (entity.mID == EntityId.Polyline || entity.mID == EntityId.Polygon) {
                    List<Point3D> plist = entity.toPointList();
                    int sp = 0;
                    if (plist[0].type == 1) {
                        arc = new Arc3D(plist.Last(), plist[0], plist[1]);
                        arcEnt = new ArcEntity(arc, mGlobal.mLayerSize);
                        arcEnt.copyProperty(entity);
                        entityList.Add(arcEnt);
                        sp = 1;
                    }
                    for (int j = sp; j < plist.Count - 1 - sp; j++) {
                        if (plist[j + 1].type == 1) {
                            arc = new Arc3D(plist[j], plist[j + 1], plist[j + 2]);
                            arcEnt = new ArcEntity(arc, mGlobal.mLayerSize);
                            arcEnt.copyProperty(entity);
                            entityList.Add(arcEnt);
                            j++;
                        } else {
                            Line3D line = new Line3D(plist[j], plist[j + 1]);
                            LineEntity lineEnt = new LineEntity(line, mGlobal.mLayerSize);
                            lineEnt.copyProperty(entity);
                            entityList.Add(lineEnt);
                        }
                    }
                }
            }
            //  サーフェスデータと2D表示テータの作成
            foreach (Entity ent in entityList) {
                ent.createVertexData();
                ent.createSurfaceData();
                ent.mLinkNo = -1;
            }
            return entityList;
        }

        /// <summary>
        /// 2D非表示を全解除
        /// </summary>
        public void disp2DReset()
        {
            mEntityList.ForEach(p => p.mDisp2D = true);
        }


        /// <summary>
        /// 押出データの作成
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>要素リスト</returns>
        public List<Entity> extrusion(List<PickData> pickEntity, Point3D sp, Point3D ep, bool surface = false)
        {
            List<Entity> entityList = new List<Entity>();
            if (pickEntity == null || pickEntity.Count == 0) return entityList;
            Point3D v = ep - sp;
            bool circle = false;
            if (mEntityList[pickEntity[0].mEntityNo].mID == EntityId.Arc) {
                ArcEntity arc = (ArcEntity)mEntityList[pickEntity[0].mEntityNo];
                if (Math.PI * 2 <= arc.mArc.mOpenAngle) circle = true;
            }
            if (mEntityList[pickEntity[0].mEntityNo].mID == EntityId.Polygon || circle) {
                //  ポリゴンとして処理
                List<Polygon3D> polygons = getPolygon(pickEntity);
                if (0 < polygons.Count) {
                    ExtrusionEntity extrusion = new ExtrusionEntity(polygons, v, mGlobal.mLayerSize);
                    extrusion.copyProperty(mEntityList[pickEntity[0].mEntityNo]);
                    extrusion.createVertexData();
                    if (surface)
                        extrusion.createSurfaceData();
                    entityList.Add(extrusion);
                }
            } else {
                //  ポリラインとして処理
                List<Polyline3D> polylines = getPolyline(pickEntity);
                if (0 < polylines.Count) {
                    foreach (Polyline3D polyline in polylines) {
                        List<Polyline3D> polylist = new List<Polyline3D> { polyline };
                        ExtrusionEntity extrusion = new ExtrusionEntity(polylist, v, mGlobal.mLayerSize);
                        extrusion.copyProperty(mEntityList[pickEntity[0].mEntityNo]);
                        extrusion.createVertexData();
                        if (surface)
                            extrusion.createSurfaceData();
                        entityList.Add(extrusion);

                    }
                }
            }
            return entityList;
        }

        /// <summary>
        /// ブレンドデータの作成
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>要素リスト</returns>
        public List<Entity> blend(List<PickData> pickEntity, bool surface = false)
        {
            List<Entity> entityList = new List<Entity>();
            if (pickEntity == null || pickEntity.Count == 0) return entityList;
            List<Polyline3D> polylines = getPolyline(pickEntity);
            if (0 < polylines.Count) {
                BlendEntity blend = new BlendEntity(polylines, mGlobal.mLayerSize);
                blend.copyProperty(mEntityList[pickEntity[0].mEntityNo]);
                blend.createVertexData();
                if (surface)
                    blend.createSurfaceData();
                entityList.Add(blend);
            }

            return entityList;
        }


        /// <summary>
        /// 回転体データの作成
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sa">終角</param>
        /// <param name="ea">終角</param>
        /// <param name="surface">3Dデータの作成</param>
        /// <returns>要素リスト</returns>
        public List<Entity> revolution(List<PickData> pickEntity, double sa, double ea, bool surface = false)
        {
            List<Entity> entityList = new List<Entity>();
            if (pickEntity == null || pickEntity.Count == 0) return entityList;
            RevolutionEntity revolusion;
            Line3D centerLine = null;
            Polyline3D outline = null;
            bool close = false;
            for (int i = 0; i < pickEntity.Count; i++) {
                if (centerLine != null && mEntityList[pickEntity[i].mEntityNo].mID == EntityId.Line) {
                    Line3D line = ((LineEntity)mEntityList[pickEntity[i].mEntityNo]).mLine.toCopy();
                    outline = new Polyline3D(line);
                } else if (mEntityList[pickEntity[i].mEntityNo].mID == EntityId.Line) {
                    centerLine = ((LineEntity)mEntityList[pickEntity[i].mEntityNo]).mLine.toCopy();
                } else if (mEntityList[pickEntity[i].mEntityNo].mID == EntityId.Polyline) {
                    outline = ((PolylineEntity)mEntityList[pickEntity[i].mEntityNo]).mPolyline;
                } else if (mEntityList[pickEntity[i].mEntityNo].mID == EntityId.Polygon) {
                    Polygon3D polygon = ((PolygonEntity)mEntityList[pickEntity[i].mEntityNo]).mPolygon;
                    outline = new Polyline3D(polygon, true);
                } else if (mEntityList[pickEntity[i].mEntityNo].mID == EntityId.Arc) {
                    Arc3D arc = ((ArcEntity)mEntityList[pickEntity[i].mEntityNo]).mArc;
                    outline = new Polyline3D(arc, mGlobal.mDivAngle);
                }
            }

            if (centerLine != null && outline != null)
                revolusion = new RevolutionEntity(centerLine, outline, sa, ea, close, mGlobal.mLayerSize);
            else
                return entityList;
            revolusion.copyProperty(mEntityList[pickEntity[1].mEntityNo]);
            revolusion.createVertexData();
            if (surface)
                revolusion.createSurfaceData();
            entityList.Add(revolusion);
            return entityList;
        }

        /// <summary>
        /// 掃引データの作成
        /// </summary>
        /// <param name="pickEntity">ピック要素</param>
        /// <param name="sa">終角</param>
        /// <param name="ea">終角</param>
        /// <param name="surface">3Dデータ作成</param>
        /// <returns>要素リスト</returns>
        public List<Entity> sweep(List<PickData> pickEntity,　double sa, double ea, bool surface = false)
        {
            List<Entity> entityList = new List<Entity>();
            if (pickEntity == null || pickEntity.Count == 0) return entityList;
            SweepEntity sweep;
            Polyline3D outLine1 = null;
            Polyline3D outLine2 = null;
            bool close = false;
            if (1 < mEntityList.Count) {
                if (mEntityList[pickEntity[0].mEntityNo].mID == EntityId.Polyline &&
                    mEntityList[pickEntity[1].mEntityNo].mID == EntityId.Polyline) {
                    outLine1 = ((PolylineEntity)mEntityList[pickEntity[0].mEntityNo]).mPolyline.toCopy();
                    outLine2 = ((PolylineEntity)mEntityList[pickEntity[1].mEntityNo]).mPolyline.toCopy();
                }
            }

            if (outLine1 != null && outLine2 != null)
                sweep = new SweepEntity(outLine1, outLine2, sa, ea, close, mGlobal.mLayerSize);
            else
                return entityList;
            sweep.copyProperty(mEntityList[pickEntity[1].mEntityNo]);
            sweep.createVertexData();
            if (surface)
                sweep.createSurfaceData();
            entityList.Add(sweep);
            return entityList;
        }

        /// <summary>
        /// ポリゴン要素の抽出(円データがあればポリゴンに変換)
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        /// <returns>ポリゴンリスト</returns>
        private List<Polygon3D> getPolygon(List<PickData> picks)
        {
            List<Polygon3D> polygons = new List<Polygon3D>();
            foreach (var pick in picks) {
                if (mEntityList[pick.mEntityNo].mID == EntityId.Polygon) {
                    PolygonEntity polygonEntity = (PolygonEntity)mEntityList[pick.mEntityNo];
                    polygons.Add(polygonEntity.mPolygon);
                } else if (mEntityList[pick.mEntityNo].mID == EntityId.Arc) {
                    ArcEntity arcEntity = (ArcEntity)mEntityList[pick.mEntityNo];
                    if (2 * Math.PI <= arcEntity.mArc.mOpenAngle) {
                        Polygon3D polygon = new Polygon3D(arcEntity.mArc.toPolyline3D(0));
                        polygons.Add(polygon);
                    }
                }
            }
            return polygons;
        }

        /// <summary>
        /// ポリライン要素の抽出(線分、円弧、ポリゴンはポリラインに変換
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        /// <returns>ポリラインリスト</returns>
        private List<Polyline3D> getPolyline(List<PickData> picks)
        {
            List<Polyline3D> polylines = new List<Polyline3D>();
            foreach (var pick in picks) {
                if (mEntityList[pick.mEntityNo].mID == EntityId.Polyline) {
                    PolylineEntity polylineEntity = (PolylineEntity)mEntityList[pick.mEntityNo];
                    polylines.Add(polylineEntity.mPolyline);
                } else if (mEntityList[pick.mEntityNo].mID == EntityId.Polygon) {
                    PolygonEntity polygonEntity = (PolygonEntity)mEntityList[pick.mEntityNo];
                    polylines.Add(polygonEntity.mPolygon.toPolyline3D());
                } else if (mEntityList[pick.mEntityNo].mID == EntityId.Arc) {
                    ArcEntity arcEntity = (ArcEntity)mEntityList[pick.mEntityNo];
                    if (2 * Math.PI > arcEntity.mArc.mOpenAngle) {
                        Polyline3D polygon = new Polyline3D(arcEntity.mArc.toPolyline3D(0));
                        polylines.Add(polygon);
                    }
                } else if (mEntityList[pick.mEntityNo].mID == EntityId.Line) {
                    LineEntity lineEntity = (LineEntity)mEntityList[pick.mEntityNo];
                    polylines.Add(new Polyline3D(lineEntity.mLine));
                }
            }
            return polylines;
        }

        /// <summary>
        /// 3D要素の解除
        /// </summary>
        /// <param name="pickEntity">要素リスト</param>
        /// <returns></returns>
        public List<Entity> release(List<PickData> pickEntity)
        {
            List<Entity> entities = new List<Entity>();
            foreach (PickData pick in pickEntity) {
                Entity entity = mEntityList[pick.mEntityNo];
                if (entity.mID == EntityId.Extrusion) {
                    //  押出解除
                    ExtrusionEntity extrusion = (ExtrusionEntity)entity;
                    foreach (var polygon in extrusion.mPolygons) {
                        if (extrusion.mClose) {
                            entities.Add(mCreateEntity.createPolygon(polygon, true));
                        } else {
                            entities.Add(mCreateEntity.createPolyline(polygon.toPolyline3D(0, false), true));
                        }
                    }
                } else if (entity.mID == EntityId.Blend) {
                    //  ブレンド解除
                    BlendEntity blend = (BlendEntity)entity;
                    foreach (var polyline in blend.mPolylines)
                        entities.Add(mCreateEntity.createPolyline(polyline, true));
                } else if (entity.mID == EntityId.Revolution) {
                    //  回転体解除
                    RevolutionEntity revolution = (RevolutionEntity)entity;
                    entities.Add(mCreateEntity.createLine(revolution.mCenterLine, true));
                    entities.Add(mCreateEntity.createPolyline(revolution.mOutLine, true));
                } else if (entity.mID == EntityId.Sweep) {
                    //  掃引解除
                    SweepEntity sweep = (SweepEntity)entity;
                    entities.Add(mCreateEntity.createPolyline(sweep.mOutLine1, true));
                    entities.Add(mCreateEntity.createPolyline(sweep.mOutLine2, true));
                }
            }
            foreach (Entity entity in entities)
                entity.setArea();
            return entities;
        }


        /// <summary>
        /// 要素の登録
        /// </summary>
        /// <param name="entity"></param>
        public void addEntity(Entity entity, int operationCount)
        {
            entity.setArea();
            entity.mOperationNo = operationCount;
            for (int i = mEntityList.Count - 1; 0 <= i; i--) {
                if (mEntityList[i].mRemove)
                    mEntityList.RemoveAt(i);
                else
                    break;
            }
            mEntityList.Add(entity);
        }

        /// <summary>
        /// リンク要素の作成
        /// </summary>
        /// <param name="linkNo">リンク要素No</param>
        public void addLink(int linkNo, int layersize, int operationCount)
        {
            LinkEntity entity = new LinkEntity(linkNo, layersize);
            entity.mOperationNo = operationCount;
            mEntityList[entity.mLinkNo].mRemove = true;
            mEntityList.Add(entity);
        }
    }
}
