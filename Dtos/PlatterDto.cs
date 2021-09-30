using System;
using System.Collections.Generic;

namespace platterr_api.Dtos
{
    public class PlatterDto
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public ICollection<PlatterFormatDto> Formats { get; set; }
    }
}