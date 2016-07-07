using System;
using GoddamnConsole.Controls;

namespace BindingSample
{
    public class MainWindow : GridWindow
    {
        private readonly TestObject _obj;

        public MainWindow()
        {
            DataContext = _obj = new TestObject();
        }

        public void ButtonClicked(object sender, EventArgs e)
        {
            _obj.NextStep();
        }
    }
}
