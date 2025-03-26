using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace AvaloniaApplication.Services
{
    /// <summary>Сервис для вызова диалогового окна и получения файлов.</summary>
    public class FilesService
    {
        #region Public
        /// <summary>Открытие диалогового окна выбора файла изображения.</summary>
        /// <returns>Файл с изображением.</returns>
        public async Task<IStorageFile?> OpenImageFileAsync(string title)
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is not null)
            {
                var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    Title = title,
                    FileTypeFilter = [FilePickerFileTypes.ImageAll],
                    AllowMultiple = false
                });

                return files.Count >= 1 ? files[0] : null;
            }
            else return null;
        }

        #endregion
    }
}
