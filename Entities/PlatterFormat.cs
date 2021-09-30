using System.ComponentModel.DataAnnotations.Schema;
using platterr_api.Entities;

namespace platterr_api.Entities
{
    [Table("Formats")]
    public class PlatterFormat
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public double Price { get; set; }
        public int PlatterId { get; set; }
        public Platter Platter { get; set; }
    }
}