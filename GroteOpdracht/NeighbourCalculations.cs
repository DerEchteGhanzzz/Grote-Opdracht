using System.Security.Cryptography;
using System;
using System.Net;

namespace GroteOpdracht;

public class NeighbourCalculations
{
    public static bool Accept(Solution s, float temp)
    {
        Action action = GetAction(s);
        if (action is null) //TODO this is for testing purposes only
        {
            return true;    // skip this action, let the count continue
        }
        
        if (action.IsPossible(s))
        {
            if (action.GetScoreDelta() < 0 || AcceptAnyways(action.GetScoreDelta(), temp))
            {
                action.Act(s);
                return true;
            }
        }
        return false;
    }

    private static Action GetAction(Solution s)
    {
        // there needs to be at least 1 order not added yet to add an order
        int lower = Program.NotVisitedAmount == 0 ? 1 : 0;

        // there need to be at least 2 orders added to be able to shift orders.
        // If there are only 2 nodes added, removing one is also a bit of a waste of time.
        int upper = Program.NotVisitedAmount >= Program.Orders.Length - 1 ? 1 : 2;
        
        // choose a random number between upper and lower.
        int randomAction = Program.random.Next(lower, upper);
        
        if (!(randomAction > 0)) // update to tweak numbers
        {
            int newNodeIndex = Program.random.Next(0, Program.NotVisitedAmount);
            
            switch (Program.Orders[newNodeIndex].Frequency)
            {
                case VisitAmount.Once:
                    return new AddSingle(s, newNodeIndex);
                case VisitAmount.Twice:
                    return new AddTwo(s, newNodeIndex);
                case VisitAmount.Thrice:
                    return new AddThree(s, newNodeIndex);
                case VisitAmount.Quadruple:
                    return new AddFour(s, newNodeIndex);
            }
        } if (!(randomAction > 1))
        {
            int newNodeIndex = Program.random.Next(Program.NotVisitedAmount, Program.Orders.Length);
            return new RemoveAction(s, newNodeIndex);
            
        } if (false && !(randomAction > 2))
        {
            /*int newNodeIndex = Program.random.Next(Program.NotVisitedAmount, Program.Orders.Length);
            return new ShiftAction(s, newNodeIndex);*/
        }

        return null;
    }

    private static bool AcceptAnyways(float timeDelta, float temp)
    {
        double probability = Math.Exp(-1 * timeDelta / temp);
        
        if (probability <= Program.random.NextDouble()) return true;
        return false;
    }
}

