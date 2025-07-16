using UnityEngine;
using System;
using System.Collections.Generic;

public static class PlayerState
{
    // 🔤 Language the player is currently learning
    public static string CurrentLanguage { get; set; } = "python";

    // 📚 Currently selected lesson
    public static LessonDataSO CurrentLesson { get; set; }

    // 🎯 Player progress
    public static int XP { get; private set; } = 0;
    public static int Coins { get; private set; } = 0;
    public static int Gems { get; private set; } = 0;

    // 🔥 Daily streak tracking
    public static DateTime LastPlayedDate { get; set; }
    public static int StreakCount { get; set; }


    // 🧠 Track completed lessons by lessonId
    private static HashSet<string> completedLessons = new();

    public static bool IsLessonCompleted(string lessonId) => completedLessons.Contains(lessonId);

    public static void MarkLessonComplete(string lessonId)
    {
        if (!completedLessons.Contains(lessonId))
            completedLessons.Add(lessonId);
    }

    public static void AddXP(int amount)
    {
        XP += amount;
    }

    public static void AddCoins(int amount)
    {
        Coins += amount;
    }

    public static void AddGems(int amount)
    {
        Gems += amount;
    }

    public static void UpdateStreak()
    {
        var today = DateTime.Today;
        if (LastPlayedDate == today.AddDays(-1))
            StreakCount++;
        else if (LastPlayedDate != today)
            StreakCount = 1;

        LastPlayedDate = today;
    }

    public static void Reset()
    {
        XP = 0;
        Coins = 0;
        Gems = 0;
        StreakCount = 0;
        LastPlayedDate = DateTime.MinValue;
        completedLessons.Clear();
        CurrentLesson = null;
        CurrentLanguage = "python";
    }

    public static List<string> GetCompletedLessons()
    {
        return new List<string>(completedLessons);
    }

}
