using System;
using System.Globalization;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    public enum ProgressBarDisplayMode
    {
        None,
        Percentage,
        Position
    }

    public class ProgressBar : Control
    {
        private double _maxValue = 1;
        private double _minValue;
        private double _position;
        private ProgressBarDisplayMode _displayMode;

        public double MinValue
        {
            get { return _minValue; }
            set { _minValue = value; OnPropertyChanged(); }
        }

        public double MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; OnPropertyChanged(); }
        }

        public double Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged(); }
        }

        public ProgressBarDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set { _displayMode = value; OnPropertyChanged(); }
        }

        public override int MaxHeight => 3;

        protected override void OnRender(DrawingContext dc)
        {
            var ah = ActualHeight;
            var aw = ActualWidth;
            int width;
            if (aw < 3 || ah < 3)
            {
                if (ah == 0 || aw == 0) return;
                width = (int) (aw * (Position - MinValue) / (MaxValue - MinValue));
                if (width < 0) width = 0;
                dc.DrawRectangle(new Rectangle(0, 0, Math.Min(aw, width), ah), ' ', new RectangleOptions
                {
                    Background = Foreground
                });
            }
            else
            {
                width = (int) ((aw - 2) * (Position - MinValue) / (MaxValue - MinValue));
                if (width < 0) width = 0;
                dc.DrawFrame(new Rectangle(0, 0, aw, ah), new FrameOptions
                {
                    Background = Background,
                    Foreground = Foreground,
                    Style = FrameStyle.Single
                });
                dc.DrawRectangle(new Rectangle(1, 1, Math.Min(width, aw - 2), ah - 2), ' ',
                                 new RectangleOptions {Background = Foreground});
            }
            if (DisplayMode != ProgressBarDisplayMode.None)
            {
                var text = DisplayMode == ProgressBarDisplayMode.Percentage
                               ? $"{((Position - MinValue) / (MaxValue - MinValue) * 100),2}"
                               : Position.ToString(CultureInfo.InvariantCulture);
                if (text.Length <= aw)
                {
                    var lpartw = width - (aw - text.Length) / 2;
                    var lpart = lpartw > 0 ? lpartw < text.Length ? text.Remove(lpartw) : text : string.Empty;
                    var rpart = lpart.Length == text.Length ? string.Empty : text.Substring(lpart.Length);
                    dc.DrawText(new Point((aw - text.Length) / 2, ah / 2), lpart, new TextOptions
                    {
                        Background = Foreground,
                        Foreground = Background
                    });
                    dc.DrawText(new Point((aw - text.Length) / 2 + lpart.Length, ah / 2), rpart, new TextOptions
                    {
                        Background = Background,
                        Foreground = Foreground
                    });
                }
            }
        }
    }
}
