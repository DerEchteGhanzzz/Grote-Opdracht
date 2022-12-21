using System;

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
        public static readonly float TimePerDay = 11 * 60 * 60; // seconds

        public static Random random = new Random();

        private static readonly int maxCount = 1_000_000;
        static readonly int maxTempLowers = 10;
        static readonly int maxReheatings = 1;
        private static readonly int targetScore = 6000;
        private static readonly float small = (float) 0.001;

        static float a = (float) 0.99;
        private static float temp = 1;

        static void Main() // TODO PROGRAM CRAHSES RANDOMLY
        {
            Solution s = new Solution();
            SimulatedAnnealling(s);   // addmode is so a run is only done with adding nodes
            //SimulatedAnnealling(s);
            Parser.PrintSolution(s);
        }

        public static void SimulatedAnnealling(Solution s, bool addmode = false)
        {
            int count = 0;
            int lowerTemp = 0;
            int reheat = 0;
            
            Console.WriteLine($"Initial score: {s.Score}");

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            while (true)
            {
                if (NeighbourCalculations.Accept(s, temp, addmode))
                {
                    
                }
                count++;
                //Console.WriteLine("Accept");
                


                if (count >= maxCount)
                {
                    temp *= a;
                    count = 0;
                    watch.Stop();
                    if (lowerTemp == 0 && reheat == 0)
                    {
                        Console.WriteLine($"{maxCount} iterations in {watch.ElapsedMilliseconds} ms");
                        Console.WriteLine(
                            $"Expected time until done: {(float) watch.ElapsedMilliseconds / 1000 / 60 * maxTempLowers * maxReheatings} minutes. (at around {DateTime.Now + TimeSpan.FromMinutes((float) watch.ElapsedMilliseconds / 1000 / 60 * maxTempLowers * maxReheatings)})");
                    }

                    lowerTemp++;
                }

                if (lowerTemp >= maxTempLowers)
                {

                    //Console.WriteLine($"Reheating temp for the {reheat+1}. time");
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
        }
    }
}