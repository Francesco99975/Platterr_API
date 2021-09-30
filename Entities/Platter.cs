using System;
using System.Collections.Generic;
using platterr_api.Dtos;

namespace platterr_api.Entities
{
    public class Platter
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public ICollection<PlatterFormat> Formats { get; set; }
    }
}