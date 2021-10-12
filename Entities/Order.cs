using System;
using System.Collections.Generic;
using platterr_api.Dtos;

namespace platterr_api.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public ICollection<PlatterRequest> Platters { get; set; }

        public String CustomerFirstName { get; set; }

        public String CustomerLastName { get; set; }

        public String PhoneNumber { get; set; }

        public String Comment { get; set; }

        public String CreatedAt { get; set; }

        public String DueDate { get; set; }
    }
}