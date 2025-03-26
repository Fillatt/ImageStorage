using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;

namespace AvaloniaApplication.ViewModels;

public class MessageWindowViewModel
{
    #region Fields
    private Window _targetWindow;
    #endregion

    #region Properties
    /// <summary>Сообщение, отображаемое в UI.</summary>
    public string Message { get; set; }

    /// <summary>Ширина окна.</summary>
    public int Width { get; set; } = 200;

    /// <summary>Высота окна.</summary>
    public int Height { get; set; } = 100;
    #endregion

    #region Commands
    /// <summary>Команда закрытия окна.</summary>
    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Инициализирует экземпляр <see cref="MessageWindowViewModel"/>.
    /// </summary>
    /// <param name="message">Сообщение, отображаемое в UI.</param>
    /// <param name="targetWindow">Экземпляр <see cref="Window"/>, которому будет принадлежать экземпляр <see cref="MessageWindowViewModel"/>.</param>
    public MessageWindowViewModel(string message, Window targetWindow)
    {
        Message = message;
        _targetWindow = targetWindow;

        CloseWindowCommand = ReactiveCommand.Create(CloseWindow);
    }
    #endregion

    #region Private Methods
    private void CloseWindow() => _targetWindow.Close();
    #endregion
}
