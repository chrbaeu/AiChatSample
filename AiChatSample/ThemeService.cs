#pragma warning disable WPF0001

using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;

namespace AiChatSample;

public record ThemeChangedEvent(bool DarkModeEnabled);

public sealed class ThemeService(IMessenger messenger)
{
    [Description("Enables or disables the dark mode.")]
    public void SetDarkMode([Description("True to enable or false to disable the dark mode.")] bool enable)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.ThemeMode = enable ? ThemeMode.Dark : ThemeMode.Light;
            messenger.Send(new ThemeChangedEvent(enable));
        });
    }

    [Description("Checks if the the dark mode is enabled.")]
    public bool IsDarkMode()
    {
        return Application.Current.ThemeMode == ThemeMode.Dark;
    }
}

#pragma warning restore WPF0001