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
        protected Thread thread;
        protected Form form = new Form();

        protected SeriesCollection series;
        public SeriesCollection Series { get { return series; } }

        /// <summary>
        /// Title of the plot window.
        /// </summary>
        public string Title { get { return form.Text; } set { form.Text = value; } }
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

        public double x0 { get { return _x0; } set { _x0 = value; Invalidate(); } }
        public double y0 { get { return _y0; } set { _y0 = value; Invalidate(); } }
        public double x1 { get { return _x1; } set { _x1 = value; Invalidate(); } }
        public double y1 { get { return _y1; } set { _y1 = value; Invalidate(); } }

        public Plot()
        {
            series = new SeriesCollection();
            series.ItemAdded += (o, e) => Invalidate();
            series.ItemRemoved += (o, e) => Invalidate();

            form = new Form() 
            { 
                Width = 300, 
                Height = 300 
            };
            form.Paint += Plot_Paint;
            form.SizeChanged += Plot_SizeChanged;

            thread = new Thread(() => Application.Run(form));
            thread.Start();
        }

        private void Plot_Paint(object sender, PaintEventArgs e)
        {
            if (!series.Any())
                return;

            PointF x0, x1;

            if (double.IsNaN(_y0) || double.IsNaN(_y1))
            {
                double min = series.Min(i => i.MinY(_x0, _x1), 0.0) - 1e-6;
                double max = series.Max(i => i.MaxY(_x0, _x1), 0.0) + 1e-6;
                double y = (min + max) / 2.0;

                x0 = new PointF((float)_x0, (float)((min - y) * 1.25 + y));
                x1 = new PointF((float)_x1, (float)((max - y) * 1.25 + y));
            }
            else
            {
                x0 = new PointF((float)_x0, (float)_y0);
                x1 = new PointF((float)_x1, (float)_y1);
            }

            Graphics G = e.Graphics;

            Matrix2D T = new Matrix2D(
                new RectangleF(new PointF(0.0f, 0.0f), (SizeF)e.ClipRectangle.Size), 
                new PointF[] { new PointF(x0.X, x1.Y), new PointF(x1.X, x1.Y), new PointF(x0.X, x0.Y) });
            T.Invert();
            G.SmoothingMode = SmoothingMode.AntiAlias;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Draw axes.
            Pen axis = Pens.Black;
            Font font = SystemFonts.DefaultFont;

            G.DrawLine(axis, Tx(T, new PointF(x0.X, 0.0f)), Tx(T, new PointF(x1.X, 0.0f)));
            G.DrawLine(axis, Tx(T, new PointF(0.0f, x0.Y)), Tx(T, new PointF(0.0f, x1.Y)));

            double dx = (x1.X - x0.X) / (Width / 100);
            double dy = (x1.Y - x0.Y) / (Height / 100);

            for (double x = x0.X; x <= x1.X; x += dx)
            {
                PointF tx = Tx(T, new PointF((float)x, 0.0f));
                G.DrawString(x.ToString("G3"), font, Brushes.Black, tx);
                G.DrawLine(axis, new PointF(tx.X, tx.Y - 5.0f), new PointF(tx.X, tx.Y + 5.0f));
            }
            for (double y = x0.Y; y <= x1.Y; y += dy)
            {
                PointF tx = Tx(T, new PointF(0.0f, (float)y));
                G.DrawString(y.ToString("G3"), font, Brushes.Black, tx);
                G.DrawLine(axis, new PointF(tx.X - 5.0f, tx.Y), new PointF(tx.X + 5.0f, tx.Y));
            }

            // Draw series.
            series.ForEach(i =>
            {
                if (i.Pen == null)
                    i.Pen = new Pen(colors.ArgMin(j => series.Count(k => k.Pen != null && k.Pen.Color == j)));
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

        private static Color[] colors = new Color[]
        { 
            Color.Red, 
            Color.Blue, 
            Color.Green, 
            Color.DarkRed, 
            Color.DarkGreen, 
            Color.DarkBlue 
        };
    }
}
