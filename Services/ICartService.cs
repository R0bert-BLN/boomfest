using BoomFest.Dtos;

namespace BoomFest.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync();
    Task AddToCartAsync(Guid festivalId, IEnumerable<CartQuantityDto> quantities);
    Task UpdateQuantitiesAsync(Dictionary<Guid, int> quantities);
}
