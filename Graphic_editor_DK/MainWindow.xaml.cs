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
        private UIElement _selectedElement;
        private BaseShape _selectedShape;
        private bool _isDragging;
        private Point _dragStartPoint;

        private TextBox _currentTextbox;
        private bool _isTextEditing = false;

        private Dictionary<UIElement, Brush> _originalStrokes = new Dictionary<UIElement, Brush>();
        private Dictionary<UIElement, Brush> _originalBackgrounds = new Dictionary<UIElement, Brush>();

        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new ViewModels.MainViewModel();
            this.DataContext = viewModel;
            viewModel.SetMainWindow(this);

            viewModel.ToolManager.ToolChanged += OnToolChanged;

            BrushSizeComboBox.SelectedIndex = 1;
            ToolsComboBox.SelectedIndex = 0;

            InitializeTextSettings();

            BoldButton.Click += (s, e) => ToggleTextBold();
            ItalicButton.Click += (s, e) => ToggleTextItalic();
            FontFamilyComboBox.SelectionChanged += (s, e) => UpdateTextFont();
            FontSizeComboBox.SelectionChanged += (s, e) => UpdateTextSize();
        }

        private void InitializeTextSettings()
        {
            BoldButton.Tag = false;
            ItalicButton.Tag = false;
        }

        private void OnToolChanged()
        {
            var currentTool = ViewModel.ToolManager.CurrentTool;
            if (currentTool != null)
            {
                CurrentToolText.Text = "Инструмент: " + GetToolDisplayName(currentTool.ToolType);
                UpdateToolsComboBoxSelection(currentTool.ToolType);

                if (currentTool.ToolType == Graphic_editor_DK.Utilities.Enums.ToolType.Text)
                {
                    TextSettingsPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    TextSettingsPanel.Visibility = Visibility.Collapsed;
                    if (_isTextEditing)
                    {
                        CompleteTextPlacement();
                    }
                }
            }
        }

        private string GetToolDisplayName(Graphic_editor_DK.Utilities.Enums.ToolType toolType)
        {
            switch (toolType)
            {
                case Graphic_editor_DK.Utilities.Enums.ToolType.Selection: return "Курсор";
                case Graphic_editor_DK.Utilities.Enums.ToolType.Line: return "Линия";
                case Graphic_editor_DK.Utilities.Enums.ToolType.Rectangle: return "Прямоугольник";
                case Graphic_editor_DK.Utilities.Enums.ToolType.Ellipse: return "Эллипс";
                case Graphic_editor_DK.Utilities.Enums.ToolType.Triangle: return "Треугольник";
                case Graphic_editor_DK.Utilities.Enums.ToolType.Brush: return "Кисть";
                case Graphic_editor_DK.Utilities.Enums.ToolType.Text: return "Текст";
                case Graphic_editor_DK.Utilities.Enums.ToolType.Eraser: return "Ластик";
                default: return "Курсор";
            }
        }

        private void UpdateToolsComboBoxSelection(Graphic_editor_DK.Utilities.Enums.ToolType toolType)
        {
            string tag = toolType.ToString();
            foreach (ComboBoxItem item in ToolsComboBox.Items)
            {
                if (item.Tag as string == tag)
                {
                    ToolsComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ToolsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsComboBox.SelectedItem is ComboBoxItem item && item.Tag is string toolTag)
            {
                Graphic_editor_DK.Utilities.Enums.ToolType toolType;

                if (toolTag == "Selection")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Selection;
                else if (toolTag == "Line")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Line;
                else if (toolTag == "Rectangle")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Rectangle;
                else if (toolTag == "Ellipse")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Ellipse;
                else if (toolTag == "Triangle")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Triangle;
                else if (toolTag == "Brush")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Brush;
                else if (toolTag == "Text")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Text;
                else if (toolTag == "Eraser")
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Eraser;
                else
                    toolType = Graphic_editor_DK.Utilities.Enums.ToolType.Selection;

                ViewModel.ToolManager.SetTool(toolType);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _selectedElement != null)
            {
                DeleteSelectedShape();
            }
            else if (e.Key == Key.Escape && _isTextEditing)
            {
                CancelTextPlacement();
            }
            else if (e.Key == Key.Escape)
            {
                ClearSelection();
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
                        ClearSelection();
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
                    if (!_isTextEditing)
                    {
                        StartTextPlacement(point);
                    }
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

        private void UpdateCurrentShape(Point currentPoint)
        {
            if (_currentShapeElement == null || _currentShape == null) return;

            if (_currentShapeElement is Line line)
            {
                line.X2 = currentPoint.X;
                line.Y2 = currentPoint.Y;
                _currentShape.EndPoint = currentPoint;
            }
            else if (_currentShapeElement is Rectangle rect)
            {
                double left = Math.Min(_startPoint.X, currentPoint.X);
                double top = Math.Min(_startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);

                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, top);
                rect.Width = width;
                rect.Height = height;

                _currentShape.EndPoint = currentPoint;
            }
            else if (_currentShapeElement is Ellipse ellipse)
            {
                double left = Math.Min(_startPoint.X, currentPoint.X);
                double top = Math.Min(_startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);

                Canvas.SetLeft(ellipse, left);
                Canvas.SetTop(ellipse, top);
                ellipse.Width = width;
                ellipse.Height = height;

                _currentShape.EndPoint = currentPoint;
            }
            else if (_currentShapeElement is Polygon polygon)
            {
                UpdateTrianglePoints(polygon, _startPoint, currentPoint);
                _currentShape.EndPoint = currentPoint;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(DrawingCanvas);
            CoordinatesText.Text = $"X: {(int)point.X}, Y: {(int)point.Y}";
        }

        private double GetCurrentBrushSize()
        {
            if (BrushSizeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string sizeTag)
            {
                if (double.TryParse(sizeTag, out double size))
                {
                    return size;
                }
            }
            return 3;
        }

        private Brush GetCurrentStrokeColor()
        {
            return new SolidColorBrush(ViewModel.ColorPaletteService.SelectedStrokeColor);
        }

        private Brush GetCurrentFillColor()
        {
            return new SolidColorBrush(ViewModel.ColorPaletteService.SelectedFillColor);
        }

        private void StartBrushStroke(Point startPoint)
        {
            _currentBrushShape = new BrushShape
            {
                Stroke = GetCurrentStrokeColor(),
                BrushSize = GetCurrentBrushSize(),
                StartPoint = startPoint,
                EndPoint = startPoint
            };
            _currentBrushShape.Points.Add(startPoint);

            var polyline = new Polyline
            {
                Stroke = GetCurrentStrokeColor(),
                StrokeThickness = GetCurrentBrushSize(),
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

        private void CreateNewShape(Point startPoint)
        {
            var currentTool = ViewModel.ToolManager.CurrentTool;
            var strokeColor = GetCurrentStrokeColor();
            var fillColor = GetCurrentFillColor();
            var brushSize = GetCurrentBrushSize();

            if (currentTool is LineTool)
            {
                var line = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = startPoint.X,
                    Y2 = startPoint.Y,
                    Stroke = strokeColor,
                    StrokeThickness = brushSize
                };

                DrawingCanvas.Children.Add(line);
                _currentShapeElement = line;

                _currentShape = new LineShape
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint,
                    Stroke = strokeColor,
                    StrokeThickness = brushSize
                };
            }
            else if (currentTool is RectangleTool)
            {
                var rectangle = new Rectangle
                {
                    Stroke = strokeColor,
                    Fill = fillColor,
                    StrokeThickness = brushSize
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
                    Stroke = strokeColor,
                    Fill = fillColor,
                    StrokeThickness = brushSize
                };
            }
            else if (currentTool is EllipseTool)
            {
                var ellipse = new Ellipse
                {
                    Stroke = strokeColor,
                    Fill = fillColor,
                    StrokeThickness = brushSize
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
                    Stroke = strokeColor,
                    Fill = fillColor,
                    StrokeThickness = brushSize
                };
            }
            else if (currentTool is TriangleTool)
            {
                var polygon = new Polygon
                {
                    Stroke = strokeColor,
                    Fill = fillColor,
                    StrokeThickness = brushSize
                };

                UpdateTrianglePoints(polygon, startPoint, startPoint);

                DrawingCanvas.Children.Add(polygon);
                _currentShapeElement = polygon;

                _currentShape = new TriangleShape
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint,
                    Stroke = strokeColor,
                    Fill = fillColor,
                    StrokeThickness = brushSize
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
                Width = 200,
                Height = 100,
                FontSize = GetCurrentFontSize(),
                FontFamily = GetCurrentFontFamily(),
                FontWeight = GetCurrentFontWeight(),
                FontStyle = GetCurrentFontStyle(),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray,
                Background = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalContentAlignment = VerticalAlignment.Top,
                Text = "Введите текст здесь..."
            };

            Canvas.SetLeft(_currentTextbox, position.X);
            Canvas.SetTop(_currentTextbox, position.Y);

            _currentTextbox.KeyDown += TextBox_KeyDown;
            _currentTextbox.LostFocus += TextBox_LostFocus;

            DrawingCanvas.Children.Add(_currentTextbox);
            _isTextEditing = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _currentTextbox.Focus();
                _currentTextbox.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CompleteTextPlacement();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelTextPlacement();
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isTextEditing)
            {
                CompleteTextPlacement();
            }
        }

        private void CompleteTextPlacement()
        {
            if (_currentTextbox != null && !string.IsNullOrWhiteSpace(_currentTextbox.Text))
            {
                var textShape = new TextShape
                {
                    StartPoint = new Point(Canvas.GetLeft(_currentTextbox), Canvas.GetTop(_currentTextbox)),
                    EndPoint = new Point(Canvas.GetLeft(_currentTextbox) + _currentTextbox.ActualWidth,
                                       Canvas.GetTop(_currentTextbox) + _currentTextbox.ActualHeight),
                    Stroke = GetCurrentStrokeColor(),
                    Text = _currentTextbox.Text,
                    FontSize = _currentTextbox.FontSize,
                    FontFamily = _currentTextbox.FontFamily.Source,
                    FontWeight = _currentTextbox.FontWeight,
                    FontStyle = _currentTextbox.FontStyle
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

            _currentTextbox = null;
            _isTextEditing = false;
        }

        private void CancelTextPlacement()
        {
            if (_currentTextbox != null)
            {
                DrawingCanvas.Children.Remove(_currentTextbox);
                _currentTextbox = null;
            }
            _isTextEditing = false;
        }

        private void DrawTextShape(TextShape textShape)
        {
            var textBlock = new TextBlock
            {
                Text = textShape.Text,
                FontSize = textShape.FontSize,
                FontFamily = new FontFamily(textShape.FontFamily),
                FontWeight = textShape.FontWeight,
                FontStyle = textShape.FontStyle,
                Foreground = textShape.Stroke,
                Background = Brushes.Transparent,
                TextWrapping = TextWrapping.Wrap
            };

            Canvas.SetLeft(textBlock, textShape.StartPoint.X);
            Canvas.SetTop(textBlock, textShape.StartPoint.Y);

            DrawingCanvas.Children.Add(textBlock);
        }

        private double GetCurrentFontSize()
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem item && double.TryParse(item.Content.ToString(), out double size))
            {
                return size;
            }
            return 14;
        }

        private FontFamily GetCurrentFontFamily()
        {
            if (FontFamilyComboBox.SelectedItem is ComboBoxItem item)
            {
                string fontName = item.Content.ToString();

                var availableFonts = Fonts.SystemFontFamilies;
                foreach (var font in availableFonts)
                {
                    if (font.Source.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                    {
                        return font;
                    }
                }
                return new FontFamily("Arial");
            }
            return new FontFamily("Arial");
        }

        private FontWeight GetCurrentFontWeight()
        {
            return (BoldButton.Tag as bool?) == true ? FontWeights.Bold : FontWeights.Normal;
        }

        private FontStyle GetCurrentFontStyle()
        {
            return (ItalicButton.Tag as bool?) == true ? FontStyles.Italic : FontStyles.Normal;
        }

        private void ToggleTextBold()
        {
            bool isBold = (BoldButton.Tag as bool?) ?? false;
            BoldButton.Tag = !isBold;
            BoldButton.FontWeight = !isBold ? FontWeights.Bold : FontWeights.Normal;

            if (_currentTextbox != null)
            {
                _currentTextbox.FontWeight = !isBold ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        private void ToggleTextItalic()
        {
            bool isItalic = (ItalicButton.Tag as bool?) ?? false;
            ItalicButton.Tag = !isItalic;
            ItalicButton.FontStyle = !isItalic ? FontStyles.Italic : FontStyles.Normal;

            if (_currentTextbox != null)
            {
                _currentTextbox.FontStyle = !isItalic ? FontStyles.Italic : FontStyles.Normal;
            }
        }

        private void UpdateTextFont()
        {
            if (_currentTextbox != null)
            {
                _currentTextbox.FontFamily = GetCurrentFontFamily();
            }
        }

        private void UpdateTextSize()
        {
            if (_currentTextbox != null)
            {
                _currentTextbox.FontSize = GetCurrentFontSize();
            }
        }

        private void SelectElement(UIElement element, BaseShape shape)
        {
            _selectedElement = element;
            _selectedShape = shape;

            if (element is Shape shapeElement)
            {
                if (!_originalStrokes.ContainsKey(element))
                {
                    _originalStrokes[element] = shapeElement.Stroke;
                }
                shapeElement.Stroke = Brushes.Red;
            }
            else if (element is TextBlock textBlock)
            {
                if (!_originalBackgrounds.ContainsKey(element))
                {
                    _originalBackgrounds[element] = textBlock.Background;
                }
                textBlock.Background = new SolidColorBrush(Color.FromArgb(80, 255, 0, 0));
            }
        }

        private void ClearSelection()
        {
            if (_selectedElement is Shape shapeElement && _originalStrokes.ContainsKey(_selectedElement))
            {
                shapeElement.Stroke = _selectedShape?.Stroke ?? _originalStrokes[_selectedElement];
                _originalStrokes.Remove(_selectedElement);
            }
            else if (_selectedElement is TextBlock textBlock && _originalBackgrounds.ContainsKey(_selectedElement))
            {
                textBlock.Background = _originalBackgrounds[_selectedElement];
                _originalBackgrounds.Remove(_selectedElement);
            }

            _selectedElement = null;
            _selectedShape = null;
        }

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

        private bool TrySelectShape(Point point)
        {
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
            var shape = _shapes.Find(s => s is RectangleShape rs &&
                Math.Abs(rs.StartPoint.X - left) < 0.1 &&
                Math.Abs(rs.StartPoint.Y - top) < 0.1);
            return shape;
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

                if (_originalStrokes.ContainsKey(_selectedElement))
                {
                    _originalStrokes[_selectedElement] = stroke;
                }
            }
            else if (_selectedElement is TextBlock textBlock && _selectedShape is TextShape textShape)
            {
                textBlock.Foreground = stroke;
                textShape.Stroke = stroke;
            }
        }

        private void BrushSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public void RefreshCanvas()
        {
            DrawingCanvas.Children.Clear();
            _shapes.Clear();
            _originalStrokes.Clear();
            _originalBackgrounds.Clear();

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

        public void OnProjectLoaded()
        {
            RefreshCanvas();
        }

        public void UpdateSelectedShapeStrokeColor(Brush strokeBrush)
        {
            if (_selectedElement != null && _selectedShape != null)
            {
                ChangeSelectedShapeColor(strokeBrush, _selectedShape.Fill ?? Brushes.Transparent);
            }
        }

        public void UpdateSelectedShapeFillColor(Brush fillBrush)
        {
            if (_selectedElement != null && _selectedShape != null)
            {
                ChangeSelectedShapeColor(_selectedShape.Stroke ?? Brushes.Black, fillBrush);
            }
        }
    }
}