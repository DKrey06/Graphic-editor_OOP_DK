using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Graphic_editor_DK.Models.Shapes;
using Graphic_editor_DK.Utilities.Enums;

namespace Graphic_editor_DK.Models.Tools
{
    public class RectangleTool : BaseTool
    {
        public override ToolType ToolType => ToolType.Rectangle;

        private RectangleShape _currentRectangle;
        private bool _isDrawing;

        public override void OnMouseDown(Point point, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _currentRectangle = new RectangleShape
                {
                    StartPoint = point,
                    EndPoint = point,
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightBlue,
                    StrokeThickness = 2
                };
                _isDrawing = true;
            }
        }

        public override void OnMouseMove(Point point, MouseEventArgs e)
        {
            if (_isDrawing && _currentRectangle != null)
            {
                _currentRectangle.EndPoint = point;
            }
        }

        public override void OnMouseUp(Point point, MouseButtonEventArgs e)
        {
            if (_isDrawing && _currentRectangle != null)
            {
                _currentRectangle.EndPoint = point;
                _isDrawing = false;
            }
        }
    }
}