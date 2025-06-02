using CoreLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Cad3DApp
{
    /// <summary>
    /// 図形描画クラス
    /// </summary>
    public class DataDraw
    {
        private Canvas mCanvas;
        private System.Windows.Controls.Image mImScreen;
        private BitmapSource mBitmapSource;                     //  CanvasのBitmap一時保存
        private bool mBitmapOn = false;                         //  Bitmap取得状態

        public Brush mBaseBackColor = Brushes.White;            //  2D背景色
        public double mWorldSize = 10.0;                        //  3D 空間サイズ
        public int mScrollSize = 19;                            //  キーによるスクロール単位
        public List<PointD> mAreaLoc = new List<PointD>() {     //  領域指定座標
            new PointD(), new PointD()
        };
        public Dictionary<FACE3D, Box> mWorldList = new Dictionary<FACE3D, Box>();  //  タブごとの表示領域
        public int mGridMinmumSize = 8;                         //  グリッドの最小スクリーンサイズ
        public Box3D mArea = new Box3D();                       //  要素表示エリア
        public Layer mLayer;                                    //  レイヤ
        public GlobalData mGlobal;                              //  グローバルデータ
        public List<Entity> mEntityList;                        //  要素リスト
        public LockPick mLocPick;                               //  ロケイト・ピック

        public YWorldDraw mGDraw;                               //  2D/3D表示ライブラリ
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="global">GlobalData</param>
        public DataDraw(GlobalData global)
        {
            mGlobal = global;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="mainWindow">MainWindow</param>
        /// <param name="face">表示面</param>
        public DataDraw(Canvas canvas, System.Windows.Controls.Image imScreen, GlobalData global)
        {
            mCanvas = canvas;
            mImScreen = imScreen;
            mGlobal = global;

            mGDraw = new YWorldDraw(mCanvas, mCanvas.ActualWidth, mCanvas.ActualHeight);
            setWorldWindow(new System.Windows.Size(mCanvas.ActualWidth, mCanvas.ActualHeight), mWorldSize);
            mGDraw.clear();
            mGDraw.mClipping = true;
        }

        /// <summary>
        /// Windowの設定
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="imScreen">ImageScreen</param>
        public void setWindowDraw(Canvas canvas, System.Windows.Controls.Image imScreen)
        {
            mCanvas = canvas;
            mImScreen = imScreen;
            mGDraw = new YWorldDraw(mCanvas, mCanvas.ActualWidth, mCanvas.ActualHeight);
            setWorldWindow(new System.Windows.Size(mCanvas.ActualWidth, mCanvas.ActualHeight), mWorldSize);
            mGDraw.clear();
            mGDraw.mClipping = true;
        }

        /// <summary>
        /// CanvasとImageを再設定
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="imScreen">Image</param>
        /// <param name="face">2D平面</param>
        public void setCanvas(Canvas canvas, System.Windows.Controls.Image imScreen)
        {
            if (!mWorldList.ContainsKey(mGlobal.mPreFace)) {
                mWorldList.Add(mGlobal.mPreFace, mGDraw.mWorld.toCopy());
            } else {
                mWorldList[mGlobal.mPreFace] = mGDraw.mWorld.toCopy();
            }
            mCanvas = canvas;
            mImScreen = imScreen;
            double width = mCanvas.ActualWidth;
            double height = mCanvas.ActualHeight;
            if (mCanvas.ActualWidth == 0 || mCanvas.ActualHeight == 0) {
                width = mGDraw.mView.Width;
                height = mGDraw.mView.Height;
            }
            if (!mWorldList.ContainsKey(mGlobal.mFace)) {
                mWorldList.Add(mGlobal.mFace, mArea.toBox(mGlobal.mFace));
            }
            mGDraw = new YWorldDraw(mCanvas, width, height);
            mGDraw.setWorldWindow(mWorldList[mGlobal.mFace]);
            mGlobal.mPreFace = mGlobal.mFace;
            mGDraw.clear();
            mGDraw.mClipping = true;
        }

        /// <summary>
        /// キー操作による2D表示処理
        /// </summary>
        /// <param name="key">キーコード</param>
        /// <param name="control">Ctrlキー</param>
        /// <param name="shift">Shiftキー</param>
        public void key2DMove(Key key, bool control, bool shift)
        {
            if (control) {
                switch (key) {
                    case Key.F1: mGlobal.mGridSize *= -1; draw(); break;    //  グリッド表示切替
                    case Key.Left: scroll(mScrollSize, 0); break;
                    case Key.Right: scroll(-mScrollSize, 0); break;
                    case Key.Up: scroll(0, mScrollSize); break;
                    case Key.Down: scroll(0, -mScrollSize); break;
                    case Key.PageUp: zoom(mGDraw.mWorld.getCenter(), 1.1); break;
                    case Key.PageDown: zoom(mGDraw.mWorld.getCenter(), 1 / 1.1); break;
                }
            } else if (shift) {
                switch (key) {
                    default: break;
                }
            } else {
                switch (key) {
                    case Key.F1: draw(true); break;                                 //  再表示
                    case Key.F2:                                                    //  領域拡大
                        mGlobal.mMainWindow.mPrevOpeMode = mGlobal.mMainWindow.mOperationMode;
                        mGlobal.mMainWindow.mOperationMode = OPEMODE.areaDisp;
                        break;
                    case Key.F3: dispFit(); break;                                  //  全体表示
                    case Key.F4: zoom(mGDraw.mWorld.getCenter(), 1.2); break;       //  拡大表示
                    case Key.F5: zoom(mGDraw.mWorld.getCenter(), 1 / 1.2); break;   //  縮小表示
                    //case Key.F6: dispWidthFit(); break;                           //  全幅表示
                    case Key.F7:                                                    //  領域ピック
                        mGlobal.mMainWindow.mPrevOpeMode = mGlobal.mMainWindow.mOperationMode;
                        mGlobal.mMainWindow.mOperationMode = OPEMODE.areaPick;
                        break;
                    default: break;
                }
            }
        }

        /// <summary>
        /// ドラッギング表示
        /// </summary>
        /// <param name="entitys">要素リスト</param>
        public void dragging(List<Entity> entitys)
        {
            if (entitys == null || entitys.Count == 0) return;
            mGDraw.clear();
            mGDraw.mBrush = Brushes.Green;
            mGDraw.mFillColor = null;
            mGDraw.mLineType = 0;
            mGDraw.mThickness = 1;
            if (mImScreen != null && mBitmapSource != null && mBitmapOn) {
                mImScreen.Source = mBitmapSource;
                mCanvas.Children.Add(mImScreen);
            } else {
                draw();
            }
            foreach(var entity in entitys)
                entity.draw2D(mGDraw, mGlobal.mFace);
        }

        /// <summary>
        /// 領域指定操作
        /// </summary>
        /// <param name="wp">領域の中心座標</param>
        /// <param name="opeMode">操作モード</param>
        /// <returns>操作結果</returns>
        public bool areaOpe(PointD pos, OPEMODE opeMode, bool onCtrl)
        {
            PointD wpos = mGDraw.cnvScreen2World(pos);

            if (mAreaLoc[0].isNaN()) {
                //  領域指定開始
                mAreaLoc[0] = wpos;
            } else {
                //  領域決定
                if (1 < mAreaLoc.Count) {
                    Box dispArea = new Box(mAreaLoc[0], mAreaLoc[1]);
                    dispArea.normalize();
                    if (1 < mGDraw.world2screenXlength(dispArea.Width)) {
                        if (opeMode == OPEMODE.areaDisp) {
                            //  領域拡大表示
                            areaDisp(dispArea);
                        } else if (opeMode == OPEMODE.areaPick) {
                            //  領域ピック
                            PointD pickPos = dispArea.getCenter();
                            if (!mLocPick.getPickNo(pickPos, dispArea, onCtrl,  true))
                                return false;
                            //  ピック色表示
                            mLocPick.setPick();
                            draw();
                            mLocPick.pickReset();
                        }
                    }
                    mAreaLoc[0] = new PointD();
                    mAreaLoc[1] = new PointD();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 領域指定の2D表示
        /// </summary>
        /// <param name="area">領域座標</param>
        public void areaDisp(Box area)
        {
            mGDraw.setWorldWindow(area);
            draw();
        }

        /// <summary>
        /// 領域枠のドラッギング
        /// </summary>
        /// <param name="loc">座標リスト</param>
        public void areaDragging(List<PointD> loc)
        {
            mCanvas.Children.Clear();
            if (mBitmapSource != null && mBitmapOn) {
                mImScreen.Source = mBitmapSource;
                mCanvas.Children.Add(mImScreen);
            } else {
                draw();
            }

            mGDraw.mBrush = Brushes.Green;
            mGDraw.mFillColor = null;
            Box b = new Box(loc[0], loc[1]);
            List<PointD> plist = b.ToPointList();
            mGDraw.drawWPolygon(plist);
        }

        /// <summary>
        /// データの表示
        /// </summary>
        /// <param name="init">初期化</param>
        /// <param name="grid">グリッド表示</param>
        /// <param name="bitmap">ビットマップ取得ビットマップ取得</param>
        public void draw(bool init = true, bool grid = true, bool bitmap = true)
        {
            if (mGlobal.mFace == FACE3D.NON || mArea == null || mArea.isNaN()) return;
            if (init) dispInit();
            draw2D(grid, bitmap);
        }

        /// <summary>
        /// 全体表示
        /// </summary>
        public void dispFit()
        {
            if (mGlobal.mFace == FACE3D.NON || mArea == null || mArea.isNaN())
                return;
            mGDraw.setWorldWindow(mArea.toBox(mGlobal.mFace));
            draw();
        }

        /// <summary>
        /// 2Dデータの表示
        /// </summary>
        /// <param name="grid">グリッド表示</param>
        /// <param name="bitmap">ビットマップ取得</param>
        public void draw2D(bool grid = true, bool bitmap = true)
        {
            if (grid)
                dispGrid(mGlobal.mGridSize);
            mLocPick.setPick();            //  ピックフラグ設定
            for (int i = 0; i < mEntityList.Count; i++) {
                if (mEntityList[i].is2DDraw(mLayer))
                    mEntityList[i].draw2D(mGDraw, mGlobal.mFace);
            }
            mLocPick.pickReset();           //  ピックフラグ解除
            if (bitmap && mCanvas != null) {
                mBitmapSource = ylib.canvas2Bitmap(mCanvas);
                mBitmapOn = true;
            } else {
                mBitmapOn = false;
            }
        }

        /// <summary>
        /// 2D表示の上下左右スクロール
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void scroll(double dx, double dy)
        {
            PointD v = new PointD(mGDraw.screen2worldXlength(dx), mGDraw.screen2worldYlength(dy));
            mGDraw.mWorld.offset(v.inverse());

            if (mImScreen == null || mBitmapSource == null || !mBitmapOn) {
                //  全体再表示
                mGDraw.mClipBox = mGDraw.mWorld;
                draw(true, true);
                return;
            }

            //  ポリゴンの塗潰しで境界線削除ためオフセットを設定
            double offset = mGDraw.screen2worldXlength(2);

            dispInit();
            mGDraw.mClipBox = mGDraw.mWorld.toCopy();

            //  横空白部分を描画
            if (0 > dx) {
                mGDraw.mClipBox.Left = mGDraw.mWorld.Right + v.x - offset;
                mGDraw.mClipBox.Width = -v.x + offset;
            } else if (0 < dx) {
                mGDraw.mClipBox.Width = v.x + offset;
            }
            if (dx != 0) {
                draw2D(true, false);
            }

            //  縦空白部分を描画
            mGDraw.mClipBox = mGDraw.mWorld.toCopy();
            if (0 > dy) {
                mGDraw.mClipBox.Top -= mGDraw.mWorld.Height - v.y - offset;
                mGDraw.mClipBox.Height = v.y + offset;
            } else if (0 < dy) {
                mGDraw.mClipBox.Height = -v.y + offset;
            }
            if (dy != 0) {
                draw2D(true, false);
            }

            //  移動した位置にBitmapの貼付け(ポリゴン塗潰しの境界線削除でoffsetを設定)
            ylib.moveImage(mCanvas, mBitmapSource, dx, dy, 1);

            //  Windowの設定を元に戻す
            mGDraw.mClipBox = mGDraw.mWorld.toCopy();
            mBitmapSource = ylib.canvas2Bitmap(mCanvas);
            mBitmapOn = true;

            //  コピーしたイメージを貼り付けなおすことで文字のクリッピングする
            //mGDraw.clear();
            //moveImage(mCanvas, mBitmapSource, 0, 0);
        }

        /// <summary>
        /// 2D表示の拡大縮小
        /// </summary>
        /// <param name="wp">拡大縮小の中心座標</param>
        /// <param name="scaleStep">拡大率</param>
        public void zoom(PointD wp, double scaleStep)
        {
            mGDraw.setWorldZoom(wp, scaleStep, true);
            mGDraw.mClipBox = mGDraw.mWorld;
            draw();
        }
        /// <summary>
        /// 2Dの画面初期化
        /// </summary>
        public void dispInit()
        {
            mGDraw.clear();
            mGDraw.mFillColor = mGlobal.mBaseBackColor;
            mGDraw.mBrush = null;
            if (mGDraw.mFillColor != null)
                mGDraw.drawRectangle(mGDraw.mView);

            mGDraw.mFillColor = null;
            mGDraw.mBrush = Brushes.Black;
        }

        /// <summary>
        /// ビットマップの取得設定
        /// </summary>
        /// <param name="on"></param>
        public void setBitmap(bool on)
        {
            mBitmapOn = on;
        }

        /// <summary>
        /// 2Dのフレームを表示
        /// </summary>
        public void drawWorldFrame()
        {
            //  背景色と枠の表示
            mGDraw.clear();
            mGDraw.mFillColor = mGlobal.mBaseBackColor;
            mGDraw.mBrush     = mGlobal.mBaseBackColor;
            //  領域再設定
            mGDraw.setViewSize(new System.Windows.Size(mCanvas.ActualWidth, mCanvas.ActualHeight));
            Box world = mGDraw.mWorld.toCopy();
            world.scale(world.getCenter(), 0.99);
            Rect rect = new Rect(world.TopLeft.toPoint(), world.BottomRight.toPoint());
            mGDraw.drawWRectangle(rect);
        }

        /// <summary>
        /// グリッドの表示
        /// グリッド10個おきに大玉を表示
        /// </summary>
        /// <param name="size">グリッドの間隔</param>
        public void dispGrid(double size)
        {
            if (0 < size && size < 1000) {
                mGDraw.mBrush = mGDraw.getColor("Black");
                mGDraw.mThickness = 1.0;
                mGDraw.mPointType = 0;
                while (mGridMinmumSize > mGDraw.world2screenXlength(size) && size < 1000) {
                    size *= 10;
                }
                if (mGridMinmumSize <= mGDraw.world2screenXlength(size)) {
                    //  グリッド間隔(mGridMinmumSize)dot以上を表示
                    double y = mGDraw.mWorld.Bottom - size;
                    y = Math.Floor(y / size) * size;
                    while (y < mGDraw.mWorld.Top) {
                        double x = mGDraw.mWorld.Left;
                        x = Math.Floor(x / size) * size;
                        while (x < mGDraw.mWorld.Right) {
                            PointD p = new PointD(x, y);
                            if (x % (size * 10) == 0 && y % (size * 10) == 0) {
                                //  10個おきの点
                                mGDraw.mPointSize = 2;
                                mGDraw.drawWPoint(p);
                            } else {
                                mGDraw.mPointSize = 1;
                                mGDraw.drawWPoint(p);
                            }
                            x += size;
                        }
                        y += size;
                    }
                }
            }
            //  原点(0,0)表示
            mGDraw.mBrush = mGDraw.getColor("Red");
            mGDraw.mPointType = 2;
            mGDraw.mPointSize = 5;
            mGDraw.drawWPoint(new PointD(0, 0));
        }

        /// <summary>
        /// ワールドウィンドウの設定
        /// </summary>
        /// <param name="viewSize">ビューサイズ</param>
        /// <param name="worldSize">ワールドサイズ</param>
        public void setWorldWindow(System.Windows.Size viewSize, double worldSize)
        {
            mGDraw.setViewSize(viewSize.Width, viewSize.Height);
            mGDraw.mAspectFix = true;
            mGDraw.mClipping = true;
            mGDraw.setWorldWindow(-worldSize * 1.1, worldSize * 1.1, worldSize * 1.1, -worldSize * 1.1);
        }

        /// <summary>
        /// 表示属性設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp)
        {
            string appName = "";
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "Area" && buf.Length == 7) {
                    mArea = new Box3D();
                    mArea.mMin.x = ylib.doubleParse(buf[1]);
                    mArea.mMin.y = ylib.doubleParse(buf[2]);
                    mArea.mMin.z = ylib.doubleParse(buf[3]);
                    mArea.mMax.x = ylib.doubleParse(buf[4]);
                    mArea.mMax.y = ylib.doubleParse(buf[5]);
                    mArea.mMax.z = ylib.doubleParse(buf[6]);
                    if (mArea.isNaN() || mArea.isEmpty())
                        mArea = new Box3D(10);
                } else if (buf[0] == "GridSize") {
                    mGlobal.mGridSize = ylib.doubleParse(buf[1]);
                    //} else if (buf[0] == "Face") {
                    //mFace = (FACE3D)Enum.Parse(typeof(FACE3D), buf[1]);
                } else if (buf[0] == "DataDrawEnd") {
                    break;
                }
            }
            return sp;
        }

        /// <summary>
        /// 表示属性保存データ
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "DataDraw" };
            list.Add(buf);
            if (mArea != null) {
                buf = new string[] { "GridSize", mGlobal.mGridSize.ToString() };
                list.Add(buf);
                //buf = new string[] { "Face", mFace.ToString() };
                //list.Add(buf);
                buf = new string[] { "Area",
                    mArea.mMin.x.ToString(), mArea.mMin.y.ToString(), mArea.mMin.z.ToString(),
                    mArea.mMax.x.ToString(), mArea.mMax.y.ToString(), mArea.mMax.z.ToString(),
                };
                list.Add(buf);
            }
            buf = new string[] { "DataDrawEnd" };
            list.Add(buf);
            return list;
        }


        /// <summary>
        /// 画面コピー
        /// </summary>
        public void screenCopy()
        {
            BitmapSource bitmapSource = toBitmapScreen();
            System.Windows.Clipboard.SetImage(bitmapSource);
        }

        /// <summary>
        /// 作図領域のコピー
        /// </summary>
        /// <returns>BitmapSource</returns>
        public BitmapSource toBitmapScreen()
        {
            Brush tmpColor = mBaseBackColor;
            mBaseBackColor = Brushes.White;
            draw(true, false, false);
            BitmapSource bitmapSource = ylib.canvas2Bitmap(mCanvas);
            mBaseBackColor = tmpColor;
            return bitmapSource;
        }
    }
}
