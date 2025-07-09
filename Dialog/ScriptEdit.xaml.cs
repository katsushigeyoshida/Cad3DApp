using CoreLib;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;

namespace Cad3DApp
{
    /// <summary>
    /// ScriptEdit.xaml の相互作用ロジック
    /// </summary>
    public partial class ScriptEdit : Window
    {
        private double mWindowWidth;            //  ウィンドウの高さ
        private double mWindowHeight;           //  ウィンドウ幅

        private double mFontSize = 12;          //  AvalonEditorのフォントサイズ
        private string mFontFamily = "";        //  AvalonEditorのフォントファミリ
        private string mScriptFolder = "";      //  スクリプトファイルのフォルダ
        private string mScriptPath = "";        //  スクリプトファイルパス
        private byte[] mSrcHash;                //  スクリプトの読み込み時ハッシュコード

        private RingBuffer<string> mOutBuffer = new RingBuffer<string>(2000);   //  出力表示のバッファ

        public List<Entity> mEntityList = new List<Entity>();   //  要素リスト
        public GlobalData mGlobal;                              //  グローバルデータ

        private KScript mScript;                //  スクリプト本体
        private List<string> mFunctList = new List<string> {
            "cad.",
        };
        private FuncCad mFuncCad;
        private Snippet mSnippet = new Snippet();   //  スニペット
        private YLib ylib = new YLib();

        public ScriptEdit(List<Entity> entityList, GlobalData global)
        {
            InitializeComponent();

            mEntityList = entityList;
            mGlobal = global;

            //  FontFamilyの設定
            cbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);

            //  snippet の読込
            setSnippetKeyword();

            //  KScriptの設定
            mScript = new KScript();
            mScript.mOutFuncList = mFunctList;      //  関数名設定
            mScript.printCallback = outputDisp;     //  出力表示先の設定
            mScript.funcCallback = function;        //  関数設定
            mFuncCad = new FuncCad(mScript, mEntityList, mGlobal);
            setTitle();

            WindowFormLoad();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closeCheck();
            WindowFormSave();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int index = -1;
            if (mFontFamily == "") mFontFamily = "MS Gothic";
            for (int i = 0; i < cbFontFamily.Items.Count; i++) {
                if (cbFontFamily.Items[i].ToString() == mFontFamily)
                    index = i;
            }
            if (0 <= index)
                cbFontFamily.SelectedIndex = index;
            if (mFontSize == 0)
                mFontSize = 12;
            avalonEditor.FontSize = mFontSize;
            tbOutput.FontFamily = new System.Windows.Media.FontFamily("MS Gothic");
            tbOutput.FontSize = mFontSize;
        }

        /// <summary>
        /// 状態の復元
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.ScriptWindowWidth < 100 ||
                Properties.Settings.Default.ScriptWindowHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.ScriptWindowHeight) {
                Properties.Settings.Default.ScriptWindowWidth = mWindowWidth;
                Properties.Settings.Default.ScriptWindowHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.ScriptWindowTop;
                Left = Properties.Settings.Default.ScriptWindowLeft;
                Width = Properties.Settings.Default.ScriptWindowWidth;
                Height = Properties.Settings.Default.ScriptWindowHeight;
            }
            mScriptFolder = Properties.Settings.Default.ScriptFileFolder;
            setScriptFilrList(mScriptFolder);
            double fontSize = Properties.Settings.Default.ScriptEditerFontSize;
            if (0 < fontSize) mFontSize = fontSize;
            string fontFamily = Properties.Settings.Default.ScriptEditerFontFamily;
            if (0 < fontFamily.Length) mFontFamily = fontFamily;
        }

        /// <summary>
        /// 状態の保存
        /// </summary>
        private void WindowFormSave()
        {
            Properties.Settings.Default.ScriptFileFolder = mScriptFolder;
            Properties.Settings.Default.ScriptEditerFontSize = mFontSize;
            Properties.Settings.Default.ScriptEditerFontFamily = mFontFamily;
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.ScriptWindowTop = Top;
            Properties.Settings.Default.ScriptWindowLeft = Left;
            Properties.Settings.Default.ScriptWindowWidth = Width;
            Properties.Settings.Default.ScriptWindowHeight = Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// キー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (keyCommand(e.Key, e.KeyboardDevice.Modifiers == ModifierKeys.Control, e.KeyboardDevice.Modifiers == ModifierKeys.Shift))
                e.Handled = true;   //  イベントキャンセル
            else
                e.Handled = false;
        }

        /// <summary>
        /// フォント変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFontFamily.SelectedItem != null) {
                avalonEditor.FontFamily = (System.Windows.Media.FontFamily)cbFontFamily.SelectedItem;
                mFontFamily = cbFontFamily.SelectedItem.ToString();
                tbOutput.FontFamily = (System.Windows.Media.FontFamily)cbFontFamily.SelectedItem;
            }
        }

        private void avalonEditor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        /// <summary>
        /// フォルダ選択のマウスダブルマリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbScriptFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("フォルダ選択", ".");
            setScriptFilrList(folder);
        }

        /// <summary>
        /// スクリプトファイル選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbScriptFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbScriptFile.SelectedIndex;
            if (0 <= index) {
                closeCheck();
                string path = cbScriptFile.SelectedItem.ToString();
                mScriptPath = Path.Combine(mScriptFolder, path) + ".sc";
                cbScriptFile.Items.Remove(path);
                cbScriptFile.Items.Insert(0, path);
                if (0 < index)
                    cbScriptFile.SelectedIndex = 0;
                cbScriptFile.Text = path;
                loadScript(mScriptPath);
            }
        }

        /// <summary>
        /// ボタン操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Name.CompareTo("btNew") == 0) {
                newScript();
            } else if (button.Name.CompareTo("btOpen") == 0) {
                //selectLoad();
            } else if (button.Name.CompareTo("btSave") == 0) {
                save();
            } else if (button.Name.CompareTo("btSaveAs") == 0) {
                saveAs();
            } else if (button.Name.CompareTo("btFontUp") == 0) {
                mFontSize += 1;
                avalonEditor.FontSize = mFontSize;
                tbOutput.FontSize = mFontSize;
            } else if (button.Name.CompareTo("btFontDown") == 0) {
                mFontSize -= 1;
                avalonEditor.FontSize = mFontSize;
                tbOutput.FontSize = mFontSize;
            } else if (button.Name.CompareTo("btExecute") == 0) {
                if (mScript.mControlData.mPause)
                    mScript.mControlData.mPause = false;
                else
                    exeute();
            } else if (button.Name.CompareTo("btAbort") == 0) {
                mScript.mControlData.mAbort = true;
                outMessage("Abort\n");
            } else if (button.Name.CompareTo("btPause") == 0) {
                mScript.mControlData.mPause = !mScript.mControlData.mPause;
                if (mScript.mControlData.mPause)
                    outMessage("Pause\n");
            } else if (button.Name.CompareTo("btSearch") == 0) {
                //search();
            } else if (button.Name.CompareTo("btHelp") == 0) {
                //ylib.openUrl(mHelpFile);
            }
        }

        /// <summary>
        /// 新規作成
        /// </summary>
        private void newScript()
        {
            closeCheck();
            avalonEditor.Text = "";
            mScriptPath = "";
            setTitle();
            //mSearchWordIndex = 0;
        }

        /// <summary>
        /// スクリプトの実行
        /// </summary>
        private void exeute()
        {
            outMessage($"Start : [{Path.GetFileNameWithoutExtension(mScriptPath)}]");
            mScript.mControlData.mAbort = false;
            mScript.mControlData.mPause = false;
            mScript.mScriptFolder = mScriptFolder;
            mScript.setScript(avalonEditor.Text);
            mScript.execute("main");
            outMessage($"End : [{Path.GetFileNameWithoutExtension(mScriptPath)}]");
        }

        /// <summary>
        /// 外部関数
        /// </summary>
        public void function()
        {
            if (0 == mScript.mOutFuncName.mValue.IndexOf("cad."))
                mScript.mOutFuncRet = mFuncCad.cadFunc(mScript.mOutFuncName, mScript.mOutFuncArg, mScript.mOutFuncRet); //  関数
        }

        /// <summary>
        /// メッセージを出力領域に出す
        /// </summary>
        /// <param name="msg"></param>
        private void outMessage(string msg)
        {
            string buf = $"[{DateTime.Now.ToString("HH:mm:ss")}] {msg}\n";
            outputDisp(buf);
        }

        /// <summary>
        /// 出力ウィンドウに表示(callback用)
        /// </summary>
        public void outputDisp()
        {
            outputDisp(mScript.mControlData.mOutputString);
        }

        /// <summary>
        /// 出力ウィンドウに表示
        /// </summary>
        /// <param name="str">文字列</param>
        public void outputDisp(string output)
        {
            mOutBuffer.Add(output);
            string buf = "";
            foreach (var text in mOutBuffer)
                buf += text;
            tbOutput.Text = buf;
            tbOutput.Select(tbOutput.Text.Length, 0);
            tbOutput.ScrollToEnd();
            ylib.DoEvents();
        }

        /// <summary>
        /// スクリプトファイルリストの登録
        /// </summary>
        /// <param name="folder">スクリプトファイルフォルダ</param>
        private void setScriptFilrList(string folder)
        {
            if (0 < folder.Length) {
                mScriptFolder = folder;
                string[] files = ylib.getFiles(Path.Combine(folder, "*.sc"));
                cbScriptFile.Items.Clear();
                foreach (string file in files) {
                    cbScriptFile.Items.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
        }

        /// <summary>
        /// タイトル設定
        /// </summary>
        private void setTitle()
        {
            Title = $"KScriptWin [ {Path.GetFileNameWithoutExtension(mScriptPath)} ]";
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="key">キーコード</param>
        /// <param name="control">Cntrolの有無</param>
        /// <param name="shift">Shiftの有無</param>
        private bool keyCommand(Key key, bool control, bool shift)
        {
            if (mScript.mControlData.mKey) {
                mScript.mControlData.mKeyCode = key;
                mScript.mControlData.mKey = false;
                return true;
            }
            if (control && shift) {
                switch (key) {
                    //case Key.S: saveAs(); break;
                    default: return false; ;
                }
            } else if (control) {
                switch (key) {
                    case Key.D:
                        mScript.mControlData.mPause = !mScript.mControlData.mPause;
                        outMessage("Pause\n");
                        break;
                    //case Key.F: search(); break;
                    case Key.N: newScript(); break;
                    //case Key.O: selectLoad(); break;
                    case Key.S: save(); break;
                    case Key.Divide: toComment(); break;
                    case Key.OemQuestion: toComment(); break;
                    default: return false; ;
                }
            } else {
                switch (key) {
                    case Key.F5: exeute(); break;       //  実行
                    //case Key.F8: snippet(); break;      //  入力候補
                    //case Key.F12: updateSnippetKeyWird(avalonEditor.Text); break;
                    case Key.Apps: snippet(); break;    //  入力候補
                    default: return false; ;
                }
            }
            return true;
        }

        /// <summary>
        /// snippets 予約語の登録
        /// </summary>
        /// <param name="filepath">追加ファイルパス</param>
        public void setSnippetKeyword(List<string> files = null)
        {
            if (mSnippet == null)
                mSnippet = new Snippet();
            else
                mSnippet.clear();
            mSnippet.add(KScript.mStatmantHelp);
            mSnippet.add(ScriptLib.mFuncNames);
            mSnippet.add(FuncArray.mFuncNames);
            mSnippet.add(FuncString.mFuncNames);
            mSnippet.add(FuncFile.mFuncNames);
            mSnippet.add(FuncCad.mFuncNames);
            mSnippet.add(YCalc.mFuncList);
            mSnippet.add(YDraw.mLineTypeHelp);
            mSnippet.add(YDraw.mPointTypeHelp);
            mSnippet.addColorList();
            if (files != null)
                mSnippet.add(files);
        }

        /// <summary>
        /// スニペット(カーソル位置の手前の単語から予約語などをメニュー表示する)
        /// </summary>
        private void snippet()
        {
            (string text, int selectStart) = mSnippet.showDialog(this, avalonEditor.Text, avalonEditor.SelectionStart);
            if (0 <= selectStart) {
                avalonEditor.Document.Text = text;
                avalonEditor.Select(selectStart, 0);
            }
        }

        /// <summary>
        /// スクリプトファイルを読み込む
        /// </summary>
        /// <param name="path">パス</param>
        private void loadScript(string path)
        {
            if (!File.Exists(path) || Path.GetExtension(path).ToLower() != ".sc") return;
            string text = ylib.loadTextFile(path);
            mScriptPath = path;
            avalonEditor.Text = text;
            setHash(text);
            setTitle();
        }

        /// <summary>
        /// スクリプトをファイルに保存
        /// </summary>
        private void save()
        {
            if (0 < mScriptPath.Length) {
                ylib.saveTextFile(mScriptPath, avalonEditor.Text);
                setHash(avalonEditor.Text);
            } else
                saveAs();
        }

        /// <summary>
        /// スクリプトを名前を付けてファイルに保存
        /// </summary>
        private void saveAs()
        {
            List<string[]> filters = new List<string[]>() { new string[] { "scファイル", "*.sc" } };
            mScriptPath = ylib.fileSaveSelectDlg("ファイル保存", mScriptFolder, filters);
            if (0 < mScriptPath.Length) {
                string ext = Path.GetExtension(mScriptPath);
                if (ext == "")
                    mScriptPath += ".sc";
                ylib.saveTextFile(mScriptPath, avalonEditor.Text);
                mScriptFolder = Path.GetDirectoryName(mScriptPath);
                setHash(avalonEditor.Text);
                setTitle();
            }
        }

        /// <summary>
        /// 文字列のハッシュ値を保存
        /// </summary>
        /// <param name="text"></param>
        private void setHash(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            MD5 md5 = MD5.Create();
            mSrcHash = md5.ComputeHash(data);
            md5.Clear();
        }

        /// <summary>
        /// プログラムのクローズチェック
        /// </summary>
        private void closeCheck()
        {
            if ((mSrcHash != null && !compareHash(avalonEditor.Text)) ||
                (mSrcHash == null && 0 < avalonEditor.Text.Length)) {
                if (System.Windows.MessageBox.Show("内容が変更されていますが保存しますか?", "確認", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    save();
                }
            }
        }

        /// <summary>
        /// ハッシュ値の比較
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <returns></returns>
        private bool compareHash(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            MD5 md5 = MD5.Create();
            byte[] tempHash = md5.ComputeHash(data);
            return mSrcHash.SequenceEqual(tempHash);
        }


        /// <summary>
        /// コメントの追加/解除
        /// </summary>
        private void toComment()
        {
            bool remove = false;
            string text = avalonEditor.Text;
            int selSp = avalonEditor.SelectionStart;
            int selEp = selSp + avalonEditor.SelectionLength;
            //  改行位置と選択行位置を求める
            int stLine = -1;
            int endLine = -1;
            List<int> crList = getCrList(text);   //  改行位置
            for (int i = 0; i < crList.Count - 1; i++) {
                if (crList[i] <= selSp && selSp < crList[i + 1])
                    stLine = i;
                if (crList[i] <= selEp && selEp < crList[i + 1])
                    endLine = i;
            }
            if (stLine < 0) stLine = crList.Count - 2;
            if (endLine < 0) endLine = crList.Count - 2;
            //  選択開始行のコメントの有無を確認
            string strLine = text.Substring(crList[stLine],
                stLine < crList.Count - 1 ? crList[stLine + 1] - crList[stLine] : text.Length - crList[stLine]);
            int index = strLine.ToList().FindIndex(c => (c != ' ' && c != '\t'));
            if (index < strLine.Length - 1 && strLine[index] == '/' && strLine[index + 1] == '/')
                remove = true;
            //  コメント化/コメント解除
            for (int i = crList.Count - 2; 0 <= i; i--) {
                if ((selSp < crList[i + 1] || selSp == avalonEditor.Document.TextLength)
                    && crList[i] <= selEp) {
                    if (remove)
                        removeComment(crList[i], selEp);
                    else
                        avalonEditor.Document.Insert(crList[i], "//");
                }
            }
            selSp = Math.Max(0, Math.Min(selSp, avalonEditor.Document.TextLength));
            avalonEditor.Select(selSp, 0);
        }

        /// <summary>
        /// テキストの改行位置をリスト化する
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <returns>改行位置リスト</returns>
        private List<int> getCrList(string text)
        {
            List<int> crList = new List<int>() { 0 };   //  改行位置
            for (int i = 0; i < text.Length; i++) {
                if (text[i] == '\n')
                    crList.Add(i + 1);
            }
            crList.Add(text.Length);
            return crList;
        }


        /// <summary>
        /// 指定位置のコメント解除
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <param name="sp">位置</param>
        /// <returns>解除後のテキスト</returns>
        private void removeComment(int sp, int ep)
        {
            for (int i = sp; i <= ep && i < avalonEditor.Document.Text.Length - 1; i++) {
                if (avalonEditor.Document.Text[i] == '/' && avalonEditor.Document.Text[i + 1] == '/') {
                    avalonEditor.Document.Remove(i, 2);
                    break;
                }
            }
        }
    }
}
