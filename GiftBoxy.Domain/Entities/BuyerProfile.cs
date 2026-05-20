using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Domain.Entities
{
    public class BuyerProfile : BaseEntity
    {
        public string? Location { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
