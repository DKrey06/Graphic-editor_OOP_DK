using Graphic_editor_DK.Models.Shapes;
using Graphic_editor_DK.Models.Tools;
using Graphic_editor_DK.Services;
using Graphic_editor_DK.Utilities.Enums;
using Graphic_editor_DK.Utilities.Extensions;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Graphic_editor_DK.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private ToolManager _toolManager;
        private DrawingService _drawingService;
        private FileService _fileService;

        public MainViewModel()
        {
            _toolManager = new ToolManager();
            _drawingService = new DrawingService();
            _fileService = new FileService();

            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand(ExecuteOpen);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExitCommand = new RelayCommand(ExecuteExit);

            SetToolCommand = new RelayCommand<ToolType>(ExecuteSetTool);
        }

        public ToolManager ToolManager => _toolManager;
        public DrawingService DrawingService => _drawingService;

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand SetToolCommand { get; }

        private void ExecuteNew()
        {
            var result = MessageBox.Show("Создать новый проект?", "Новый проект",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _drawingService.Clear();
                MessageBox.Show("Новый проект создан", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteSave()
        {
            var shapesList = new List<BaseShape>(_drawingService.Shapes);
            _fileService.SaveProject(shapesList);
        }

        private void ExecuteOpen()
        {
            var shapes = _fileService.LoadProject();
            if (shapes != null)
            {
                _drawingService.Clear();
                foreach (var shape in shapes)
                {
                    _drawingService.Shapes.Add(shape);
                }
            }
        }

        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }

        private void ExecuteSetTool(ToolType toolType)
        {
            _toolManager.SetTool(toolType);
        }
    }
}