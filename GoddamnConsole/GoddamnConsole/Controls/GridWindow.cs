using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xaml;
using GoddamnConsole.Drawing;
using static GoddamnConsole.Drawing.FrameOptions;

namespace GoddamnConsole.Controls
{
    public class GridWindow : WindowBase, IChildrenControl
    {
        // + Attached properties

        private static readonly AttachableMemberIdentifier RowProperty = new AttachableMemberIdentifier(typeof(GridWindow), "Row");

        public static int GetRow(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(RowProperty);

        public static void SetRow(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(RowProperty, value);
        }

        private static readonly AttachableMemberIdentifier RowSpanProperty = new AttachableMemberIdentifier(typeof(GridWindow), "RowSpan");

        public static int GetRowSpan(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(RowSpanProperty);

        public static void SetRowSpan(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(RowSpanProperty, value);
        }

        private static readonly AttachableMemberIdentifier ColumnProperty = new AttachableMemberIdentifier(typeof(GridWindow), "Column");

        public static int GetColumn(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(ColumnProperty);

        public static void SetColumn(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(ColumnProperty, value);
        }

        private static readonly AttachableMemberIdentifier ColumnSpanProperty = new AttachableMemberIdentifier(typeof(GridWindow), "ColumnSpan");

        public static int GetColumnSpan(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(ColumnSpanProperty);

        public static void SetColumnSpan(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(ColumnSpanProperty, value);
        }

        // - Attached properties

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
            Children.GroupBy(GetColumn)
                    .Max(x => x.Sum(y => y.ActualHeight)) + BoundingBoxReduction.Height;

        public override int MaxWidth =>
            Children.GroupBy(GetRow)
                    .Max(x => x.Sum(y => y.ActualWidth)) + BoundingBoxReduction.Width;

        /// <summary>
        /// Returns a collection of row definitions
        /// </summary>
        public IList<GridSize> RowDefinitions { get; } = new List<GridSize>();
        /// <summary>
        /// Returns a collection of column definitions
        /// </summary>
        public IList<GridSize> ColumnDefinitions { get; } = new List<GridSize>();

        private int[] MeasureSizes(bool measureColumns)
        {
            var boxSize = measureColumns ? ActualWidth : ActualHeight;
            if (!DrawBorders) boxSize -= 2;
            var definitions = measureColumns ? ColumnDefinitions : RowDefinitions;
            if (definitions.Count == 0)
                definitions = new[]
                {
                    new GridSize(GridUnitType.Auto, 0)
                };
            var sizes = new long[definitions.Count];
            for (var i = 0; i < definitions.Count; i++)
            {
                switch (definitions[i].UnitType)
                {
                    case GridUnitType.Auto:
                        var children =
                            Children.Where(x => measureColumns ? GetColumn(x) == i : GetRow(x) == i).ToArray();
                        if (children.Length > 0)
                            sizes[i] =
                                measureColumns
                                    ? children.Max(
                                        x =>
                                        x.Width.Type != ControlSizeType.BoundingBoxSize
                                            ? x.ActualWidth + (DrawBorders ? i == 0 ? 2 : 1 : 0)
                                            : long.MaxValue)
                                    : children.Max(
                                        x =>
                                        x.Height.Type != ControlSizeType.BoundingBoxSize
                                            ? x.ActualHeight + (DrawBorders ? i == 0 ? 2 : 1 : 0)
                                            : long.MaxValue);
                        else sizes[i] = 0;
                        break;
                    case GridUnitType.Fixed:
                        sizes[i] = definitions[i].Value + (DrawBorders ? i == 0 ? 2 : 1 : 0);
                        break;
                    case GridUnitType.Grow:
                        break; // process later
                }
            }
            var remaining = boxSize - sizes.Sum(x => x < int.MaxValue ? x : 0);
            var alignedToBox = sizes.Count(x => x >= int.MaxValue);
            if (alignedToBox > 0)
            {
                var size = remaining / alignedToBox;
                var first = true;
                for (var i = 0; i < sizes.Length; i++)
                {
                    if (sizes[i] >= int.MaxValue)
                    {
                        if (first)
                        {
                            sizes[i] = size + remaining % alignedToBox;
                            first = false;
                        }
                        else sizes[i] = size;
                    }
                }
            }
            else if (remaining > 0)
            {
                var totalGrowRate = definitions.Sum(x => x.UnitType == GridUnitType.Grow ? x.Value : 0);
                if (totalGrowRate > 0)
                {
                    var growUnit = remaining / totalGrowRate;
                    var first = true;
                    for (var i = 0; i < sizes.Length; i++)
                    {
                        if (definitions[i].UnitType == GridUnitType.Grow)
                        {
                            if (first)
                            {
                                sizes[i] = definitions[i].Value * growUnit + remaining % growUnit;
                                first = false;
                            }
                            else sizes[i] = definitions[i].Value * growUnit;
                        }
                    }
                }
            }
            else
            {
                for (var i = 0; i < sizes.Length; i++) if (sizes[i] < 0) sizes[i] = 0;
            }
            return sizes.Select(x => (int)x).ToArray();
        }

        public override Rectangle MeasureBoundingBox(Control child)
        {
            if (!Children.Contains(child)) return new Rectangle(0, 0, 0, 0);
            var rows = MeasureSizes(false);
            var columns = MeasureSizes(true);
            var row = Math.Max(0, Math.Min(GetRow(child), rows.Length));
            var column = Math.Max(0, Math.Min(GetColumn(child), columns.Length));
            var rowSpan = Math.Max(1, Math.Min(GetRowSpan(child), rows.Length - row));
            var columnSpan = Math.Max(1, Math.Min(GetColumnSpan(child), columns.Length - column));
            var x = columns.Take(column).Sum();
            var y = rows.Take(row).Sum();
            var w = columns.Skip(column).Take(columnSpan).Sum();
            var h = rows.Skip(row).Take(rowSpan).Sum();
            if (DrawBorders)
            {
                var nfc = column > 0 ? 1 : 0;
                var nfr = row > 0 ? 1 : 0;
                return new Rectangle(x + 1 - nfc, y + 1 - nfr, w - 2 + nfc, h - 2 + nfr);
            }
            return new Rectangle(x + 1, y + 1, w, h);
        }

        public override Point GetScrollOffset(Control child)
        {
            return new Point(0, 0);
        }

        public override bool IsChildVisible(Control child)
        {
            return true;
        }

        private bool HasSpanningChildren(int row, int column, bool vertical)
        {
            return
                vertical
                    ? Children.Any(x =>
                    {
                        var xrow = GetRow(x);
                        var xcol = GetColumn(x);
                        return (xrow <= row) && (xcol == column) &&
                               (xrow + GetRowSpan(x) - 1 > row);
                    })
                    : Children.Any(x =>
                    {
                        var xrow = GetRow(x);
                        var xcol = GetColumn(x);
                        return (xrow == row) && (xcol <= column) &&
                               (xcol + GetColumnSpan(x) - 1 > column);
                    });
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
            var rows = MeasureSizes(false);
            var columns = MeasureSizes(true);
            for (var column = 0; column < Math.Max(1, ColumnDefinitions.Count); column++)
                for (var row = 0; row < Math.Max(1, RowDefinitions.Count); row++)
                {
                    var child = Children.FirstOrDefault(x => GetRow(x) == row && GetColumn(x) == column);
                    var nfc = column > 0 ? 1 : 0;
                    var nfr = row > 0 ? 1 : 0;
                    var boundingBox =
                        child != null
                            ? MeasureBoundingBox(child).Offset(-1, -1).Expand(2, 2)
                            : HasSpanningChildren(row - 1, column, true)
                                  ? new Rectangle(0, 0, 0, 0)
                                  : HasSpanningChildren(row, column - 1, false)
                                        ? new Rectangle(0, 0, 0, 0)
                                        : new Rectangle(columns.Take(column).Sum() - nfc, 
                                                        rows.Take(row).Sum() - nfr,
                                                        columns[column] + nfc,
                                                        rows[row] + nfr);
                    if (boundingBox.Width == 0 || boundingBox.Height == 0) continue;
                    dc.DrawFrame(boundingBox, new FrameOptions
                    {
                        Foreground = Foreground,
                        Background = Background,
                        Style = style,
                    });
                    if (column > 0 || row > 0)
                        dc.PutChar(new Point(boundingBox.X, boundingBox.Y),
                                   column > 0
                                       ? row > 0 && !HasSpanningChildren(row - 1, column - 1, false)
                                             ? !HasSpanningChildren(row - 1, column - 1, true)
                                                   ? Piece(FramePiece.Cross, style)
                                                   : Piece(FramePiece.Vertical | FramePiece.Right, style)
                                             : Piece(FramePiece.Horizontal | FramePiece.Bottom, style)
                                       : Piece(FramePiece.Vertical | FramePiece.Right, style),
                                   Foreground, Background, CharAttribute.None);
                    if (column == Math.Max(1, ColumnDefinitions.Count) - 1 && row > 0)
                        dc.PutChar(new Point(boundingBox.X + boundingBox.Width - 1, boundingBox.Y),
                                   Piece(FramePiece.Vertical | FramePiece.Left, style),
                                   Foreground, Background, CharAttribute.None);
                    if (row == Math.Max(1, RowDefinitions.Count) - 1 && column > 0)
                        dc.PutChar(new Point(boundingBox.X, boundingBox.Y + boundingBox.Height - 1),
                                   Piece(FramePiece.Horizontal | FramePiece.Top, style),
                                   Foreground, Background, CharAttribute.None);
                }
            dc.DrawText(new Point(2, 0), truncated, new TextOptions
            {
                Background = Background,
                Foreground = Foreground
            });
        }

        public IList<Control> Children { get; }
        public IList<Control> FocusableChildren => Children;
        public event EventHandler<ChildRemovedEventArgs> ChildRemoved;

        private class ChildrenCollection : IList<Control>
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

            public int Count => _internal.Count;
            public bool IsReadOnly { get; } = false;

            public int IndexOf(Control item)
            {
                return _internal.IndexOf(item);
            }

            public void Insert(int index, Control item)
            {
                _internal.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _internal.RemoveAt(index);
            }

            public Control this[int index]
            {
                get { return _internal[index]; }
                set { _internal[index] = value; }
            }
        }
    }
}
