using System.Security.Cryptography;
using System;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using GroteOpdracht;
using System.Runtime.InteropServices;

namespace GroteOpdracht;

public class NeighbourCalculations
{
    public static bool Accept(Solution s, float temp, bool addmode)
    {
        Action action = GetAction(s, addmode);
        if (action is null)
        {
            return true;    // skip this action, let the count continue
        }
        if (action.IsPossible(s))
        {
            if (action.GetScoreDelta() < 0 || AcceptAnyways(action.GetScoreDelta(), temp))
            {
                action.Act(s);
                s.Score += action.GetScoreDelta();
                return true;
            }else
            {
                //Console.WriteLine("Action too expensive");
            }
        }
        else
        {
            //Console.WriteLine("Action not possible");
        }
        return false;
    }

    private static Action GetAction(Solution s, bool addmode)
    {
        // choose a random number between upper and lower.
        int max = addmode ? Program.NotVisitedAmount : Program.Orders.Length;
        
        int randomOrder = Program.random.Next(0, max);
        
        if (randomOrder < Program.NotVisitedAmount) // update to tweak numbers
        {
            switch (Program.Orders[randomOrder].Frequency)
            {
                case VisitAmount.Once:
                    return new AddSingle(s, randomOrder);
                case VisitAmount.Twice:
                    return new AddTwo(s, randomOrder);
                case VisitAmount.Thrice:
                    return new AddThree(s, randomOrder);
                case VisitAmount.Quadruple:
                    return new AddFour(s, randomOrder);
            }
        } else if (randomOrder >= Program.NotVisitedAmount)
        {
            return new RemoveAction(s, randomOrder);
            
        } if (false && !(randomOrder > 2))
        {
            /*int newNodeIndex = Program.random.Next(Program.NotVisitedAmount, Program.Orders.Length);
            return new ShiftAction(s, newNodeIndex);*/
        }

        return null;
    }

    private static bool AcceptAnyways(float scoreDelta, float temp)
    {
        //Console.WriteLine("Wow");
        double probability = Math.Exp(- scoreDelta / temp);
        double randomDouble = Program.random.NextDouble();
        //Console.WriteLine(probability + " :: " + randomDouble + " :: " + temp + " :: " + scoreDelta);
        if (probability <= randomDouble) return true;
        return false;
    }
}

