using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using AvaloniaApplication.Models;
using AvaloniaApplication.Services;
using AvaloniaApplication.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaloniaApplication.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {
        #region Fields
        private FilesService _filesService;

        private ClientService _clientService;

        private AvaloniaList<ImageItem>? _imageItems;

        private ImageItem? _selectedItem;

        private bool _isAddButtonEnabled = true;

        private bool _isChangeButtonEnabled = false;

        private bool _isDeleteButtonEnabled = false;
        #endregion

        #region Properties
        /// <summary>Список <see cref="ImageItem"/>, отображаемый в UI.</summary>
        public AvaloniaList<ImageItem>? ImageItems
        {
            get => _imageItems;
            set => this.RaiseAndSetIfChanged(ref _imageItems, value);
        }

        /// <summary>Выбранный <see cref="ImageItem"/> элемент списка.</summary>
        public ImageItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedItem, value);
                if (_selectedItem != null)
                {
                    IsChangeButtonEnabled = true;
                    IsDeleteButtonEnabled = true;
                }
                else
                {
                    IsChangeButtonEnabled = false;
                    IsDeleteButtonEnabled = false;
                }
            }
        }

        /// <summary>Флаг доступа кнопки "ДОБАВИТЬ"</summary>
        public bool IsAddButtonEnabled
        {
            get => _isAddButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _isAddButtonEnabled, value);
        }

        /// <summary>Флаг доступа кнопки "ИЗМЕНИТЬ".</summary>
        public bool IsChangeButtonEnabled
        {
            get => _isChangeButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _isChangeButtonEnabled, value);
        }

        /// <summary>Флаг доступа кнопки "УДАЛИТЬ".</summary>
        public bool IsDeleteButtonEnabled
        {
            get => _isDeleteButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _isDeleteButtonEnabled, value);
        }
        #endregion

        #region Commands
        /// <summary>Команда получения изображений.</summary>
        public ReactiveCommand<Unit, Unit> GetCommand { get; }

        /// <summary>Команда добавления изображения.</summary>
        public ReactiveCommand<Unit, Unit> AddCommand { get; }

        /// <summary>Команда изменения изображения.</summary>
        public ReactiveCommand<Unit, Unit> ChangeCommand { get; }

        /// <summary>Команда удаления изображения.</summary>
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Инициализирует экземпляр <see cref="MainWindowViewModel"/>.
        /// </summary>
        /// <param name="filesService">Сервис для работы с файлами.</param>
        /// <param name="clientService">Сервис для взаимодействия с Web-API</param>
        public MainWindowViewModel(FilesService filesService, ClientService clientService)
        {
            _filesService = filesService;
            _clientService = clientService;

            var list = _clientService.GetAsync();

            AddCommand = ReactiveCommand.CreateFromTask(AddAsync);
            ChangeCommand = ReactiveCommand.CreateFromTask(ChangeAsync);
            DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync);
            GetCommand = ReactiveCommand.CreateFromTask(GetAsync);

            GetCommand.Execute();
        }
        #endregion

        #region Private Methods
        /// <summary>Получение изображений.</summary>
        private async Task GetAsync()
        {
            try
            {
                var dictionary = await _clientService.GetAsync();
                ImageItems = GetListOfImageItems(dictionary);
            }
            catch (Exception ex) { OnConnectionError(ex); }
        }

        /// <summary>Добавление изображения.</summary>
        private async Task AddAsync()
        {
            IStorageFile? imageFile = await _filesService.OpenImageFileAsync("Добавить изображение");
            try
            {
                if (imageFile != null)
                {
                    if (!(ImageItems != null ? ImageItems.Any(x => x.Name == imageFile.Name) : false))
                    {
                        await AddInDataBaseAsync(imageFile);
                        await AddInUIAsync(imageFile);
                    }
                    else ShowMessageDialog("Изображение с таким именем уже существует.");
                }
            }
            catch (Exception ex) { OnConnectionError(ex); }
        }

        /// <summary>Изменение изображения.</summary>
        private async Task ChangeAsync()
        {

            IStorageFile? imageFile = await _filesService.OpenImageFileAsync("Изменить изображение");
            try
            {
                if (imageFile != null)
                {
                    if (!(ImageItems != null ? ImageItems.Any(x => x.Name == imageFile.Name) : false))
                    {
                        await UpdateInDataBaseAsync(imageFile);
                        await ChangeInUIAsync(imageFile);
                    }
                    else ShowMessageDialog("Изображение с таким именем уже существует.");
                }
            }
            catch (Exception ex) { OnConnectionError(ex); }
        }

        /// <summary>Удаление изображения.</summary>
        private async Task DeleteAsync()
        {
            try
            {
                await DeleteInDataBaseAsync();
                DeleteInUI();

                ShowMessageDialog("Изображение удалено.");
            }
            catch (Exception ex) { OnConnectionError(ex); }
        }

        /// <summary>Добавление изображения в лист UI.</summary>
        /// <param name="imageFile">Файл с изобржением.</param>
        private async Task AddInUIAsync(IStorageFile imageFile)
        {
            if (ImageItems != null) ImageItems.Add(await CreateImageItemAsync(imageFile));
            else ImageItems = [await CreateImageItemAsync(imageFile)];
        }

        /// <summary>Изменение изображение в UI.</summary>
        /// <param name="imageFile">Файл с изобржением.</param>
        private async Task ChangeInUIAsync(IStorageFile imageFile)
        {
            ImageItem imageItem = await CreateImageItemAsync(imageFile);
            var list = ImageItems?.Select(x => x.Name == SelectedItem?.Name ? imageItem : x).ToList();
            if (list != null)
            {
                ImageItems = new(list);
                SelectedItem = imageItem;
            }
            else ClearUI();
        }

        /// <summary>Удаление изображение из листа UI.</summary>
        private void DeleteInUI()
        {
            var list = ImageItems?.Where(x => x.Name != SelectedItem?.Name).ToList();
            if (list != null) ImageItems = new(list);
            else ImageItems = null;
            SelectedItem = null;
        }

        /// <summary>Отправка изображения на Web-API для последующего сохранения в базе данных.</summary>
        /// <param name="imageFile">Файл с изображением.</param>
        private async Task AddInDataBaseAsync(IStorageFile imageFile)
        {
            await _clientService.SendAsync(imageFile.Path);
        }

        /// <summary>Отправка изобржанеия на Web-API для последующей замены изображения.</summary>
        /// <param name="imageFile">Файл с изобржаением.</param>

        private async Task UpdateInDataBaseAsync(IStorageFile imageFile)
        {
            await _clientService.UpdateImageAsync(GetIndexOfSelectedImage(), imageFile.Path);
        }

        /// <summary>Удаление изображения из базы данных.</summary>
        private async Task DeleteInDataBaseAsync()
        {
            await _clientService.DeleteImageAsync(GetIndexOfSelectedImage());
        }

        /// <summary>Получение <see cref="AvaloniaList{T}"/>, содержащий <see cref="ImageItem"/>.</summary>
        /// <param name="dictionary">Словарь, хранящий элементы: key - имя изображения, value - массив байтов изображения.</param>
        /// <returns><see cref="AvaloniaList{T}"/>, содержащий <see cref="ImageItem"/>.</returns>
        private AvaloniaList<ImageItem>? GetListOfImageItems(Dictionary<string, byte[]>? dictionary)
        {
            if (dictionary != null)
            {
                AvaloniaList<ImageItem> list = new();
                foreach (var item in dictionary)
                {
                    var imageItem = CreateImageItem(item.Key, item.Value);
                    list.Add(imageItem);
                }

                return list;
            }
            else return null;
        }

        private async Task<ImageItem> CreateImageItemAsync(IStorageFile file) =>
            new ImageItem(new Bitmap(await file.OpenReadAsync()), file.Name);

        private ImageItem CreateImageItem(string name, byte[] bytes) =>
            new ImageItem(new Bitmap(new MemoryStream(bytes)), name);

        private int GetIndexOfSelectedImage() => ImageItems.IndexOf(SelectedItem);

        /// <summary>Вывод диалогового окна с сообщением.</summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="width">Ширина окна.</param>
        /// <param name="height">Высота окна.</param>
        private void ShowMessageDialog(string message, int width = 200, int height = 100)
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is not null)
            {
                var messageWindow = new MessageWindow();
                MessageWindowViewModel messageWindowVM = new(message, messageWindow)
                {
                    Width = width,
                    Height = height,
                };
                messageWindow.DataContext = messageWindowVM;
                messageWindow.ShowDialog(desktop.MainWindow);
            }
        }

        private void OnConnectionError(Exception ex)
        {
            ShowMessageDialog(ex.Message, 800);

            IsAddButtonEnabled = false;
            IsChangeButtonEnabled = false;
            IsDeleteButtonEnabled = false;

            ClearUI();
        }

        private void ClearUI()
        {
            ImageItems = null;
            SelectedItem = null;
        }
        #endregion
    }
}

