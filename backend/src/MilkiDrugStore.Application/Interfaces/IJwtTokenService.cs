namespace MilkiDrugStore.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(MilkiDrugStore.Domain.Entities.User user);
}
