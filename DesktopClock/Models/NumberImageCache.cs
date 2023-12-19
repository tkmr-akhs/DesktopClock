using Microsoft.UI.Xaml.Media.Imaging;
using DesktopClock.Helpers;

namespace DesktopClock.Models;

/// <summary>
/// Caches images of numbers with a specific style and size for efficient retrieval.
/// </summary>
internal class NumberImageCache
{
    /// <summary>
    /// Gets the height of the number images in the cache.
    /// </summary>
    public float Height { get; private set; }

    /// <summary>
    /// Gets the width of the number images in the cache.
    /// </summary>
    public float Width { get; private set; }

    private readonly Dictionary<char, BitmapImage> numberImages;

    /// <summary>
    /// Initializes a new instance of the NumberImageCache class using specified numbers, text style, border width, and font height.
    /// </summary>
    /// <param name="numbers">Array of characters representing numbers to cache.</param>
    /// <param name="style">The style to apply to the number images.</param>
    /// <param name="borderWidth">Width of the border around the numbers.</param>
    /// <param name="desiredFontHeight">Desired height of the font for the numbers.</param>
    public NumberImageCache(char[] numbers, TextStyle style, int borderWidth, int desiredFontHeight)
    {
        var textMetrics = TextImagingHelper.GetMaxTextBounds(numbers, style, borderWidth, desiredFontHeight);

        Height = textMetrics.Bounds.Height;
        Width = textMetrics.Bounds.Width;

        numberImages = new Dictionary<char, BitmapImage>();

        foreach (var number in numbers)
        {
            numberImages[number] = TextImagingHelper.GenerateCharacterBitmapImage(number.ToString()[0], style, textMetrics);
        }
    }

    /// <summary>
    /// Retrieves the image for a specific number character.
    /// </summary>
    /// <param name="number">The character representing the number whose image is to be retrieved.</param>
    /// <returns>The bitmap image of the specified number.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified number is not in the cache.</exception>
    public BitmapImage GetImage(char number)
    {
        if (!numberImages.ContainsKey(number))
        {
            throw new ArgumentException();
        }
        
        return numberImages[number];
    }
}
