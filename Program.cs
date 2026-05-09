using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Themes.Fluent;

BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();

class App : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new GameWindow();
        base.OnFrameworkInitializationCompleted();
    }
}

class GameWindow : Window
{
    readonly BoardControl _board = new();
    readonly TextBlock _status = new()
    {
        FontSize = 18,
        FontWeight = FontWeight.Bold,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Thickness(0, 10, 0, 0)
    };

    public GameWindow()
    {
        Title = "Tic Tac Toe";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        Background = new SolidColorBrush(Color.Parse("#1E1E2E"));

        var restart = new Button
        {
            Content = "Restart",
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(28, 10),
            FontSize = 14,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 6, 0, 16)
        };
        restart.Click += (_, _) => { _board.Restart(); Refresh(); };

        var layout = new StackPanel();
        layout.Children.Add(_board);
        layout.Children.Add(_status);
        layout.Children.Add(restart);
        Content = layout;

        _board.StateChanged += Refresh;
        Refresh();
    }

    void Refresh()
    {
        if (_board.GameOver)
        {
            bool draw = _board.WinLine.Length == 0;
            _status.Text = draw ? "Draw!" : $"Player {_board.Current} wins!";
            _status.Foreground = new SolidColorBrush(Color.Parse(draw ? "#F9E2AF" : "#A6E3A1"));
        }
        else
        {
            _status.Text = $"Player {_board.Current}'s turn";
            _status.Foreground = new SolidColorBrush(
                Color.Parse(_board.Current == 'X' ? "#F38BA8" : "#89B4FA"));
        }
    }
}

class BoardControl : Control
{
    const int Cell = 160;
    const int Pad = 20;

    readonly char[] _cells = new char[9];
    public char Current { get; private set; } = 'X';
    public bool GameOver { get; private set; }
    public int[] WinLine { get; private set; } = [];

    public event Action? StateChanged;

    public BoardControl()
    {
        Width = Cell * 3 + Pad * 2;
        Height = Cell * 3 + Pad * 2;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public void Restart()
    {
        Array.Clear(_cells);
        Current = 'X';
        GameOver = false;
        WinLine = [];
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (GameOver) return;

        var pt = e.GetPosition(this);
        int col = (int)((pt.X - Pad) / Cell);
        int row = (int)((pt.Y - Pad) / Cell);
        if (col < 0 || col > 2 || row < 0 || row > 2) return;

        int idx = row * 3 + col;
        if (_cells[idx] != '\0') return;

        _cells[idx] = Current;

        if (CheckWinner(out int[] line))
        {
            WinLine = line;
            GameOver = true;
        }
        else if (Array.TrueForAll(_cells, c => c != '\0'))
        {
            GameOver = true;
        }
        else
        {
            Current = Current == 'X' ? 'O' : 'X';
        }

        InvalidateVisual();
        StateChanged?.Invoke();
    }

    bool CheckWinner(out int[] line)
    {
        int[][] lines =
        [
            [0, 1, 2], [3, 4, 5], [6, 7, 8],
            [0, 3, 6], [1, 4, 7], [2, 5, 8],
            [0, 4, 8], [2, 4, 6]
        ];
        foreach (var l in lines)
        {
            if (_cells[l[0]] != '\0' && _cells[l[0]] == _cells[l[1]] && _cells[l[1]] == _cells[l[2]])
            {
                line = l;
                return true;
            }
        }
        line = [];
        return false;
    }

    public override void Render(DrawingContext ctx)
    {
        ctx.FillRectangle(new SolidColorBrush(Color.Parse("#1E1E2E")), new Rect(0, 0, Width, Height));

        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#585B70")), 4,
            lineCap: PenLineCap.Round);

        for (int i = 1; i < 3; i++)
        {
            double x = Pad + i * Cell;
            ctx.DrawLine(gridPen, new Point(x, Pad), new Point(x, Pad + Cell * 3));
            double y = Pad + i * Cell;
            ctx.DrawLine(gridPen, new Point(Pad, y), new Point(Pad + Cell * 3, y));
        }

        int p = 28;
        for (int i = 0; i < 9; i++)
        {
            if (_cells[i] == '\0') continue;
            double x = Pad + i % 3 * Cell;
            double y = Pad + i / 3 * Cell;

            if (_cells[i] == 'X')
            {
                var pen = new Pen(new SolidColorBrush(Color.Parse("#F38BA8")), 8,
                    lineCap: PenLineCap.Round);
                ctx.DrawLine(pen, new Point(x + p, y + p), new Point(x + Cell - p, y + Cell - p));
                ctx.DrawLine(pen, new Point(x + Cell - p, y + p), new Point(x + p, y + Cell - p));
            }
            else
            {
                var pen = new Pen(new SolidColorBrush(Color.Parse("#89B4FA")), 8);
                double r = Cell / 2.0 - p;
                ctx.DrawEllipse(null, pen, new Point(x + Cell / 2.0, y + Cell / 2.0), r, r);
            }
        }

        if (WinLine.Length == 3)
        {
            double Cx(int i) => Pad + i % 3 * Cell + Cell / 2.0;
            double Cy(int i) => Pad + i / 3 * Cell + Cell / 2.0;
            var pen = new Pen(new SolidColorBrush(Color.Parse("#A6E3A1")), 6,
                lineCap: PenLineCap.Round);
            ctx.DrawLine(pen,
                new Point(Cx(WinLine[0]), Cy(WinLine[0])),
                new Point(Cx(WinLine[2]), Cy(WinLine[2])));
        }
    }
}
