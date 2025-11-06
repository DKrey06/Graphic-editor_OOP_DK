using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Graphic_editor_DK.Models.Shapes
{
    public abstract class BaseShape
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Brush Stroke {  get; set; } = Brushes.Black;
        public Brush Fill { get; set; } = Brushes.Transparent;
        public double StrokeThickness { get; set; } = 2;

        public abstract void Draw(DrawingContext drawingContext);
        public abstract bool ContainsPoint(Point point);
    }
}
