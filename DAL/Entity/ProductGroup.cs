using System;

namespace first.DAL.Entity
{
    public class ProductGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Picture { get; set; } = null!;
    }
}
