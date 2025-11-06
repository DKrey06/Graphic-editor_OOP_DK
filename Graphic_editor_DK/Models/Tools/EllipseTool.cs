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
    public class EllipseTool : BaseTool
    {
        public override ToolType ToolType => ToolType.Ellipse;

        private EllipseShape _currentEllipse;
        private bool _isDrawing;

        public override void OnMouseDown(Point point, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _currentEllipse = new EllipseShape
                {
                    StartPoint = point,
                    EndPoint = point,
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightBlue,
                    StrokeThickness = 2
                };
                _isDrawing = true;
                ToolManager.OnShapeCreated?.Invoke(_currentEllipse);
            }
        }

        public override void OnMouseMove(Point point, MouseEventArgs e)
        {
            if (_isDrawing && _currentEllipse != null)
            {
                _currentEllipse.EndPoint = point;
                ToolManager.OnShapeUpdated?.Invoke(_currentEllipse);
            }
        }

        public override void OnMouseUp(Point point, MouseButtonEventArgs e)
        {
            if (_isDrawing && _currentEllipse != null)
            {
                _currentEllipse.EndPoint = point;
                _isDrawing = false;
                ToolManager.OnShapeFinalized?.Invoke(_currentEllipse);
            }
        }
    }
}
