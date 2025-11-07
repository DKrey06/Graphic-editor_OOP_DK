using Graphic_editor_DK.Models.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Graphic_editor_DK.Services
{
    public class FileService
    {
        public void SaveProject(List<BaseShape> shapes)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Graphic Editor Project (*.json)|*.json|XML File (*.xml)|*.xml",
                DefaultExt = ".json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();

                    if (extension == ".json")
                    {
                        var settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            TypeNameHandling = TypeNameHandling.Auto
                        };

                        string json = JsonConvert.SerializeObject(shapes, settings);
                        File.WriteAllText(saveDialog.FileName, json);
                    }
                    else
                    {
                        var serializer = new XmlSerializer(typeof(List<BaseShape>),
                            new[] { typeof(LineShape), typeof(RectangleShape),
                                    typeof(EllipseShape), typeof(TriangleShape) });

                        using (var stream = File.Create(saveDialog.FileName))
                        {
                            serializer.Serialize(stream, shapes);
                        }
                    }

                    MessageBox.Show($"Проект сохранен: {saveDialog.FileName}", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public List<BaseShape> LoadProject()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Graphic Editor Project (*.json;*.xml)|*.json;*.xml",
                DefaultExt = ".json"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(openDialog.FileName).ToLower();
                    List<BaseShape> shapes;

                    if (extension == ".json")
                    {
                        string json = File.ReadAllText(openDialog.FileName);
                        var settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        };

                        shapes = JsonConvert.DeserializeObject<List<BaseShape>>(json, settings);
                    }
                    else
                    {
                        var serializer = new XmlSerializer(typeof(List<BaseShape>),
                            new[] { typeof(LineShape), typeof(RectangleShape),
                                    typeof(EllipseShape), typeof(TriangleShape) });

                        using (var stream = File.OpenRead(openDialog.FileName))
                        {
                            shapes = (List<BaseShape>)serializer.Deserialize(stream);
                        }
                    }

                    MessageBox.Show($"Проект загружен: {openDialog.FileName}", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return shapes;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        public void ExportToImage(Canvas drawingCanvas)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|BMP Image (*.bmp)|*.bmp",
                DefaultExt = ".png"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    double minX = double.MaxValue, minY = double.MaxValue;
                    double maxX = double.MinValue, maxY = double.MinValue;

                    foreach (UIElement child in drawingCanvas.Children)
                    {
                        if (child is Line line)
                        {
                            minX = Math.Min(minX, Math.Min(line.X1, line.X2));
                            minY = Math.Min(minY, Math.Min(line.Y1, line.Y2));
                            maxX = Math.Max(maxX, Math.Max(line.X1, line.X2));
                            maxY = Math.Max(maxY, Math.Max(line.Y1, line.Y2));
                        }
                        else if (child is Rectangle rect)
                        {
                            double left = Canvas.GetLeft(rect);
                            double top = Canvas.GetTop(rect);
                            minX = Math.Min(minX, left);
                            minY = Math.Min(minY, top);
                            maxX = Math.Max(maxX, left + rect.Width);
                            maxY = Math.Max(maxY, top + rect.Height);
                        }
                        else if (child is Ellipse ellipse)
                        {
                            double left = Canvas.GetLeft(ellipse);
                            double top = Canvas.GetTop(ellipse);
                            minX = Math.Min(minX, left);
                            minY = Math.Min(minY, top);
                            maxX = Math.Max(maxX, left + ellipse.Width);
                            maxY = Math.Max(maxY, top + ellipse.Height);
                        }
                        else if (child is Polygon polygon)
                        {
                            foreach (Point point in polygon.Points)
                            {
                                minX = Math.Min(minX, point.X);
                                minY = Math.Min(minY, point.Y);
                                maxX = Math.Max(maxX, point.X);
                                maxY = Math.Max(maxY, point.Y);
                            }
                        }
                        else if (child is Polyline polyline)
                        {
                            foreach (Point point in polyline.Points)
                            {
                                minX = Math.Min(minX, point.X);
                                minY = Math.Min(minY, point.Y);
                                maxX = Math.Max(maxX, point.X);
                                maxY = Math.Max(maxY, point.Y);
                            }
                        }
                    }

                    double padding = 20;
                    double width = Math.Max(maxX - minX + padding * 2, 100);
                    double height = Math.Max(maxY - minY + padding * 2, 100);

                    var renderCanvas = new Canvas
                    {
                        Width = width,
                        Height = height,
                        Background = Brushes.White
                    };

                    foreach (UIElement child in drawingCanvas.Children)
                    {
                        if (child is Line line)
                        {
                            var newLine = new Line
                            {
                                X1 = line.X1 - minX + padding,
                                Y1 = line.Y1 - minY + padding,
                                X2 = line.X2 - minX + padding,
                                Y2 = line.Y2 - minY + padding,
                                Stroke = line.Stroke,
                                StrokeThickness = line.StrokeThickness
                            };
                            renderCanvas.Children.Add(newLine);
                        }
                        else if (child is Rectangle rect)
                        {
                            var newRect = new Rectangle
                            {
                                Stroke = rect.Stroke,
                                Fill = rect.Fill,
                                StrokeThickness = rect.StrokeThickness,
                                Width = rect.Width,
                                Height = rect.Height
                            };
                            Canvas.SetLeft(newRect, Canvas.GetLeft(rect) - minX + padding);
                            Canvas.SetTop(newRect, Canvas.GetTop(rect) - minY + padding);
                            renderCanvas.Children.Add(newRect);
                        }
                        else if (child is Ellipse ellipse)
                        {
                            var newEllipse = new Ellipse
                            {
                                Stroke = ellipse.Stroke,
                                Fill = ellipse.Fill,
                                StrokeThickness = ellipse.StrokeThickness,
                                Width = ellipse.Width,
                                Height = ellipse.Height
                            };
                            Canvas.SetLeft(newEllipse, Canvas.GetLeft(ellipse) - minX + padding);
                            Canvas.SetTop(newEllipse, Canvas.GetTop(ellipse) - minY + padding);
                            renderCanvas.Children.Add(newEllipse);
                        }
                        else if (child is Polygon polygon)
                        {
                            var newPolygon = new Polygon
                            {
                                Stroke = polygon.Stroke,
                                Fill = polygon.Fill,
                                StrokeThickness = polygon.StrokeThickness
                            };
                            foreach (Point point in polygon.Points)
                            {
                                newPolygon.Points.Add(new Point(
                                    point.X - minX + padding,
                                    point.Y - minY + padding));
                            }
                            renderCanvas.Children.Add(newPolygon);
                        }
                        else if (child is Polyline polyline)
                        {
                            var newPolyline = new Polyline
                            {
                                Stroke = polyline.Stroke,
                                StrokeThickness = polyline.StrokeThickness,
                                StrokeLineJoin = polyline.StrokeLineJoin,
                                StrokeStartLineCap = polyline.StrokeStartLineCap,
                                StrokeEndLineCap = polyline.StrokeEndLineCap
                            };
                            foreach (Point point in polyline.Points)
                            {
                                newPolyline.Points.Add(new Point(
                                    point.X - minX + padding,
                                    point.Y - minY + padding));
                            }
                            renderCanvas.Children.Add(newPolyline);
                        }
                    }

                    renderCanvas.Measure(new Size(width, height));
                    renderCanvas.Arrange(new Rect(0, 0, width, height));

                    var renderBitmap = new RenderTargetBitmap(
                        (int)width, (int)height,
                        96d, 96d, PixelFormats.Pbgra32);

                    renderBitmap.Render(renderCanvas);

                    BitmapEncoder encoder;
                    string extension = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        case ".png":
                        default:
                            encoder = new PngBitmapEncoder();
                            break;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                    using (var fileStream = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show($"Изображение экспортировано: {saveDialog.FileName}", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}