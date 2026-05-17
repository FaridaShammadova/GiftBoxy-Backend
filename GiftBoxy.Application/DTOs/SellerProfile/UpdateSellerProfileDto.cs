using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Application.DTOs.SellerProfile
{
    public class UpdateSellerProfileDto
    {
        public string? StoreName { get; set; }
        public string? ShopUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public List<string>? Categories { get; set; }
    }
}
