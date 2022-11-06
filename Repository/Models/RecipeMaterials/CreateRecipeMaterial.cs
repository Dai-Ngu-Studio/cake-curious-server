namespace Repository.Models.RecipeMaterials
{
    public class CreateRecipeMaterial
    {
        public Guid? Id { get; set; }
        public int? MaterialType { get; set; }
        public string? MaterialName { get; set; }
        public decimal? Amount { get; set; }
        public string? Measurement { get; set; }
        public string? Color { get; set; }
    }
}
