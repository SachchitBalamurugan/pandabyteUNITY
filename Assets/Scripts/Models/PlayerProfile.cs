using System.Collections.Generic;

[System.Serializable]
public class PlayerProfile
{
    public string userId;
    public string displayName;
    public int level;
    public int xp;
    public int coins;
    public int gems;
    public int dailyStreak;
    public string selectedCharacter;
    public List<string> ownedCharacters;
    public long lastLoginUnix;
}