namespace IndividualWork;

public sealed class PolygonIntersectionService
{
    private const float Epsilon = 1e-4f;
    private const float PointTolerance = 1e-3f;

    public bool IsConvex(PolygonShape polygon)
    {
        var vertices = polygon.Vertices;
        if (vertices.Count <= 3)
            return true;

        int sign = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];
            var c = vertices[(i + 2) % vertices.Count];
            var cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            if (Math.Abs(cross) <= PointTolerance)
                continue;
            int currentSign = cross > 0 ? 1 : -1;
            if (sign == 0)
                sign = currentSign;
            else if (sign != currentSign)
                return false;
        }
        return true;
    }

    public List<PointF> Intersect(PolygonShape first, PolygonShape second)
    {
        if (first.Vertices.Count <= 0 || second.Vertices.Count <= 0)
            return new List<PointF>();

        if (first.Vertices.Count == 1 && second.Vertices.Count == 1)
        {
            var result = new List<PointF>();
            if (Distance(first.Vertices[0], second.Vertices[0]) <= PointTolerance)
                result.Add(first.Vertices[0]);
            return result;
        }

        if (first.Vertices.Count == 1)
            return IntersectPointPolygon(first.Vertices[0], second);

        if (second.Vertices.Count == 1)
            return IntersectPointPolygon(second.Vertices[0], first);

        if (first.Vertices.Count == 2 && second.Vertices.Count == 2)
            return IntersectSegmentSegment(first.Vertices[0], first.Vertices[1], second.Vertices[0], second.Vertices[1]);

        if (first.Vertices.Count == 2)
            return IntersectSegmentPolygon(first.Vertices[0], first.Vertices[1], second);

        if (second.Vertices.Count == 2)
            return IntersectSegmentPolygon(second.Vertices[0], second.Vertices[1], first);

        var output = new List<PointF>(first.Vertices);
        var clip = second.Vertices;
        var counterClockwise = SignedArea(clip) >= 0f;

        for (var i = 0; i < clip.Count && output.Count > 0; i++)
        {
            var edge = new Segment(clip[i], clip[(i + 1) % clip.Count]);
            output = ClipByEdge(output, edge, counterClockwise);
        }

        return RemoveDuplicates(output);
    }

    private static List<PointF> ClipByEdge(List<PointF> input, Segment edge, bool counterClockwise)
    {
        var output = new List<PointF>(input.Count + 1);

        for (var i = 0; i < input.Count; i++)
        {
            var current = input[i];
            var previous = input[(i - 1 + input.Count) % input.Count];

            var currentInside = IsInside(edge, current, counterClockwise);
            var previousInside = IsInside(edge, previous, counterClockwise);

            if (currentInside)
            {
                if (!previousInside)
                    output.Add(IntersectLines(previous, current, edge));
                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(IntersectLines(previous, current, edge));
            }
        }

        return output;
    }

    private static bool IsInside(Segment edge, PointF point, bool counterClockwise)
    {
        var side = ClassifyPoint(edge, point);
        return counterClockwise ? side != PointSide.Right : side != PointSide.Left;
    }

    public static PointSide ClassifyPoint(Segment edge, PointF point)
    {
        var cross = Cross(edge.A, edge.B, point);
        if (cross > Epsilon)
            return PointSide.Left;
        if (cross < -Epsilon)
            return PointSide.Right;
        return PointSide.OnSegment;
    }

    private static PointF IntersectLines(PointF p, PointF q, Segment edge)
    {
        var a = edge.A;
        var b = edge.B;
        var rx = b.X - a.X;
        var ry = b.Y - a.Y;
        var sx = q.X - p.X;
        var sy = q.Y - p.Y;

        var denominator = rx * sy - ry * sx;
        if (Math.Abs(denominator) < Epsilon)
            return q;

        var t = ((p.X - a.X) * sy - (p.Y - a.Y) * sx) / denominator;
        return new PointF(a.X + t * rx, a.Y + t * ry);
    }

    private static List<PointF> FilterInside(IReadOnlyList<PointF> points, PolygonShape polygon)
    {
        var result = new List<PointF>();
        foreach (var point in points)
        {
            if (ContainsPoint(polygon, point))
                result.Add(point);
        }

        return RemoveDuplicates(result);
    }

    public static bool ContainsPoint(PolygonShape polygon, PointF point)
    {
        var vertices = polygon.Vertices;

        if (vertices.Count == 0)
            return false;
        if (vertices.Count == 1)
            return Distance(vertices[0], point) <= PointTolerance;
        if (vertices.Count == 2)
            return IsOnSegment(vertices[0], vertices[1], point);

        for (var i = 0; i < vertices.Count; i++)
        {
            if (IsOnSegment(vertices[i], vertices[(i + 1) % vertices.Count], point))
                return true;
        }

        var inside = false;
        for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
        {
            var a = vertices[i];
            var b = vertices[j];

            if ((a.Y > point.Y) == (b.Y > point.Y))
                continue;

            var t = (point.Y - a.Y) / (b.Y - a.Y);
            if (point.X < a.X + t * (b.X - a.X))
                inside = !inside;
        }

        return inside;
    }

    private static bool IsOnSegment(PointF a, PointF b, PointF point)
    {
        if (Math.Abs(Cross(a, b, point)) > PointTolerance)
            return false;

        return point.X >= Math.Min(a.X, b.X) - PointTolerance
            && point.X <= Math.Max(a.X, b.X) + PointTolerance
            && point.Y >= Math.Min(a.Y, b.Y) - PointTolerance
            && point.Y <= Math.Max(a.Y, b.Y) + PointTolerance;
    }

    private static float Cross(PointF a, PointF b, PointF point) =>
        (b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X);

    private static float SignedArea(IReadOnlyList<PointF> vertices)
    {
        float area = 0f;
        for (var i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];
            area += a.X * b.Y - b.X * a.Y;
        }

        return area * 0.5f;
    }

    private static List<PointF> RemoveDuplicates(List<PointF> points)
    {
        var result = new List<PointF>(points.Count);
        foreach (var point in points)
        {
            if (result.Count == 0 || Distance(result[^1], point) > PointTolerance)
                result.Add(point);
        }

        if (result.Count > 1 && Distance(result[0], result[^1]) <= PointTolerance)
            result.RemoveAt(result.Count - 1);

        return result;
    }

    private static List<PointF> IntersectPointPolygon(PointF point, PolygonShape polygon)
    {
        var result = new List<PointF>();
        if (ContainsPoint(polygon, point))
            result.Add(point);
        return result;
    }

    private static List<PointF> IntersectSegmentPolygon(PointF p1, PointF p2, PolygonShape polygon)
    {
        var result = new List<PointF>();

        if (ContainsPoint(polygon, p1))
            AddIfMissing(result, p1);
        if (ContainsPoint(polygon, p2))
            AddIfMissing(result, p2);

        var vertices = polygon.Vertices;
        for (var i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];

            if (SegmentsIntersectProperOrBoundary(p1, p2, a, b, out var pt))
                AddIfMissing(result, pt);
        }

        result.Sort((x, y) => Projection(p1, p2, x).CompareTo(Projection(p1, p2, y)));
        return RemoveDuplicates(result);
    }

    private static List<PointF> IntersectSegmentSegment(PointF p1, PointF p2, PointF q1, PointF q2)
    {
        var result = new List<PointF>();

        if (PointOnSegment(p1, q1, q2))
            AddIfMissing(result, p1);
        if (PointOnSegment(p2, q1, q2))
            AddIfMissing(result, p2);
        if (PointOnSegment(q1, p1, p2))
            AddIfMissing(result, q1);
        if (PointOnSegment(q2, p1, p2))
            AddIfMissing(result, q2);

        if (result.Count == 0)
        {
            if (SegmentsIntersectProper(p1, p2, q1, q2, out var pt))
                result.Add(pt);
        }

        result.Sort((x, y) => Projection(p1, p2, x).CompareTo(Projection(p1, p2, y)));
        return RemoveDuplicates(result);
    }

    private static bool SegmentsIntersectProperOrBoundary(PointF p1, PointF p2, PointF q1, PointF q2, out PointF point)
    {
        point = default;

        int o1 = Orientation(p1, p2, q1);
        int o2 = Orientation(p1, p2, q2);
        int o3 = Orientation(q1, q2, p1);
        int o4 = Orientation(q1, q2, p2);

        if (o1 != 0 && o2 != 0 && o3 != 0 && o4 != 0 && o1 != o2 && o3 != o4)
        {
            point = LineIntersectionPoint(p1, p2, q1, q2);
            return true;
        }

        if (o1 == 0 && PointOnSegment(p1, q1, p2)) { point = q1; return true; }
        if (o2 == 0 && PointOnSegment(p1, q2, p2)) { point = q2; return true; }
        if (o3 == 0 && PointOnSegment(q1, p1, q2)) { point = p1; return true; }
        if (o4 == 0 && PointOnSegment(q1, p2, q2)) { point = p2; return true; }

        return false;
    }

    private static bool SegmentsIntersectProper(PointF p1, PointF p2, PointF q1, PointF q2, out PointF point)
    {
        point = default;

        int o1 = Orientation(p1, p2, q1);
        int o2 = Orientation(p1, p2, q2);
        int o3 = Orientation(q1, q2, p1);
        int o4 = Orientation(q1, q2, p2);

        if (o1 != 0 && o2 != 0 && o3 != 0 && o4 != 0 && o1 != o2 && o3 != o4)
        {
            point = LineIntersectionPoint(p1, p2, q1, q2);
            return true;
        }

        return false;
    }

    private static PointF LineIntersectionPoint(PointF p1, PointF p2, PointF q1, PointF q2)
    {
        var rx = p2.X - p1.X;
        var ry = p2.Y - p1.Y;
        var sx = q2.X - q1.X;
        var sy = q2.Y - q1.Y;
        var denom = rx * sy - ry * sx;

        if (Math.Abs(denom) < Epsilon)
            return q1;

        var t = ((q1.X - p1.X) * sy - (q1.Y - p1.Y) * sx) / denom;
        return new PointF(p1.X + t * rx, p1.Y + t * ry);
    }

    private static int Orientation(PointF p, PointF q, PointF r)
    {
        var val = Cross(p, q, r);
        if (Math.Abs(val) <= PointTolerance)
            return 0;
        return val > 0 ? 1 : 2;
    }

    private static bool PointOnSegment(PointF p, PointF q, PointF r)
    {
        if (Math.Abs(Cross(p, r, q)) > PointTolerance)
            return false;

        return q.X <= Math.Max(p.X, r.X) + PointTolerance &&
               q.X >= Math.Min(p.X, r.X) - PointTolerance &&
               q.Y <= Math.Max(p.Y, r.Y) + PointTolerance &&
               q.Y >= Math.Min(p.Y, r.Y) - PointTolerance;
    }

    private static float Projection(PointF p1, PointF p2, PointF pt)
    {
        var vx = p2.X - p1.X;
        var vy = p2.Y - p1.Y;
        var wx = pt.X - p1.X;
        var wy = pt.Y - p1.Y;
        return vx * wx + vy * wy;
    }

    private static void AddIfMissing(List<PointF> list, PointF point)
    {
        foreach (var existing in list)
        {
            if (Distance(existing, point) <= PointTolerance)
                return;
        }

        list.Add(point);
    }

    private static float Distance(PointF a, PointF b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}