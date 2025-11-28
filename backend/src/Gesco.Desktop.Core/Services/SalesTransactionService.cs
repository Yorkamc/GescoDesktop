using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    /// <summary>
    /// Servicio de Transacciones de Venta - ACTUALIZADO CON SOPORTE PARA COMBOS
    /// </summary>
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
            var cashRegister = await _context.CashRegisters.FindAsync(sale.CashRegisterId);
            var status = await _context.SalesStatuses.FindAsync(sale.SalesStatusId);
            
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
                var sale = await _context.SalesTransactions.FindAsync(longId);

                return sale != null ? await MapToDtoAsync(sale) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sale {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crear nueva venta - ACTUALIZADO: Ahora soporta productos Y combos
        /// </summary>
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
                    TotalAmount = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                _context.SalesTransactions.Add(sale);
                await _context.SaveChangesAsync();

                // ✅ NUEVO: Procesar items (productos Y combos)
                decimal totalAmount = 0;
                foreach (var item in request.Items)
                {
                    // Validar que tenga ProductId O ComboId
                    if (!item.ProductId.HasValue && !item.ComboId.HasValue)
                    {
                        throw new ArgumentException("Each item must have either ProductId or ComboId");
                    }

                    if (item.ProductId.HasValue && item.ComboId.HasValue)
                    {
                        throw new ArgumentException("Item cannot have both ProductId and ComboId");
                    }

                    // ✅ SI ES COMBO
                    if (item.ComboId.HasValue)
                    {
                        totalAmount += await ProcessComboItemAsync(
                            sale, 
                            item.ComboId.Value, 
                            item.Quantity, 
                            request.CreatedBy
                        );
                    }
                    // ✅ SI ES PRODUCTO
                    else if (item.ProductId.HasValue)
                    {
                        totalAmount += await ProcessProductItemAsync(
                            sale, 
                            item.ProductId.Value, 
                            item.Quantity, 
                            request.CreatedBy
                        );
                    }
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

        /// <summary>
        /// ✅ NUEVO: Procesar un producto individual en la venta
        /// </summary>
        private async Task<decimal> ProcessProductItemAsync(
            SalesTransaction sale, 
            Guid productGuid, 
            int quantity, 
            string createdBy)
        {
            var productId = MapGuidToLong(productGuid);
            var product = await _context.CategoryProducts.FindAsync(productId);
            
            if (product == null)
            {
                throw new ArgumentException($"Product {productGuid} not found");
            }

            if (!product.Active)
            {
                throw new ArgumentException($"Product {product.Name} is not active");
            }

            // Verificar stock disponible (NO descontar aún, se descuenta al completar)
            if (product.CurrentQuantity < quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for {product.Name}. " +
                    $"Available: {product.CurrentQuantity}, Requested: {quantity}"
                );
            }

            var detail = new TransactionDetail
            {
                SalesTransactionId = sale.Id,
                ProductId = productId,
                ComboId = null,
                Quantity = quantity,
                UnitPrice = product.UnitPrice,
                TotalAmount = product.UnitPrice * quantity,
                IsCombo = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.TransactionDetails.Add(detail);
            
            _logger.LogInformation(
                "Product added to sale: {ProductName} x{Quantity}, Price: {Price}",
                product.Name, quantity, product.UnitPrice
            );

            return detail.TotalAmount;
        }

        /// <summary>
        /// ✅ NUEVO: Procesar un combo en la venta (verifica stock de productos del combo)
        /// </summary>
        private async Task<decimal> ProcessComboItemAsync(
            SalesTransaction sale, 
            Guid comboGuid, 
            int quantity, 
            string createdBy)
        {
            var comboId = MapGuidToLong(comboGuid);
            
            // Cargar combo con sus items
            var combo = await _context.SalesCombos
                .Include(c => c.ComboItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == comboId);
            
            if (combo == null)
            {
                throw new ArgumentException($"Combo {comboGuid} not found");
            }

            if (!combo.Active)
            {
                throw new ArgumentException($"Combo {combo.Name} is not active");
            }

            if (combo.ComboItems == null || !combo.ComboItems.Any())
            {
                throw new InvalidOperationException($"Combo {combo.Name} has no items");
            }

            // ✅ Verificar stock de TODOS los productos del combo ANTES de crear detalles
            foreach (var comboItem in combo.ComboItems)
            {
                var requiredQuantity = comboItem.Quantity * quantity;
                
                if (comboItem.Product == null)
                {
                    throw new InvalidOperationException($"Product in combo {combo.Name} not found");
                }

                if (!comboItem.Product.Active)
                {
                    throw new ArgumentException($"Product {comboItem.Product.Name} in combo {combo.Name} is not active");
                }

                if (comboItem.Product.CurrentQuantity < requiredQuantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for {comboItem.Product.Name} in combo {combo.Name}. " +
                        $"Available: {comboItem.Product.CurrentQuantity}, Required: {requiredQuantity}"
                    );
                }
            }

            // ✅ Crear UN detalle para el combo completo
            var comboDetail = new TransactionDetail
            {
                SalesTransactionId = sale.Id,
                ProductId = null,
                ComboId = comboId,
                Quantity = quantity,
                UnitPrice = combo.ComboPrice,
                TotalAmount = combo.ComboPrice * quantity,
                IsCombo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.TransactionDetails.Add(comboDetail);

            _logger.LogInformation(
                "Combo added to sale: {ComboName} x{Quantity}, Price: {Price}, Products: {ProductCount}",
                combo.Name, quantity, combo.ComboPrice, combo.ComboItems.Count
            );

            return comboDetail.TotalAmount;
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

                // ✅ ACTUALIZADO: Agregar nuevos detalles (productos Y combos)
                decimal totalAmount = 0;
                foreach (var item in request.Items)
                {
                    if (!item.ProductId.HasValue && !item.ComboId.HasValue)
                    {
                        throw new ArgumentException("Each item must have either ProductId or ComboId");
                    }

                    if (item.ProductId.HasValue && item.ComboId.HasValue)
                    {
                        throw new ArgumentException("Item cannot have both ProductId and ComboId");
                    }

                    if (item.ComboId.HasValue)
                    {
                        totalAmount += await ProcessComboItemAsync(sale, item.ComboId.Value, item.Quantity, request.CreatedBy);
                    }
                    else if (item.ProductId.HasValue)
                    {
                        totalAmount += await ProcessProductItemAsync(sale, item.ProductId.Value, item.Quantity, request.CreatedBy);
                    }
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

        /// <summary>
        /// Completar venta - ACTUALIZADO: Ahora maneja combos correctamente
        /// </summary>
        public async Task<SalesTransactionDto?> CompleteSaleAsync(Guid id, List<CreatePaymentRequest> payments)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var sale = await _context.SalesTransactions
                    .Include(st => st.TransactionDetails)
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

                // ✅ ACTUALIZADO: Actualizar inventario (maneja productos Y combos)
                var saleMovementType = await _context.InventoryMovementTypes
                    .FirstOrDefaultAsync(t => t.Name == "Venta");

                foreach (var detail in sale.TransactionDetails)
                {
                    // ✅ SI ES COMBO - Expandir y rebajar stock de cada producto
                    if (detail.IsCombo && detail.ComboId.HasValue)
                    {
                        await ProcessComboInventoryAsync(
                            detail.ComboId.Value,
                            detail.Quantity,
                            sale.Id,
                            saleMovementType?.Id,
                            payments.FirstOrDefault()?.ProcessedBy
                        );
                    }
                    // ✅ SI ES PRODUCTO - Rebajar stock normalmente
                    else if (detail.ProductId.HasValue)
                    {
                        await ProcessProductInventoryAsync(
                            detail.ProductId.Value,
                            detail.Quantity,
                            sale.Id,
                            saleMovementType?.Id,
                            payments.FirstOrDefault()?.ProcessedBy
                        );
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

        /// <summary>
        /// ✅ NUEVO: Procesar inventario de un producto individual
        /// </summary>
        private async Task ProcessProductInventoryAsync(
            long productId,
            int quantity,
            long saleId,
            long? movementTypeId,
            string? performedBy)
        {
            var product = await _context.CategoryProducts.FindAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product {productId} not found");
            }

            var previousQuantity = product.CurrentQuantity;
            product.CurrentQuantity -= quantity;
            product.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Stock reduced: {ProductName} from {Previous} to {New} (-{Quantity})",
                product.Name, previousQuantity, product.CurrentQuantity, quantity
            );

            // Crear movimiento de inventario
            if (movementTypeId.HasValue)
            {
                var movement = new InventoryMovement
                {
                    ProductId = productId,
                    MovementTypeId = movementTypeId.Value,
                    Quantity = -quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = product.CurrentQuantity,
                    SalesTransactionId = saleId,
                    MovementDate = DateTime.UtcNow,
                    PerformedBy = performedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryMovements.Add(movement);
            }
        }

        /// <summary>
        /// ✅ NUEVO: Procesar inventario de un combo (expandir a productos)
        /// </summary>
        private async Task ProcessComboInventoryAsync(
            long comboId,
            int comboQuantity,
            long saleId,
            long? movementTypeId,
            string? performedBy)
        {
            // Cargar items del combo
            var comboItems = await _context.ComboItems
                .Include(ci => ci.Product)
                .Where(ci => ci.ComboId == comboId)
                .ToListAsync();

            if (!comboItems.Any())
            {
                throw new InvalidOperationException($"Combo {comboId} has no items");
            }

            var combo = await _context.SalesCombos.FindAsync(comboId);

            _logger.LogInformation(
                "Processing combo inventory: {ComboName} x{ComboQuantity}, Total products: {ProductCount}",
                combo?.Name ?? "Unknown", comboQuantity, comboItems.Count
            );

            // ✅ Rebajar stock de CADA producto del combo
            foreach (var comboItem in comboItems)
            {
                if (comboItem.Product == null)
                {
                    throw new InvalidOperationException($"Product {comboItem.ProductId} in combo {comboId} not found");
                }

                var totalQuantity = comboItem.Quantity * comboQuantity;
                
                _logger.LogInformation(
                    "  -> Combo item: {ProductName} x{ItemQty} * {ComboQty} = {Total}",
                    comboItem.Product.Name, comboItem.Quantity, comboQuantity, totalQuantity
                );

                await ProcessProductInventoryAsync(
                    comboItem.ProductId,
                    totalQuantity,
                    saleId,
                    movementTypeId,
                    performedBy
                );
            }

            _logger.LogInformation(
                "Combo inventory processed: {ComboName} x{Quantity}",
                combo?.Name ?? "Unknown", comboQuantity
            );
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

        /// <summary>
        /// Revertir inventario al cancelar venta completada - ACTUALIZADO: Maneja combos
        /// </summary>
        private async Task RevertInventoryAsync(SalesTransaction sale)
        {
            var details = await _context.TransactionDetails
                .Where(td => td.SalesTransactionId == sale.Id)
                .ToListAsync();

            var adjustmentType = await _context.InventoryMovementTypes
                .FirstOrDefaultAsync(t => t.Name == "Ajuste");

            foreach (var detail in details)
            {
                // ✅ SI ES COMBO - Revertir stock de cada producto del combo
                if (detail.IsCombo && detail.ComboId.HasValue)
                {
                    var comboItems = await _context.ComboItems
                        .Include(ci => ci.Product)
                        .Where(ci => ci.ComboId == detail.ComboId.Value)
                        .ToListAsync();

                    foreach (var comboItem in comboItems)
                    {
                        if (comboItem.Product == null) continue;

                        var totalQuantity = comboItem.Quantity * detail.Quantity;
                        var previousQuantity = comboItem.Product.CurrentQuantity;
                        
                        comboItem.Product.CurrentQuantity += totalQuantity;
                        comboItem.Product.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation(
                            "Reverting combo item stock: {ProductName} +{Quantity} (from {Previous} to {New})",
                            comboItem.Product.Name, totalQuantity, previousQuantity, comboItem.Product.CurrentQuantity
                        );

                        if (adjustmentType != null)
                        {
                            var movement = new InventoryMovement
                            {
                                ProductId = comboItem.ProductId,
                                MovementTypeId = adjustmentType.Id,
                                Quantity = totalQuantity,
                                PreviousQuantity = previousQuantity,
                                NewQuantity = comboItem.Product.CurrentQuantity,
                                SalesTransactionId = sale.Id,
                                Justification = $"Sale cancellation - combo revert",
                                MovementDate = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.InventoryMovements.Add(movement);
                        }
                    }
                }
                // ✅ SI ES PRODUCTO - Revertir normalmente
                else if (detail.ProductId.HasValue)
                {
                    var product = await _context.CategoryProducts.FindAsync(detail.ProductId.Value);
                    if (product == null) continue;

                    var previousQuantity = product.CurrentQuantity;
                    product.CurrentQuantity += detail.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Reverting product stock: {ProductName} +{Quantity} (from {Previous} to {New})",
                        product.Name, detail.Quantity, previousQuantity, product.CurrentQuantity
                    );

                    if (adjustmentType != null)
                    {
                        var movement = new InventoryMovement
                        {
                            ProductId = detail.ProductId.Value,
                            MovementTypeId = adjustmentType.Id,
                            Quantity = detail.Quantity,
                            PreviousQuantity = previousQuantity,
                            NewQuantity = product.CurrentQuantity,
                            SalesTransactionId = sale.Id,
                            Justification = "Sale cancellation - inventory revert",
                            MovementDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.InventoryMovements.Add(movement);
                    }
                }
            }
        }

        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            Array.Copy(longBytes, 0, bytes, 0, 8);
            bytes[8] = 0x5A; bytes[9] = 0x1E;
            return new Guid(bytes);
        }

        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}