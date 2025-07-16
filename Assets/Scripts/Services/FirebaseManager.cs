using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using System;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    public FirebaseUser CurrentUser { get; private set; }
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    public event Action<PlayerProfile> OnProfileLoaded;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); InitFirebase(); }
        else Destroy(gameObject);
    }

    private void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SignInAnonymously(Action onSuccess)
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                CurrentUser = task.Result.User;
                Debug.Log("Signed in anonymously: " + CurrentUser.UserId);
                onSuccess?.Invoke();
            }
            else Debug.LogError("Sign-in failed: " + task.Exception);
        });
    }

    public void LoadOrCreateProfile()
    {
        DocumentReference docRef = db.Collection("users").Document(CurrentUser.UserId);

        docRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    var dict = snapshot.ToDictionary();
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
                    PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(json);
                    OnProfileLoaded?.Invoke(profile);
                }
                else
                {
                    PlayerProfile newProfile = new PlayerProfile
                    {
                        userId = CurrentUser.UserId,
                        displayName = "Panda " + UnityEngine.Random.Range(1000, 9999),
                        level = 1,
                        xp = 0,
                        coins = 200,
                        gems = 5,
                        dailyStreak = 1,
                        selectedCharacter = "default",
                        ownedCharacters = new List<string> { "default" },
                        lastLoginUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };
                    SaveProfile(newProfile);
                    OnProfileLoaded?.Invoke(newProfile);
                }
            }
        });
    }

    public void SaveProfile(PlayerProfile profile)
    {
        DocumentReference docRef = db.Collection("users").Document(profile.userId);
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "userId", profile.userId },
            { "displayName", profile.displayName },
            { "level", profile.level },
            { "xp", profile.xp },
            { "coins", profile.coins },
            { "gems", profile.gems },
            { "dailyStreak", profile.dailyStreak },
            { "selectedCharacter", profile.selectedCharacter },
            { "ownedCharacters", profile.ownedCharacters },
            { "lastLoginUnix", profile.lastLoginUnix },
        };
        docRef.SetAsync(dict);
    }
}
