using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a control, which can have more than one child
    /// </summary>
    [ContentProperty(nameof(Children))]
    public class ChildrenControl : ParentControl, IChildrenControl
    {
        [ContentWrapper(typeof(Control))]
        public class ChildrenCollection : IList<Control>, IList, IReadOnlyList<Control>
        {
            public ChildrenCollection(ChildrenControl parent)
            {
                _parent = parent;
            }

            private readonly ChildrenControl _parent;
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
                return _internal.Contains((Control)value);
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
                return value is Control ? _internal.IndexOf((Control)value) : -1;
            }

            public void Insert(int index, object value)
            {
                Insert(index, (Control)value);
            }

            public void Remove(object value)
            {
                Remove((Control)value);
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
            public object SyncRoot => ((ICollection)_internal).SyncRoot;
            public bool IsSynchronized => ((ICollection)_internal).IsSynchronized;
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
                set { this[index] = (Control)value; }
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

        public ChildrenControl()
        {
            Children = new ChildrenCollection(this);
        }
        
        /// <summary>
        /// Called when children collection is updated
        /// </summary>
        public virtual void OnChildrenUpdated() { }

        public override Rectangle MeasureBoundingBox(Control child) 
            => new Rectangle(0, 0, ActualWidth, ActualHeight);

        public override Point GetScrollOffset(Control child) => new Point(0, 0);

        public override bool IsChildVisible(Control child) => true;

        public ChildrenCollection Children { get; }
        IList<Control> IChildrenControl.Children => Children; 
        public virtual IList<Control> FocusableChildren => Children;
        public event EventHandler<ChildRemovedEventArgs> ChildRemoved;
    }
}
