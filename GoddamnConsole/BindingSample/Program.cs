using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GoddamnConsole;
using GoddamnConsole.Controls;
using GoddamnConsole.DataBinding;
using GoddamnConsole.Drawing;
using GoddamnConsole.Xaml;

namespace BindingSample
{
    class Program
    {
        static void Main()
        {
            Console.Windows.Add(XamlServices.LoadControl<MainWindow>());
            Console.Start();
        }
    }
}