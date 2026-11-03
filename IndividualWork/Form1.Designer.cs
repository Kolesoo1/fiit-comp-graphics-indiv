#nullable enable

namespace IndividualWork;

partial class Form1
{
    private System.ComponentModel.IContainer? components;
    private DrawingCanvas canvasPanel = null!;
    private ComboBox polygonA = null!;
    private ComboBox polygonB = null!;
    private Button finishButton = null!;
    private Button undoButton = null!;
    private Button clearButton = null!;
    private Button intersectButton = null!;
    private Button resetButton = null!;
    private Label status = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        ClientSize = new Size(1120, 700);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Индивидуальная работа — пересечение выпуклых полигонов";
        BackColor = SystemColors.Control;
        Font = new Font("Segoe UI", 9F);
        Controls.Add(BuildLayout());

        ResumeLayout(true);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(6)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var sidebar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        sidebar.Controls.Add(SectionTitle("Рисование"));

        finishButton = WideButton("Завершить полигон");
        finishButton.Click += FinishPolygon_Click;
        undoButton = WideButton("Удалить последнюю точку");
        undoButton.Click += UndoPoint_Click;
        clearButton = WideButton("Очистить всё");
        clearButton.Click += ClearScene_Click;

        sidebar.Controls.Add(finishButton);
        sidebar.Controls.Add(undoButton);
        sidebar.Controls.Add(clearButton);

        sidebar.Controls.Add(SectionTitle("Пересечение"));

        polygonA = new ComboBox
        {
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3, 2, 3, 6)
        };
        polygonB = new ComboBox
        {
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3, 2, 3, 6)
        };

        sidebar.Controls.Add(FieldLabel("Полигон 1"));
        sidebar.Controls.Add(polygonA);
        sidebar.Controls.Add(FieldLabel("Полигон 2"));
        sidebar.Controls.Add(polygonB);

        intersectButton = WideButton("Найти пересечение");
        intersectButton.Click += Intersect_Click;
        resetButton = WideButton("Сбросить пересечение");
        resetButton.Click += ResetIntersection_Click;

        sidebar.Controls.Add(intersectButton);
        sidebar.Controls.Add(resetButton);

        sidebar.Controls.Add(new Label
        {
            AutoSize = false,
            Width = 220,
            Height = 78,
            ForeColor = SystemColors.GrayText,
            Text = "ЛКМ — добавить точку.\nПКМ — удалить последнюю точку.\n«Завершить полигон» соединяет последнюю точку с первой."
        });

        canvasPanel = new DrawingCanvas
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };
        canvasPanel.MouseDown += Canvas_MouseDown;

        status = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            Padding = new Padding(4),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "Щёлкайте по холсту, чтобы нарисовать полигон."
        };

        body.Controls.Add(sidebar, 0, 0);
        body.Controls.Add(canvasPanel, 1, 0);

        root.Controls.Add(body, 0, 0);
        root.Controls.Add(status, 0, 1);
        return root;
    }

    private static Label SectionTitle(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Width = 220,
        Height = 28,
        Font = new Font("Segoe UI Semibold", 10F),
        Margin = new Padding(3, 10, 3, 2)
    };

    private static Label FieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Width = 220,
        Height = 22,
        TextAlign = ContentAlignment.BottomLeft,
        Margin = new Padding(3, 2, 3, 2)
    };

    private static Button WideButton(string text) => new()
    {
        Text = text,
        Width = 220,
        Height = 30,
        AutoSize = false,
        UseVisualStyleBackColor = true,
        Margin = new Padding(3, 4, 3, 2)
    };
}
