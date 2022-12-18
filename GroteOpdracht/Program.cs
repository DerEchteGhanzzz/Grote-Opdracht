namespace GroteOpdracht
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static int[,] TimeMatrix = Parser.ReadDistances();
        public static Order[] Orders = Parser.ReadOrders();
        public static int NotVisitedAmount = Orders.Length;
        public static readonly int DepotID = 287;
        public static readonly int TruckVolume = 100_000;
        public static readonly float TimePerDay = 12 * 60;

        public static Random random = new Random();

        static readonly int maxCount = 1000000;
        static readonly int maxTempLowers = 1000;
        static readonly int maxReheatings = 10;
        private static readonly int targetScore = 6000;
        private static readonly float small = (float)0.1;
        
        static float a = (float)0.99;
        private static float temp = 1;
        
        static void Main()
        {
            int count = 0;
            int lowerTemp = 0;
            int reheat = 0;

            Solution s = new Solution();
            
            while (true)
            {
                if (NeighbourCalculations.Accept(s, temp))
                {
                    count++;
                }
                
                if (count >= maxCount)
                {
                    temp *= a; count = 0;
                    lowerTemp++;
                }

                if (lowerTemp >= maxTempLowers)
                {
                    Console.WriteLine($"Lowering temp for the {reheat+1}. time");
                    if (s.Score <= targetScore) // we willen eerst onder de 6000
                    {
                        break;
                    }
                    temp += small;
                    lowerTemp = 0;
                    reheat++;
                }

                if (reheat >= maxReheatings)
                {
                    break;
                }
            }
            Parser.PrintSolution(s);
        }
    }
    
}