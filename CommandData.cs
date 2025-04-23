namespace Cad3DApp
{
    /// <summary>
    /// 操作モード
    /// </summary>
    public enum OPEMODE
    {
        non, pick, loc, areaDisp, areaPick, exec, clear, close, reload, updateData
    }

    /// <summary>
    /// 操作コマンドコード
    /// </summary>
    public enum OPERATION
    {
        non, loc, pick,
        point, line, circle, arc, rect, polyline, polygon,
        translate, rotate, offset, mirror, trim, stretch, stretchArc, scale,
        fillet, connect, divide, disassemble, changeProperty, changePropertyAll, changeEntityData,
        copyTranslate, copyRotate, copyOffset, copyMirror, copyTrim, copyScale, copyEntity, pasteEntity,
        extrusion, blend, revolution, sweep, release,
        measure, measureDistance, measureAngle,
        zumenComment, dispLayer, addLayer, removeLayer, disp2DAll, disp3DAll, info,
        remove, undo, redo,
        setGrid, setColor,
        screenCopy, screenSave, imageTrimming, memo,
        save, load, back, cancel, close, systemProperty
    }

    /// <summary>
    /// コマンドレベル
    /// </summary>
    public enum COMMANDLEVEL
    {
        non, main, sub
    }

    public class Command
    {
        public string mMainCommand;
        public string mSubCommand;
        public OPERATION mOperation;


        public Command(string main, string sub, OPERATION ope)
        {
            mMainCommand = main;
            mSubCommand = sub;
            mOperation = ope;
        }

    }

    public class CommandData
    {
        public List<Command> mCommandData = new() {
            //new Command("作成",       "点",           OPERATION.point),
            new Command("作成",       "線分",         OPERATION.line),
            new Command("作成",       "円",           OPERATION.circle),
            new Command("作成",       "円弧",         OPERATION.arc),
            new Command("作成",       "四角",         OPERATION.rect),
            new Command("作成",       "ポリライン",   OPERATION.polyline),
            new Command("作成",       "ポリゴン",     OPERATION.polygon),
            new Command("作成",       "戻る",         OPERATION.back),
            new Command("2D編集",     "移動",         OPERATION.translate),
            new Command("2D編集",     "回転",         OPERATION.rotate),
            new Command("2D編集",     "オフセット",   OPERATION.offset),
            new Command("2D編集",     "反転",         OPERATION.mirror),
            new Command("2D編集",     "トリム",       OPERATION.trim),
            new Command("2D編集",     "ストレッチ",   OPERATION.stretch),
            new Command("2D編集",     "拡大縮小",     OPERATION.scale),
            new Command("2D編集",     "分解",         OPERATION.disassemble),
            new Command("2D編集",     "分割",         OPERATION.divide),
            new Command("2D編集",     "Ｒ面",         OPERATION.fillet),
            new Command("2D編集",     "接続",         OPERATION.connect),
            new Command("2D編集",     "属性変更",     OPERATION.changeProperty),
            //new Command("2D編集",     "一括属性変更", OPERATION.changePropertyAll),
            new Command("2D編集",     "戻る",         OPERATION.back),
            new Command("2Dコピー",   "移動",         OPERATION.copyTranslate),
            new Command("2Dコピー",   "回転",         OPERATION.copyRotate),
            new Command("2Dコピー",   "オフセット",   OPERATION.copyOffset),
            new Command("2Dコピー",   "反転",         OPERATION.copyMirror),
            new Command("2Dコピー",   "トリム",       OPERATION.copyTrim),
            new Command("2Dコピー",   "拡大縮小",     OPERATION.copyScale),
            new Command("2Dコピー",   "要素コピー",   OPERATION.copyEntity),
            new Command("2Dコピー",   "要素貼付け",   OPERATION.pasteEntity),
            new Command("2Dコピー",   "戻る",         OPERATION.back),
            new Command("3D編集",     "押出",         OPERATION.extrusion),
            new Command("3D編集",     "ブレンド",     OPERATION.blend),
            new Command("3D編集",     "回転体",       OPERATION.revolution),
            new Command("3D編集",     "掃引",         OPERATION.sweep),
            new Command("3D編集",     "解除",         OPERATION.release),
            new Command("3D編集",     "戻る",         OPERATION.back),
            new Command("設定",       "図面コメント", OPERATION.zumenComment),
            new Command("設定",       "表示レイヤ",   OPERATION.dispLayer),
            new Command("設定",       "非表示解除",   OPERATION.disp2DAll),
            //new Command("設定",       "色設定",       OPERATION.setColor),
            //new Command("設定",       "グリッド設定", OPERATION.setGrid),
            new Command("設定",       "システム設定", OPERATION.systemProperty),
            new Command("設定",       "戻る",         OPERATION.back),
            new Command("計測",       "距離",         OPERATION.measureDistance),
            new Command("計測",       "角度",         OPERATION.measureAngle),
            new Command("計測",       "距離・角度",   OPERATION.measure),
            new Command("計測",       "戻る",         OPERATION.back),
            new Command("情報",       "情報",         OPERATION.info),
            new Command("削除",       "削除",         OPERATION.remove),
            new Command("アンドゥ",   "アンドゥ",     OPERATION.undo),
            new Command("リドゥ",     "リドゥ",       OPERATION.redo),
            //new Command("ファイル",   "保存",         OPERATION.save),
            //new Command("ファイル", "読込",         OPERATION.load),
            new Command("ツール",     "画面コピー",   OPERATION.screenCopy),
            //new Command("ツール",     "画面保存",     OPERATION.screenSave),
            //new Command("ツール",    "イメージトリミング", OPERATION.imageTrimming),
            new Command("ツール",     "メモ",         OPERATION.memo),
            new Command("ツール",     "戻る",         OPERATION.back),
            new Command("キャンセル", "キャンセル",   OPERATION.cancel),
            new Command("終了",       "終了",         OPERATION.close),
        };
        private string mMainCommand = "";

        /// <summary>
        /// メインコマンドリストの取得
        /// </summary>
        /// <returns>コマンドリスト</returns>
        public List<string> getMainCommand()
        {
            mMainCommand = "";
            List<string> main = new List<string>();
            foreach (var cmd in mCommandData) {
                if (!main.Contains(cmd.mMainCommand) && cmd.mMainCommand != "")
                    main.Add(cmd.mMainCommand);
            }
            return main;
        }

        /// <summary>
        /// サブコマンドリストの取得
        /// </summary>
        /// <param name="main">メインコマンド</param>
        /// <returns>コマンドリスト</returns>
        public List<string> getSubCommand(string main)
        {
            mMainCommand = main;
            List<string> sub = new List<string>();
            foreach (var cmd in mCommandData) {
                if (cmd.mMainCommand == main || cmd.mMainCommand == "") {
                    if (!sub.Contains(cmd.mSubCommand))
                        sub.Add(cmd.mSubCommand);
                }
            }
            return sub;
        }

        /// <summary>
        /// コマンドレベルの取得
        /// </summary>
        /// <param name="menu">コマンド名</param>
        /// <returns>コマンドレベル</returns>
        public COMMANDLEVEL getCommandLevl(string menu)
        {
            int n = mCommandData.FindIndex(p => p.mSubCommand == menu);
            if (0 <= n)
                return COMMANDLEVEL.sub;
            n = mCommandData.FindIndex(p => p.mMainCommand == menu);
            if (0 <= n)
                return COMMANDLEVEL.main;
            return COMMANDLEVEL.non;
        }

        /// <summary>
        /// コマンドコードの取得
        /// </summary>
        /// <param name="menu">サブコマンド名</param>
        /// <returns>コマンドコード</returns>
        public OPERATION getCommand(string menu)
        {
            if (0 <= mCommandData.FindIndex(p => (mMainCommand == "" || p.mMainCommand == mMainCommand) && p.mSubCommand == menu)) {
                Command com = mCommandData.Find(p => (mMainCommand == "" || p.mMainCommand == mMainCommand) && p.mSubCommand == menu);
                return com.mOperation;
            }
            return OPERATION.non;
        }
    }
}
