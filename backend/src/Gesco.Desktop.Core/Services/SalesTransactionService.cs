using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class SalesTransactionService : ISalesTransactionService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<SalesTransactionService> _logger;

        public SalesTransactionService(LocalDbContext context, ILogger<SalesTransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SalesTransactionDto>> GetSalesAsync(Guid? cashRegisterId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.SalesTransactions.AsQueryable();

                if (cashRegisterId.HasValue)
                {
                    var longId = MapGuidToLong(cashRegisterId.Value);
                    query = query.Where(st => st.CashRegisterId == longId);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(st => st.TransactionDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(st => st.TransactionDate <= endDate.Value);
                }

                var sales = await query
                    .OrderByDescending(st => st.TransactionDate)
                    .ToListAsync();

                // ✅ Mapear manualmente
                var result = new List<SalesTransactionDto>();

                foreach (var sale in sales)
                {
                    var dto = await MapToDtoAsync(sale);
                    result.Add(dto);
                }

                _logger.LogInformation("Retrieved {Count} sales transactions", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales transactions: {Message}", ex.Message);
                throw;
            }
        }
private async Task<SalesTransactionDto> MapToDtoAsync(SalesTransaction sale)
{
    // Cargar relaciones
    var cashRegister = await _context.CashRegisters.FindAsync(sale.CashRegisterId);
    var status = await _context.SalesStatuses.FindAsync(sale.SalesStatusId);
    
    // Cargar detalles
    var details = await _context.TransactionDetails
        .Where(td => td.SalesTransactionId == sale.Id)
        .ToListAsync();
    
    var detailDtos = new List<TransactionDetailDto>();
    foreach (var detail in details)
    {
        CategoryProduct? product = null;
        SalesCombo? combo = null;
        
        if (detail.ProductId.HasValue)
        {
            product = await _context.CategoryProducts.FindAsync(detail.ProductId.Value);
        }
        
        if (detail.ComboId.HasValue)
        {
            combo = await _context.SalesCombos.FindAsync(detail.ComboId.Value);
        }
        
        detailDtos.Add(new TransactionDetailDto
        {
            Id = MapLongToGuid(detail.Id),
            ProductId = detail.ProductId.HasValue ? MapLongToGuid(detail.ProductId.Value) : null,
            ProductName = product?.Name ?? combo?.Name,
            ComboId = detail.ComboId.HasValue ? MapLongToGuid(detail.ComboId.Value) : null,
            ComboName = combo?.Name,
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            TotalAmount = detail.TotalAmount,
            IsCombo = detail.IsCombo
        });
    }
    
    // Cargar pagos
    var payments = await _context.TransactionPayments
        .Where(tp => tp.SalesTransactionId == sale.Id)
        .ToListAsync();
    
    var paymentDtos = new List<TransactionPaymentDto>();
    foreach (var payment in payments)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(payment.PaymentMethodId);
        
        paymentDtos.Add(new TransactionPaymentDto
        {
            Id = MapLongToGuid(payment.Id),
            PaymentMethodId = (int)payment.PaymentMethodId,
            PaymentMethodName = paymentMethod?.Name,
            Amount = payment.Amount,
            Reference = payment.Reference,
            ProcessedAt = payment.ProcessedAt,
            ProcessedBy = payment.ProcessedBy,
            ProcessedByName = payment.ProcessedBy
        });
    }
    
    return new SalesTransactionDto
    {
        Id = MapLongToGuid(sale.Id),
        CashRegisterId = MapLongToGuid(sale.CashRegisterId),
        TransactionNumber = sale.TransactionNumber,
        InvoiceNumber = sale.InvoiceNumber,
        SalesStatusId = (int)sale.SalesStatusId,
        StatusName = status?.Name,
        TransactionDate = sale.TransactionDate,
        TotalAmount = sale.TotalAmount,
        Details = detailDtos,
        Payments = paymentDtos
    };
}

        public async Task<SalesTransactionDto?> GetSaleByIdAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                var sale = await _context.SalesTransactions
                    .Include(st => st.CashRegister)
                    .Include(st => st.SalesStatus)
                    .Include(st => st.TransactionDetails)
                        .ThenInclude(td => td.Product)
                    .Include(st => st.TransactionPayments)
                        .ThenInclude(tp => tp.PaymentMethod)
                    .Include(st => st.TransactionPayments)
                        .ThenInclude(tp => tp.ProcessedByUser)
                    .FirstOrDefaultAsync(st => st.Id == longId);

                return sale != null ? MapToDto(sale) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sale {Id}", id);
                throw;
            }
        }

        public async Task<SalesTransactionDto> CreateSaleAsync(CreateSaleRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating new sale for cash register: {CashRegisterId}", request.CashRegisterId);

                // Validar caja registradora
                var cashRegisterId = MapGuidToLong(request.CashRegisterId);
                var cashRegister = await _context.CashRegisters.FindAsync(cashRegisterId);
                
                if (cashRegister == null)
                {
                    throw new ArgumentException($"Cash register {request.CashRegisterId} not found");
                }

                if (!cashRegister.IsOpen)
                {
                    throw new InvalidOperationException("Cash register is not open");
                }

                // Obtener estado "Pending"
                var pendingStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Pending");
                
                if (pendingStatus == null)
                {
                    throw new InvalidOperationException("Pending sales status not found");
                }

                // Crear transacción
                var sale = new SalesTransaction
                {
                    CashRegisterId = cashRegisterId,
                    TransactionNumber = await GenerateTransactionNumberAsync(cashRegisterId),
                    SalesStatusId = pendingStatus.Id,
                    TransactionDate = DateTime.UtcNow,
                    TotalAmount = 0, // Se calculará después
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                _context.SalesTransactions.Add(sale);
                await _context.SaveChangesAsync(); // Guardar para obtener el ID

                // Crear detalles
                decimal totalAmount = 0;
                foreach (var item in request.Items)
                {
                    // Validar producto
                    var productId = MapGuidToLong(item.ProductId);
                    var product = await _context.CategoryProducts.FindAsync(productId);
                    
                    if (product == null)
                    {
                        throw new ArgumentException($"Product {item.ProductId} not found");
                    }

                    if (!product.Active)
                    {
                        throw new ArgumentException($"Product {product.Name} is not active");
                    }

                    // Verificar stock disponible (no descontar aún)
                    if (product.CurrentQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.CurrentQuantity}, Requested: {item.Quantity}");
                    }

                    var detail = new TransactionDetail
                    {
                        SalesTransactionId = sale.Id,
                        ProductId = productId,
                        Quantity = item.Quantity,
                        UnitPrice = product.UnitPrice,
                        TotalAmount = product.UnitPrice * item.Quantity,
                        IsCombo = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.CreatedBy
                    };

                    _context.TransactionDetails.Add(detail);
                    totalAmount += detail.TotalAmount;
                }

                // Actualizar total
                sale.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sale created: {Id}, Total: {Amount}", sale.Id, totalAmount);

                return await GetSaleByIdAsync(MapLongToGuid(sale.Id))
                    ?? throw new InvalidOperationException("Failed to retrieve created sale");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating sale");
                throw;
            }
        }

        public async Task<SalesTransactionDto?> UpdateSaleAsync(Guid id, CreateSaleRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var sale = await _context.SalesTransactions
                    .Include(st => st.TransactionDetails)
                    .FirstOrDefaultAsync(st => st.Id == longId);

                if (sale == null)
                    return null;

                // Solo permitir actualización si está en estado Pending
                var pendingStatus = await _context.SalesStatuses.FirstOrDefaultAsync(s => s.Name == "Pending");
                if (sale.SalesStatusId != pendingStatus?.Id)
                {
                    throw new InvalidOperationException("Can only update pending sales");
                }

                // Eliminar detalles existentes
                _context.TransactionDetails.RemoveRange(sale.TransactionDetails);

                // Agregar nuevos detalles
                decimal totalAmount = 0;
                foreach (var item in request.Items)
                {
                    var productId = MapGuidToLong(item.ProductId);
                    var product = await _context.CategoryProducts.FindAsync(productId);
                    
                    if (product == null || !product.Active)
                    {
                        throw new ArgumentException($"Product {item.ProductId} not found or inactive");
                    }

                    if (product.CurrentQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}");
                    }

                    var detail = new TransactionDetail
                    {
                        SalesTransactionId = sale.Id,
                        ProductId = productId,
                        Quantity = item.Quantity,
                        UnitPrice = product.UnitPrice,
                        TotalAmount = product.UnitPrice * item.Quantity,
                        IsCombo = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = request.CreatedBy
                    };

                    _context.TransactionDetails.Add(detail);
                    totalAmount += detail.TotalAmount;
                }

                sale.TotalAmount = totalAmount;
                sale.UpdatedAt = DateTime.UtcNow;
                sale.UpdatedBy = request.CreatedBy;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sale updated: {Id}", id);
                return await GetSaleByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating sale {Id}", id);
                throw;
            }
        }

        public async Task<bool> CancelSaleAsync(Guid id, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var sale = await _context.SalesTransactions
                    .Include(st => st.TransactionDetails)
                    .FirstOrDefaultAsync(st => st.Id == longId);

                if (sale == null)
                    return false;

                // Obtener estado cancelado
                var cancelledStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Cancelled");

                if (cancelledStatus == null)
                {
                    throw new InvalidOperationException("Cancelled status not found");
                }

                // Si estaba completada, revertir inventario
                var completedStatus = await _context.SalesStatuses.FirstOrDefaultAsync(s => s.Name == "Completed");
                if (sale.SalesStatusId == completedStatus?.Id)
                {
                    await RevertInventoryAsync(sale);
                }

                sale.SalesStatusId = cancelledStatus.Id;
                sale.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sale cancelled: {Id}, Reason: {Reason}", id, reason);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sale {Id}", id);
                throw;
            }
        }

        public async Task<SalesTransactionDto?> CompleteSaleAsync(Guid id, List<CreatePaymentRequest> payments)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var sale = await _context.SalesTransactions
                    .Include(st => st.TransactionDetails)
                        .ThenInclude(td => td.Product)
                    .Include(st => st.TransactionPayments)
                    .FirstOrDefaultAsync(st => st.Id == longId);

                if (sale == null)
                    return null;

                // Validar que esté pendiente
                var pendingStatus = await _context.SalesStatuses.FirstOrDefaultAsync(s => s.Name == "Pending");
                if (sale.SalesStatusId != pendingStatus?.Id)
                {
                    throw new InvalidOperationException("Sale is not in pending status");
                }

                // Validar pagos
                var totalPayments = payments.Sum(p => p.Amount);
                if (totalPayments < sale.TotalAmount)
                {
                    throw new ArgumentException($"Insufficient payment amount. Required: {sale.TotalAmount}, Provided: {totalPayments}");
                }

                // Procesar pagos
                foreach (var payment in payments)
                {
                    var paymentMethod = await _context.PaymentMethods.FindAsync((long)payment.PaymentMethodId);
                    if (paymentMethod == null)
                    {
                        throw new ArgumentException($"Payment method {payment.PaymentMethodId} not found");
                    }

                    if (paymentMethod.RequiresReference && string.IsNullOrWhiteSpace(payment.Reference))
                    {
                        throw new ArgumentException($"Payment method {paymentMethod.Name} requires a reference");
                    }

                    var transactionPayment = new TransactionPayment
                    {
                        SalesTransactionId = sale.Id,
                        PaymentMethodId = (long)payment.PaymentMethodId,
                        Amount = payment.Amount,
                        Reference = payment.Reference,
                        ProcessedAt = DateTime.UtcNow,
                        ProcessedBy = payment.ProcessedBy,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = payment.ProcessedBy
                    };

                    _context.TransactionPayments.Add(transactionPayment);
                }

                // Actualizar inventario y crear movimientos
                var saleMovementType = await _context.InventoryMovementTypes
                    .FirstOrDefaultAsync(t => t.Name == "Sale");

                foreach (var detail in sale.TransactionDetails)
                {
                    if (detail.Product == null) continue;

                    var previousQuantity = detail.Product.CurrentQuantity;
                    detail.Product.CurrentQuantity -= detail.Quantity;
                    detail.Product.UpdatedAt = DateTime.UtcNow;

                    // Crear movimiento de inventario
                    if (saleMovementType != null)
                    {
                        var movement = new InventoryMovement
                        {
                            ProductId = detail.ProductId ?? 0,
                            MovementTypeId = saleMovementType.Id,
                            Quantity = -detail.Quantity,
                            PreviousQuantity = previousQuantity,
                            NewQuantity = detail.Product.CurrentQuantity,
                            SalesTransactionId = sale.Id,
                            MovementDate = DateTime.UtcNow,
                            PerformedBy = payments.FirstOrDefault()?.ProcessedBy,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.InventoryMovements.Add(movement);
                    }
                }

                // Cambiar estado a completado
                var completedStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Completed");

                if (completedStatus == null)
                {
                    throw new InvalidOperationException("Completed status not found");
                }

                sale.SalesStatusId = completedStatus.Id;
                sale.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sale completed: {Id}, Total: {Amount}, Payments: {PaymentCount}", 
                    id, sale.TotalAmount, payments.Count);

                return await GetSaleByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error completing sale {Id}", id);
                throw;
            }
        }

        public async Task<List<SalesTransactionDto>> GetSalesByCashRegisterAsync(Guid cashRegisterId)
        {
            return await GetSalesAsync(cashRegisterId);
        }

        public async Task<SalesSummaryDto> GetSalesSummaryAsync(Guid? cashRegisterId = null, DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                var query = _context.SalesTransactions.AsQueryable();

                if (cashRegisterId.HasValue)
                {
                    var longId = MapGuidToLong(cashRegisterId.Value);
                    query = query.Where(st => st.CashRegisterId == longId);
                }

                query = query.Where(st => st.TransactionDate.Date == targetDate.Date);

                var completedStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Completed");

                var completedSales = query.Where(st => completedStatus == null || st.SalesStatusId == completedStatus.Id);

                var summary = new SalesSummaryDto
                {
                    Date = targetDate,
                    TotalTransactions = await query.CountAsync(),
                    CompletedTransactions = await completedSales.CountAsync(),
                    TotalSales = await completedSales.SumAsync(st => (decimal?)st.TotalAmount) ?? 0m,
                    AverageTransaction = await completedSales.AnyAsync() 
                        ? await completedSales.AverageAsync(st => st.TotalAmount)
                        : 0m,
                    TotalItemsSold = await _context.TransactionDetails
                        .Where(td => completedSales.Select(cs => cs.Id).Contains(td.SalesTransactionId))
                        .SumAsync(td => td.Quantity)
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales summary");
                throw;
            }
        }

        // ============================================
        // MÉTODOS PRIVADOS
        // ============================================

        private async Task<string> GenerateTransactionNumberAsync(long cashRegisterId)
        {
            var today = DateTime.Today;
            var count = await _context.SalesTransactions
                .Where(st => st.CashRegisterId == cashRegisterId && st.TransactionDate.Date == today)
                .CountAsync();

            return $"TXN-{cashRegisterId}-{today:yyyyMMdd}-{(count + 1):D4}";
        }

        private async Task RevertInventoryAsync(SalesTransaction sale)
        {
            var details = await _context.TransactionDetails
                .Include(td => td.Product)
                .Where(td => td.SalesTransactionId == sale.Id)
                .ToListAsync();

            var adjustmentType = await _context.InventoryMovementTypes
                .FirstOrDefaultAsync(t => t.Name == "Adjustment");

            foreach (var detail in details)
            {
                if (detail.Product == null) continue;

                var previousQuantity = detail.Product.CurrentQuantity;
                detail.Product.CurrentQuantity += detail.Quantity; // Devolver al inventario
                detail.Product.UpdatedAt = DateTime.UtcNow;

                if (adjustmentType != null)
                {
                    var movement = new InventoryMovement
                    {
                        ProductId = detail.ProductId ?? 0,
                        MovementTypeId = adjustmentType.Id,
                        Quantity = detail.Quantity,
                        PreviousQuantity = previousQuantity,
                        NewQuantity = detail.Product.CurrentQuantity,
                        SalesTransactionId = sale.Id,
                        Justification = "Sale cancellation - inventory revert",
                        MovementDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryMovements.Add(movement);
                }
            }
        }

        private SalesTransactionDto MapToDto(SalesTransaction sale)
        {
            return new SalesTransactionDto
            {
                Id = MapLongToGuid(sale.Id),
                CashRegisterId = MapLongToGuid(sale.CashRegisterId),
                TransactionNumber = sale.TransactionNumber,
                InvoiceNumber = sale.InvoiceNumber,
                SalesStatusId = (int)sale.SalesStatusId,
                StatusName = sale.SalesStatus?.Name,
                TransactionDate = sale.TransactionDate,
                TotalAmount = sale.TotalAmount,
                Details = sale.TransactionDetails?.Select(td => new TransactionDetailDto
                {
                    Id = MapLongToGuid(td.Id),
                    ProductId = td.ProductId.HasValue ? MapLongToGuid(td.ProductId.Value) : null,
                    ProductName = td.Product?.Name,
                    Quantity = td.Quantity,
                    UnitPrice = td.UnitPrice,
                    TotalAmount = td.TotalAmount,
                    IsCombo = td.IsCombo
                }).ToList() ?? new List<TransactionDetailDto>(),
                Payments = sale.TransactionPayments?.Select(tp => new TransactionPaymentDto
                {
                    Id = MapLongToGuid(tp.Id),
                    PaymentMethodId = (int)tp.PaymentMethodId,
                    PaymentMethodName = tp.PaymentMethod?.Name,
                    Amount = tp.Amount,
                    Reference = tp.Reference,
                    ProcessedAt = tp.ProcessedAt,
                    ProcessedBy = tp.ProcessedBy,
                    ProcessedByName = tp.ProcessedByUser?.FullName ?? tp.ProcessedByUser?.Username
                }).ToList() ?? new List<TransactionPaymentDto>()
            };
        }

        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            Array.Copy(longBytes, 0, bytes, 0, 8);
            bytes[8] = 0x5A; bytes[9] = 0x1E; // "SALE" identifier
            return new Guid(bytes);
        }

        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }

    // NOTA: Los DTOs CreateSaleRequest, CreateSaleItemRequest, CreatePaymentRequest 
    // y SalesSummaryDto deben estar en Gesco.Desktop.Shared.DTOs
}