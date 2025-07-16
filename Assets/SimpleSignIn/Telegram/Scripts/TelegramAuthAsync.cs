using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.SimpleSignIn.Telegram.Scripts
{
    public partial class TelegramAuth
    {
        /// <summary>
        /// Returns an access token async.
        /// </summary>
        public async Task<UserInfo> SignInAsync()
        {
            var completed = false;
            string error = null;
            UserInfo userInfo = null;

            SignIn((success, e, result) =>
            {
                if (success)
                {
                    userInfo = result;
                }
                else
                {
                    error = e;
                }

                completed = true;
            }, caching: true);

            while (!completed)
            {
                await Task.Yield();
            }

            if (userInfo == null) throw new Exception(error);

            Log($"userInfo={JsonConvert.SerializeObject(userInfo)}");

            return userInfo;
        }
    }
}