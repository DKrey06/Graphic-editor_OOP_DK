using Graphic_editor_DK.Models.Shapes;
using Graphic_editor_DK.Models.Tools;
using Graphic_editor_DK.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graphic_editor_DK
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private List<BaseShape> _shapes = new List<BaseShape>();
        private BaseShape _currentShape;
        private Shape _currentShapeElement;

        private Rectangle _selectionRectangle;
        private bool _isSelecting;
        private Point _startPoint;

        private Polyline _currentBrushStroke;
        private Brush _currentBrushColor = Brushes.Black;
        private double _currentBrushSize = 3;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.MainViewModel();
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(DrawingCanvas);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = point;

                if (ViewModel.ToolManager.CurrentTool is SelectionTool)
                {
                    StartSelection(point);
                }
                else if (ViewModel.ToolManager.CurrentTool is BrushTool)
                {
                    StartBrushStroke(point);
                }
                else
                {
                    CreateNewShape(point);
                }
            }

            ViewModel.ToolManager.CurrentTool?.OnMouseDown(point, e);
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(DrawingCanvas);

            if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateSelection(point);
            }
            else if (_currentBrushStroke != null && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateBrushStroke(point);
            }
            else if (_currentShapeElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCurrentShape(point);
            }

            ViewModel.ToolManager.CurrentTool?.OnMouseMove(point, e);
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(DrawingCanvas);

            if (_isSelecting)
            {
                EndSelection(point);
            }

            else if (_currentBrushStroke != null)
            {
                _currentBrushStroke = null;
            }

            else if (_currentShape != null && _currentShapeElement != null)
            {
                UpdateCurrentShape(point);
                _shapes.Add(_currentShape);
                _currentShape = null;
                _currentShapeElement = null;
            }

            ViewModel.ToolManager.CurrentTool?.OnMouseUp(point, e);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(DrawingCanvas);
            CoordinatesText.Text = $"X: {(int)point.X}, Y: {(int)point.Y}";
        }

        // МЕТОДЫ ДЛЯ КИСТИ
        private void StartBrushStroke(Point startPoint)
        {
            _currentBrushStroke = new Polyline
            {
                Stroke = _currentBrushColor,
                StrokeThickness = _currentBrushSize,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            _currentBrushStroke.Points.Add(startPoint);

            DrawingCanvas.Children.Add(_currentBrushStroke);
        }

        private void UpdateBrushStroke(Point currentPoint)
        {
            if (_currentBrushStroke != null)
            {
                _currentBrushStroke.Points.Add(currentPoint);
            }
        }

        // МЕТОДЫ ДЛЯ ФИГУР
        private void CreateNewShape(Point startPoint)
        {
            var currentTool = ViewModel.ToolManager.CurrentTool;

            if (currentTool is LineTool)
            {
                var line = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = startPoint.X,
                    Y2 = startPoint.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                DrawingCanvas.Children.Add(line);
                _currentShapeElement = line;

                _currentShape = new LineShape
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
            }
            else if (currentTool is RectangleTool)
            {
                var rectangle = new Rectangle
                {
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightBlue,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(rectangle, startPoint.X);
                Canvas.SetTop(rectangle, startPoint.Y);
                rectangle.Width = 0;
                rectangle.Height = 0;

                DrawingCanvas.Children.Add(rectangle);
                _currentShapeElement = rectangle;

                _currentShape = new RectangleShape
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint,
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightBlue,
                    StrokeThickness = 2
                };
            }
            else if (currentTool is EllipseTool)
            {
                var ellipse = new Ellipse
                {
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightGreen,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(ellipse, startPoint.X);
                Canvas.SetTop(ellipse, startPoint.Y);
                ellipse.Width = 0;
                ellipse.Height = 0;

                DrawingCanvas.Children.Add(ellipse);
                _currentShapeElement = ellipse;

                _currentShape = new EllipseShape
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint,
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightGreen,
                    StrokeThickness = 2
                };
            }
        }

        private void UpdateCurrentShape(Point currentPoint)
        {
            if (_currentShapeElement is Line line)
            {
                line.X2 = currentPoint.X;
                line.Y2 = currentPoint.Y;
                _currentShape.EndPoint = currentPoint;
            }
            else if (_currentShapeElement is Rectangle rectangle)
            {
                var start = _currentShape.StartPoint;
                Canvas.SetLeft(rectangle, Math.Min(start.X, currentPoint.X));
                Canvas.SetTop(rectangle, Math.Min(start.Y, currentPoint.Y));
                rectangle.Width = Math.Abs(currentPoint.X - start.X);
                rectangle.Height = Math.Abs(currentPoint.Y - start.Y);
                _currentShape.EndPoint = currentPoint;
            }
            else if (_currentShapeElement is Ellipse ellipse)
            {
                var start = _currentShape.StartPoint;
                Canvas.SetLeft(ellipse, Math.Min(start.X, currentPoint.X));
                Canvas.SetTop(ellipse, Math.Min(start.Y, currentPoint.Y));
                ellipse.Width = Math.Abs(currentPoint.X - start.X);
                ellipse.Height = Math.Abs(currentPoint.Y - start.Y);
                _currentShape.EndPoint = currentPoint;
            }
        }

        // МЕТОДЫ ДЛЯ ВЫДЕЛЕНИЯ
        private void StartSelection(Point startPoint)
        {
            _selectionRectangle = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = new SolidColorBrush(Color.FromArgb(30, 0, 120, 215))
            };

            Canvas.SetLeft(_selectionRectangle, startPoint.X);
            Canvas.SetTop(_selectionRectangle, startPoint.Y);
            _selectionRectangle.Width = 0;
            _selectionRectangle.Height = 0;

            DrawingCanvas.Children.Add(_selectionRectangle);
            _isSelecting = true;
        }

        private void UpdateSelection(Point currentPoint)
        {
            if (_selectionRectangle != null)
            {
                Canvas.SetLeft(_selectionRectangle, Math.Min(_startPoint.X, currentPoint.X));
                Canvas.SetTop(_selectionRectangle, Math.Min(_startPoint.Y, currentPoint.Y));
                _selectionRectangle.Width = Math.Abs(currentPoint.X - _startPoint.X);
                _selectionRectangle.Height = Math.Abs(currentPoint.Y - _startPoint.Y);
            }
        }

        private void EndSelection(Point endPoint)
        {
            if (_selectionRectangle != null)
            {
                DrawingCanvas.Children.Remove(_selectionRectangle);
                _selectionRectangle = null;
            }
            _isSelecting = false;
        }

        // ОБРАБОТЧИКИ ПАЛИТРЫ
        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorComboBox.SelectedItem is ComboBoxItem item && item.Tag is string colorName)
            {
                _currentBrushColor = (Brush)new BrushConverter().ConvertFromString(colorName);
                Console.WriteLine($"Selected color: {colorName}");
            }
        }

        private void BrushSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BrushSizeComboBox.SelectedItem is ComboBoxItem item && item.Content is string sizeText)
            {
                if (double.TryParse(sizeText, out double size))
                {
                    _currentBrushSize = size;
                    Console.WriteLine($"Selected brush size: {size}");
                }
            }
        }
    }
}