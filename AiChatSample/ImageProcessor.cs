using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AiChatSample;

public class ImageProcessor
{
    public static BitmapSource GetDownscaledImage(string imagePath, int maxSize = 512)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new ArgumentException("The image path is invalid or empty.", nameof(imagePath));
        }
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"The file at path {imagePath} does not exist.", imagePath);
        }
        BitmapImage bitmapImage = LoadBitmapImage(imagePath);
        return ResizeBitmap(bitmapImage, maxSize);
    }

    public static byte[] ConvertBitmapSourceToByteArray(BitmapSource bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        using MemoryStream ms = new();
        BmpBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        return ms.ToArray();
    }

    public static byte[] ConvertBitmapSourceToJpegByteArray(BitmapSource bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        using MemoryStream ms = new();
        JpegBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        return ms.ToArray();
    }

    private static TransformedBitmap ResizeBitmap(BitmapSource sourceBitmap, int maxSize)
    {
        double scale = Math.Min(maxSize / (double)sourceBitmap.PixelWidth, maxSize / (double)sourceBitmap.PixelHeight);
        ScaleTransform transform = new(scale, scale);
        TransformedBitmap resizedBitmap = new(sourceBitmap, transform);
        resizedBitmap.Freeze();
        return resizedBitmap;
    }

    private static BitmapImage LoadBitmapImage(string imagePath)
    {
        BitmapImage bitmapImage = new();
        using FileStream fs = new(imagePath, FileMode.Open, FileAccess.Read);
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = fs;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }
}
