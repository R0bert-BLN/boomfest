using System.Text.Json;
using BoomFest.Dtos;
using BoomFest.Repositories;
using Microsoft.AspNetCore.Http;

namespace BoomFest.Services;

public class CartService : ICartService
{
    private const string CartSessionKey = "boomfest_cart";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFestivalRepository _festivalRepository;

    public CartService(IHttpContextAccessor httpContextAccessor, IFestivalRepository festivalRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _festivalRepository = festivalRepository;
    }

    public async Task<CartDto> GetCartAsync()
    {
        var state = GetState();
        if (state.Lines.Count == 0)
        {
            return new CartDto();
        }

        var categoryIds = state.Lines.Select(line => line.CategoryId).Distinct().ToList();
        var categories = await _festivalRepository.GetTicketCategoriesByIdsAsync(categoryIds);

        var cartItems = state.Lines
            .Join(categories, line => line.CategoryId, category => category.Id, (line, category) => new
            {
                Line = line,
                Category = category
            })
            .ToList();

        var festivalGroups = cartItems
            .GroupBy(item => item.Category.FestivalId)
            .Select(group =>
            {
                var festival = group.First().Category.Festival;
                var items = group.Select(item => new CartItemDto
                {
                    CategoryId = item.Category.Id,
                    CategoryName = item.Category.Name,
                    Quantity = item.Line.Quantity,
                    UnitPrice = item.Category.Price,
                    TotalPrice = item.Category.Price * item.Line.Quantity
                }).ToList();

                var subtotal = items.Sum(item => item.TotalPrice);
                var location = string.IsNullOrWhiteSpace(festival.Location)
                    ? string.Join(", ", new[] { festival.City, festival.Country }.Where(value => !string.IsNullOrWhiteSpace(value)))
                    : festival.Location;

                return new CartFestivalDto
                {
                    FestivalId = festival.Id,
                    Title = festival.Title,
                    StartDate = festival.StartDate,
                    Location = location,
                    Items = items,
                    Subtotal = subtotal
                };
            })
            .ToList();

        return new CartDto
        {
            Festivals = festivalGroups,
            TotalTickets = cartItems.Sum(item => item.Line.Quantity),
            TotalPrice = festivalGroups.Sum(group => group.Subtotal)
        };
    }

    public async Task AddToCartAsync(Guid festivalId, IEnumerable<CartQuantityDto> quantities)
    {
        var filtered = quantities
            .Where(entry => entry.Quantity > 0)
            .ToDictionary(entry => entry.CategoryId, entry => entry.Quantity);

        if (filtered.Count == 0)
        {
            return;
        }

        var categories = await _festivalRepository.GetTicketCategoriesByIdsAsync(filtered.Keys);
        var validCategories = categories
            .Where(category => category.FestivalId == festivalId)
            .ToList();

        if (validCategories.Count == 0)
        {
            return;
        }

        var state = GetState();
        foreach (var category in validCategories)
        {
            var quantity = filtered.TryGetValue(category.Id, out var qty) ? qty : 0;
            if (quantity <= 0)
            {
                continue;
            }

            var existing = state.Lines.FirstOrDefault(line => line.CategoryId == category.Id);
            if (existing == null)
            {
                state.Lines.Add(new CartLine { CategoryId = category.Id, Quantity = quantity });
            }
            else
            {
                existing.Quantity += quantity;
            }
        }

        SaveState(state);
    }

    public Task UpdateQuantitiesAsync(Dictionary<Guid, int> quantities)
    {
        var state = GetState();

        foreach (var line in state.Lines.ToList())
        {
            if (!quantities.TryGetValue(line.CategoryId, out var quantity) || quantity <= 0)
            {
                state.Lines.Remove(line);
                continue;
            }

            line.Quantity = quantity;
        }

        SaveState(state);
        return Task.CompletedTask;
    }

    private CartState GetState()
    {
        var session = GetSession();
        var json = session.GetString(CartSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new CartState();
        }

        try
        {
            return JsonSerializer.Deserialize<CartState>(json) ?? new CartState();
        }
        catch (JsonException)
        {
            return new CartState();
        }
    }

    private void SaveState(CartState state)
    {
        var session = GetSession();
        var json = JsonSerializer.Serialize(state);
        session.SetString(CartSessionKey, json);
    }

    private ISession GetSession()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new InvalidOperationException("Session is unavailable.");
        }

        return context.Session;
    }

    private class CartState
    {
        public List<CartLine> Lines { get; set; } = new();
    }

    private class CartLine
    {
        public Guid CategoryId { get; set; }
        public int Quantity { get; set; }
    }
}
