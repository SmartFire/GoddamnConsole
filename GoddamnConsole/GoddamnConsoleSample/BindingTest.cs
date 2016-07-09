using System;
using GoddamnConsole.Controls;

namespace GoddamnConsoleSample
{
    public class BindingTest : GridWindow
    {
        private readonly TestObject _obj;

        public BindingTest()
        {
            DataContext = _obj = new TestObject();
        }

        public void ButtonClicked(object sender, EventArgs e)
        {
            _obj.NextStep();
        }
    }
}
