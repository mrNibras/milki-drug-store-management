using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces;

namespace MilkiDrugStore.Application.Services;

public class UserActivityService : IUserActivityService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserActivityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsActiveAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return false;

        // Translated to a single indexed EXISTS query against the primary key.
        var activeUsers = await _unitOfWork.Users
            .FindAsync(u => u.UserId == userId && u.IsActive && u.IsApproved);

        return await activeUsers.AnyAsync(cancellationToken);
    }
}
