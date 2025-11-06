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
    public class LineTool : BaseTool
    {
        public override ToolType ToolType => ToolType.Line;

        private LineShape _currentLine;
        private bool _isDrawing;

        public override void OnMouseDown(Point point, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _currentLine = new LineShape
                {
                    StartPoint = point,
                    EndPoint = point,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                _isDrawing = true;

                //Временно просто создаем фигуру, без событий. Позже подключить к холсту
            }
        }

        public override void OnMouseMove(Point point, MouseEventArgs e)
        {
            if (_isDrawing && _currentLine != null)
            {
                _currentLine.EndPoint = point;
                //Временно без событий
            }
        }

        public override void OnMouseUp(Point point, MouseButtonEventArgs e)
        {
            if (_isDrawing && _currentLine != null)
            {
                _currentLine.EndPoint = point;
                _isDrawing = false;
                //Временно без событий
            }
        }
    }
}