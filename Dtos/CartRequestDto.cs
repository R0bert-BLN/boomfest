using System;
using System.Collections.Generic;

namespace BoomFest.Dtos;

public class AddToCartDto
{
    public Guid FestivalId { get; set; }
    public List<CartQuantityDto> Quantities { get; set; } = new();
}

public class CartQuantityDto
{
    public Guid CategoryId { get; set; }
    public int Quantity { get; set; }
}

