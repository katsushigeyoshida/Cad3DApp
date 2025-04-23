using CoreLib;
using System.IO;
using System.Windows;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Cad3DApp
{
    /// <summary>
    /// SystemDlg.xaml の相互作用ロジック
    /// </summary>
    public partial class SystemDlg : Window
    {
        public double mArcDivideAngle = 30;                 //  円変換分割角度
        public double mRevolutionDivideAngle = 30;          //  回転体の分割角度
        public double mSweepDivideAngle = 30;               //  掃引の分割角度
        public bool mSurfaceVertex = false;                 //  多角形の分割線2D表示
        public bool mWireFrame = false;                     //  ワイヤーフレーム表示3D
        public Brush mBackColor = Brushes.White;            //  背景色
        public string mDataFolder = "";                     //  データフォルダ
        public string mBackupFolder = "";                   //  バックアップフォルダ
        public string mDiffTool = "";                       //  ファイル比較ツール
        public FileData mFileData;                          //  バックアップ比較

        private string mDataFolderListPath = "DataFolderList.csv";      //  データフォルダパスリストファイルパス
        private List<string> mDataFolderList = new List<string>();      //  データフォルダパスリスト

        private YLib ylib = new YLib();

        public SystemDlg()
        {
            InitializeComponent();
            cbBackColor.DataContext = ylib.mBrushList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbArcDivideAng.Text = ylib.double2StrZeroSup(ylib.R2D(mArcDivideAngle), "F8");
            tbRevolutionDivideAng.Text = ylib.double2StrZeroSup(ylib.R2D(mRevolutionDivideAngle), "F8");
            tbSweepDivideAng.Text = ylib.double2StrZeroSup(ylib.R2D(mSweepDivideAngle), "F8");
            chPolygonTriangles.IsChecked = mSurfaceVertex;
            chWireFrame.IsChecked = mWireFrame;
            int colorIndex = ylib.getBrushNo(mBackColor);
            if (0 <= colorIndex)
                cbBackColor.SelectedIndex = colorIndex;
            List<string[]> llistf = ylib.loadCsvData(mDataFolderListPath);
            foreach (var buf in llistf) {
                if (!mDataFolderList.Contains(buf[0]))
                    mDataFolderList.Add(buf[0]);
            }
            cbDataFolder.Text = mDataFolder;
            if (0 < mDataFolder.Length)
                mDataFolderList.Insert(0, mDataFolder);
            cbDataFolder.ItemsSource = mDataFolderList;
            tbBackupFolder.Text = mBackupFolder;
            tbDiffTool.Text = mDiffTool;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<string[]> llist = new List<string[]>();
            foreach (var buf in mDataFolderList)
                llist.Add([buf]);
            ylib.saveCsvData(mDataFolderListPath, llist);
        }

        private void tbDataFolder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("データフォルダ", mDataFolder);
            if (folder != null && 0 < folder.Length)
                cbDataFolder.Text = folder;
        }

        private void tbBackupFolder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("バックアップフォルダ", mBackupFolder);
            if (folder != null && 0 < folder.Length)
                tbBackupFolder.Text = folder;
        }

        private void tbDiffTool_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            List<string[]> filters = new List<string[]>() {
                    new string[] { "実行ファイル", "*.exe" },
                    new string[] { "すべてのファイル", "*.*"}
                };
            string filePath = ylib.fileOpenSelectDlg("ツール選択", Path.GetDirectoryName(mDiffTool), filters);
            if (0 < filePath.Length)
                tbDiffTool.Text = filePath;
        }

        private void btDataBackup_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            count += mFileData.dataBackUp(mDataFolder, mBackupFolder, false);
            ylib.messageBox(this, $"{count} ファイルのバックアップを更新しました。");
        }

        private void btDataRestore_Click(object sender, RoutedEventArgs e)
        {
            mFileData.dataRestor(mDataFolder, mBackupFolder);
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            mArcDivideAngle = ylib.D2R(ylib.string2double(tbArcDivideAng.Text));
            mRevolutionDivideAngle = ylib.D2R(ylib.string2double(tbRevolutionDivideAng.Text));
            mSweepDivideAngle = ylib.D2R(ylib.string2double(tbSweepDivideAng.Text));
            mSurfaceVertex = chPolygonTriangles.IsChecked == true;
            mWireFrame = chWireFrame.IsChecked == true;
            if (0 <= cbBackColor.SelectedIndex) {
                mBackColor = ylib.mBrushList[cbBackColor.SelectedIndex].brush;
            }
            mDataFolder = cbDataFolder.Text;
            if (mDataFolderList.Contains(mDataFolder))
                mDataFolderList.Remove(mDataFolder);
            mDataFolderList.Insert(0, mDataFolder);
            mBackupFolder = tbBackupFolder.Text;
            mDiffTool = tbDiffTool.Text;

            DialogResult = true;
            Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
