namespace Demo.Abstractions;

public sealed record DemoPageDescriptor(
    string Title,
    PageSurfaceMode Mode = PageSurfaceMode.Utility,
    string? EyebrowText = null);
