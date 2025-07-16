using Cysharp.Threading.Tasks;

public static class GoogleAuthHelper
{
    public static async UniTask<bool> SignInAsync()
    {
        // Simulate API call or Firebase auth here
        await UniTask.Delay(1500);

        // Replace with actual logic
        return true;
    }
}
