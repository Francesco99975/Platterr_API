using System.ComponentModel.DataAnnotations.Schema;
using platterr_api.Entities;

namespace platterr_api.Entities
{
    [Table("PlatterRequests")]
    public class PlatterRequest
    {
        public int Id { get; set; }
        public int PlatterId { get; set; }
        public Platter Platter { get; set; }
        public PlatterFormat Format { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}