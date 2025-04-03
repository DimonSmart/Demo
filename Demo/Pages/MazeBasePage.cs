using Demo.Services;
using DimonSmart.MazeGenerator;
using Microsoft.AspNetCore.Components;

namespace Demo.Pages;

public abstract class MazeBaseComponent<TCell> : ComponentBase where TCell : ICell
{
    [Inject]
    public required BrowserService BrowserService { get; set; }

    private int _xSize = 31;
    protected int XSize
    {
        get => _xSize;
        set
        {
            if (value % 2 == 0) return;
            if (_xSize != value)
            {
                _xSize = value;
                if (!IsSlowVisualization) _ = GenerateMazeAsync();
            }
        }
    }

    private int _ySize = 21;
    protected int YSize
    {
        get => _ySize;
        set
        {
            if (value % 2 == 0) return;
            if (_ySize != value)
            {
                _ySize = value;
                if (!IsSlowVisualization) _ = GenerateMazeAsync();
            }
        }
    }

    private int _emptiness = 0;
    protected int Emptiness
    {
        get => _emptiness;
        set
        {
            if (_emptiness != value)
            {
                _emptiness = value;
                if (!IsSlowVisualization) _ = GenerateMazeAsync();
            }
        }
    }

    private double _wallShortness = 5.0;
    protected double WallShortness
    {
        get => _wallShortness;
        set
        {
            _wallShortness = value;
            if (!IsSlowVisualization) _ = GenerateMazeAsync();
        }
    }

    protected bool IsSlowVisualization { get; set; }

    protected IMaze<TCell>? _maze;

    public abstract string GetMazeContainerId();
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // var pageSizes = await BrowserService.GetPageDimensionsWithoutPaddingAsync(GetMazeContainerId());

                var mazeArea = await BrowserService.GetElementSizeByIdAsync(GetMazeContainerId());
                XSize = Math.Max(11, (mazeArea.Width - 21 * 2) / 21) | 1;
                YSize = Math.Max(11, (mazeArea.Height - 21 * 2) / 21) | 1;
            }
            catch (Exception)
            {
            }
        }
    }

    protected abstract Task GenerateMazeAsync();

    protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    protected void CancelGeneration()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource = new CancellationTokenSource();
    }
}
