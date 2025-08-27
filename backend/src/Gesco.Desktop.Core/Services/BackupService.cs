using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace Gesco.Desktop.Core.Services
{
    public interface IBackupService
    {
        Task CreateBackupAsync();
        Task<bool> RestoreBackupAsync(string backupPath);
        Task CleanOldBackupsAsync(int daysToKeep = 30);
    }

    public class BackupService : IBackupService, IHostedService
    {
        private readonly ILogger<BackupService> _logger;
        private readonly Timer _timer;
        private readonly string _dbPath;
        private readonly string _backupFolder;

        public BackupService(ILogger<BackupService> logger)
        {
            _logger = logger;
            _dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
            _backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            
            // Crear carpeta de backups si no existe
            if (!Directory.Exists(_backupFolder))
            {
                Directory.CreateDirectory(_backupFolder);
            }

            // Timer para backup automático cada 6 horas
            _timer = new Timer(async _ => await CreateBackupAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(6));
        }

        public async Task CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupName = $"gesco_backup_{timestamp}.zip";
                var backupPath = Path.Combine(_backupFolder, backupName);

                using (var fileStream = new FileStream(backupPath, FileMode.Create))
                using (var zipStream = new ZipOutputStream(fileStream))
                {
                    zipStream.SetLevel(6);

                    // Backup de la base de datos
                    if (Directory.Exists(_dbPath))
                    {
                        foreach (var file in Directory.GetFiles(_dbPath, "*.db"))
                        {
                            await AddFileToZip(zipStream, file, Path.GetFileName(file));
                        }
                    }

                    // Backup de logs importantes
                    var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                    if (Directory.Exists(logsPath))
                    {
                        foreach (var file in Directory.GetFiles(logsPath, "*.log"))
                        {
                            if (file.Contains("audit") || file.Contains("security"))
                            {
                                await AddFileToZip(zipStream, file, $"logs/{Path.GetFileName(file)}");
                            }
                        }
                    }
                }

                _logger.LogInformation("Backup created successfully: {BackupPath}", backupPath);
                
                // Limpiar backups antiguos
                await CleanOldBackupsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
            }
        }

        private async Task AddFileToZip(ZipOutputStream zipStream, string filePath, string entryName)
        {
            var entry = new ZipEntry(entryName)
            {
                DateTime = File.GetLastWriteTime(filePath)
            };
            
            zipStream.PutNextEntry(entry);
            
            using (var fileStream = File.OpenRead(filePath))
            {
                await fileStream.CopyToAsync(zipStream);
            }
            
            zipStream.CloseEntry();
        }

        public async Task<bool> RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    _logger.LogError("Backup file not found: {BackupPath}", backupPath);
                    return false;
                }

                using (var fileStream = new FileStream(backupPath, FileMode.Open, FileAccess.Read))
                using (var zipStream = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;
                    while ((entry = zipStream.GetNextEntry()) != null)
                    {
                        if (!entry.IsFile) continue;

                        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), entry.Name);
                        var outputDir = Path.GetDirectoryName(outputPath);
                        
                        if (!Directory.Exists(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                        }

                        using (var output = File.Create(outputPath))
                        {
                            await zipStream.CopyToAsync(output);
                        }
                    }
                }

                _logger.LogInformation("Backup restored successfully from: {BackupPath}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup from: {BackupPath}", backupPath);
                return false;
            }
        }

        public async Task CleanOldBackupsAsync(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var backupFiles = Directory.GetFiles(_backupFolder, "gesco_backup_*.zip");

                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        _logger.LogInformation("Deleted old backup: {FileName}", fileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning old backups");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Backup Service started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("Backup Service stopped");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
