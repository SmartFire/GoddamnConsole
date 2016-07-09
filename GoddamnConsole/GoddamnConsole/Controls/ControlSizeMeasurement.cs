using System;
using System.ComponentModel;
using System.Globalization;

namespace GoddamnConsole.Controls
{
    public abstract partial class Control
    {
        private ControlSize _width = ControlSizeType.BoundingBoxSize;
        private ControlSize _height = ControlSizeType.BoundingBoxSize;
        private bool _visibility = true;

        /// <summary>
        /// Gets or sets the width of this control
        /// </summary>
        [AlsoNotifyFor(nameof(ActualWidth))]
        public ControlSize Width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the height of this control
        /// </summary>
        [AlsoNotifyFor(nameof(ActualHeight))]
        public ControlSize Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(); }
        }

        [AlsoNotifyFor(nameof(ActualWidth))]
        public int MaxWidth
        {
            get { return _maxWidth; }
            set { _maxWidth = value; OnPropertyChanged(); }
        }

        [AlsoNotifyFor(nameof(ActualHeight))]
        public int MaxHeight
        {
            get { return _maxHeight; }
            set { _maxHeight = value; OnPropertyChanged(); }
        }

        [AlsoNotifyFor(nameof(ActualWidth))]
        public int MinWidth
        {
            get { return _minWidth; }
            set { _minWidth = value; OnPropertyChanged(); }
        }

        [AlsoNotifyFor(nameof(ActualHeight))]
        public int MinHeight
        {
            get { return _minHeight; }
            set { _minHeight = value; OnPropertyChanged(); }
        }

        private bool _isMeasuringMaxWidth;
        private bool _isMeasuringMaxHeight;
        private int _maxWidth = int.MaxValue;
        private int _maxHeight = int.MaxValue;
        private int _minWidth;
        private int _minHeight;

        protected abstract int MaxWidthByContent { get; }
        protected abstract int MaxHeightByContent { get; }

        public int MeasureWidth(ControlSize? overrideSize = null)
        {
            var size = overrideSize ?? Width;
            switch (size.Type)
            {
                case ControlSizeType.Fixed:
                    return Math.Max(0, size.Value);
                case ControlSizeType.Infinite:
                    return int.MaxValue;
                case ControlSizeType.BoundingBoxSize:
                    return Parent?.MeasureBoundingBox(this)?.Width ?? Console.WindowWidth;
                case ControlSizeType.MaxByContent:
                    if (_isMeasuringMaxWidth)
                    {
                        // measurement loop
                        return MaxWidth;
                    }
                    _isMeasuringMaxWidth = true;
                    var value = MaxWidthByContent;
                    _isMeasuringMaxWidth = false;
                    return value;
                default:
                    return 0;
            }
        }

        public int MeasureHeight(ControlSize? overrideSize = null)
        {
            var size = overrideSize ?? Height;
            switch (size.Type)
            {
                case ControlSizeType.Fixed:
                    return Math.Max(0, size.Value);
                case ControlSizeType.Infinite:
                    return int.MaxValue;
                case ControlSizeType.BoundingBoxSize:
                    return Parent?.MeasureBoundingBox(this)?.Height ?? Console.WindowHeight;
                case ControlSizeType.MaxByContent:
                    if (_isMeasuringMaxHeight)
                    {
                        // measurement loop
                        return MaxHeight;
                    }
                    _isMeasuringMaxHeight = true;
                    var value = MaxHeightByContent;
                    _isMeasuringMaxHeight = false;
                    return value;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the measured width of this control
        /// </summary>
        public int ActualWidth => Math.Max(0, Math.Max(MinWidth, Math.Min(MaxWidth, MeasureWidth())));

        /// <summary>
        /// Returns the measured height of this control
        /// </summary>
        public int ActualHeight => Math.Max(0, Math.Max(MinHeight, Math.Min(MaxHeight, MeasureHeight())));

        /// <summary>
        /// Gets or sets a value that indicates whether control is visible
        /// </summary>
        [AlsoNotifyFor(nameof(ActualVisibility))]
        public bool Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Returns a value that indicates whether control is actually visible
        /// </summary>
        public bool ActualVisibility => Visibility && (Parent?.IsChildVisible(this) ?? true);
    }
    
    /// <summary>
    /// Describes the kind of value that a ControlSize object is holding
    /// </summary>
    public enum ControlSizeType
    {
        /// <summary>
        /// Control size is fixed value
        /// </summary>
        Fixed,
        /// <summary>
        /// Control size is infinite value
        /// </summary>
        Infinite,
        /// <summary>
        /// Control size equals to size of bounding box
        /// </summary>
        BoundingBoxSize,
        /// <summary>
        /// Control size equals of maximal content size
        /// </summary>
        MaxByContent,
        /// <summary>
        /// Control size equals of minimal content size
        /// </summary>
        [Obsolete]
        MinByContent
    }

    /// <summary>
    /// Represents a size of control that supports different kinds of sizing
    /// </summary>
    [TypeConverter(typeof(ControlSizeConverter))]
    public struct ControlSize
    {
        public ControlSize(ControlSizeType type, int value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the type of sizing
        /// </summary>
        public ControlSizeType Type { get; set; }

        /// <summary>
        /// Gets or sets the fixed value (used only in Fixed sizing)
        /// </summary>
        public int Value { get; set; }

        public static implicit operator ControlSize(uint size)
            => new ControlSize(ControlSizeType.Fixed, (int)Math.Max(size, int.MaxValue));

        public static implicit operator ControlSize(int size)
            => new ControlSize(ControlSizeType.Fixed, Math.Max(0, size));

        public static implicit operator ControlSize(ControlSizeType type)
        {
            return new ControlSize(type, 0);
        }

    }

    public class ControlSizeConverter : TypeConverter
    {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(ControlSize);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = (string)value;
            int iv;
            if (!int.TryParse(str, out iv))
            {
                ControlSizeType utv;
                if (!Enum.TryParse(str, out utv)) throw new InvalidCastException();
                return new ControlSize(utv, 0);
            }
            return new ControlSize(ControlSizeType.Fixed, iv);
        }
    }
}
