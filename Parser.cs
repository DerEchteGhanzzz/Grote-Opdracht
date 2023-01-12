using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GroteOpdracht;
using Microsoft.VisualBasic.FileIO;

public class Parser
{
    public static int[,] ReadDistances(string path = @"InputFiles/AfstandenMatrix.csv")
    {
        // maak een matrix aan (2-dimensionale array) van lengte/breedte = alle bestemmingen
        int[,] timeMatrix = new int[1099, 1099];    // er zijn 1099 bestemmingen
        using (TextFieldParser csvParser = new TextFieldParser(path))
        {
            csvParser.CommentTokens = new string[] { "#" };
            csvParser.SetDelimiters(new string[] { ";" });
            csvParser.HasFieldsEnclosedInQuotes = false;
            
            // Skip first row
            csvParser.ReadFields();
            
            while (!csvParser.EndOfData)
            {
                // Read current line fields, pointer moves to the next line.
                string[] line = csvParser.ReadFields();
                int matrixX = Int32.Parse(line[0]);
                int matrixY = Int32.Parse(line[1]);
                int time = Int32.Parse(line[3]);

                timeMatrix[matrixX, matrixY] = time;
            }
        }
        return timeMatrix;
    }

    public static Order[] ReadOrders(string path = @"InputFiles/Orderbestand.csv")
    {
        // zorg ervoor dat de "." als seperator gezien zal worden        
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";


        Order[] orders = new Order[1177]; //Niet PG Hans
        
        using (TextFieldParser csvParser = new TextFieldParser(path))
        {
            csvParser.CommentTokens = new string[] { "#" };
            csvParser.SetDelimiters(new string[] { ";" });
            csvParser.HasFieldsEnclosedInQuotes = false;

            // Skip the row with the column names
            string[] fieldnames = csvParser.ReadFields();

            int newOrderNumber = 0;
            while (!csvParser.EndOfData)
            {
                // Read current line fields, pointer moves to the next line.
                string[] line = csvParser.ReadFields();
                // int frequency, float emptyTime, int containerVol, int containerCount

                int orderID = Int32.Parse(line[0]);
                int frequency = Int32.Parse(line[2].Substring(0, 1));
                int containerCount = Int32.Parse(line[3]);
                int containerVol = Int32.Parse(line[4]);
                float emptyTime = float.Parse(line[5],NumberStyles.Any,ci);
                int matrixID = Int32.Parse(line[6]);
                //Console.WriteLine(line[5] + " -> " + emptyTime);
                Order currentOrder = new Order(frequency, emptyTime*60, containerVol, containerCount, matrixID, orderID);
                
                orders[newOrderNumber] = currentOrder;
                newOrderNumber++;
            }

            return orders;
        }
    }
    
    public static void PrintSolution(Solution s)
    {
        using (StreamWriter writer = new StreamWriter(@"./Solution.txt"))  
        {
             writer.Write(s.ToString());
        }

        float best = GetBestScore(s);
        if (s.Score <= best && s.IsValid())
        {
            using (StreamWriter writer = new StreamWriter(@"./BestScore.txt"))  
            {
                writer.Write(s.Score);
            }
            using (StreamWriter writer = new StreamWriter(@"./SolutionBest.txt"))  
            {
                writer.Write(s.ToString());
            }
        }
        
        Console.WriteLine($"@ {DateTime.Now}");
        Console.WriteLine($"Solution Printed! Score: {s.Score} seconds ({s.Score/60} minutes)");
        Console.WriteLine($"Old best score: {best} seconds ({best/60} minutes). New score better than best? {s.Score < best}");
    }

    public static float GetBestScore(Solution s)
    {
        try
        {
            string text = File.ReadAllText(@"./BestScore.txt").Split("\n")[0];
            return float.Parse(text);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return s.Score;
        }
    }
}
