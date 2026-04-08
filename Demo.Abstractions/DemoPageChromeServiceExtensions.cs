namespace Demo.Abstractions;

public static class DemoPageChromeServiceExtensions
{
    public static void SetPage(
        this IDemoPageChromeService pageChrome,
        string title,
        PageSurfaceMode mode = PageSurfaceMode.Utility,
        string? eyebrowText = null)
    {
        ArgumentNullException.ThrowIfNull(pageChrome);
        pageChrome.SetCurrent(new DemoPageDescriptor(title, mode, eyebrowText));
    }
}
