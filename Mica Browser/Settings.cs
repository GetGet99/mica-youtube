using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicaWindow;
namespace MicaBrowser;
public interface IMicaBrowserSettings
{
    Uri URI { get; set; }
    string PreferedTitle { get; set; }
    IMicaWindowSettings MicaWindowSettings { get; }
}
partial class MicaBrowser : IMicaBrowserSettings
{
    public new IMicaBrowserSettings Settings => this;

    public Uri URI { get => WebView2.Source; set => WebView2.Source = value; }

    public IMicaWindowSettings MicaWindowSettings => this;

    string _PreferedTitle = "Mica Browser";
    public string PreferedTitle
    {
        get => _PreferedTitle; set
        {
            _PreferedTitle = value;
            UpdateTitle();
        }
    }
}