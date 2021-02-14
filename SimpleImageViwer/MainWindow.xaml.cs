using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleImageViwer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point start;
        private Point origin;
        private Point relativeOrigin;
        private Boolean isZoomed = false;

        private TranslateTransform GetTT()
        {
            var tg = ImageCanvas.RenderTransform as TransformGroup;
            var tt = tg.Children[1] as TranslateTransform;
            return tt;
        }

        private ScaleTransform GetST()
        {
            var tg = ImageCanvas.RenderTransform as TransformGroup;
            var st = tg.Children[0] as ScaleTransform;
            return st;
        }
        private void DebugPosition()
        {
            var tt = GetTT();
            Trace.WriteLine($"Origin: ({origin.X},{origin.Y})");
            Trace.WriteLine($"Relative Origin: ({relativeOrigin.X},{relativeOrigin.Y})");
            Trace.WriteLine($"Current ({tt.X},{tt.Y})");
        }

        private void Reset()
        {
            var tt = GetTT();
            var st = GetST();

            tt.X = 0;
            tt.Y = 0;
            st.ScaleX = 1;
            st.ScaleY = 1;

            isZoomed = false;
        }

        public MainWindow()
        {
            InitializeComponent();
            var args = Environment.GetCommandLineArgs().Skip(1);
            
            if(args.Count() > 0)
            {
                var filePath = args.First();
                ImageCanvas.Source = new BitmapImage(new Uri(filePath));
                this.Title = filePath.Split(@"\").Last();
            } 
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TranslateTransform tt = GetTT();
            start = e.GetPosition(this);
            origin = new Point(tt.X, tt.Y);
            relativeOrigin = ImageCanvas.TransformToAncestor(this).Transform(new Point(0, 0));

            this.Cursor = Cursors.SizeAll;
            ImageCanvas.CaptureMouse();

        }

        private void ImageCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tt = GetTT();
            ImageCanvas.ReleaseMouseCapture();
            this.Cursor = Cursors.Arrow;

            relativeOrigin = ImageCanvas.TransformToAncestor(this).Transform(new Point(0, 0));
        }

        private void ImageCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ImageCanvas.IsMouseCaptured)
            {
                //Point relativePoint = ImageCanvas.TransformToAncestor(this).Transform(new Point(0, 0));
                var tt = GetTT();
                var st = GetST();
                Vector delta = start - e.GetPosition(this);
                tt.X = origin.X - delta.X;
                tt.Y = origin.Y - delta.Y;

            }
        }

        private void ImageCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleTransform st = GetST();
            TranslateTransform tt = GetTT();

            double zoom = e.Delta > 0 ? 0.15 : -0.15;

            Point relative = e.GetPosition(ImageCanvas);
            double absoluteX = relative.X * st.ScaleX + tt.X;
            double absoluteY = relative.Y * st.ScaleY + tt.Y;

            if (e.Delta < 0)
            {
                // If scrolldown reset everything
                Reset();
            }
            else
            {
                isZoomed = true;
                st.ScaleX += zoom;
                st.ScaleY += zoom;
                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;
            }
        }

        private void ImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Reset zoom and position
            if (e.ChangedButton == MouseButton.Middle) { Reset(); }
        }
    }
}
