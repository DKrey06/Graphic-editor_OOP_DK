using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Graphic_editor_DK.Models.Shapes
{
    public class RectangleShape : BaseShape
    {
        public override void Draw(DrawingContext drawingContext)
        {
            var rect = new Rect(StartPoint, EndPoint);
            drawingContext.DrawRectangle(Fill, new Pen(Stroke, StrokeThickness), rect);
        }

        public override bool ContainsPoint(Point point)
        {
            var rect = new Rect(StartPoint, EndPoint);
            return rect.Contains(point);
        }
    }
}
