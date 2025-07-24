using CoreLib;
using System.Windows;

namespace Cad3DApp
{
    /// <summary>
    /// ピックデータ
    /// </summary>
    public class PickData
    {
        public int mEntityNo;                   //  要素No
        public PointD mPos;                     //  ピック位置
        public FACE3D mFace;                    //  表示面

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="no">要素No</param>
        /// <param name="pos">ピック位置</param>
        /// <param name="face">表示面</param>
        public PickData(int no, PointD pos, FACE3D face)
        {
            mEntityNo = no;
            mPos = pos;
            mFace = face;
        }
    }

    /// <summary>
    /// ロケイト・ピック処理クラス
    /// </summary>
    public class LockPick
    {
        //  アプリキーによるロケイトメニュー
        private List<string> mLocMenu = new List<string>() {
            "座標入力", "相対座標入力"
        };
        //  Ctrl + マウス右ピックによるロケイトメニュー
        private List<string> mLocSelectMenu = new List<string>() {
            "端点・中間点", "3分割点", "4分割点", "5分割点", "6分割点", "8分割点",
            "垂点",
        };

        public List<PickData> mPickEntity = new List<PickData>();   //  ピック要素リスト
        public List<PickData> mLocPickEntity = new List<PickData>();//  ロケイトピック要素リスト
        public List<Point3D> mLocList = new List<Point3D>();        //  ロケイトの保存
        public int mDivideNo = 4;                                   //  autoLocの分割数
        public Group mGroup;                                        //  グループ
        public Layer mLayer;                                        //  レイヤ
        public bool mBaseLoc = true;                                //  ロケイト座標を指定面の投影位置

        private FACE3D mFace;
        private List<Entity> mEntityList;
        private Window mMainWindow;
        private YLib ylib = new YLib();
        private YCalc ycalc = new YCalc();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainWindow">MainWindow</param>
        /// <param name="entityList">要素リスト</param>
        /// <param name="face">2D平面</param>
        public LockPick(Window mainWindow, List<Entity> entityList, FACE3D face)
        {
            mMainWindow = mainWindow;
            mEntityList = entityList;
            mFace = face;
        }

        /// <summary>
        /// 表示面を設定
        /// </summary>
        /// <param name="face">2D平面</param>
        public void setCanvasFace(FACE3D face)
        {
            mFace = face;
        }

        /// <summary>
        /// ピックリストとロケイトリストをクリア
        /// </summary>
        public void clear()
        {
            mPickEntity.Clear();
            mLocPickEntity.Clear();
            mLocList.Clear();
        }

        /// <summary>
        /// オートロケイト
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="pickSize">ピックサイズ</param>
        /// <param name="onCtrl">Ctrlキー</param>
        /// <param name="onAlt">Altキー</param>
        /// <returns>ロケイト登録完了</returns>
        public bool autoLoc(PointD pickPos, bool onCtrl, bool onAlt)
        {
            Point3D pos3 = null;
            if (onCtrl) {
                //  メニュー表示
                pos3 = locSelect(pickPos, mLocPickEntity, mFace);
            } else if (onAlt) {
                //  別々にピックした２要素の交点(Alt + RightMouse)
                if (mLocPickEntity.Count == 2) {
                    pos3 = mEntityList[mLocPickEntity[0].mEntityNo].intersection2(
                        mEntityList[mLocPickEntity[1].mEntityNo], pickPos, mFace);
                } else {
                    return false;
                }
            } else if (mLocPickEntity.Count == 1) {
                //  分割点でピック位置に最も近い座標
                pos3 = mEntityList[mLocPickEntity[0].mEntityNo].nearPoint(pickPos, mDivideNo, mFace);
            } else if (mLocPickEntity.Count == 2) {
                //  2要素の交点
                pos3 = mEntityList[mLocPickEntity[0].mEntityNo].intersection2(
                            mEntityList[mLocPickEntity[1].mEntityNo], pickPos, mFace);
            }
            if (pos3 == null && 1 < mLocPickEntity.Count) {
                int n = pickSelect(mLocPickEntity);         //  要素選択
                if (0 <= n)
                    pos3 = mEntityList[mLocPickEntity[n].mEntityNo].nearPoint(pickPos, mDivideNo, mFace);
            }
            mLocPickEntity.Clear();
            if (pos3 == null) return false;
            if (mBaseLoc)
                mLocList.Add(new Point3D(pos3.toPoint(mFace), mFace));
            else
                mLocList.Add(pos3);
            return true;
        }

        /// <summary>
        /// ロケイト時のピック処理
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <param name="pickArea">ピック領域</param>
        /// <param name="onCtrl">Ctrlキー</param>
        /// <param name="onAlt">Altキー</param>
        /// <returns>ピックの有無</returns>
        public bool getLocPickNo(PointD pos, Box pickArea, bool onAlt)
        {
            List<PickData> pickList = getPickList(pos, pickArea);
            int pickNo = 0;
            if (onAlt && 1 < pickList.Count) {
                pickNo = pickSelect(pickList);
                if (pickNo < 0) return false;
                mLocPickEntity.Add(pickList[0]);
            } else
                mLocPickEntity.AddRange(pickList);
            return 0 < mLocPickEntity.Count;
        }

        /// <summary>
        /// ピック要素の取得
        /// ピック済みの要素はリストから削除(unpick)
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <param name="pickSize">ピックサイズ</param>
        /// <param name="onCtrl">Ctrlキー</param>
        /// <param name="area">領域指定</param>
        /// <returns>ピックの有無</returns>
        public bool getPickNo(PointD pos, Box pickArea, bool ctrl, bool area = false)
        {
            List<PickData> pickList = getPickList(pos, pickArea);
            if (pickList.Count == 0) return false;
            //bool ctrl = ylib.onControlKey();
            int pickNo = -1;
            //  複数ピックの時の要素選択
            if (1 < pickList.Count && !area)
                pickNo = pickSelect(pickList);
            //  グループピック(ctrl)
            if (ctrl) {
                pickList = getGroup(pickList);
                if (0 < pickList.Count)
                    mPickEntity.AddRange(pickList);
            } else {
                //  アンピック処理
                if (0 <= pickNo) {
                    int index = mPickEntity.FindIndex(p => p.mEntityNo == pickList[pickNo].mEntityNo);
                    if (0 <= index)
                        mPickEntity.RemoveAt(index);
                    else
                        mPickEntity.Add(pickList[pickNo]);
                } else {
                    foreach (var pick in pickList) {
                        int index = mPickEntity.FindIndex(p => p.mEntityNo == pick.mEntityNo);
                        if (0 <= index)
                            mPickEntity.RemoveAt(index);
                        else
                            mPickEntity.Add(pick);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// ピック要素の選択
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <returns></returns>
        private int pickSelect(List<PickData> picks)
        {
            List<string> menu = new List<string>();
            for (int i = 0; i < picks.Count; i++) {
                Entity ent = mEntityList[picks[i].mEntityNo];
                menu.Add($"{picks[i].mEntityNo} {ent.mID.ToString()} {ent.mArea.ToString("F2")}");
            }
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "要素選択";
            dlg.mMainWindow = mMainWindow;
            dlg.mHorizontalAliment = 1;
            dlg.mVerticalAliment = 1;
            dlg.mOneClick = true;
            dlg.mMenuList = menu;
            dlg.ShowDialog();
            if (dlg.mResultMenu == "")
                return -1;
            else
                return picks.FindIndex(p => p.mEntityNo == ylib.string2int(dlg.mResultMenu));
        }


        /// <summary>
        /// ピック要素の取得
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <param name="pickArea">ピック領域</param>
        /// <returns>ピック要素リスト</returns>
        public List<PickData> getPickList(PointD pos, Box pickArea)
        {
            List<PickData> pickList = new List<PickData>();
            for (int i = 0; i < mEntityList.Count; i++) {
                if (mEntityList[i].is2DDraw(mLayer))
                    if (mEntityList[i].isPick(pickArea, mFace))
                        pickList.Add(new PickData(i, pos, mFace));
            }
            return pickList;
        }

        /// <summary>
        /// グループピック
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <returns>ピック要素リスト</returns>
        public List<PickData> getGroup(List<PickData> picks)
        {
            List<PickData> pickList = new List<PickData>();
            foreach (var pick in picks) {
                List<int> groupList = getGroup(pick.mEntityNo);
                foreach (int entityNo in groupList) {
                    pickList.Add(new PickData(entityNo, pick.mPos, pick.mFace));
                }
            }
            return pickList;
        }

        /// <summary>
        /// グループ要素を取得
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <returns>要素リスト</returns>
        public List<int> getGroup(int pick)
        {
            List<int> grouplist = new List<int>();
            if (0 < mEntityList[pick].mGroup) {
                grouplist.AddRange(mGroup.getGroupNoList(
                        mEntityList, mEntityList[pick].mGroup));
            }
            //  重複削除
            for (int i = grouplist.Count - 1; i >= 0; i--) {
                for (int j = 0; j < i; j++) {
                    if (grouplist[j] == grouplist[i]) {
                        grouplist.RemoveAt(i);
                        break;
                    }
                }
            }
            return grouplist;
        }

        /// <summary>
        /// ピックフラグの設定
        /// </summary>
        public void setPick()
        {
            for (int i = 0; i < mPickEntity.Count; i++)
                mEntityList[mPickEntity[i].mEntityNo].mPick = true;
        }

        /// <summary>
        /// ピックフラグの全解除
        /// </summary>
        public void pickReset()
        {
            for (int i = 0; i < mPickEntity.Count; i++)
                mEntityList[mPickEntity[i].mEntityNo].mPick = false;
        }

        /// <summary>
        /// ロケイト選択メニューによる座標指定
        /// </summary>
        /// <param name="pos">ロケイト位置</param>
        /// <param name="picks">ピック要素</param>
        /// <param name="face">操作面</param>
        /// <returns>座標</returns>
        private Point3D locSelect(PointD pos, List<PickData> picks, FACE3D face)
        {
            int n = 0;
            if (picks.Count < 1) return new Point3D(pos, face); ;
            if (1 < picks.Count)
                n = pickSelect(picks);         //  要素選択
            if (n < 0) return null;
            List<string> locMenu = mLocSelectMenu.ToList();
            Entity ent;
            if (picks.Count == 1) {
                ent = mEntityList[picks[0].mEntityNo];
                if (ent.mID == EntityId.Arc)
                    locMenu.Add("中心点");
            } else
                return null;
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "ロケイトメニュー";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = locMenu;
            dlg.ShowDialog();
            if (0 < dlg.mResultMenu.Length)
                return getLocSelectPos(dlg.mResultMenu, pos, picks, face);
            return null;
        }

        /// <summary>
        /// 選択座標
        /// </summary>
        /// <param name="selMenu">選択座標メニュー</param>
        /// <param name="pos">ロケイト位置</param>
        /// <param name="picks">ピック要素</param>
        /// <param name="face">操作面</param>
        /// <returns>座標</returns>
        private Point3D getLocSelectPos(string selMenu, PointD pos, List<PickData> picks, FACE3D face)
        {
            Entity ent = mEntityList[picks[0].mEntityNo];
            Point3D pos3 = null;
            Point3D lastLoc = 0 < mLocList.Count ? mLocList.Last() : pos3;
            switch (selMenu) {
                case "端点・中間点": pos3 = ent.nearPoint(pos, 2, face); break;
                case "3分割点": pos3 = ent.nearPoint(pos, 3, face); break;
                case "4分割点": pos3 = ent.nearPoint(pos, 4, face); break;
                case "5分割点": pos3 = ent.nearPoint(pos, 5, face); break;
                case "6分割点": pos3 = ent.nearPoint(pos, 6, face); break;
                case "8分割点": pos3 = ent.nearPoint(pos, 8, face); break;
                case "垂点": pos3 = ent.nearPoint(lastLoc.toPoint(face), 0, face); break;
                case "中心点":
                    ArcEntity arcEnt = (ArcEntity)ent;
                    pos3 = arcEnt.mArc.mPlane.mCp;
                    break;
            }
            return pos3;
        }


        /// <summary>
        /// ロケイトメニューの表示(Windowsメニューキー)
        /// </summary>
        /// <param name="operation">操作</param>
        public void locMenu(OPERATION operation)
        {
            List<string> locMenu = new List<string>();
            locMenu.AddRange(mLocMenu);
            if (operation == OPERATION.circle || operation == OPERATION.arc) {
                locMenu.Add("半径");
            } else if (operation == OPERATION.rotate || operation == OPERATION.copyRotate) {
                locMenu.Add("回転角");
            }
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "ロケイトメニュー";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = locMenu;
            dlg.ShowDialog();
            if (0 < dlg.mResultMenu.Length) {
                getInputLoc(dlg.mResultMenu);
            }
        }

        /// <summary>
        /// ロケイトメニュー処理
        /// </summary>
        /// <param name="title">選択メニューのタイトル</param>
        /// <param name="operation">操作</param>
        private void getInputLoc(string title)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = title;
            if (dlg.ShowDialog() == true) {
                string[] valstr = dlg.mEditText.Split(',');
                List<double> valList = new List<double>();
                foreach (string val in valstr)
                    valList.Add(ycalc.expression(val));
                Point3D wp3;
                if (valList.Count == 1) {
                    PointD wp = new PointD(valList[0], 0);
                    wp3 = new Point3D(wp, mFace);
                } else if (valList.Count == 2) {
                    PointD wp = new PointD(valList[0], valList[1]);
                    wp3 = new Point3D(wp, mFace);
                } else if (valList.Count > 2) {
                    wp3 = new Point3D(valList[0], valList[1], valList[2]);
                } else
                    return;
                switch (title) {
                    case "座標入力":          //  xxx,yyy で入力
                        mLocList.Add(wp3);
                        break;
                    case "相対座標入力":      //  xxx,yyy で入力
                        mLocList.Add(mLocList.Last() + wp3);
                        break;
                    case "半径":              //  円の作成
                        wp3 = mLocList.Last() + new Point3D(new PointD(valList[0], 0), mFace);
                        mLocList.Add(wp3);
                        break;
                    case "回転角":
                        if (mLocList.Count == 1) {
                            mLocList.Add(mLocList[0] + new Point3D(new PointD(1, 0), mFace));
                        }
                        PointD vec = mLocList.Last().toPoint(mFace) - mLocList[0].toPoint(mFace);
                        vec.rotate(ylib.D2R(valList[0]));
                        PointD p = mLocList[0].toPoint(mFace) + vec;
                        mLocList.Add(new Point3D(p, mFace));
                        break;
                }
            }
        }

        /// <summary>
        /// グループピックメニュー
        /// </summary>
        /// <param name="pos">位置</param>
        /// <param name="face">2D平面</param>
        /// <returns>ピックの可否</returns>
        public bool groupSelectPick(PointD pos, FACE3D face)
        {
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "グループピックメニュー";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = mGroup.getGroupNameList();
            dlg.ShowDialog();
            if (0 < dlg.mResultMenu.Length) {
                int groupNo = mGroup.getGroupNo(dlg.mResultMenu);
                if (0 < groupNo) {
                    List<PickData> groupList = new List<PickData>();
                    for (int j = 0; j < mEntityList.Count; j++) {
                        if (groupNo == mEntityList[j].mGroup &&
                            !mEntityList[j].mRemove)
                            groupList.Add(new PickData(j, pos, face));
                    }
                    mPickEntity.AddRange(groupList);
                    return true;
                }
            }
            return false;
        }
    }
}
