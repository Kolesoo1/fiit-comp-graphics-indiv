namespace IndividualWork;

public sealed class PolygonShape
{
    public required string Name { get; init; }
    public List<PointF> Vertices { get; } = [];
}

public readonly record struct Segment(PointF A, PointF B);

public enum PointSide
{
    Left,
    Right,
    OnSegment
}
