using CoreLib;
using System.Windows;
using System.Windows.Input;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Cad3DApp
{
    /// <summary>
    /// PropertyDlg.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyDlg : Window
    {
        public Brush mLineColor = Brushes.Black;    //  2D表示色
        public string mLineColorName = "Black";
        public bool mLineColorEnable = false;
        public Brush mFaceColor = Brushes.Blue;     //  3D表示色
        public string mFaceColorName = "Blue";
        public bool mFaceColorEnable = false;
        public int mLineFont = 0;                   //  線種
        public bool mLineFontOn = true;
        public bool mLineFontEnable = false;
        public bool mDisp2D = true;                 //  2D表示
        public bool mDisp2DEnable = false;
        public bool mDisp3D = true;                 //  3D表示
        public bool mDisp3DEnable = false;
        public bool mReverse = true;                //  逆順表示
        public bool mReverseEnable = false;
        public bool mEdgeDisp = true;               //  端面表示
        public bool mEdgeDispEnable = false;
        public bool mEdgeReverse = true;            //  端面逆順表示
        public bool mEdgeReverseEnable = false;
        public List<CheckBoxListItem> mChkList;     //  レイヤー使用リスト
        public bool mCkkListAdd = true;             //  レイヤ追加チェック
        public bool mCkkListEnable = false;
        public string mGroup = "";                  //  グループ名
        public List<string> mGroupList;
        public bool mGroupEnable = false;

        public bool mPropertyAll = false;           //  複数要素設定

        private string[] mLineFontName = new string[] {
            "実線", "破線", "一点鎖線", "二点鎖線"};
        private YLib ylib = new YLib();

        public PropertyDlg()
        {
            InitializeComponent();

            Title = "要素属性";
            cbLineColor.DataContext = ylib.mBrushList;
            cbFaceColor.DataContext = ylib.mBrushList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int colorindex = ylib.getBrushNo(mLineColor);
            if (0 <= colorindex)
                cbLineColor.SelectedIndex = colorindex;
            colorindex = ylib.getBrushNo(mFaceColor);
            if (0 <= colorindex)
                cbFaceColor.SelectedIndex = ylib.getBrushNo(mFaceColor);
            cbLineFont.ItemsSource = mLineFontName;
            cbLineFont.SelectedIndex = mLineFont;
            cbLineFont.IsEnabled = mLineFontOn;
            chDisp2D.IsChecked = mDisp2D;
            chDisp3D.IsChecked = mDisp3D;
            chReverse.IsChecked = mReverse;
            chEdgeDisp.IsChecked = mEdgeDisp;
            chEdgeReverse.IsChecked = mEdgeReverse;
            cbLayerList.Items.Clear();
            if (mChkList != null)
                mChkList.ForEach(p => cbLayerList.Items.Add(p));
            chLayerListAdd.IsChecked = mCkkListAdd;
            if (0 < mChkList.Count) {
                int n = mChkList.FindIndex(p => p.Checked);
                cbLayerList.SelectedIndex = Math.Max(0, n);
            }
            cbGroup.ItemsSource = mGroupList;
            cbGroup.Text = mGroup;

            if (!mPropertyAll) {
                //  一括変更以外
                chLineColorEnable.Visibility = Visibility.Hidden;
                chFaceColorEnable.Visibility = Visibility.Hidden;
                chLineFontEnable.Visibility = Visibility.Hidden;
                chDisp2DEnable.Visibility = Visibility.Hidden;
                chDisp3DEnable.Visibility = Visibility.Hidden;
                chReverseEnable.Visibility = Visibility.Hidden;
                chEdgeDispEnable.Visibility = Visibility.Hidden;
                chEdgeReverseEnable.Visibility = Visibility.Hidden;
                chLayerListAdd.Visibility = Visibility.Hidden;
                chLayerListEnable.Visibility = Visibility.Hidden;
                chGroupEnable.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void cbLayerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InputBox dlg = new InputBox();
            dlg.Title = "レイヤー名の追加";
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dlg.ShowDialog() == true) {
                CheckBoxListItem item = new CheckBoxListItem(false, dlg.mEditText);
                if (0 > mChkList.FindIndex(p => p.Text.CompareTo(item.Text) == 0)) {
                    mChkList.Add(item);
                    cbLayerList.Items.Clear();
                    mChkList.ForEach(p => cbLayerList.Items.Add(p));
                }
            }
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (0 <= cbLineColor.SelectedIndex) {
                mLineColor = ylib.mBrushList[cbLineColor.SelectedIndex].brush;
                mLineColorName = ylib.mBrushList[cbLineColor.SelectedIndex].colorTitle;
            }
            mLineFont = cbLineFont.SelectedIndex;
            mLineFontEnable = chLineFontEnable.IsChecked == true;
            if (0 <= cbFaceColor.SelectedIndex) {
                mFaceColor = ylib.mBrushList[cbFaceColor.SelectedIndex].brush;
                mFaceColorName = ylib.mBrushList[cbFaceColor.SelectedIndex].colorTitle;
            }
            mLineColorEnable = chLineColorEnable.IsChecked == true;
            mFaceColorEnable = chFaceColorEnable.IsChecked == true;
            mDisp2D = chDisp2D.IsChecked == true;
            mDisp2DEnable = chDisp2DEnable.IsChecked == true;
            mDisp3D = chDisp3D.IsChecked == true;
            mDisp3DEnable = chDisp3DEnable.IsChecked == true;
            mReverse = chReverse.IsChecked == true;
            mReverseEnable = chReverseEnable.IsChecked == true;
            mEdgeDisp = chEdgeDisp.IsChecked == true;
            mEdgeDispEnable = chEdgeDispEnable.IsChecked == true;
            mEdgeReverse = chEdgeReverse.IsChecked == true;
            mEdgeReverseEnable = chEdgeReverseEnable.IsChecked == true;
            mCkkListAdd = chLayerListAdd.IsChecked == true;
            mCkkListEnable = chLayerListEnable.IsChecked == true;
            mGroup = cbGroup.Text;
            mGroupEnable = chGroupEnable.IsChecked == true;

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
