namespace Demo.Demos.TSM.GeneralGenetic
{
    public interface IRandomProvider
    {
        int Next();
        int Next(int length);
    }
}