using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.Areas.Admin.ViewModels
{
    public class CreateProductVM
    {
        [Required]
        public string Name { get; set; }
        public decimal Price { get; set; }

        public string SKU { get; set; }

        public string Description { get; set; }

        [Required]
        public int? CategoryId { get; set; }
    }
}
