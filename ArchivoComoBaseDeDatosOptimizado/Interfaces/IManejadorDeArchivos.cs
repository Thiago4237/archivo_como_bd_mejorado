using ArchivoComoBaseDeDatosOptimizado.Structs;

namespace ArchivoComoBaseDeDatosOptimizado.Interfaces;

internal interface IManejadorDeArchivos
{
    public void Add(AulaStruct aula);
    public bool Delete(string codigoAula);
    public bool Update(string codigoAula, AulaStruct updatedAula);
    public IEnumerable<AulaStruct> ListAll();
    public void Export(bool formatoLegible = false);
    public AulaStruct? FindByCodigo(string codigoAula);
    public void Import(string sourcePath);
}
