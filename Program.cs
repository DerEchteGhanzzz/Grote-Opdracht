namespace GroteOpdracht
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static int[,] TimeMatrix = Parser.ReadDistances();
        public static List<Order>[] Orders = Parser.ReadOrders();
        public static int DepotID = 287;
        public static int TruckVolume = 100_000;
        public static float TimePerDay = 12 * 60;

        static readonly int maxCount = 1000;
        static readonly int maxTempLowers = 1000;
        static readonly int maxReheatings = 10;
        private static readonly int targetScore = 6000;
        private static readonly float small = (float)0.1;

        static float a = (float)0.99;
        private static float T = 1;
        
        static void Main()
        {
            int count = 0;
            int lowerTemp = 0;
            int reheat = 0;

            Solution s = new Solution();
            
            while (true)
            {
                Neighbour neigh = new Neighbour(s, T);
                if (neigh.Accepted)
                {
                    s = neigh.S;
                    count++;
                }

                if (count >= maxCount)
                {
                    T *= a; count = 0;
                    lowerTemp++;
                }

                if (lowerTemp >= maxTempLowers)
                {
                    if (s.Score <= targetScore) // we willen eerst onder de 6000
                    {
                        break;
                    }
                    T += small;
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