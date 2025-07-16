using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Assets.SimpleSignIn.Google.Scripts;
using Cysharp.Threading.Tasks;

namespace PandaBytes
{
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance;

        private FirebaseFirestore _firestore;
        private GoogleAuth _googleAuth;

        private string userId;
        private string userName;
        private string userEmail;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    _firestore = FirebaseFirestore.DefaultInstance;
                    _googleAuth = new GoogleAuth();

                    Debug.Log("✅ Firestore and GoogleAuth initialized");

                    // Try auto-login if previous session exists
                     _googleAuth.TryResume(OnGoogleSignIn, null);

                    //_googleAuth = new GoogleAuth(); // ✅ initialize
                    //TryAutoLoginAsync().Forget();
                }
                else
                {
                    Debug.LogError("❌ Firebase dependency error: " + task.Result);
                }
            });
        }

        // Manual sign-in (e.g., from button)
        //public void SignInWithGoogle()
        //{
        //    _googleAuth.SignIn(OnGoogleSignIn, caching: true);
        //}

        public async UniTask<bool> SignInWithGoogle()
        {
            var tcs = new UniTaskCompletionSource<bool>();

            _googleAuth.SignIn(
                (success, error, user) =>
                {
                    if (!success)
                    {
                        Debug.LogError("❌ Google Sign-In failed: " + error);
                        tcs.TrySetResult(false);
                        return;
                    }

                    userId = user.sub;
                    userName = user.name;
                    userEmail = user.email;

                    Debug.Log($"✅ Google User: {userName} ({userEmail})");

                    LoadOrCreateUserData(userId, userName, userEmail);
                    tcs.TrySetResult(true);
                },
                caching: true
            );

            return await tcs.Task;
        }

        // Sign out completely
        public void SignOut()
        {
            _googleAuth.SignOut(revokeAccessToken: true);
            userId = userName = userEmail = null;
            Debug.Log("👋 Signed out from Google");
        }

        // Handle Google sign-in result
        private void OnGoogleSignIn(bool success, string error, UserInfo userInfo)
        {
            if (!success)
            {
                Debug.LogError("❌ Google Sign-In failed: " + error);
                return;
            }

            userId = userInfo.sub;
            userName = userInfo.name;
            userEmail = userInfo.email;

            Debug.Log($"✅ Google User: {userName} ({userEmail})");

            LoadOrCreateUserData(userId, userName, userEmail);
        }

        // Load user from Firestore or create new record
        private void LoadOrCreateUserData(string id, string name, string email)
        {
            var docRef = _firestore.Collection("users").Document(id);

            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("❌ Firestore fetch failed: " + task.Exception?.Flatten().Message);
                    return;
                }

                var snap = task.Result;
                if (snap.Exists)
                {
                    Debug.Log("📥 User data loaded from Firestore");
                    var dict = snap.ToDictionary();
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
                    var data = JsonUtility.FromJson<PlayerProgressData>(json);

                    ApplyDataToPlayerState(data);
                }
                else
                {
                    Debug.Log("🆕 No Firestore record – creating new user");
                    var data = new PlayerProgressData
                    {
                        userId = id,
                        userName = name,
                        email = email,
                        currentLanguage = "python",
                        xp = 0,
                        coins = 0,
                        gems = 0,
                        streak = 0,
                        lastPlayedDate = DateTime.UtcNow.ToString("o"),
                        completedLessons = new List<string>()
                    };

                    SaveToFirestore(data);
                }
            });
        }

        private void ApplyDataToPlayerState(PlayerProgressData data)
        {
            PlayerState.CurrentLanguage = data.currentLanguage;
            PlayerState.AddXP(data.xp);
            PlayerState.AddCoins(data.coins);
            PlayerState.AddGems(data.gems);

            PlayerState.LastPlayedDate = DateTime.Parse(data.lastPlayedDate);
            PlayerState.StreakCount = data.streak;

            foreach (var lessonId in data.completedLessons)
                PlayerState.MarkLessonComplete(lessonId);

            Debug.Log("✅ PlayerState updated from Firebase");
        }

        public void SaveToFirestore(PlayerProgressData data)
        {
            if (_firestore == null || string.IsNullOrEmpty(userId)) return;

            var dict = new Dictionary<string, object>
    {
        { "userId", data.userId },
        { "userName", data.userName },
        { "email", data.email },
        { "currentLanguage", data.currentLanguage },
        { "xp", data.xp },
        { "coins", data.coins },
        { "gems", data.gems },
        { "streak", data.streak },
        { "lastPlayedDate", data.lastPlayedDate },
        { "completedLessons", data.completedLessons }
    };

            _firestore.Collection("users").Document(userId).SetAsync(dict).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("✅ Progress saved to Firestore");
                else
                    Debug.LogError("❌ Failed to save progress");
            });
        }

        public async UniTask<bool> TryAutoLoginAsync()
        {
            if (_googleAuth == null)
                _googleAuth = new GoogleAuth();

            Debug.Log("🔍 Checking cached user info...");

            if (_googleAuth.HasCachedUserInfo())
            {
                var userInfo = _googleAuth.SavedAuth.UserInfo;

                userId = userInfo.sub;
                userName = userInfo.name;
                userEmail = userInfo.email;

                Debug.Log($"✅ Auto login with cached user: {userName} ({userEmail})");

                LoadOrCreateUserData(userId, userName, userEmail);
                return true;
            }

            Debug.Log("❌ No cached session found");
            return false;
        }




        // Public accessors
        public bool IsSignedIn() => !string.IsNullOrEmpty(userId);
        public string GetUserId() => userId;
        public string GetUserName() => userName ?? "Guest";
        public string GetUserEmail() => userEmail ?? "No Email";
    }
}
