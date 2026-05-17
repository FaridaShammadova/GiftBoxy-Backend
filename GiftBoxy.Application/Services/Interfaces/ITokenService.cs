using GiftBoxy.Domain.Entities;

namespace GiftBoxy.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
