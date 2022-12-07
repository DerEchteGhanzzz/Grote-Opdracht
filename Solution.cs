namespace GroteOpdracht;

public class Solution
{
    public Day[] Days;
    public float Score;
    public Solution()
    {
        Score = InitialScore();
        Days = new Day[5];
        for (int i = 0; i < 5; i++)
        {
            Days[i] = new Day((WorkDay)i);
        }
    }

    public override string ToString()
    {
        string solutionString = "";
        string dlm = "; ";

        foreach (var day in Days)   // loop through days
        {
            for (int truck = 0; truck < 2; truck++) // loop through trucks
            {
                int adressCount = 1;    // how many adresses does this truck do today?
                for (int tripIndex = 0; tripIndex < day.TripCount[truck]; tripIndex++)  // loop through all trips that this truck does today
                {
                    Node currentNode = day.Schedules[truck, tripIndex].Depot;   // start at the depot
                    while (currentNode.Next is not null)    // check if we can go on
                    {
                        // constantly add the next node in line (as to not add the depot in the first run)
                        currentNode = currentNode.Next;
                        solutionString += $"{truck.ToString()}; {day.ToString()}; {adressCount.ToString()}; {currentNode.Order.ToString()}\n";
                    }
                    // add the depot at the end of the trip
                    solutionString += $"{truck.ToString()}{dlm}{day.ToString()}{dlm}{adressCount.ToString()}{dlm}{day.Schedules[truck, tripIndex].Depot.Order.ToString()}\n";
                }
            }
        }
        
        return solutionString;
    }

    private static float InitialScore()
    {
        float score = 0;
        foreach (var order in Program.Orders)
        {
            score += order.PenaltyPerVisit * (float)order.Frequency;
            
        }
        return score;
    }
}