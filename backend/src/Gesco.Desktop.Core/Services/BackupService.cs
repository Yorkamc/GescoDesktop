using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Gesco.Desktop.Core.Services
{
    public interface IBackupService
    {
        Task CreateBackupAsync();
        Task<bool> RestoreBackupAsync(string backupPath);
        Task CleanOldBackupsAsync(int daysToKeep = 30);
    }

    public class BackupService : IBackupService
    {
        private readonly ILogger<BackupService> _logger;
        private readonly string _dbPath;
        private readonly string _backupFolder;

        public BackupService(ILogger<BackupService> logger)
        {
            _logger = logger;
            _dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
            _backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            
            if (!Directory.Exists(_backupFolder))
            {
                Directory.CreateDirectory(_backupFolder);
            }
        }

        public Task CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupName = $"gesco_backup_{timestamp}.zip";
                var backupPath = Path.Combine(_backupFolder, backupName);

                if (Directory.Exists(_dbPath))
                {
                    var dbFiles = Directory.GetFiles(_dbPath, "*.db");
                    if (dbFiles.Length > 0)
                    {
                        foreach (var dbFile in dbFiles)
                        {
                            var fileName = Path.GetFileName(dbFile);
                            var backupDbPath = Path.Combine(_backupFolder, $"{timestamp}_{fileName}");
                            File.Copy(dbFile, backupDbPath, true);
                        }
                        _logger.LogInformation("Backup created successfully in: {BackupFolder}", _backupFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
            }

            return Task.CompletedTask;
        }

        public Task<bool> RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    _logger.LogError("Backup file not found: {BackupPath}", backupPath);
                    return Task.FromResult(false);
                }

                var fileName = Path.GetFileName(backupPath);
                if (fileName.Contains("gesco_backup_") && fileName.EndsWith(".db"))
                {
                    var targetPath = Path.Combine(_dbPath, "gesco_local.db");
                    File.Copy(backupPath, targetPath, true);
                    _logger.LogInformation("Backup restored successfully from: {BackupPath}", backupPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup from: {BackupPath}", backupPath);
                return Task.FromResult(false);
            }
        }

        public Task CleanOldBackupsAsync(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var backupFiles = Directory.GetFiles(_backupFolder, "gesco_backup_*.db");

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

            return Task.CompletedTask;
        }
    }

    public class BackupHostedService : BackgroundService
    {
        private readonly ILogger<BackupHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _backupInterval = TimeSpan.FromHours(6);

        public BackupHostedService(ILogger<BackupHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Backup Hosted Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                    
                    await backupService.CreateBackupAsync();
                    await backupService.CleanOldBackupsAsync();
                    
                    _logger.LogInformation("Automatic backup completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during automatic backup");
                }

                try
                {
                    await Task.Delay(_backupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Backup Hosted Service stopped");
        }
    }
}