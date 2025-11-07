using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Graphic_editor_DK.Models.Shapes
{
    [Serializable]
    public class TextShape : BaseShape
    {
        public string Text { get; set; } = "";
        public double FontSize { get; set; } = 12;
        public string FontFamily { get; set; } = "Arial";

        public override void Draw(DrawingContext drawingContext)
        {
            var formattedText = new FormattedText(
                Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily),
                FontSize,
                Stroke,
                1.0);

            drawingContext.DrawText(formattedText, StartPoint);
        }

        public override bool ContainsPoint(Point point)
        {
            return Math.Abs(StartPoint.X - point.X) < 100 &&
                   Math.Abs(StartPoint.Y - point.Y) < 50;
        }
    }
}