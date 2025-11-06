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
    public class SelectionTool : BaseTool
    {
        public override ToolType ToolType => ToolType.Selection;

        private Point _startPoint;
        private bool _isSelecting;

        public override void OnMouseDown(Point point, MouseButtonEventArgs e)
        {
            _startPoint = point;
            _isSelecting = true;

            Console.WriteLine($"Selection started at: {point}");
        }

        public override void OnMouseMove(Point point, MouseEventArgs e)
        {
            if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                Console.WriteLine($"Selecting area from {_startPoint} to {point}");
            }
        }

        public override void OnMouseUp(Point point, MouseButtonEventArgs e)
        {
            _isSelecting = false;
            Console.WriteLine($"Selection ended at: {point}");
        }
    }
}