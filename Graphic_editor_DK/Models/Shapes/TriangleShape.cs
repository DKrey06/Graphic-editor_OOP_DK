using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Graphic_editor_DK.Models.Shapes
{
    public class TriangleShape : BaseShape
    {
        public override void Draw(DrawingContext drawingContext)
        {
            var points = CalculateTrianglePoints();
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                context.BeginFigure(points[0], true, true);
                context.LineTo(points[1], true, false);
                context.LineTo(points[2], true, false);
            }

            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), geometry);
        }

        public override bool ContainsPoint(Point point)
        {
            var points = CalculateTrianglePoints();
            return IsPointInTriangle(point, points[0], points[1], points[2]);
        }

        private Point[] CalculateTrianglePoints()
        {
            var points = new Point[3];
            points[0] = new Point((StartPoint.X + EndPoint.X) / 2, StartPoint.Y);
            points[1] = new Point(StartPoint.X, EndPoint.Y);
            points[2] = new Point(EndPoint.X, EndPoint.Y);

            return points;
        }

        private bool IsPointInTriangle(Point p, Point p0, Point p1, Point p2)
        {
            var s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
            var t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0 && t > 0 && (s + t) < A;
        }
    }
}