using Graphic_editor_DK.Models.Shapes;
using Graphic_editor_DK.Models.Tools;
using Graphic_editor_DK.Services;
using Graphic_editor_DK.Utilities.Enums;
using Graphic_editor_DK.Utilities.Extensions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace Graphic_editor_DK.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private ToolManager _toolManager;
        private DrawingService _drawingService;
        private FileService _fileService;
        private ColorPaletteService _colorPaletteService;
        private MainWindow _mainWindow;

        public MainViewModel()
        {
            _toolManager = new ToolManager();
            _drawingService = new DrawingService();
            _fileService = new FileService();
            _colorPaletteService = new ColorPaletteService();

            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand(ExecuteOpen);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
            ExitCommand = new RelayCommand(ExecuteExit);
            SetToolCommand = new RelayCommand<ToolType>(ExecuteSetTool);
            ShowColorPickerCommand = new RelayCommand<string>(ShowColorPicker);
        }

        public void SetMainWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public ToolManager ToolManager => _toolManager;
        public DrawingService DrawingService => _drawingService;
        public ColorPaletteService ColorPaletteService => _colorPaletteService;

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SetToolCommand { get; }
        public ICommand ShowColorPickerCommand { get; }

        private void ShowColorPicker(string colorType)
        {
            var colorPicker = new Xceed.Wpf.Toolkit.ColorPicker
            {
                AvailableColorsSortingMode = Xceed.Wpf.Toolkit.ColorSortingMode.HueSaturationBrightness,
                ShowStandardColors = true,
                ShowAvailableColors = true,
                ShowRecentColors = true,
                SelectedColor = colorType == "Stroke" ?
                    _colorPaletteService.SelectedStrokeColor :
                    _colorPaletteService.SelectedFillColor,
                Width = 300,
                Height = 30,
                IsOpen = true 
            };


            var okButton = new Button { Content = "OK", Width = 80, Margin = new Thickness(0, 10, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(0, 10, 0, 0) };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };
            stackPanel.Children.Add(colorPicker);
            stackPanel.Children.Add(buttonPanel);

            var dialog = new Window
            {
                Title = colorType == "Stroke" ? "Выбор цвета обводки" : "Выбор цвета заливки",
                Width = 350,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Content = stackPanel,
                ResizeMode = ResizeMode.NoResize
            };

            Color? selectedColor = null;

            colorPicker.SelectedColorChanged += (s, e) =>
            {
                if (colorPicker.SelectedColor.HasValue)
                {
                    selectedColor = colorPicker.SelectedColor.Value;
                }
            };

            okButton.Click += (s, e) =>
            {
                if (selectedColor.HasValue)
                {
                    if (colorType == "Stroke")
                    {
                        _colorPaletteService.SelectedStrokeColor = selectedColor.Value;
                        _mainWindow?.UpdateSelectedShapeStrokeColor(new SolidColorBrush(selectedColor.Value));
                    }
                    else
                    {
                        _colorPaletteService.SelectedFillColor = selectedColor.Value;
                        _mainWindow?.UpdateSelectedShapeFillColor(new SolidColorBrush(selectedColor.Value));
                    }
                }
                dialog.DialogResult = true;
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
            };


            if (dialog.ShowDialog() == true)
            {
            }
        }

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