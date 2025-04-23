using CoreLib;
using OpenTK;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using Point = System.Windows.Point;

namespace Cad3DApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// OpenGL を使う
    ///     NuGet で OpenTK 3.x OpenTK.GLControl 3.xをインストール
    ///     OpenTK 3.Xを使うときは WindowsFormsにチェックを入れる
    ///     WindowsFormsHost を使う
    ///         WindowsFormsHost は一度ツールボックスから設定した後
    ///         xaml ファイルを再表示してのち 設定する
    ///     (OpenTK 4..X は .NET,WPF 用であるが SH67H3では使えにい KIRAでは使える)
    /// </summary>
    public partial class MainWindow : Window
    {
        private double mWindowWidth;                        //  ウィンドウの高さ
        private double mWindowHeight;                       //  ウィンドウ幅
        private double mPrevWindowWidth;                    //  変更前のウィンドウ幅
        private System.Windows.WindowState mWindowState = System.Windows.WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        public string mAppName = "Cad3DApp";                //  アプリ名
        private Canvas mCurCanvas;                          //  描画キャンバス
        private System.Windows.Controls.Image mCurImage;    //  描画イメージ

        public double[] mGridSizeMenu = {                   //  グリッドサイズメニュー
            0, 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 1, 1.25, 1.5, 2, 2.5, 3, 4, 5, 10,
            20, 30, 40, 50, 100, 200, 300, 400, 500, 1000
        };
        private List<double> mFilletSizeMenu = new List<double>() { //  フィレットサイズメニュー
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 15, 16, 18, 20, 25, 30
        };

        private Point mPreMousePos;                         //  マウスの前回位置(screen座標)
        private PointD mPrePosition;                        //  マウスの前回位置(world座標)
        private bool mMouseLeftButtonDown = false;          //  左ボタン状態
        private bool mMouseRightButtonDown = false;         //  右ボタン状態
        private int mMouseScroolSize = 5;                   //  マウスによるスクロール単位

        public OPEMODE mOperationMode = OPEMODE.non;        //  操作モード(loc,pick)
        public OPEMODE mPrevOpeMode = OPEMODE.non;              //  操作モードの前回値
        public FACE3D mFace = FACE3D.FRONT;                 //  表示モード(FRONT/TOP/RIGHT/3D)

        public CommandData mCommandData;                    //  コマンドデータ
        public DataManage mDataManage;                      //  データ処理
        public FileData mFileData;                          //  ファイル管理

        public GL3DLib m3Dlib;                              //  三次元表示ライブラリ
        private GLControl glControl;                        //  OpenTK.GLcontrol
        private YLib ylib = new YLib();

        public MainWindow()
        {
            InitializeComponent();

            mAppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Title = mAppName;

            //  OpenGL 初期化
            glControl = new GLControl();
            m3Dlib = new GL3DLib(glControl, glGraph);
            m3Dlib.initPosition(1.3f, 0f, 0f, 0f);
            //  OpenGLイベント処理
            glControl.Load       += GlControl_Load;
            glControl.Paint      += GlControl_Paint;
            glControl.Resize     += GlControl_Resize;
            glControl.MouseDown  += GlControl_MouseDown;
            glControl.MouseUp    += GlControl_MouseUp;
            glControl.MouseMove  += GlControl_MouseMove;
            glControl.MouseWheel += GlControl_MouseWheel;
            glGraph.Child = glControl;

            mCommandData = new CommandData();
            mDataManage = new DataManage(this);
            mOperationMode = OPEMODE.pick;
            mFileData = new FileData(this, mDataManage.mGlobal);
            mDataManage.setFileData(mFileData);

            WindowFormLoad();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //  Canvas,Image,Faceの初期値設定
            mCurCanvas = cvCanvasFRONT;
            mCurImage = imScreenFRONT;
            mDataManage.setDataDraw(mCurCanvas, mCurImage);
            mDataManage.worldFrame();
            mDataManage.draw();
            //  コントロールの初期化
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            cbColor.DataContext = ylib.mBrushList;
            cbGridSize.ItemsSource = mGridSizeMenu;
            cbColor.SelectedIndex = ylib.getBrushNo(mDataManage.mGlobal.mEntityBrush);
            cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(Math.Abs(mDataManage.getGridSize()));
            mFilletSizeMenu.ForEach(p => cbFilletSize.Items.Add(p));
            cbFilletSize.SelectedIndex = 0;
            cbBaseLoc.IsChecked = true;
            //  データファイルの設定
            mFileData.setBaseDataFolder();
            setDataFileList();
            mDataManage.mDataPath = mFileData.getCurItemFilePath();
        }

        /// <summary>
        /// Windowの大きさ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            if (WindowState != mWindowState && WindowState == System.Windows.WindowState.Maximized) {
                //  ウィンドウの最大化時
                mWindowWidth = SystemParameters.WorkArea.Width;
                mWindowHeight = SystemParameters.WorkArea.Height;
            } else if (WindowState != mWindowState ||
                mWindowWidth != Width || mWindowHeight != Height) {
                //  ウィンドウサイズが変わった時
                mWindowWidth = Width;
                mWindowHeight = Height;
            } else {
                //  ウィンドウサイズが変わらない時は何もしない
                mWindowState = WindowState;
                return;
            }
            mWindowState = WindowState;
            if (mCurCanvas != null && mCurImage != null &&
                mDataManage != null && mDataManage.mDataDraw.mGlobal.mFace != FACE3D.NON) {
                mDataManage.worldFrame();
                mDataManage.draw();
            }
        }

        /// <summary>
        /// Windowクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mDataManage.mLayerChkListDlg != null)
                mDataManage.mLayerChkListDlg.Close();
            mDataManage.saveFile();
            WindowFormSave();
        }

        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.MainWindowWidth < 100 ||
                Properties.Settings.Default.MainWindowHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.MainWindowHeight) {
                Properties.Settings.Default.MainWindowWidth = mWindowWidth;
                Properties.Settings.Default.MainWindowHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.MainWindowTop;
                Left = Properties.Settings.Default.MainWindowLeft;
                Width = Properties.Settings.Default.MainWindowWidth;
                Height = Properties.Settings.Default.MainWindowHeight;
            }
            //  図面データ保存フォルダ
            if (0 < Properties.Settings.Default.BaseDataFolder.Length)
                mDataManage.mGlobal.mBaseDataFolder = Properties.Settings.Default.BaseDataFolder;
            if (0 < Properties.Settings.Default.BackupFolder.Length)
                mDataManage.mGlobal.mBackupFolder = Properties.Settings.Default.BackupFolder;
            mFileData.mDiffTool = Properties.Settings.Default.DiffTool;
            //  図面分類
            if (0 < Properties.Settings.Default.GenreName.Length)
                mFileData.mGenreName = Properties.Settings.Default.GenreName;
            if (0 < Properties.Settings.Default.CategoryName.Length)
                mFileData.mCategoryName = Properties.Settings.Default.CategoryName;
            if (0 < Properties.Settings.Default.DataName.Length)
                mFileData.mDataName = Properties.Settings.Default.DataName;
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  図面分類
            Properties.Settings.Default.BaseDataFolder = mDataManage.mGlobal.mBaseDataFolder;
            Properties.Settings.Default.BackupFolder = mDataManage.mGlobal.mBackupFolder;
            Properties.Settings.Default.GenreName = mFileData.mGenreName;
            Properties.Settings.Default.CategoryName = mFileData.mCategoryName;
            Properties.Settings.Default.DataName = mFileData.mDataName;
            Properties.Settings.Default.DiffTool = mFileData.mDiffTool;

            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.MainWindowTop = Top;
            Properties.Settings.Default.MainWindowLeft = Left;
            Properties.Settings.Default.MainWindowWidth = Width;
            Properties.Settings.Default.MainWindowHeight = Height;
            Properties.Settings.Default.Save();
        }

        private void GlControl_Load(object? sender, EventArgs e)
        {
            m3Dlib.initLight();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            //  Surfaceデータの取得と表示
            renderFrame();
        }

        private void GlControl_Resize(object? sender, EventArgs e)
        {
            m3Dlib.resize();
        }

        private void GlControl_MouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                m3Dlib.setMoveStart(true, e.X, e.Y);
            } else if (e.Button == MouseButtons.Right) {
                m3Dlib.setMoveStart(false, e.X, e.Y);
            }
        }

        private void GlControl_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            m3Dlib.setMoveEnd();
        }

        private void GlControl_MouseMove(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (m3Dlib.moveObject(e.X, e.Y)) {
                m3Dlib.setArea(mDataManage.mDataDraw.mArea);
                m3Dlib.renderFrame(mDataManage.getSurfaceData());
            }
        }

        private void GlControl_MouseWheel(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            float delta = (float)e.Delta / 1000f;// - wheelPrevious;
            m3Dlib.setZoom(delta);
            m3Dlib.setArea(mDataManage.mDataDraw.mArea);
            m3Dlib.renderFrame(mDataManage.getSurfaceData());

        }

        /// <summary>
        /// 3Dデータ表示
        /// </summary>
        public void renderFrame()
        {
            m3Dlib.setArea(mDataManage.mDataDraw.mArea);
            m3Dlib.renderFrame(mDataManage.getSurfaceData());
        }

        /// <summary>
        /// マウス左ボタンダウン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mDataManage.mDataDraw.mGlobal.mFace == FACE3D.NON) return;
            mMouseLeftButtonDown = true;
            Point pos = e.GetPosition(mCurCanvas);
            if (mOperationMode == OPEMODE.areaDisp || mOperationMode == OPEMODE.areaPick) {
                //  領域表示
                if (mDataManage.mDataDraw.areaOpe(new PointD(pos), mOperationMode))
                    mOperationMode = mPrevOpeMode;
            }
            if (mOperationMode == OPEMODE.loc) {
                //  ロケイトモード
                mDataManage.locate(new PointD(pos));
            } else if (mOperationMode == OPEMODE.pick) {
                //  ピックモード
            }
            if (mDataManage.definData(false, ylib.onAltKey()))
                commandCancel();
        }

        /// <summary>
        /// マウス左ボタンアップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mMouseLeftButtonDown = false;
        }

        /// <summary>
        /// マウス右ボタンダウン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mDataManage.mDataDraw.mGlobal.mFace == FACE3D.NON) return;
            mMouseRightButtonDown = true;
            Point pos = e.GetPosition(mCurCanvas);
            if (mOperationMode == OPEMODE.loc) {
                mDataManage.setBaseLoc(cbBaseLoc.IsChecked == true);
                //  オートロケイト
                bool lastloc = false;
                if (!mDataManage.locPick(new PointD(pos))) {
                    //  空ピックの場合コマンド実行(Polyline,Polygon)
                    lastloc = true;
                } else
                    if (!mDataManage.autoLocate(new PointD(pos)))
                        return;
                if (mDataManage.definData(lastloc, ylib.onAltKey()))
                    commandCancel();
            } else if (mOperationMode == OPEMODE.pick) {
                //  ピック
                if (mDataManage.pick(new PointD(pos)))
                    mDataManage.draw();
            }
            dispStatus(mDataManage.getWpos(new PointD(pos)));
        }

        /// <summary>
        /// マウス右ボタンアップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            mMouseRightButtonDown = false;
        }

        /// <summary>
        /// マウスの移動(ドラッギング表示,スクロール,領域指定))
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point pos = e.GetPosition(mCurCanvas);
            if (pos == mPreMousePos)
                return;
            PointD wpos;
            if ((mOperationMode == OPEMODE.areaDisp || mOperationMode == OPEMODE.areaPick)) {
                //  領域表示/選択
                wpos = mDataManage.areaDragging(new PointD(pos), mOperationMode);
            } else if (mMouseLeftButtonDown && ylib.onControlKey()) {
                //  2Dスクロール
                if (mMouseScroolSize < ylib.distance(pos, mPreMousePos)) {
                    wpos = mDataManage.scroll(new PointD(pos), new PointD(mPreMousePos));
                } else
                    return;
            } else {
                //  ドラッギング表示
                wpos = mDataManage.dragging(new PointD(pos), ylib.onAltKey());
            }
            dispStatus(wpos);
            mPreMousePos = pos;     //  スクリーン座標
            mPrePosition = wpos;    //  ワールド座標
        }

        /// <summary>
        /// マウスホィール(拡大縮小)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (0 != e.Delta) {
                //  2D表示
                double scaleStep = e.Delta > 0 ? 1.2 : 1 / 1.2;
                Point pos = e.GetPosition(mCurCanvas);
                PointD wpos = mDataManage.zoom(new PointD(pos), scaleStep);
            }
        }

        /// <summary>
        /// 要素上でマウスダブルクリック(属性表示,Alt+ 要素データ表示)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mOperationMode == OPEMODE.pick) {
                Point pos = e.GetPosition(mCurCanvas);
                if (mDataManage.pick(new PointD(pos))) {
                    if (ylib.onAltKey())
                        mDataManage.commandExec(OPERATION.changeEntityData);    //  要素データ表示
                    else
                        mDataManage.commandExec(OPERATION.changeProperty);      //  要素属性表示
                }
            }
        }

        /// <summary>
        /// キー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            keyCommand(e.Key, 
                e.KeyboardDevice.Modifiers == ModifierKeys.Control,
                e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
            //btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// コマンド選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCommand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbCommand.SelectedIndex;
            if (lbCommand.Items != null && 0 <= index) {
                string menu = lbCommand.Items[index].ToString() ?? "";
                COMMANDLEVEL level = mCommandData.getCommandLevl(menu);
                if (level == COMMANDLEVEL.main) {
                    //  メインコマンド
                    lbCommand.ItemsSource = mCommandData.getSubCommand(menu);
                } else if (level == COMMANDLEVEL.sub) {
                    //  サブコマンド
                    OPERATION ope = mCommandData.getCommand(menu);
                    if (mDataManage.commandExec(ope)) {
                        //  コマンド完了
                        commandCancel();
                    } else {
                        mOperationMode = mDataManage.mOperationMode;
                        if (mOperationMode == OPEMODE.close)            //  終了
                            Close();
                        else if (mOperationMode == OPEMODE.reload) {    //  図面構成再取込み
                            mFileData.setBaseDataFolder();
                            setDataFileList();
                        }
                    }
                }
                dispStatus(null);
            }
        }

        /// <summary>
        /// タブの切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (TabItem)tabCanvas.SelectedItem;
            if (mCurCanvas == null || mCurImage == null ||
                item == null || mDataManage == null) return;
            bt3DDispReset.IsEnabled = false;
            if (item.Name == "CanvasFRONT") {
                mDataManage.setFace(FACE3D.FRONT);
                mCurCanvas = cvCanvasFRONT;
                mCurImage = imScreenFRONT;
            } else if (item.Name == "CanvasTOP") {
                mDataManage.setFace(FACE3D.TOP);
                mCurCanvas = cvCanvasTOP;
                mCurImage = imScreenTOP;
            } else if (item.Name == "CanvasRIGHT") {
                mDataManage.setFace(FACE3D.RIGHT);
                mCurCanvas = cvCanvasRIGHT;
                mCurImage = imScreenRIGHT;
            } else if (item.Name == "OpenGL") {
                mDataManage.setFace(FACE3D.NON);
                bt3DDispReset.IsEnabled = true;
            } else
                return;
            if (item.Name != "OpenGL") {
                mDataManage.setCanvas(mCurCanvas, mCurImage);
                mDataManage.draw(false, false);
            }
            dispStatus(null);
            btDummy.Focus();                //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// コマンドのキャンセル(初期状態)
        /// </summary>
        private void commandCancel()
        {
            setDataFileList();
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            lbCommand.SelectedIndex = -1;
            mOperationMode = OPEMODE.pick;
            mMouseLeftButtonDown = false;
            mMouseRightButtonDown = false;
        }

        /// <summary>
        /// 色設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 <= cbColor.SelectedIndex)
                mDataManage.commandExec(OPERATION.setColor);
            btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// グリッドの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGridSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 <= cbGridSize.SelectedIndex)
                mDataManage.commandExec(OPERATION.setGrid);
            btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// ジャンル選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGenreList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbGenreList.SelectedIndex;
            if (0 <= index) {
                mFileData.setGenreFolder(cbGenreList.SelectedItem.ToString() ?? "");
                lbCategoryList.ItemsSource = mFileData.getCategoryList();
                lbCategoryList.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// カテゴリ選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbCategoryList.SelectedIndex;
            if (0 <= index) {
                mFileData.setCategoryFolder(lbCategoryList.SelectedItem.ToString() ?? "");
                lbItemList.ItemsSource = mFileData.getItemFileList();
                lbItemList.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// 図面選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbItemList.SelectedIndex;
            if (0 <= index) {
                //  カレントデータ終了処理
                mDataManage.saveFile();
                //  新規データ読込
                mFileData.mDataName = lbItemList.Items[index].ToString() ?? "";
                mDataManage.clear(mCurCanvas, mCurImage);
                mDataManage.mDataPath = mFileData.getCurItemFilePath();
                mDataManage.loadFile();
                //  データの表示
                cbColor.SelectedIndex = ylib.getBrushNo(mDataManage.mGlobal.mEntityBrush);
                cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(Math.Abs(mDataManage.getGridSize()));
                tabCanvas.SelectedIndex = 0;
                commandCancel();
                mDataManage.draw(true);
            }
        }

        /// <summary>
        /// ジャンル選択のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGenreMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            if (menuItem.Name.CompareTo("cbGenreAddMenu") == 0) {
                //  大分類(Genre)の追加
                string genre = mFileData.addGenre();
                if (0 < genre.Length) {
                    cbGenreList.ItemsSource = mFileData.getGenreList();
                    int index = cbGenreList.Items.IndexOf(genre);
                    if (0 <= index)
                        cbGenreList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("cbGenreRenameMenu") == 0) {
                //  大分類名の変更
                string genre = mFileData.renameGenre(cbGenreList.SelectedItem.ToString() ?? "");
                if (0 < genre.Length) {
                    cbGenreList.ItemsSource = mFileData.getGenreList();
                    int index = cbGenreList.Items.IndexOf(genre);
                    if (0 <= index)
                        cbGenreList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("cbGenreRemoveMenu") == 0) {
                //  大分類名の削除
                if (mFileData.removeGenre(cbGenreList.SelectedItem.ToString() ?? "")) {
                    cbGenreList.ItemsSource = mFileData.getGenreList();
                    if (0 < cbGenreList.Items.Count)
                        cbGenreList.SelectedIndex = 0;
                }
             }
        }

        /// <summary>
        /// カテゴリ選択のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCategoryMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            string category = "";
            if (0 <= lbCategoryList.SelectedIndex)
                category = lbCategoryList.SelectedItem.ToString() ?? "";
            if (menuItem.Name.CompareTo("lbCategoryAddMenu") == 0) {
                //  分類(Category)の追加
                category = mFileData.addCategory();
                if (0 < category.Length) {
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    int index = lbCategoryList.Items.IndexOf(category);
                    if (0 <= index)
                        lbCategoryList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryRenameMenu") == 0) {
                //  分類名の変更
                category = mFileData.renameCategory(category);
                if (0 < category.Length) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    int index = lbCategoryList.Items.IndexOf(category);
                    if (0 <= index)
                        lbCategoryList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryRemoveMenu") == 0) {
                //  分類の削除
                if (mFileData.removeCategory(lbCategoryList.SelectedItem.ToString() ?? "")) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count)
                        lbCategoryList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryCopyMenu") == 0) {
                //  分類のコピー
                mFileData.copyCategory(category);
            } else if (menuItem.Name.CompareTo("lbCategoryMoveMenu") == 0) {
                //  分類の移動
                if (mFileData.copyCategory(category, true)) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count)
                        lbCategoryList.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 図面選択のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbItemMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            string itemName = null;
            if (0 <= lbItemList.SelectedIndex)
                itemName = lbItemList.SelectedItem.ToString() ?? "";
            mDataManage.saveFile();
            if (menuItem.Name.CompareTo("lbItemAddMenu") == 0) {
                //  図面(Item)の追加
                itemName = mFileData.addItem();
                if (0 < itemName.Length) {
                    mDataManage.clear(mCurCanvas, mCurImage);
                    mDataManage.mDataPath = mFileData.getItemFilePath(itemName);
                    mDataManage.saveFile(true);
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
                }
            } else if (menuItem.Name.CompareTo("lbItemRenameMenu") == 0 && itemName != null) {
                //  図面名の変更
                itemName = mFileData.renameItem(itemName);
                if (0 < itemName.Length) {
                    mDataManage.mDataPath = mFileData.getItemFilePath(itemName);
                    mDataManage.loadFile();
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
                }
            } else if (menuItem.Name.CompareTo("lbItemRemoveMenu") == 0 && itemName != null) {
                //  図面の削除
                if (mFileData.removeItem(itemName)) {
                    if (mDataManage.mDataPath == mFileData.getItemFilePath(itemName))
                        mDataManage.clear(mCurCanvas, mCurImage);
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbItemCopyMenu") == 0 && itemName != null) {
                //  図面のコピー
                mFileData.copyItem(itemName);
            } else if (menuItem.Name.CompareTo("lbItemMoveMenu") == 0 && itemName != null) {
                //  図面の移動
                if (mFileData.copyItem(itemName, true)) {
                    mDataManage.mDataPath = "";
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbItemImportMenu") == 0) {
                //  インポート
                mDataManage.import(mFileData.getCurCategoryPath());
                lbItemList.ItemsSource = mFileData.getItemFileList();
                itemName = Path.GetFileNameWithoutExtension(mDataManage.mDataPath);
                lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
            } else if (menuItem.Name.CompareTo("lbItemPropertyMenu") == 0 && itemName != null) {
                //  プロパティ
                string buf = mFileData.getItemFileProperty(itemName);
                ylib.messageBox(this, buf, "", "ファイルプロパティ");
            }
        }

        /// <summary>
        /// コマンドボタン処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCommand_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Name.CompareTo("btZoomArea") == 0) {
                //  表示領域指定
                mPrevOpeMode = mOperationMode;
                mOperationMode = OPEMODE.areaDisp;
            } else if (button.Name.CompareTo("btZoomIn") == 0) {
                //  拡大表示
                mDataManage.mDataDraw.zoom(mDataManage.mDataDraw.mGDraw.mWorld.getCenter(), 1.2);
            } else if (button.Name.CompareTo("btZoomOut") == 0) {
                    //  縮小表示
                mDataManage.mDataDraw.zoom(mDataManage.mDataDraw.mGDraw.mWorld.getCenter(), 1 / 1.2);
            } else if (button.Name.CompareTo("btZoomFit") == 0) {
                //  全体表示
                mDataManage.mDataDraw.dispFit();
            } else if (button.Name.CompareTo("btZoomWidthFit") == 0) {
                //  全幅表示
            } else if (button.Name.CompareTo("bt3DDispReset") == 0) {
                // 3D表示
                m3Dlib.keyMove(System.Windows.Input.Key.End, true, false);
                renderFrame();
            } else if (button.Name.CompareTo("btAreaPick") == 0) {
                //  領域ピック
                if (mOperationMode == OPEMODE.pick) {
                    mPrevOpeMode = mOperationMode;
                    mOperationMode = OPEMODE.areaPick;
                }
            } else if (button.Name.CompareTo("btPropertyChange") == 0) {
                //  属性変更
                mDataManage.commandExec(OPERATION.changeProperty);
            } else if (button.Name.CompareTo("btSetting") == 0) {
                //  システム設定
                mDataManage.commandExec(OPERATION.systemProperty);
            }
        }

        private void cbCommand_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void cbFilletSize_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                mDataManage.mGlobal.mFilletSize = ylib.doubleParse(cbFilletSize.Text, 0);
                if (!mFilletSizeMenu.Contains(mDataManage.mGlobal.mFilletSize)) {
                    mFilletSizeMenu.Add(mDataManage.mGlobal.mFilletSize);
                    mFilletSizeMenu.Sort();
                    cbFilletSize.Items.Clear();
                    mFilletSizeMenu.ForEach(p => cbFilletSize.Items.Add(p));
                }
            }
        }

        private void cbFilletSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbFilletSize.SelectedIndex;
            if (0 <= index)
                mDataManage.mGlobal.mFilletSize = mFilletSizeMenu[index];
            btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        private void cbBaseLoc_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="key"></param>
        /// <param name="control"></param>
        /// <param name="shift"></param>
        private void keyCommand(Key key, bool control, bool shift)
        {
            //  コマンド入力時は無効
            if (cbCommand.IsKeyboardFocusWithin)
                return;

            if (mDataManage.mGlobal.mFace == FACE3D.NON) {
                // 3D表示
                m3Dlib.keyMove(key, control, shift);
                renderFrame();
            } else {
                //  2D表示
                if (control) {
                    switch (key) {
                        case Key.S: mDataManage.commandExec(OPERATION.save); break;
                        case Key.Z: mDataManage.commandExec(OPERATION.undo); break;
                        default:
                            mDataManage.mDataDraw.key2DMove(key, control, shift);
                            break;
                    }
                } else {
                    switch (key) {
                        case Key.F2:                                 //  領域拡大
                            mPrevOpeMode = mOperationMode;
                            mOperationMode = OPEMODE.areaDisp;
                            break;
                        case Key.F7:                                 //  領域ピック
                            if (mOperationMode == OPEMODE.pick) {
                                mPrevOpeMode = mOperationMode;
                                mOperationMode = OPEMODE.areaPick;
                            }
                            break;
                        case Key.Escape: commandCancel(); break;     //  ESCキーでキャンセル
                        case Key.Back: mDataManage.backLoc(); break; //  ロケイト点を一つ戻す
                        case Key.Apps:                               //  コンテキストメニューキー
                            if (mDataManage.locMenu()) {
                                if (mDataManage.definData(false, ylib.onAltKey()))
                                    commandCancel();
                            } else if (mDataManage.groupPickMenu()) {
                                mDataManage.draw();
                            }
                            break;
                        default: 
                            mDataManage.mDataDraw.key2DMove(key, control, shift);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 操作モードとマウス位置の表示
        /// </summary>
        /// <param name="wpos">マウス位置(World座標)</param>
        public void dispStatus(PointD wpos)
        {
            if (mPrePosition == null)
                return;
            if (wpos == null)
                wpos = mPrePosition;
            tbStatus.Text = $"[{mDataManage.mGlobal.mFace}] [{mOperationMode}] Pick [{mDataManage.getPickCount()}] Loc [{mDataManage.getLocCount()}] Grid[{mDataManage.getGridSize()}] {wpos.ToString("f2")}";
            string zumenName = Path.GetFileNameWithoutExtension(mDataManage.mDataPath);
            Title = $"{mAppName} [{zumenName}][{mDataManage.getDispEntCount()} / {mDataManage.mEntityList.Count}]";
        }

        /// <summary>
        /// データファイルのリストをリストビューに設定する
        /// </summary>
        public void setDataFileList()
        {
            cbGenreList.ItemsSource = mFileData.getGenreList();
            int index = cbGenreList.Items.IndexOf(mFileData.mGenreName);
            if (0 <= index) {
                //  ジャンルを設定
                if (cbGenreList.SelectedIndex == index) {
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                } else
                    cbGenreList.SelectedIndex = index;
                index = lbCategoryList.Items.IndexOf(mFileData.mCategoryName);
                if (0 <= index) {
                    if (lbCategoryList.SelectedIndex == index)
                        lbItemList.ItemsSource = mFileData.getItemFileList();
                    else
                        lbCategoryList.SelectedIndex = index;
                    index = lbItemList.Items.IndexOf(mFileData.mDataName);
                    if (0 <= index) {
                        lbItemList.SelectedIndex = index;
                    }
                }
            } else {
                //  ジャンル不定
                if (0 < cbGenreList.Items.Count) {
                    mFileData.mGenreName = cbGenreList.Items[0].ToString() ?? "";
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count) {
                        mFileData.mCategoryName = lbCategoryList.Items[0].ToString() ?? "";
                        lbItemList.ItemsSource = mFileData.getItemFileList();
                    }
                }
            }
        }
    }
}