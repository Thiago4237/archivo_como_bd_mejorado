using ArchivoComoBaseDeDatosOptimizado.Interfaces;
using ArchivoComoBaseDeDatosOptimizado.Structs;
using System.Globalization;
using System.Text;

namespace ArchivoComoBaseDeDatosOptimizado.Logic;

internal class ManejadorDeArchivos: IManejadorDeArchivos
{
    /// <summary>
    /// tamaño del registro 
    /// codigo aula = 10
    /// color = 20
    /// ancho y largo = 8
    /// capacidad = 4
    /// tipo = 2 (codigo del dato asociado)
    /// </summary>
    private const int _RecordSize = 52;
    private readonly string _filePath = "C:\\Temp\\aulas.dat";

    /// <summary>
    /// esta funcion:
    /// recibe los datos del aula
    /// saca los bytes de los datos de color y codigo, haciendo que cumpla con el tamaño estabelcido
    /// posteriormente se organiza en memora el retorno de la informacion en forma de bytes, 
    /// los cuales contienen toda la informacion especificada
    /// </summary>
    /// <param name="aula"></param>
    /// <returns></returns>
    private byte[] SerializeAula(AulaStruct aula)
    {
        byte[] codigo = Encoding.UTF8.GetBytes(aula.CodigoAula.PadRight(10).Substring(0, 10));
        byte[] color = Encoding.UTF8.GetBytes(aula.Color.PadRight(20).Substring(0, 20));

        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(codigo, 0, 10);
            ms.Write(color, 0, 20);

            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(aula.Largo);
                writer.Write(aula.Ancho);
                writer.Write(aula.Capacidad);
                writer.Write(aula.TipoAula);
            }
            return ms.ToArray();
        }
    }

    /// <summary>
    /// esta fucion lee los bytes y los parseae en 
    /// los tipos de datos que corresponden, ya sea cadena o numero 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private AulaStruct DeserializeAula(byte[] data)
    {
        return new AulaStruct
        {
            CodigoAula = Encoding.UTF8.GetString(data, 0, 10).Trim(),
            Color = Encoding.UTF8.GetString(data, 10, 20).Trim(),
            Largo = BitConverter.ToDouble(data, 30),
            Ancho = BitConverter.ToDouble(data, 38),
            Capacidad = BitConverter.ToInt32(data, 46),
            TipoAula = BitConverter.ToInt16(data, 50)
        };
    }

    /// <summary>
    /// esta funcion:
    /// recibe los datos del aula en un "modelo"
    /// abre el mode de escritura para el archivo en la ruta especificada 
    /// serializa como byte's los datos del aula 
    /// hace la insercion al final del registro
    /// </summary>
    /// <param name="aula"></param>
    public void Add(AulaStruct aula)
    {
        using (var fs = new FileStream(_filePath, FileMode.Append, FileAccess.Write))
        {
            byte[] record = SerializeAula(aula);
            fs.Write(record, 0, _RecordSize);
        }
    }

    /// <summary>
    /// esta funcion:
    /// solicita el codigo del aula 
    /// abre el archivo en modo de lectura y escritura 
    /// valida que el archivo tenga datos para realizar el proceso
    /// busca la posicion en la cual se encuentra el codigo indicado 
    /// si no se encuentra se finaliza sino 
    /// donde el valor fue encontrado este, es remplazado por el ultimo registro del archivo
    /// se reduce el tamaño del archivo
    /// </summary>
    /// <param name="codigoAula"></param>
    /// <returns></returns>
    public bool Delete(string codigoAula)
    {
        using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite))
        {
            //longitud del archivo
            long fileSize = fs.Length;

            if (fileSize == 0)
            {
                return false;
            }
            // inicializa variables a usar 
            long lastPos = fileSize - _RecordSize;
            long deletePos = -1;

            // Búsqueda optimizada
            for (long pos = 0; pos <= lastPos; pos += _RecordSize)
            {
                // seek ayuda con el posicionamiento del puntero en el archivo 
                fs.Seek(pos, SeekOrigin.Begin);
                // lee la informacion y la guarda en memoria (solo el registro leido)
                byte[] buffer = new byte[10];
                fs.Read(buffer, 0, 10);
                // saca el codigo del registro
                string currentCodigo = Encoding.UTF8.GetString(buffer).Trim();
                // si el codigo obtenido y el indicado son iguales finaliza esta seccion
                if (currentCodigo == codigoAula)
                {
                    deletePos = pos;
                    break;
                }
            }
            
            // en caso de no encontrar se finaliza la ejecucion de la opcion
            if (deletePos == -1) { 
                return false;
            }

            // Mover último registro
            fs.Seek(lastPos, SeekOrigin.Begin);
            byte[] lastRecord = new byte[_RecordSize];
            fs.Read(lastRecord, 0, _RecordSize);

            fs.Seek(deletePos, SeekOrigin.Begin);
            fs.Write(lastRecord, 0, _RecordSize);

            // Truncar archivo
            fs.SetLength(fileSize - _RecordSize);
            return true;
        }
    }

    /// <summary>
    /// esta fucnion
    /// recibe el codigo del aula a modificar y la informacion con la cual se ha de modificar
    /// abre el archivo en modo lectura y escritura
    /// va posicion a posicion del archivo hasta encontrar el coodigo indicado
    /// se hace el remplzao de la informacion en la linea indicada 
    /// </summary>
    /// <param name="codigoAula"></param>
    /// <param name="updatedAula"></param>
    /// <returns></returns>
    public bool Update(string codigoAula, AulaStruct updatedAula)
    {
        using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite))
        {
            for (long pos = 0; pos < fs.Length; pos += _RecordSize)
            {
                // salta posicion a posicion en el archivo para sacar los datos
                fs.Seek(pos, SeekOrigin.Begin);
                byte[] buffer = new byte[10];
                fs.Read(buffer, 0, 10);

                // saca el codigo del registro
                string currentCodigo = Encoding.UTF8.GetString(buffer).Trim();

                // si los codigos son iguales se organiza la informacion y finaliza el proceso
                if (currentCodigo == codigoAula)
                {
                    byte[] record = SerializeAula(updatedAula);
                    fs.Seek(pos, SeekOrigin.Begin);
                    fs.Write(record, 0, _RecordSize);
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// esta funcion:
    /// valida existencia del archivo fuente
    /// abre el archivo en modo lectura
    /// retorna la informacion del listado leido luego de convertir los bytes a algo legible 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AulaStruct> ListAll()
    {
        // valida si el archivo no existe en la ubicacion indicada y finaliza el proceso
        if (!File.Exists(_filePath)) yield break;

        using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[_RecordSize];
            while (fs.Read(buffer, 0, _RecordSize) == _RecordSize)
            {
                yield return DeserializeAula(buffer);
            }
        }
    }

    /// <summary>
    /// esta funcion 
    /// se encarga de realizar la exportacion de los datos 
    /// agregando una cabecera para mejorar la lectura de la informacion
    /// </summary>
    /// <param name="destPath"></param>
    /// <param name="formatoLegible"></param>
    public void Export(bool formatoLegible = false)
    {
        string pathExport = Path.Combine("C:", "temp");
        if (Directory.Exists(pathExport))
        {
            Directory.CreateDirectory(pathExport);
        }

        string extencion = (formatoLegible) ? "txt" : "dat";
        string path = Path.Combine(pathExport, $"export{DateTime.Now.Ticks}.{extencion}");

        if (formatoLegible)
        {
            ExportarFormatoLegible(path);
        }
        else
        {
            File.Copy(_filePath, path, true); // Copia binaria original
        }
    }

    /// <summary>
    /// coloca de forma legible en caracteres utf la informacion para la exportacion 
    /// </summary>
    /// <param name="destPath"></param>
    private void ExportarFormatoLegible(string destPath)    
    {
        using (StreamWriter writer = new(destPath, false, Encoding.UTF8))
        {
            // Encabezado en español
            writer.WriteLine("Código|Largo|Ancho|Capacidad|Color|Tipo de Aula");

            foreach (AulaStruct aula in ListAll())
            {
                writer.WriteLine($"{aula.CodigoAula.Trim()} | " +
                    $"{aula.Largo.ToString("F2", CultureInfo.GetCultureInfo("es-ES"))} | " +
                    $"{aula.Ancho.ToString("F2", CultureInfo.GetCultureInfo("es-ES"))} | " +
                    $"{aula.Capacidad} | " +
                    $"{aula.Color.Trim()} | " +
                    $"{aula.TipoAula}");
            }
        }
    }

    /// <summary>
    /// esta funcion 
    /// recibe el codigo del aula buscado
    /// abre el archivo en modo de lectura 
    /// en caso de que el codigo coincida con con el indicado se retorna la informacion
    /// relacionada cambiando de binario a utf
    /// </summary>
    /// <param name="codigoAula"></param>
    /// <returns></returns>
    public AulaStruct? FindByCodigo(string codigoAula)
    {
        using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] codigoBuffer = new byte[10];
            byte[] recordBuffer = new byte[_RecordSize];

            while (fs.Read(recordBuffer, 0, _RecordSize) == _RecordSize)
            {
                Array.Copy(recordBuffer, codigoBuffer, 10);
                string currentCodigo = Encoding.UTF8.GetString(codigoBuffer).Trim();

                if (currentCodigo == codigoAula)
                {
                    return DeserializeAula(recordBuffer);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// importa un archivo indicado para registrar los datos en la "bd"
    /// </summary>
    /// <param name="sourcePath"></param>
    public void Import(string sourcePath)
    {
        using (var source = File.OpenRead(sourcePath))
        using (var dest = new FileStream(_filePath, FileMode.Append, FileAccess.Write))
        {
            source.CopyTo(dest);
        }
    }

}
