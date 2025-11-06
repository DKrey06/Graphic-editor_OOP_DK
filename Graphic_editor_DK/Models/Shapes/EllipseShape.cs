using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Graphic_editor_DK.Models.Shapes
{
    public class EllipseShape : BaseShape 
    {
        public override void Draw(DrawingContext drawingContext)
        {
            var center = new Point((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
            var radiusX = Math.Abs(EndPoint.X - StartPoint.X) / 2;
            var radiusY = Math.Abs(EndPoint.Y - StartPoint.Y) / 2;

            var geometry = new EllipseGeometry(center, radiusX, radiusY);
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), geometry);
        }

        public override bool ContainsPoint(Point point)
        {
            var center = new Point((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
            var radiusX = Math.Abs(EndPoint.X - StartPoint.X) / 2;
            var radiusY = Math.Abs(EndPoint.Y - StartPoint.Y) / 2;

            var geometry = new EllipseGeometry(center, radiusX, radiusY);
            return geometry.FillContains(point) || geometry.StrokeContains(new Pen(Stroke, StrokeThickness + 5), point);
        }
    }
}
