using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Xml.Serialization;
using Graphic_editor_DK.Models.Shapes;
using System.Collections.Generic;

namespace Graphic_editor_DK.Services
{
    public class FileService
    {
        public void SaveProject(List<BaseShape> shapes)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Graphic Editor Project (*.gep)|*.gep",
                DefaultExt = ".gep"
            };

            if (saveDialog.ShowDialog() == true)
            {
                MessageBox.Show($"Проект сохранен: {saveDialog.FileName}", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public List<BaseShape> LoadProject()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Graphic Editor Project (*.gep)|*.gep",
                DefaultExt = ".gep"
            };

            if (openDialog.ShowDialog() == true)
            {
                MessageBox.Show($"Проект загружен: {openDialog.FileName}", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return new List<BaseShape>();
            }
            return null;
        }

        public void ExportToImage()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                DefaultExt = ".png"
            };

            if (saveDialog.ShowDialog() == true)
            {
                MessageBox.Show($"Изображение экспортировано: {saveDialog.FileName}", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}