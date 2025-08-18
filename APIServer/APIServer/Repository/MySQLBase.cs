using System.Data;
using System.Transactions;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using IsolationLevel = System.Data.IsolationLevel;

namespace APIServer.Repository;

public abstract class MySQLBase
{
    private string _dbConfig;
    protected MySqlConnection _conn;
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

    protected void EnsureOpen()
    {
        if (_conn.State != ConnectionState.Open)
            _conn.Open();
    }
    

    // 비동기, 반환값 없음
    protected async Task<Result> WithTransactionAsync(
        Func<QueryFactory, Task<ErrorCode>> action,
        IsolationLevel isolation = IsolationLevel.ReadCommitted)
    {
        var txOptions = new TransactionOptions
        {
            IsolationLevel = MapIsolation(isolation),
            Timeout = TransactionManager.DefaultTimeout
        };

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            txOptions,
            TransactionScopeAsyncFlowOption.Enabled);
        
        EnsureOpen();
        _conn.EnlistTransaction(Transaction.Current!); 

        var ec = await action(_queryFactory);

        if (ec == ErrorCode.None)
            scope.Complete(); 

        return ec;
    }

    // 비동기, 반환값 있음
    protected async Task<TResult> WithTransactionAsync<TResult>(
        Func<QueryFactory, Task<TResult>> func,
        IsolationLevel isolation = IsolationLevel.ReadCommitted)
    {
        EnsureOpen();

        var txOptions = new TransactionOptions
        {
            IsolationLevel = MapIsolation(isolation),
            Timeout = TransactionManager.DefaultTimeout
        };

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            txOptions,
            TransactionScopeAsyncFlowOption.Enabled);
        var result = await func(_queryFactory);
        scope.Complete();
        return result;
    }
    
    protected static System.Transactions.IsolationLevel MapIsolation(IsolationLevel iso) =>
        iso switch
        {
            IsolationLevel.ReadUncommitted => System.Transactions.IsolationLevel.ReadUncommitted,
            IsolationLevel.ReadCommitted   => System.Transactions.IsolationLevel.ReadCommitted,
            IsolationLevel.RepeatableRead  => System.Transactions.IsolationLevel.RepeatableRead,
            IsolationLevel.Serializable    => System.Transactions.IsolationLevel.Serializable,
            IsolationLevel.Snapshot        => System.Transactions.IsolationLevel.Snapshot,
            _ => System.Transactions.IsolationLevel.ReadCommitted
        };
}