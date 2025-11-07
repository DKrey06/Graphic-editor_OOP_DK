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

        private BrushShape _currentBrushShape;
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
            else if (_currentBrushShape != null && e.LeftButton == MouseButtonState.Pressed)
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
            else if (_currentBrushShape != null)
            {

                _currentBrushShape.Points.Add(point);
                _shapes.Add(_currentBrushShape);
                ViewModel.DrawingService.Shapes.Add(_currentBrushShape);
                _currentBrushShape = null;
                _currentShapeElement = null;
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

        // МЕТОДЫ ДЛЯ КИСТИ - ПЕРЕРАБОТАННЫЕ
        private void StartBrushStroke(Point startPoint)
        {
            _currentBrushShape = new BrushShape
            {
                Stroke = _currentBrushColor,
                BrushSize = _currentBrushSize,
                StartPoint = startPoint,
                EndPoint = startPoint
            };
            _currentBrushShape.Points.Add(startPoint);

            var polyline = new Polyline
            {
                Stroke = _currentBrushColor,
                StrokeThickness = _currentBrushSize,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            polyline.Points.Add(startPoint);

            DrawingCanvas.Children.Add(polyline);
            _currentShapeElement = polyline;
        }

        private void UpdateBrushStroke(Point currentPoint)
        {
            if (_currentBrushShape != null && _currentShapeElement is Polyline polyline)
            {
                _currentBrushShape.Points.Add(currentPoint);
                polyline.Points.Add(currentPoint);
                _currentBrushShape.EndPoint = currentPoint;
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

        // МЕТОД ДЛЯ ТЕКСТА
        private void StartTextPlacement(Point position)
        {
            _currentTextbox = new TextBox
            {
                Width = 200,
                Height = 30,
                FontSize = 14,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray,
                Background = Brushes.White
            };

            Canvas.SetLeft(_currentTextbox, position.X);
            Canvas.SetTop(_currentTextbox, position.Y);

            _currentTextbox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    CompleteTextPlacement();
                }
                else if (e.Key == Key.Escape)
                {
                    CancelTextPlacement();
                }
            };

            _currentTextbox.LostFocus += (s, e) =>
            {
                CompleteTextPlacement();
            };

            DrawingCanvas.Children.Add(_currentTextbox);
            _currentTextbox.Focus();
            _isPlacingText = true;
        }

        private void CompleteTextPlacement()
        {
            if (_currentTextbox != null && !string.IsNullOrWhiteSpace(_currentTextbox.Text))
            {
                var textShape = new TextShape
                {
                    StartPoint = new Point(Canvas.GetLeft(_currentTextbox), Canvas.GetTop(_currentTextbox)),
                    EndPoint = new Point(Canvas.GetLeft(_currentTextbox) + 200, Canvas.GetTop(_currentTextbox) + 30),
                    Stroke = _currentBrushColor,
                    Text = _currentTextbox.Text,
                    FontSize = 14
                };

                _shapes.Add(textShape);
                ViewModel.DrawingService.Shapes.Add(textShape);

                DrawingCanvas.Children.Remove(_currentTextbox);
                DrawTextShape(textShape);
            }
            else
            {
                CancelTextPlacement();
            }
            _isPlacingText = false;
            _currentTextbox = null;
        }

        private void CancelTextPlacement()
        {
            if (_currentTextbox != null)
            {
                DrawingCanvas.Children.Remove(_currentTextbox);
                _currentTextbox = null;
            }
            _isPlacingText = false;
        }

        private void DrawTextShape(TextShape textShape)
        {
            var textBlock = new TextBlock
            {
                Text = textShape.Text,
                FontSize = textShape.FontSize,
                Foreground = textShape.Stroke,
                FontFamily = new FontFamily(textShape.FontFamily),
                Background = Brushes.Transparent
            };

            Canvas.SetLeft(textBlock, textShape.StartPoint.X);
            Canvas.SetTop(textBlock, textShape.StartPoint.Y);

            DrawingCanvas.Children.Add(textBlock);
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
                else if (element is Polyline polyline && IsPointOnPolyline(polyline, point))
                {
                    SelectElement(element, FindShapeForPolyline(polyline));
                    return true;
                }
                else if (element is TextBlock textBlock && IsPointInTextBlock(textBlock, point))
                {
                    SelectElement(element, FindShapeForTextBlock(textBlock));
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

        private bool IsPointInTextBlock(TextBlock textBlock, Point point)
        {
            double left = Canvas.GetLeft(textBlock);
            double top = Canvas.GetTop(textBlock);
            return point.X >= left && point.X <= left + textBlock.ActualWidth &&
                   point.Y >= top && point.Y <= top + textBlock.ActualHeight;
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
            else if (element is TextBlock textBlock)
            {
                textBlock.Background = new SolidColorBrush(Color.FromArgb(80, 255, 0, 0));
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
            else if (_selectedElement is TextBlock textBlock)
            {
                textBlock.Background = Brushes.Transparent;
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
            double left = Canvas.GetLeft(rect);
            double top = Canvas.GetTop(rect);
            return _shapes.Find(s => s is RectangleShape rs &&
                Math.Abs(rs.StartPoint.X - left) < 0.1 &&
                Math.Abs(rs.StartPoint.Y - top) < 0.1);
        }

        private BaseShape FindShapeForEllipse(Ellipse ellipse)
        {
            double left = Canvas.GetLeft(ellipse);
            double top = Canvas.GetTop(ellipse);
            return _shapes.Find(s => s is EllipseShape es &&
                Math.Abs(es.StartPoint.X - left) < 0.1 &&
                Math.Abs(es.StartPoint.Y - top) < 0.1);
        }

        private BaseShape FindShapeForPolygon(Polygon polygon)
        {
            return _shapes.Find(s => s is TriangleShape);
        }

        private BaseShape FindShapeForPolyline(Polyline polyline)
        {
            if (polyline.Points.Count > 0)
            {
                var firstPoint = polyline.Points[0];
                return _shapes.Find(s => s is BrushShape bs &&
                    bs.Points.Count > 0 &&
                    Math.Abs(bs.Points[0].X - firstPoint.X) < 0.1 &&
                    Math.Abs(bs.Points[0].Y - firstPoint.Y) < 0.1);
            }
            return null;
        }

        private BaseShape FindShapeForTextBlock(TextBlock textBlock)
        {
            double left = Canvas.GetLeft(textBlock);
            double top = Canvas.GetTop(textBlock);
            return _shapes.Find(s => s is TextShape ts &&
                Math.Abs(ts.StartPoint.X - left) < 0.1 &&
                Math.Abs(ts.StartPoint.Y - top) < 0.1);
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

                if (_selectedShape is BrushShape brushShape)
                {
                    for (int i = 0; i < brushShape.Points.Count; i++)
                    {
                        brushShape.Points[i] = new Point(brushShape.Points[i].X + deltaX, brushShape.Points[i].Y + deltaY);
                    }
                    brushShape.StartPoint = new Point(brushShape.StartPoint.X + deltaX, brushShape.StartPoint.Y + deltaY);
                    brushShape.EndPoint = new Point(brushShape.EndPoint.X + deltaX, brushShape.EndPoint.Y + deltaY);
                }
            }
            else if (_selectedElement is TextBlock textBlock)
            {
                double newLeft = Canvas.GetLeft(textBlock) + deltaX;
                double newTop = Canvas.GetTop(textBlock) + deltaY;
                Canvas.SetLeft(textBlock, newLeft);
                Canvas.SetTop(textBlock, newTop);

                if (_selectedShape != null)
                {
                    _selectedShape.StartPoint = new Point(_selectedShape.StartPoint.X + deltaX, _selectedShape.StartPoint.Y + deltaY);
                    _selectedShape.EndPoint = new Point(_selectedShape.EndPoint.X + deltaX, _selectedShape.EndPoint.Y + deltaY);
                }
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
                else if (element is TextBlock textBlock && IsPointInTextBlock(textBlock, point))
                {
                    DrawingCanvas.Children.RemoveAt(i);

                    var shapeToRemove = FindShapeForTextBlock(textBlock);
                    if (shapeToRemove != null)
                    {
                        _shapes.Remove(shapeToRemove);
                        ViewModel.DrawingService.Shapes.Remove(shapeToRemove);
                    }
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
            if (element is Line line) return FindShapeForLine(line);
            if (element is Rectangle rect) return FindShapeForRectangle(rect);
            if (element is Ellipse ellipse) return FindShapeForEllipse(ellipse);
            if (element is Polygon polygon) return FindShapeForPolygon(polygon);
            if (element is Polyline polyline) return FindShapeForPolyline(polyline);
            if (element is TextBlock textBlock) return FindShapeForTextBlock(textBlock);
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
            else if (_selectedElement is TextBlock textBlock && _selectedShape is TextShape textShape)
            {
                textBlock.Foreground = stroke;
                textShape.Stroke = stroke;
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
                else if (shape is BrushShape brushShape)
                {
                    var polyline = new Polyline
                    {
                        Stroke = brushShape.Stroke,
                        StrokeThickness = brushShape.BrushSize,
                        StrokeLineJoin = PenLineJoin.Round,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    };

                    foreach (var point in brushShape.Points)
                    {
                        polyline.Points.Add(point);
                    }

                    DrawingCanvas.Children.Add(polyline);
                    _shapes.Add(brushShape);
                }
                else if (shape is TextShape textShape)
                {
                    DrawTextShape(textShape);
                    _shapes.Add(textShape);
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