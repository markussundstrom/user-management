using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace User_management
{
    public class User
    {
        public enum Permissions
        {
            User = 1,
            Moderator = 2,
            Administrator = 4
        };

        public string Username { get; private set; }
        public string FullName { get; set; }
        public string Email { get; private set; }
        public Permissions Access { get; private set; }
        public string Password { get; private set; } = String.Empty;
        public string Salt { get; private set; } = String.Empty;

        public event EventHandler UserDataChanged;

        [JsonConstructor]
        public User(string username, string fullname, string email, User.Permissions access, string password, string salt)
        {
            Username = username;
            FullName = fullname;
            Email = email;
            Access = access;
            Password = password;
            Salt = salt;
        }

        public User(string username, string fullname, User.Permissions access) : this(username, fullname, "", access, "", "")
        {
        }

        public bool TryPassword(string password)
        {
            password += Salt;
            byte[] pwBytes = Encoding.UTF8.GetBytes(password);
            HashAlgorithm algo = HashAlgorithm.Create("SHA512");
            byte[] hash = algo.ComputeHash(pwBytes);
            return (Password == Convert.ToBase64String(hash));
        }

        public bool SetPassword(string newPassword)
        {
            if (!Regex.IsMatch(newPassword, @"^(?=\P{Ll}*\p{Ll})(?=\P{Lu}*\p{Lu})(?=\P{N}*\p{N})(?=[\p{L}\p{N}]*[^\p{L}\p{N}])[\s\S]{8,}$"))
            {
                return false;
            }
            string salt = Guid.NewGuid().ToString("n").Substring(0, 16);
            newPassword += salt;
            byte[] pwBytes = Encoding.UTF8.GetBytes(newPassword);
            HashAlgorithm algo = HashAlgorithm.Create("SHA512");
            byte[] hash = algo.ComputeHash(pwBytes);
            Password = Convert.ToBase64String(hash);
            Salt = salt;
            OnUserDataChanged();
            return true;
        }

        public bool SetEmail(string newEmail)
        {
            if (Regex.IsMatch(newEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                Email = newEmail;
                OnUserDataChanged();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Promote()
        {
            if (Access < Permissions.Administrator)
            {
                switch (Access)
                {
                    case Permissions.User:
                        Access = Permissions.Moderator;
                        break;
                    case Permissions.Moderator:
                        Access = Permissions.Administrator;
                        break;
                }
                OnUserDataChanged();
            }
        }

        public void Demote()
        {
            if (Access > Permissions.User)
            {
                switch (Access)
                {
                    case Permissions.Administrator:
                        Access = Permissions.Moderator;
                        break;
                    case Permissions.Moderator:
                        Access = Permissions.User;
                        break;
                }
                OnUserDataChanged();
            }
        }
        protected virtual void OnUserDataChanged()
        {
            EventHandler e = UserDataChanged;
            if (e != null)
            {
                e(this, EventArgs.Empty);
            }
        }
    }
}