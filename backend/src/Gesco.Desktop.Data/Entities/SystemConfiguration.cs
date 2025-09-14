using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
    // CONFIGURACIÓN DEL SISTEMA - CAMBIA A int
    [Table("system_configuration")] // CORREGIDO: Singular para coincidir con migraciones Laravel
    public class SystemConfiguration : AuditableEntityInt
    {
        [Column("key")]
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        [Required]
        public string Value { get; set; } = string.Empty;

        [Column("data_type")]
        [MaxLength(20)]
        public string DataType { get; set; } = "string"; // string, int, bool, decimal

        [Column("category")]
        [MaxLength(50)]
        public string Category { get; set; } = "general";

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("is_editable")]
        public bool IsEditable { get; set; } = true;

        [Column("access_level")]
        [MaxLength(20)]
        public string AccessLevel { get; set; } = "admin"; // admin, user, system

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("validation_pattern")]
        [MaxLength(200)]
        public string? ValidationPattern { get; set; }

        [Column("min_value")]
        public decimal? MinValue { get; set; }

        [Column("max_value")]
        public decimal? MaxValue { get; set; }

        [Column("allowed_values")]
        public string? AllowedValues { get; set; } // JSON array for enum-like values

        [Column("is_sensitive")]
        public bool IsSensitive { get; set; } = false; // For passwords, API keys, etc.

        [Column("environment")]
        [MaxLength(20)]
        public string Environment { get; set; } = "all"; // development, production, all

        [Column("restart_required")]
        public bool RestartRequired { get; set; } = false;
        [Column("organization_id")]
public Guid? OrganizationId { get; set; } // Faltaba este campo

[ForeignKey("OrganizationId")]
public virtual Organization? Organization { get; set; }

        // Métodos de utilidad
        public T? GetValue<T>()
        {
            if (string.IsNullOrEmpty(Value))
                return default(T);

            try
            {
                return (T)Convert.ChangeType(Value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public bool SetValue<T>(T value)
        {
            try
            {
                Value = value?.ToString() ?? string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GetBoolValue() => GetValue<bool>();
        public int GetIntValue() => GetValue<int>();
        public decimal GetDecimalValue() => GetValue<decimal>();
        public string GetStringValue() => Value;

        public void SetBoolValue(bool value) => SetValue(value);
        public void SetIntValue(int value) => SetValue(value);
        public void SetDecimalValue(decimal value) => SetValue(value);
        public void SetStringValue(string value) => Value = value ?? string.Empty;
    }

    // DTO para las configuraciones del sistema
    public class SystemConfigurationDto
    {
        public int Id { get; set; } // CAMBIA a int
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEditable { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? ValidationPattern { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? AllowedValues { get; set; }
        public bool IsSensitive { get; set; }
        public string Environment { get; set; } = string.Empty;
        public bool RestartRequired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Request para actualizar configuración
    public class UpdateSystemConfigurationRequest
    {
        [Required]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        public string Value { get; set; } = string.Empty;
    }
}