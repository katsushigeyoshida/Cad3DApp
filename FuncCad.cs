using CoreLib;
using System.Windows.Controls;

namespace Cad3DApp
{
    /// <summary>
    /// スクリプト拡張関数クラス(CADコントロール関数)
    /// </summary>
    public class FuncCad
    {
        public static string[] mFuncNames = new string[] {
            "cad.disp(); 表示",
            "cad.setColor(\"Blue\"); 色の設定",
            "cad.setLineType(\"dash\"); 線種の設定(\"solid\", \"dash\", \"center\", \"phantom\")",
            "cad.setLineThickness(2); 線の太さの設定",
            "cad.setFace(\"xy\"); 作成面の設定(\"front\", \"top\", \"right\")",
            "cad.line(xs,ys,zs,xe,ye,ze); 線分を作成",
            "cad.line(sp[],ep[]); 線分を作成",
            "cad.arc(cp[],r[,sa[,ea]]); 円/円弧の作成",
            "cad.arc(sp[],mp[],ep); 三点円弧の作成",
            "cad.circle(sp[],mp[],ep); 三点円の作成",
            "cad.polyline(p[,]); ポリラインの作成",
            "cad.polygon(p[,]); ポリゴンの作成",
            "cad.extrusion(v[],p[,][,p1[,]...]); 押出要素の作成",
            "cad.blend(p[,][,p1[,]...]); ブレンドの作成",
            "cad.revolution(centerlin[,],polylin[,][,sa[,ea[,close]]]); 回転体の作成",
            "cad.sweep(polyline0[,],polyline1[,][,sa[,ea[,close]]]); 掃引の作成",
        };

        public KScript mScript;
        public List<Entity> mEntityList = new List<Entity>();   //  要素リスト
        public GlobalData mGlobal;                              //  グローバルデータ

        CreateEntity mCreateEntity;
        EditEntity mEditEntity;

        private KParse mParse;
        private Variable mVar;
        private KLexer mLexer = new KLexer();
        private YLib ylib = new YLib();
        private YDraw ydraw = new YDraw();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="script">KScript</param>
        /// <param name="entityList">要素リスト</param>
        /// <param name="global">グローバルデータ</param>
        public FuncCad(KScript script, List<Entity> entityList, GlobalData global)
        {
            mScript = script;
            mParse = script.mParse;
            mVar = script.mVar;
            mEntityList = entityList;
            mGlobal = global;
            mCreateEntity = new CreateEntity(mGlobal);
            mEditEntity = new EditEntity(mGlobal, mEntityList);
        }

        /// <summary>
        /// 拡張関数
        /// </summary>
        /// <param name="funcName">関数名</param>
        /// <param name="arg">引数</param>
        /// <param name="ret">返値</param>
        /// <returns></returns>
        public Token cadFunc(Token funcName, Token arg, Token ret)
        {
            List<Token> args = mScript.getFuncArgs(arg.mValue);
            switch (funcName.mValue) {
                case "cad.init": init(); break;
                case "cad.disp": disp(); break;
                case "cad.setColor": setColor(args); break;
                case "cad.setLineType": setLineType(args); break;
                case "cad.setLineThickness": setLineThickness(args); break;
                case "cad.setFace": setFace(args); break;
                case "cad.line": line(args); break;
                case "cad.arc": arc(args); break;
                case "cad.circle": circle(args); break;
                case "cad.polyline": polyline(args); break;
                case "cad.polygon": polygon(args); break;
                case "cad.extrusion": extrusion(args); break;
                case "cad.blend": blend(args); break;
                case "cad.revolution": revolution(args); break;
                case "cad.sweep": sweep(args); break;
                default: return new Token("not found func", TokenType.ERROR);
            }
            return new Token("", TokenType.EMPTY);
        }

        /// <summary>
        /// パラメータの初期化
        /// </summary>
        private void init()
        {
            mGlobal.mEntityBrush = ylib.mBrushList[mGlobal.mMainWindow.cbColor.SelectedIndex].brush;
            mGlobal.mLineType = ydraw.mLineTypeName.FindIndex("solid");
            mGlobal.mLineThickness = 1;
            var item = (TabItem)mGlobal.mMainWindow.tabCanvas.SelectedItem;
            mGlobal.mMainWindow.setFace(item.Name);
            mGlobal.mFace = mGlobal.mMainWindow.mFace;
        }

        /// <summary>
        /// 再表示
        /// </summary>
        private void disp()
        {
            mGlobal.mMainWindow.mDataManage.commandClear();
        }

        /// <summary>
        /// 色設定
        /// </summary>
        /// <param name="args"></param>
        private void setColor(List<Token> args)
        {
            if (0 < args.Count) {
                string colorName = ylib.stripBracketString(args[0].mValue, '"');
                //mGlobal.mMainWindow.cbColor.SelectedIndex = ylib.getBrushNo(ylib.getColor(colorName));
                mGlobal.mEntityBrush = ylib.getColor(colorName);
            }
        }

        /// <summary>
        /// 線種の設定(solid,dash,center,phantom)
        /// </summary>
        /// <param name="args"></param>
        private void setLineType(List<Token> args)
        {
            if (0 < args.Count) {
                string lineType = ylib.stripBracketString(args[0].mValue, '"');
                mGlobal.mLineType = ydraw.mLineTypeName.FindIndex(lineType);
            }
        }

        /// <summary>
        /// 線の太さの設定
        /// </summary>
        /// <param name="args"></param>
        private void setLineThickness(List<Token> args)
        {
            if (0 < args.Count) {
                string thickness = ylib.stripBracketString(args[0].mValue, '"');
                mGlobal.mLineThickness = ylib.doubleParse(thickness);
            }
        }

        /// <summary>
        /// 作成面の設定
        /// </summary>
        /// <param name="args"></param>
        private void setFace(List<Token> args)
        {
            if (0 < args.Count) {
                string face = ylib.stripBracketString(args[0].mValue, '"').ToUpper();
                mGlobal.mFace = (FACE3D)Enum.Parse(typeof(FACE3D), face);
            }
        }


        /// <summary>
        /// 線の作成(line(sx,sy,sz,ex,ey,ez),line(sp[],ep[]),line{plist[,]))
        /// </summary>
        /// <param name="args"></param>
        private void line(List<Token> args)
        {
            Point3D sp = null;
            Point3D ep = null;
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 1 && mVar.getArrayOder(args[1]) == 1) {
                //  line(sp[],ep[])
                List<double> spList = mVar.cnvListDouble(args[0]);
                List<double> epList = mVar.cnvListDouble(args[1]);
                sp = new Point3D(spList[0], spList[1], spList[2]);
                ep = new Point3D(epList[0], epList[1], epList[2]);
            } else if (0 < args.Count && mVar.getArrayOder(args[0]) == 2) {
                //  line(plist[,])
                double[,] plist = mVar.cnvArrayDouble2(args[0]);
                if (1 < plist.GetLength(0) && 2 < plist.GetLength(1)) {
                    sp = new Point3D(plist[0,0], plist[0,1], plist[0,2]);
                    ep = new Point3D(plist[1,0], plist[1,1], plist[1,2]);
                }
            } else if (6 <= args.Count) {
                //  line(sx,sy,sz,ex,ey,ez)
                List<double> datas = new List<double>();
                for (int i = 0; i < args.Count; i++)
                    if (mVar.getArrayOder(args[i]) == 0)
                        datas.Add(ylib.doubleParse(args[i].mValue));
                if (6 <= datas.Count) {
                    sp = new Point3D(datas[0], datas[1], datas[2]);
                    ep = new Point3D(datas[3], datas[4], datas[5]);
                }
            }
            //  Entity作成
            if (sp != null && ep != null) {
                Entity entity = mCreateEntity.createLine(sp, ep, true);
                mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                mGlobal.mMainWindow.mDataManage.updateArea();
            }
        }

        /// <summary>
        /// 円弧の作成(arc(cp[],r,sa,ea)
        /// </summary>
        /// <param name="args"></param>
        private void arc(List<Token> args)
        {
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 1 && mVar.getArrayOder(args[1]) == 0) {
                //  arc(cp[],r[sa[.ea]])
                Point3D cp = null;
                double r = 1, sa = 0, ea = Math.PI * 2;
                List<double> cpList = mVar.cnvListDouble(args[0]);
                if (2 < cpList.Count) {
                    cp = new Point3D(cpList[0], cpList[1], cpList[2]);
                    r = ylib.doubleParse(args[1].mValue);
                }
                if (2 < args.Count)
                    sa = ylib.doubleParse(args[2].mValue);
                if (3 < args.Count)
                    ea = ylib.doubleParse(args[3].mValue);
                if (cp != null) {
                    Entity entity = mCreateEntity.createArc(cp, r, sa, ea, mGlobal.mFace, true);
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            } else if (2 < args.Count && mVar.getArrayOder(args[0]) == 1 &&
                mVar.getArrayOder(args[1]) == 1 && mVar.getArrayOder(args[2]) == 1) {
                //  arc(sp[],mp[],ep[])
                Point3D sp = null, mp = null, ep = null;
                List<double> spList = mVar.cnvListDouble(args[0]);
                List<double> mpList = mVar.cnvListDouble(args[1]);
                List<double> epList = mVar.cnvListDouble(args[2]);
                if (2 < spList.Count)
                    sp = new Point3D(spList[0], spList[1], spList[2]);
                if (2 < mpList.Count)
                    mp = new Point3D(mpList[0], mpList[1], mpList[2]);
                if (2 < epList.Count)
                    ep = new Point3D(epList[0], epList[1], epList[2]);
                if (sp != null && mp != null && ep != null) {
                    Entity entity = mCreateEntity.createArc(sp, mp, ep, true);
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            }
        }

        /// <summary>
        /// 円の作成(circle(sp[],mp[],ep[]))
        /// </summary>
        /// <param name="args"></param>
        private void circle(List<Token> args)
        {
            if (2 < args.Count && mVar.getArrayOder(args[0]) == 1 &&
                mVar.getArrayOder(args[1]) == 1 && mVar.getArrayOder(args[2]) == 1) {
                //  arc(sp[],mp[],ep[])
                Point3D sp = null, mp = null, ep = null;
                List<double> spList = mVar.cnvListDouble(args[0]);
                List<double> mpList = mVar.cnvListDouble(args[1]);
                List<double> epList = mVar.cnvListDouble(args[2]);
                if (2 < spList.Count)
                    sp = new Point3D(spList[0], spList[1], spList[2]);
                if (2 < mpList.Count)
                    mp = new Point3D(mpList[0], mpList[1], mpList[2]);
                if (2 < epList.Count)
                    ep = new Point3D(epList[0], epList[1], epList[2]);
                if (sp != null && mp != null && ep != null) {
                    Arc3D arc = new Arc3D(sp, mp, ep);
                    arc.mSa = 0;
                    arc.mEa = Math.PI * 2;
                    Entity entity = mCreateEntity.createArc(arc, true);
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            }
        }

        /// <summary>
        /// ポリラインの作成(polyline(plist[,]))
        /// </summary>
        /// <param name="args"></param>
        private void polyline(List<Token> args)
        {
            if (0 < args.Count && mVar.getArrayOder(args[0]) == 2) {
                double[,] points = mVar.cnvArrayDouble2(args[0]);
                Entity entity = null;
                if (points.GetLength(1) == 2) {
                    //  3D座標で作成
                    List<PointD> plist = new List<PointD>();
                    for (int i = 0; i < points.GetLength(0); i++) {
                        PointD p = new PointD(points[i, 0], points[i, 1]);
                        plist.Add(p);
                    }
                    entity = mCreateEntity.createPolyline(plist, mGlobal.mFace, true);
                } else if (2 < points.GetLength(1)) {
                    //  2D座標で指定面に作成
                    List<Point3D> plist = new List<Point3D>();
                    for (int i = 0; i < points.GetLength(0); i++) {
                        Point3D p = new Point3D(points[i, 0], points[i, 1], points[i, 2]);
                        plist.Add(p);
                    }
                    entity = mCreateEntity.createPolyline(plist, true);
                }
                if (entity != null) {
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            }
        }

        /// <summary>
        /// ポリゴンの作成(polygon(plist[,]))
        /// </summary>
        /// <param name="args"></param>
        private void polygon(List<Token> args)
        {
            if (0 < args.Count && mVar.getArrayOder(args[0]) == 2) {
                double[,] points = mVar.cnvArrayDouble2(args[0]);
                Entity entity = null;
                if (points.GetLength(1) == 2) {
                    //  2D座標+作成面
                    List<PointD> plist = new List<PointD>();
                    for (int i = 0; i < points.GetLength(0); i++) {
                        PointD p = new PointD(points[i, 0], points[i, 1]);
                        plist.Add(p);
                    }
                    entity = mCreateEntity.createPolygon(plist, mGlobal.mFace, true);
                } else if (2 < points.GetLength(1)) {
                    //  3D座標
                    List<Point3D> plist = new List<Point3D>();
                    for (int i = 0; i < points.GetLength(0); i++) {
                        Point3D p = new Point3D(points[i, 0], points[i, 1], points[i, 2]);
                        plist.Add(p);
                    }
                    entity = mCreateEntity.createPolygon(plist, true);
                }
                if (entity != null) {
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            }
        }

        /// <summary>
        /// 押出要素の作成(extrusion(v[],plist[,],plist2[,],...);)
        /// v = { 0,0,10} 押出ベクトル
        /// plist[,] = { {0,0}, {10,0},{10,10},{0,10}};
        /// </summary>
        /// <param name="args"></param>
        private void extrusion(List<Token> args)
        {
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 1) {
                //  押出ベクトル
                List<double> plist = mVar.cnvListDouble(args[0]);
                Point3D v = null;
                if (2 < plist.Count)
                    v = new Point3D(plist[0], plist[1], plist[2]);
                List<Polygon3D> polygons = new List<Polygon3D>();
                for (int i = 1; i < args.Count; i++) {
                    double [,] pplist = mVar.cnvArrayDouble2(args[i]);
                    if (pplist.GetLength(1) == 2) {
                        //  2D座標+作成面
                        List<PointD> points = new List<PointD>();
                        for (int j = 0; j < pplist.GetLength(0); j++) {
                            PointD p = new PointD(pplist[j, 0], pplist[j, 1]);
                            points.Add(p);
                        }
                        polygons.Add(new Polygon3D(points, mGlobal.mFace));
                    } else if (pplist.GetLength(1) == 3) {
                        //  3D座標
                        List<Point3D> points = new List<Point3D>();
                        for (int j = 0; j < pplist.GetLength(0); j++) {
                            Point3D p = new Point3D(pplist[j, 0], pplist[j, 1], pplist[j, 2]);
                            points.Add(p);
                        }
                        polygons.Add(new Polygon3D(points));
                    }
                    if (0 < polygons.Count) {
                        Entity entity = mCreateEntity.createExtrusion(polygons, v, true);
                        mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                        mGlobal.mMainWindow.mDataManage.updateArea();
                    }
                }
            }
        }

        /// <summary>
        /// ブレンド要素の作成(blend(plist0[,][,plist1[,]...]))
        /// </summary>
        /// <param name="args"></param>
        private void blend(List<Token> args)
        {
            if (0 < args.Count) {
                List<Polyline3D> polylines = new List<Polyline3D>();
                for (int i = 0; i < args.Count; i++) {
                    double[,] pplist = mVar.cnvArrayDouble2(args[i]);
                    if (pplist.GetLength(1) == 2) {
                        //  2D座標+作成面
                        List<PointD> points = new List<PointD>();
                        for (int j = 0; j < pplist.GetLength(0); j++) {
                            PointD p = new PointD(pplist[j, 0], pplist[j, 1]);
                            points.Add(p);
                        }
                        polylines.Add(new Polyline3D(points, mGlobal.mFace));
                    } else if (pplist.GetLength(1) == 3) {
                        //  3D座標
                        List<Point3D> points = new List<Point3D>();
                        for (int j = 0; j < pplist.GetLength(0); j++) {
                            Point3D p = new Point3D(pplist[j, 0], pplist[j, 1], pplist[j, 2]);
                            points.Add(p);
                        }
                        polylines.Add(new Polyline3D(points));
                    }
                }
                if (0 < polylines.Count) {
                    Entity entity = mCreateEntity.createBlend(polylines, true);
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            }
        }

        /// <summary>
        /// 回転体の作成(revolution(centerlin[,],polylin[,][,sa[,ea[,close]]]))
        /// </summary>
        /// <param name="args"></param>
        private void revolution(List<Token> args)
        {
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 2 && mVar.getArrayOder(args[1]) == 2) {
                //  回転中心線
                double[,] plist = mVar.cnvArrayDouble2(args[0]);
                Line3D centerline = null;
                double sa = 0;
                double ea = Math.PI * 2;
                bool close = true;
                if (1 < plist.GetLength(0)) {
                    Point3D sp = new Point3D(plist[0,0], plist[0,1], plist[0,2]);
                    Point3D ep = new Point3D(plist[1,0], plist[1,1], plist[1,2]);
                    centerline = new Line3D(sp, ep);
                }
                //  外形線
                double[,] pplist = mVar.cnvArrayDouble2(args[1]);
                Polyline3D polyline = null;
                if (pplist.GetLength(1) == 2) {
                    //  2D座標+作成面
                    List<PointD> points = new List<PointD>();
                    for (int j = 0; j < pplist.GetLength(0); j++) {
                        PointD p = new PointD(pplist[j, 0], pplist[j, 1]);
                        points.Add(p);
                    }
                    polyline = new Polyline3D(points, mGlobal.mFace);
                } else if (pplist.GetLength(1) == 3) {
                    //  3D座標
                    List<Point3D> points = new List<Point3D>();
                    for (int j = 0; j < pplist.GetLength(0); j++) {
                        Point3D p = new Point3D(pplist[j, 0], pplist[j, 1], pplist[j, 2]);
                        points.Add(p);
                    }
                    polyline = new Polyline3D(points);
                }
                if (2 < args.Count)
                    sa = ylib.doubleParse(args[2].mValue);
                if (3 < args.Count)
                    ea = ylib.doubleParse(args[3].mValue);
                if (4 < args.Count)
                    close = ylib.boolParse(args[4].mValue);
                if (centerline != null && polyline != null) {
                    Entity entity = mCreateEntity.createRevolution(centerline, polyline, sa, ea, close, true);
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            }
        }

        /// <summary>
        /// 掃引(スイープ)の作成(sweep(polyline0[,],polyline1[,][,sa[,ea[,close]]]))
        /// </summary>
        /// <param name="args"></param>
        private void sweep(List<Token> args)
        {
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 2 && mVar.getArrayOder(args[1]) == 2) {
                //  外形線
                List<Polyline3D> polylines = new List<Polyline3D>();
                for (int i = 0; i < args.Count; i++) {
                    double[,] pplist = mVar.cnvArrayDouble2(args[i]);
                    if (pplist.GetLength(1) == 2) {
                        //  2D座標+作成面
                        List<PointD> points = new List<PointD>();
                        for (int j = 0; j < pplist.GetLength(0); j++) {
                            PointD p = new PointD(pplist[j, 0], pplist[j, 1]);
                            points.Add(p);
                        }
                        polylines.Add(new Polyline3D(points, mGlobal.mFace));
                    } else if (pplist.GetLength(1) == 3) {
                        //  3D座標
                        List<Point3D> points = new List<Point3D>();
                        for (int j = 0; j < pplist.GetLength(0); j++) {
                            Point3D p = new Point3D(pplist[j, 0], pplist[j, 1], pplist[j, 2]);
                            points.Add(p);
                        }
                        polylines.Add(new Polyline3D(points));
                    }
                }
                double sa = 0;
                double ea = Math.PI * 2;
                bool close = true;
                if (2 < args.Count)
                    sa = ylib.doubleParse(args[2].mValue);
                if (3 < args.Count)
                    ea = ylib.doubleParse(args[3].mValue);
                if (4 < args.Count)
                    close = ylib.boolParse(args[4].mValue);
                if (1 < polylines.Count) {
                    Entity entity = mCreateEntity.createSweep(polylines[0], polylines[1], sa, ea, close, true);
                    mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                    mGlobal.mMainWindow.mDataManage.updateArea();
                }
            }
        }
    }
}
