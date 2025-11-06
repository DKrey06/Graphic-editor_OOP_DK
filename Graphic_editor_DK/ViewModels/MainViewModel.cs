using System.Windows;
using System.Windows.Input;
using Graphic_editor_DK.Models.Tools;
using Graphic_editor_DK.Utilities.Enums;
using Graphic_editor_DK.Utilities.Extensions;
using Microsoft.Win32;

namespace Graphic_editor_DK.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private ToolManager _toolManager;

        public MainViewModel()
        {
            _toolManager = new ToolManager();

            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand(ExecuteOpen);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExitCommand = new RelayCommand(ExecuteExit);

            SetToolCommand = new RelayCommand<ToolType>(ExecuteSetTool);
        }

        public ToolManager ToolManager => _toolManager;

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand SetToolCommand { get; }

        private void ExecuteNew()
        {
            var result = MessageBox.Show("Создать новый проект?",
                                        "Новый проект",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Новый проект создан", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                //Очистку холста добавить надо будет
            }
        }

        private void ExecuteOpen()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                MessageBox.Show($"Открытие файла: {openFileDialog.FileName}");
            }
        }

        private void ExecuteSave()
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                MessageBox.Show($"Сохранение в: {saveFileDialog.FileName}");
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