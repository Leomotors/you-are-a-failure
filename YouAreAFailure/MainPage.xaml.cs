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
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace YouAreAFailure;

/// <summary>
/// Main page.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        // https://docs.microsoft.com/en-us/windows/apps/design/style/mica#title-bar-code-behind

        var titleBar = ApplicationView.GetForCurrentView().TitleBar;

        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        // Hide default title bar.
        var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        coreTitleBar.ExtendViewIntoTitleBar = true;
        UpdateTitleBarLayout(coreTitleBar);

        // Set XAML element as a draggable region.
        Window.Current.SetTitleBar(AppTitleBar);

#if DEBUG
        AppTitle.Text = "You are a Failure! (DEBUG Edition)";
#endif

        // Register a handler for when the size of the overlaid caption control changes.
        // For example, when the app moves to a screen with a different DPI.
        coreTitleBar.LayoutMetricsChanged += (sender, args) =>
        {
            UpdateTitleBarLayout(sender);
        };

        // Register a handler for when the title bar visibility changes.
        // For example, when the title bar is invoked in full screen mode.
        coreTitleBar.IsVisibleChanged += (sender, args) =>
        {
            AppTitleBar.Visibility = sender.IsVisible
                                        ? Visibility.Visible
                                        : Visibility.Collapsed;
        };

        // Register a handler for when the window changes focus
        Window.Current.Activated += Current_Activated;

        Failure.VideoList.OnListViewClickHandler = OnVideoListSelected;

        // Classes.AppState.OnStateChanged = OnWatchedUpdate;
        App.Current.State.OnStateChanged = async () =>
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, OnWatchedUpdate);

        // Register a handler for when a page want to browse to other page
        Failure.WelcomeFailure.Navigator = RemoteNavigation;
    }

    private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
    {
        // Update title bar control size as needed to account for system size changes.
        AppTitleBar.Height = coreTitleBar.Height;

        // Ensure the custom title bar does not overlap window caption controls
        Thickness currMargin = AppTitleBar.Margin;
        AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
    }

    // Update the TitleBar based on the inactive/active state of the app
    private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
    {
        SolidColorBrush defaultForegroundBrush =
            (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        SolidColorBrush inactiveForegroundBrush =
            (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

        if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
        {
            AppTitle.Foreground = inactiveForegroundBrush;
        }
        else
        {
            AppTitle.Foreground = defaultForegroundBrush;
        }
    }

    // Update the TitleBar content layout depending on NavigationView DisplayMode
    private void NavigationViewControl_DisplayModeChanged(MUXC.NavigationView sender, MUXC.NavigationViewDisplayModeChangedEventArgs args)
    {
        const int topIndent = 16;
        const int expandedIndent = 48;
        int minimalIndent = 104;

        // If the back button is not visible, reduce the TitleBar content indent.
        if (NavigationViewControl.IsBackButtonVisible.Equals(MUXC.NavigationViewBackButtonVisible.Collapsed))
        {
            minimalIndent = 48;
        }

        Thickness currMargin = AppTitleBar.Margin;

        // Set the TitleBar margin dependent on NavigationView display mode
        if (sender.PaneDisplayMode == MUXC.NavigationViewPaneDisplayMode.Top)
        {
            AppTitleBar.Margin = new Thickness(topIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
        }
        else if (sender.DisplayMode == MUXC.NavigationViewDisplayMode.Minimal)
        {
            AppTitleBar.Margin = new Thickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
        }
        else
        {
            AppTitleBar.Margin = new Thickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
        }
    }

    private void NavigationView_Loaded(object sender, RoutedEventArgs e)
    {
        NavigationViewControl.SelectedItem = Welcome;

        var todayDone = App.Current.State.IsAllWatched;

        foreach (var video in Classes.Steven.VideoList)
        {
            MotivationalVideo.MenuItems.Add(new MUXC.NavigationViewItem
            {
                Content = video.FileName + (todayDone ? " ✅" : ""),
            });
        }
    }

    private void OnWatchedUpdate()
    {
        for (int i = 0; i < Classes.Steven.VideoList.Length; i++)
        {
            var element = MotivationalVideo.MenuItems[i] as MUXC.NavigationViewItem;
            var video = Classes.Steven.VideoList[i];

            element.Content = video.FileName
                + (App.Current.State.Watched[i] ? " ✅" : "");
        }
    }

    private void NavigationView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            FailureFrame.Navigate(typeof(Failure.Settings));
            return;
        }

        var selected = args.SelectedItem as MUXC.NavigationViewItem;

        if (selected == Welcome)
        {
            FailureFrame.Navigate(typeof(Failure.WelcomeFailure));
        }
        else if (selected == MotivationalVideo)
        {
            FailureFrame.Navigate(typeof(Failure.VideoList));
        }
        else if (selected == Statistics)
        {
            FailureFrame.Navigate(typeof(Failure.Statistics));
        }
        else
        {
            if (selected.Content is not string videoName) return;

            Classes.Video selectedVideo =
                Classes.Steven.VideoList.Where(
                    vid => vid.FileName == videoName.Split(" ")[0]
                ).First();

            FailureFrame.Navigate(typeof(Failure.TortureChamber), selectedVideo);
        }
    }

    public void OnVideoListSelected(string selected)
    {
        MotivationalVideo.IsExpanded = true;

        NavigationViewControl.SelectedItem =
            MotivationalVideo.MenuItems
            .Where(
                menu =>
                    ((menu as MUXC.NavigationViewItem).Content as string)
                        .Split(" ")[0] == selected
            ).First();
    }

    public void RemoteNavigation(Classes.NavigationTarget target)
    {
        switch (target)
        {
            case Classes.NavigationTarget.MotivationalVideo:
                MotivationalVideo.IsSelected = true;
                break;

            case Classes.NavigationTarget.Statistics:
                Statistics.IsSelected = true;
                break;
        }
    }
}
