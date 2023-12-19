using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.UI.Xaml.Media.Imaging;

namespace DesktopClock.Helpers;

/// <summary>
/// Provides helper methods for image manipulation, such as combining multiple images into one and converting image formats.
/// </summary>
public static class ImagingHelper
{
    /// <summary>
    /// Combines multiple Bitmap images into a single Bitmap, either horizontally or vertically.
    /// </summary>
    /// <param name="bitmaps">The list of Bitmap images to combine.</param>
    /// <param name="orientation">The orientation for combining images, either horizontal or vertical.</param>
    /// <returns>A new Bitmap containing the combined images, or null if the input list is null or empty.</returns>
    public static Bitmap CombineBitmaps(IList<Bitmap> bitmaps, ImageCombineOrientation orientation = ImageCombineOrientation.Horizontal)
    {
        if (bitmaps == null || bitmaps.Count == 0)
            return null;

        // Calculate the total size of the combined image
        int width = 0;
        int height = 0;
        foreach (var bitmap in bitmaps)
        {
            if (orientation == ImageCombineOrientation.Horizontal)
            {
                width += bitmap.Width;
                height = Math.Max(height, bitmap.Height);
            }
            else
            {
                width = Math.Max(width, bitmap.Width);
                height += bitmap.Height;
            }
        }

        // Create a new Bitmap for the combined image
        var combinedBitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(combinedBitmap))
        {
            int offset = 0;
            foreach (var bitmap in bitmaps)
            {
                if (orientation == ImageCombineOrientation.Horizontal)
                {
                    g.DrawImage(bitmap, offset, 0);
                    offset += bitmap.Width;
                }
                else
                {
                    g.DrawImage(bitmap, 0, offset);
                    offset += bitmap.Height;
                }
            }
        }

        return combinedBitmap;
    }

    /// <summary>
    /// Converts a Bitmap to a BitmapImage, suitable for use in XAML UIs.
    /// </summary>
    /// <param name="bitmap">The Bitmap to convert.</param>
    /// <returns>A BitmapImage created from the provided Bitmap.</returns>
    public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
    {
        var bitmapImage = new BitmapImage();

        using var imageStream = new MemoryStream();
        bitmap.Save(imageStream, ImageFormat.Png);
        imageStream.Position = 0;

        // Set the MemoryStream as the source for BitmapImage
        bitmapImage.SetSource(imageStream.AsRandomAccessStream());

        return bitmapImage;
    }
}

/// <summary>
/// Specifies the orientation for combining images.
/// </summary>
public enum ImageCombineOrientation
{
    /// <summary>Images are combined side by side</summary>
    Horizontal,

    /// <summary>Images are stacked on top of each other</summary>
    Vertical
}