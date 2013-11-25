using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Matrix2D = System.Drawing.Drawing2D.Matrix;

namespace ComputerAlgebra.Plotting
{
    /// <summary>
    /// A single plot window.
    /// </summary>
    public class Plot
    {
        protected SeriesCollection series;
        /// <summary>
        /// The series displayed in this plot.
        /// </summary>
        public SeriesCollection Series { get { return series; } }

        /// <summary>
        /// Width of the plot window.
        /// </summary>
        public int Width { get { return form.Width; } set { form.Width = value; } }
        /// <summary>
        /// Height of the plot window.
        /// </summary>
        public int Height { get { return form.Height; } set { form.Height = value; } }

        private double _x0 = -10.0, _x1 = 10.0;
        private double _y0 = double.NaN, _y1 = double.NaN;
        /// <summary>
        /// Plot area bounds.
        /// </summary>
        public double x0 { get { return _x0; } set { _x0 = value; Invalidate(); } }
        public double y0 { get { return _y0; } set { _y0 = value; Invalidate(); } }
        public double x1 { get { return _x1; } set { _x1 = value; Invalidate(); } }
        public double y1 { get { return _y1; } set { _y1 = value; Invalidate(); } }

        private string xlabel = null, ylabel = null;
        /// <summary>
        /// Axis labels.
        /// </summary>
        public string xLabel { get { return xlabel; } set { xlabel = value; Invalidate(); } }
        public string yLabel { get { return ylabel; } set { ylabel = value; Invalidate(); } }

        string title = null;
        /// <summary>
        /// Title of the plot window.
        /// </summary>
        public string Title { get { return title; } set { form.Text = title = value; Invalidate(); } }

        protected Thread thread;
        protected Form form = new Form();

        public Plot()
        {
            series = new SeriesCollection();
            series.ItemAdded += (o, e) => Invalidate();
            series.ItemRemoved += (o, e) => Invalidate();

            form = new Form() 
            { 
                Text = "Plot",
                Width = 300, 
                Height = 300,
            };
            form.Paint += Plot_Paint;
            form.SizeChanged += Plot_SizeChanged;

            thread = new Thread(() => Application.Run(form));
            thread.Start();
        }

        private void Plot_Paint(object sender, PaintEventArgs e)
        {
            if (series.Count == 0)
                return;

            Graphics G = e.Graphics;
            G.SmoothingMode = SmoothingMode.AntiAlias;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Pen axis = Pens.Black;
            Pen grid = Pens.LightGray;
            Font font = SystemFonts.DefaultFont;
            Font labelFont = new Font(font.FontFamily, font.Size * 1.25f);
            Font titleFont = new Font(font.FontFamily, font.Size * 1.5f, FontStyle.Bold);

            // Compute plot area.
            SizeF marginTopLeft = new SizeF(30.0f, 10.0f);
            SizeF marginBottomRight = new SizeF(10.0f, 20.0f);

            if (title != null)
                marginTopLeft.Height += 20.0f;
            if (ylabel != null)
                marginTopLeft.Width += 20.0f;
            if (xlabel != null)
                marginBottomRight.Height += 20.0f;
            RectangleF area = new RectangleF((PointF)marginTopLeft, (SizeF)e.ClipRectangle.Size - marginTopLeft - marginBottomRight);

            // Draw background.
            G.FillRectangle(Brushes.White, area);
            G.DrawRectangle(Pens.Gray, area.Left, area.Top, area.Width, area.Height);

            // Draw title.
            if (title != null)
            {
                SizeF sz = G.MeasureString(title, font);
                G.DrawString(title, titleFont, Brushes.Black, new PointF(area.Left + (area.Width - sz.Width) / 2, e.ClipRectangle.Top + 5.0f));
            }

            // Draw axis labels.
            if (ylabel != null)
            {
                SizeF sz = G.MeasureString(ylabel, font);
                G.TranslateTransform(e.ClipRectangle.Left + 5.0f, area.Top + (area.Height + sz.Width) / 2.0f);
                G.RotateTransform(-90.0f);
                G.DrawString(ylabel, labelFont, Brushes.Black, new PointF(0.0f, 0.0f));
                G.ResetTransform();
            }
            if (xlabel != null)
            {
                SizeF sz = G.MeasureString(xlabel, font);
                G.DrawString(xlabel, labelFont, Brushes.Black, new PointF(area.Left + (area.Width - sz.Width) / 2, e.ClipRectangle.Bottom - sz.Height - 5.0f));
            }

            // Compute plot bounds.
            PointF x0, x1;
            if (double.IsNaN(_y0) || double.IsNaN(_y1))
            {
                double min = double.PositiveInfinity;
                double max = double.NegativeInfinity;
                series.ForEach(i =>
                {
                    min = Math.Min(min, i.MinY(_x0, _x1) - 1e-6f);
                    max = Math.Max(max, i.MaxY(_x0, _x1) + 1e-6f);
                });
                double y = (min + max) / 2.0;

                if (max <= min)
                    return;

                x0 = new PointF((float)_x0, (float)((min - y) * 1.25 + y));
                x1 = new PointF((float)_x1, (float)((max - y) * 1.25 + y));
            }
            else
            {
                x0 = new PointF((float)_x0, (float)_y0);
                x1 = new PointF((float)_x1, (float)_y1);
            }
            
            Matrix2D T = new Matrix2D(
                area, 
                new PointF[] { new PointF(x0.X, x1.Y), new PointF(x1.X, x1.Y), new PointF(x0.X, x0.Y) });
            T.Invert();
            
            // Draw axes.
            double dx = Partition((x1.X - x0.X) / (Width / 80));
            for (double x = x0.X - x0.X % dx; x <= x1.X; x += dx)
            {
                PointF tx = Tx(T, new PointF((float)x, 0.0f));
                string s = x.ToString("G3");
                SizeF sz = G.MeasureString(s, font);
                G.DrawString(s, font, Brushes.Black, new PointF(tx.X - sz.Width / 2, area.Bottom + 3.0f));
                G.DrawLine(grid, new PointF(tx.X, area.Top), new PointF(tx.X, area.Bottom));
            }

            double dy = Partition((x1.Y - x0.Y) / (Height / 50));
            for (double y = x0.Y - x0.Y % dy; y <= x1.Y; y += dy)
            {
                PointF tx = Tx(T, new PointF(0.0f, (float)y));
                string s = y.ToString("G3");
                SizeF sz = G.MeasureString(s, font);
                G.DrawString(s, font, Brushes.Black, new PointF(area.Left - sz.Width, tx.Y - sz.Height / 2));
                G.DrawLine(grid, new PointF(area.Left, tx.Y), new PointF(area.Right, tx.Y));
            }

            G.DrawLine(axis, Tx(T, new PointF(x0.X, 0.0f)), Tx(T, new PointF(x1.X, 0.0f)));
            G.DrawLine(axis, Tx(T, new PointF(0.0f, x0.Y)), Tx(T, new PointF(0.0f, x1.Y)));
            G.DrawRectangle(Pens.Gray, area.Left, area.Top, area.Width, area.Height);
            
            // Draw series.
            series.ForEach(i =>
            {
                if (i.Pen == null)
                    i.Pen = new Pen(colors.ArgMin(j => series.Count(k => k.Pen != null && k.Pen.Color == j)), 0.5f);
                i.Paint(T, x0.X, x1.X, G);
            });
        }

        private static PointF Tx(Matrix2D T, PointF x)
        {
            PointF[] xs = new[] { x };
            T.TransformPoints(xs);
            return xs[0];
        }

        private void Invalidate() { form.Invalidate(); }

        private void Plot_SizeChanged(object sender, EventArgs e) { form.Invalidate(); }

        private static double Partition(double P)
        {
            double[] Partitions = { 10.0, 4.0, 2.0 };

            double p = Math.Pow(10.0, Math.Ceiling(Math.Log10(P)));
            foreach (double i in Partitions)
                if (p / i > P)
                    return p / i;
            return p;
        }

        private static Color[] colors = new Color[]
        { 
            Color.Red, 
            Color.Blue, 
            Color.Green, 
            Color.DarkRed, 
            Color.DarkGreen, 
            Color.DarkBlue,
        };
    }
}
