using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class CosmeticCategoryRepository : Repository<CosmeticCategory>, ICosmeticCategoryRepository
{
    public CosmeticCategoryRepository(AppDbContext context) : base(context) { }
}
