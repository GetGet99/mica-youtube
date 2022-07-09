using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mica_YouTube
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static string BasePath =>
#if DEBUG
                            "../../..";
#else
                            ".";
#endif
        public App()
        {
            var MicaBrowser = new MicaBrowser.MicaBrowser
            {
                Settings =
                {
                    URI = new Uri("https://www.youtube.com/"),
                    PreferedTitle = "YouTube",
                    MicaWindowSettings =
                    {
                        ThemeColor = MicaWindow.BackdropTheme.Dark
                    }
                }
            };
            var WebView2 = MicaBrowser.WebView2;
            WebView2.CoreWebView2InitializationCompleted += delegate
            {
                var CoreWebView2 = WebView2.CoreWebView2;
                Debug.Assert(CoreWebView2 is not null);
                string OriginalUserAgent = CoreWebView2.Settings.UserAgent;
                string GoogleSignInUserAgent = OriginalUserAgent.Substring(0, OriginalUserAgent.IndexOf("Edg/"))
                .Replace("Mozilla/5.0", "Mozilla/4.0");
                CoreWebView2.NavigationStarting += (_, e) =>
                {
                    var isGoogleLogin = new Uri(e.Uri).Host.Contains("accounts.google.com");
                    CoreWebView2.Settings.UserAgent = isGoogleLogin ? GoogleSignInUserAgent : OriginalUserAgent;
                };
                CoreWebView2.NavigationCompleted += async delegate
                {
                    bool IsDarkTheme = await CoreWebView2.ExecuteScriptAsync("document.getElementsByTagName('html')[0].getAttribute('dark')")
                    is "\"true\"";
                    MicaBrowser.MicaWindowSettings.ThemeColor =
                        IsDarkTheme ? MicaWindow.BackdropTheme.Dark : MicaWindow.BackdropTheme.Light;
                    await CoreWebView2.ExecuteScriptAsync(@$"
(function () {{
    let style = document.createElement('style');
    style.innerHTML = `{File.ReadAllText($"{BasePath}/CSS.css")}`;
    document.head.appendChild(style);
}})()");
                    await CoreWebView2.ExecuteScriptAsync(
                        File.ReadAllText($"{BasePath}/js.js")
                    );
                    MicaBrowser.RefreshFrame();
                    
                };
                CoreWebView2.FrameCreated += (_, e) =>
                {
                    var frame = e.Frame;
                    frame.NavigationCompleted += delegate
                    {
                        _ = frame.ExecuteScriptAsync(@$"
(function () {{
    let style = document.createElement('style');
    style.innerHTML = `{File.ReadAllText($"{BasePath}/CSS.css")}`;
    document.head.appendChild(style);
}})()");
                        _ = frame.ExecuteScriptAsync(
                            File.ReadAllText($"{BasePath}/js.js")
                        );
                    };
                };
            };

            MainWindow = MicaBrowser;
            MainWindow.Show();
        }
    }
}
