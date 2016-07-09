using System;
using System.Linq;
using System.Windows.Markup;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a control, which can have only one child
    /// </summary>
    [ContentProperty(nameof(Content))]
    public abstract class ContentControl : ParentControl, IContentControl
    {
        private Control _content;
        
        public Control Content
        {
            get { return _content; }
            set
            {
                if (_content != null)
                {
                    var pc = _content;
                    _content = null;
                    ContentDetached?.Invoke(this, new ChildRemovedEventArgs(pc));
                }
                if (value?.Name != null &&
                    AllControls.Any(x => x.Name == value.Name))
                    throw new Exception("Control with exact name already exists");
                if (value == null || value.Parent == this) _content = value;
                else value.Parent = this;
                OnPropertyChanged();
            }
        }

        public event EventHandler<ChildRemovedEventArgs> ContentDetached;

        public override Rectangle MeasureBoundingBox(Control child) 
            => new Rectangle(0, 0, ActualWidth, ActualHeight);

        public override Point GetScrollOffset(Control child) => new Point(0, 0);
                
        public override bool IsChildVisible(Control child) => true;

        protected override int MaxWidthByContent => Content?.MeasureWidth(ControlSizeType.MaxByContent) ?? 0;
        protected override int MaxHeightByContent => Content?.MeasureHeight(ControlSizeType.MaxByContent) ?? 0;
    }
}
