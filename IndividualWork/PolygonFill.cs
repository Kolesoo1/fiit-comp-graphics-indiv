using FastBitmap;

namespace IndividualWork;

public static class PolygonFill
{
    public static void Fill(Bitmap bitmap, IReadOnlyList<PointF> polygon, Color color)
    {
        if (polygon.Count < 3)
            return;

        var minY = float.MaxValue;
        var maxY = float.MinValue;
        foreach (var point in polygon)
        {
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);
        }

        var startY = Math.Max(0, (int)MathF.Ceiling(minY - 0.5f));
        var endY = Math.Min(bitmap.Height - 1, (int)MathF.Floor(maxY - 0.5f));
        if (startY > endY)
            return;

        using var fast = new FastBitmap.FastBitmap(bitmap);
        var intersections = new List<float>(polygon.Count);

        for (var y = startY; y <= endY; y++)
        {
            var scanY = y + 0.5f;
            intersections.Clear();

            for (var i = 0; i < polygon.Count; i++)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % polygon.Count];

                if ((a.Y <= scanY && b.Y > scanY) || (b.Y <= scanY && a.Y > scanY))
                {
                    var t = (scanY - a.Y) / (b.Y - a.Y);
                    intersections.Add(a.X + t * (b.X - a.X));
                }
            }

            intersections.Sort();

            for (var i = 0; i + 1 < intersections.Count; i += 2)
            {
                var fromX = Math.Max(0, (int)MathF.Ceiling(intersections[i] - 0.5f));
                var toX = Math.Min(bitmap.Width - 1, (int)MathF.Floor(intersections[i + 1] - 0.5f));

                for (var x = fromX; x <= toX; x++)
                    fast[x, y] = color;
            }
        }
    }
}
