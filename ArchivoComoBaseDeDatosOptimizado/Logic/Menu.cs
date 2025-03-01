using ArchivoComoBaseDeDatosOptimizado.Interfaces;
using ArchivoComoBaseDeDatosOptimizado.Structs;

namespace ArchivoComoBaseDeDatosOptimizado.Logic;

internal class Menu: IMenu
{

    private readonly IManejadorDeArchivos _manager;

    public Menu(IManejadorDeArchivos manejador)
    {
        _manager = manejador;
    }

    /// <summary>
    /// esta funcion es para mostrar el menu
    /// </summary>
    public void ShowMenu()
    {
        Console.WriteLine("SISTEMA DE GESTIÓN DE AULAS");
        Console.WriteLine("1. Agregar aula");
        Console.WriteLine("2. Eliminar aula");
        Console.WriteLine("3. Actualizar aula");
        Console.WriteLine("4. Listar todas las aulas");
        Console.WriteLine("5. Buscar aula por código");
        Console.WriteLine("6. Importar aulas desde archivo");
        Console.WriteLine("7. Exportar aulas a archivo");
        Console.WriteLine("8. Salir");
        Console.Write("Seleccione una opción: ");
    }

    /// <summary>
    /// se selecciona la opcion del menu a mostrar
    /// </summary>
    /// <param name="opcion"></param>
    public void Opcion(string opcion)
    {
        switch (opcion)
        {
            case "1": AddAula(); break;
            case "2": DeleteAula(); break;
            case "3": UpdateAula(); break;
            case "4": ListAulas(); break;
            case "5": SearchAula(); break;
            case "6": ImportAulas(); break;
            case "7": ExportAulas(); break;
            case "8": Environment.Exit(0); break;
            default: ShowError("Opción inválida"); break;
        }
    }

    /// <summary>
    /// esta funcion se encarga de realizar la insercion de una nueva aula
    /// </summary>
    private void AddAula()
    {
        try
        {
            AulaStruct aula = new AulaStruct();

            Console.Write("Código de aula (10 caracteres máx): ");
            aula.CodigoAula = ReadFixedString(10);

            Console.Write("Largo: ");
            aula.Largo = (double)ReadDouble();

            Console.Write("Ancho: ");
            aula.Ancho = (double)ReadDouble();

            Console.Write("Capacidad: ");
            aula.Capacidad = (int)ReadInt();

            Console.Write("Color (20 caracteres máx): ");
            aula.Color = ReadFixedString(20);

            Console.Write("Tipo de aula (short): ");
            aula.TipoAula = (short)ReadShort();

            _manager.Add(aula);
            ShowSuccess("Aula agregada exitosamente!");
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// esta funcion se encarga de realizar la eliminacion del aula 
    /// </summary>
    private void DeleteAula()
    {
        Console.Write("Ingrese código de aula a eliminar: ");
        string codigo = Console.ReadLine().Trim();

        if (_manager.Delete(codigo))
        {
            ShowSuccess("Aula eliminada correctamente");
        }
        else
        {
            ShowError("Aula no encontrada");
        }
    }

    /// <summary>
    /// esta funcion se encarga de actualizar la informacion del aula
    /// </summary>
    private void UpdateAula()
    {
        try
        {
            Console.Write("Ingrese código de aula a actualizar: ");
            string codigo = Console.ReadLine().Trim();

            var aula = _manager.FindByCodigo(codigo);
            if (!aula.HasValue)
            {
                ShowError("Aula no encontrada");
                return;
            }

            AulaStruct updated = aula.Value;

            Console.Write("\n Si no quiere modificar la informacion solo de enter para continuar. \n");

            Console.Write($"Nuevo largo ({updated.Largo}): ");
            updated.Largo = ReadDouble(true) ?? updated.Largo;

            Console.Write($"Nuevo ancho ({updated.Ancho}): ");
            updated.Ancho = ReadDouble(true) ?? updated.Ancho;

            Console.Write($"Nueva capacidad ({updated.Capacidad}): ");
            updated.Capacidad = ReadInt(true) ?? updated.Capacidad;

            Console.Write($"Nuevo color ({updated.Color}): ");
            updated.Color = ReadFixedString(20, true) ?? updated.Color;

            Console.Write($"Nuevo tipo ({updated.TipoAula}): ");
            updated.TipoAula = ReadShort(true) ?? updated.TipoAula;

            if (_manager.Update(codigo, updated))
            {
                ShowSuccess("Aula actualizada correctamente");
            }
            else
            {
                ShowError("Error al actualizar el aula");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// muestra el listado de las aulas regsitradas
    /// </summary>
    private void ListAulas()
    {
        Console.WriteLine("\nLISTADO DE AULAS:");
        int count = 0;
        foreach (var aula in _manager.ListAll())
        {
            PrintAula(aula);
            count++;
        }
        Console.WriteLine($"\nTotal aulas registradas: {count}");
        Console.ReadKey();
    }

    /// <summary>
    /// se encarga de realizar la busqueda del aula con 
    /// el codigo que se indique 
    /// </summary>
    private void SearchAula()
    {
        Console.Write("Ingrese código de aula a buscar: ");
        string codigo = Console.ReadLine().Trim();

        var aula = _manager.FindByCodigo(codigo);
        if (aula.HasValue)
        {
            Console.WriteLine("\nAULA ENCONTRADA:");
            PrintAula(aula.Value);
        }
        else
        {
            ShowError("Aula no encontrada");
        }
        Console.ReadKey();
    }

    /// <summary>
    /// esta funcion se encarga de realizar la importacion de la inforamacion 
    /// </summary>
    private void ImportAulas()
    {
        Console.Write("Ingrese ruta del archivo a importar: ");
        string path = Console.ReadLine().Trim();

        if (File.Exists(path))
        {
            _manager.Import(path);
            ShowSuccess("Importación completada");
        }
        else
        {
            ShowError("Archivo no encontrado");
        }
    }

    /// <summary>
    /// esta funcion se encarga de realizar la exportacion de la informacion a un txt
    /// </summary>
    private void ExportAulas()
    {
        
        Console.Write("¿Formato legible? (S/N): ");
        bool formatoLegible = Console.ReadLine().Trim().ToUpper() == "S";

        try
        {
            _manager.Export(formatoLegible);
            ShowSuccess($"Exportación completada en formato {(formatoLegible ? "legible" : "binario")}");
        }
        catch (Exception ex)
        {
            ShowError($"Error en exportación: {ex.Message}");
        }
    }

    /// <summary>
    /// coloca la informacion del aula en formato legible
    /// </summary>
    /// <param name="aula"></param>
    private void PrintAula(AulaStruct aula)
    {
        Console.WriteLine($"Código: {aula.CodigoAula}");
        Console.WriteLine($"Dimensiones: {aula.Largo}x{aula.Ancho}");
        Console.WriteLine($"Capacidad: {aula.Capacidad}");
        Console.WriteLine($"Color: {aula.Color}");
        Console.WriteLine($"Tipo: {aula.TipoAula}");
        Console.WriteLine(new string('-', 40));
    }

    /// <summary>
    /// lee un valor en cadena 
    /// </summary>
    /// <param name="maxLength"></param>
    /// <param name="allowEmpty"></param>
    /// <returns></returns>
    private string ReadFixedString(int maxLength, bool allowEmpty = false)
    {
        string input;
        do
        {
            input = Console.ReadLine().Trim();
            if (allowEmpty && string.IsNullOrEmpty(input)) return null;
        } while (input.Length == 0);

        return input.Length > maxLength ? input.Substring(0, maxLength) : input;
    }

    /// <summary>
    /// solicita el valor en double 
    /// </summary>
    /// <param name="allowEmpty"></param>
    /// <returns></returns>
    private double? ReadDouble(bool allowEmpty = false)
    {
        string input = Console.ReadLine().Trim();
        if (allowEmpty && string.IsNullOrEmpty(input)) return null;

        while (!double.TryParse(input, out double result))
        {
            ShowError("Ingrese un número válido");
            input = Console.ReadLine().Trim();
        }
        return double.Parse(input);
    }

    /// <summary>
    /// solicita el valor en int
    /// </summary>
    /// <param name="allowEmpty"></param>
    /// <returns></returns>
    private int? ReadInt(bool allowEmpty = false)
    {
        string input = Console.ReadLine().Trim();
        if (allowEmpty && string.IsNullOrEmpty(input)) return null;

        while (!int.TryParse(input, out int result))
        {
            ShowError("Ingrese un entero válido");
            input = Console.ReadLine().Trim();
        }
        return int.Parse(input);
    }

    /// <summary>
    /// solicita el valor short 
    /// </summary>
    /// <param name="allowEmpty"></param>
    /// <returns></returns>
    private short? ReadShort(bool allowEmpty = false)
    {
        string input = Console.ReadLine().Trim();
        if (allowEmpty && string.IsNullOrEmpty(input)) return null;

        while (!short.TryParse(input, out short result))
        {
            ShowError("Ingrese un short válido");
            input = Console.ReadLine().Trim();
        }
        return short.Parse(input);
    }

    /// <summary>
    /// muestra el mensaje en verde
    /// </summary>
    /// <param name="message"></param>
    private void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.ReadKey();
    }

    /// <summary>
    /// muestra el mensaje en rojo 
    /// </summary>
    /// <param name="message"></param>
    private void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.ReadKey();
    }

}
