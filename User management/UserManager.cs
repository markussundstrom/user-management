namespace User_management
{
    public class UserManager
    {
        private List<User> _users = new List<User>();
        public User? CurrentUser { get; private set; }
        private UserStorage _storage;

        public event EventHandler UserDataChanged;

        public UserManager()
        {
            _storage = new UserStorage();
            _storage.GetUserData(_users);
            if (_users.Count == 0)
            {
                User adminUser = new User("admin", "Administrator", User.Permissions.Administrator);
                adminUser.SetPassword("Admin123!");
                _users.Add(adminUser);
            }
            foreach (User u in _users)
            {
                u.UserDataChanged += HandleUserDataChanged;
            }
            UserDataChanged += HandleUserDataChanged;
        }

        public void TryLogin(string username, string password)
        {
            User? requestedUser = _users.Where(u => u.Username == username).SingleOrDefault();
            if (requestedUser != null)
            {
                CurrentUser = requestedUser.TryPassword(password) ? requestedUser : null;
            }
        }

        public List<User> GetUserList()
        {
            return _users.ToList();
        }

        public void LogOut()
        {
            CurrentUser = null;
        }

        public bool AddUser(string username, string fullname, string email, int access, string password, out string error)
        {
            if (access == (int)User.Permissions.Moderator || access == (int)User.Permissions.Administrator)
            {
                if (CurrentUser == null || (int)CurrentUser.Access < access)
                {
                    error = "Unable to set requested permission level";
                    return false;
                }
            }
            else
            {
                access = (int)User.Permissions.User;
            }
            if (_users.Any(u => u.Username == username))
            {
                error = "User already exists";
                return false;
            }
            User newUser = new User(username, fullname, (User.Permissions)access);
            if (!String.IsNullOrEmpty(email))
            {
                if (!newUser.SetEmail(email))
                {
                    error = "Invalid e-mail address";
                    return false;
                }
            }
            if (!newUser.SetPassword(password))
            {
                error = "Password does not meet security requirements";
                return false;
            }
            _users.Add(newUser);
            newUser.UserDataChanged += HandleUserDataChanged;
            OnUserDataChanged();
            error = "";
            return true;
        }

        public void PromoteUser(User user)
        {
            if (CurrentUser.Access > user.Access)
            {
                user.Promote();
            }
        }

        public void DemoteUser(User user)
        {
            if (user.Access > User.Permissions.User && CurrentUser.Access >= user.Access)
            {
                user.Demote();
            }
        }

        public bool DeleteUser(User user)
        {
            if (CurrentUser.Access == User.Permissions.Administrator)
            {
                _users.Remove(user);
                OnUserDataChanged();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ShutdownUserManager()
        {
            _storage.StoreUserData(_users);
        }

        private void HandleUserDataChanged(Object sender, EventArgs e)
        {
            _storage.StoreUserData(_users);
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


        