namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>Filas iniciales; <see cref="TiposDocumentoIdentidadSeeder"/> las sincroniza a la BD.</summary>
public static class CatalogoTiposDocumentoIdentidad
{
    public static readonly (string Codigo, string Nombre)[] Filas =
    [
        ("dni", "DNI / identidad"),
        ("pasaporte", "Pasaporte"),
        ("otro", "Otro"),
    ];
}