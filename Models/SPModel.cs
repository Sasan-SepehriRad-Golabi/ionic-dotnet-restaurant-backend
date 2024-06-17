using Microsoft.EntityFrameworkCore;

[Keyless]
public class SPModel
{
    public int? DishId { get; set; }
    public int? compNumber { get; set; }
    public int? mainEnergy { get; set; }
    public int? RestaurantId { get; set; }
    public int? DishType { get; set; }
    public int? Dietary { get; set; }
}