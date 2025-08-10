namespace APIServer.Repository;

public interface IMasterDb
{
    public Task<bool> Load();
}