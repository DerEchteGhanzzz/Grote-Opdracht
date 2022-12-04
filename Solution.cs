namespace GroteOpdracht;

public class Solution
{
    public Day[] Days;
    public float Score;
    public Solution()
    {
        Score = 0;
        Days = new Day[5];
        for (int i = 0; i < 5; i++)
        {
            Days[i] = new Day((WorkDay)i);
        }
    }

    public void SwitchBetweenDays()
    {
        
    }
}