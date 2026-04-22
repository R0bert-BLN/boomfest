using BoomFest.Data;
using Microsoft.EntityFrameworkCore;

namespace BoomFest.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> CountOrdersAsync()
    {
        return _context.Orders.AsNoTracking().CountAsync();
    }
}

