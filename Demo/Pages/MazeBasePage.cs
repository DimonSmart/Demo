using Microsoft.AspNetCore.Components;
using Demo.Services;
using DimonSmart.MazeGenerator;

public abstract class MazeBaseComponent : ComponentBase
{
    [Inject]
    protected BrowserService BrowserService { get; set; }

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

    private double _stopWallProbability = 50.0;
    protected double StopWallProbability
    {
        get => _stopWallProbability;
        set
        {
            if (_stopWallProbability != value)
            {
                _stopWallProbability = value;
                if (!IsSlowVisualization) _ = GenerateMazeAsync();
            }
        }
    }

    private bool _isSlowVisualization = false;
    protected bool IsSlowVisualization
    {
        get => _isSlowVisualization;
        set
        {
            if (_isSlowVisualization != value)
            {
                _isSlowVisualization = value;
            }
        }
    }

    protected IMaze? _maze;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var pageSizes = await BrowserService.GetPageDimensionsWithoutPaddingAsync("mazegeneratordemoid");
                XSize = ((pageSizes.Width - 24 - 24) / 20) | 1;
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
        }
    }

    protected abstract Task GenerateMazeAsync();

    protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    protected void CancelGeneration()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
    }
}
