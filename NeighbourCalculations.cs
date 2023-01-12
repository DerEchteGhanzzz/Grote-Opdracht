using System.Security.Cryptography;
using System;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using GroteOpdracht;
using System.Runtime.InteropServices;

namespace GroteOpdracht;

public class NeighbourCalculations
{
    public static bool Accept(Solution s, float temp, bool addmode, bool reheating)
    {
        Action action = GetAction(s, addmode);
        // kijk of de actie mogelijk is
        if (action.IsPossible(s))
        {
            // kijk of hij niet te duur is (of pas de formule toe)
            if (CalculateAcceptance(action.GetScoreDelta(), action.GetOvertimeReduction(s), temp, reheating))
            {
                // doe de actie!
                action.Act(s);
                // update score
                s.Score += action.GetScoreDelta();

                //Console.WriteLine(s.Overtime + " :: " + action.GetOvertimeReduction(s));
                return true;
            }
        }
        return false;
    }

    private static Action GetAction(Solution s, bool addmode)
    {
        // in addmode wordt er alleen maar toegevoegd, anders kunnen alle orders gekozen worden
        int max = addmode ? Program.NotVisitedAmount : Program.Orders.Length;
        //max = Program.Validating ? Program.NotVisitedAmount : max;
        
        int randomOrder = Program.random.Next(0, max);
        if (randomOrder < Program.NotVisitedAmount) // update to tweak numbers
        {
            switch (Program.Orders[randomOrder].Frequency)
            {
                // kijk of de order die moet worden toegevoegd 1 of meer keer moeten worden ingepland
                case VisitAmount.Once:
                    return new AddSingle(s, randomOrder);
                case VisitAmount.Twice:
                    return new AddTwo(s, randomOrder);
                case VisitAmount.Thrice:
                    return new AddThree(s, randomOrder);
                case VisitAmount.Quadruple:
                    return new AddFour(s, randomOrder);
            }
        }
        int removeShiftChance = Program.random.Next(0, 1000);
        if (removeShiftChance < 20)
            return new RemoveAction(s, randomOrder);
        if (removeShiftChance < 150)
        //     return new ShiftDayAction();
            return new ShiftTripAction();
        return new SwapNeighbourNodes();
    }
    
    private static bool CalculateAcceptance(float scoreDelta, float overtimeDelta, float temp, bool reheating)
    {
        if (scoreDelta < 0 && overtimeDelta < 0 || reheating) return true;
        
        double scoreProbability = Math.Exp(- (scoreDelta + Math.Pow(overtimeDelta, Program.BeginTemp-temp)) / temp);

        double randomDouble = Program.random.NextDouble();
        
        // kijk of de random double kleiner is dan de probability.
        if (scoreProbability >= randomDouble)
        {
            return true;
        }
        
        return false;
    }
}
