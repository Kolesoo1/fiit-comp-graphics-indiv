using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace IndividualWork;

public sealed class DrawingCanvas : Panel
{
    private static readonly Color NeutralColor = Color.FromArgb(55, 75, 90);

    public List<PolygonShape> Polygons { get; } = [];

    public PolygonShape? CurrentPolygon { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int PolygonAIndex { get; set; } = -1;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int PolygonBIndex { get; set; } = -1;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<PointF>? Intersection { get; set; }

    public DrawingCanvas()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        Cursor = Cursors.Cross;
    }

    public void AddPoint(PointF point)
    {
        CurrentPolygon ??= new PolygonShape { Name = $"Полигон {Polygons.Count + 1}" };
        CurrentPolygon.Vertices.Add(point);
    }

    public void RemoveLastPoint()
    {
        if (CurrentPolygon is null)
            return;

        if (CurrentPolygon.Vertices.Count > 0)
            CurrentPolygon.Vertices.RemoveAt(CurrentPolygon.Vertices.Count - 1);

        if (CurrentPolygon.Vertices.Count == 0)
            CurrentPolygon = null;
    }

    public PolygonShape? FinishPolygon()
    {
        if (CurrentPolygon is null || CurrentPolygon.Vertices.Count == 0)
            return null;

        var vertices = CurrentPolygon.Vertices;

        if (vertices.Count > 1 && Distance(vertices[0], vertices[^1]) <= 1e-3f)
            vertices.RemoveAt(vertices.Count - 1);

        if (vertices.Count == 0)
        {
            CurrentPolygon = null;
            return null;
        }

        var polygon = CurrentPolygon;
        CurrentPolygon = null;
        Polygons.Add(polygon);
        return polygon;
    }

    public void ClearScene()
    {
        Polygons.Clear();
        CurrentPolygon = null;
        Intersection = null;
        PolygonAIndex = -1;
        PolygonBIndex = -1;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        DrawIntersection(graphics);
        DrawPolygons(graphics);
    }

    private void DrawIntersection(Graphics graphics)
    {
        if (Intersection is not { Count: > 0 } intersection)
            return;

        if (intersection.Count >= 3)
        {
            using var buffer = new Bitmap(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));
            PolygonFill.Fill(buffer, intersection, Color.FromArgb(110, 220, 70, 70));
            graphics.DrawImageUnscaled(buffer, 0, 0);

            using var outline = new Pen(Color.Crimson, 2.5f) { LineJoin = LineJoin.Round };
            graphics.DrawPolygon(outline, intersection.ToArray());
        }

        using var marker = new SolidBrush(Color.Crimson);
        foreach (var point in intersection)
            graphics.FillEllipse(marker, point.X - 3f, point.Y - 3f, 6f, 6f);
    }

    private void DrawPolygons(Graphics graphics)
    {
        for (var i = 0; i < Polygons.Count; i++)
        {
            var color = i == PolygonAIndex
                ? Color.RoyalBlue
                : i == PolygonBIndex
                    ? Color.ForestGreen
                    : NeutralColor;

            DrawPolygon(graphics, Polygons[i], color, closed: true);
        }

        if (CurrentPolygon is not null)
            DrawPolygon(graphics, CurrentPolygon, Color.DarkOrange, closed: false);
    }

    private static void DrawPolygon(Graphics graphics, PolygonShape polygon, Color color, bool closed)
    {
        var vertices = polygon.Vertices;
        if (vertices.Count == 0)
            return;

        using var pen = new Pen(color, 2f) { LineJoin = LineJoin.Round };

        if (vertices.Count >= 2)
        {
            if (closed)
                graphics.DrawPolygon(pen, vertices.ToArray());
            else
                graphics.DrawLines(pen, vertices.ToArray());
        }

        using var brush = new SolidBrush(color);
        foreach (var vertex in vertices)
            graphics.FillEllipse(brush, vertex.X - 4f, vertex.Y - 4f, 8f, 8f);
    }

    private static float Distance(PointF a, PointF b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
