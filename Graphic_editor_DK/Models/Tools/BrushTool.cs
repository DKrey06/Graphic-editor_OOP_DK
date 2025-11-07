using Graphic_editor_DK.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graphic_editor_DK.Models.Tools
{
    public class BrushTool : BaseTool
    {
        public override ToolType ToolType => ToolType.Brush;

        private Polyline _currentStroke;
        private bool _isDrawing;

        public BrushTool()
        {
            BrushColor = Brushes.Black;
            BrushSize = 3;
        }

        public Brush BrushColor { get; set; }
        public double BrushSize { get; set; }

        public override void OnMouseDown(Point point, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _currentStroke = new Polyline
                {
                    Stroke = BrushColor,
                    StrokeThickness = BrushSize,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                _currentStroke.Points.Add(point);
                _isDrawing = true;

                StrokeStarted?.Invoke(_currentStroke);
            }
        }

        public override void OnMouseMove(Point point, MouseEventArgs e)
        {
            if (_isDrawing && _currentStroke != null && e.LeftButton == MouseButtonState.Pressed)
            {
                _currentStroke.Points.Add(point);
                StrokeUpdated?.Invoke(_currentStroke);
            }
        }

        public override void OnMouseUp(Point point, MouseButtonEventArgs e)
        {
            if (_isDrawing && _currentStroke != null)
            {
                _currentStroke.Points.Add(point);
                _isDrawing = false;
                StrokeCompleted?.Invoke(_currentStroke);
            }
        }


        public event Action<Polyline> StrokeStarted;
        public event Action<Polyline> StrokeUpdated;
        public event Action<Polyline> StrokeCompleted;
    }
}