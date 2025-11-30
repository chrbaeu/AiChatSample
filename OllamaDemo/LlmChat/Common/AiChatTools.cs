using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;

namespace OllamaDemo.LlmChat.Common;

public record AiChatToolThemeChangeEvent(bool DarkModeEnabled);

public class AiChatTools(IMessenger messenger)
{
    [Description("Returns the current date/time in the yyyy-MM-dd HH:mm:ss format (local time).")]
    public string GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

#pragma warning disable WPF0001

    [Description("Enables or disables the dark mode.")]
    public void SetDarkMode([Description("True to enable or false to disable the dark mode.")] bool enable)
    {
        messenger.Send(new AiChatToolThemeChangeEvent(enable));
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.ThemeMode = enable ? ThemeMode.Dark : ThemeMode.Light;
        });
    }

    [Description("Checks if the the dark mode is enabled.")]
    public bool IsDarkMode()
    {
        return Application.Current.ThemeMode == ThemeMode.Dark;
    }

#pragma warning restore WPF0001

}
