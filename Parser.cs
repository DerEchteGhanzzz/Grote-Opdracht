using GroteOpdracht;
using Microsoft.VisualBasic.FileIO;

public class Parser
{
    public static int[,] ReadDistances(string path = @"InputFiles/AfstandenMatrix.csv")
    {
        // maak een matrix aan (2-dimensionale array) van lengte/breedte = alle bestemmingen
        int[,] timeMatrix = new int[1099, 1099];
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

        Order[] orders = new Order[1099]; //Niet PG Hans
        
        using (TextFieldParser csvParser = new TextFieldParser(path))
        {
            csvParser.CommentTokens = new string[] { "#" };
            csvParser.SetDelimiters(new string[] { ";" });
            csvParser.HasFieldsEnclosedInQuotes = false;

            // Skip the row with the column names
            string[] fieldnames = csvParser.ReadFields();
            
            while (!csvParser.EndOfData)
            {
                // Read current line fields, pointer moves to the next line.
                string[] line = csvParser.ReadFields();
                // int frequency, float emptyTime, int containerVol, int containerCount

                int orderID = Int32.Parse(line[0]);
                int frequency = Int32.Parse(line[2].Substring(0, 1));
                int containerCount = Int32.Parse(line[3]);
                int containerVol = Int32.Parse(line[4]);
                float emptyTime = float.Parse(line[5]);
                int matrixID = Int32.Parse(line[6]);

                Order currentOrder = new Order(frequency, emptyTime, containerVol, containerCount, matrixID, orderID);

                orders[matrixID] = currentOrder ;
            }

            return orders;
        }
    }

    public static void PrintOrders(List<Order>[] orders, string delim = " ")
    {
        foreach (List<Order> orderList in orders)
        {
            PrintOrderList(orderList);
        }
        Console.Write("\n");
    }

    public static void PrintOrderList(List<Order> orderList)
    {
        foreach (Order order in orderList)
        {
            Console.Write(order.ToString());
        }
    }

    public static void PrintDistanceMatrix(List<List<int>> matrix, string delim = " ")
    {
        foreach (var item in matrix)
        {
            // implementeer
        }
        Console.Write("\n");
    }

    public static void PrintSolution(Solution s)
    {
        using (StreamWriter writer = new StreamWriter(@"C:/Users/Hans/RiderProjects/GroteOpdracht/GroteOpdracht/bin/Debug/net6.0/OutputFiles/Solution.txt"))  
        {
             writer.WriteLine(s.ToString());
        }
        Console.WriteLine($"Solution Printed! Score: {s.Score}");
    }
}