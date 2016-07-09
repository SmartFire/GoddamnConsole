﻿using System;
using System.Linq;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public class StackPanel : ChildrenControl
    {
        private StackPanelOrientation _orientation = StackPanelOrientation.Vertical;

        public override Rectangle MeasureBoundingBox(Control child)
        {
            if (Orientation == StackPanelOrientation.Vertical)
            {
                var yofs =
                    (int)
                    Math.Min(int.MaxValue,
                             Children.TakeWhile(x => x != child)
                                     .Sum(
                                         x =>
                                         x.Height.Type == ControlSizeType.BoundingBoxSize
                                             ? (long) x.MeasureHeight(ControlSizeType.MaxByContent)
                                             : x.ActualHeight));
                if (yofs > ActualHeight) return new Rectangle(0, 0, 0, 0);
                return
                    new Rectangle(
                        0,
                        yofs,
                        Math.Min(
                            ActualWidth,
                            child.Width.Type == ControlSizeType.BoundingBoxSize
                                ? ActualWidth
                                : child.ActualWidth),
                        Math.Min(ActualHeight - yofs,
                                 child.Height.Type == ControlSizeType.BoundingBoxSize
                                     ? child.MeasureHeight(ControlSizeType.MaxByContent)
                                     : child.ActualHeight));
            }
            var xofs =
                (int)
                Math.Min(int.MaxValue,
                         Children.TakeWhile(x => x != child)
                                 .Sum(
                                     x =>
                                     x.Width.Type == ControlSizeType.BoundingBoxSize
                                         ? (long) x.MeasureWidth(ControlSizeType.MaxByContent)
                                         : x.ActualWidth));
            if (xofs > ActualWidth) return new Rectangle(0, 0, 0, 0);
            return
                new Rectangle(
                    xofs,
                    0,
                    Math.Min(
                        ActualWidth - xofs,
                        child.Width.Type == ControlSizeType.BoundingBoxSize
                            ? child.MeasureWidth(ControlSizeType.MaxByContent)
                            : child.ActualWidth),
                    Math.Min(ActualHeight,
                             child.Height.Type == ControlSizeType.BoundingBoxSize
                                 ? ActualHeight
                                 : child.ActualHeight));
        }

        public StackPanelOrientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; OnPropertyChanged(); }
        }

        protected override int MaxHeightByContent =>
            Orientation == StackPanelOrientation.Vertical
                ? Children.Sum(x => x.ActualHeight)
                : Children.Max(x => x.ActualHeight);

        protected override int MaxWidthByContent =>
            Orientation == StackPanelOrientation.Horizontal
                ? Children.Sum(x => x.ActualWidth)
                : Children.Max(x => x.ActualWidth);
    }

    public enum StackPanelOrientation
    {
        Horizontal,
        Vertical
    }
}