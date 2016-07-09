using GoddamnConsole.Xaml;
using Console = GoddamnConsole.Console;

namespace GoddamnConsoleSample
{
    internal class Program
    {
        private static void Main()
        {
            Console.Windows.Add(XamlServices.LoadControl<GridWindowTest>());
            Console.Windows.Add(XamlServices.LoadControl<ControlsTest>());
            Console.Windows.Add(XamlServices.LoadControl<BindingTest>());
            Console.Start();
        }
    }
}
