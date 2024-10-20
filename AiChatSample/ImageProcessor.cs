using System.IO;
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
        BitmapImage bitmapImage = new();
        using FileStream fs = new(imagePath, FileMode.Open, FileAccess.Read);
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = fs;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        WriteableBitmap writeableBitmap = new(bitmapImage);
        TransformedBitmap resizedBitmap = ResizeBitmap(writeableBitmap, maxSize, maxSize);
        return resizedBitmap;
    }
    public static byte[] ConvertBitmapSourceToByteArray(BitmapSource bitmap)
    {
        using MemoryStream ms = new();
        BmpBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        return ms.ToArray();
    }

    private static TransformedBitmap ResizeBitmap(WriteableBitmap sourceBitmap, int maxWidth, int maxHeight)
    {
        double width = sourceBitmap.PixelWidth;
        double height = sourceBitmap.PixelHeight;
        double ratioX = maxWidth / width;
        double ratioY = maxHeight / height;
        double ratio = Math.Min(ratioX, ratioY);
        TransformedBitmap scaledBitmap = new();
        scaledBitmap.BeginInit();
        scaledBitmap.Source = sourceBitmap;
        scaledBitmap.Transform = new System.Windows.Media.ScaleTransform(ratio, ratio);
        scaledBitmap.EndInit();
        return scaledBitmap;
    }

}
