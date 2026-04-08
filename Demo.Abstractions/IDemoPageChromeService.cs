namespace Demo.Abstractions;

public interface IDemoPageChromeService
{
    DemoPageDescriptor Current { get; }

    event Func<DemoPageDescriptor, Task>? CurrentChanged;

    void SetCurrent(DemoPageDescriptor descriptor);
}
