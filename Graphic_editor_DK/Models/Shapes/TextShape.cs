using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Graphic_editor_DK.Models.Shapes
{
    [Serializable]
    public class TextShape : BaseShape
    {
        public string Text { get; set; } = "Текст";
        public double FontSize { get; set; } = 14;
        public string FontFamily { get; set; } = "Arial";
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        public FontStyle FontStyle { get; set; } = FontStyles.Normal;

        [XmlIgnore]
        public Brush Background { get; set; } = Brushes.Transparent;
        public string SerializedBackground
        {
            get => Background.ToString();
            set => Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        public override void Draw(DrawingContext drawingContext)
        {
            var formattedText = new FormattedText(
                Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily(FontFamily), FontStyle, FontWeight, FontStretches.Normal),
                FontSize,
                Stroke,
                1.0);

            drawingContext.DrawText(formattedText, StartPoint);
        }

        public override bool ContainsPoint(Point point)
        {
            return point.X >= StartPoint.X && point.X <= EndPoint.X &&
                   point.Y >= StartPoint.Y && point.Y <= EndPoint.Y;
        }
    }
}