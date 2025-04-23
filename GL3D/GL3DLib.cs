using CoreLib;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cad3DApp
{
    /// <summary>
    /// OpenGLを使って3Dデータを標示するライブラリ
    /// OPenTK 3.x
    ///     OpenTK,OpenTK.GLControlをNuGetでインストール
    ///     System.Drawingを参照に追加
    ///     System.Windows.Formsを参照に追加
    ///     XAMLにWindowsFormsHostをツールボックスから追加
    /// </summary>

    public class GL3DLib
    {
        public int mWorldWidth;
        public int mWorldHeight;

        private double m3DScale = 5;                    //  3D表示の初期スケール
        private Color4 mBackColor = Color4.White;       //  背景色
        private bool mIsCameraRotating;                 //  カメラが回転状態かどうか
        private bool mIsTransrate;                      //  移動状態
        private Vector2 mCcurrent, mPrevious;           //  現在の点、前の点
        private Matrix4 mRotate;                        //  回転行列
        private float mZoom;                            //  拡大度
        private float mZoomMax = 2.0f;                  //  最大拡大率
        private float mZoomMin = 0.5f;                  //  最小拡大率
        private Box3D mArea;                            //  表示領域

        private GLControl mGlControl;                   //  OpenTK.GLcontrol
        private WindowsFormsHost mGlGraph;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="gLControl">OpenTKのGLControl</param>
        public GL3DLib(GLControl gLControl, WindowsFormsHost glGraph)
        {
            mGlControl = gLControl;
            mGlGraph = glGraph;
        }

        /// <summary>
        /// 表示位置関係を初期化する 
        /// </summary>
        /// <param name="zoom">拡大率</param>
        /// <param name="xrotate">X軸回転(deg)</param>
        /// <param name="yrotate">Y軸回転(deg)</param>
        /// <param name="zrotate">Z軸回転(deg)</param>
        public void initPosition(float zoom, float xrotate, float yrotate, float zrotate)
        {
            mIsCameraRotating = false;
            mIsTransrate = false;
            mCcurrent = Vector2.Zero;
            mPrevious = Vector2.Zero;
            mRotate = Matrix4.Identity;
            mZoom = zoom;
            //  XYZ軸を中心回転
            OpenTK.Quaternion after = new OpenTK.Quaternion(xrotate / 180f * (float)Math.PI, yrotate / 180f * (float)Math.PI, zrotate / 180f * (float)Math.PI);
            mRotate *= Matrix4.CreateFromQuaternion(after);
            //  移動の追加
            //mRotate *= Matrix4.CreateTranslation(0f, 0f, 0f);
        }

        /// <summary>
        /// 光源の設定
        /// </summary>
        public void initLight()
        {
            GL.Enable(EnableCap.DepthTest);         //  デプスバッファ
            GL.Enable(EnableCap.ColorMaterial);     //  材質設定
            GL.Enable(EnableCap.Lighting);          //  光源の使用

            //setLight();
            //setMaterial();
            float[] position0 = new float[] { 1.0f, 1.0f, 2.0f, 0.0f };
            float[] position1 = new float[] { -1.0f, 1.0f, 2.0f, 0.0f };
            GL.Light(LightName.Light0, LightParameter.Position, position0);
            GL.Light(LightName.Light1, LightParameter.Position, position1);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Light1);

            GL.PointSize(3.0f);                     //  点の大きさ
            GL.LineWidth(1.5f);                     //  線の太さ
        }

        /// <summary>
        /// キーコントロール(実行後に rendeform()で描画更新要)
        /// </summary>
        /// <param name="key">キーコード</param>
        /// <param name="control">Ctrlキー</param>
        /// <param name="shift">Shiftキー</param>
        public void keyMove(Key key, bool control, bool shift)
        {
            float translateStep = 0.1f;
            float rotateStep = 5f / 180f * (float)Math.PI;
            float scaleStep = 1 / 10f;
            if (control) {
                switch (key) {
                    case Key.Left: translate(translateStep, 0, 0); break;
                    case Key.Right: translate(-translateStep, 0, 0); break;
                    case Key.Up: translate(0, -translateStep, 0); break;
                    case Key.Down: translate(0, translateStep, 0); break;
                    case Key.PageUp: translate(0, 0, translateStep); break;
                    case Key.PageDown: translate(0, 0, -translateStep); break;
                    case Key.End: mRotate = Matrix4.Identity; break;
                    default: break;
                }
            } else if (shift) {
                switch (key) {
                    case Key.End: setZoom(scaleStep); break;
                    default: break;
                }
            } else {
                switch (key) {
                    case Key.Left: rotateY(rotateStep); break;
                    case Key.Right: rotateY(-rotateStep); break;
                    case Key.Up: rotateX(-rotateStep); break;
                    case Key.Down: rotateX(rotateStep); break;
                    case Key.PageUp: rotateZ(-rotateStep); break;
                    case Key.PageDown: rotateZ(rotateStep); break;
                    case Key.End: setZoom(-scaleStep); break;
                    default: break;
                }
            }
        }

        /// <summary>
        /// 移動の追加 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void translate(float x, float y, float z)
        {
            mRotate *= Matrix4.CreateTranslation(x, y, z);
        }

        /// <summary>
        /// X軸で回転
        /// </summary>
        /// <param name="rot">回転角(rad)</param>
        public void rotateX(float rot)
        {
            mRotate *= Matrix4.CreateRotationX(rot);
        }

        /// <summary>
        /// Y軸で回転
        /// </summary>
        /// <param name="rot">回転角(rad)</param>
        public void rotateY(float rot)
        {
            mRotate *= Matrix4.CreateRotationY(rot);
        }

        /// <summary>
        /// Z軸で回転
        /// </summary>
        /// <param name="rot">回転角(rad)</param>
        public void rotateZ(float rot)
        {
            mRotate *= Matrix4.CreateRotationZ(rot);
        }

        /// <summary>
        /// 3D領域を設定
        /// </summary>
        /// <param name="area">領域</param>
        public void setArea(Box3D area)
        {
            mArea = area;
        }

        /// <summary>
        /// 三次元データ表示
        /// </summary>
        public void renderFrame(List<SurfaceData> slist)
        {
            mWorldWidth = (int)mGlGraph.ActualWidth;
            mWorldHeight = (int)mGlGraph.ActualHeight;
            if (mWorldWidth == 0 || mWorldHeight == 0 || mArea == null)
                return;
            setBackColor(mBackColor);
            renderFrameStart();
            //  表示領域にはいるようにスケールと位置移動ベクトルを求める
            double scale = m3DScale / mArea.getSize();
            Point3D v = mArea.getCenter();
            v.inverse();
            //  データの登録
            for (int i = 0; i < slist.Count; i++) {
                drawSurface(slist[i].mVertexList, slist[i].mDrawType,
                    slist[i].mFaceColor, scale, v);
            }
            //setAreaFrameDisp(false);
            //drawAxis(scale, v);
            rendeFrameEnd();
        }

        /// <summary>
        /// Point3Dリストデータを登録
        /// </summary>
        /// <param name="vertexList">座標点リスト</param>
        /// <param name="drawType">描画方法</param>
        /// <param name="brush">カラー</param>
        /// <param name="scale">スケール</param>
        /// <param name="v">中心移動</param>
        public void drawSurface(List<Point3D> vertexList, DRAWTYPE drawType,
            System.Windows.Media.Brush brush, double scale, Point3D v)
        {
            GL.Begin(cnvDrawType2PrimitiveType(drawType));
            GL.Color4(brush2Color4(brush));
            if (drawType == DRAWTYPE.POLYGON)
                GL.Normal3(getNormalVector(vertexList));
            for (int i = 0; i < vertexList.Count; i++) {
                //  法線の登録
                if (drawType == DRAWTYPE.TRIANGLES && (i % 3 == 0) && i < vertexList.Count - 2) {
                    GL.Normal3(getNormalVector(vertexList[i], vertexList[i + 1], vertexList[i + 2]));
                } else if (drawType == DRAWTYPE.QUADS && (i % 4 == 0) && i < vertexList.Count - 3) {
                    GL.Normal3(getNormalVector(vertexList[i], vertexList[i + 1], vertexList[i + 2]));
                } else if (drawType == DRAWTYPE.TRIANGLE_STRIP && (i % 2 == 0) && i < vertexList.Count - 2) {
                    GL.Normal3(getNormalVector(vertexList[i], vertexList[i + 1], vertexList[i + 2]));
                } else if (drawType == DRAWTYPE.QUAD_STRIP && (i % 2 == 0) && i < vertexList.Count - 3) {
                    GL.Normal3(getNormalVector(vertexList[i], vertexList[i + 1], vertexList[i + 3]));
                } else if (drawType == DRAWTYPE.TRIANGLE_FAN && (i % 2 == 0) && i < vertexList.Count - 3) {
                    GL.Normal3(getNormalVector(vertexList[0], vertexList[i + 1], vertexList[i + 3]));
                }
                //  座標の登録
                GL.Vertex3(point2Vector(vertexList[i], scale, v));
            }
            GL.End();
        }

        /// <summary>
        /// DRAWTYPEをPrimitiveType(OpenGL)に変換
        /// </summary>
        /// <param name="drawType">DRAWTYPE</param>
        /// <returns>PrimitiveType</returns>
        public PrimitiveType cnvDrawType2PrimitiveType(DRAWTYPE drawType)
        {
            PrimitiveType primType = PrimitiveType.Points;
            switch (drawType) {
                case DRAWTYPE.POINTS: primType = PrimitiveType.Points; break;
                case DRAWTYPE.LINES: primType = PrimitiveType.Lines; break;
                case DRAWTYPE.LINE_STRIP: primType = PrimitiveType.LineStrip; break;
                case DRAWTYPE.LINE_LOOP: primType = PrimitiveType.LineLoop; break;
                case DRAWTYPE.TRIANGLES: primType = PrimitiveType.Triangles; break;
                case DRAWTYPE.QUADS: primType = PrimitiveType.Quads; break;
                case DRAWTYPE.TRIANGLE_STRIP: primType = PrimitiveType.TriangleStrip; break;
                case DRAWTYPE.QUAD_STRIP: primType = PrimitiveType.QuadStrip; break;
                case DRAWTYPE.TRIANGLE_FAN: primType = PrimitiveType.TriangleFan; break;
                case DRAWTYPE.POLYGON: primType = PrimitiveType.Polygon; break;
                default: break;
            }
            return primType;
        }

        /// <summary>
        /// BrushをColor4に変換
        /// </summary>
        /// <param name="brush">Brush</param>
        /// <returns>Color4</returns>
        public Color4 brush2Color4(System.Windows.Media.Brush brush)
        {
            Color4 col = new Color4();
            if (brush == null)
                return col;
            System.Windows.Media.Color color = (brush as SolidColorBrush).Color;
            col.R = (float)color.R / 256;
            col.G = (float)color.G / 256;
            col.B = (float)color.B / 256;
            col.A = (float)color.A / 256;
            return col;
        }

        /// <summary>
        /// 座標リスト全体から法線ベクトルを求める
        /// </summary>
        /// <param name="plist">座標リスト</param>
        /// <returns></returns>
        private Vector3 getNormalVector(List<Point3D> plist)
        {
            Point3D crossProduct = new Point3D();
            if (2 < plist.Count) {
                for (int i = 0; i < plist.Count - 2; i++) {
                    Point3D v1 = plist[i].vector(plist[i + 1]);
                    Point3D v2 = plist[i + 1].vector(plist[i + 2]);
                    crossProduct += v1.crossProduct(v2);
                }
                crossProduct.unit();
            }
            return new Vector3((float)crossProduct.x, (float)crossProduct.y, (float)crossProduct.z);

        }

        /// <summary>
        /// 法線ベクトルを求める
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Vector3 getNormalVector(Point3D p0, Point3D p1, Point3D p2)
        {
            Point3D v0 = p1 - p0;
            Point3D v1 = p2 - p1;
            Point3D v2 = v0.crossProduct(v1);
            v2.unit();
            return new Vector3((float)v2.x, (float)v2.y, (float)v2.z);
        }

        /// <summary>
        /// Point3DをVector3に変換
        /// </summary>
        /// <param name="pos">Point3D</param>
        /// <returns>Vector3</returns>
        private Vector3 point2Vector(Point3D p, double scale, Point3D v)
        {
            Point3D pos = p.toCopy();
            pos.translate(v);
            pos.scale(new Point3D(scale));
            return new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
        }

        /// <summary>
        /// 三次元データ表示開始
        /// 視点の設定をする
        /// 使い方例
        ///     priave void renderFrame()
        ///     {
        ///         renderFrameStart();
        ///         objectの描画
        ///         renderFrameEnd();
        ///      }
        /// </summary>
        public void renderFrameStart()
        {
            GL.ClearColor(mBackColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //  視界の設定
            Matrix4 modelView = Matrix4.LookAt(Vector3.UnitZ * 10 / mZoom, Vector3.Zero, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);
            GL.MultMatrix(ref mRotate);
            //  視体積の設定
            //Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
            //    MathHelper.PiOver4 / mZoom, mGlControl.Width / mGlControl.Height, 1.0f, 64.0f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4 / mZoom, 1.0f, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        /// <summary>
        /// 三次元データ表示終了
        /// バッファをスワップして表示
        /// </summary>
        public void rendeFrameEnd()
        {
            mGlControl.SwapBuffers();
        }

        /// <summary>
        /// 背景色の設定
        /// </summary>
        /// <param name="backColor"></param>
        public void setBackColor(Color4 backColor)
        {
            mBackColor = backColor;
        }

        /// <summary>
        /// ビューポートサイズの設定
        /// </summary>
        public void resize()
        {
            GL.Viewport(mGlControl.ClientRectangle);
        }

        /// <summary>
        /// 拡大率の設定
        /// </summary>
        /// <param name="zoom"></param>
        public void setZoom(float zoom)
        {
            mZoom *= (float)Math.Pow(1.2, zoom);
            if (mZoomMax < mZoom)
                mZoom = mZoomMax;
            if (mZoom < mZoomMin)
                mZoom = mZoomMin;
        }

        /// <summary>
        /// 視点(カメラ)の移動開始
        /// 通常はglControl_MouseDown()から呼ばれ、マウスの開始位置を指定する
        /// </summary>
        /// <param name="isRotate">回転/移動の選択</param>
        /// <param name="x">開始X座標</param>
        /// <param name="y">開始Y座標</param>
        public void setMoveStart(bool isRotate, float x, float y)
        {
            if (isRotate)
                mIsCameraRotating = true;
            else
                mIsTransrate = true;
            mCcurrent = new Vector2(x, y);
        }

        /// <summary>
        /// 視点(カメラ)の移動終了
        /// 通常はglControl_MouseUp()から呼ばれる
        /// </summary>
        public void setMoveEnd()
        {
            mIsCameraRotating = false;
            mIsTransrate = false;
            mPrevious = Vector2.Zero;
        }

        /// <summary>
        /// 視点(カメラ)の移動処理
        /// 通常はglControl_MosueMove()から呼ばれる
        /// </summary>
        /// <param name="x">移動X座標</param>
        /// <param name="y">移動Y座標</param>
        /// <returns></returns>
        public bool moveObject(float x, float y)
        {
            if (mIsCameraRotating) {
                //  回転
                mPrevious = mCcurrent;
                mCcurrent = new Vector2(x, y);
                Vector2 delta = mCcurrent - mPrevious;
                delta /= (float)Math.Sqrt(mGlControl.Width * mGlControl.Width + mGlControl.Height * mGlControl.Height);
                float length = delta.Length;
                if (0.0 < length) {
                    float rad = length * MathHelper.Pi;
                    float theta = (float)Math.Sin(rad) / length;
                    OpenTK.Quaternion after = new OpenTK.Quaternion(delta.Y * theta, delta.X * theta, 0.0f, (float)Math.Cos(rad));
                    mRotate *= Matrix4.CreateFromQuaternion(after);
                }
            } else if (mIsTransrate) {
                //  移動
                mPrevious = mCcurrent;
                mCcurrent = new Vector2(x, y);
                Vector2 delta = mCcurrent - mPrevious;
                mRotate *= Matrix4.CreateTranslation(delta.X * 4f / mGlControl.Width, -delta.Y * 4f / mGlControl.Height, 0f);
            } else {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 3D表示の画面コピー
        /// </summary>
        public void screenCopy()
        {
            BitmapSource bitmapSource = ylib.bitmap2BitmapSource(ToBitmap());
            System.Windows.Clipboard.SetImage(bitmapSource);
        }

        /// <summary>
        /// OpenGL画面の画面コピー
        /// </summary>
        /// <returns>Bitmap</returns>
        public System.Drawing.Bitmap ToBitmap()
        {
            //formhostをWindowで表示した時は描画される
            try {
                mGlControl.Refresh();
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(mWorldWidth, mWorldHeight);
                var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.ReadPixels(0, 0, mWorldWidth, mWorldHeight, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, bmpData.Scan0);
                bmp.UnlockBits(bmpData);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                return bmp;
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"GL3DLib ToBitmap : {e.Message}");
            }
            return null;
        }
    }
}
