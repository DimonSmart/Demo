namespace Demo.Services
{
    public class PageTitleService
    {
        public event Func<string, Task>? TitleChanged;

        private string _title = string.Empty;

        public string Title
        {
            get => _title;
            private set
            {
                _title = value;
                NotifyTitleChanged(_title);
            }
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        private void NotifyTitleChanged(string title)
        {
            if (TitleChanged != null)
                _ = TitleChanged.Invoke(title);
        }
    }
}