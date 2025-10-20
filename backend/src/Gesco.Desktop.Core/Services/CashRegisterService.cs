using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class CashRegisterService : ICashRegisterService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<CashRegisterService> _logger;

        public CashRegisterService(LocalDbContext context, ILogger<CashRegisterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CashRegisterDto>> GetCashRegistersAsync(Guid? activityId = null)
        {
            try
            {
                var query = _context.CashRegisters
                    .Include(cr => cr.Activity)
                    .Include(cr => cr.OperatorUser)
                    .Include(cr => cr.SupervisorUser)
                    .AsQueryable();

                if (activityId.HasValue)
                {
                    var longActivityId = MapGuidToLong(activityId.Value);
                    query = query.Where(cr => cr.ActivityId == longActivityId);
                }

                var registers = await query
                    .OrderBy(cr => cr.RegisterNumber)
                    .Select(cr => MapToDto(cr))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} cash registers", registers.Count);
                return registers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cash registers");
                throw;
            }
        }

        public async Task<CashRegisterDto?> GetCashRegisterByIdAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                var register = await _context.CashRegisters
                    .Include(cr => cr.Activity)
                    .Include(cr => cr.OperatorUser)
                    .Include(cr => cr.SupervisorUser)
                    .FirstOrDefaultAsync(cr => cr.Id == longId);

                return register != null ? MapToDto(register) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cash register {Id}", id);
                throw;
            }
        }

        public async Task<CashRegisterDto> CreateCashRegisterAsync(CreateCashRegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating cash register: {Name}", request.Name);

                // Validar actividad
                var activityId = MapGuidToLong(request.ActivityId);
                var activity = await _context.Activities.FindAsync(activityId);
                if (activity == null)
                {
                    throw new ArgumentException($"Activity {request.ActivityId} not found");
                }

                // Validar que no exista otro registro con el mismo número en la actividad
                var existingRegister = await _context.CashRegisters
                    .AnyAsync(cr => cr.ActivityId == activityId && cr.RegisterNumber == request.RegisterNumber);

                if (existingRegister)
                {
                    throw new ArgumentException($"Cash register number {request.RegisterNumber} already exists for this activity");
                }

                // Validar operador si se proporciona
                if (!string.IsNullOrWhiteSpace(request.OperatorUserId))
                {
                    var operatorExists = await _context.Users
                        .AnyAsync(u => u.Id == request.OperatorUserId && u.Active);
                    if (!operatorExists)
                    {
                        throw new ArgumentException($"Operator user {request.OperatorUserId} not found or inactive");
                    }
                }

                // Validar supervisor si se proporciona
                if (!string.IsNullOrWhiteSpace(request.SupervisorUserId))
                {
                    var supervisorExists = await _context.Users
                        .AnyAsync(u => u.Id == request.SupervisorUserId && u.Active);
                    if (!supervisorExists)
                    {
                        throw new ArgumentException($"Supervisor user {request.SupervisorUserId} not found or inactive");
                    }
                }

                var cashRegister = new CashRegister
                {
                    ActivityId = activityId,
                    RegisterNumber = request.RegisterNumber,
                    Name = request.Name?.Trim(),
                    Location = request.Location?.Trim(),
                    IsOpen = false,
                    OperatorUserId = string.IsNullOrWhiteSpace(request.OperatorUserId) ? null : request.OperatorUserId.Trim(),
                    SupervisorUserId = string.IsNullOrWhiteSpace(request.SupervisorUserId) ? null : request.SupervisorUserId.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.OperatorUserId?.Trim()
                };

                _context.CashRegisters.Add(cashRegister);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cash register created: {Id}", cashRegister.Id);

                return await GetCashRegisterByIdAsync(MapLongToGuid(cashRegister.Id))
                    ?? throw new InvalidOperationException("Failed to retrieve created cash register");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating cash register");
                throw;
            }
        }

        public async Task<CashRegisterDto?> UpdateCashRegisterAsync(Guid id, CreateCashRegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var register = await _context.CashRegisters.FindAsync(longId);

                if (register == null)
                    return null;

                // No permitir cambios si la caja está abierta
                if (register.IsOpen)
                {
                    throw new InvalidOperationException("Cannot update an open cash register");
                }

                register.Name = request.Name?.Trim();
                register.Location = request.Location?.Trim();
                register.OperatorUserId = string.IsNullOrWhiteSpace(request.OperatorUserId) ? null : request.OperatorUserId.Trim();
                register.SupervisorUserId = string.IsNullOrWhiteSpace(request.SupervisorUserId) ? null : request.SupervisorUserId.Trim();
                register.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cash register updated: {Id}", id);
                return await GetCashRegisterByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating cash register {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCashRegisterAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var register = await _context.CashRegisters.FindAsync(longId);

                if (register == null)
                    return false;

                // No permitir eliminación si está abierta o tiene transacciones
                if (register.IsOpen)
                {
                    throw new InvalidOperationException("Cannot delete an open cash register");
                }

                var hasTransactions = await _context.SalesTransactions
                    .AnyAsync(st => st.CashRegisterId == longId);

                if (hasTransactions)
                {
                    throw new InvalidOperationException("Cannot delete cash register with existing transactions");
                }

                _context.CashRegisters.Remove(register);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cash register deleted: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting cash register {Id}", id);
                throw;
            }
        }

        public async Task<CashRegisterDto?> OpenCashRegisterAsync(Guid id, string operatorUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var register = await _context.CashRegisters.FindAsync(longId);

                if (register == null)
                    return null;

                if (register.IsOpen)
                {
                    throw new InvalidOperationException("Cash register is already open");
                }

                // Validar operador
                var operatorExists = await _context.Users
                    .AnyAsync(u => u.Id == operatorUserId && u.Active);

                if (!operatorExists)
                {
                    throw new ArgumentException($"Operator user {operatorUserId} not found or inactive");
                }

                register.IsOpen = true;
                register.OpenedAt = DateTime.UtcNow;
                register.OperatorUserId = operatorUserId;
                register.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cash register opened: {Id} by {Operator}", id, operatorUserId);
                return await GetCashRegisterByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error opening cash register {Id}", id);
                throw;
            }
        }

        public async Task<CashRegisterDto?> CloseCashRegisterAsync(Guid id, CloseCashRegisterRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var register = await _context.CashRegisters
                    .Include(cr => cr.SalesTransactions)
                    .FirstOrDefaultAsync(cr => cr.Id == longId);

                if (register == null)
                    return null;

                if (!register.IsOpen)
                {
                    throw new InvalidOperationException("Cash register is not open");
                }

                if (!register.OpenedAt.HasValue)
                {
                    throw new InvalidOperationException("Cash register has no opening date");
                }

                // Calcular estadísticas de cierre
                var completedStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Completed");

                var transactions = register.SalesTransactions
                    .Where(st => completedStatus == null || st.SalesStatusId == completedStatus.Id)
                    .ToList();

                var totalTransactions = transactions.Count;
                var totalSalesAmount = transactions.Sum(st => st.TotalAmount);

                // Calcular totales por método de pago
                var paymentTotals = await _context.TransactionPayments
                    .Where(tp => transactions.Select(t => t.Id).Contains(tp.SalesTransactionId))
                    .Include(tp => tp.PaymentMethod)
                    .GroupBy(tp => tp.PaymentMethod.Name)
                    .Select(g => new { Method = g.Key, Total = g.Sum(tp => tp.Amount) })
                    .ToListAsync();

                var cashTotal = paymentTotals.FirstOrDefault(pt => pt.Method == "Cash")?.Total ?? 0m;
                var cardTotal = paymentTotals.FirstOrDefault(pt => pt.Method == "Card")?.Total ?? 0m;
                var sinpeTotal = paymentTotals.FirstOrDefault(pt => pt.Method == "SINPE Mobile")?.Total ?? 0m;

                // Crear cierre de caja
                var closure = new CashRegisterClosure
                {
                    CashRegisterId = longId,
                    OpeningDate = register.OpenedAt.Value,
                    ClosingDate = DateTime.UtcNow,
                    TotalTransactions = totalTransactions,
                    TotalItemsSold = await CalculateTotalItemsSoldAsync(longId),
                    TotalSalesAmount = totalSalesAmount,
                    CashCalculated = cashTotal,
                    CardsCalculated = cardTotal,
                    SinpeCalculated = sinpeTotal,
                    CashDeclared = request.CashDeclared,
                    CashDifference = request.CashDeclared - cashTotal,
                    ClosedBy = request.ClosedBy,
                    SupervisedBy = request.SupervisedBy,
                    Observations = request.Observations,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ClosedBy
                };

                _context.CashRegisterClosures.Add(closure);

                // Cerrar caja
                register.IsOpen = false;
                register.ClosedAt = DateTime.UtcNow;
                register.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cash register closed: {Id}, Total sales: {Amount}", id, totalSalesAmount);
                return await GetCashRegisterByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error closing cash register {Id}", id);
                throw;
            }
        }

        public async Task<List<CashRegisterDto>> GetOpenCashRegistersAsync()
        {
            try
            {
                var registers = await _context.CashRegisters
                    .Include(cr => cr.Activity)
                    .Include(cr => cr.OperatorUser)
                    .Include(cr => cr.SupervisorUser)
                    .Where(cr => cr.IsOpen)
                    .OrderBy(cr => cr.OpenedAt)
                    .Select(cr => MapToDto(cr))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} open cash registers", registers.Count);
                return registers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving open cash registers");
                throw;
            }
        }

        public async Task<CashRegisterClosureDto?> GetLastClosureAsync(Guid cashRegisterId)
        {
            try
            {
                var longId = MapGuidToLong(cashRegisterId);
                var closure = await _context.CashRegisterClosures
                    .Include(crc => crc.ClosedByUser)
                    .Include(crc => crc.SupervisedByUser)
                    .Where(crc => crc.CashRegisterId == longId)
                    .OrderByDescending(crc => crc.ClosingDate)
                    .FirstOrDefaultAsync();

                return closure != null ? MapClosureToDto(closure) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving last closure for cash register {Id}", cashRegisterId);
                throw;
            }
        }

        // ============================================
        // MÉTODOS PRIVADOS
        // ============================================

        private async Task<int> CalculateTotalItemsSoldAsync(long cashRegisterId)
        {
            return await _context.TransactionDetails
                .Where(td => _context.SalesTransactions
                    .Where(st => st.CashRegisterId == cashRegisterId)
                    .Select(st => st.Id)
                    .Contains(td.SalesTransactionId))
                .SumAsync(td => td.Quantity);
        }

        private CashRegisterDto MapToDto(CashRegister register)
        {
            return new CashRegisterDto
            {
                Id = MapLongToGuid(register.Id),
                ActivityId = MapLongToGuid(register.ActivityId),
                ActivityName = register.Activity?.Name,
                RegisterNumber = register.RegisterNumber,
                Name = register.Name,
                Location = register.Location,
                IsOpen = register.IsOpen,
                OpenedAt = register.OpenedAt,
                ClosedAt = register.ClosedAt,
                OperatorUserId = register.OperatorUserId,
                OperatorUserName = register.OperatorUser?.FullName ?? register.OperatorUser?.Username,
                SupervisorUserId = register.SupervisorUserId,
                SupervisorUserName = register.SupervisorUser?.FullName ?? register.SupervisorUser?.Username,
                CreatedAt = register.CreatedAt
            };
        }

        private CashRegisterClosureDto MapClosureToDto(CashRegisterClosure closure)
        {
            return new CashRegisterClosureDto
            {
                Id = MapLongToGuid(closure.Id),
                CashRegisterId = MapLongToGuid(closure.CashRegisterId),
                OpeningDate = closure.OpeningDate,
                ClosingDate = closure.ClosingDate,
                TotalTransactions = closure.TotalTransactions,
                TotalItemsSold = closure.TotalItemsSold,
                TotalSalesAmount = closure.TotalSalesAmount,
                CashCalculated = closure.CashCalculated,
                CardsCalculated = closure.CardsCalculated,
                SinpeCalculated = closure.SinpeCalculated,
                CashDeclared = closure.CashDeclared,
                CashDifference = closure.CashDifference,
                ClosedBy = closure.ClosedBy,
                ClosedByName = closure.ClosedByUser?.FullName ?? closure.ClosedByUser?.Username,
                SupervisedBy = closure.SupervisedBy,
                SupervisedByName = closure.SupervisedByUser?.FullName ?? closure.SupervisedByUser?.Username,
                Observations = closure.Observations
            };
        }

        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            Array.Copy(longBytes, 0, bytes, 0, 8);
            bytes[8] = 0xCA; bytes[9] = 0x5E; // "CASE" identifier
            return new Guid(bytes);
        }

        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }

    // ✅ ELIMINADO: CloseCashRegisterRequest ahora está en Gesco.Desktop.Shared.DTOs
}