using System;
using System.Linq;
using System.Xaml;
using GoddamnConsole.Drawing;

namespace GoddamnConsole.Controls
{
    /// <summary>
    /// Represents a flexible grid area that consists of rows and columns. Child elements of the Grid are measured and arranged according to their row/column assignments
    /// </summary>
    public class Grid : ChildrenControl
    {
        // + Attached properties

        private static readonly AttachableMemberIdentifier RowProperty = new AttachableMemberIdentifier(typeof(Grid), "Row");

        public static int GetRow(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(RowProperty);

        public static void SetRow(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(RowProperty, value);
        }

        private static readonly AttachableMemberIdentifier RowSpanProperty = new AttachableMemberIdentifier(typeof(Grid), "RowSpan");

        public static int GetRowSpan(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(RowSpanProperty);

        public static void SetRowSpan(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(RowSpanProperty, value);
        }

        private static readonly AttachableMemberIdentifier ColumnProperty = new AttachableMemberIdentifier(typeof(Grid), "Column");

        public static int GetColumn(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(ColumnProperty);

        public static void SetColumn(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(ColumnProperty, value);
        }

        private static readonly AttachableMemberIdentifier ColumnSpanProperty = new AttachableMemberIdentifier(typeof(Grid), "ColumnSpan");

        public static int GetColumnSpan(IBetterAttachedPropertyStore dp) => dp.GetValue<int>(ColumnSpanProperty);

        public static void SetColumnSpan(IBetterAttachedPropertyStore dp, int value)
        {
            dp.SetProperty(ColumnSpanProperty, value);
        }

        // - Attached properties

        private bool _drawBorders;

        public bool DrawBorders
        {
            get { return _drawBorders; }
            set { _drawBorders = value; OnPropertyChanged(); }
        }
        
        protected override int MaxHeightByContent =>
            Children.GroupBy(GetColumn)
                    .Max(x => x.Sum(y => y.ActualHeight))
            + (DrawBorders ? Math.Max(1, RowDefinitions.Count) + 1 : 0);

        protected override int MaxWidthByContent =>
            Children.GroupBy(GetRow)
                    .Max(x => x.Sum(y => y.ActualWidth))
            + (DrawBorders ? Math.Max(1, ColumnDefinitions.Count) + 1 : 0);

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
                Children, child, DrawBorders, ActualWidth, ActualHeight, 
                ColumnDefinitions, RowDefinitions, 0, 0);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (DrawBorders)
                GridInternal.Render(
                    dc,
                    Background,
                    Foreground,
                    FrameStyle.Single,
                    Children,
                    ColumnDefinitions, RowDefinitions,
                    ActualWidth, ActualHeight,
                    0, 0);
        }
    }

    /// <summary>
    /// Describes the kind of value that a GridUnitType object is holding
    /// </summary>
    public enum GridUnitType
    {
        /// <summary>
        /// The value is expressed as a weighted proportion of available space
        /// </summary>
        Grow,
        /// <summary>
        /// The size is determined by the size of content object
        /// </summary>
        Auto,
        /// <summary>
        /// The value is expressed in pixels
        /// </summary>
        Fixed
    }

    /// <summary>
    /// Represents a row/column size value
    /// </summary>
    public class GridSize
    {
        public GridSize() { }

        public GridSize(GridUnitType unit, int val)
        {
            UnitType = unit;
            Value = Math.Max(0, val);
        }

        /// <summary>
        /// Gets or sets the fixed value (used only in Fixed sizing)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets the type of sizing
        /// </summary>
        public GridUnitType UnitType { get; set; }
    }
}
