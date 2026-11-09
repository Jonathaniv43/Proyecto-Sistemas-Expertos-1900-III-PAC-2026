using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Common;
using Norvia.Infrastructure.Persistence.Conversiones;

namespace Norvia.Infrastructure.Persistence.Configurations;

/// <summary>Atajos para aplicar las convenciones del modelo de datos (§1 y §2).</summary>
internal static class ConfiguracionExtensions
{
    /// <summary>Columna "*En" con la hora del servidor (DEFAULT now()).</summary>
    public static PropertyBuilder<DateTime> FechaDelServidor(this PropertyBuilder<DateTime> propiedad) =>
        propiedad.HasDefaultValueSql("now()");

    /// <summary>
    /// Booleano con DEFAULT true. El centinela true hace que EF sí envíe los false
    /// (sin él, EF trataría false como "sin asignar" y la base pondría true).
    /// </summary>
    public static PropertyBuilder<bool> PorDefectoTrue(this PropertyBuilder<bool> propiedad) =>
        propiedad.HasDefaultValue(true).HasSentinel(true);

    public static PropertyBuilder<bool> PorDefectoFalse(this PropertyBuilder<bool> propiedad) =>
        propiedad.HasDefaultValue(false);

    /// <summary>Enum guardado como varchar(n) en snake_case, con su CHECK (... IN (...)).</summary>
    public static EntityTypeBuilder<T> EnumComoTexto<T, TEnum>(
        this EntityTypeBuilder<T> entidad, System.Linq.Expressions.Expression<Func<T, TEnum>> propiedad, int largo)
        where T : class
        where TEnum : struct, Enum
    {
        var p = entidad.Property(propiedad).HasConversion(new SnakeCaseEnumConverter<TEnum>()).HasMaxLength(largo);
        return entidad.CheckEnum<T, TEnum>(p.Metadata.Name);
    }

    public static EntityTypeBuilder<T> EnumComoTexto<T, TEnum>(
        this EntityTypeBuilder<T> entidad, System.Linq.Expressions.Expression<Func<T, TEnum?>> propiedad, int largo)
        where T : class
        where TEnum : struct, Enum
    {
        var p = entidad.Property(propiedad).HasConversion(new SnakeCaseEnumConverter<TEnum>()).HasMaxLength(largo);
        return entidad.CheckEnum<T, TEnum>(p.Metadata.Name);
    }

    private static EntityTypeBuilder<T> CheckEnum<T, TEnum>(this EntityTypeBuilder<T> entidad, string columna)
        where T : class
        where TEnum : struct, Enum
    {
        var valores = string.Join(", ", SnakeCaseEnumConverter<TEnum>.Valores.Select(v => $"'{v}'"));
        return entidad.Check(columna, $"\"{columna}\" IN ({valores})");
    }

    /// <summary>CHECK con nombre CK_{Tabla}_{sufijo}.</summary>
    public static EntityTypeBuilder<T> Check<T>(this EntityTypeBuilder<T> entidad, string sufijo, string sql)
        where T : class
    {
        entidad.ToTable(t => t.HasCheckConstraint($"CK_{typeof(T).Name}_{sufijo}", sql));
        return entidad;
    }

    /// <summary>
    /// Reglas comunes de toda tabla de empresa (§2): EmpresaId obligatorio y el único
    /// (EmpresaId, Id), que es el destino de las FK compuestas entre tablas de empresa.
    /// La FK simple hacia Empresa se configura en cada entidad, porque algunas tienen navegación y otras no.
    /// </summary>
    public static EntityTypeBuilder<T> TablaDeEmpresa<T>(this EntityTypeBuilder<T> entidad)
        where T : class, ITieneEmpresa
    {
        entidad.Property(e => e.EmpresaId).IsRequired();
        entidad.HasAlternateKey(nameof(ITieneEmpresa.EmpresaId), "Id");
        return entidad;
    }
}
