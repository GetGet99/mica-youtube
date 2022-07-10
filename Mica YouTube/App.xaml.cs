extern alias WV2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Mica_YouTube
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static UISettings UISettings = new();
        static string BasePath =>
#if DEBUG
                            "../../..";
#else
                            ".";
#endif
        //static IEnumerable<string> AdblockURIs = File.ReadAllLines("AdblockerText.txt");
        public App()
        {
            var MicaBrowser = new MicaBrowser.MicaBrowser
            {
                Settings =
                {
                    AutoFullScreen = true,
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
                    var isGoogleLogin = new Uri(e.Uri).Host.Contains("accounts.google.com") || e.Uri.Contains("microsoftedge.microsoft.com/addons");
                    CoreWebView2.Settings.UserAgent = isGoogleLogin ? GoogleSignInUserAgent : OriginalUserAgent;
                };
                CoreWebView2.NavigationStarting += async delegate
                {
                    await CoreWebView2.ExecuteScriptAsync("delete window.chrome.webview");
                };
                CoreWebView2.NavigationCompleted += async delegate
                {
                    if (!CoreWebView2.Source.Contains("youtube")) return;
                    
                    bool IsDarkTheme = await CoreWebView2.ExecuteScriptAsync("document.getElementsByTagName('html')[0].getAttribute('dark')")
                    is "\"true\"";
                    Color c;
                    if (IsDarkTheme)
                        c = UISettings.GetColorValue(UIColorType.AccentLight1);
                    else
                        c = UISettings.GetColorValue(UIColorType.AccentDark1);
                    MicaBrowser.MicaWindowSettings.ThemeColor =
                        IsDarkTheme ? MicaWindow.BackdropTheme.Dark : MicaWindow.BackdropTheme.Light;
                    await CoreWebView2.ExecuteScriptAsync(@$"
(function () {{
    let style = document.createElement('style');
    style.innerHTML = `
:root {{
    --accent: rgba({c.R}, {c.G}, {c.B}, {c.A / 255d});
}}

{File.ReadAllText($"{BasePath}/CSS.css")}`;
    document.head.appendChild(style);
}})()");
                    await CoreWebView2.ExecuteScriptAsync(
                        File.ReadAllText($"{BasePath}/js.js")
                    );
                    MicaBrowser.RefreshFrame();
                    
                };
                CoreWebView2.FrameCreated += (_, e) =>
                {
                    if (!CoreWebView2.Source.Contains("youtube")) return;
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
                
                //foreach (var uri in AdblockURIs)
                //    if (!uri.StartsWith('!'))
                //        CoreWebView2.AddWebResourceRequestedFilter(uri,
                //            WV2::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.All);

                //CoreWebView2.WebResourceRequested += (_, e) =>
                //{
                //    var uri = e.Request.Uri;
                //    if (AdblockURIs.Any(x =>
                //    {
                //        if (x.StartsWith('!')) return false;
                //        return uri.Contains(x);
                //    }))
                //    {

                //    }
                //};
            };

            MainWindow = MicaBrowser;
            MainWindow.Show();
        }
    }
}
