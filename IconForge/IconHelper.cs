using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class IconHelper {
    /// <summary>
    /// Save multiple images as a multi-resolution .ico file.
    /// </summary>
    public static void SaveIcon(string iconFilePath, params Image[] images) {
        if (images == null || images.Length == 0)
            throw new ArgumentException("At least one image must be provided.", nameof(images));

        using (FileStream fs = new FileStream(iconFilePath, FileMode.Create, FileAccess.Write))
        using (BinaryWriter bw = new BinaryWriter(fs)) {
            // ICONDIR (6 bytes)
            bw.Write((short)0);                // Reserved
            bw.Write((short)1);                // Type = 1 (icon)
            bw.Write((short)images.Length);    // Number of images

            long imageDataOffset = 6 + (16 * images.Length); // Header + entries
            MemoryStream[] imageStreams = new MemoryStream[images.Length];

            try {
                // Write entries
                for (int i = 0; i < images.Length; i++) {
                    Bitmap bmp = new Bitmap(images[i]);
                    bmp.SetResolution(96, 96);

                    // Save to PNG
                    MemoryStream ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Png);
                    imageStreams[i] = ms;

                    int width = bmp.Width;
                    int height = bmp.Height;
                    if (width >= 256) width = 0;
                    if (height >= 256) height = 0;

                    bw.Write((byte)width);
                    bw.Write((byte)height);
                    bw.Write((byte)0);              // Palette
                    bw.Write((byte)0);              // Reserved
                    bw.Write((short)1);             // Planes
                    bw.Write((short)32);            // Bits per pixel
                    bw.Write((int)ms.Length);       // Size
                    bw.Write((int)imageDataOffset); // Offset

                    imageDataOffset += ms.Length;
                }

                // Write image data
                for (int i = 0; i < images.Length; i++) {
                    imageStreams[i].WriteTo(fs);
                }
            } finally {
                foreach (var ms in imageStreams)
                    ms?.Dispose();
            }
        }
    }

    /// <summary>
    /// Generate resized versions of a base image and save as .ico.
    /// </summary>
    public static void SaveIcon(string iconFilePath, Image baseImage, params int[] sizes) {
        if (baseImage == null)
            throw new ArgumentNullException(nameof(baseImage));
        if (sizes == null || sizes.Length == 0)
            throw new ArgumentException("At least one size must be provided.", nameof(sizes));

        Image[] scaled = new Image[sizes.Length];
        try {
            for (int i = 0; i < sizes.Length; i++) {
                int size = sizes[i];
                Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                bmp.SetResolution(baseImage.HorizontalResolution, baseImage.VerticalResolution);

                using (Graphics g = Graphics.FromImage(bmp)) {
                    g.Clear(Color.Transparent);
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    g.DrawImage(baseImage, new Rectangle(0, 0, size, size));
                }

                scaled[i] = bmp;
            }

            SaveIcon(iconFilePath, scaled);
        } finally {
            foreach (var img in scaled)
                img?.Dispose();
        }
    }
}
