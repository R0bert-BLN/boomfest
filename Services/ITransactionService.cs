namespace BoomFest.Services;

public interface ITransactionService
{
    Task<int> GetTotalOrdersAsync();
}

