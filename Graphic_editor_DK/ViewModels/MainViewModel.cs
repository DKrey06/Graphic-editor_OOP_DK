using Graphic_editor_DK.Models.Shapes;
using Graphic_editor_DK.Models.Tools;
using Graphic_editor_DK.Services;
using Graphic_editor_DK.Utilities.Enums;
using Graphic_editor_DK.Utilities.Extensions;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Graphic_editor_DK.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private ToolManager _toolManager;
        private DrawingService _drawingService;
        private FileService _fileService;
        private MainWindow _mainWindow;

        public MainViewModel()
        {
            _toolManager = new ToolManager();
            _drawingService = new DrawingService();
            _fileService = new FileService();

            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand(ExecuteOpen);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
            ExitCommand = new RelayCommand(ExecuteExit);

            SetToolCommand = new RelayCommand<ToolType>(ExecuteSetTool);
        }
        public void SetMainWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public ToolManager ToolManager => _toolManager;
        public DrawingService DrawingService => _drawingService;

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand SetToolCommand { get; }

        private void ExecuteNew()
        {
            var result = MessageBox.Show("Создать новый проект? Все несохраненные данные будут потеряны.", "Новый проект",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _drawingService.Clear();
                _mainWindow?.RefreshCanvas();
                MessageBox.Show("Новый проект создан", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteSave()
        {
            if (_drawingService.Shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для сохранения", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var shapesList = new List<BaseShape>(_drawingService.Shapes);
            _fileService.SaveProject(shapesList);
        }

        private void ExecuteOpen()
        {
            var result = MessageBox.Show("Загрузить проект? Все несохраненные данные будут потеряны.", "Загрузка проекта",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var shapes = _fileService.LoadProject();
                if (shapes != null)
                {
                    _drawingService.Clear();
                    foreach (var shape in shapes)
                    {
                        _drawingService.Shapes.Add(shape);
                    }
                    _mainWindow?.RefreshCanvas();
                }
            }
        }

        private void ExecuteExport()
        {
            if (_drawingService.Shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для экспорта", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_mainWindow != null)
            {
                _fileService.ExportToImage(_mainWindow.DrawingCanvas);
            }
        }

        private void ExecuteExit()
        {
            var result = MessageBox.Show("Выйти из программы? Все несохраненные данные будут потеряны.", "Выход",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void ExecuteSetTool(ToolType toolType)
        {
            _toolManager.SetTool(toolType);
        }
    }
}