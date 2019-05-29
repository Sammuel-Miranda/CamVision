#region HEAD
[assembly: global::System.Reflection.AssemblyDescription("Single Device Cam Vision")]
[assembly: global::System.Reflection.AssemblyCompany("Sammuel S. Miranda")]
[assembly: global::System.Reflection.AssemblyCopyright("Copyright © Sammuel S. Miranda 2019")]
[assembly: global::System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: global::System.Reflection.AssemblyFileVersion("1.0.0.0")]
[assembly: global::System.Reflection.AssemblyInformationalVersion("1.0.0.0")]
[assembly: global::System.Runtime.InteropServices.ComVisible(false)]
[assembly: global::System.Resources.NeutralResourcesLanguageAttribute("pt-BR")]
[assembly: global::System.Runtime.CompilerServices.SuppressIldasmAttribute] //https://blogs.msdn.microsoft.com/amb/2011/05/27/how-to-prevent-ildasm-from-disassembling-my-net-code/
[assembly: global::System.Reflection.AssemblyTitle("CamVision")]
[assembly: global::System.Reflection.AssemblyProduct("CamVision")]
[assembly: global::System.Runtime.InteropServices.Guid("3badc94d-abec-40e6-854b-6e2b331ab48f")]
#endregion
#region CODE
namespace Cam //Documentation: https://docs.microsoft.com/pt-br/windows/desktop/Multimedia/video-capture-reference | http://www.gentle.it/alvise/AVICAP.TXT
{
#region CLSS
    public class Capture : global::System.Windows.Forms.Control //https://stackoverflow.com/questions/16184659/how-to-copy-image-without-using-the-clipboard
    {
        internal const int StandardWidth = 640; //Never change this!
        internal const int StandardHeight = 480; //Never change this!
        private const int WM_USER = 1024;
        private const int WM_CAP_CONNECT = 1034;
        private const int WM_CAP_DISCONNECT = 1035;
        private const int WM_CAP_START = global::Cam.Capture.WM_USER;
        private const int WM_CAP_GET_VIDEOFORMAT = global::Cam.Capture.WM_CAP_START + 44;
        private const int WM_CAP_SET_VIDEOFORMAT = global::Cam.Capture.WM_CAP_START + 45;
        private const int WM_CAP_SET_PREVIEW = global::Cam.Capture.WM_CAP_START + 50;
        private const int WM_CAP_SET_CALLBACK_FRAME = global::Cam.Capture.WM_CAP_START + 5;
        private const int WM_CAP_GRAB_FRAME = global::Cam.Capture.WM_CAP_START + 60; 
#if CLIPBOARD
        private const int WM_CAP_GET_FRAME = 1084;
        private const int WM_CAP_COPY = 1054;
        private global::System.Windows.Forms.IDataObject tempObj;
#else
        //http://msdn.microsoft.com/en-us/library/windows/desktop/dd757688(v=vs.85).aspx | https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/NativeMethods.cs,233db30be1eb4c51
        [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential)] private struct VIDEOHDR
        {
            public global::System.IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesUsed;
            public uint dwTimeCaptured;
            public global::System.IntPtr dwUser;
            public uint dwFlags;
            [global::System.Runtime.InteropServices.MarshalAs(global::System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 4)] public global::System.UIntPtr[] dwReserved;
        }

        [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential)] private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential)] private struct BITMAPINFO
        {
            public global::Cam.Capture.BITMAPINFOHEADER bmiHeader;
            public int bmiColors;
        }
#endif
        private delegate global::System.IntPtr capVideoStreamCallbackMethod(global::System.UIntPtr hWnd, ref global::Cam.Capture.VIDEOHDR lpVHdr);
        [global::System.Runtime.InteropServices.DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA")] private static extern int capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, int hwndParent, int nID);
        [global::System.Runtime.InteropServices.DllImport("user32", EntryPoint = "SendMessage")] private static extern int SendMessage(int hWnd, uint Msg, int wParam, global::Cam.Capture.capVideoStreamCallbackMethod routine);
        [global::System.Runtime.InteropServices.DllImport("user32", EntryPoint = "SendMessage")] private static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        [global::System.Runtime.InteropServices.DllImport("user32", EntryPoint = "SendMessage")] private static extern int SendMessage(int hWnd, uint wMsg, int wParam, ref global::Cam.Capture.BITMAPINFO lParam);
#if CLIPBOARD
        [global::System.Runtime.InteropServices.DllImport("user32", EntryPoint = "OpenClipboard")] private static extern int OpenClipboard(int hWnd);
        [global::System.Runtime.InteropServices.DllImport("user32", EntryPoint = "EmptyClipboard")] private static extern int EmptyClipboard();
        [global::System.Runtime.InteropServices.DllImport("user32", EntryPoint = "CloseClipboard")] private static extern int CloseClipboard();
#endif
        private global::System.ComponentModel.IContainer components;
        private global::System.Windows.Forms.Timer worker;
        private int m_Interval = 20; //Machine standard
        //This can be the problem: //https://www.codeguru.com/cpp/g-m/multimedia/video/article.php/c1601/Wrapper-for-AVICap-Window.htm
        //NB: valid NTSC formats : 640x480, 320x240, 160x120 etc.
        //	  valid PAL  formats : 768x576, 384x288, 196x144 etc.
        private int m_Width = (global::Cam.Capture.StandardWidth / 2);
        private int m_Height = (global::Cam.Capture.StandardHeight / 2);
        private int mCapHwnd = int.MinValue;
        private global::System.Drawing.Image tempImg;
        private bool bStopped = true;
        private bool bStarted = false;
#if !CLIPBOARD
        private bool bCapturing = false;
#endif
        private global::System.Drawing.Pen crossPen;
        public bool Stopped { get { return this.bStopped; } }
        public bool Paused { get { return (this.bStopped && this.worker == null && this.tempImg != null); } }
        public int FrameInterval { get { return this.m_Interval; } set { if (this.bStopped) { this.m_Interval = (value < 1 ? 1 : value); } } }
        public int CaptureHeight { get { return this.m_Height; } set { if (this.bStopped) { this.m_Height = value; } } }
        public int CaptureWidth { get { return this.m_Width; } set { if (this.bStopped) { this.m_Width = value; } } }
        private void InitPen() { if (this.crossPen == null) { this.crossPen = new global::System.Drawing.Pen(global::System.Drawing.Color.Yellow, 1.0f); } }
        public event global::System.EventHandler OnStart;
        public event global::System.EventHandler OnStop;
        public event global::System.EventHandler OnTick;
        internal bool DebugStart;
#if !CLAHE && !HISTOGRAM
        private static global::System.Drawing.Imaging.ImageAttributes grayAttributes;

        private static void InitAtt()
        {
            if (global::Cam.Capture.grayAttributes == null)
            {
                global::Cam.Capture.grayAttributes = new global::System.Drawing.Imaging.ImageAttributes();
                grayAttributes.SetColorMatrix(new global::System.Drawing.Imaging.ColorMatrix(new float[][] { new float[] { .3f, .3f, .3f, 0, 0 }, new float[] { .59f, .59f, .59f, 0, 0 }, new float[] { .11f, .11f, .11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 } }), global::System.Drawing.Imaging.ColorMatrixFlag.Default, global::System.Drawing.Imaging.ColorAdjustType.Bitmap);
            }
        }
#endif
        protected override void OnPaint(global::System.Windows.Forms.PaintEventArgs e)
        {
            if (this.tempImg == null) { e.Graphics.Clear(this.BackColor); } else { e.Graphics.DrawImage(this.tempImg, 0, 0, this.Width, this.Height); }
            this.InitPen();
            int midX = this.Width / 2;
            int midY = this.Height / 2;
            int psSTlow = midX - 5;
            int psSThig = midX + 5;
            int psHTlow = midX - 10;
            int psHThig = midX + 10;
            int cnt = 0;
            int Jump = (int)global::System.Math.Floor(((double)this.Height / (double)global::Cam.Capture.StandardHeight) * 7.00);
            int pos = midY - Jump;
            while (pos > 0)
            {
                if (cnt == 5)
                {
                    e.Graphics.DrawLine(this.crossPen, psHTlow, pos, psHThig, pos);
                    cnt = 0;
                }
                else
                {
                    e.Graphics.DrawLine(this.crossPen, psSTlow, pos, psSThig, pos);
                    cnt++;
                }
                pos -= Jump;
            }
            pos = midY + Jump;
            cnt = 0;
            while (pos < this.Height)
            {
                if (cnt == 5)
                {
                    e.Graphics.DrawLine(this.crossPen, psHTlow, pos, psHThig, pos);
                    cnt = 0;
                }
                else
                {
                    e.Graphics.DrawLine(this.crossPen, psSTlow, pos, psSThig, pos);
                    cnt++;
                }
                pos += Jump;
            }
            psSTlow = midY - 5;
            psSThig = midY + 5;
            psHTlow = midY - 10;
            psHThig = midY + 10;
            Jump = (int)global::System.Math.Floor(((double)this.Width / (double)global::Cam.Capture.StandardWidth) * 7.00);
            pos = midX - Jump;
            cnt = 0;
            while (pos > 0)
            {
                if (cnt == 5)
                {
                    e.Graphics.DrawLine(this.crossPen, pos, psHTlow, pos, psHThig);
                    cnt = 0;
                }
                else
                {
                    e.Graphics.DrawLine(this.crossPen, pos, psSTlow, pos, psSThig);
                    cnt++;
                }
                pos -= Jump;
            }
            pos = midX + Jump;
            cnt = 0;
            while (pos < this.Width)
            {
                if (cnt == 5)
                {
                    e.Graphics.DrawLine(this.crossPen, pos, psHTlow, pos, psHThig);
                    cnt = 0;
                }
                else
                {
                    e.Graphics.DrawLine(this.crossPen, pos, psSTlow, pos, psSThig);
                    cnt++;
                }
                pos += Jump;
            }
            e.Graphics.DrawLine(this.crossPen, midX, 0, midX, this.Height);
            e.Graphics.DrawLine(this.crossPen, 0, midY, this.Width, midY);
            base.OnPaint(e);
        }
#if HISTOGRAM //https://csharpvault.com/image-histogram-processing/
        private static float[] histR = new float[256];
        private static float[] histG = new float[256];
        private static float[] histB = new float[256];
        private static int[] temp = new int[256];

        public enum Rgb : byte
        {
            R = 0,
            G = 1,
            B = 2
        }

        public unsafe static int[] GetHistogram(global::System.Drawing.Bitmap Image, global::Cam.Capture.Rgb component)
        {
            for (int i = 0; i < 256; i++) { global::Cam.Capture.temp[i] = 0; }
            global::System.Drawing.Imaging.BitmapData data = Image.LockBits(new global::System.Drawing.Rectangle(0, 0, Image.Width, Image.Height), global::System.Drawing.Imaging.ImageLockMode.ReadWrite, global::System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            int offset = data.Stride - Image.Width * 3;
            byte* p = (byte*)data.Scan0.ToPointer();
            for (var i = 0; i < Image.Height; i++)
            {
                for (var j = 0; j < Image.Width; j++, p += 3)
                    switch (component)
                    {
                        case global::Cam.Capture.Rgb.R: global::Cam.Capture.temp[p[2]]++; break;
                        case global::Cam.Capture.Rgb.G: global::Cam.Capture.temp[p[1]]++; break;
                        default: global::Cam.Capture.temp[p[0]]++; break;
                    }
                p += offset;
            }
            Image.UnlockBits(data);
            return global::Cam.Capture.temp;
        }

        public unsafe static global::System.Drawing.Bitmap EqualizeHistogram(global::System.Drawing.Bitmap Image)
        {
            global::System.Drawing.Bitmap finalImg = new global::System.Drawing.Bitmap(Image.Width, Image.Height);
            int[] rHistogram = global::Cam.Capture.GetHistogram(Image, Rgb.R);
            int[] gHistogram = global::Cam.Capture.GetHistogram(Image, Rgb.G);
            int[] bHistogram = global::Cam.Capture.GetHistogram(Image, Rgb.B);
            var data = Image.LockBits(new global::System.Drawing.Rectangle(0, 0, Image.Width, Image.Height), global::System.Drawing.Imaging.ImageLockMode.ReadWrite, Image.PixelFormat);
            var finalData = finalImg.LockBits(new global::System.Drawing.Rectangle(0, 0, finalImg.Width, finalImg.Height), global::System.Drawing.Imaging.ImageLockMode.ReadWrite, Image.PixelFormat);
            global::Cam.Capture.histR[0] = (rHistogram[0] * rHistogram.Length) / (finalData.Width * finalData.Height);
            global::Cam.Capture.histG[0] = (gHistogram[0] * gHistogram.Length) / (finalData.Width * finalData.Height);
            global::Cam.Capture.histB[0] = (bHistogram[0] * bHistogram.Length) / (finalData.Width * finalData.Height);
            for (int i = 1; i < 256; i++)
            {
                global::Cam.Capture.histR[i] = 0;
                global::Cam.Capture.histG[i] = 0;
                global::Cam.Capture.histB[i] = 0;
            }
            long cumulativeR = rHistogram[0];
            long cumulativeG = gHistogram[0];
            long cumulativeB = bHistogram[0];
            for (var i = 1; i < histR.Length; i++)
            {
                cumulativeR += rHistogram[i];
                global::Cam.Capture.histR[i] = (cumulativeR * rHistogram.Length) / (finalData.Width * finalData.Height);
                cumulativeG += gHistogram[i];
                global::Cam.Capture.histG[i] = (cumulativeG * gHistogram.Length) / (finalData.Width * finalData.Height);
                cumulativeB += bHistogram[i];
                global::Cam.Capture.histB[i] = (cumulativeB * bHistogram.Length) / (finalData.Width * finalData.Height);
            }
            byte* ptr = (byte*)data.Scan0;
            byte* ptrFinal = (byte*)finalData.Scan0;
            int remain = data.Stride - data.Width * 3;
            for (var i = 0; i < data.Height; i++, ptr += remain, ptrFinal += remain)
            {
                for (var j = 0; j < data.Width; j++, ptrFinal += 3, ptr += 3)
                {
                    byte intensityR = ptr[2];
                    byte intensityG = ptr[1];
                    byte intensityB = ptr[0];
                    byte nValueR = (byte)global::Cam.Capture.histR[intensityR];
                    byte nValueG = (byte)global::Cam.Capture.histG[intensityG];
                    byte nValueB = (byte)global::Cam.Capture.histB[intensityB];
                    if (global::Cam.Capture.histR[intensityR] < 255.0f) { nValueR = 255; }
                    if (global::Cam.Capture.histG[intensityG] < 255.0f) { nValueG = 255; }
                    if (global::Cam.Capture.histB[intensityB] < 255.0f) { nValueB = 255; }
                    ptrFinal[2] = nValueR;
                    ptrFinal[1] = nValueG;
                    ptrFinal[0] = nValueB;
                }
            }
            Image.UnlockBits(data);
            finalImg.UnlockBits(finalData);
            return finalImg;
        }
#endif
#if CLAHE
        private class Bitplane
        {
            public int Width;
            public int Height;
            public byte[,] PixelData;
            public byte GetPixel(int x, int y) { return this.PixelData[y, x]; }
            public void SetPixel(int x, int y, byte value) { this.PixelData[y, x] = value; }

            public Bitplane(int w, int h)
            {
                this.Width = w;
                this.Height = h;
                this.PixelData = new byte[h, w];
            }

            public Bitplane(global::Cam.Capture.Bitplane bitplane) : this(bitplane.Width, bitplane.Height) { for (int y = 0; y < this.Height; ++y) { for (int x = 0; x < this.Width; ++x) { this.SetPixel(x, y, bitplane.GetPixel(x, y)); } } }
        }

        private class MyImage
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int NumCh { get; set; }
            public global::System.Collections.Generic.List<global::Cam.Capture.Bitplane> Bitplane = new global::System.Collections.Generic.List<global::Cam.Capture.Bitplane>();

            public global::System.Drawing.Bitmap GetBitmap()
            {
                global::System.Drawing.Bitmap bmp;
                switch (NumCh)
                {
                    case 1: bmp = new global::System.Drawing.Bitmap(Width, Height, global::System.Drawing.Imaging.PixelFormat.Format8bppIndexed); break;
                    case 2: bmp = new global::System.Drawing.Bitmap(Width, Height, global::System.Drawing.Imaging.PixelFormat.Format16bppGrayScale); break;
                    case 3: bmp = new global::System.Drawing.Bitmap(Width, Height, global::System.Drawing.Imaging.PixelFormat.Format24bppRgb); break;
                    case 4: bmp = new global::System.Drawing.Bitmap(Width, Height, global::System.Drawing.Imaging.PixelFormat.Format32bppArgb); break;
                    default: bmp = new global::System.Drawing.Bitmap(Width, Height, global::System.Drawing.Imaging.PixelFormat.Format8bppIndexed); break;
                }
                byte[] pixels = new byte[this.Width * this.Height * this.NumCh];
                int pos = 0;
                for (int y = 0; y < Height; ++y) { for (int x = 0; x < Width; ++x) { for (int ch = 0; ch < NumCh; ++ch) { pixels[pos++] = this.Bitplane[ch].GetPixel(x, y); } } }
                global::System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new global::System.Drawing.Rectangle(0, 0, Width, Height), global::System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                global::System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bd.Scan0, pixels.Length);
                bmp.UnlockBits(bd);
                return bmp;
            }

            public MyImage(global::System.Drawing.Bitmap bmp)
            {
                global::System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new global::System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), global::System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                switch (bmp.PixelFormat)
                {
                    case global::System.Drawing.Imaging.PixelFormat.Format8bppIndexed: this.NumCh = 1; break;
                    case global::System.Drawing.Imaging.PixelFormat.Format16bppGrayScale: this.NumCh = 2; break;
                    case global::System.Drawing.Imaging.PixelFormat.Format24bppRgb: this.NumCh = 3; break;
                    case global::System.Drawing.Imaging.PixelFormat.Format32bppArgb: this.NumCh = 4; break;
                    default: NumCh = 1; break;
                }
                byte[] pixels = new byte[bmp.Width * bmp.Height * this.NumCh];
                global::System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, pixels, 0, pixels.Length);
                bmp.UnlockBits(bd);
                this.Width = bmp.Width;
                this.Height = bmp.Height;
                for (int i = 0; i < NumCh; i++) { this.Bitplane.Add(new global::Cam.Capture.Bitplane(Width, Height)); }
                int pos = 0;
                for (int j = 0; j < Height; ++j) { for (int i = 0; i < Width; ++i) { for (int ch = 0; ch < NumCh; ++ch) { this.Bitplane[ch].SetPixel(i, j, pixels[pos++]); } } }
                bmp.Dispose();
            }

            public MyImage(int w, int h, int ch)
            {
                this.NumCh = ch;
                this.Width = w;
                this.Height = h;
                for (int i = 0; i < NumCh; i++) { this.Bitplane.Add(new global::Cam.Capture.Bitplane(Width, Height)); }
            }
        }

        private static void CreateWindow(ref global::Cam.Capture.Bitplane bitplane, int windowSize, ref global::Cam.Capture.Bitplane window, int y, int x)
        {
            int jIndex = 0;
            int iIndex;
            int i;
            for (int j = 0 - (windowSize / 2); j < (windowSize / 2); ++j)
            {
                iIndex = 0;
                for (i = 0 - (windowSize / 2); i < (windowSize / 2); ++i)
                {
                    int xx = x + i;
                    if (xx < 0) { xx = global::System.Math.Abs(xx); }
                    if (xx >= bitplane.Width) { xx = (bitplane.Width - 1) + ((bitplane.Width) - (xx + windowSize)); }
                    int yy = y + j;
                    if (yy < 0) { yy = global::System.Math.Abs(yy); }
                    if (yy >= bitplane.Height) { yy = (bitplane.Height - 1) + ((bitplane.Height) - (yy + windowSize)); }
                    window.SetPixel(iIndex, jIndex, bitplane.GetPixel(xx, yy));
                    ++iIndex;
                }
                ++jIndex;
            }
        }

        private static double findMin(double[] array)
        {
            double min = double.MaxValue;
            foreach (double value in array) { if (value < min) { min = value; } }
            return min;
        }

        private static double[] calculateHistogram(global::Cam.Capture.Bitplane bitplane)
        {
            double[] histogram = new double[256];
            int x;
            for (int y = 0; y < bitplane.Height; ++y) { for (x = 0; x < bitplane.Width; ++x) { ++histogram[bitplane.GetPixel(x, y)]; } }
            return histogram;
        }

        private static double[] calculateComulativeFrequency(double[] array)
        {
            int size = array.Length;
            double[] comulativeFreq = new double[size];
            comulativeFreq[0] = array[0];
            for (int i = 1; i < size; ++i) { comulativeFreq[i] = comulativeFreq[i - 1] + array[i]; }
            return comulativeFreq;
        }

        private static void CLHE(ref global::Cam.Capture.Bitplane bitplane, double contrastLimit)
        {
            double cl = (contrastLimit * (bitplane.Width * bitplane.Height)) / 256;
            double top = cl;
            double bottom = 0;
            double SUM = 0;
            int i;
            double[] histogram = global::Cam.Capture.calculateHistogram(bitplane);
            while (top - bottom > 1)
            {
                double middle = (top + bottom) / 2;
                SUM = 0;
                foreach (double value in histogram) { if (value > middle) { SUM += value - middle; } }
                if (SUM > (cl - middle) * 256) { top = middle; } else { bottom = middle; }
            }
            double clipLevel = global::System.Math.Round(bottom + (SUM / 256));
            double L = cl - clipLevel;
            for (i = 0; i < 256; i++) { if (histogram[i] >= clipLevel) { histogram[i] = clipLevel; } else { histogram[i] += L; } }
            double perBin = SUM / 256;
            for (i = 0; i < 256; i++) { histogram[i] += perBin; }
            histogram = global::Cam.Capture.calculateComulativeFrequency(histogram);
            int[] finalFreq = new int[256];
            double min = global::Cam.Capture.findMin(histogram);
            for (i = 0; i < 256; i++) { finalFreq[i] = (int)((histogram[i] - min) / ((bitplane.Width * bitplane.Height) - 2) * 255); }
            int x;
            for (int y = 0; y < bitplane.Height; ++y) { for (x = 0; x < bitplane.Width; ++x) { bitplane.SetPixel(x, y, (byte)finalFreq[bitplane.GetPixel(x, y)]); } }
        }

        private static void CLAHE(ref global::Cam.Capture.Bitplane bitplane, int windowSize, double contrastLimit)
        {
            global::Cam.Capture.Bitplane newBitplane = new global::Cam.Capture.Bitplane(bitplane.Width, bitplane.Height);
            global::Cam.Capture.Bitplane window = new global::Cam.Capture.Bitplane(windowSize, windowSize);
            int x;
            int y;
            for (y = 0; y < bitplane.Height; ++y)
            {
                for (x = 0; x < bitplane.Width; ++x)
                {
                    global::Cam.Capture.CreateWindow(ref bitplane, windowSize, ref window, y, x);
                    global::Cam.Capture.CLHE(ref window, contrastLimit);
                    newBitplane.SetPixel(x, y, window.GetPixel(windowSize / 2, windowSize / 2));
                }
            }
            for (y = 0; y < bitplane.Height; ++y) { for (x = 0; x < bitplane.Width; ++x) { bitplane.SetPixel(x, y, newBitplane.GetPixel(x, y)); } }
        }
#endif
        public static global::System.Drawing.Bitmap ConvertBlackAndWhite(global::System.Drawing.Bitmap Image)
        {
#if !CLAHE && !HISTOGRAM
            global::Cam.Capture.InitAtt();
            global::System.Drawing.Bitmap nImage = new global::System.Drawing.Bitmap(Image.Width, Image.Height);
            using (global::System.Drawing.Graphics g = global::System.Drawing.Graphics.FromImage(nImage)) { g.DrawImage(Image, new global::System.Drawing.Rectangle(0, 0, Image.Width, Image.Height), 0, 0, Image.Width, Image.Height, global::System.Drawing.GraphicsUnit.Pixel, global::Cam.Capture.grayAttributes); }
#endif
#if HISTOGRAM
            global::System.Drawing.Bitmap tImage = global::Cam.Capture.EqualizeHistogram(Image);
            Image.Dispose();
            Image = tImage;
#endif
#if CLAHE
            global::Cam.Capture.MyImage canvas = new global::Cam.Capture.MyImage(Image);
            Image.Dispose();
            global::Cam.Capture.Bitplane trade = null;
            for (int i = 0; i < canvas.Bitplane.Count; i++)
            {
                trade = canvas.Bitplane[i];
                global::Cam.Capture.CLAHE(ref trade, 200, 2.00);
                canvas.Bitplane[i] = trade;
            }
            Image = canvas.GetBitmap();
            canvas = null;
#endif
            return Image;
        }
#if !CLIPBOARD
        private global::System.IntPtr capVideoStreamCallback(global::System.UIntPtr hWnd, ref global::Cam.Capture.VIDEOHDR lpVHdr)
        {
            byte did = 0;
            try
            {
                byte[] buffOf = new byte[lpVHdr.dwBufferLength];
                global::System.Runtime.InteropServices.Marshal.Copy(lpVHdr.lpData, buffOf, 0, (int)lpVHdr.dwBufferLength);
                did = 1;
                global::System.Drawing.Bitmap bmp = new global::System.Drawing.Bitmap(this.m_Width, this.m_Height, global::System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                did = 2;
                global::System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new global::System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), global::System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                did = 3;
                global::System.Runtime.InteropServices.Marshal.Copy(buffOf, 0, bmpData.Scan0, buffOf.Length);
                did = 4;
                buffOf = null;
                bmp.UnlockBits(bmpData);
                bmpData = null;
                bmp.RotateFlip(global::System.Drawing.RotateFlipType.Rotate180FlipX);
                this.tempImg = global::Cam.Capture.ConvertBlackAndWhite(bmp);
                did = 5;
                bmp.Dispose();
                bmp = null;
                this.bCapturing = false;
                return System.IntPtr.Zero;
            }
            catch (global::System.Exception e)
            {
                global::System.Windows.Forms.MessageBox.Show("Erro ao Converter o Bitmap. (D.I.D.C. " + did.ToString() + ")" + global::System.Environment.NewLine + e.Message);
                throw e;
            }
        }
#endif
        private void Tick(object sender, global::System.EventArgs e)
        {
            try
            {
                this.worker.Stop();
#if CLIPBOARD
                global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_GET_FRAME, 0, 0);
                global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_COPY, 0, 0);
                this.tempObj = global::System.Windows.Forms.Clipboard.GetDataObject();
                this.tempImg = (global::System.Drawing.Bitmap)this.tempObj.GetData(global::System.Windows.Forms.DataFormats.Bitmap);
#else
                this.bCapturing = true;
                global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_GRAB_FRAME, 0, 0);
                while (this.bCapturing) { /* NOTHING */ }
#endif
                this.Invalidate();
                global::System.Windows.Forms.Application.DoEvents();
                if (this.OnTick != null) { this.OnTick(this, global::System.EventArgs.Empty); }
                if (!this.bStopped) { this.worker.Start(); }
            }
            catch (global::System.Exception excep)
            {
                global::System.Windows.Forms.MessageBox.Show("Ocorreu um erro ao Capturar a Imagem. A captura de será encerrada." + global::System.Environment.NewLine + excep.Message);
                this.Stop();
            }
        }

        public void Disconnect()
        {
            global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_DISCONNECT, 0, 0);
            this.bStarted = false;
            if (this.OnStop != null) { this.OnStop(this, global::System.EventArgs.Empty); }
        }

        public bool Pause()
        {
            if (!this.bStopped)
            {
                this.bStopped = true;
                if (this.worker != null)
                {
                    this.worker.Stop();
                    this.worker = null;
                }
                this.Disconnect();
                return true;
            } else { return false; }
        }

        public bool Stop()
        {
            try
            {
                bool RunPaused = this.Pause();
                if (this.tempImg != null) { this.tempImg.Dispose(); }
                this.tempImg = null;
                global::System.Windows.Forms.Application.DoEvents();
                this.Invalidate();
                return RunPaused;
            } catch { /* NOTHING */ }
            return false;
        }

        public void Start()
        {
            byte did = 0;
            try
            {
                if (!this.bStarted)
                {
                    this.Stop();
#if USECTRLASWINDOW
                    this.mCapHwnd = global::Cam.Capture.capCreateCaptureWindowA("WebCap", 0, 0, 0, this.m_Width, this.m_Height, this.Handle.ToInt32(), 0);
#else
                    this.mCapHwnd = global::Cam.Capture.capCreateCaptureWindowA("WebCap", 0, 0, 0, this.m_Width, this.m_Height, 0, 0);
#endif
#if !CLIPBOARD
                    global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_SET_CALLBACK_FRAME, 0, this.capVideoStreamCallback);
#endif
                    global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_CONNECT, 0, 0);
#if !CLIPBOARD
                    did = 1;
                    if (this.DebugStart) { global::System.IO.File.WriteAllText(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.ApplicationData) + "\\debug.camx.txt", "PRECONNET"); }
                    if (global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_GET_VIDEOFORMAT, 0, 0) > 0) //https://www.codeproject.com/Questions/566966/howplustoplussetplusresolutionplusofpluswebpluscam
                    {
                        global::Cam.Capture.BITMAPINFO bInfo = new global::Cam.Capture.BITMAPINFO();
                        bInfo.bmiHeader = new global::Cam.Capture.BITMAPINFOHEADER();
                        did = 2;
                        global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_GET_VIDEOFORMAT, global::System.Runtime.InteropServices.Marshal.SizeOf(bInfo), ref bInfo);
                        if (this.DebugStart)
                        {
                            global::System.Text.StringBuilder tech = new global::System.Text.StringBuilder();
                            tech.Append("{bInfo.bmiColors:" + bInfo.bmiColors.ToString());
                            tech.Append("[bInfo.bmiHeader.biBitCount:" + bInfo.bmiHeader.biBitCount.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biClrImportant:" + bInfo.bmiHeader.biClrImportant.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biClrUsed:" + bInfo.bmiHeader.biClrUsed.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biCompression:" + bInfo.bmiHeader.biCompression.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biHeight:" + bInfo.bmiHeader.biHeight.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biPlanes:" + bInfo.bmiHeader.biPlanes.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biSize:" + bInfo.bmiHeader.biSize.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biSizeImage:" + bInfo.bmiHeader.biSizeImage.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biWidth:" + bInfo.bmiHeader.biWidth.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biXPelsPerMeter:" + bInfo.bmiHeader.biXPelsPerMeter.ToString() + "|");
                            tech.Append("bInfo.bmiHeader.biYPelsPerMeter:" + bInfo.bmiHeader.biYPelsPerMeter.ToString() + "]}");
                            global::System.IO.File.AppendAllText(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.ApplicationData) + "\\debug.camx.txt", tech.ToString());
                            tech = null;
                        }
                        did = 3;
                        bInfo.bmiHeader.biSize = (uint)global::System.Runtime.InteropServices.Marshal.SizeOf(bInfo.bmiHeader);
                        bInfo.bmiHeader.biBitCount = 16; //16rgb555
                        bInfo.bmiHeader.biSizeImage = (uint)(bInfo.bmiHeader.biWidth * bInfo.bmiHeader.biHeight * (int)bInfo.bmiHeader.biBitCount / 8);
                        global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_SET_VIDEOFORMAT, global::System.Runtime.InteropServices.Marshal.SizeOf(bInfo), ref bInfo);
                        did = 4;
                    }
                    if (this.DebugStart) { global::System.IO.File.AppendAllText(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.ApplicationData) + "\\debug.camx.txt", ("DID:" + did.ToString())); }
#endif
                    global::Cam.Capture.SendMessage(this.mCapHwnd, global::Cam.Capture.WM_CAP_SET_PREVIEW, 0, 0);
                    global::System.Windows.Forms.Application.DoEvents();
                }
                this.worker = new global::System.Windows.Forms.Timer(this.components);
                this.worker.Tick += new global::System.EventHandler(this.Tick);
                this.worker.Interval = this.m_Interval;
                this.bStopped = false;
                this.worker.Start();
                if (this.OnStart != null) { this.OnStart(this, global::System.EventArgs.Empty); }
            }
            catch (global::System.Exception excep)
            {
                global::System.Windows.Forms.MessageBox.Show("Ocorreu um erro ao iniciar a Câmera. (D.I.D.S. " + did.ToString() + ")" + global::System.Environment.NewLine + excep.Message);
                this.Stop();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null) { this.components.Dispose(); }
            if (this.tempImg != null) { this.tempImg.Dispose(); }
            if (this.crossPen != null) { this.crossPen.Dispose(); }
            if (this.worker != null) { this.worker.Dispose(); }
            this.tempImg = null;
            this.crossPen = null;
            this.worker = null;
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SetStyle(global::System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, true);
            this.BackColor = global::System.Drawing.SystemColors.ControlDark;
            this.components = new global::System.ComponentModel.Container();
            this.Name = "Capture";
            this.Size = new global::System.Drawing.Size(342, 252);
            this.InitPen();
        }

        protected override void OnResize(global::System.EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        public Capture() { this.InitializeComponent(); }
        ~Capture() { this.Stop(); }
    }
#endregion
#region FORM
    internal class FormMain : global::System.Windows.Forms.Form
    {
        private bool wasMinimized = false;
        private global::Cam.Capture cap;
#if CANPAUSE
        private const string infoHowToPause = "Duplo Click (Esquerdo) para Pausar, Duplo Click (Direito) para Restaurar Tamanho original";
        private const string infoHowToStart = "Duplo Click (Esquerdo) para Reiniciar, Duplo Click (Direito) para Restaurar Tamanho original";
        private global::System.Windows.Forms.Label info;
        private void cap_Stop(object sender, global::System.EventArgs e) { this.info.Text = global::Cam.FormMain.infoHowToStart; }
        private void cap_Start(object sender, global::System.EventArgs e) { this.info.Text = global::Cam.FormMain.infoHowToPause; }
#endif
        private void cap_Tick(object sender, global::System.EventArgs e)
        {
            this.SuspendLayout();
            this.Location = new global::System.Drawing.Point(0, 0);
            this.cap.OnTick -= this.cap_Tick;
            this.ResumeLayout(true);
        }

        private void cap_DoubleClick(object sender, global::System.EventArgs e)
        {
            if (((global::System.Windows.Forms.MouseEventArgs)e).Button == global::System.Windows.Forms.MouseButtons.Right)
            {
                this.WindowState = global::System.Windows.Forms.FormWindowState.Normal;
                this.ClientSize = new global::System.Drawing.Size(global::Cam.Capture.StandardWidth, global::Cam.Capture.StandardHeight);
            }
            else
            {
#if CANPAUSE
                if (this.cap.Stopped)
                {
                    this.cap.Start();
                    this.info.Text = global::Cam.FormMain.infoHowToPause;
                }
                else
                {
                    this.cap.Pause();
                    this.info.Text = global::Cam.FormMain.infoHowToStart;
                }
#else
                if (this.cap.Stopped) { this.cap.Start(); } else { this.cap.Stop(); }
#endif
            }
        }

        protected override void OnShown(global::System.EventArgs e)
        {
            base.OnShown(e);
            this.MinimumSize = this.Size; //With full Window Size (Title bar, once showed)
#if CANPAUSE
            this.cap.Start();
#endif
        }

        protected override void OnClosing(global::System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            try { if (!this.cap.Stop()) { this.cap.Disconnect(); } } catch { /* NOTHING */ }
        }

        protected override void OnResize(global::System.EventArgs e)
        {
            base.OnResize(e);
            if (this.WindowState == global::System.Windows.Forms.FormWindowState.Minimized)
            {
                this.wasMinimized = true;
                this.cap.Pause();
            }
            else if (this.wasMinimized)
            {
                this.wasMinimized = false;
                this.cap.Start();
            }
        }

        [global::System.STAThread] internal static int Main(string[] args)
        {
            global::System.Windows.Forms.Application.EnableVisualStyles();
            global::System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            if (args == null) { args = new string[0]; }
            global::System.Windows.Forms.Application.Run(new global::Cam.FormMain(args));
            return 0;
        }

        public FormMain(string[] args) : base()
        {
            this.TopMost = true;
#if DEBUG
#if CLIPBOARD
            this.Text = "Cam - Clipboard, Single Device";
#else
            this.Text = "Cam - function CallBack, 180 flip, Single Device";
#endif
#else
            this.Text = "Cam Vision Single Device";
#endif
            this.Font = new global::System.Drawing.Font("Verdana", 9.75f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 136);
            this.AutoScaleDimensions = new global::System.Drawing.SizeF(96.0f, 96.0f);
            this.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new global::System.Drawing.Size(global::Cam.Capture.StandardWidth, global::Cam.Capture.StandardHeight);
            this.cap = new global::Cam.Capture() { Name = "cap", Dock = global::System.Windows.Forms.DockStyle.Fill };
            this.cap.DoubleClick += this.cap_DoubleClick;
            //this.cap.OnTick += this.cap_Tick;
#if CANPAUSE
            this.cap.OnStart += this.cap_Start;
            this.cap.OnStop += this.cap_Stop;
            this.info = new global::System.Windows.Forms.Label() { Name = "info", AutoSize = false, Dock = global::System.Windows.Forms.DockStyle.Bottom, Height = 20, Text = global::Cam.FormMain.infoHowToStart };
            this.ClientSize = new global::System.Drawing.Size(this.ClientSize.Width, (this.ClientSize.Height + 20));
            this.Controls.Add(this.info);
#endif
            this.Controls.Add(this.cap);
            if (args != null && args.Length > 0)
            {
                try
                {
                    int arg = int.Parse(args[0]);
                    if (arg > 0) { this.cap.FrameInterval = arg; }
                    if (args.Length > 2)
                    {
                        arg = int.Parse(args[1]);
                        if (arg > 0) { this.cap.CaptureWidth = arg; }
                        arg = int.Parse(args[2]);
                        if (arg > 0) { this.cap.CaptureHeight = arg; }
                        if (args.Length > 3)
                        {
                            arg = int.Parse(args[3]);
                            if (arg == 1) { this.cap.DebugStart = true; }
                        }
                    }
#if DEBUG
                    global::System.Windows.Forms.MessageBox.Show("Argumento FrameInterval:" + this.cap.FrameInterval.ToString());
#endif
                } catch { /* NOTHING */ }
            }
        }

        public FormMain() : this(new string[0]) { /* NOTHING */ }
    }
#endregion
}
#endregion
