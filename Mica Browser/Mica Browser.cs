extern alias WV2;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text;
using MicaWindow;
using WV2::Microsoft.Web.WebView2.Wpf;
using SysDrawColor = System.Drawing.Color;
using Constants = MicaWindow.Environment.Constants;
using Button = MicaWindow.Controls.Button;
using static MicaWindow.Environment.Extension;
using System.Windows.Media;
using System.ComponentModel;
using Keys = System.Windows.Forms.Keys;
using Control = System.Windows.Forms.Control;
#if WINDOWS10_0_17763_0_OR_GREATER
using Windows.Graphics;
#endif

namespace MicaBrowser;

public partial class MicaBrowser : MicaWindow.MicaWindow
{
    public WebView2 WebView2 { get; }

    readonly TextBlock WebsiteTitle, TitleLabel;
    
    void UpdateTitle()
    {
        var PreferedTitle = this.PreferedTitle;
        TitleLabel.Text = PreferedTitle;
        if (WebView2.CoreWebView2?.DocumentTitle is string str)
        {
            bool shorten = PreferedTitle == str;
            WebsiteTitle.Text = shorten ? "" : str;
            Title = shorten ? PreferedTitle : ($"{PreferedTitle} - {str}");
        }
    }
    public MicaBrowser()
    {
        Resources["ButtonBackgroundMouseOver"] = new SolidColorBrush(Colors.Red);
        Resources["IconFont"] = new FontFamily(Constants.OSVersion.IsWindows11OrAbove() ? "Segoe Fluent Icons" : "Segoe MDL2 Assets");
        static Button CreateButton(string Icon)
            => new Button()
            {
                //Style = MicaWindow.Controls.Styles.ButtonStyle,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Width = 40,
                Height = 40,
                TextBlock =
                {
                    Text = Icon
                }
            }.Edit(x =>
            {

                x.SetResourceReference(FontFamilyProperty, "IconFont");
                x.SetResourceReference(ForegroundProperty, "Foreground");
                MakeHitTestVisibleInTitleBar(x);
            });

        Customization.TitleBarElement = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition(),
            },
            Children =
            {
                new TextBlock
                {
                    Padding = new Thickness(10),
                    Height = 40,
                    Text = Title,
                    VerticalAlignment = VerticalAlignment.Center,
                }
                .Edit(x => x.SetResourceReference(ForegroundProperty, "Foreground"))
                .Assign(out TitleLabel)
                .Edit(x => Grid.SetColumn(x, 0)),
                CreateButton("\uE112").Assign(out var Back)
                .Edit(x => Grid.SetColumn(x, 1)),
                CreateButton("\uE111").Assign(out var Forward)
                .Edit(x => Grid.SetColumn(x, 2)),
                CreateButton("\uE149").Assign(out var Reload)
                .Edit(x => Grid.SetColumn(x, 3)),
                CreateButton("\uE115").Assign(out var Settings).Edit(x => x.Visibility = Visibility.Collapsed)
                .Edit(x => Grid.SetColumn(x, 4)),
                new TextBlock
                {
                    Padding = new Thickness(10),
                    Height = 40,
                    Text = "",
                    VerticalAlignment = VerticalAlignment.Center
                }
                .Edit(x => x.SetResourceReference(ForegroundProperty, "Foreground"))
                .Assign(out WebsiteTitle)
                .Edit(x => Grid.SetColumn(x, 5)),
            }
        };
        
#if WINDOWS10_0_17763_0_OR_GREATER
        CalculateDragRectangles -= DefaultCalculateDragRectangles;
        CalculateDragRectangles += (ref List<RectInt32> l) =>
        {
            static int Round(double value) => (int)Math.Round(value);
            var Title1Location = TitleLabel.TransformToVisual(this).Transform(new Point());
            l.Add(new RectInt32
            {
                Y = 0,
                Height = Customization.TitleBarHeight,
                X = Round(Title1Location.X),
                Width = Round(TitleLabel.ActualWidth),
            });
            var Title2Location = WebsiteTitle.TransformToVisual(this).Transform(new Point());
            l.Add(new RectInt32
            {
                Y = 0,
                Height = Customization.TitleBarHeight,
                X = Round(Title2Location.X),
                Width = Round(WebsiteTitle.ActualWidth),
            });
        };
#endif
        
        Customization.MainContent = WebView2 = new WebView2
        {
            DefaultBackgroundColor = SysDrawColor.Transparent,
            Source = new Uri("about:blank")
        };


        Back.IsEnabled = false;
        Back.Click += (_, _) => WebView2.GoBack();
        Forward.IsEnabled = false;
        Forward.Click += (_, _) => WebView2.GoForward();
        Reload.Click += (_, _) => WebView2.Reload();
        //Settings.Click += (_, _) => 

        WebView2.CoreWebView2InitializationCompleted += delegate
        {
            var CoreWebView2 = WebView2.CoreWebView2;
            CoreWebView2.NewWindowRequested += (_, e) =>
            {
                if (Control.ModifierKeys.HasFlag(Keys.Shift)) return;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri,
                    UseShellExecute = true
                });
                e.Handled = true;
            };
            CoreWebView2.DocumentTitleChanged += (_, _) => UpdateTitle();
            CoreWebView2.NavigationCompleted += (_, _) => UpdateTitle();
            CoreWebView2.HistoryChanged += delegate
            {
                Back.IsEnabled = WebView2.CanGoBack;
                Forward.IsEnabled = WebView2.CanGoForward;
            };
        };
        UpdateTitle();
    }
}
static class Extension
{
    public static T Assign<T>(this T value, out T variable)
    {
        variable = value;
        return value;
    }
    public static T Edit<T>(this T value, Action<T> action)
    {
        action?.Invoke(value);
        return value;
    }
}