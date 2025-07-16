using Assets.SimpleSignIn.Telegram.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Assets.SimpleSignIn.Telegram.Scripts
{
    public class SavedAuth
    {
        public string BotId;
        public UserInfo UserInfo;

        [Preserve]
        private SavedAuth()
        {
        }

        public SavedAuth(string botId, UserInfo userInfo)
        {
            BotId = botId;
            UserInfo = userInfo;
        }

        public static SavedAuth GetInstance(string botId)
        {
            var key = GetKey(botId);

            if (!PlayerPrefs.HasKey(key)) return null;

            try
            {
                var encrypted = PlayerPrefs.GetString(key);
                var json = AES.Decrypt(encrypted, SystemInfo.deviceUniqueIdentifier);

                return JsonConvert.DeserializeObject<SavedAuth>(json);
            }
            catch
            {
                return null;
            }
        }

        public void Save()
        {
            var key = GetKey(BotId);
            var json = JsonConvert.SerializeObject(this);
            var encrypted = AES.Encrypt(json, SystemInfo.deviceUniqueIdentifier);

            PlayerPrefs.SetString(key, encrypted);
            PlayerPrefs.Save();
        }

        public void Delete()
        {
            var key = GetKey(BotId);

            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        private static string GetKey(string clientId)
        {
            return Md5.ComputeHash(nameof(SavedAuth) + ':' + clientId);
        }
    }
}