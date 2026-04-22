using BoomFest.Repositories;

namespace BoomFest.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public Task<int> GetTotalOrdersAsync()
    {
        return _transactionRepository.CountOrdersAsync();
    }
}

