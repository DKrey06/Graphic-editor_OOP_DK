using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Graphic_editor_DK.Utilities.Enums;

namespace Graphic_editor_DK.Models.Tools
{
    public class TextTool : BaseTool
    {
        public override ToolType ToolType => ToolType.Text;

        private bool _isPlacingText;

        public override void OnMouseDown(Point point, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isPlacingText = true;
                TextPlaced?.Invoke(point);
            }
        }

        public override void OnMouseMove(Point point, MouseEventArgs e)
        {
        }

        public override void OnMouseUp(Point point, MouseButtonEventArgs e)
        {
            _isPlacingText = false;
        }

        public event Action<Point> TextPlaced;
    }
}