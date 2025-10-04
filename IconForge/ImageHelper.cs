using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

public static class ImageHelper {
    public static Image ResizeImage(Image img, Size boxSize, bool keepAspect = true) {
        if (img == null)
            throw new ArgumentNullException(nameof(img));

        int targetWidth = boxSize.Width;
        int targetHeight = boxSize.Height;

        int newWidth = targetWidth;
        int newHeight = targetHeight;

        if (keepAspect) {
            // Compute best fit preserving aspect ratio
            double ratioX = (double)targetWidth / img.Width;
            double ratioY = (double)targetHeight / img.Height;
            double ratio = Math.Min(ratioX, ratioY);

            newWidth = (int)(img.Width * ratio);
            newHeight = (int)(img.Height * ratio);
        }

        // Create destination bitmap with target box size
        Bitmap result = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);
        result.SetResolution(img.HorizontalResolution, img.VerticalResolution);

        using (Graphics g = Graphics.FromImage(result)) {
            g.Clear(Color.Transparent); // Or Color.White, depending on desired background
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Center inside box
            int offsetX = (targetWidth - newWidth) / 2;
            int offsetY = (targetHeight - newHeight) / 2;

            g.DrawImage(img, offsetX, offsetY, newWidth, newHeight);
        }

        return result;
    }
}
