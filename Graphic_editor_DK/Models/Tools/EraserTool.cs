using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Graphic_editor_DK.Utilities.Enums;

namespace Graphic_editor_DK.Models.Tools
{
    public class EraserTool : BaseTool
    {
        public override ToolType ToolType => ToolType.Eraser;

        public override void OnMouseDown(Point point, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ShapeErased?.Invoke(point);
            }
        }

        public override void OnMouseMove(Point point, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ShapeErased?.Invoke(point);
            }
        }

        public override void OnMouseUp(Point point, MouseButtonEventArgs e)
        {
        }

        public event Action<Point> ShapeErased;
    }
}