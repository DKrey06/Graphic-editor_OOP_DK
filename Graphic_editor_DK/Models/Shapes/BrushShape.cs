using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Graphic_editor_DK.Models.Shapes
{
    [Serializable]
    public class BrushShape : BaseShape
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public double BrushSize { get; set; } = 3;

        public override void Draw(DrawingContext drawingContext)
        {
            if (Points.Count < 2) return;

            var pen = new Pen(Stroke, BrushSize)
            {
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };

            for (int i = 0; i < Points.Count - 1; i++)
            {
                drawingContext.DrawLine(pen, Points[i], Points[i + 1]);
            }
        }

        public override bool ContainsPoint(Point point)
        {
            foreach (var pt in Points)
            {
                if (Math.Abs(pt.X - point.X) < BrushSize + 5 &&
                    Math.Abs(pt.Y - point.Y) < BrushSize + 5)
                    return true;
            }
            return false;
        }
    }
}