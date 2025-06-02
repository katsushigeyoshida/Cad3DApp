using CoreLib;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Cad3DApp
{
    /// <summary>
    /// キー入力によるコマンドの実行処理
    /// </summary>
    public class KeyCommand
    {
        private List<string> mMainCmd = new List<string>() {        //  Main Command
            "line", "rect", "polyline", "polygon", "arc", "circle", 
            "translate", "rotate", "offset", "mirror", "trim", "scaling",
            "stretch", "divide", "fillet", "connect", "disassemble",
            "extrusion", "blend", "revolution", "sweep", "release",
            "remove", "undo", "redo", "close",
            "color", "linetype", "thickness"
        };
        private List<string> mParaName = new List<string>() {       //  Parameter
            "x", "y", "z", "dx", "dy", "dz", "p", "r", "sa", "ea", "angle", "scale", "copy",
        };

        public enum EXETYPE  {                                      //  処理形式
            createEntity, editEntity, color, linetype, thickness,
            undo, redo, close, non };
        public EXETYPE mExeType = EXETYPE.non;

        //  外部参照用
        public Entity mEntity;
        public List<Entity> mEditEntityList = new List<Entity>();
        public List<PickData> mPickEnt = new List<PickData>();
        public Brush mColor;
        public int mLineType;
        public double mThickness;
        public bool mCopy = false;

        private List<Point3D> mPoints = new List<Point3D>();
        private double mRadius = 0;
        private double mSa = 0;
        private double mEa = Math.PI * 2;
        private double mAng = 0;
        private double mScale = 1;
        private double mValue = 0;
        private string mValString = "";
        private string mTextString = "";                             //  文字列データ

        private GlobalData mGlobalData = new GlobalData();
        public List<Entity> mEntityList;

        private YLib ylib = new YLib();
        private YDraw ydraw = new YDraw();
        private YCalc ycalc = new YCalc();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="globalData">グローバルデータ</param>
        /// <param name="entityList">要素リスト</param>
        public KeyCommand(GlobalData globalData, List<Entity> entityList)
        {
            mGlobalData = globalData;
            mEntityList = entityList;
        }

        /// <summary>
        /// パラメータの初期化
        /// </summary>
        private void initParameter()
        {
            mEntity = null;
            mEditEntityList.Clear();
            mPoints.Clear();
            mPickEnt.Clear();
            mRadius = 0;
            mSa = 0;
            mEa = Math.PI * 2;
            mAng = 0;
            mScale = 1;
            mValue = 0;
            mCopy = false;
        }

        /// <summary>
        /// コマンドの実行処理
        /// </summary>
        /// <param name="command">コマンド</param>
        /// <param name="face">作成面</param>
        /// <returns>処理形式</returns>
        public EXETYPE execCommand(string command, FACE3D face)
        {
            EXETYPE ret = EXETYPE.non;
            if (command.Length == 0) return ret;

            CreateEntity createEntity = new CreateEntity(mGlobalData);
            EditEntity editEntity = new EditEntity(mGlobalData, mEntityList);
            initParameter();
            try {
                int commandNo = getCommandParameter(command, face);
                if (commandNo < 0) return ret;
                switch (mMainCmd[commandNo]) {
                    case "color":
                        mColor = ylib.getBrsh(mTextString);
                        ret = EXETYPE.color;
                        break;
                    case "linetype":
                        if (0 < mTextString.Length)
                            mLineType = ydraw.mLineTypeName.FindIndex(mTextString);
                        else
                            mLineType = (int)mValue;
                        if (mLineType < 0 || ydraw.mLineTypeName.Length <= mLineType)
                            mLineType = 0;
                        ret = EXETYPE.linetype;
                        break;
                    case "thickness":
                        mThickness = Math.Max(1, mValue);
                        ret = EXETYPE.thickness;
                        break;
                    case "line":
                        if (1 < mPoints.Count)
                            mEntity = createEntity.createLine(mPoints[0], mPoints[1]);
                            ret = EXETYPE.createEntity;
                        break;
                    case "rect":
                        if (1 < mPoints.Count)
                            mEntity = createEntity.createRect(mPoints[0], mPoints[1], face);
                        ret = EXETYPE.createEntity;
                        break;
                    case "arc":
                        if (0 < mRadius && 0 < mPoints.Count)
                            mEntity = createEntity.createArc(mPoints[0], mRadius, mSa, mEa, face);
                        else if (2 < mPoints.Count)
                            mEntity = createEntity.createArc(mPoints[0], mPoints[2], mPoints[1]);
                        ret = EXETYPE.createEntity;
                        break;
                    case "circle":
                        if (0 < mRadius && 0 < mPoints.Count)
                            mEntity = createEntity.createArc(mPoints[0], mRadius, 0, Math.PI * 2, face);
                        else if (1 < mPoints.Count)
                            mEntity = createEntity.createArc(mPoints[0], mPoints[0].length(mPoints[1]), 0, Math.PI * 2, face);
                        ret = EXETYPE.createEntity;
                        break;
                    case "polyline":
                        if (1 < mPoints.Count)
                            mEntity = createEntity.createPolyline(mPoints);
                        ret = EXETYPE.createEntity;
                        break;
                    case "polygon":
                        if (1 < mPoints.Count)
                            mEntity = createEntity.createPolygon(mPoints);
                        ret = EXETYPE.createEntity;
                        break;
                    case "translate":
                        if (0 < mPickEnt.Count) {
                            if (1 < mPoints.Count)
                                mEditEntityList = editEntity.translate(mPickEnt, mPoints[0], mPoints[1], true);
                            else if (mPoints.Count == 1)
                                mEditEntityList = editEntity.translate(mPickEnt, new Point3D(0,0,0), mPoints[0], true);
                            else
                                break;
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "rotate":
                        if (2 < mPoints.Count && 0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.rotate(mPickEnt, mPoints[0], mPoints[1], mPoints[2], face, true);
                            ret = EXETYPE.editEntity;
                        } else if (0 < mAng && 0 < mPoints.Count && 0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.rotate(mPickEnt, mPoints[0], mAng, face, true);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "mirror":
                        if (1 < mPoints.Count && 0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.mirror(mPickEnt, mPoints[0], mPoints[1], face, true);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "offset":
                        break;
                    case "trim":
                        if (1 < mPoints.Count && 0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.trim(mPickEnt, mPoints[0], mPoints[1], face, true);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "scaling":
                        if (1 != mScale && 0 != mScale && 0 < mPoints.Count && 0 < mPickEnt.Count) {
                            Point3D sp = mPoints[0] + new Point3D(1);
                            Point3D ep = mPoints[0] + new Point3D(mScale);
                            mEditEntityList = editEntity.scale(mPickEnt, mPoints[0], sp, ep, face, true);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "divide":
                        if (0 < mPoints.Count && 0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.divide(mPickEnt, mPoints[0], face);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "fillet":
                        break;
                    case "connect":
                        if (0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.connect(mPickEnt, face);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "disassemble":
                        if (0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.disassemble(mPickEnt);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "extrusion":
                        if (0 < mPickEnt.Count) {
                            if (1 < mPoints.Count)
                                mEditEntityList = editEntity.extrusion(mPickEnt, mPoints[0], mPoints[1], true);
                            else if (mPoints.Count == 1)
                                mEditEntityList = editEntity.extrusion(mPickEnt, new Point3D(0, 0, 0), mPoints[0], true);
                            else
                                break;
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "blend":
                        if (1 < mPickEnt.Count) {
                            mEditEntityList = editEntity.blend(mPickEnt, true);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "revolution":
                        if (1 < mPickEnt.Count) {
                            mEditEntityList = editEntity.revolution(mPickEnt, mSa, mEa, true);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "sweep":
                        if (1 < mPickEnt.Count) {
                            mEditEntityList = editEntity.sweep(mPickEnt, mSa, mEa, true);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "release":
                        if (0 < mPickEnt.Count) {
                            mEditEntityList = editEntity.release(mPickEnt);
                            ret = EXETYPE.editEntity;
                        }
                        break;
                    case "remove":
                        return EXETYPE.editEntity;
                    case "undo":
                        ret = EXETYPE.undo;
                        break;
                    case "redo":
                        ret = EXETYPE.redo;
                        break;
                    case "close":
                        ret = EXETYPE.close;
                        break;
                    default:
                        ret = EXETYPE.non;
                        break;
                }
                if (mValString == "copy")
                    mCopy = true;
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"execCommand: {e.Message}");
                ret = EXETYPE.non;
            }
            if (ret == EXETYPE.createEntity && mEntity == null)
                ret = EXETYPE.non;
            else if (ret == EXETYPE.editEntity && mEditEntityList == null && mEditEntityList.Count == 0)
                ret = EXETYPE.non;
            return ret;
        }


        /// <summary>
        /// パラメータの抽出
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <param name="face">作成面</param>
        /// <returns>コマンドNo</returns>
        private int getCommandParameter(string command, FACE3D face)
        {
            int commandNo = -1;
            List<string> cmd = commandSplit(command);
            for (int i = 0; i < cmd.Count; i++) {
                if (0 > cmd[i].IndexOf("\""))
                    cmd[i] = cmd[i].ToLower();
                if (commandNo < 0) {
                    //  コマンド検索
                    commandNo = mMainCmd.FindIndex(p => 0 <= p.IndexOf(cmd[i]));
                } else {
                    (string name, string val) = splitPara(cmd[i]);
                    if (0 == name.IndexOf("x") || 0 == name.IndexOf("y") || 0 == name.IndexOf("z") ||
                        0 == name.IndexOf("dx") || 0 == name.IndexOf("dy") || 0 == name.IndexOf("dz")) {
                        //  座標/相対座標
                        Point3D dp = getPoint(cmd[i],
                            mPoints.Count < 1 ? new Point3D(0, 0, 0) : mPoints[mPoints.Count - 1], face);
                        if (!dp.isNaN())
                            mPoints.Add(dp);
                    } else if (0 == name.IndexOf("p")) {
                        //  要素番号
                        mPickEnt.Add(new PickData((int)ycalc.expression(val), new PointD(0, 0), face));
                    } else if (0 == name.IndexOf("r")) {
                        //  半径
                        mRadius = ycalc.expression(val);
                    } else if (0 == name.IndexOf("sa")) {
                        //  始角
                        mSa = ycalc.expression(val);
                    } else if (0 == name.IndexOf("ea")) {
                        //  終角
                        mEa = ycalc.expression(val);
                    } else if (0 == name.IndexOf("ang")) {
                        //  角度
                        mAng = ycalc.expression(val);
                    } else if (0 == name.IndexOf("scale")) {
                        //  倍率
                        mScale = ycalc.expression(val);
                    } else if (char.IsDigit(cmd[i][0]) || cmd[i][0] == '-') {
                        //  数値
                        mValue = ylib.string2double(cmd[i]);
                    } else if (cmd[i][0] == '"') {
                        //  文字列
                        mTextString = cmd[i].Trim('"');
                    } else {
                        //  その他の文字列
                        mValString = cmd[i];
                    }

                }
            }
            return commandNo;
        }

        /// <summary>
        /// パラメータ文字列からパラメータ名と値を分離
        /// </summary>
        /// <param name="para">パラメータ文字列</param>
        /// <returns>(パラメータ名,値)</returns>
        private (string name, string val) splitPara(string para)
        {
            string name = "";
            string val = "";
            string[] coords = { "x", "y", "z", "dx", "dy", "dz" };
            for (int i = 0; i < para.Length; i++) {
                if (!Char.IsLetter(para[i])) {
                    val = para.Substring(i);
                    break;
                } else
                    name += para[i].ToString();
            }
            if (0 <= coords.FindIndex(name)) {
                //  座標データ(個別処理)
                return (para, "");
            } else {
                int n = mParaName.FindIndex(p => 0 == p.IndexOf(name));
                if (0 <= n)
                    name = mParaName[n];
                return (name, val);
            }
        }

        /// <summary>
        /// 座標データの取得(相対座標は前回値から絶対座標に変換)
        /// </summary>
        /// <param name="xyz">座標データ文字列</param>
        /// <param name="prev">前回座標</param>
        /// <param name="face">作成面</param>
        /// <returns>座標</returns>
        private Point3D getPoint(string xyz, Point3D prev, FACE3D face)
        {
            Point3D p = new Point3D();
            bool zflag = false;
            string[] sep = { "x", "y", "z", "dx", "dy", "dz", ",", " " };
            List<string> list = ylib.splitString(xyz, sep);
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == "x" && i + 1 < list.Count) {
                    p.x = ycalc.expression(list[++i]);
                } else if (list[i] == "y" && i + 1 < list.Count) {
                    p.y = ycalc.expression(list[++i]);
                } else if (list[i] == "z" && i + 1 < list.Count) {
                    p.z = ycalc.expression(list[++i]);
                    zflag = true;
                } else if (list[i] == "dx" && i + 1 < list.Count) {
                    p.x = ycalc.expression(list[++i]) + prev.x;
                } else if (list[i] == "dy" && i + 1 < list.Count) {
                    p.y = ycalc.expression(list[++i]) + prev.y;
                } else if (list[i] == "dz" && i + 1 < list.Count) {
                    p.z = ycalc.expression(list[++i]) + prev.z;
                    zflag = true;
                }
            }
            if (!zflag) {
                PointD pos = new PointD(p.x, p.y);
                p = new Point3D(pos, face);
            }
            return p;
        }

        /// <summary>
        /// コマンド文字列から
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private List<string> commandSplit(string command)
        {
            List<string> cmd = new List<string>();
            string buf = "";
            for (int i = 0; i < command.Length; i++) {
                if (command[i] == ' ' || command[i] == ',') {
                    if (0 < buf.Length) {
                        cmd.Add(buf);
                        buf = "";
                    }
                } else if (command[i] == '"') {
                    if (0 < buf.Length) {
                        cmd.Add(buf);
                        buf = "";
                    }
                    buf += command[i++];
                    do {
                        buf += command[i];
                    } while (i < command.Length - 1 && command[i++] != '"');
                    cmd.Add(buf);
                    buf = "";
                } else {
                    buf += command[i];
                }
            }
            if (0 < buf.Length)
                cmd.Add(buf);
            return cmd;
        }
    }
}
