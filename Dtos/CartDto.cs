using System;
using System.Collections.Generic;

namespace BoomFest.Dtos;

public class CartDto
{
    public IReadOnlyList<CartFestivalDto> Festivals { get; init; } = Array.Empty<CartFestivalDto>();
    public int TotalTickets { get; init; }
    public decimal TotalPrice { get; init; }
}

public class CartFestivalDto
{
    public Guid FestivalId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public IReadOnlyList<CartItemDto> Items { get; init; } = Array.Empty<CartItemDto>();
    public decimal Subtotal { get; init; }
}

public class CartItemDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}

