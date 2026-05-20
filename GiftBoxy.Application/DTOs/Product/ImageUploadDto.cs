using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Application.DTOs.Product
{
    public class ImageUploadDto
    {
        public List<IFormFile> Images { get; set; }
    }
}
