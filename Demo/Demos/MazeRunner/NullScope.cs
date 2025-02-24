namespace Demo.Demos.MazeRunner
{
    // A simple scope that does nothing.
    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose() { }
    }
}
