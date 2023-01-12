using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace GroteOpdracht // Nic van der Stegen, Hans Blankensteijn en Santiago Nuñez Velasco
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static int[,] TimeMatrix = Parser.ReadDistances();

        public static Order[] Orders = Parser.ReadOrders();
        public static int NotVisitedAmount; // een pointer die weet hoeveel orders er nog niet in de oplossing zitten
        
        public static readonly int DepotID = 287;
        public static readonly int TruckVolume = 100_000;       
        public static readonly float TimePerDay = 12 * 60 * 60; // seconds

        public static Random random = new Random(); // de random number generator

        private static int maxCount;  // aantal iteraties voordat temp omlaag gaat

        static float a = (float) 0.99;
        public static float BeginTemp = 2_000;

        static void Main()
        {

            while (true) {  // we gaan gewoon oneindig door
                try
                {
                    NotVisitedAmount = Orders.Length;
                    Solution s = new Solution(); // we maken een oplossing aan
                    
                    maxCount = 10_000_000;
                    SimulatedAnnealling(s, true); // addmode staat aan, dus we gaan alleen dingen toevoegen
                    
                    maxCount = 250_000;
                    SimulatedAnnealling(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Orders = Parser.ReadOrders();
                }
            }
        }
        
        public static void SimulatedAnnealling(Solution s, bool addmode = false)
        {
            int count = 0;
            bool reheating = false;
            float temp = BeginTemp;
            
            float currentBest = 5900*60;
            
            Console.WriteLine($"Initial score: {s.Score} seconds ({s.Score / 60} minutes)");
            
            while (true)
            {
                NeighbourCalculations.Accept(s, temp, addmode, reheating);
                count++;
                
                if (count >= maxCount)
                {
                    reheating = false;
                    temp *= a;  // verlaag de temperatuur
                    count = 0;  // reset de count
                    
                    if (addmode)
                    {
                        s.ToString();
                        return;
                    }
                }

                if (s.Score < currentBest)
                {
                    if (s.IsValid())
                    {
                        Parser.PrintSolution(s);
                        currentBest = s.Score;
                    }
                }
                
                if (reheating || temp > 1)
                {
                    continue;
                }
                
                // going to reheat
                count = maxCount - 5; // oplossing opschudden
                reheating = true;
                temp = 100;
                
                Console.WriteLine($"Reheating. Current score = {s.Score / 60} minutes.");
            }
        }
    }
}