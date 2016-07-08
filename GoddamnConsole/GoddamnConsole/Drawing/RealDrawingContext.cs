﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static GoddamnConsole.Drawing.FrameOptions;

namespace GoddamnConsole.Drawing
{
    internal sealed class RealDrawingContext : DrawingContext
    {
        public override int RenderOffsetX => _x;
        public override int RenderOffsetY => _y;

        public RealDrawingContext(bool lowBrightness = false)
        {
            _lowBrightness = lowBrightness;
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;
            _x = _scrollX = _y = _scrollY = 0;
        }

        private readonly bool _lowBrightness;
        private int _scrollX;
        private int _scrollY; 
        private int _x;
        private int _y;
        private int _width;
        private int _height;

        public override DrawingContext Scroll(Point sourceOffset)
        {
            if (_width - sourceOffset.X <= 0 || _height - sourceOffset.Y <= 0) return new ImaginaryDrawingContext();
            return new RealDrawingContext(_lowBrightness)
            {
                _x = _x,
                _y = _y,
                _width = _width,
                _height = _height,
                _scrollX = _scrollX + sourceOffset.X,
                _scrollY = _scrollY + sourceOffset.Y
            };
        }

        public override DrawingContext Shrink(Point sourceOffset, Rectangle targetArea)
        {
            return Shrink(targetArea).Scroll(sourceOffset);
        }

        public override DrawingContext Shrink(Rectangle targetArea)
        {
            if (targetArea.X < 0 || targetArea.Y < 0) throw new ArgumentException(nameof(targetArea));
            var nx = _x + targetArea.X;
            var ny = _y + targetArea.Y;
            var nw = Math.Min(_width - targetArea.X, targetArea.Width);
            var nh = Math.Min(_height - targetArea.Y, targetArea.Height);
            if (nw <= 0 || nh <= 0) return new ImaginaryDrawingContext();
            return new RealDrawingContext(_lowBrightness)
            {
                _x = nx,
                _y = ny,
                _width = nw,
                _height = nh
            };
        }

        public override void Clear(CharColor background)
        {
            for (var x = 0; x < _width - _scrollX; x++)
                for (var y = 0; y < _height - _scrollY; y++)
                {
                    PutChar(new Point(x, y), ' ', CharColor.White, background, CharAttribute.None);
                }
        }

        public static CharColor Darken(CharColor color)
        {
            return color == CharColor.Black ? CharColor.Black : CharColor.DarkGray;
        }

        public override void PutChar(Point pt, char chr, CharColor foreground, CharColor background, CharAttribute attribute)
        {
            pt = pt.Offset(_scrollX, _scrollY);
            if (pt.X < 0 || pt.Y < 0 || pt.X >= _width || pt.Y >= _height) return;
            Console.Provider.PutChar(
                new Character(
                    chr,
                    _lowBrightness ? Darken(foreground) : foreground,
                    _lowBrightness ? Darken(background) : background,
                    attribute), 
                pt.X + _x, pt.Y + _y);
        }

        public override void DrawRectangle(Rectangle rect, char fill, RectangleOptions opts = null)
        {
            var clippedRect = rect.Clip(0, 0, _width, _height);
            if (clippedRect.Width == 0 || clippedRect.Height == 0) return;
            for (var x = clippedRect.X; x < clippedRect.X + clippedRect.Width; x++)
                for (var y = clippedRect.Y; y < clippedRect.Y + clippedRect.Height; y++)
                {
                    PutChar(new Point(x, y), fill, opts?.Foreground ?? CharColor.White,
                        opts?.Background ?? CharColor.Black, opts?.Attributes ?? CharAttribute.None);
                }
        }

        public override void DrawText(Point point, string line, TextOptions opts = null)
        {
            var ptY = point.Y + _scrollY;
            if (ptY < 0 || ptY >= _height) return;
            line = Regex.Replace(line, "[\r\n\t\f\x85]", " ");
            for (int x = point.X, i = 0; x < Math.Min(line.Length, _width - point.X - _scrollX) + point.X; x++, i++)
            {
                PutChar(
                    new Point(x, point.Y), 
                    line[i], 
                    opts?.Foreground ?? CharColor.White, 
                    opts?.Background ?? CharColor.Black, 
                    opts?.Attributes ?? CharAttribute.None);
            }
        }

        public override void DrawText(Rectangle rect, string text, TextOptions opts = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var maxWid = rect.Width + rect.X - rect.X;
            IEnumerable<string> lines = text.Replace("\r\n", "\n").Split(new[] {'\n', '\x85'}, StringSplitOptions.None);
            lines = (opts?.TextWrapping ?? TextWrapping.NoWrap) == TextWrapping.Wrap
                ? lines.SelectMany(x => x.Split(maxWid/* -_scrollX*/)) // wrapped text shouldn't be influenced by scrolling 
                : lines.Select(x => x.Length > maxWid - _scrollX ? x.Remove(maxWid - _scrollX) : x);
            if (rect.X < 0)
                lines = lines.Select(x => x.Length > -rect.X ? x.Substring(-rect.X) : "");
            var skip = rect.Y < 0 ? -rect.Y : 0;
            var xOfs = rect.X > 0 ? rect.X : 0;
            var yOfs = rect.Y > 0 ? rect.Y : 0;
            var lineArray = lines.ToArray();
            switch (opts?.VerticalAlignment ?? Alignment.Begin)
            {
                case Alignment.Center:
                    yOfs += (rect.Height - lineArray.Length) / 2;
                    break;
                case Alignment.End:
                    yOfs += rect.Height - lineArray.Length;
                    break;
            }
            foreach (var line in lineArray.Skip(skip).Take((int) Math.Max(int.MaxValue, Math.Min((long) rect.Height - _scrollY, (long) _height - rect.Y - _scrollY))))
            {
                var alignX = 0;
                switch (opts?.HorizontalAlignment ?? Alignment.Begin)
                {
                    case Alignment.Center:
                        alignX = (rect.Width - line.Length) / 2;
                        break;
                    case Alignment.End:
                        alignX = rect.Width - line.Length;
                        break;
                }
                DrawText(new Point(xOfs + alignX, yOfs++), line, opts);
            }
        }

        public override void DrawFrame(Rectangle rect, FrameOptions opts = null)
        {
            var style = opts?.Style ?? FrameStyle.Single;
            var rectOpts = new RectangleOptions
            {
                Attributes = opts?.Attributes ?? CharAttribute.None,
                Background = opts?.Background ?? CharColor.Black,
                Foreground = opts?.Foreground ?? CharColor.White
            };
            DrawRectangle(new Rectangle(rect.X + 1, rect.Y, rect.Width - 2, 1), Piece(FramePiece.Horizontal, style), rectOpts);
            DrawRectangle(new Rectangle(rect.X + 1, rect.Y + rect.Height - 1, rect.Width - 2, 1), Piece(FramePiece.Horizontal, style), rectOpts);
            DrawRectangle(new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 2), Piece(FramePiece.Vertical, style), rectOpts);
            DrawRectangle(new Rectangle(rect.X + rect.Width - 1, rect.Y + 1, 1, rect.Height - 2), Piece(FramePiece.Vertical, style), rectOpts);
            PutChar(new Point(rect.X, rect.Y), Piece(FramePiece.Bottom | FramePiece.Right, style), rectOpts.Foreground,
                    rectOpts.Background, rectOpts.Attributes);
            PutChar(new Point(rect.X + rect.Width - 1, rect.Y), Piece(FramePiece.Bottom | FramePiece.Left, style),
                    rectOpts.Foreground,
                    rectOpts.Background, rectOpts.Attributes);
            PutChar(new Point(rect.X, rect.Y + rect.Height - 1), Piece(FramePiece.Top | FramePiece.Right, style),
                    rectOpts.Foreground,
                    rectOpts.Background, rectOpts.Attributes);
            PutChar(new Point(rect.X + rect.Width - 1, rect.Y + rect.Height - 1),
                    Piece(FramePiece.Top | FramePiece.Left, style),
                    rectOpts.Foreground, rectOpts.Background, rectOpts.Attributes);
        }
    }

    internal static class Helpers
    {
        public static IEnumerable<string> Split(this string s, int len)
        {
            if (len <= 0)
            {
                yield return string.Empty;
                yield break;
            }
            if (s == "")
            {
                yield return s;
                yield break;
            }
            var ofs = 0;
            var slen = s.Length;
            while (slen - ofs >= len)
            {
                yield return s.Substring(ofs, len);
                ofs += len;
            }
            if (slen - ofs > 0) yield return s.Substring(ofs);
        }
    }
}
