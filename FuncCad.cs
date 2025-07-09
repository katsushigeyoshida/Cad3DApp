using CoreLib;

namespace Cad3DApp
{
    public class FuncCad
    {
        public static string[] mFuncNames = new string[] {
            "cad.disp(); 表示",
            "cad.setColor(\"Blue\"); 色の設定",
            "cad.setLineType(\"dash\"); 線種の設定(\"solid\", \"dash\", \"center\", \"phantom\")",
            "cad.setLineThickness(2); 線の太さの設定",
            "cad.line(xs,ys,zs,xe,ye,ze); 線分を作成",
            "cad.line(sp[],ep); 線分を作成",
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

        public Token cadFunc(Token funcName, Token arg, Token ret)
        {
            List<Token> args = mScript.getFuncArgs(arg.mValue);
            switch (funcName.mValue) {
                case "cad.disp": disp(); break;
                case "cad.setColor": setColor(args); break;
                case "cad.setLineType": setLineType(args); break;
                case "cad.setLineThickness": setLineThickness(args); break;
                case "cad.line": line(args); break;
                default: return new Token("not found func", TokenType.ERROR);
            }
            return new Token("", TokenType.EMPTY);
        }

        private void disp()
        {
            mGlobal.mMainWindow.mDataManage.commandClear();
        }

        private void setColor(List<Token> args)
        {
            if (0 < args.Count) {
                string colorName = ylib.stripBracketString(args[0].mValue, '"');
                //mGlobal.mMainWindow.cbColor.SelectedIndex = ylib.getBrushNo(ylib.getColor(colorName));
                mGlobal.mEntityBrush = ylib.getColor(colorName);
            }
        }

        private void setLineType(List<Token> args)
        {
            if (0 < args.Count) {
                string lineType = ylib.stripBracketString(args[0].mValue, '"');
                mGlobal.mLineType = ydraw.mLineTypeName.FindIndex(lineType);
            }
        }

        private void setLineThickness(List<Token> args)
        {
            if (0 < args.Count) {
                string thickness = ylib.stripBracketString(args[0].mValue, '"');
                mGlobal.mLineThickness = ylib.doubleParse(thickness);
            }
        }

        private void line(List<Token> args)
        {
            List<double> datas = new List<double>();
            Point3D sp = null;
            Point3D ep = null;
            if (1 < args.Count && mVar.getArrayOder(args[0]) == 1 && mVar.getArrayOder(args[1]) == 1) {
                List<double> spList = mVar.cnvListDouble(args[0]);
                List<double> epList = mVar.cnvListDouble(args[1]);
                sp = new Point3D(spList[0], spList[1], spList[2]);
                ep = new Point3D(epList[0], epList[1], epList[2]);
            } else if (6 <= args.Count) {
                for (int i = 0; i < args.Count; i++)
                    if (mVar.getArrayOder(args[i]) == 0)
                        datas.Add(ylib.doubleParse(args[i].mValue));
                if (6 <= datas.Count) {
                    sp = new Point3D(datas[0], datas[1], datas[2]);
                    ep = new Point3D(datas[3], datas[4], datas[5]);
                }
            }
            if (sp != null && ep != null) {
                Entity entity = mCreateEntity.createLine(sp, ep, true);
                mEditEntity.addEntity(entity, ++mGlobal.mOperationCount);
                mGlobal.mMainWindow.mDataManage.updateArea();
            }
        }
    }
}
