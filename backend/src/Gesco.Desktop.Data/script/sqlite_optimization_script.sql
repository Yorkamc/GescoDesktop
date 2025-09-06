-- =====================================================
-- GESCO SQLite Database Optimization Script
-- Basado en las entidades del backend .NET Core
-- =====================================================

-- Habilitar foreign keys y WAL mode para mejor rendimiento
PRAGMA foreign_keys = ON;
PRAGMA journal_mode = WAL;
PRAGMA synchronous = NORMAL;
PRAGMA cache_size = 10000;
PRAGMA temp_store = MEMORY;

-- =====================================================
-- 1. ÍNDICES OPTIMIZADOS PARA CONSULTAS FRECUENTES
-- =====================================================

-- Índices para autenticación y usuarios
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_org_active ON users(organization_id, active);
CREATE INDEX IF NOT EXISTS idx_users_last_login ON users(last_login_at DESC);

-- Índices para organizaciones
CREATE INDEX IF NOT EXISTS idx_organizations_active ON organizations(active);
CREATE INDEX IF NOT EXISTS idx_organizations_name ON organizations(name);

-- Índices para actividades
CREATE INDEX IF NOT EXISTS idx_activities_org_status ON activities(organization_id, activity_status_id);
CREATE INDEX IF NOT EXISTS idx_activities_date_range ON activities(start_date, end_date);
CREATE INDEX IF NOT EXISTS idx_activities_manager ON activities(manager_user_id);

-- Índices para productos y categorías
CREATE INDEX IF NOT EXISTS idx_category_products_category ON category_products(activity_category_id);
CREATE INDEX IF NOT EXISTS idx_category_products_active ON category_products(active);
CREATE INDEX IF NOT EXISTS idx_category_products_stock ON category_products(current_quantity, alert_quantity);
CREATE INDEX IF NOT EXISTS idx_category_products_price ON category_products(unit_price);
CREATE INDEX IF NOT EXISTS idx_category_products_name ON category_products(name);

-- Índices para ventas y transacciones
CREATE INDEX IF NOT EXISTS idx_sales_transactions_register ON sales_transactions(cash_register_id);
CREATE INDEX IF NOT EXISTS idx_sales_transactions_date ON sales_transactions(transaction_date DESC);
CREATE INDEX IF NOT EXISTS idx_sales_transactions_status ON sales_transactions(sales_status_id);
CREATE INDEX IF NOT EXISTS idx_sales_transactions_date_status ON sales_transactions(transaction_date, sales_status_id);
CREATE INDEX IF NOT EXISTS idx_sales_transactions_total ON sales_transactions(total_amount);

-- Índices para detalles de transacciones
CREATE INDEX IF NOT EXISTS idx_transaction_details_sale ON transaction_details(sales_transaction_id);
CREATE INDEX IF NOT EXISTS idx_transaction_details_product ON transaction_details(product_id);
CREATE INDEX IF NOT EXISTS idx_transaction_details_combo ON transaction_details(combo_id);

-- Índices para pagos
CREATE INDEX IF NOT EXISTS idx_transaction_payments_sale ON transaction_payments(sales_transaction_id);
CREATE INDEX IF NOT EXISTS idx_transaction_payments_method ON transaction_payments(payment_method_id);
CREATE INDEX IF NOT EXISTS idx_transaction_payments_date ON transaction_payments(processed_at);

-- Índices para inventario
CREATE INDEX IF NOT EXISTS idx_inventory_movements_product ON inventory_movements(product_id);
CREATE INDEX IF NOT EXISTS idx_inventory_movements_type ON inventory_movements(movement_type_id);
CREATE INDEX IF NOT EXISTS idx_inventory_movements_date ON inventory_movements(movement_date DESC);
CREATE INDEX IF NOT EXISTS idx_inventory_movements_sale ON inventory_movements(sales_transaction_id);

-- Índices para cajas registradoras
CREATE INDEX IF NOT EXISTS idx_cash_registers_activity ON cash_registers(activity_id);
CREATE INDEX IF NOT EXISTS idx_cash_registers_open ON cash_registers(is_open);
CREATE INDEX IF NOT EXISTS idx_cash_registers_operator ON cash_registers(operator_user_id);

-- Índices para cierres
CREATE INDEX IF NOT EXISTS idx_cash_closures_register ON cash_register_closures(cash_register_id);
CREATE INDEX IF NOT EXISTS idx_cash_closures_date ON cash_register_closures(closing_date DESC);
CREATE INDEX IF NOT EXISTS idx_activity_closures_activity ON activity_closures(activity_id);
CREATE INDEX IF NOT EXISTS idx_activity_closures_date ON activity_closures(closure_date DESC);

-- Índices para sincronización
CREATE INDEX IF NOT EXISTS idx_sync_version_org ON organizations(sync_version);
CREATE INDEX IF NOT EXISTS idx_sync_version_activities ON activities(sync_version);
CREATE INDEX IF NOT EXISTS idx_sync_version_products ON category_products(sync_version);
CREATE INDEX IF NOT EXISTS idx_sync_version_sales ON sales_transactions(sync_version);

-- Índices para activación y licencias
CREATE INDEX IF NOT EXISTS idx_activation_keys_code ON activation_keys(activation_code);
CREATE INDEX IF NOT EXISTS idx_activation_keys_org ON activation_keys(used_by_organization_id);
CREATE INDEX IF NOT EXISTS idx_activation_keys_status ON activation_keys(is_used, is_expired, is_revoked);

-- =====================================================
-- 2. VISTAS OPTIMIZADAS PARA EL BACKEND
-- =====================================================

-- Vista para dashboard statistics
CREATE VIEW IF NOT EXISTS v_dashboard_stats AS
SELECT 
    -- Actividades
    (SELECT COUNT(*) FROM activities) AS total_activities,
    (SELECT COUNT(*) FROM activities WHERE activity_status_id = 2) AS active_activities, -- En curso
    
    -- Ventas del día
    (SELECT COALESCE(SUM(total_amount), 0) 
     FROM sales_transactions 
     WHERE DATE(transaction_date) = DATE('now', 'localtime') 
     AND sales_status_id = 2) AS today_sales,
    
    (SELECT COUNT(*) 
     FROM sales_transactions 
     WHERE DATE(transaction_date) = DATE('now', 'localtime')) AS today_transactions,
    
    -- Ventas del mes
    (SELECT COALESCE(SUM(total_amount), 0) 
     FROM sales_transactions 
     WHERE strftime('%Y-%m', transaction_date) = strftime('%Y-%m', 'now', 'localtime')
     AND sales_status_id = 2) AS month_sales,
    
    (SELECT COUNT(*) 
     FROM sales_transactions 
     WHERE strftime('%Y-%m', transaction_date) = strftime('%Y-%m', 'now', 'localtime')) AS month_transactions,
    
    -- Usuarios
    (SELECT COUNT(*) FROM users) AS total_users,
    (SELECT COUNT(*) FROM users WHERE active = 1) AS active_users,
    
    -- Productos
    (SELECT COUNT(*) FROM category_products) AS total_products,
    (SELECT COUNT(*) FROM category_products WHERE active = 1) AS active_products,
    (SELECT COUNT(*) FROM category_products WHERE current_quantity <= alert_quantity AND active = 1) AS low_stock_products,
    
    datetime('now', 'localtime') AS report_date;

-- Vista para transacciones con detalles completos
CREATE VIEW IF NOT EXISTS v_sales_transactions_detailed AS
SELECT 
    st.id,
    st.transaction_number,
    st.invoice_number,
    st.transaction_date,
    st.total_amount,
    ss.name AS status_name,
    cr.name AS register_name,
    cr.register_number,
    a.name AS activity_name,
    u.username AS operator_username,
    COUNT(td.id) AS total_items,
    SUM(td.quantity) AS total_quantity,
    st.created_at,
    st.updated_at
FROM sales_transactions st
JOIN sales_statuses ss ON st.sales_status_id = ss.id
JOIN cash_registers cr ON st.cash_register_id = cr.id
JOIN activities a ON cr.activity_id = a.id
LEFT JOIN users u ON cr.operator_user_id = u.id
LEFT JOIN transaction_details td ON st.id = td.sales_transaction_id
GROUP BY st.id, st.transaction_number, st.invoice_number, st.transaction_date,
         st.total_amount, ss.name, cr.name, cr.register_number, a.name,
         u.username, st.created_at, st.updated_at;

-- Vista para productos con stock bajo
CREATE VIEW IF NOT EXISTS v_low_stock_products AS
SELECT 
    cp.id,
    cp.code,
    cp.name,
    cp.current_quantity,
    cp.alert_quantity,
    cp.unit_price,
    (cp.alert_quantity - cp.current_quantity) AS quantity_needed,
    sc.name AS category_name,
    a.name AS activity_name,
    o.name AS organization_name
FROM category_products cp
JOIN activity_categories ac ON cp.activity_category_id = ac.id
JOIN service_categories sc ON ac.service_category_id = sc.id
JOIN activities a ON ac.activity_id = a.id
LEFT JOIN organizations o ON a.organization_id = o.id
WHERE cp.current_quantity <= cp.alert_quantity
AND cp.active = 1
ORDER BY (cp.alert_quantity - cp.current_quantity) DESC;

-- Vista para análisis de ventas por día
CREATE VIEW IF NOT EXISTS v_daily_sales_summary AS
SELECT 
    DATE(st.transaction_date) AS sale_date,
    COUNT(*) AS transaction_count,
    SUM(st.total_amount) AS total_sales,
    AVG(st.total_amount) AS average_transaction,
    SUM(td.quantity) AS total_items_sold,
    COUNT(DISTINCT st.cash_register_id) AS registers_used,
    COUNT(DISTINCT cr.activity_id) AS activities_involved
FROM sales_transactions st
JOIN cash_registers cr ON st.cash_register_id = cr.id
LEFT JOIN transaction_details td ON st.id = td.sales_transaction_id
WHERE st.sales_status_id = 2 -- completed
GROUP BY DATE(st.transaction_date)
ORDER BY sale_date DESC;

-- Vista para productos más vendidos
CREATE VIEW IF NOT EXISTS v_top_selling_products AS
SELECT 
    cp.id,
    cp.name,
    cp.code,
    cp.unit_price,
    SUM(td.quantity) AS total_sold,
    SUM(td.total_amount) AS total_revenue,
    COUNT(DISTINCT td.sales_transaction_id) AS times_sold,
    AVG(td.unit_price) AS average_sale_price,
    DATE(MAX(st.transaction_date)) AS last_sold_date
FROM category_products cp
JOIN transaction_details td ON cp.id = td.product_id
JOIN sales_transactions st ON td.sales_transaction_id = st.id
WHERE st.sales_status_id = 2 -- completed
AND st.transaction_date >= DATE('now', '-30 days') -- últimos 30 días
GROUP BY cp.id, cp.name, cp.code, cp.unit_price
ORDER BY total_sold DESC;

-- Vista para actividades con estadísticas
CREATE VIEW IF NOT EXISTS v_activities_with_stats AS
SELECT 
    a.id,
    a.name,
    a.description,
    a.start_date,
    a.end_date,
    a.location,
    ast.name AS status_name,
    o.name AS organization_name,
    u.username AS manager_username,
    -- Estadísticas de ventas
    COALESCE(sales.total_sales, 0) AS total_sales,
    COALESCE(sales.transaction_count, 0) AS transaction_count,
    -- Estadísticas de productos
    COALESCE(products.total_products, 0) AS total_products,
    COALESCE(products.active_products, 0) AS active_products,
    -- Estadísticas de cajas
    COALESCE(registers.total_registers, 0) AS total_registers,
    COALESCE(registers.open_registers, 0) AS open_registers,
    a.created_at,
    a.updated_at
FROM activities a
JOIN activity_statuses ast ON a.activity_status_id = ast.id
LEFT JOIN organizations o ON a.organization_id = o.id
LEFT JOIN users u ON a.manager_user_id = u.id
LEFT JOIN (
    SELECT 
        cr.activity_id,
        SUM(st.total_amount) AS total_sales,
        COUNT(st.id) AS transaction_count
    FROM cash_registers cr
    JOIN sales_transactions st ON cr.id = st.cash_register_id
    WHERE st.sales_status_id = 2
    GROUP BY cr.activity_id
) sales ON a.id = sales.activity_id
LEFT JOIN (
    SELECT 
        a2.id AS activity_id,
        COUNT(cp.id) AS total_products,
        COUNT(CASE WHEN cp.active = 1 THEN 1 END) AS active_products
    FROM activities a2
    JOIN activity_categories ac ON a2.id = ac.activity_id
    JOIN category_products cp ON ac.id = cp.activity_category_id
    GROUP BY a2.id
) products ON a.id = products.activity_id
LEFT JOIN (
    SELECT 
        activity_id,
        COUNT(*) AS total_registers,
        COUNT(CASE WHEN is_open = 1 THEN 1 END) AS open_registers
    FROM cash_registers
    GROUP BY activity_id
) registers ON a.id = registers.activity_id;

-- =====================================================
-- 3. TRIGGERS PARA AUTOMATIZACIÓN
-- =====================================================

-- Trigger para actualizar sync_version automáticamente
CREATE TRIGGER IF NOT EXISTS update_sync_version_organizations
    AFTER UPDATE ON organizations
    FOR EACH ROW
    WHEN NEW.sync_version = OLD.sync_version
BEGIN
    UPDATE organizations 
    SET sync_version = OLD.sync_version + 1,
        last_sync = NULL,
        updated_at = datetime('now')
    WHERE id = NEW.id;
END;

CREATE TRIGGER IF NOT EXISTS update_sync_version_activities
    AFTER UPDATE ON activities
    FOR EACH ROW
    WHEN NEW.sync_version = OLD.sync_version
BEGIN
    UPDATE activities 
    SET sync_version = OLD.sync_version + 1,
        last_sync = NULL,
        updated_at = datetime('now')
    WHERE id = NEW.id;
END;

CREATE TRIGGER IF NOT EXISTS update_sync_version_products
    AFTER UPDATE ON category_products
    FOR EACH ROW
    WHEN NEW.sync_version = OLD.sync_version
BEGIN
    UPDATE category_products 
    SET sync_version = OLD.sync_version + 1,
        last_sync = NULL,
        updated_at = datetime('now')
    WHERE id = NEW.id;
END;

-- Trigger para generar números de transacción automáticamente
CREATE TRIGGER IF NOT EXISTS generate_transaction_number
    AFTER INSERT ON sales_transactions
    FOR EACH ROW
    WHEN NEW.transaction_number IS NULL OR NEW.transaction_number = ''
BEGIN
    UPDATE sales_transactions 
    SET transaction_number = 'TXN-' || NEW.cash_register_id || '-' || 
                           strftime('%Y%m%d', 'now', 'localtime') || '-' ||
                           printf('%04d', 
                               (SELECT COUNT(*) + 1 
                                FROM sales_transactions st2 
                                WHERE st2.cash_register_id = NEW.cash_register_id 
                                AND DATE(st2.transaction_date) = DATE('now', 'localtime')
                                AND st2.id <= NEW.id))
    WHERE id = NEW.id;
END;

-- Trigger para actualizar stock automáticamente cuando se completa una venta
CREATE TRIGGER IF NOT EXISTS update_stock_on_sale_complete
    AFTER UPDATE OF sales_status_id ON sales_transactions
    FOR EACH ROW
    WHEN NEW.sales_status_id = 2 AND OLD.sales_status_id != 2 -- Completada
BEGIN
    -- Actualizar stock de productos vendidos
    UPDATE category_products 
    SET current_quantity = current_quantity - (
        SELECT COALESCE(SUM(td.quantity), 0)
        FROM transaction_details td
        WHERE td.sales_transaction_id = NEW.id 
        AND td.product_id = category_products.id
    ),
    updated_at = datetime('now')
    WHERE id IN (
        SELECT DISTINCT td.product_id
        FROM transaction_details td
        WHERE td.sales_transaction_id = NEW.id 
        AND td.product_id IS NOT NULL
    );
    
    -- Crear movimientos de inventario
    INSERT INTO inventory_movements (
        product_id, movement_type_id, quantity, previous_quantity,
        new_quantity, sales_transaction_id, performed_by, movement_date, created_at
    )
    SELECT 
        td.product_id,
        2, -- Sale movement type
        -td.quantity,
        cp.current_quantity + td.quantity, -- Previous quantity
        cp.current_quantity, -- New quantity after update
        NEW.id,
        NEW.updated_by,
        NEW.transaction_date,
        datetime('now')
    FROM transaction_details td
    JOIN category_products cp ON td.product_id = cp.id
    WHERE td.sales_transaction_id = NEW.id 
    AND td.product_id IS NOT NULL;
END;

-- =====================================================
-- 4. CONFIGURACIONES DE RENDIMIENTO
-- =====================================================

-- Configurar PRAGMA para mejor rendimiento
PRAGMA optimize;
PRAGMA analysis_limit = 1000;

-- Crear estadísticas para el optimizador
ANALYZE;

-- =====================================================
-- 5. FUNCIONES DE UTILIDAD (usando CTE y subconsultas)
-- =====================================================

-- Esta vista simula una función para obtener estadísticas de actividad
CREATE VIEW IF NOT EXISTS v_activity_performance AS
WITH activity_stats AS (
    SELECT 
        a.id AS activity_id,
        a.name AS activity_name,
        COUNT(DISTINCT cr.id) AS total_registers,
        COUNT(DISTINCT st.id) AS total_transactions,
        COALESCE(SUM(st.total_amount), 0) AS total_revenue,
        COUNT(DISTINCT DATE(st.transaction_date)) AS active_days,
        AVG(st.total_amount) AS avg_transaction_value
    FROM activities a
    LEFT JOIN cash_registers cr ON a.id = cr.activity_id
    LEFT JOIN sales_transactions st ON cr.id = st.cash_register_id AND st.sales_status_id = 2
    GROUP BY a.id, a.name
)
SELECT 
    *,
    CASE 
        WHEN active_days > 0 THEN total_revenue / active_days 
        ELSE 0 
    END AS daily_average_revenue,
    CASE 
        WHEN total_registers > 0 THEN total_transactions / total_registers 
        ELSE 0 
    END AS transactions_per_register
FROM activity_stats;

-- =====================================================
-- OPTIMIZACIONES FINALES
-- =====================================================

-- Forzar que SQLite use los índices recién creados
PRAGMA optimize;

-- Verificar integridad después de las optimizaciones
PRAGMA integrity_check;

-- =====================================================
-- SCRIPT COMPLETADO
-- =====================================================
-- Este script optimiza SQLite para:
-- 1. Consultas más rápidas con índices específicos
-- 2. Vistas que reducen joins complejos en el backend
-- 3. Triggers que automatizan procesos de negocio
-- 4. Configuraciones de rendimiento mejoradas
-- =====================================================