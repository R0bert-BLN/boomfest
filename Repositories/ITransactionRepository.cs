namespace BoomFest.Repositories;

public interface ITransactionRepository
{
    Task<int> CountOrdersAsync();
}

