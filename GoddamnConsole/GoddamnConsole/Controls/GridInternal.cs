using System;
using System.Collections.Generic;
using System.Linq;
using GoddamnConsole.Drawing;
using static GoddamnConsole.Drawing.FrameOptions;

namespace GoddamnConsole.Controls
{
    internal class GridInternal
    {
        private static int[] MeasureSizes(
            bool measureColumns,
            bool drawBorders,
            IList<Control> ichildren,
            int actualWidth,
            int actualHeight,
            IList<GridSize> columnDefinitions,
            IList<GridSize> rowDefinitions,
            int offset)
        {
            var boxSize = measureColumns ? actualWidth : actualHeight;
            if (!drawBorders) boxSize -= offset * 2;
            IList<GridSize> definitions = measureColumns ? columnDefinitions : rowDefinitions;
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
                            ichildren.Where(x => measureColumns ? Grid.GetColumn(x) == i : Grid.GetRow(x) == i).ToArray();
                        if (children.Length > 0)
                            sizes[i] =
                                measureColumns
                                    ? children.Max(
                                        x =>
                                        x.Width.Type != ControlSizeType.BoundingBoxSize
                                            ? x.ActualWidth + (drawBorders ? i == 0 ? 2 : 1 : 0)
                                            : long.MaxValue)
                                    : children.Max(
                                        x =>
                                        x.Height.Type != ControlSizeType.BoundingBoxSize
                                            ? x.ActualHeight + (drawBorders ? i == 0 ? 2 : 1 : 0)
                                            : long.MaxValue);
                        else sizes[i] = long.MaxValue;
                        break;
                    case GridUnitType.Fixed:
                        sizes[i] = definitions[i].Value + (drawBorders ? i == 0 ? 2 : 1 : 0);
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
                var totalGrowRate = definitions.Sum(x => x.UnitType == GridUnitType.Grow ? Math.Max(1, x.Value) : 0);
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
                                sizes[i] = Math.Max(1, definitions[i].Value) * growUnit + remaining % growUnit;
                                first = false;
                            }
                            else sizes[i] = Math.Max(1, definitions[i].Value) * growUnit;
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

        public static Rectangle MeasureBoundingBox(
            IList<Control> children,
            Control child,
            bool drawBorders,
            int actualWidth,
            int actualHeight,
            IList<GridSize> columnDefinitions,
            IList<GridSize> rowDefinitions,
            int xoffset,
            int yoffset)
        {
            if (!children.Contains(child)) return new Rectangle(0, 0, 0, 0);
            var rows = MeasureSizes(false, drawBorders, children, actualWidth, actualHeight,
                columnDefinitions, rowDefinitions, yoffset);
            var columns = MeasureSizes(true, drawBorders, children, actualWidth, actualHeight,
                columnDefinitions, rowDefinitions, xoffset);
            var row = Math.Max(0, Math.Min(Grid.GetRow(child), rows.Length));
            var column = Math.Max(0, Math.Min(Grid.GetColumn(child), columns.Length));
            var rowSpan = Math.Max(1, Math.Min(Grid.GetRowSpan(child), rows.Length - row));
            var columnSpan = Math.Max(1, Math.Min(Grid.GetColumnSpan(child), columns.Length - column));
            var x = columns.Take(column).Sum();
            var y = rows.Take(row).Sum();
            var w = columns.Skip(column).Take(columnSpan).Sum();
            var h = rows.Skip(row).Take(rowSpan).Sum();
            if (drawBorders)
            {
                var nfc = column > 0 ? 1 : 0;
                var nfr = row > 0 ? 1 : 0;
                return new Rectangle(x + 1 - nfc, y + 1 - nfr, w - 2 + nfc, h - 2 + nfr);
            }
            return new Rectangle(x + xoffset, y + yoffset, w, h);
        }

        private static bool HasSpanningChildren(
            IList<Control> children,
            int row, int column, bool vertical)
        {
            return
                vertical
                    ? children.Any(x =>
                    {
                        var xrow = Grid.GetRow(x);
                        var xcol = Grid.GetColumn(x);
                        return (xrow <= row) && (xcol == column) &&
                               (xrow + Grid.GetRowSpan(x) - 1 > row);
                    })
                    : children.Any(x =>
                    {
                        var xrow = Grid.GetRow(x);
                        var xcol = Grid.GetColumn(x);
                        return (xrow == row) && (xcol <= column) &&
                               (xcol + Grid.GetColumnSpan(x) - 1 > column);
                    });
        }

        public static void Render(
            DrawingContext dc,
            CharColor background,
            CharColor foreground,
            FrameStyle style,
            IList<Control> children,
            IList<GridSize> columnDefinitions,
            IList<GridSize> rowDefinitions,
            int width,
            int height,
            int xoffset,
            int yoffset)
        {

            var rows = MeasureSizes(false, true, children, width, height,
                                    columnDefinitions, rowDefinitions, yoffset);
            var columns = MeasureSizes(true, true, children, width, height,
                                       columnDefinitions, rowDefinitions, xoffset);
            for (var column = 0; column < Math.Max(1, columnDefinitions.Count); column++)
                for (var row = 0; row < Math.Max(1, rowDefinitions.Count); row++)
                {
                    var child = children.FirstOrDefault(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == column);
                    var nfc = column > 0 ? 1 : 0;
                    var nfr = row > 0 ? 1 : 0;
                    var boundingBox =
                        child != null
                            ? MeasureBoundingBox(children, child, true, width, height, columnDefinitions, rowDefinitions,
                                                 xoffset, yoffset).Offset(-1, -1).Expand(2, 2)
                            : HasSpanningChildren(children, row - 1, column, true)
                                  ? new Rectangle(0, 0, 0, 0)
                                  : HasSpanningChildren(children, row, column - 1, false)
                                        ? new Rectangle(0, 0, 0, 0)
                                        : new Rectangle(columns.Take(column).Sum() - nfc,
                                                        rows.Take(row).Sum() - nfr,
                                                        columns[column] + nfc,
                                                        rows[row] + nfr);
                    if (boundingBox.Width == 0 || boundingBox.Height == 0) continue;
                    dc.DrawFrame(boundingBox, new FrameOptions
                    {
                        Foreground = foreground,
                        Background = background,
                        Style = style,
                    });
                    if (column > 0 || row > 0)
                        dc.PutChar(new Point(boundingBox.X, boundingBox.Y),
                                   column > 0
                                       ? row > 0 && !HasSpanningChildren(children, row - 1, column - 1, false)
                                             ? !HasSpanningChildren(children, row - 1, column - 1, true)
                                                   ? Piece(FramePiece.Cross, style)
                                                   : Piece(FramePiece.Vertical | FramePiece.Right, style)
                                             : Piece(FramePiece.Horizontal | FramePiece.Bottom, style)
                                       : Piece(FramePiece.Vertical | FramePiece.Right, style),
                                   foreground, background, CharAttribute.None);
                    if (column == Math.Max(1, columnDefinitions.Count) - 1 && row > 0)
                        dc.PutChar(new Point(boundingBox.X + boundingBox.Width - 1, boundingBox.Y),
                                   Piece(FramePiece.Vertical | FramePiece.Left, style),
                                   foreground, background, CharAttribute.None);
                    if (row == Math.Max(1, rowDefinitions.Count) - 1 && column > 0)
                        dc.PutChar(new Point(boundingBox.X, boundingBox.Y + boundingBox.Height - 1),
                                   Piece(FramePiece.Horizontal | FramePiece.Top, style),
                                   foreground, background, CharAttribute.None);
                }
        }
    }
}
