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

        private UIElement _selectedElement;
        private BaseShape _selectedShape;
        private bool _isDragging;
        private Point _dragStartPoint;

        private TextBox _currentTextbox;
        private bool _isPlacingText;

        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new ViewModels.MainViewModel();
            this.DataContext = viewModel;
            viewModel.SetMainWindow(this);


            ColorComboBox.SelectedIndex = 0;
            BrushSizeComboBox.SelectedIndex = 1;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _selectedElement != null)
            {
                DeleteSelectedShape();
            }
            base.OnKeyDown(e);
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(DrawingCanvas);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = point;
                _dragStartPoint = point;

                if (ViewModel.ToolManager.CurrentTool is SelectionTool)
                {
                    if (!TrySelectShape(point))
                    {
                        StartSelection(point);
                    }
                    else
                    {
                        _isDragging = true;
                    }
                }
                else if (ViewModel.ToolManager.CurrentTool is BrushTool)
                {
                    StartBrushStroke(point);
                }
                else if (ViewModel.ToolManager.CurrentTool is EraserTool)
                {
                    EraseAtPoint(point);
                }
                else if (ViewModel.ToolManager.CurrentTool is TextTool)
                {
                    StartTextPlacement(point);
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

            if (_isDragging && _selectedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                MoveSelectedShape(point);
            }
            else if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
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
            else if (ViewModel.ToolManager.CurrentTool is EraserTool && e.LeftButton == MouseButtonState.Pressed)
            {
                EraseAtPoint(point);
            }

            ViewModel.ToolManager.CurrentTool?.OnMouseMove(point, e);
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(DrawingCanvas);

            _isDragging = false;

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
                ViewModel.DrawingService.Shapes.Add(_currentShape);
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
                    Stroke = _currentBrushColor,
                    StrokeThickness = _currentBrushSize
                };

                DrawingCanvas.Children.Add(line);
                _currentShapeElement = line;

                _currentShape = new LineShape
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint,
                    Stroke = _currentBrushColor,
                    StrokeThickness = _currentBrushSize
                };
            }
            else if (currentTool is RectangleTool)
            {
                var rectangle = new Rectangle
                {
                    Stroke = _currentBrushColor,
                    Fill = Brushes.LightBlue,
                    StrokeThickness = _currentBrushSize
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
                    Stroke = _currentBrushColor,
                    Fill = Brushes.LightBlue,
                    StrokeThickness = _currentBrushSize
                };
            }
            else if (currentTool is EllipseTool)
            {
                var ellipse = new Ellipse
                {
                    Stroke = _currentBrushColor,
                    Fill = Brushes.LightGreen,
                    StrokeThickness = _currentBrushSize
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
                    Stroke = _currentBrushColor,
                    Fill = Brushes.LightGreen,
                    StrokeThickness = _currentBrushSize
                };
            }
            else if (currentTool is TriangleTool)
            {
                var polygon = new Polygon
                {
                    Stroke = _currentBrushColor,
                    Fill = Brushes.LightCoral,
                    StrokeThickness = _currentBrushSize
                };

                UpdateTrianglePoints(polygon, startPoint, startPoint);

                DrawingCanvas.Children.Add(polygon);
                _currentShapeElement = polygon;

                _currentShape = new TriangleShape
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint,
                    Stroke = _currentBrushColor,
                    Fill = Brushes.LightCoral,
                    StrokeThickness = _currentBrushSize
                };
            }
        }

        private void UpdateTrianglePoints(Polygon polygon, Point start, Point end)
        {
            polygon.Points.Clear();
            polygon.Points.Add(new Point((start.X + end.X) / 2, start.Y));
            polygon.Points.Add(new Point(start.X, end.Y));
            polygon.Points.Add(new Point(end.X, end.Y));
        }

        private void StartTextPlacement(Point position)
        {
            _currentTextbox = new TextBox
            {
                Width = 100,
                Height = 25,
                FontSize = 12,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray
            };

            Canvas.SetLeft(_currentTextbox, position.X);
            Canvas.SetTop(_currentTextbox, position.Y);

            DrawingCanvas.Children.Add(_currentTextbox);
            _currentTextbox.Focus();
            _isPlacingText = true;
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
            else if (_currentShapeElement is Polygon polygon)
            {
                var start = _currentShape.StartPoint;
                UpdateTrianglePoints(polygon, start, currentPoint);
                _currentShape.EndPoint = currentPoint;
            }
        }

        // МЕТОДЫ ДЛЯ ВЫДЕЛЕНИЯ ОБЛАСТИ
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

        // МЕТОДЫ ДЛЯ ВЫДЕЛЕНИЯ И ПЕРЕМЕЩЕНИЯ ФИГУР
        private bool TrySelectShape(Point point)
        {
            ClearSelection();

            for (int i = DrawingCanvas.Children.Count - 1; i >= 0; i--)
            {
                var element = DrawingCanvas.Children[i];

                if (element is Line line && IsPointOnLine(line, point))
                {
                    SelectElement(element, FindShapeForLine(line));
                    return true;
                }
                else if (element is Rectangle rect && IsPointInRectangle(rect, point))
                {
                    SelectElement(element, FindShapeForRectangle(rect));
                    return true;
                }
                else if (element is Ellipse ellipse && IsPointInEllipse(ellipse, point))
                {
                    SelectElement(element, FindShapeForEllipse(ellipse));
                    return true;
                }
                else if (element is Polygon polygon && IsPointInPolygon(polygon, point))
                {
                    SelectElement(element, FindShapeForPolygon(polygon));
                    return true;
                }
                else if (element is Polyline brushStroke && IsPointOnPolyline(brushStroke, point))
                {
                    SelectElement(element, null);
                    return true;
                }
                else if (element is TextBox textBox && IsPointInTextBox(textBox, point))
                {
                    SelectElement(element, null);
                    return true;
                }
            }

            return false;
        }

        private bool IsPointOnLine(Line line, Point point)
        {
            double distance = PointToLineDistance(point, new Point(line.X1, line.Y1), new Point(line.X2, line.Y2));
            return distance < 10;
        }

        private bool IsPointInRectangle(Rectangle rect, Point point)
        {
            double left = Canvas.GetLeft(rect);
            double top = Canvas.GetTop(rect);
            return point.X >= left && point.X <= left + rect.Width &&
                   point.Y >= top && point.Y <= top + rect.Height;
        }

        private bool IsPointInEllipse(Ellipse ellipse, Point point)
        {
            double left = Canvas.GetLeft(ellipse);
            double top = Canvas.GetTop(ellipse);
            double centerX = left + ellipse.Width / 2;
            double centerY = top + ellipse.Height / 2;

            double normalizedX = Math.Pow((point.X - centerX) / (ellipse.Width / 2), 2);
            double normalizedY = Math.Pow((point.Y - centerY) / (ellipse.Height / 2), 2);
            return normalizedX + normalizedY <= 1;
        }

        private bool IsPointInPolygon(Polygon polygon, Point point)
        {
            var bounds = new Rect(polygon.Points[0], polygon.Points[0]);
            foreach (var pt in polygon.Points)
            {
                bounds.Union(pt);
            }
            bounds.Inflate(polygon.StrokeThickness, polygon.StrokeThickness);
            return bounds.Contains(point);
        }

        private bool IsPointOnPolyline(Polyline polyline, Point point)
        {
            var bounds = new Rect(polyline.Points[0], polyline.Points[0]);
            foreach (var pt in polyline.Points)
            {
                bounds.Union(pt);
            }
            bounds.Inflate(polyline.StrokeThickness, polyline.StrokeThickness);
            return bounds.Contains(point);
        }

        private bool IsPointInTextBox(TextBox textBox, Point point)
        {
            double left = Canvas.GetLeft(textBox);
            double top = Canvas.GetTop(textBox);
            return point.X >= left && point.X <= left + textBox.Width &&
                   point.Y >= top && point.Y <= top + textBox.Height;
        }

        private double PointToLineDistance(Point point, Point lineStart, Point lineEnd)
        {
            double A = point.X - lineStart.X;
            double B = point.Y - lineStart.Y;
            double C = lineEnd.X - lineStart.X;
            double D = lineEnd.Y - lineStart.Y;

            double dot = A * C + B * D;
            double lenSq = C * C + D * D;
            double param = (lenSq != 0) ? dot / lenSq : -1;

            double xx, yy;

            if (param < 0)
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * C;
                yy = lineStart.Y + param * D;
            }

            double dx = point.X - xx;
            double dy = point.Y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void SelectElement(UIElement element, BaseShape shape)
        {
            _selectedElement = element;
            _selectedShape = shape;

            if (element is Shape shapeElement)
            {
                shapeElement.Stroke = Brushes.Red;
                shapeElement.StrokeThickness *= 2;
            }
            else if (element is TextBox textBox)
            {
                textBox.BorderBrush = Brushes.Red;
            }
        }

        private void ClearSelection()
        {
            if (_selectedElement is Shape shapeElement)
            {
                shapeElement.Stroke = Brushes.Black;
                if (shapeElement.StrokeThickness > 2)
                {
                    shapeElement.StrokeThickness /= 2;
                }
            }
            else if (_selectedElement is TextBox textBox)
            {
                textBox.BorderBrush = Brushes.Gray;
            }

            _selectedElement = null;
            _selectedShape = null;
        }

        private BaseShape FindShapeForLine(Line line)
        {
            return _shapes.Find(s => s is LineShape ls &&
                Math.Abs(ls.StartPoint.X - line.X1) < 0.1 &&
                Math.Abs(ls.StartPoint.Y - line.Y1) < 0.1 &&
                Math.Abs(ls.EndPoint.X - line.X2) < 0.1 &&
                Math.Abs(ls.EndPoint.Y - line.Y2) < 0.1);
        }

        private BaseShape FindShapeForRectangle(Rectangle rect)
        {
            return _shapes.Find(s => s is RectangleShape);
        }

        private BaseShape FindShapeForEllipse(Ellipse ellipse)
        {
            return _shapes.Find(s => s is EllipseShape);
        }

        private BaseShape FindShapeForPolygon(Polygon polygon)
        {
            return _shapes.Find(s => s is TriangleShape);
        }

        private void MoveSelectedShape(Point currentPoint)
        {
            double deltaX = currentPoint.X - _dragStartPoint.X;
            double deltaY = currentPoint.Y - _dragStartPoint.Y;

            if (_selectedElement is Line line)
            {
                line.X1 += deltaX;
                line.Y1 += deltaY;
                line.X2 += deltaX;
                line.Y2 += deltaY;

                if (_selectedShape != null)
                {
                    _selectedShape.StartPoint = new Point(_selectedShape.StartPoint.X + deltaX, _selectedShape.StartPoint.Y + deltaY);
                    _selectedShape.EndPoint = new Point(_selectedShape.EndPoint.X + deltaX, _selectedShape.EndPoint.Y + deltaY);
                }
            }
            else if (_selectedElement is Rectangle rect)
            {
                double newLeft = Canvas.GetLeft(rect) + deltaX;
                double newTop = Canvas.GetTop(rect) + deltaY;
                Canvas.SetLeft(rect, newLeft);
                Canvas.SetTop(rect, newTop);

                if (_selectedShape != null)
                {
                    _selectedShape.StartPoint = new Point(_selectedShape.StartPoint.X + deltaX, _selectedShape.StartPoint.Y + deltaY);
                    _selectedShape.EndPoint = new Point(_selectedShape.EndPoint.X + deltaX, _selectedShape.EndPoint.Y + deltaY);
                }
            }
            else if (_selectedElement is Ellipse ellipse)
            {
                double newLeft = Canvas.GetLeft(ellipse) + deltaX;
                double newTop = Canvas.GetTop(ellipse) + deltaY;
                Canvas.SetLeft(ellipse, newLeft);
                Canvas.SetTop(ellipse, newTop);

                if (_selectedShape != null)
                {
                    _selectedShape.StartPoint = new Point(_selectedShape.StartPoint.X + deltaX, _selectedShape.StartPoint.Y + deltaY);
                    _selectedShape.EndPoint = new Point(_selectedShape.EndPoint.X + deltaX, _selectedShape.EndPoint.Y + deltaY);
                }
            }
            else if (_selectedElement is Polygon polygon)
            {
                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    polygon.Points[i] = new Point(polygon.Points[i].X + deltaX, polygon.Points[i].Y + deltaY);
                }

                if (_selectedShape != null)
                {
                    _selectedShape.StartPoint = new Point(_selectedShape.StartPoint.X + deltaX, _selectedShape.StartPoint.Y + deltaY);
                    _selectedShape.EndPoint = new Point(_selectedShape.EndPoint.X + deltaX, _selectedShape.EndPoint.Y + deltaY);
                }
            }
            else if (_selectedElement is Polyline polyline)
            {
                for (int i = 0; i < polyline.Points.Count; i++)
                {
                    polyline.Points[i] = new Point(polyline.Points[i].X + deltaX, polyline.Points[i].Y + deltaY);
                }
            }
            else if (_selectedElement is TextBox textBox)
            {
                double newLeft = Canvas.GetLeft(textBox) + deltaX;
                double newTop = Canvas.GetTop(textBox) + deltaY;
                Canvas.SetLeft(textBox, newLeft);
                Canvas.SetTop(textBox, newTop);
            }

            _dragStartPoint = currentPoint;
        }

        private void DeleteSelectedShape()
        {
            if (_selectedElement != null)
            {
                DrawingCanvas.Children.Remove(_selectedElement);
                if (_selectedShape != null)
                {
                    _shapes.Remove(_selectedShape);
                    ViewModel.DrawingService.Shapes.Remove(_selectedShape);
                }
                ClearSelection();
            }
        }

        // МЕТОД ДЛЯ ЛАСТИКА
        private void EraseAtPoint(Point point)
        {
            for (int i = DrawingCanvas.Children.Count - 1; i >= 0; i--)
            {
                var element = DrawingCanvas.Children[i];

                if (element is Shape shape && IsPointNearShape(shape, point))
                {
                    DrawingCanvas.Children.RemoveAt(i);

                    var shapeToRemove = FindShapeForElement(element);
                    if (shapeToRemove != null)
                    {
                        _shapes.Remove(shapeToRemove);
                        ViewModel.DrawingService.Shapes.Remove(shapeToRemove);
                    }
                    break;
                }
                else if (element is TextBox textBox && IsPointInTextBox(textBox, point))
                {
                    DrawingCanvas.Children.RemoveAt(i);
                    break;
                }
            }
        }

        private bool IsPointNearShape(Shape shape, Point point)
        {
            if (shape is Line line)
            {
                return IsPointOnLine(line, point);
            }
            else if (shape is Rectangle rect)
            {
                return IsPointInRectangle(rect, point);
            }
            else if (shape is Ellipse ellipse)
            {
                return IsPointInEllipse(ellipse, point);
            }
            else if (shape is Polygon polygon)
            {
                return IsPointInPolygon(polygon, point);
            }
            else if (shape is Polyline polyline)
            {
                return IsPointOnPolyline(polyline, point);
            }
            return false;
        }

        private BaseShape FindShapeForElement(UIElement element)
        {
            foreach (var shape in _shapes)
            {
                if (shape is LineShape lineShape && element is Line line)
                {
                    if (Math.Abs(lineShape.StartPoint.X - line.X1) < 0.1 &&
                        Math.Abs(lineShape.StartPoint.Y - line.Y1) < 0.1)
                        return shape;
                }
                else if (shape is RectangleShape rectShape && element is Rectangle rect)
                {
                    return rectShape;
                }
                else if (shape is EllipseShape ellipseShape && element is Ellipse ellipse)
                {
                    return ellipseShape;
                }
                else if (shape is TriangleShape triangleShape && element is Polygon polygon)
                {
                    return triangleShape;
                }
            }
            return null;
        }

        // ОБРАБОТЧИКИ ПАЛИТРЫ
        private void ChangeSelectedShapeColor(Brush stroke, Brush fill)
        {
            if (_selectedElement is Shape shapeElement && _selectedShape != null)
            {
                shapeElement.Stroke = stroke;
                _selectedShape.Stroke = stroke;

                if (shapeElement is Rectangle || shapeElement is Ellipse || shapeElement is Polygon)
                {
                    if (shapeElement is Rectangle rect) rect.Fill = fill;
                    else if (shapeElement is Ellipse ellipse) ellipse.Fill = fill;
                    else if (shapeElement is Polygon polygon) polygon.Fill = fill;

                    _selectedShape.Fill = fill;
                }
            }
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorComboBox.SelectedItem is ComboBoxItem item && item.Tag is string colorName)
            {
                try
                {
                    var brush = (Brush)new BrushConverter().ConvertFromString(colorName);
                    _currentBrushColor = brush;
                    if (_selectedElement != null)
                    {
                        ChangeSelectedShapeColor(brush, _selectedShape?.Fill ?? Brushes.Transparent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при изменении цвета: {ex.Message}");
                }
            }
        }

        private void BrushSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BrushSizeComboBox.SelectedItem is ComboBoxItem item && item.Content is string sizeText)
            {
                if (double.TryParse(sizeText, out double size))
                {
                    _currentBrushSize = size;
                }
            }
        }

        // МЕТОД ДЛЯ ОБНОВЛЕНИЯ ХОЛСТА ПРИ ЗАГРУЗКЕ ПРОЕКТА
        public void RefreshCanvas()
        {
            DrawingCanvas.Children.Clear();
            _shapes.Clear();

            foreach (var shape in ViewModel.DrawingService.Shapes)
            {
                if (shape is LineShape lineShape)
                {
                    var line = new Line
                    {
                        X1 = lineShape.StartPoint.X,
                        Y1 = lineShape.StartPoint.Y,
                        X2 = lineShape.EndPoint.X,
                        Y2 = lineShape.EndPoint.Y,
                        Stroke = lineShape.Stroke,
                        StrokeThickness = lineShape.StrokeThickness
                    };
                    DrawingCanvas.Children.Add(line);
                    _shapes.Add(lineShape);
                }
                else if (shape is RectangleShape rectShape)
                {
                    var rect = new Rectangle
                    {
                        Stroke = rectShape.Stroke,
                        Fill = rectShape.Fill,
                        StrokeThickness = rectShape.StrokeThickness
                    };

                    var start = rectShape.StartPoint;
                    var end = rectShape.EndPoint;
                    Canvas.SetLeft(rect, Math.Min(start.X, end.X));
                    Canvas.SetTop(rect, Math.Min(start.Y, end.Y));
                    rect.Width = Math.Abs(end.X - start.X);
                    rect.Height = Math.Abs(end.Y - start.Y);

                    DrawingCanvas.Children.Add(rect);
                    _shapes.Add(rectShape);
                }
                else if (shape is EllipseShape ellipseShape)
                {
                    var ellipse = new Ellipse
                    {
                        Stroke = ellipseShape.Stroke,
                        Fill = ellipseShape.Fill,
                        StrokeThickness = ellipseShape.StrokeThickness
                    };

                    var start = ellipseShape.StartPoint;
                    var end = ellipseShape.EndPoint;
                    Canvas.SetLeft(ellipse, Math.Min(start.X, end.X));
                    Canvas.SetTop(ellipse, Math.Min(start.Y, end.Y));
                    ellipse.Width = Math.Abs(end.X - start.X);
                    ellipse.Height = Math.Abs(end.Y - start.Y);

                    DrawingCanvas.Children.Add(ellipse);
                    _shapes.Add(ellipseShape);
                }
                else if (shape is TriangleShape triangleShape)
                {
                    var polygon = new Polygon
                    {
                        Stroke = triangleShape.Stroke,
                        Fill = triangleShape.Fill,
                        StrokeThickness = triangleShape.StrokeThickness
                    };

                    UpdateTrianglePoints(polygon, triangleShape.StartPoint, triangleShape.EndPoint);

                    DrawingCanvas.Children.Add(polygon);
                    _shapes.Add(triangleShape);
                }
            }
        }

        // МЕТОД ДЛЯ ОБНОВЛЕНИЯ ПРИ ЗАГРУЗКЕ ПРОЕКТА
        public void OnProjectLoaded()
        {
            RefreshCanvas();
        }
    }
}