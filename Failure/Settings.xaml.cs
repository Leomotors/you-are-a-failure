﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace you_are_a_failure.Failure;

/// <summary>
/// Settings Page, Also contains information about app &amp; credits.
/// </summary>
public sealed partial class Settings : Page
{
    // From Alert Box at System > Display > Custom Scaling
    private readonly SolidColorBrush BackgroundYellow = new(
            App.Current.IsLightTheme
                ? Color.FromArgb(255, 255, 244, 206)
                : Color.FromArgb(255, 67, 53, 25)
        );

    // From Alert Box at System > Display > Custom Scaling
    private readonly SolidColorBrush ForegroundYellow = new(
            App.Current.IsLightTheme
                ? Color.FromArgb(255, 157, 93, 0)
                : Color.FromArgb(255, 252, 225, 0)
        );

    public Settings()
    {
        this.InitializeComponent();

#if DEBUG
        bool IsDebug = true;
#else
        bool IsDebug = false;
#endif

        // Copied from rabbit-house-menu which is from microsoft/Xaml-Controls-Gallery
        var version = Windows.ApplicationModel.Package.Current.Id.Version;
        AppVersion.Text =
            $"Version: {version.Major}.{version.Minor} Build {version.Build}.{version.Revision}"
            + (IsDebug ? " (DEBUG)" : "");
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Array.ForEach(
            Classes.Steven.VideoList,
            video => CreditsPanel.Children.Add(
                new HyperlinkButton
                {
                    NavigateUri = new Uri(video.YoutubeLink),
                    Content = "Compilation Video: " + video.FileName,
                }
            )
        );

        base.OnNavigatedTo(e);
    }

    // Copied from rabbit-house-menu
    private async void ViewLicense_Click(object sender, RoutedEventArgs e)
    {
        string License = await FileIO.ReadTextAsync(
                await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri("ms-appx:///LICENSE")
                )
            );

        const string tmpToken = "__@$";

        var tokens = License.Split("\n");

        var dialog = new ContentDialog
        {
            Title = tokens[0],
            // Remove "\n" but not "\n\n"
            Content = string.Join("\n", tokens.Skip(2))
                        .Replace("\n\n", tmpToken)
                        .Replace("\n", " ")
                        .Replace(tmpToken, "\n\n"),
            CloseButtonText = "Close",
        };

        await dialog.ShowAsync();
    }

    private void ThemeSelector_Loaded(object sender, RoutedEventArgs e)
    {
        (sender as MUXC.RadioButtons).SelectedIndex = App.Current.CurrentTheme;
    }

    private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var buttons = sender as MUXC.RadioButtons;

        ApplicationData.Current.LocalSettings.Values["themeSetting"] =
            buttons.SelectedIndex;

        var showAlert = App.Current.CurrentTheme == buttons.SelectedIndex;

        ThemeChangeAlert.Visibility =
            showAlert ? Visibility.Collapsed : Visibility.Visible;

        ThemeChangeAlertBorder.Margin =
            showAlert ? new Thickness(0) : new Thickness(0, 10, 0, 10);
    }

    private async void OpenSave_Click(object sender, RoutedEventArgs e)
    {
        var folder = ApplicationData.Current.RoamingFolder;
        await Launcher.LaunchFolderAsync(folder);
    }

    private async void ResetData_Click(object sender, RoutedEventArgs e)
    {
        App.Current.State.WatchedDate = new List<DateTime>();
        await App.Current.State.SaveDatabase();
    }
}