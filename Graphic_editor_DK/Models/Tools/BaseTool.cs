using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Graphic_editor_DK.Utilities.Enums;

namespace Graphic_editor_DK.Models.Tools
{
    public abstract class BaseTool
    {
        public abstract ToolType ToolType { get; }
        public abstract void OnMouseDown(Point point, MouseButtonEventArgs e);
        public abstract void OnMouseMove(Point point, MouseEventArgs e);
        public abstract void OnMouseUp(Point point, MouseButtonEventArgs e);
    }
}
