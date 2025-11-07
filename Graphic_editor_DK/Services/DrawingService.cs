using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphic_editor_DK.Models.Shapes;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Graphic_editor_DK.Services
{
    public class DrawingService
    {
        public ObservableCollection<BaseShape> Shapes { get; } = new ObservableCollection<BaseShape>();

        public void DrawShapes(DrawingContext drawingContext)
        {
            foreach (var shape in Shapes)
            {
                shape.Draw(drawingContext);
            }
        }

        public BaseShape GetShapeAtPoint(Point point)
        {
            for (int i = Shapes.Count - 1; i >= 0; i--)
            {
                if (Shapes[i].ContainsPoint(point))
                    return Shapes[i];
            }
            return null;
        }

        public void Clear()
        {
            Shapes.Clear();
        }
    }
}