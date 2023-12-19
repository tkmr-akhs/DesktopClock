using System.Drawing;
using Microsoft.UI.Xaml.Media.Imaging;
using DesktopClock.Models;

namespace DesktopClock.Helpers;

/// <summary>
/// Provides methods for creating and manipulating bitmap images of text with specified font styles and sizes.
/// </summary>
internal static class TextImagingHelper
{
    private const int DefaultFontSize = 14;
    private const string Ascenders = "bdfhijkl年月日時分秒(){}";
    private const string Descenders = "gjpqy年月日時分秒(){}";

    /// <summary>
    /// Converts Windows.UI.Text font styles to System.Drawing.FontStyle.
    /// </summary>
    /// <param name="fontStyle">Windows.UI.Text.FontStyle value.</param>
    /// <param name="fontWeight">Windows.UI.Text.FontWeight value.</param>
    /// <returns>System.Drawing.FontStyle equivalent.</returns>
    private static FontStyle ConvertToFontStyle(Windows.UI.Text.FontStyle fontStyle, Windows.UI.Text.FontWeight fontWeight)
    {
        var fontStyleFlags = FontStyle.Regular;

        if (fontStyle == Windows.UI.Text.FontStyle.Italic || fontStyle == Windows.UI.Text.FontStyle.Oblique)
        {
            fontStyleFlags |= FontStyle.Italic;
        }

        if (fontWeight.Weight >= Microsoft.UI.Text.FontWeights.Bold.Weight)
        {
            fontStyleFlags |= FontStyle.Bold;
        }

        return fontStyleFlags;
    }

    /// <summary>
    /// Calculates the maximum bounds for a given array of characters with specified font information.
    /// </summary>
    /// <param name="characters">Array of characters to measure.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="borderWidth">Width of the border around the text.</param>
    /// <param name="desiredFontHeight">Desired height of the font.</param>
    /// <returns>Text metrics including the actual font size and bounding information.</returns>
    public static TextMetrics GetMaxTextBounds(char[] characters, TextStyle style, int borderWidth, int desiredFontHeight)
    {
        var finalTop = 0.0F;
        var finalWidth = 0.0F;
        var finalFontHeight = desiredFontHeight;

        var drawingFontStyle = ConvertToFontStyle(style.FontStyle, style.FontWeight);

        var minTop = float.MaxValue;
        var maxBottom = float.MinValue;
        var boundsLeft = 0.0F;

        var maxOriginalWidth = 0.0F;

        var fontHeightToBeChecked = (int)Math.Floor(desiredFontHeight - borderWidth * 2 - desiredFontHeight * 0.05);

        if (fontHeightToBeChecked < 1) throw new ArgumentException("Font height is too small or border width is too large.");

        using (var ff = new FontFamily(style.FontFamily))
        {
            // Scans for the appropriate height
            while (maxBottom - minTop < desiredFontHeight - borderWidth * 2)
            {
                // Assume current values as temporary results
                finalTop = minTop;
                finalFontHeight = fontHeightToBeChecked;

                // Increment the font size to be checked
                fontHeightToBeChecked++;

                RectangleF bounds;
                using (var font = new Font(ff, fontHeightToBeChecked, drawingFontStyle, GraphicsUnit.Pixel))
                using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    // Create a path with the string and get the boundary information
                    gp.AddString(new String(characters), ff, (int)drawingFontStyle, font.Size, new PointF(0, 0), StringFormat.GenericDefault);
                    bounds = gp.GetBounds();
                }

                // Obtain top and bottom information from the latest bounds
                minTop = bounds.Top;
                maxBottom = bounds.Bottom;
            }

            // Based on the obtained height, scan for width
            using (var bitmap = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bitmap))
            using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
            using (var font = new Font(ff, finalFontHeight, drawingFontStyle, GraphicsUnit.Pixel))
            {
                foreach (var c in characters)
                {
                    maxOriginalWidth = Math.Max(maxOriginalWidth, g.MeasureString(c.ToString(), font).Width);
                    gp.AddString(c.ToString(), ff, (int)drawingFontStyle, font.Size, new PointF(0, 0), StringFormat.GenericDefault);
                }

                var bounds = gp.GetBounds();
                boundsLeft = bounds.Left;
                finalWidth = Math.Max(finalWidth, bounds.Width);
            }
        }

        // Compensate for the left-right bias of the original font data
        var finalLeft = boundsLeft - (maxOriginalWidth - finalWidth) / 2;

        return new TextMetrics(finalFontHeight, borderWidth, new RectangleF(finalLeft - borderWidth, finalTop - borderWidth, finalWidth + borderWidth * 2, desiredFontHeight), true);
    }

    /// <summary>
    /// Calculates the maximum bounds for given text with specified font information.
    /// </summary>
    /// <param name="text">Text to measure.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="borderWidth">Width of the border around the text.</param>
    /// <param name="desiredFontHeight">Desired height of the font, if applicable.</param>
    /// <param name="desiredStringWidth">Desired width of the string, if applicable.</param>
    /// <returns>Text metrics including the actual font size and bounding information.</returns>
    public static TextMetrics GetMaxTextBounds(string text, TextStyle style, int borderWidth, int desiredFontHeight = 0, int desiredStringWidth = 0)
    {
        if (desiredFontHeight <= 0 && desiredStringWidth <= 0 || desiredFontHeight > 0 && desiredStringWidth > 0) { throw new ArgumentException("Provide either desired font height or string width, not both or none."); }

        return ( desiredFontHeight > 0)
                   ? GetMaxBoundsForHeight(text, style, borderWidth, desiredFontHeight)
                   : GetMaxBoundsForWidth(text, style, borderWidth, desiredStringWidth);
    }

    /// <summary>
    /// Calculates the maximum bounds of text for a given string width.
    /// </summary>
    /// <param name="text">Text to be measured.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="borderWidth">Width of the border around the text.</param>
    /// <param name="desiredStringWidth">Desired width of the string.</param>
    /// <returns>Text metrics including the actual font size and bounding information.</returns>
    private static TextMetrics GetMaxBoundsForWidth(string text, TextStyle style, int borderWidth, int desiredStringWidth = 0)
    {
        var drawingFontStyle = ConvertToFontStyle(style.FontStyle, style.FontWeight);

        // Calculates the height-to-width ratio
        float heightPerWidthRate;
        using (var ff = new FontFamily(style.FontFamily))
        using (var font = new Font(ff, DefaultFontSize, drawingFontStyle, GraphicsUnit.Pixel))
        using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
        {
            gp.AddString(text, ff, (int)drawingFontStyle, font.Size, new PointF(0, 0), StringFormat.GenericDefault);
            var bounds = gp.GetBounds();
            heightPerWidthRate = bounds.Height / bounds.Width;
        }

        // Determines the appropriate font height
        var desiredFontHeight = (int)Math.Floor((desiredStringWidth - borderWidth * 2) * heightPerWidthRate + borderWidth * 2);

        return GetMaxBoundsForHeight(text, style, borderWidth, desiredFontHeight, desiredStringWidth);
    }

    /// <summary>
    /// Calculates the maximum bounds of text for a given font height.
    /// </summary>
    /// <param name="text">Text to be measured.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="borderWidth">Width of the border around the text.</param>
    /// <param name="desiredFontHeight">Desired height of the font.</param>
    /// <param name="desiredStringWidth">Desired width of the string, if applicable.</param>
    /// <returns>Text metrics including the actual font size and bounding information.</returns>
    private static TextMetrics GetMaxBoundsForHeight(string text, TextStyle style, int borderWidth, int desiredFontHeight = 0, int desiredStringWidth = 0)
    {
        var checkHeightText = text + Ascenders + Descenders;
        var drawingFontStyle = ConvertToFontStyle(style.FontStyle, style.FontWeight);

        var temporaryBounds = new RectangleF(0, 0, 0, 0);
        var finalFontHeight = desiredFontHeight;
        var finalStringWidth = 0.0F;
        var finalX = 0.0F;
        var finalY = 0.0F;

        var fontHeightToBeChecked = (int)Math.Floor(desiredFontHeight - borderWidth * 2 - desiredFontHeight * 0.05);
        
        if (fontHeightToBeChecked < 1) throw new ArgumentException("Font height is too small or border width is too large.");

        using (var ff = new FontFamily(style.FontFamily))
        {
            // Scans for the appropriate height
            while (temporaryBounds.Height < desiredFontHeight - borderWidth * 2)
            {
                finalFontHeight = fontHeightToBeChecked;
                finalX = temporaryBounds.X;
                finalY = temporaryBounds.Y;

                fontHeightToBeChecked++;

                using (var font = new Font(ff, fontHeightToBeChecked, drawingFontStyle, GraphicsUnit.Pixel))
                using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    gp.AddString(checkHeightText, ff, (int)drawingFontStyle, font.Size, new PointF(0, 0), StringFormat.GenericDefault);
                    temporaryBounds = gp.GetBounds();
                }
            }

            // Measures the width
            if (desiredStringWidth <= 0)
            {
                using (var font = new Font(ff, finalFontHeight, drawingFontStyle, GraphicsUnit.Pixel))
                using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    gp.AddString(text, ff, (int)drawingFontStyle, font.Size, new PointF(0, 0), StringFormat.GenericDefault);
                    temporaryBounds = gp.GetBounds();
                    finalStringWidth = temporaryBounds.Width;
                }
            }
            else
            {
                finalStringWidth = desiredStringWidth - borderWidth * 2;
            }
        }

        return new TextMetrics(finalFontHeight, borderWidth, new RectangleF(finalX - borderWidth, finalY - borderWidth, finalStringWidth + borderWidth * 2, desiredFontHeight), false);
    }

    /// <summary>
    /// Generates a bitmap image of a single character with specified font style and text metrics.
    /// </summary>
    /// <param name="character">Character to render.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="textMetrics">Text metrics including size and bounding information.</param>
    /// <returns>Bitmap image of the character.</returns>
    public static BitmapImage GenerateCharacterBitmapImage(char character, TextStyle style, TextMetrics textMetrics)
    {
        return GenerateStringBitmapImage(character.ToString(), style, textMetrics);
    }


    /// <summary>
    /// Generates a bitmap of a single character with specified font style and text metrics.
    /// </summary>
    /// <param name="character">Character to render.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="textMetrics">Text metrics including size and bounding information.</param>
    /// <returns>Bitmap of the character.</returns>
    public static Bitmap GenerateCharacterBitmap(char character, TextStyle style, TextMetrics textMetrics)
    {
        return GenerateStringBitmap(character.ToString(), style, textMetrics);
    }


    /// <summary>
    /// Generates a bitmap image of a string with specified font style and text metrics.
    /// </summary>
    /// <param name="text">String to render.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="textMetrics">Text metrics including size and bounding information.</param>
    /// <returns>Bitmap image of the string.</returns>
    public static BitmapImage GenerateStringBitmapImage(string text, TextStyle style, TextMetrics textMetrics)
    {
        using var bitmap = GenerateStringBitmap(text, style, textMetrics);
        return ImagingHelper.ConvertBitmapToBitmapImage(bitmap);
    }

    /// <summary>
    /// Generates a bitmap of a string with specified font style and text metrics.
    /// </summary>
    /// <param name="text">String to render.</param>
    /// <param name="style">Font style information.</param>
    /// <param name="textMetrics">Text metrics including size and bounding information.</param>
    /// <returns>Bitmap of the string.</returns>
    public static Bitmap GenerateStringBitmap(string text, TextStyle style, TextMetrics textMetrics)
    {
        var drawingFontStyle = ConvertToFontStyle(style.FontStyle, style.FontWeight);
        var drawingBrushColor = Color.FromArgb(style.FontColor.A, style.FontColor.R, style.FontColor.G, style.FontColor.B);
        var drawingPenColor = Color.FromArgb(style.BorderColor.A, style.BorderColor.R, style.BorderColor.G, style.BorderColor.B);

        var bitmapImageWidth = (int)Math.Ceiling(textMetrics.Bounds.Width);
        var bitmapImageHeight = (int)Math.Ceiling(textMetrics.Bounds.Height);

        var bitmap = new Bitmap(bitmapImageWidth, bitmapImageHeight);

        using (var imageStream = new MemoryStream())
        using (var g = Graphics.FromImage(bitmap))
        using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
        using (var ff = new FontFamily(style.FontFamily))
        using (var font = new Font(style.FontFamily, textMetrics.ActualFontHeight, drawingFontStyle, GraphicsUnit.Pixel))
        {
            PointF addPoint;

            // Calculate horizontal padding
            if (textMetrics.IsCenter)
            {
                // Measure the size of the text
                var textSize = g.MeasureString(text, font);
                var horizontalPadding = (textMetrics.Bounds.Width - textMetrics.BorderWidth * 2 - textSize.Width) / 2;
                addPoint = new PointF(-textMetrics.Bounds.X + horizontalPadding, -textMetrics.Bounds.Y);
            }
            else
            {
                addPoint = new PointF(-textMetrics.Bounds.X, -textMetrics.Bounds.Y);
            }

            // Add text to GraphicsPath
            gp.AddString(text, ff, (int)drawingFontStyle, font.Size, addPoint, StringFormat.GenericDefault);

            // Set the drawing area
            var clip = new RectangleF(0, 0, bitmapImageWidth, bitmapImageHeight);
            g.SetClip(clip);

            // Perform the drawing
            DrawPath(g, gp, drawingBrushColor, drawingPenColor, textMetrics.BorderWidth);
        }

        return bitmap;
    }

    /// <summary>
    /// Renders the specified text path using the provided graphics object and color information.
    /// </summary>
    /// <param name="g">Graphics object for drawing.</param>
    /// <param name="gp">Graphics path representing the text.</param>
    /// <param name="brushColor">Color of the brush to fill the text.</param>
    /// <param name="penColor">Color of the pen for the text border.</param>
    /// <param name="borderWidth">Width of the text border.</param>
    private static void DrawPath(Graphics g, System.Drawing.Drawing2D.GraphicsPath gp, Color brushColor, Color penColor, int borderWidth)
    {
        // Enable anti-aliasing for smoother text
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Draw the outline
        var baseAlpha = (double)penColor.A / byte.MaxValue;
        for (int i = 0; i < borderWidth; i++)
        {
            // As it gets closer to the text, the transparency decreases proportionally, considering the overlay of drawings
            var alpha = baseAlpha / (borderWidth - i);

            // Create a pen for drawing
            using var pen = new Pen(Color.FromArgb((byte)Math.Round(byte.MaxValue * alpha), penColor), (borderWidth - i) * 2);
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            // Draw the outline
            g.DrawPath(pen, gp);
        }

        // Temporarily erase the inside of the text
        var currentCompositingMode = g.CompositingMode;
        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
        g.FillPath(new SolidBrush(Color.Transparent), gp);
        g.CompositingMode = currentCompositingMode;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Fill in the missing outline
        using (var pen = new Pen(penColor, 1))
        {
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            g.DrawPath(pen, gp);
        }

        // Fill the inside of the text
        using var brush = new SolidBrush(brushColor);
        g.FillPath(brush, gp);
    }
}
