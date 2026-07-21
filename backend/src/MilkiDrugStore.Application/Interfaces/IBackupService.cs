using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MilkiDrugStore.Application.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(string backupPath);
    Task<bool> RestoreBackupAsync(string backupPath);
    Task<string> GetBackupDirectoryAsync();
}
