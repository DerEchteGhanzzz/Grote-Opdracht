using GroteOpdrachtUtils;

namespace GroteOpdracht
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static int[,] timeMatrix = Parser.ReadDistances();
        public static List<Order>[] orders = Parser.ReadOrders();
        static void Main()
        {
            
        }
    }
}