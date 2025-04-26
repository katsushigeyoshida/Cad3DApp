using CoreLib;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Cad3DApp
{
    /// <summary>
    /// グローバルデータクラス
    /// </summary>
    public class GlobalData
    {
        public Brush mEntityBrush = Brushes.Green;              //  Entityの色設定
        public Brush mLineColor = Brushes.Black;                //  線の色
        public Brush mFaceColor = Brushes.Blue;                 //  面の色
        public double mLineThickness = 1.0;                     //  線の太さ
        public int mLineType = 0;                               //  線種(0:solid 1:dash 2:center 3:phantom)
        public int mLayerSize = 64;                             //  レイヤサイズ(8byte)
        public double mDivAngle = Math.PI / 12;                 //  円弧分割角度
        public double mFilletSize = 0;                          //  R面取り(フィレット)半径
        public double mGridSize = 1;                            //  グリッドのサイズ
        public FACE3D mFace = FACE3D.FRONT;                     //  表示面
        public FACE3D mPreFace = FACE3D.FRONT;                  //  切り替える前の表示面
        public int mOperationCount = 0;                         //  コマンド実行数
        public string mZumenComment = "";                       //  図面コメント
        public string mMemo = "";                               //  メモ
        public bool mWireFrame = false;                         //  ワイヤーフレーム表示

        public Box3D mCopyArea;
        public List<Entity> mCopyEntities;

        public Brush mBaseBackColor = Brushes.White;            //  2D背景色
        public string mBaseDataFolder = "3DZumen";
        public string mBackupFolder = "3DZumen";
        public MainWindow mMainWindow;
    }

    /// <summary>
    /// データ管理クラス
    /// </summary>
    public class DataManage
    {
        private string mVersion = "0";
        private string mRevsion = "1";
        public string mDataPath = "zumenData.csv";

        public OPEMODE mOperationMode = OPEMODE.pick;           //  操作モード

        public List<Entity> mEntityList = new List<Entity>();   //  要素リスト
        public OPERATION mOperation = OPERATION.non;            //  処理コマンド

        public Layer mLayer;                                    //  レイヤ
        public ChkListDialog mLayerChkListDlg = null;           //  表示レイヤー設定ダイヤログ
        public Group mGroup;                                    //  グループ
        public DataDraw mDataDraw;                              //  データ表示
        public GlobalData mGlobal = new GlobalData();           //  グローバル変数

        private EditEntity mEditEntity;                         //  データ要素の変更
        private CreateEntity mCreateEntity;                     //  データ要素の作成
        private LockPick mLocPick;                              //  ロケイト・ピック処理
        private CommandOpe mCommandOpe;                         //  コマンド処理
        private double mPickSize = 10;                          //  ピック領域の大きさ
        private int mFirstEntityCount = 0;                      //  編集開始時の要素数

        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainWindow">メインウィンドウ</param>
        /// <param name="face">作成面</param>
        public DataManage(MainWindow mainWindow, FACE3D face = FACE3D.FRONT)
        {
            mGlobal.mMainWindow = mainWindow;
            mLocPick = new LockPick(mainWindow, mEntityList, face);
            mCommandOpe = new CommandOpe(mEntityList, mGlobal);
            mCreateEntity = new CreateEntity(mGlobal);
            mEditEntity = new EditEntity(mGlobal, mEntityList);
            mLayer = new Layer(mGlobal.mLayerSize);
            mGroup = new Group();
            mLocPick.mGroup = mGroup;
            mLocPick.mLayer = mLayer;
            mCommandOpe.mLayer = mLayer;
            mCommandOpe.mGroup = mGroup;
            mDataDraw = new DataDraw(mGlobal);
            mDataDraw.mLocPick = mLocPick;
            mDataDraw.mLayer = mLayer;
            mDataDraw.mEntityList = mEntityList;
        }

        /// <summary>
        /// FileDataの設定
        /// </summary>
        /// <param name="fileData">FileData</param>
        public void setFileData(FileData fileData)
        {
            mCommandOpe.mFileData = fileData;
        }

        /// <summary>
        /// DataDraw の設定
        /// </summary>
        /// <param name="canvas">キャンバス</param>
        /// <param name="imScreen">イメージ</param>
        public void setDataDraw(Canvas canvas, System.Windows.Controls.Image imScreen)
        {
            mDataDraw.setWindowDraw(canvas, imScreen);
        }

        /// <summary>
        /// 全データをクリアする
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="imScreen">ImageScreen</param>
        public void clear(Canvas canvas, System.Windows.Controls.Image imScreen)
        {
            mDataDraw.setWindowDraw(canvas, imScreen);
            clear();
        }


        /// <summary>
        /// 全データをクリアする
        /// </summary>
        public void clear()
        {
            mEntityList.Clear();
            mLocPick.clear();
            mLayer.clear();
            mGroup.mGroupList.Clear();
            mGlobal.mOperationCount = 0;
            mGlobal.mZumenComment = "";
            mGlobal.mMemo = "";
            mFirstEntityCount = 0;
        }

        /// <summary>
        /// 作成面の設定
        /// </summary>
        /// <param name="canvas">キャンバス</param>
        /// <param name="imScreen">イメージ</param>
        public void setCanvas(Canvas canvas, System.Windows.Controls.Image imScreen)
        {
            mDataDraw.setCanvas(canvas, imScreen);
            mLocPick.setCanvasFace(mGlobal.mFace);
        }

        /// <summary>
        /// 基準面でのロケイト位置設定
        /// </summary>
        /// <param name="baseLoc">基準面</param>
        public void setBaseLoc(bool baseLoc)
        {
            mLocPick.mBaseLoc = baseLoc;
        }

        /// <summary>
        /// コマンドの実行
        /// </summary>
        /// <param name="ope">コマンド完了</param>
        public bool commandExec(OPERATION ope)
        {
            if (ope != OPERATION.close &&
                (ope == OPERATION.back || ope == OPERATION.cancel)) {
                commandClear();
                return true;
            }
            mOperation = ope;
            mOperationMode = mCommandOpe.execCommand(ope, mLocPick.mPickEntity);
            if (mOperationMode == OPEMODE.clear) {
                commandClear();
                if (ope == OPERATION.changeProperty)
                    layerDlgUpdate();   //  表示レイヤーダイヤログの更新
            } else if (mOperationMode == OPEMODE.reload) {
                return false;
            } else if (mOperationMode == OPEMODE.updateData) {
                updateData();           //  データ再作成
                commandClear();
            } else if (mOperationMode == OPEMODE.exec) {
                execCommand(ope);       //  ロケイトを必要としないコマンドの実行
                commandClear();
            } else
                return false;
            return true;
        }

        /// <summary>
        /// レイヤ設定ダイヤログのデータ更新
        /// </summary>
        public void layerDlgUpdate()
        {
            if (mLayerChkListDlg != null && mLayerChkListDlg.IsVisible) {
                setDispLayer();
            }
        }

        /// <summary>
        /// 要素の作成
        /// </summary>
        /// <param name="lastLoc">最終ロケイト</param>
        /// <param name="arc">円弧ストレッチ</param>
        /// <returns>要素の可否</returns>
        public bool definData(bool lastLoc = false, bool arc = false)
        {
            List<Entity> entityList = new List<Entity>();
            bool copy = false;
 
            switch (mOperation) {
                case OPERATION.line:
                    if (mLocPick.mLocList.Count == 2) {
                        entityList.Add(mCreateEntity.createLine(mLocPick.mLocList[0], mLocPick.mLocList[1], true));
                    }
                    break;
                case OPERATION.arc:
                    if (mLocPick.mLocList.Count == 3) {
                        entityList.Add(mCreateEntity.createArc(mLocPick.mLocList[0], mLocPick.mLocList[2], mLocPick.mLocList[1], true));
                    }
                    break;
                case OPERATION.circle:
                    if (mLocPick.mLocList.Count == 2) {
                        entityList.Add(mCreateEntity.createArc(mLocPick.mLocList[0], mLocPick.mLocList[1], 0, Math.PI * 2, mGlobal.mFace, true));
                    }
                    break;
                case OPERATION.polyline:
                    if (lastLoc && 2 <mLocPick.mLocList.Count) {
                        entityList.Add(mCreateEntity.createPolyline(mLocPick.mLocList, mGlobal.mFace, true));
                    }
                    break;
                case OPERATION.polygon:
                    if (lastLoc && 2 < mLocPick.mLocList.Count) {
                        entityList.Add(mCreateEntity.createPolygon(mLocPick.mLocList, mGlobal.mFace, true));
                    }
                    break;
                case OPERATION.rect:
                    if (mLocPick.mLocList.Count == 2) {
                        entityList.Add(mCreateEntity.createRect(mLocPick.mLocList[0], mLocPick.mLocList[1], mGlobal.mFace, true));
                    }
                    break;
                case OPERATION.translate:
                case OPERATION.copyTranslate:
                    if (1 <  mLocPick.mLocList.Count) {
                        copy = mOperation == OPERATION.copyTranslate;
                        entityList = mEditEntity.translate(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], true);
                    }
                    break;
                case OPERATION.rotate:
                case OPERATION.copyRotate:
                    if (2 < mLocPick.mLocList.Count) {
                        copy = mOperation == OPERATION.copyRotate;
                        entityList = mEditEntity.rotate(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], mLocPick.mLocList[2], mGlobal.mFace, true);
                    }
                    break;
                case OPERATION.offset:
                case OPERATION.copyOffset:
                    if (1 < mLocPick.mLocList.Count) {
                        copy = mOperation == OPERATION.copyOffset;
                        entityList = mEditEntity.offset(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], mGlobal.mFace, true);
                    }
                    break;
                case OPERATION.mirror:
                case OPERATION.copyMirror:
                    if (1 < mLocPick.mLocList.Count) {
                        copy = mOperation == OPERATION.copyMirror;
                        entityList = mEditEntity.mirror(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], mGlobal.mFace, true);
                    }
                    break;
                case OPERATION.trim:
                case OPERATION.copyTrim:
                    if (1 < mLocPick.mLocList.Count) {
                        copy = mOperation == OPERATION.copyTrim;
                        entityList = mEditEntity.trim(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], mGlobal.mFace, true);
                    }
                    break;
                case OPERATION.scale:
                case OPERATION.copyScale:
                    if (2 < mLocPick.mLocList.Count) {
                        copy = mOperation == OPERATION.copyScale;
                        entityList = mEditEntity.scale(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], mLocPick.mLocList[2], mGlobal.mFace, true);
                    }
                    break;
                case OPERATION.stretch:
                    if (1 < mLocPick.mLocList.Count)
                        entityList = mEditEntity.stretch(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], arc, mGlobal.mFace, true);
                    break;
                case OPERATION.divide:
                    entityList = mEditEntity.divide(mLocPick.mPickEntity, mLocPick.mLocList[0], mGlobal.mFace);
                    break;
                case OPERATION.extrusion:
                    if (1 < mLocPick.mLocList.Count)
                        entityList = mEditEntity.extrusion(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], true);
                    break;
                case OPERATION.measureDistance:
                    if (mLocPick.mLocList.Count == 2) {
                        measure(mLocPick.mLocList);
                        commandClear();
                        return true;
                    }
                    break;
                case OPERATION.measureAngle:
                    if (mLocPick.mLocList.Count == 3) {
                        measure(mLocPick.mLocList);
                        commandClear();
                        return true;
                    }
                    break;
                case OPERATION.pasteEntity:
                    if (mLocPick.mLocList.Count == 1 && mGlobal.mCopyArea != null) {
                        entityList.AddRange(pasteEntity(mLocPick.mLocList[0]));
                    }
                    break;
            }
            if (entityList.Count == 0) return false;
            foreach (var entity in entityList)
                mEditEntity.addEntity(entity, mGlobal.mOperationCount);
            if (!copy) {
                foreach (var pickEnt in mLocPick.mPickEntity)
                    mEditEntity.addLink(pickEnt.mEntityNo, mGlobal.mLayerSize, mGlobal.mOperationCount);
            }
            updateArea();
            commandClear();
            return true;
        }

        /// <summary>
        /// ドラッギング表示
        /// </summary>
        /// <param name="pos">最終ロケイト(スクリーン座標)</param>
        /// <param name="arc">円弧ストレッチ</param>
        /// <returns>最終ロケイト座標</returns>
        public PointD dragging(PointD pos, bool arc)
        {
            PointD wpos = mDataDraw.mGDraw.cnvScreen2World(pos);
            Point3D lastPos = new Point3D(wpos, mGlobal.mFace);
            List<Entity> entityList = new List<Entity>();
            if (mLocPick.mLocList.Count == 0 && mOperation != OPERATION.pasteEntity) return wpos;

            switch (mOperation) {
                case OPERATION.line:
                    entityList.Add(mCreateEntity.createLine(mLocPick.mLocList[0], lastPos));
                    break;
                case OPERATION.arc:
                    if (1 < mLocPick.mLocList.Count) {
                        entityList.Add(mCreateEntity.createArc(mLocPick.mLocList[0], lastPos, mLocPick.mLocList[1]));
                    }
                    break;
                case OPERATION.circle:
                    entityList.Add(mCreateEntity.createArc(mLocPick.mLocList[0], lastPos, 0, Math.PI * 2, mGlobal.mFace));
                    break;
                case OPERATION.polyline:
                    if (1 < mLocPick.mLocList.Count) {
                        mLocPick.mLocList.Add(lastPos);
                        entityList.Add(mCreateEntity.createPolyline(mLocPick.mLocList, mGlobal.mFace));
                        mLocPick.mLocList.RemoveAt(mLocPick.mLocList.Count - 1);
                    }
                    break;
                case OPERATION.polygon:
                    if (1 < mLocPick.mLocList.Count) {
                        mLocPick.mLocList.Add(lastPos);
                        entityList.Add(mCreateEntity.createPolygon(mLocPick.mLocList, mGlobal.mFace));
                        mLocPick.mLocList.RemoveAt(mLocPick.mLocList.Count - 1);
                    }
                    break;
                case OPERATION.rect:
                    entityList.Add(mCreateEntity.createRect(mLocPick.mLocList[0], new Point3D(wpos, mGlobal.mFace), mGlobal.mFace));
                    break;
                case OPERATION.translate:
                case OPERATION.copyTranslate:
                    entityList = mEditEntity.translate(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], lastPos, false);
                    break;
                case OPERATION.rotate:
                case OPERATION.copyRotate:
                    if (1 < mLocPick.mLocList.Count)
                        entityList = mEditEntity.rotate(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], lastPos, mGlobal.mFace, false);
                    break;
                case OPERATION.offset:
                case OPERATION.copyOffset:
                    entityList = mEditEntity.offset(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], lastPos, mGlobal.mFace, false);
                    break;
                case OPERATION.mirror:
                case OPERATION.copyMirror:
                    entityList = mEditEntity.mirror(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], lastPos, mGlobal.mFace, false);
                    break;
                case OPERATION.trim:
                case OPERATION.copyTrim:
                    entityList = mEditEntity.trim(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], lastPos, mGlobal.mFace, false);
                    break;
                case OPERATION.scale:
                case OPERATION.copyScale:
                    if (1 < mLocPick.mLocList.Count)
                        entityList = mEditEntity.scale(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], mLocPick.mLocList[1], lastPos, mGlobal.mFace, false);
                    break;
                case OPERATION.stretch:
                    entityList = mEditEntity.stretch(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], lastPos, arc, mGlobal.mFace, false);
                    break;
                case OPERATION.extrusion:
                    entityList = mEditEntity.extrusion(mLocPick.mPickEntity,
                            mLocPick.mLocList[0], lastPos, false);
                    break;
                case OPERATION.pasteEntity:
                    if (mGlobal.mCopyArea != null) {
                        Point3D v = lastPos - mGlobal.mCopyArea.mMin;
                        Point3D maxPos = mGlobal.mCopyArea.mMax + v;
                        entityList.Add(mCreateEntity.createRect(lastPos, maxPos, mGlobal.mFace));
                    }
                    break;
            }
            if (mLocPick.mLocList.Count == 1)
                mDataDraw.setBitmap(false);
            if (entityList.Count == 0) {
                //  要素作成条件を満たしていない時
                mLocPick.mLocList.Add(lastPos);
                entityList.Add(mCreateEntity.createPolyline(mLocPick.mLocList, mGlobal.mFace));
                entityList.Last().mLineColor = Brushes.Aqua;
                mLocPick.mLocList.RemoveAt(mLocPick.mLocList.Count - 1);
            }
            mDataDraw.dragging(entityList);
            return wpos;
        }

        /// <summary>
        /// ロケイトを必要としないコマンドでダイヤログ表示の必要なコマンドの実行
        /// </summary>
        /// <param name="ope">コマンド</param>
        public void execCommand(OPERATION ope)
        {
            switch (ope) {
                case OPERATION.dispLayer: setDispLayer(); break;
                case OPERATION.save: saveFile(mDataPath); break;
                case OPERATION.setColor: setColor(); break;
                case OPERATION.setGrid: setGrid(); break;
            }
        }

        /// <summary>
        /// コピー要素を指定位置の要素データに変換
        /// </summary>
        /// <param name="loc">指定位置</param>
        /// <returns>要素データリスト</returns>
        public List<Entity> pasteEntity(Point3D loc)
        {
            List<Entity> enityList = new List<Entity>();
            Point3D v = loc - mGlobal.mCopyArea.mMin;
            for (int i = 0; i < mGlobal.mCopyEntities.Count; i++) {
                Entity entity = mGlobal.mCopyEntities[i];
                entity.translate(v);
                entity.createSurfaceData();
                entity.createVertexData();
                enityList.Add(entity);
            }
            return enityList;
        }

        /// <summary>
        /// 計測
        /// </summary>
        /// <param name="locList">ロケイトリスト</param>
        public void measure(List<Point3D> locList)
        {
            if (locList.Count == 2) {
                string buf = "距離 : " + ylib.double2StrZeroSup(locList[0].length(locList[1]), "F8");
                ylib.messageBox(mGlobal.mMainWindow, buf, "", "計測");
            } else if (locList.Count == 3) {
                string buf = "角度 : " + ylib.double2StrZeroSup(ylib.R2D(locList[1].angle(locList[0], locList[2])), "F8");
                ylib.messageBox(mGlobal.mMainWindow, buf, "", "計測");
            }
        }

        /// <summary>
        /// 要素データからSurfaceDataを取得
        /// </summary>
        /// <returns>SurfaceDataリスト</returns>
        public List<SurfaceData> getSurfaceData()
        {
            List<SurfaceData> surfaveDataList = new List<SurfaceData>();
            for (int i = 0; i < mEntityList.Count; i++) {
                if (mEntityList[i].is3DDraw(mLayer)) {
                    if (mGlobal.mWireFrame)
                        surfaveDataList.AddRange(mEntityList[i].toWireFrame());
                    else
                        surfaveDataList.AddRange(mEntityList[i].mSurfaceDataList);
                }
            }
            return surfaveDataList;
        }

        /// <summary>
        /// コマンドクリア
        /// </summary>
        public void commandClear()
        {
            mOperation = OPERATION.non;
            updateArea();
            mDataDraw.setBitmap(false);
            mLocPick.clear();
            mDataDraw.draw();
        }

        /// <summary>
        /// 要素表示
        /// </summary>
        /// <param name="fit">全体表示</param>
        /// <param name="bitmap">Bitmap使用</param>
        public void draw(bool fit = false, bool bitmap = true)
        {
            if (fit)
                mDataDraw.dispFit();
            else
                mDataDraw.draw(true, true, bitmap);
        }

        /// <summary>
        /// 要素表示領域の更新
        /// </summary>
        public void updateArea()
        {
            mDataDraw.mArea = firstArea();
            //  領域設定
            for (int i = 0; i < mEntityList.Count; i++) {
                if (!mEntityList[i].mRemove && mEntityList[i].mID != EntityId.Link)
                    mDataDraw.mArea.extension(mEntityList[i].mArea);
            }
            if (mDataDraw.mArea.isNaN() || mDataDraw.mArea.isEmpty())
                mDataDraw.mArea = new Box3D(10);
        }

        /// <summary>
        /// 最初の有効要素の領域取得
        /// </summary>
        /// <returns>領域</returns>
        private Box3D firstArea()
        {
            Box3D area = new Box3D();
            //  初期値
            for (int i = 0; i < mEntityList.Count; i++) {
                if (!mEntityList[i].mRemove && mEntityList[i].mID != EntityId.Link) {
                   area = mEntityList[i].mArea.toCopy();
                    break;
                }
            }
            return area;
        }

        /// <summary>
        /// 全要素の2D・3Dデータの再作成
        /// </summary>
        public void updateData()
        {
            if (mEntityList == null) return;
            mDataDraw.mArea = firstArea();
            for (int i = 0; i < mEntityList.Count; i++) {
                if (!mEntityList[i].mRemove && mEntityList[i].mID != EntityId.Link) {
                    mEntityList[i].createVertexData();
                    mEntityList[i].createSurfaceData();
                    mDataDraw.mArea.extension(mEntityList[i].mArea);
                }
            }
        }


        /// <summary>
        /// 要素作成の色設定
        /// </summary>
        public void setColor()
        {
            mGlobal.mEntityBrush = ylib.mBrushList[mGlobal.mMainWindow.cbColor.SelectedIndex].brush;
            mGlobal.mLineColor = ylib.mBrushList[mGlobal.mMainWindow.cbColor.SelectedIndex].brush;
            mGlobal.mFaceColor = ylib.mBrushList[mGlobal.mMainWindow.cbColor.SelectedIndex].brush;
        }

        /// <summary>
        /// グリッド幅の設定
        /// </summary>
        public void setGrid()
        {
            mGlobal.mGridSize = mGlobal.mMainWindow.mGridSizeMenu[mGlobal.mMainWindow.cbGridSize.SelectedIndex];
            mDataDraw.draw();
        }

        /// <summary>
        /// 表示レイヤの設定
        /// </summary>
        public void setDispLayer()
        {
            if (mLayerChkListDlg != null)
                mLayerChkListDlg.Close();
            mLayerChkListDlg = new ChkListDialog();
            mLayerChkListDlg.Topmost = true;
            mLayerChkListDlg.mTitle = "表示レイヤー";
            mLayerChkListDlg.mAddMenuEnable = true;
            mLayerChkListDlg.mEditMenuEnable = true;
            mLayerChkListDlg.mDeleteMenuEnable = true;
            mLayerChkListDlg.mLayerAllEnable = true;
            mLayerChkListDlg.mChkList = mLayer.getLayerChkList();
            mLayerChkListDlg.mLayerAll = mLayer.mLayerAll;
            mLayerChkListDlg.mCallBackOn = true;
            mLayerChkListDlg.callback = setLayerChk;
            mLayerChkListDlg.callbackRename = layerRename;
            mLayerChkListDlg.Show();
            mGlobal.mOperationCount++;
        }

        /// <summary>
        /// レイヤーチェックリストに表示を更新(コールバック)
        /// </summary>
        public void setLayerChk()
        {
            mLayer.setLayerChkList(mLayerChkListDlg.mChkList);
            mLayer.mLayerAll = mLayerChkListDlg.mLayerAll;
            if (mGlobal.mFace == FACE3D.NON)
                mGlobal.mMainWindow.renderFrame();
            else
                mDataDraw.draw(true);
            //mMainWindow.dispTitle();
        }

        /// <summary>
        /// レイヤ名の変更(コールバック)
        /// </summary>
        public void layerRename()
        {
            mLayer.rename(mLayerChkListDlg.mSrcName, mLayerChkListDlg.mDestName);
            setDispLayer();
        }

        /// <summary>
        /// ワールド座標の領域を設定
        /// </summary>
        public void worldFrame()
        {
            mDataDraw.drawWorldFrame();
        }

        /// <summary>
        /// ピック処理
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <returns>ピックの有無</returns>
        public bool pick(PointD pos)
        {
            if (mGlobal.mFace == FACE3D.NON) return false;
            return mLocPick.getPickNo(getWpos(pos), pickBox(pos));
        }

        /// <summary>
        /// ロケイト時のピック
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <returns>ピックの有無</returns>
        public bool locPick(PointD pos)
        {
            if (mGlobal.mFace == FACE3D.NON) return false;
            return mLocPick.getLocPickNo(getWpos(pos), pickBox(pos));
        }

        /// <summary>
        /// ロケイト処理
        /// </summary>
        /// <param name="pos">ロケイト位置</param>
        public void locate(PointD pos)
        {
            PointD wpos = mDataDraw.mGDraw.cnvScreen2World(pos);
            if (0 < mGlobal.mGridSize)
                wpos.round(Math.Abs(mGlobal.mGridSize));
            mLocPick.mLocList.Add(new Point3D(wpos, mGlobal.mFace));
        }

        /// <summary>
        /// オートロケイト処理
        /// </summary>
        /// <param name="pos">ロケイト位置</param>
        public bool autoLocate(PointD pos)
        {
            PointD wpos = mDataDraw.mGDraw.cnvScreen2World(pos);
            return mLocPick.autoLoc(wpos);
        }

        /// <summary>
        /// ロケイトメニュー処理
        /// </summary>
        /// <returns>選択の可否</returns>
        public bool locMenu()
        {
            if (mOperationMode == OPEMODE.loc) {
                mLocPick.locMenu(mOperation);
                return true;
            }
            return false;
        }

        /// <summary>
        /// グループピックメニュー
        /// </summary>
        /// <returns>選択の可否</returns>
        public bool groupPickMenu()
        {
            if (mOperationMode != OPEMODE.loc) {
                return mLocPick.groupSelectPick(new PointD(), mGlobal.mFace); 
            }
            return false;
        }

        /// <summary>
        /// ロケイト数の取得
        /// </summary>
        /// <returns>ロケイト数</returns>
        public int getLocCount()
        {
            return mLocPick.mLocList.Count;
        }

        /// <summary>
        /// ピック要素数の取得
        /// </summary>
        /// <returns>ピック数</returns>
        public int getPickCount()
        {
            return mLocPick.mPickEntity.Count;
        }


        /// <summary>
        /// 最後のロケイトを削除する
        /// </summary>
        public void backLoc()
        {
            if (0 < mLocPick.mLocList.Count)
                mLocPick.mLocList.RemoveAt(mLocPick.mLocList.Count - 1);
        }

        /// <summary>
        /// 画面スクロール
        /// </summary>
        /// <param name="pos">マウス位置</param>
        /// <param name="prePos">前回マウス位置</param>
        /// <returns>マウス位置</returns>
        public PointD scroll(PointD pos, PointD prePos)
        {
            PointD wpos = mDataDraw.mGDraw.cnvScreen2World(pos);
            mDataDraw.scroll(pos.x - prePos.x, pos.y - prePos.y);
            return wpos;
        }

        /// <summary>
        /// 拡大・縮小
        /// </summary>
        /// <param name="pos">マウス位置</param>
        /// <param name="scaleStep">拡大率</param>
        /// <returns>マウス位置</returns>
        public PointD zoom(PointD pos, double scaleStep)
        {
            PointD wp = mDataDraw.mGDraw.cnvScreen2World(new PointD(pos));
            mDataDraw.zoom(wp, scaleStep);
            return wp;
        }

        /// <summary>
        /// 表示・操作面の設定
        /// </summary>
        /// <param name="face"></param>
        public void setFace(FACE3D face)
        {
            mGlobal.mFace = face;
            mDataDraw.setBitmap(false);
        }

        /// <summary>
        /// グリッドサイズの取得
        /// </summary>
        /// <returns>グリッドサイズ</returns>
        public double getGridSize()
        {
            return mGlobal.mGridSize;
        }

        /// <summary>
        /// グリッドサイズを設定
        /// </summary>
        /// <param name="size"></param>
        public void setGridSize(double size)
        {
            mGlobal.mGridSize = size;
        }

        /// <summary>
        /// 領域表示/選択
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="opeMode"></param>
        /// <returns></returns>
        public PointD areaDragging(PointD pos, OPEMODE opeMode)
        {
            PointD wpos = mDataDraw.mGDraw.cnvScreen2World(pos);
            mDataDraw.mAreaLoc[1] = wpos;
            mDataDraw.areaDragging(mDataDraw.mAreaLoc);

            return wpos;
        }

        /// <summary>
        /// ピックボックスの大きさ
        /// </summary>
        /// <returns></returns>
        public double pickBoxSize()
        {
            return mDataDraw.mGDraw.screen2worldXlength(mPickSize);
        }

        /// <summary>
        /// ピックボックスの領域
        /// </summary>
        /// <param name="pos">中心座標(スクリーン座標)</param>
        /// <returns></returns>
        public Box pickBox(PointD pos)
        {
            return new Box(getWpos(pos), pickBoxSize());
        }

        /// <summary>
        /// ワールド座標に変換
        /// </summary>
        /// <param name="pos">スクリーン座標</param>
        /// <returns>ワールド座標</returns>
        public PointD getWpos(PointD pos)
        {
            return mDataDraw.mGDraw.cnvScreen2World(pos);
        }

        /// <summary>
        /// 表示要素数の取得
        /// </summary>
        /// <returns>要素数</returns>
        public int getDispEntCount()
        {
            int count = 0;
            for (int i = 0; i < mEntityList.Count; i++) {
                if (mEntityList[i].is2DDraw(mLayer))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 画面コピー
        /// </summary>
        public void screenCopy()
        {
            mDataDraw.screenCopy();
        }

        /// <summary>
        /// 図面属性を設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp)
        {
            string appName = "";
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "AppName") {
                    appName = buf[1];
                } else if (buf[0] == "EntityBrush") {
                    mGlobal.mEntityBrush = ylib.getBrsh(buf[1]);
                } else if (buf[0] == "ArcDivideAngle") {
                    //mArcDivideAng = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "RevolutionDivideAngle") {
                    //mRevolutionDivideAng = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "SweepDivideAngle") {
                    //mSweepDivideAng = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "SurfaceVertex") {
                    //mSurfaceVertex = ylib.boolParse(buf[1]);
                } else if (buf[0] == "WireFrame") {
                    //mWireFrame = ylib.boolParse(buf[1]);
                } else if (buf[0] == "ZumenComment") {
                    mGlobal.mZumenComment = ylib.strControlCodeRev(buf[1]);
                } else if (buf[0] == "Memo") {
                    mGlobal.mMemo = ylib.strControlCodeRev(buf[1]);
                } else if (buf[0] == "Layer") {
                    sp = mLayer.setDataList(dataList, sp);
                } else if (buf[0] == "Group") {
                    sp = mGroup.setDataList(dataList, sp);
                } else if (buf[0] == "DataManageEnd") {
                    break;
                }
            }
            if (appName == mGlobal.mMainWindow.mAppName || appName == "Mini3DCad" || appName == "")
                return sp;
            return -1;
        }

        /// <summary>
        /// 図面属性を文字列配列リストに変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "DataManage" };
            list.Add(buf);
            buf = new string[] { "EntityBrush", ylib.getBrushName(mGlobal.mEntityBrush) };
            list.Add(buf);
            //buf = new string[] { "ArcDivideAngle", mArcDivideAng.ToString() };
            //list.Add(buf);
            //buf = new string[] { "RevolutionDivideAngle", mRevolutionDivideAng.ToString() };
            //list.Add(buf);
            //buf = new string[] { "SweepDivideAngle", mSweepDivideAng.ToString() };
            //list.Add(buf);
            //buf = new string[] { "SurfaceVertex", mSurfaceVertex.ToString() };
            //list.Add(buf);
            //buf = new string[] { "WireFrame", mWireFrame.ToString() };
            //list.Add(buf);
            buf = new string[] { "ZumenComment", ylib.strControlCodeCnv(mGlobal.mZumenComment) };
            list.Add(buf);
            buf = new string[] { "Memo", ylib.strControlCodeCnv(mGlobal.mMemo) };
            list.Add(buf);
            list.AddRange(mLayer.toDataList());
            mGroup.squeeze(mEntityList);
            list.AddRange(mGroup.toDataList());
            buf = new string[] { "DataManageEnd" };
            list.Add(buf);
            return list;
        }

        /// <summary>
        /// Mini3DCadデータからの要素取得
        /// </summary>
        /// <param name="dataList">文字列リスト</param>
        /// <param name="sp">データ位置</param>
        /// <returns>データ位置</returns>
        public int setElementDataList(List<string[]> dataList, int sp)
        {
            int layerSize = 8;
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                try {
                    if (buf[0] == "PrimitiveId") {
                        (sp, Entity? entity) = mCreateEntity.getMini3DCadData(buf, dataList, sp);
                        if (entity != null) {
                            mEntityList.Add(entity);
                            mEntityList.Last().createVertexData();
                            mEntityList.Last().createSurfaceData();
                            mEntityList.Last().setArea();
                        }
                    } else if (buf[0] == "Name") {
                        //mName = buf[1];
                    } else if (buf[0] == "IsShading") {
                        //mBothShading = ylib.boolParse(buf[1]);
                    } else if (buf[0] == "Disp3D") {
                        mEntityList.Last().mDisp3D = ylib.boolParse(buf[1]);
                    } else if (buf[0] == "Group") {
                        mEntityList.Last().mGroup = ylib.intParse(buf[1]);
                    } else if (buf[0] == "LayerSize") {
                        layerSize = ylib.intParse(buf[1]);
                    } else if (buf[0] == "DispLayerBit") {
                        byte[] layerBit = new byte[layerSize];
                        for (int i = 0; i < layerBit.Length && i < buf.Length - 1; i++) {
                            layerBit[i] = byte.Parse(buf[i + 1], NumberStyles.HexNumber);
                        }
                        mEntityList.Last().mLayerBit = layerBit;
                    } else if (buf[0] == "ElementEnd") {
                        break;
                    }
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine($"setElementDataList:{sp} {e.Message}");
                    if (0 < mEntityList.Count &&
                        (mEntityList[mEntityList.Count - 1].mVertexList == null ||
                        mEntityList[mEntityList.Count - 1].mSurfaceDataList == null ||
                        mEntityList[mEntityList.Count - 1].mArea == null))
                        mEntityList[mEntityList.Count - 1].mRemove = true;
                }
            }
            return sp;
        }

        /// <summary>
        /// 文字列リストから要素にデータを設定する
        /// </summary>
        /// <param name="dataList">文字列リスト</param>
        /// <param name="sp">データ位置</param>
        /// <returns>データ位置</returns>
        public int setEntityDataList(List<string[]> dataList, int sp)
        {
            while (sp < dataList.Count) {
                try {
                    string[] buf = dataList[sp++];
                    if (buf[0] == "EntityEnd") break;
                    if (buf[0] != "ID") continue;
                    (sp, Entity? entity) = mCreateEntity.dataList2Entity(buf[1], dataList, sp);
                    if (entity != null) {
                        mEntityList.Add(entity);
                        mEntityList.Last().createVertexData();
                        mEntityList.Last().createSurfaceData();
                        mEntityList.Last().setArea();
                    }
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine($"setEntityDataList:{sp} {e.Message}");
                    if (0 < mEntityList.Count && 
                        (mEntityList.Last().mVertexList == null ||
                        mEntityList.Last().mSurfaceDataList == null ||
                        mEntityList.Last().mArea == null))
                        mEntityList.Last().mRemove = true;
                }
            }
            return sp;
        }

        /// <summary>
        /// 要素データを文字列リストに変換
        /// </summary>
        /// <returns>文字列リスト</returns>
        public List<string[]> toEntityDataList()
        {
            List<string[]> dataList = new List<string[]>();
            dataList.Add(new string[] { "Entity" });
            for (int i = 0; i < mEntityList.Count; i++) {
                if (!mEntityList[i].mRemove && mEntityList[i].mID != EntityId.Link) {
                    dataList.AddRange(mEntityList[i].toPropertyList());
                    dataList.AddRange(mEntityList[i].toDataList());
                }
            }
            dataList.Add(new string[] { "EntityEnd" });

            return dataList;
        }

        /// <summary>
        /// ファイルからデータを読み込む
        /// </summary>
        public void loadFile()
        {
            loadFile(mDataPath);
        }

        /// <summary>
        /// ファイルからデータを読み込む
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void loadFile(string path)
        {
            if (!File.Exists(path))
                return;
            List<string[]> dataList = ylib.loadCsvData(path);
            if (2 > dataList.Count) return;
            clear();
            int sp = 0;
            if (dataList[0][0] == "DataManage" && dataList[1][0] == "AppName" && dataList[1][1] == "Mini3DCad") {
                //  Mini3DCadデータ
                sp = 0;
                while (0 <= sp && sp < dataList.Count) {
                    string[] buf = dataList[sp++];
                    if (buf[0] == "Element") {
                        sp = setElementDataList(dataList, sp);
                    } else if (buf[0] == "DataManage") {
                        sp = setDataList(dataList, sp);
                    } else if (buf[0] == "DataDraw") {
                        sp = mDataDraw.setDataList(dataList, sp);
                    }
                }

            } else if ((dataList[0][0] == "AppName" && dataList[0][1] == mGlobal.mMainWindow.mAppName)) {
                //  Cad3DApp
                sp = 1;
                while (0 <= sp && sp < dataList.Count) {
                    string[] buf = dataList[sp++];
                    if (buf[0] == "Entity") {
                        sp = setEntityDataList(dataList, sp);
                    } else if (buf[0] == "DataManage") {
                        sp = setDataList(dataList, sp);
                    } else if (buf[0] == "DataDraw") {
                        sp = mDataDraw.setDataList(dataList, sp);
                    }
                }
            } else
                return;
            updateArea();
            mFirstEntityCount = mEntityList.Count;
        }

        /// <summary>
        /// ファイルに保存
        /// </summary>
        /// <param name="forth">強制保存</param>
        public void saveFile(bool forth = false)
        {
            saveFile(mDataPath, forth);
        }

        /// <summary>
        /// ファイルに保存
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="forth">強制保存</param>
        public void saveFile(string path, bool forth = false)
        {
            if (path.Length == 0) return;
            if (!forth && (mFirstEntityCount == mEntityList.Count &&
                0 == mGlobal.mOperationCount))
                return;
            mCommandOpe.setMemoText();
            List<string[]> dataList = new List<string[]>();
            string[] buf = { "AppName", mGlobal.mMainWindow.mAppName, "ver", mVersion, "rew", mRevsion };
            dataList.Add(buf);
            dataList.AddRange(toDataList());
            dataList.AddRange(mDataDraw.toDataList());
            dataList.AddRange(toEntityDataList());
            ylib.saveCsvData(path, dataList);
        }

        /// <summary>
        /// インポート機能(Min3DCadデータ)
        /// </summary>
        /// <param name="categoryPath">保存カテゴリ名</param>
        public void import(string categoryPath)
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "CSVファイル", "*.csv" },
                    //new string[] { "DXFファイル", "*.dxf" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string path = ylib.fileOpenSelectDlg("インポート", ".", filters);
            if (0 < path.Length) {
                mDataPath = Path.Combine(categoryPath, Path.GetFileName(path));
                string fname = Path.GetFileNameWithoutExtension(path);
                int n = 1;
                while (File.Exists(mDataPath)) {
                    mDataPath = Path.Combine(categoryPath, fname);
                    mDataPath += $"_{n}.csv";
                    n++;
                }
                loadFile(path);
                if (0 < mEntityList.Count)
                    saveFile(true);
            }
        }
    }
}
