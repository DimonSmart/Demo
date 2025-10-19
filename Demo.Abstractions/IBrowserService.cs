namespace Demo.Abstractions;

public interface IBrowserService
{
    Task<Dimension> GetPageDimensionsWithoutPaddingAsync(string elementId);
    Task<Dimension> GetElementSizeByIdAsync(string elementId);
}
