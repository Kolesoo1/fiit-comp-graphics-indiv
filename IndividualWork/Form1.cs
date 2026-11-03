namespace IndividualWork;

public partial class Form1 : Form
{
    private readonly PolygonIntersectionService intersectionService = new();

    public Form1()
    {
        InitializeComponent();

        polygonA.SelectedIndexChanged += PolygonSelectionChanged;
        polygonB.SelectedIndexChanged += PolygonSelectionChanged;
    }

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            canvasPanel.AddPoint(e.Location);
        else if (e.Button == MouseButtons.Right)
            canvasPanel.RemoveLastPoint();
        else
            return;

        canvasPanel.Invalidate();
        UpdateStatus();
    }

    private void FinishPolygon_Click(object? sender, EventArgs e)
    {
        if (canvasPanel.FinishPolygon() is not { } polygon)
        {
            status.Text = "Поставьте хотя бы одну точку, затем завершите полигон.";
            return;
        }

        var index = canvasPanel.Polygons.Count - 1;
        polygonA.Items.Add(polygon.Name);
        polygonB.Items.Add(polygon.Name);

        if (polygonA.SelectedIndex < 0)
            polygonA.SelectedIndex = index;

        if (polygonB.SelectedIndex < 0 && polygonA.SelectedIndex != index)
            polygonB.SelectedIndex = index;

        canvasPanel.Invalidate();
        UpdateStatus();
    }

    private void UndoPoint_Click(object? sender, EventArgs e)
    {
        canvasPanel.RemoveLastPoint();
        canvasPanel.Invalidate();
        UpdateStatus();
    }

    private void ClearScene_Click(object? sender, EventArgs e)
    {
        canvasPanel.ClearScene();
        polygonA.Items.Clear();
        polygonB.Items.Clear();
        status.Text = "Сцена очищена. Щёлкайте по холсту, чтобы нарисовать полигон.";
    }

    private void Intersect_Click(object? sender, EventArgs e)
    {
        var first = SelectedPolygon(polygonA);
        var second = SelectedPolygon(polygonB);

        if (first is null || second is null)
        {
            status.Text = "Выберите оба полигона в выпадающих списках.";
            return;
        }

        var firstConvex = intersectionService.IsConvex(first);
        var secondConvex = intersectionService.IsConvex(second);
        if (!firstConvex || !secondConvex)
        {
            canvasPanel.Intersection = null;
            canvasPanel.Invalidate();
            if (!firstConvex && !secondConvex)
                status.Text = $"Ошибка: полигон «{first.Name}» и полигон «{second.Name}» невыпуклые. Пересечение поддерживается только для выпуклых полигонов.";
            else if (!firstConvex)
                status.Text = $"Ошибка: полигон «{first.Name}» невыпуклый. Пересечение поддерживается только для выпуклых полигонов.";
            else
                status.Text = $"Ошибка: полигон «{second.Name}» невыпуклый. Пересечение поддерживается только для выпуклых полигонов.";
            return;
        }

        var result = intersectionService.Intersect(first, second);
        canvasPanel.Intersection = result;
        canvasPanel.Invalidate();

        status.Text = result.Count switch
        {
            0 => $"Пересечение «{first.Name}» и «{second.Name}» отсутствует.",
            1 => "Пересечение: одна точка.",
            2 => "Пересечение: отрезок (2 точки).",
            _ => $"Пересечение: {result.Count} вершин."
        };
    }

    private void ResetIntersection_Click(object? sender, EventArgs e)
    {
        canvasPanel.Intersection = null;
        canvasPanel.Invalidate();
        UpdateStatus();
    }

    private void PolygonSelectionChanged(object? sender, EventArgs e)
    {
        canvasPanel.PolygonAIndex = polygonA.SelectedIndex;
        canvasPanel.PolygonBIndex = polygonB.SelectedIndex;
        canvasPanel.Intersection = null;
        canvasPanel.Invalidate();
        UpdateStatus();
    }

    private PolygonShape? SelectedPolygon(ComboBox combo)
    {
        var index = combo.SelectedIndex;
        return index >= 0 && index < canvasPanel.Polygons.Count
            ? canvasPanel.Polygons[index]
            : null;
    }

    private void UpdateStatus()
    {
        var current = canvasPanel.CurrentPolygon?.Vertices.Count ?? 0;
        var first = SelectedPolygon(polygonA)?.Name ?? "—";
        var second = SelectedPolygon(polygonB)?.Name ?? "—";

        status.Text =
            $"Полигонов: {canvasPanel.Polygons.Count}. " +
            $"Текущий полигон: {current} точек. " +
            $"Выбрано: {first} ∩ {second}.";
    }
}
