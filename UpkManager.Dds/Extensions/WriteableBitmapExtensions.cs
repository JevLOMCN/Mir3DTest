using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UpkManager.Dds.Extensions {

  internal static class WriteableBitmapExtensions {

        public static WriteableBitmap ResizeHighQuality(this BitmapSource source, int width, int height)
        {
            var rect = new Rect(0, 0, width, height);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
                context.DrawDrawing(group);

            // Render at Pbgra32 and wrap in WriteableBitmap
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            return new WriteableBitmap(rtb);
        }

        public static byte[] ConvertToRgba(this WriteableBitmap source)
        {
            // Ensure Pbgra32 for predictable bytes
            if (source.Format != PixelFormats.Pbgra32)
            {
                var converted = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
                source = new WriteableBitmap(converted);
            }

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int bpp = source.Format.BitsPerPixel; // 32 for Pbgra32
            int stride = (width * bpp + 7) / 8;

            var bgra = new byte[height * stride];
            source.CopyPixels(bgra, stride, 0);

            var rgba = new byte[width * height * 4];
            int j = 0;

            for (int i = 0; i < bgra.Length; i += 4)
            {
                byte b = bgra[i + 0];
                byte g = bgra[i + 1];
                byte r = bgra[i + 2];
                byte a = bgra[i + 3];

                // Un-premultiply (from Pbgra32 to straight)
                if (a != 0)
                {
                    r = (byte)Math.Min(255, (255 * r + (a / 2)) / a);
                    g = (byte)Math.Min(255, (255 * g + (a / 2)) / a);
                    b = (byte)Math.Min(255, (255 * b + (a / 2)) / a);
                }

                rgba[j++] = r;
                rgba[j++] = g;
                rgba[j++] = b;
                rgba[j++] = a;
            }

            return rgba;
        }
    }
}
