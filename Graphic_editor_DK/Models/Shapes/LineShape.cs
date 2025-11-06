using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Graphic_editor_DK.Models.Shapes
{
    public class LineShape : BaseShape
    {
        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawLine(new Pen(Stroke, StrokeThickness), StartPoint, EndPoint);
        }

        public override bool ContainsPoint(Point point)
        {
            var geometry = new LineGeometry(StartPoint, EndPoint);
            return geometry.StrokeContains(new Pen(Stroke, StrokeThickness + 5), point);

        }
    }
}
