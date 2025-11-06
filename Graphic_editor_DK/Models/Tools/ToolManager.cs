using Graphic_editor_DK.Models.Shapes;
using Graphic_editor_DK.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphic_editor_DK.Models.Tools
{
    public class ToolManager
    {
        private BaseTool _currentTool;

        public BaseTool CurrentTool
        {
            get => _currentTool;
            set
            {
                _currentTool = value;
                ToolChanged?.Invoke();
            }
        }

        public event Action ToolChanged;

        public void SetTool(ToolType toolType)
        {
            CurrentTool = CreateTool(toolType);
        }

        private BaseTool CreateTool(ToolType toolType)
        {
            switch (toolType)
            {
                case ToolType.Selection:
                    return new SelectionTool();
                case ToolType.Line:
                    return new LineTool();
                case ToolType.Rectangle:
                    return new RectangleTool();
                case ToolType.Ellipse:
                    return new EllipseTool();
                default:
                    return new SelectionTool();
            }
        }
    }
}