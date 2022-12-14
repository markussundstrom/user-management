namespace User_management
{
    public class UserManager
    {
        private List<User> _users = new List<User>();
        public User? CurrentUser { get; private set; }
        private UserStorage _storage;

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

        }

        public void TryLogin(string username, string password)
        {
            User? requestedUser = _users.Where(u => u.Username == username).SingleOrDefault();
            CurrentUser = requestedUser.TryPassword(password) ? requestedUser : null;
        }

        public List<User> GetUserList()
        {
            return _users.ToList();
        }

        public void LogOut()
        {
            CurrentUser = null;
        }

        public bool AddUser(string username, string fullname, string email, int access, string password)
        {
            if (access == (int)User.Permissions.Moderator || access == (int)User.Permissions.Administrator)
            {
                if (CurrentUser == null || (int)CurrentUser.Access < access)
                {
                    return false;
                }
            }
            else
            {
                access = (int)User.Permissions.User;
            }
            if (_users.Any(u => u.Username == username))
            {
                return false;
            }
            User newUser = new User(username, fullname, (User.Permissions)access);
            if (!String.IsNullOrEmpty(email))
            {
                if (!newUser.SetEmail(email))
                {
                    return false;
                }
            }
            if (!newUser.SetPassword(password))
            {
                return false;
            }
            _users.Add(newUser);
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
    }
}


        