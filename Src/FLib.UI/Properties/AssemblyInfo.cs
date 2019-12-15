using System.Reflection;
using System.Runtime.InteropServices;
#if !WINRT
using System.Windows;
using System.Windows.Markup;
#endif

[assembly: AssemblyTitle("FLib.UI")]
[assembly: AssemblyProduct("FLib.UI")]
[assembly: AssemblyDescription("")]

[assembly: AssemblyVersion("0.4.6.0")]
[assembly: AssemblyFileVersion("0.4.6.0")]

#if !WINRT
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: XmlnsDefinition("http://schemas.fiolek.org/FLib", "FLib.Data", AssemblyName = "FLib")]
[assembly: XmlnsDefinition("http://schemas.fiolek.org/FLib", "FLib.Data.Converters", AssemblyName = "FLib")]

[assembly: XmlnsDefinition("http://schemas.fiolek.org/FLib", "FLib.UI.Controls")]
[assembly: XmlnsDefinition("http://schemas.fiolek.org/FLib", "FLib.UI.Data.Converters")]
[assembly: XmlnsDefinition("http://schemas.fiolek.org/FLib", "FLib.UI.Data.Validators")]
[assembly: XmlnsDefinition("http://schemas.fiolek.org/FLib", "FLib.UI.Utilities")]
[assembly: XmlnsDefinition("http://schemas.fiolek.org/FLib", "FLib.UI.XAML")]
#endif