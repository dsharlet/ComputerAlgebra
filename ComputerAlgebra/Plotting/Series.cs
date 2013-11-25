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
    public enum PointStyle
    {
        None,
        Square,
        Circle,
        Cross,
    }

    /// <summary>
    /// A data series.
    /// </summary>
    public abstract class Series
    {
        private string name = "y[x]";
        public string Name { get { return name; } set { name = value; } }

        private Pen pen = null;
        public Pen Pen { get { return pen; } set { pen = value; } }

        protected PointStyle pointStyle = PointStyle.Square;
        public PointStyle PointStyle { get { return pointStyle; } set { pointStyle = value; } }

        protected abstract PointF[] Evaluate(double x0, double x1);

        public void Paint(Matrix2D T, double x0, double x1, Graphics G)
        {
            PointF[] points = Evaluate(x0, x1);
            T.TransformPoints(points);
            G.DrawLines(Pen, points);
        }

        public double MinY(double x0, double x1) { return Evaluate(x0, x1).Min(i => i.Y, double.PositiveInfinity); }
        public double MaxY(double x0, double x1) { return Evaluate(x0, x1).Max(i => i.Y, double.NegativeInfinity); }
    }

    /// <summary>
    /// Data series derived from a lambda function.
    /// </summary>
    public class Function : Series
    {
        protected Func<double, double> f;
        public Function(Func<double, double> f) { this.f = f; }

        protected override PointF[] Evaluate(double x0, double x1)
        {
            int N = 2048;

            PointF[] points = new PointF[N + 1];
            for (int i = 0; i <= N; ++i)
            {
                double x = ((x1 - x0) * i) / N + x0;
                points[i].X = (float)x;
                points[i].Y = (float)f(x);
            }

            return points;
        }
    }

    /// <summary>
    /// Explicit point list.
    /// </summary>
    public class Scatter : Series
    {
        protected KeyValuePair<double, double>[] points;
        public Scatter(KeyValuePair<double, double>[] Points) { points = Points; }

        protected override PointF[] Evaluate(double x0, double x1)
        {
            return points.Select(i => new PointF((float)i.Key, (float)i.Value)).ToArray();
        }
    }
}
