using System.Drawing.Imaging;

namespace FastBitmap;

public unsafe class FastBitmap : IDisposable
{
    public readonly int Width;
    public readonly int Height;
    public int Count => Height * Width;

    private readonly Bitmap source;
    private readonly BitmapData bitmapData;
    private readonly int bytesPerPixel;
    private readonly int stride;
    private readonly byte* scan0;

    private const PixelFormat TargetPixelFormat = PixelFormat.Format32bppArgb;

    public FastBitmap(Bitmap bitmap)
    {
        Width = bitmap.Width;
        Height = bitmap.Height;
        source = bitmap;
        bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite,
            TargetPixelFormat);
        stride = bitmapData.Stride;
        bytesPerPixel = Image.GetPixelFormatSize(TargetPixelFormat) / 8;
        scan0 = (byte*)bitmapData.Scan0.ToPointer();
    }

    private byte* PixelOffset(Point point) =>
        scan0 + point.Y * stride + point.X * bytesPerPixel;

    public void SetPixel(Point point, Color color)
    {
        var data = PixelOffset(point);
        data[Channel.A] = color.A;
        data[Channel.R] = color.R;
        data[Channel.G] = color.G;
        data[Channel.B] = color.B;
    }

    public Color GetPixel(Point point)
    {
        var data = PixelOffset(point);
        return Color.FromArgb(
            data[Channel.A],
            data[Channel.R],
            data[Channel.G],
            data[Channel.B]);
    }

    public Color this[int x, int y]
    {
        get => GetPixel(new Point(x, y));
        set => SetPixel(new Point(x, y), value);
    }

    public void Dispose() => source.UnlockBits(bitmapData);

    private static class Channel
    {
        public const int A = 3;
        public const int R = 2;
        public const int G = 1;
        public const int B = 0;
    }
}
