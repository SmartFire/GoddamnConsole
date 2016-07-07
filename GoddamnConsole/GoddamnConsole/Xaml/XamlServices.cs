using System.Resources;
using GoddamnConsole.Controls;

namespace GoddamnConsole.Xaml
{
    public static class XamlServices
    {
        public static T LoadControl<T>() where T : Control
        {
            var tt = typeof (T);
            var assembly = tt.Assembly;
            var stream = assembly.GetManifestResourceStream($"{tt.Assembly.GetName().Name}.{tt.Name}.xaml");
            if (stream != null)
                using (stream) return (T) System.Xaml.XamlServices.Load(stream);
            throw new MissingManifestResourceException();
        }

        public static T ParseXaml<T>(string xaml) where T : Control
        {
            return (T) System.Xaml.XamlServices.Parse(xaml);
        }
    }
}
