using CoreLib;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Cad3DApp
{

    /// <summary>
    /// 描画方式
    /// </summary>
    public enum DRAWTYPE
    {
        POINTS, LINES, LINE_STRIP, LINE_LOOP,
        TRIANGLES, QUADS, POLYGON, TRIANGLE_STRIP,
        QUAD_STRIP, TRIANGLE_FAN, MULTI
    };


    /// <summary>
    /// Surfaceの元データ
    /// </summary>
    public class SurfaceData
    {
        public List<Point3D> mVertexList;           //  座標点リスト
        public DRAWTYPE mDrawType;                  //  描画方式
        public Brush mFaceColor = Brushes.Blue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SurfaceData()
        {
        }


        /// <summary>
        /// コンストラクタ(ポリラインをサーフェスデータに変換する)
        /// </summary>
        /// <param name="polyline">ポリライン</param>
        public SurfaceData(Polyline3D polyline)
        {
            mVertexList = polyline.toPoint3D();
            mDrawType = DRAWTYPE.LINE_STRIP;
            mFaceColor = mFaceColor;
        }

        /// <summary>
        /// 座標点の移動
        /// </summary>
        /// <param name="v">移動ベクトル</param>
        public void translate(Point3D v)
        {
            for (int i = 0; i < mVertexList.Count; i++) {
                mVertexList[i].translate(v);
            }
        }

        /// <summary>
        /// 座標点の回転
        /// </summary>
        /// <param name="cp">回転中心</param>
        /// <param name="ang">回転角</param>
        /// <param name="face">回転面</param>
        public void rotate(Point3D cp, double ang, FACE3D face)
        {
            for (int i = 0; i < mVertexList.Count; i++) {
                mVertexList[i].rotate(cp, ang, face);
            }
        }

        /// <summary>
        /// 面データを逆回りに変換する
        /// </summary>
        /// <param name="reverse">変換する</param>
        public void reverse(bool reverse = true)
        {
            if (!reverse)
                return;
            Point3D t = new Point3D();
            switch (mDrawType) {
                case DRAWTYPE.TRIANGLES:
                    for (int i = 0; i < mVertexList.Count; i += 3) {
                        t = mVertexList[i];
                        mVertexList[i] = mVertexList[i + 2];
                        mVertexList[i + 2] = t;
                    }
                    break;
                case DRAWTYPE.QUADS:
                    for (int i = 0; i < mVertexList.Count; i += 4) {
                        t = mVertexList[i + 1];
                        mVertexList[i + 1] = mVertexList[i + 3];
                        mVertexList[i + 3] = t;
                    }
                    break;
                case DRAWTYPE.TRIANGLE_STRIP:
                case DRAWTYPE.QUAD_STRIP:
                    for (int i = 0; i < mVertexList.Count; i += 2) {
                        t = mVertexList[i];
                        mVertexList[i] = mVertexList[i + 1];
                        mVertexList[i + 1] = t;
                    }
                    break;
                case DRAWTYPE.TRIANGLE_FAN:
                    t = mVertexList[0];
                    mVertexList.RemoveAt(0);
                    mVertexList.Reverse();
                    mVertexList.Insert(0, t);
                    break;
                case DRAWTYPE.POLYGON:
                    mVertexList.Reverse();
                    break;
            }
        }

        /// <summary>
        /// サーフェースデータをポリラインの座標リストに変換
        /// </summary>
        /// <returns>座標リスト</returns>
        public List<List<Point3D>> toPolylineList()
        {
            List<List<Point3D>> polylineList = new List<List<Point3D>>();
            List<Point3D> bufList;
            switch (mDrawType) {
                case DRAWTYPE.LINES:
                    for (int i = 0; i < mVertexList.Count - 1; i += 2) {
                        List<Point3D> buf = new List<Point3D>() {
                            mVertexList[i],
                            mVertexList[i + 1],
                        };
                        polylineList.Add(buf);
                    }
                    break;
                case DRAWTYPE.LINE_STRIP:
                    bufList = new List<Point3D>();
                    for (int i = 0; i < mVertexList.Count; i++) {
                        bufList.Add(mVertexList[i]);
                    }
                    polylineList.Add(bufList);
                    break;
                case DRAWTYPE.LINE_LOOP:
                    bufList = new List<Point3D>();
                    for (int i = 0; i < mVertexList.Count; i++) {
                        bufList.Add(mVertexList[i]);
                    }
                    bufList.Add(mVertexList[0]);
                    polylineList.Add(bufList);
                    break;
                case DRAWTYPE.TRIANGLES:
                    for (int i = 0; i < mVertexList.Count - 2; i += 3) {
                        List<Point3D> buf = new List<Point3D>() {
                            mVertexList[i],
                            mVertexList[i + 1],
                            mVertexList[i + 2],
                        };
                        polylineList.Add(buf);
                    }
                    break;
                case DRAWTYPE.QUADS:
                    for (int i = 0; i < mVertexList.Count - 3; i += 4) {
                        List<Point3D> buf = new List<Point3D>() {
                            mVertexList[i],
                            mVertexList[i + 1],
                            mVertexList[i + 2],
                            mVertexList[i + 3],
                        };
                        polylineList.Add(buf);
                    }
                    break;
                case DRAWTYPE.TRIANGLE_STRIP:
                    for (int i = 0; i < mVertexList.Count - 2; i += 2) {
                        List<Point3D> buf = new List<Point3D>() {
                            mVertexList[i],
                            mVertexList[i + 1],
                            mVertexList[i + 2],
                            mVertexList[i],
                        };
                        polylineList.Add(buf);
                    }
                    break;
                case DRAWTYPE.QUAD_STRIP:
                    for (int i = 0; i < mVertexList.Count - 3; i += 2) {
                        List<Point3D> buf = new List<Point3D>() {
                            mVertexList[i],
                            mVertexList[i + 1],
                            mVertexList[i + 3],
                            mVertexList[i + 2],
                            mVertexList[i],
                        };
                        polylineList.Add(buf);
                    }
                    break;
                case DRAWTYPE.TRIANGLE_FAN:
                    for (int i = 1; i < mVertexList.Count - 1; i++) {
                        List<Point3D> buf = new List<Point3D>() {
                            mVertexList[0],
                            mVertexList[i],
                            mVertexList[i + 1],
                            mVertexList[0],
                        };
                        polylineList.Add(buf);
                    }
                    break;
                case DRAWTYPE.POLYGON:
                    bufList = new List<Point3D>();
                    for (int i = 0; i < mVertexList.Count; i++) {
                        bufList.Add(mVertexList[i]);
                    }
                    bufList.Add(mVertexList[0]);
                    polylineList.Add(bufList);
                    break;
            }
            return polylineList;
        }

        /// <summary>
        /// コピーを作成
        /// </summary>
        /// <returns></returns>
        public SurfaceData toCopy()
        {
            SurfaceData surfaceData = new SurfaceData();
            surfaceData.mVertexList = mVertexList.ConvertAll(p => p.toCopy());
            surfaceData.mDrawType = mDrawType;
            surfaceData.mFaceColor = mFaceColor;
            return surfaceData;
        }

        /// <summary>
        /// 文字列に変換
        /// </summary>
        /// <returns></returns>
        public string toString(string form = "F2")
        {
            string buf = mDrawType.ToString();
            for (int i = 0; i < mVertexList.Count; i++)
                buf += "," + mVertexList[i].ToString(form);
            return buf;
        }

        /// <summary>
        /// 座標点リストを2Dに変換する
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public List<PointD> toPointDList(FACE3D face)
        {
            return mVertexList.ConvertAll(p => p.toPoint(face));
        }
    }
}
