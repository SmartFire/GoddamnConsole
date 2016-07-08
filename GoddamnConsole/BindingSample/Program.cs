using System.Collections;
using System.Collections.Generic;
using GoddamnConsole;
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