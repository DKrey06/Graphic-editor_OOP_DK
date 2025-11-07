using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Graphic_editor_DK.Models.Shapes
{
    [XmlInclude(typeof(LineShape))]
    [XmlInclude(typeof(RectangleShape))]
    [XmlInclude(typeof(EllipseShape))]
    [XmlInclude(typeof(TriangleShape))]
    [Serializable]
    [JsonObject(IsReference = false)]
    public abstract class BaseShape
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public Brush Stroke { get; set; } = Brushes.Black;

        [XmlIgnore]
        [JsonIgnore]
        public Brush Fill { get; set; } = Brushes.Transparent;

        public double StrokeThickness { get; set; } = 2;

        public string StrokeColor
        {
            get => (Stroke as SolidColorBrush)?.Color.ToString();
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
                    }
                    catch
                    {
                        Stroke = Brushes.Black;
                    }
                }
            }
        }

        public string FillColor
        {
            get => (Fill as SolidColorBrush)?.Color.ToString();
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
                    }
                    catch
                    {
                        Fill = Brushes.Transparent;
                    }
                }
            }
        }

        public abstract void Draw(DrawingContext drawingContext);
        public abstract bool ContainsPoint(Point point);
    }
}