using System.IO;
using System.Resources;
using System.Xaml;
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
            {
                var reader = new XamlXmlReader(stream, new XamlXmlReaderSettings
                {
                    LocalAssembly = assembly
                });
                using (stream) return (T) System.Xaml.XamlServices.Load(reader);
            }
            throw new MissingManifestResourceException();
        }

        public static T ParseXaml<T>(string xaml) where T : Control
        {
            var assembly = typeof(T).Assembly;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(xaml);
                writer.Flush();
                stream.Position = 0;
                var reader = new XamlXmlReader(stream, new XamlXmlReaderSettings
                {
                    LocalAssembly = assembly
                });
                using (stream) return (T) System.Xaml.XamlServices.Load(reader);
            }
        }
    }
}
