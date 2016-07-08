using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    [ContentProperty(nameof(Children))]
    public class GridWindow : WindowBase, IChildrenControl
    {
        public GridWindow()
        {
            Children = new ChildrenCollection(this);
        }

        private bool _drawBorders;

        public bool DrawBorders
        {
            get { return _drawBorders; }
            set { _drawBorders = value; OnPropertyChanged(); }
        }

        public override Size BoundingBoxReduction
            => DrawBorders ? new Size(Math.Max(1, ColumnDefinitions.Count) + 1, Math.Max(1, RowDefinitions.Count) + 1) : new Size(2, 2);

        public override int MaxHeight =>
            Children.GroupBy(Grid.GetColumn)
                    .Max(x => x.Sum(y => y.ActualHeight)) + BoundingBoxReduction.Height;

        public override int MaxWidth =>
            Children.GroupBy(Grid.GetRow)
                    .Max(x => x.Sum(y => y.ActualWidth)) + BoundingBoxReduction.Width;

        /// <summary>
        /// Returns a collection of row definitions
        /// </summary>
        public GridSizeList RowDefinitions { get; } = new GridSizeList();
        /// <summary>
        /// Returns a collection of column definitions
        /// </summary>
        public GridSizeList ColumnDefinitions { get; } = new GridSizeList();

        public override Rectangle MeasureBoundingBox(Control child)
        {
            return GridInternal.MeasureBoundingBox(
                Children,
                child,
                DrawBorders,
                ActualWidth,
                ActualHeight,
                ColumnDefinitions,
                RowDefinitions,
                1,
                1);
        }

        public override Point GetScrollOffset(Control child)
        {
            return new Point(0, 0);
        }

        public override bool IsChildVisible(Control child)
        {
            return true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var style = Console.FocusedWindow == this ? FrameStyle.Double : FrameStyle.Single;
            var truncated =
                Title == null
                    ? string.Empty
                    : Title.Length + 2 > ActualWidth - 4
                          ? ActualWidth < 9
                                ? string.Empty
                                : $" {Title.Remove(ActualWidth - 9)}... "
                          : ActualWidth < 9
                                ? string.Empty
                                : $" {Title} ";
            if (!DrawBorders)
            {
                dc.DrawFrame(new Rectangle(0, 0, ActualWidth, ActualHeight),
                             new FrameOptions {Background = Background, Foreground = Foreground, Style = style});
                dc.DrawText(new Point(2, 0), truncated, new TextOptions
                {
                    Background = Background,
                    Foreground = Foreground
                });
                return;
            }
            GridInternal.Render(
                dc, Background, Foreground, style, Children, ColumnDefinitions, RowDefinitions,
                ActualWidth, ActualHeight, 1, 1);
            dc.DrawText(new Point(2, 0), truncated, new TextOptions
            {
                Background = Background,
                Foreground = Foreground
            });
        }

        IList<Control> IChildrenControl.Children => Children;
        public ChildrenCollection Children { get; }
        public IList<Control> FocusableChildren => Children;
        public event EventHandler<ChildRemovedEventArgs> ChildRemoved;
        
        [ContentWrapper(typeof(Control))]
        public class ChildrenCollection : IList<Control>, IList, IReadOnlyList<Control>
        {
            public ChildrenCollection(GridWindow parent)
            {
                _parent = parent;
            }

            private readonly GridWindow _parent;
            private readonly List<Control> _internal = new List<Control>();

            public IEnumerator<Control> GetEnumerator()
            {
                return _internal.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _internal.GetEnumerator();
            }

            public void Add(Control item)
            {
                if (item.Name != null &&
                    _parent.AllControls.Any(x => x.Name == item.Name))
                    throw new Exception("Control with exact name already exists");
                if (item.Parent != _parent)
                {
                    item.Parent = _parent;
                    return;
                }
                _internal.Add(item);
                _parent.Invalidate();
            }

            public int Add(object value)
            {
                Add((Control)value);
                return Count - 1;
            }

            public bool Contains(object value)
            {
                return _internal.Contains((Control) value);
            }

            public void Clear()
            {
                var copy = _internal.ToArray();
                _internal.Clear();
                foreach (var item in copy)
                {
                    _parent.ChildRemoved?.Invoke(_parent, new ChildRemovedEventArgs(item));
                }
                _parent.Invalidate();
            }

            public int IndexOf(object value)
            {
                return value is Control ? _internal.IndexOf((Control) value) : -1;
            }

            public void Insert(int index, object value)
            {
                Insert(index, (Control) value);
            }

            public void Remove(object value)
            {
                Remove((Control) value);
            }

            public bool Contains(Control item)
            {
                return _internal.Contains(item);
            }

            public void CopyTo(Control[] array, int arrayIndex)
            {
                _internal.CopyTo(array, arrayIndex);
            }

            public bool Remove(Control item)
            {
                if (!Contains(item)) return false;
                _internal.Remove(item);
                _parent.ChildRemoved?.Invoke(_parent, new ChildRemovedEventArgs(item));
                _parent.Invalidate();
                return true;
            }

            public void CopyTo(Array array, int index)
            {
                ((IList)_internal).CopyTo(array, index);
            }

            public int Count => _internal.Count;
            public object SyncRoot => ((ICollection) _internal).SyncRoot;
            public bool IsSynchronized => ((ICollection) _internal).IsSynchronized;
            public bool IsReadOnly => false;
            public bool IsFixedSize => false;

            public int IndexOf(Control item)
            {
                return _internal.IndexOf(item);
            }

            public void Insert(int index, Control item)
            {
                if (item.Name != null &&
                    _parent.AllControls.Any(x => x.Name == item.Name))
                    throw new Exception("Control with exact name already exists");
                if (item.Parent != _parent)
                {
                    item.Parent = _parent;
                    return;
                }
                _internal.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _internal.RemoveAt(index);
            }

            object IList.this[int index]
            {
                get { return _internal[index]; }
                set { this[index] = (Control) value; }
            }

            public Control this[int index]
            {
                get { return _internal[index]; }
                set
                {
                    var pv = _internal[index];
                    _parent.ChildRemoved?.Invoke(_parent, new ChildRemovedEventArgs(_internal[index]));
                    _parent.Invalidate();
                    if (value.Name != null &&
                        _parent.AllControls.Any(x => x.Name == value.Name))
                        throw new Exception("Control with exact name already exists");
                    if (value.Parent != _parent)
                    {
                        value.Parent = _parent;
                        _internal.Remove(pv);
                        return;
                    }
                    _internal[index] = value;
                }
            }
        }
    }

    [ContentWrapper(typeof(GridSize))]
    public class GridSizeList : List<GridSize> { }
}
