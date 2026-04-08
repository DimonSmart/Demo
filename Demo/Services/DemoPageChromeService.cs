using Demo.Abstractions;

namespace Demo.Services
{
    public class DemoPageChromeService : IDemoPageChromeService
    {
        public event Func<DemoPageDescriptor, Task>? CurrentChanged;

        private DemoPageDescriptor _current = new(string.Empty, PageSurfaceMode.Utility);

        public DemoPageDescriptor Current
        {
            get => _current;
            private set
            {
                _current = value;
                NotifyCurrentChanged(_current);
            }
        }

        public void SetCurrent(DemoPageDescriptor descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);

            if (_current == descriptor)
            {
                return;
            }

            Current = descriptor;
        }

        private void NotifyCurrentChanged(DemoPageDescriptor descriptor)
        {
            if (CurrentChanged != null)
            {
                _ = CurrentChanged.Invoke(descriptor);
            }
        }
    }
}
