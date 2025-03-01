using ArchivoComoBaseDeDatosOptimizado.Interfaces;
using ArchivoComoBaseDeDatosOptimizado.Logic;
using System.Text;

namespace AulaManager;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        IManejadorDeArchivos manejador = new ManejadorDeArchivos();

        IMenu menu = new Menu(manejador);

        while (true)
        {
            Console.Clear();
            menu.ShowMenu();

            var option = Console.ReadLine();

            menu.Opcion(option);
        }
    }
}