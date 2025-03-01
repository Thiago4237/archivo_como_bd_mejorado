namespace ArchivoComoBaseDeDatosOptimizado.Structs;

/// <summary>
/// estructura con la cual se van a manejar los datos internamente 
/// es decir en momento de ejecucion
/// </summary>
public struct AulaStruct
{
    public string CodigoAula;    // 10 caracteres
    public double Largo;         // 8 bytes
    public double Ancho;         // 8 bytes
    public int Capacidad;        // 4 bytes
    public string Color;         // 20 caracteres
    public short TipoAula;       // 2 bytes
}
