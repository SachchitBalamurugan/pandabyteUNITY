using System.Collections.Generic;

[System.Serializable]
public class PlayerProgressData
{
    public string userId;
    public string userName;
    public string email;

    public string currentLanguage;
    public int xp;
    public int coins;
    public int gems;
    public int streak;
    public string lastPlayedDate;

    public List<string> completedLessons = new();
}
