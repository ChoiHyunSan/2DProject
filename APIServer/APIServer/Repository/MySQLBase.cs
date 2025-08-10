using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace APIServer.Repository;

public abstract class MySQLBase
{
    private string _dbConfig;
    private MySqlConnection _conn;
    protected QueryFactory _queryFactory;

    protected MySQLBase(string dbConfig)
    {
        _dbConfig = dbConfig;

        Open();
        
        var compiler = new MySqlCompiler();
        _queryFactory = new QueryFactory(_conn, compiler);
    }

    public void Dispose()
    {
        Close();
    }
    
    private void Open()
    {
        _conn = new MySqlConnection(_dbConfig);
        _conn.Open();
    }

    private void Close()
    {
        _conn.Close();
    }
}