namespace Demo.Demos.Pdd;

public interface IPddDataService
{
    Task<PddDatabase> LoadDatabaseAsync();
}
