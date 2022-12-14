using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace User_management
{
    public class UserStorage
    {
        private string _file;

        public UserStorage()
        {
            _file = $"{Environment.CurrentDirectory}\\users.json";
        }

        public void GetUserData(List<User> userlist)
        {
            userlist.Clear();
            string jsondata = "";
            if (File.Exists(_file))
            {
                using (StreamReader r = new StreamReader(_file))
                {
                    jsondata = r.ReadToEnd();
                }
                userlist.AddRange(JsonSerializer.Deserialize<List<User>>(jsondata,
                                  new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, WriteIndented = true }));
            }
        }

        public void StoreUserData(List<User> userlist)
        {
            string jsondata = JsonSerializer.Serialize(userlist, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(_file, jsondata);
        }
    }
}


        