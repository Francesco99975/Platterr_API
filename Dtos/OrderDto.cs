using System;
using System.Collections.Generic;

namespace platterr_api.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public ICollection<PlatterRequestDto> Platters { get; set; }

        public String CustomerFirstName { get; set; }

        public String CustomerLastName { get; set; }

        public String PhoneNumber { get; set; }

        public String Comment { get; set; }

        public bool Delivery { get; set; }

        public bool Paid { get; set; }

        public String CreatedAt { get; set; }

        public String DueDate { get; set; }
    }
}