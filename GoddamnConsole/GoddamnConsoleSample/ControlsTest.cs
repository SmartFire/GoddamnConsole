using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoddamnConsole.Controls;

namespace GoddamnConsoleSample
{
    public class ControlsTest : ContentWindow
    {
        private const string Lorem =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
            "sed do eiusmod tempor incididunt ut labore et dolore magn" +
            "a aliqua. Ut enim ad minim veniam, quis nostrud exercitat" +
            "ion ullamco laboris nisi ut aliquip ex ea commodo consequ" +
            "at. Duis aute irure dolor in reprehenderit in voluptate v" +
            "elit esse cillum dolore eu fugiat nulla pariatur. Excepte" +
            "ur sint occaecat cupidatat non proident, sunt in culpa qu" +
            "i officia deserunt mollit anim id est laborum.";

        private static readonly string LongLorem = string.Join("\n", Enumerable.Repeat(Lorem, 10));

        private int _clickCount;

        public int ClickCount
        {
            get { return _clickCount; }
            set { _clickCount = value; OnPropertyChanged(); }
        }

        public string TextViewText => "Read-only text!\n" + LongLorem;
        public string ScrollViewerText => LongLorem;
        private string _textBoxText = "asd";

        public string TextBoxText
        {
            get { return _textBoxText; }
            set
            {
                _textBoxText = value;
                OnPropertyChanged();
            }
        }

        public ControlsTest()
        {
            DataContext = this;
        }

        public void ButtonClicked(object sender, EventArgs e)
        {
            ClickCount++;
        }
    }
}
