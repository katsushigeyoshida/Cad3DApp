using CoreLib;
using System.Windows;

namespace Cad3DApp
{
    public class CommandOpe
    {
        public int mSaveOperationCount = 10;                    //  定期保存の操作回数
        public Layer mLayer;                                    //  レイヤ
        public Group mGroup;                                    //  グループ
        public List<Entity> mEntityList = new List<Entity>();   //  要素リスト
        public GlobalData mGlobal;
        public FileData mFileData;

        public bool mOriginalDataText = false;                  //  要素データを生データでテキスト化
        public InputBox mMemoDlg = null;                        //  メモダイヤログ

        private EditEntity mEditEntity;                         //  データ要素の変更
        private CreateEntity mCreateEntity;                     //  データ要素の作成

        private YLib ylib = new YLib();

        public CommandOpe(List<Entity> entityList, GlobalData global)
        {
            mGlobal = global;
            mEntityList = entityList;
            mCreateEntity = new CreateEntity(mGlobal);
            mEditEntity = new EditEntity(mGlobal, mEntityList);
        }

        /// <summary>
        /// コマンドからオペレーションモードを設定
        /// </summary>
        /// <param name="ope">コマンド</param>
        /// <param name="picks">ピック要素</param>
        /// <returns></returns>
        public OPEMODE execCommand(OPERATION ope, List<PickData> picks)
        {
            mGlobal.mOperationCount++;
            OPEMODE opeMode = OPEMODE.loc;
            switch (ope) {
                //  ロケイト操作の必要なコマンド(DataMangeで処理)
                case OPERATION.point: break;
                case OPERATION.line: break;
                case OPERATION.circle: break;
                case OPERATION.arc: break;
                case OPERATION.polyline: break;
                case OPERATION.rect: break;
                case OPERATION.polygon: break;
                case OPERATION.translate: break;
                case OPERATION.rotate: break;
                case OPERATION.offset: break;
                case OPERATION.mirror: break;
                case OPERATION.trim: break;
                case OPERATION.stretch: break;
                case OPERATION.scale: break;
                case OPERATION.copyTranslate: break;
                case OPERATION.copyRotate: break;
                case OPERATION.copyOffset: break;
                case OPERATION.copyMirror: break;
                case OPERATION.copyTrim: break;
                case OPERATION.copyScale: break;
                case OPERATION.extrusion: break;
                case OPERATION.divide: break;
                case OPERATION.measureAngle: break;
                case OPERATION.measureDistance: break;
                //  ロケイト操作の不要なコマンド
                case OPERATION.fillet: opeMode = fillet(picks); break;
                case OPERATION.connect: opeMode = connect(picks); break;
                case OPERATION.disassemble: opeMode = disassemble(picks); break;
                case OPERATION.changeProperty: opeMode = changeProperty(picks); break;
                case OPERATION.changeEntityData: opeMode = changeEntityData(picks); break;
                case OPERATION.changePropertyAll: opeMode = changePropertyAll(picks); break;
                case OPERATION.copyEntity: opeMode = copyEntity(picks); break;
                case OPERATION.pasteEntity: opeMode = pasteEntity(); break;
                case OPERATION.blend: opeMode = blend(picks); break;
                case OPERATION.revolution: opeMode = revolution(picks); break;
                case OPERATION.sweep: opeMode = sweep(picks); break;
                case OPERATION.release: opeMode = release(picks); break;
                case OPERATION.zumenComment: opeMode = zumenComment(); break;
                case OPERATION.dispLayer: opeMode = OPEMODE.exec; break;
                case OPERATION.addLayer: break;
                case OPERATION.disp2DAll: opeMode = OPEMODE.clear; break;
                case OPERATION.measure: opeMode = OPEMODE.clear; break;
                case OPERATION.info: opeMode = OPEMODE.clear; break;
                case OPERATION.remove: opeMode = remove(picks); break;
                case OPERATION.undo: opeMode = undo(); break;
                case OPERATION.redo: opeMode = redo(); break;
                case OPERATION.setColor: opeMode = OPEMODE.exec; break;
                case OPERATION.setGrid: opeMode = OPEMODE.exec; break;
                case OPERATION.screenCopy: opeMode = screenCopy(); break;
                case OPERATION.screenSave: opeMode = OPEMODE.clear; break;
                case OPERATION.systemProperty: opeMode = setSystemProperty(); break;
                case OPERATION.imageTrimming: opeMode = OPEMODE.clear; break;
                case OPERATION.memo: opeMode = zumenMemo(); break;
                case OPERATION.back: opeMode = OPEMODE.clear; break;
                case OPERATION.save: opeMode = OPEMODE.exec; break;
                case OPERATION.cancel: opeMode = OPEMODE.clear; break;
                case OPERATION.close: opeMode = OPEMODE.close; break;
                default: opeMode = OPEMODE.clear; break;
            }
            return opeMode;
        }

        /// <summary>
        /// ブレンドの作成
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        public OPEMODE blend(List<PickData> picks)
        {
            if (1 < picks.Count) {
                List<Entity> entityList = mEditEntity.Blend(picks, true);
                addEntity(entityList, picks);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// 回転体の作成
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        public OPEMODE revolution(List<PickData> picks)
        {
            if (1 < picks.Count) {
                List<Entity> entityList = mEditEntity.Revolution(picks, true);
                addEntity(entityList, picks);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// 掃引の作成
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        public OPEMODE sweep(List<PickData> picks)
        {
            if (1 < picks.Count) {
                List<Entity> entityList = mEditEntity.Sweep(picks, true);
                addEntity(entityList, picks);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// フィレット作成
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        public OPEMODE fillet(List<PickData> picks)
        {
            if (picks.Count != 0) {
                List<Entity> entityList = mEditEntity.fillet(picks, mGlobal.mFilletSize, mGlobal.mFace);
                if (0 < entityList.Count)
                    addEntity(entityList, picks);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// 要素同士の接続(1要素時はポリゴン化)
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        /// <returns></returns>
        public OPEMODE connect(List<PickData> picks)
        {
            if (picks.Count != 0) {
                List<Entity> entityList = mEditEntity.connect(picks, mGlobal.mFace);
                if (0 < entityList.Count)
                    addEntity(entityList, picks);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// ポリラインまたはポリゴンを線分と円弧に分解する
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        /// <returns></returns>
        public OPEMODE disassemble(List<PickData> picks)
        {
            if (picks.Count != 0) {
                List<Entity> entityList = mEditEntity.disassemble(picks);
                if (0 < entityList.Count)
                    addEntity(entityList, picks);
            }
            return OPEMODE.clear;
        }



        /// <summary>
        /// 作成要素を登録し元要素のリンクを作成して削除する
        /// </summary>
        /// <param name="entityList">ピック要素リスト</param>
        private void addEntity(List<Entity> entityList, List<PickData> picks)
        {
            if (0 < entityList.Count) {
                foreach (var entity in entityList)
                    mEditEntity.addEntity(entity, mGlobal.mOperationCount);
                foreach (var pickEnt in picks)
                    mEditEntity.addLink(pickEnt.mEntityNo, mGlobal.mLayerSize, mGlobal.mOperationCount);
            }
        }

        /// <summary>
        /// 要素削除
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        public OPEMODE remove(List<PickData> picks)
        {
            for (int i = 0; i < picks.Count; i++) {
                mEditEntity.addLink(picks[i].mEntityNo,
                    mEntityList[picks[i].mEntityNo].mLayerBit.Length * 8, mGlobal.mOperationCount);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// 解除(押出や回転体を解除して元のポリゴンや線分に戻す)
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public OPEMODE release(List<PickData> picks)
        {
            foreach (PickData pick in picks) {
                Entity entity = mEntityList[pick.mEntityNo];
                if (entity.mID == EntityId.Extrusion) {
                    //  押出解除
                    ExtrusionEntity extrusion = (ExtrusionEntity)entity;
                    foreach (var polygon in extrusion.mPolygons) {
                        if (extrusion.mClose) {
                            mEditEntity.addEntity(mCreateEntity.createPolygon(polygon, true), mGlobal.mOperationCount);
                        } else {
                            mEditEntity.addEntity(mCreateEntity.createPolyline(polygon.toPolyline3D(0, false), true), mGlobal.mOperationCount);
                        }
                    }
                } else if (entity.mID == EntityId.Blend) {
                    //  ブレンド解除
                    BlendEntity blend = (BlendEntity)entity;
                    foreach (var polyline in blend.mPolylines)
                        mEditEntity.addEntity(mCreateEntity.createPolyline(polyline, true), mGlobal.mOperationCount);
                } else if (entity.mID == EntityId.Revolution) {
                    //  回転体解除
                    RevolutionEntity revolution = (RevolutionEntity)entity;
                    mEditEntity.addEntity(mCreateEntity.createLine(revolution.mCenterLine, true), mGlobal.mOperationCount);
                    mEditEntity.addEntity(mCreateEntity.createPolyline(revolution.mOutLine, true), mGlobal.mOperationCount);
                } else if (entity.mID == EntityId.Sweep) {
                    //  掃引解除
                    SweepEntity sweep = (SweepEntity)entity;
                    mEditEntity.addEntity(mCreateEntity.createPolyline(sweep.mOutLine1, true), mGlobal.mOperationCount);
                    mEditEntity.addEntity(mCreateEntity.createPolyline(sweep.mOutLine2, true), mGlobal.mOperationCount);
                }
                mEditEntity.addLink(pick.mEntityNo, mGlobal.mLayerSize, mGlobal.mOperationCount);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// アンドゥ処理
        /// </summary>
        public OPEMODE undo()
        {
            if (0 < mEntityList.Count) {
                int entNo = lastEntNo();
                int opeNo = mEntityList[entNo].mOperationNo;
                while (0 <= entNo && 0 <= opeNo && opeNo == mEntityList[entNo].mOperationNo) {
                    if (0 <= mEntityList[entNo].mLinkNo && !mEntityList[entNo].mRemove)
                        mEntityList[mEntityList[entNo].mLinkNo].mRemove = false;
                    mEntityList[entNo].mRemove = true;
                    entNo--;
                }
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// リドゥ処理
        /// </summary>
        /// <returns></returns>
        public OPEMODE redo()
        {
            if (0 < mEntityList.Count) {
                int entNo = lastEntNo() + 1;
                if (mEntityList.Count <= entNo) return OPEMODE.non;
                int opeNo = mEntityList[entNo].mOperationNo;
                while (entNo < mEntityList.Count && 0 <= opeNo && opeNo == mEntityList[entNo].mOperationNo) {
                    if (0 <= mEntityList[entNo].mLinkNo && mEntityList[entNo].mRemove)
                        mEntityList[mEntityList[entNo].mLinkNo].mRemove = true;
                    mEntityList[entNo].mRemove = false;
                    entNo++;
                }
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// 有効要素の最終位置
        /// </summary>
        /// <returns>要素位置</returns>
        private int lastEntNo()
        {
            for (int i = mEntityList.Count - 1; 0 <= i; i--)
                if (!mEntityList[i].mRemove)
                    return i;
            return 0;
        }

        /// <summary>
        /// 要素の共通属性変更
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public OPEMODE changeProperty(List<PickData> picks)
        {
            if (picks != null) {
                if (picks.Count == 1) {
                    if (changeProperty(mEntityList[picks[0].mEntityNo].toCopy(), picks[0].mEntityNo)) {
                        mEditEntity.addLink(picks[0].mEntityNo,
                            mEntityList[picks[0].mEntityNo].mLayerBit.Length * 8, mGlobal.mOperationCount);
                    }
                } else if (1 < picks.Count)
                    return changePropertyAll(picks);
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// 要素データの表示・変更(テキスト編集)
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public OPEMODE changeEntityData(List<PickData> picks)
        {
            if (picks != null && picks.Count == 1) {
                if (changeEntityData(mEntityList[picks[0].mEntityNo].toCopy(), picks[0].mEntityNo)) {
                    mEditEntity.addLink(picks[0].mEntityNo,
                        mEntityList[picks[0].mEntityNo].mLayerBit.Length * 8, mGlobal.mOperationCount);
                }
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// 要素データの表示・変更(テキスト編集)
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <param name="entityNo">要素No</param>
        /// <returns>変更可否</returns>
        private bool changeEntityData(Entity entity, int entityNo)
        {
            List<string[]> dataList = entity.toDataList();  //  生データ
            List<string> datas = entity.getDataText();      //  3D変換データ
            string buf = "";
            if (mOriginalDataText) {
                foreach (string[] data in dataList) {
                    for (int i = 0; i < data.Length - 1; i++)
                        buf += data[i] + ",";
                    buf += data.Last() + "\n";
                }
                buf = buf.TrimEnd('\n');
            } else {
                foreach (string s in datas)
                    buf += s + '\n';
                buf = buf.TrimEnd('\n');
            }

            InputBox dlg = new InputBox();
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMultiLine = true;
            dlg.mWindowSizeOutSet = true;
            dlg.Title = $"要素データ [{entityNo}][{entity.mID}]";
            dlg.mEditText = buf;

            if (dlg.ShowDialog() == true) {
                string[] data = dlg.mEditText.Split('\n');
                if (mOriginalDataText) {
                    dataList = new List<string[]>();
                    for (int i = 0; i < data.Length; i++) {
                        string[] texts = data[i].Split(',');
                        dataList.Add(texts);
                    }
                    entity.setDataList(dataList, 0);
                } else {
                    datas.Clear();
                    foreach (string t in data)
                        datas.Add(t);
                    entity.setDataText(datas);
                }

                entity.mOperationNo = mGlobal.mOperationCount;
                entity.createVertexData();
                entity.createSurfaceData();
                entity.setArea();
                mEntityList.Add(entity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 要素の共通属性変更
        /// </summary>
        /// <param name="entity">要素データ</param>
        /// <param name="entityNo">要素No</param>
        /// <returns>変更可否</returns>
        private bool changeProperty(Entity entity, int entityNo)
        {
            PropertyDlg dlg = new PropertyDlg();
            dlg.Title = $"要素属性 [{entityNo}][{entity.mID}]";
            dlg.mLineColor = entity.mLineColor;
            dlg.mLineFont = entity.mLineType;
            dlg.mFaceColor = entity.mFaceColor;
            dlg.mDisp2D = entity.mDisp2D;
            dlg.mDisp3D = entity.mDisp3D;
            dlg.mReverse = entity.mReverse;
            dlg.mEdgeDisp = entity.mEdgeDisp;
            dlg.mEdgeReverse = entity.mEdgeReverse;
            dlg.mChkList = mLayer.getLayerChkList(entity.mLayerBit);
            dlg.mGroupList = mGroup.getGroupNameList();
            dlg.mGroup = mGroup.getGroupName(entity.mGroup);

            if (dlg.ShowDialog() == true) {
                entity.mLineColor = dlg.mLineColor;
                entity.mLineType = dlg.mLineFont;
                entity.mFaceColor = dlg.mFaceColor;
                entity.mDisp2D = dlg.mDisp2D;
                entity.mDisp3D = dlg.mDisp3D;
                entity.mReverse = dlg.mReverse;
                entity.mEdgeDisp = dlg.mEdgeDisp;
                entity.mEdgeReverse = dlg.mEdgeReverse;
                entity.mLayerBit = mLayer.setLayerChkList(entity.mLayerBit, dlg.mChkList);
                entity.mOperationNo = mGlobal.mOperationCount;
                entity.mGroup = mGroup.add(dlg.mGroup);

                entity.createVertexData();
                entity.createSurfaceData();
                entity.setArea();

                mEntityList.Add(entity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 要素属性一括変更
        /// </summary>
        /// <param name="picks">要素リスト</param>
        public OPEMODE changePropertyAll(List<PickData> picks)
        {
            PropertyDlg dlg = new PropertyDlg();
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "一括属性変更 ";
            dlg.mPropertyAll = true;
            dlg.mChkList = mLayer.getLayerChkList(true);
            dlg.mGroupList = mGroup.getGroupNameList();
            if (dlg.ShowDialog() == true) {
                for (int i = 0; i < picks.Count; i++) {
                    Entity entity = mEntityList[picks[i].mEntityNo].toCopy();
                    if (dlg.mLineColorEnable)
                        entity.mLineColor = dlg.mLineColor;
                    if (dlg.mLineFontEnable)
                        entity.mLineType = dlg.mLineFont;
                    if (dlg.mFaceColorEnable)
                        entity.mFaceColor = dlg.mFaceColor;
                    if (dlg.mDisp2DEnable)
                        entity.mDisp2D = dlg.mDisp2D;
                    if (dlg.mDisp3DEnable)
                        entity.mDisp3D = dlg.mDisp3D;
                    if (dlg.mReverseEnable)
                        entity.mReverse = dlg.mReverse;
                    if (dlg.mEdgeDispEnable)
                        entity.mEdgeDisp = dlg.mEdgeDisp;
                    if (dlg.mEdgeReverseEnable)
                        entity.mEdgeReverse = dlg.mEdgeReverse;
                    if (dlg.mCkkListEnable)
                        entity.mLayerBit = mLayer.setLayerChkList(entity.mLayerBit, dlg.mChkList);
                    if (dlg.mGroupEnable)
                        entity.mGroup = mGroup.add(dlg.mGroup);
                    entity.mOperationNo = mGlobal.mOperationCount;

                    entity.createVertexData();
                    entity.createSurfaceData();
                    entity.setArea();
                    mEntityList.Add(entity);
                    mEditEntity.addLink(picks[i].mEntityNo,
                        mEntityList[picks[i].mEntityNo].mLayerBit.Length * 8, mGlobal.mOperationCount);
                }
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// システム設定
        /// </summary>
        /// <returns>データフォルダの変更の可否</returns>
        public OPEMODE setSystemProperty()
        {
            OPEMODE result = OPEMODE.non;
            SystemDlg dlg = new SystemDlg();
            dlg.Owner = mGlobal.mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //dlg.mArcDivideAngle = mArcDivideAng;
            //dlg.mRevolutionDivideAngle = mRevolutionDivideAng;
            //dlg.mSweepDivideAngle = mSweepDivideAng;
            //dlg.mSurfaceVertex = mSurfaceVertex;
            dlg.mWireFrame = mGlobal.mWireFrame;
            dlg.mBackColor = mGlobal.mBaseBackColor;
            dlg.mDataFolder = mGlobal.mBaseDataFolder;
            dlg.mBackupFolder = mGlobal.mBackupFolder;
            dlg.mDiffTool = mFileData.mDiffTool;
            dlg.mFileData = mFileData;
            if (dlg.ShowDialog() == true) {
                //mArcDivideAng = dlg.mArcDivideAngle;
                //mRevolutionDivideAng = dlg.mRevolutionDivideAngle;
                //mSweepDivideAng = dlg.mSweepDivideAngle;
                if (mGlobal.mWireFrame != dlg.mWireFrame) {
                    //  3D表示変更
                    mGlobal.mWireFrame = dlg.mWireFrame;
                    result = OPEMODE.updateData;
                }
                mGlobal.mBaseBackColor = dlg.mBackColor;
                mGlobal.mBackupFolder = dlg.mBackupFolder;
                mFileData.mDiffTool = dlg.mDiffTool;
                mGlobal.mOperationCount++;
                if (mGlobal.mBaseDataFolder != dlg.mDataFolder) {
                    //  データフォルダ変更
                    mGlobal.mBaseDataFolder = dlg.mDataFolder;
                    result = OPEMODE.reload;
                }
            }
            return result;
        }

        /// <summary>
        /// 図面情報の表示と編集(ダイヤログ表示)
        /// </summary>
        public OPEMODE zumenComment()
        {
            InputBox dlg = new InputBox();
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMultiLine = true;
            dlg.mWindowSizeOutSet = true;
            dlg.Title = "図面のコメント";
            dlg.mEditText = mGlobal.mZumenComment;
            if (dlg.ShowDialog() == true) {
                if (mGlobal.mZumenComment != dlg.mEditText) {
                    mGlobal.mZumenComment = dlg.mEditText;
                    mGlobal.mOperationCount++;
                }
            }
            return OPEMODE.clear;
        }

        /// <summary>
        /// メモデータ入力ダイヤログを開く
        /// </summary>
        public OPEMODE zumenMemo()
        {
            if (mMemoDlg != null)
                mMemoDlg.Close();
            mMemoDlg = new InputBox();
            mMemoDlg.Topmost = true;
            mMemoDlg.mMultiLine = true;
            mMemoDlg.Title = "めも";
            mMemoDlg.mEditText = mGlobal.mMemo;
            mMemoDlg.mCallBackOn = true;
            mMemoDlg.callback = setMemoText;
            mMemoDlg.Show();
            return OPEMODE.clear;
        }

        /// <summary>
        /// 図面のメモデータをmParaに設定する(CallBack)
        /// </summary>
        public void setMemoText()
        {
            if (mMemoDlg != null) {
                mMemoDlg.updateData();
                if (mGlobal.mMemo != mMemoDlg.mEditText) {
                    mGlobal.mMemo = mMemoDlg.mEditText;
                    mGlobal.mOperationCount++;
                }
            }
        }

        /// <summary>
        /// ピックした要素をクリップボードにコピー
        /// </summary>
        /// <param name="picks">ピック要素リスト</param>
        /// <returns>処理モード</returns>
        public OPEMODE copyEntity(List<PickData> picks)
        {
            if (picks.Count == 0) return OPEMODE.non;
            string copyBuf = mGlobal.mMainWindow.mAppName + "\n";
            //  要素領域
            Box3D area = mEntityList[picks[0].mEntityNo].mArea;
            for (int i = 1; i < picks.Count; i++) {
                area.extension(mEntityList[picks[i].mEntityNo].mArea);
            }
            copyBuf += $"area,{area.mMin.x},{area.mMin.y},{area.mMin.z},{area.mMax.x},{area.mMax.y},{area.mMax.z}\n";
            //  要素データ
            List<string[]> dataList = new List<string[]>();
            for (int i = 0; i < picks.Count; i++) {
                dataList.AddRange(mEntityList[picks[i].mEntityNo].toPropertyList());
                dataList.AddRange(mEntityList[picks[i].mEntityNo].toDataList());
            }
            //  文字列に変換
            foreach (string[] str in dataList) {
                copyBuf += ylib.arrayStr2CsvData(str) + "\n";
            }
            //  Clipboardにコピー
            System.Windows.DataObject data = new System.Windows.DataObject(System.Windows.DataFormats.Text, copyBuf);
            System.Windows.Clipboard.SetDataObject(data, true);

            return OPEMODE.clear;
        }

        /// <summary>
        /// クリップボードのデータを要素データに変換登録
        /// mCopyAreaとmCopyEntitiesに登録
        /// </summary>
        /// <returns>処理モード</returns>
        public OPEMODE pasteEntity()
        {
            string data = System.Windows.Clipboard.GetText();
            if (data == null || data.Length == 0) return OPEMODE.non;

            List<string[]> dataList = new List<string[]>();
            string[] strList = data.Split(new char[] { '\n' });
            for (int i = 0; i < strList.Length; i++)
                dataList.Add(ylib.csvData2ArrayStr(strList[i]));

            if (0 < dataList.Count) {
                if (dataList[0][0] == mGlobal.mMainWindow.mAppName) {
                    setEntityData(dataList);
                } else if (dataList[0][0] == "Mini3DCad") {
                    setMini3DCadData(dataList);
                } else if (dataList[0][0] == "CadApp") {
                    setCadAppData(dataList);
                }
            }

            return OPEMODE.loc;
        }

        /// <summary>
        /// 自アプリのデータ取込み(mCopyEntiesに登録)
        /// </summary>
        /// <param name="dataList">要素データリスト</param>
        private void setEntityData(List<string[]> dataList)
        {
            int sp = 1;
            if (mGlobal.mCopyEntities == null)
                mGlobal.mCopyEntities = new List<Entity>();
            else
                mGlobal.mCopyEntities.Clear();

            while (sp < dataList.Count - 1) {
                string[] strArray = dataList[sp++];
                if (4 < strArray.Length && strArray[0] == "area") {
                    mGlobal.mCopyArea = new Box3D($"{strArray[1]},{strArray[2]},{strArray[3]},{strArray[4]},{strArray[5]},{strArray[6]}");
                    mGlobal.mCopyArea.normalize();
                } else if (1 < strArray.Length && strArray[0] == "ID") {
                    (sp, Entity? entity) = mCreateEntity.dataList2Entity(strArray[1], dataList, sp);
                    if (entity != null) {
                        mGlobal.mCopyEntities.Add(entity);
                        mGlobal.mCopyEntities.Last().createVertexData();
                        mGlobal.mCopyEntities.Last().createSurfaceData();
                        mGlobal.mCopyEntities.Last().setArea();
                    }
                }
            }
        }

        /// <summary>
        /// Min3DCadデータの取込み
        /// </summary>
        /// <param name="dataList">要素データリスト</param>
        private void setMini3DCadData(List<string[]> dataList)
        {
            int sp = 1;
            if (mGlobal.mCopyEntities == null)
                mGlobal.mCopyEntities = new List<Entity>();
            else
                mGlobal.mCopyEntities.Clear();

            while (sp < dataList.Count - 1) {
                string[] strArray = dataList[sp++];
                if (4 < strArray.Length && strArray[0] == "area") {
                    mGlobal.mCopyArea = new Box3D($"{strArray[1]},{strArray[2]},{strArray[3]},{strArray[4]},{strArray[5]},{strArray[6]}");
                    mGlobal.mCopyArea.normalize();
                } else if (1 < strArray.Length && strArray[0] == "PrimitiveId") {
                    (sp, Entity? entity) = mCreateEntity.getMini3DCadData(strArray, dataList, sp);
                    if (entity != null) {
                        mGlobal.mCopyEntities.Add(entity);
                        mGlobal.mCopyEntities.Last().createVertexData();
                        mGlobal.mCopyEntities.Last().createSurfaceData();
                        mGlobal.mCopyEntities.Last().setArea();
                    }
                }
            }
        }

        /// <summary>
        /// CadAppデータの取込み
        /// </summary>
        /// <param name="dataList">要素データリスト</param>
        private void setCadAppData(List<string[]> dataList)
        {
            int sp = 1;
            if (mGlobal.mCopyEntities == null)
                mGlobal.mCopyEntities = new List<Entity>();
            else
                mGlobal.mCopyEntities.Clear();

            string[] strArray = dataList[sp++];
            if (4 < strArray.Length && strArray[0] == "area") {
                mGlobal.mCopyArea = new Box3D($"{strArray[1]},{strArray[2]},{strArray[3]},{strArray[4]},{strArray[5]},{strArray[6]}");
                mGlobal.mCopyArea.normalize();
            }
            while (sp < dataList.Count - 1) {
                string[] property = dataList[sp++];
                if (property.Length <= 4) continue;
                string[] data = dataList[sp++];
                Entity? entity = mCreateEntity.getCadAppData(property, data, mGlobal.mFace);
                if (entity != null) {
                    mGlobal.mCopyEntities.Add(entity);
                    mGlobal.mCopyEntities.Last().createVertexData();
                    mGlobal.mCopyEntities.Last().createSurfaceData();
                    mGlobal.mCopyEntities.Last().setArea();
                }
            }
        }

        /// <summary>
        /// 画面コピー
        /// </summary>
        /// <returns>処理モード</returns>
        public OPEMODE screenCopy()
        {
            if (mGlobal.mFace == FACE3D.NON) {
                mGlobal.mMainWindow.m3Dlib.screenCopy();
            } else {
                mGlobal.mMainWindow.mDataManage.screenCopy();
            }
            return OPEMODE.clear;
        }
    }
}