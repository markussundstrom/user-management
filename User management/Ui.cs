namespace User_management
{
    public class Ui
    {
        public string ErrorMessage = "";
        public string InfoMessage = "";
        private UserManager _usermanager;
        private bool _running = true;
        private List<User> _userList = new List<User>();

        public Ui(UserManager usermanager)
        {
            this._usermanager = usermanager;
        }

        public void Run()
        {
            while (_running)
            {
                if (_usermanager.CurrentUser == null)
                {
                    ShowLoginMenu();
                }
                else
                {
                    ShowUserSystemMenu();
                }
            }
        }


        public void ShowLoginMenu()
        {
            Console.Clear();
            ShowError();
            ShowInfo();
            Console.WriteLine("l: Login\nc: Create user\nq: Quit\n");
            LoginMenuOptions();
        }

        public void ShowUserSystemMenu()
        {
            Console.Clear();
            ShowError();
            ShowInfo();
            Console.WriteLine($"Userlist (Logged in as {_usermanager.CurrentUser.Username})");
            _userList.Clear();
            _userList = _usermanager.GetUserList();

            for (int i = 0; i < _userList.Count; i++)
            {
                Console.WriteLine($"{i} || {_userList[i].Username} || {_userList[i].FullName} || {_userList[i].Email} || {_userList[i].Access}");
            }
            Console.WriteLine("e: edit profile | l: Log out");
            if (_usermanager.CurrentUser.Access >= User.Permissions.Moderator)
            {
                Console.WriteLine("E: edit user");
            }
            if (_usermanager.CurrentUser.Access >= User.Permissions.Administrator)
            {
                Console.WriteLine("c: create user");
            }
            UserSystemMenuOptions();
        }

        public void LoginMenuOptions()
        {
            switch (Console.ReadKey(true).KeyChar)
            {
                case 'l':
                    InputRequest("Enter username");
                    string usernameInput = Console.ReadLine();
                    InputRequest("Enter password");
                    string passwordInput = HiddenEntry();
                    _usermanager.TryLogin(usernameInput, passwordInput);
                    if (_usermanager.CurrentUser == null)
                    {
                        ErrorMessage = "Unable to login";
                    }
                    break;

                case 'c':
                    CreateUser();
                    break;
                    
                case 'q':
                    InputRequest("Do you want to quit? (y/n)");
                    if (GetConfirmation())
                    {
                        _running = false;
                    }
                    break;
            }
        }

        public void UserSystemMenuOptions()
        {
            char choice = Console.ReadKey(true).KeyChar;

            switch (choice)
            {
                case 'e':
                    EditProfile(_usermanager.CurrentUser);
                    break;
                case 'l':
                    _usermanager.LogOut();
                    return;

                case 'E':
                    if (_usermanager.CurrentUser.Access >= User.Permissions.Moderator)
                    {
                        InputRequest("Enter id of row to edit");
                        int id = Int32.Parse(Console.ReadLine());
                        if (0 > id || id > _userList.Count)
                        {
                            ErrorMessage = "Invalid id to edit";
                        }
                        else
                        {
                            EditUser(_userList[id]);
                        }
                    }
                    break;

                case 'c':
                    if (_usermanager.CurrentUser.Access >= User.Permissions.Administrator)
                    {
                        CreateUser();
                    }
                    break;
            }
        }

        public void EditProfile(User user)
        {
            bool editProfile = true;

            while (editProfile)
            {
                Console.Clear();
                ShowInfo();
                ShowError();
                Console.WriteLine($"Edit profile for user {user.Username}:");
                Console.WriteLine($"Full name: {user.FullName} (press f to edit)");
                Console.WriteLine($"E-mail: {user.Email} (press e to edit)");
                Console.WriteLine("Press p to change password");
                Console.WriteLine("Press b to go back");

                switch (Console.ReadKey(true).KeyChar)
                {
                    case 'f':
                        InputRequest("Enter new Full name:");
                        user.FullName = Console.ReadLine();
                        InfoMessage = "Full name changed for account.";
                        break;

                    case 'e':
                        InputRequest("Enter new e-mail");
                        if (user.SetEmail(Console.ReadLine()))
                        {
                            InfoMessage = "Email changed";
                        }
                        else
                        {
                            ErrorMessage = "Invalid email format";
                        }
                        break;

                    case 'p':
                        InputRequest("Enter new password");
                        string pw1 = HiddenEntry();
                        InputRequest("Enter password again");
                        string pw2 = HiddenEntry();
                        if (pw1 != pw2)
                        {
                            ErrorMessage = "Supplied password entries do not match, password not changed";
                        }
                        else
                        {
                            if (user.SetPassword(pw1))
                            {
                                InfoMessage = "Password changed!";
                            }
                            else
                            {
                                ErrorMessage = "Password does not meet security requirements";
                            }
                        }
                        break;

                    case 'b':
                        editProfile = false;
                        break;
                }
            }
        }

        public void EditUser(User user)
        {
            bool editUser = true;

            while (editUser)
            {
                Console.Clear();
                ShowError();
                ShowInfo();
                Console.WriteLine($"Username: {user.Username}");
                Console.WriteLine($"Full name: {user.FullName}");
                Console.WriteLine($"E-mail: {user.Email}");
                Console.WriteLine($"Access: {user.Access.ToString()}");
                if (_usermanager.CurrentUser.Access > user.Access)
                {
                    Console.WriteLine("Press p to promote user");
                }
                if (user.Access > User.Permissions.User && _usermanager.CurrentUser.Access >= user.Access)
                {
                    Console.WriteLine("Press d to demote user");
                }
                if (_usermanager.CurrentUser.Access == User.Permissions.Administrator)
                {
                    Console.WriteLine("Press x to delete user");
                }
                Console.WriteLine("Press b to go back");

                switch (Console.ReadKey(true).KeyChar)
                {
                    case 'p':
                        _usermanager.PromoteUser(user);
                        break;

                    case 'd':
                        _usermanager.DemoteUser(user);
                        break;

                    case 'x':
                        InputRequest("Really delete user?");
                        if (GetConfirmation())
                        {
                            if (_usermanager.DeleteUser(user))
                            {
                                InfoMessage = "User Deleted";
                                editUser = false;
                            }
                            else
                            {
                                ErrorMessage = "Unable to delete user";
                            }
                        }
                        else
                        {
                            InfoMessage = "User deletion cancelled";
                        }
                        break;

                    case 'b':
                        editUser = false;
                        break;
                }
            }
        }

        public void CreateUser()
        {
            Console.Clear();

            InputRequest("Enter username for new user");
            string username = Console.ReadLine();

            InputRequest("Enter full name for new user");
            string fullname = Console.ReadLine();

            InputRequest("Enter e-mail for new user (optional)");
            string email = Console.ReadLine();

            int access = (int)User.Permissions.User;
            if (_usermanager.CurrentUser != null)
            {
                InputRequest("Select access level for new user");
                foreach (User.Permissions p in Enum.GetValues(typeof(User.Permissions)))
                {
                    Console.WriteLine($"{(int)p}: {p}");
                }
                access = int.TryParse(Console.ReadLine(), out access) ? access : (int)User.Permissions.User;
            }

            string pw1;
            string pw2;
            do
            {
                InputRequest("Enter password for new user");
                pw1 = HiddenEntry();
                InputRequest("Enter password again");
                pw2 = HiddenEntry();
            } while (pw1 != pw2);

            if (_usermanager.AddUser(username, fullname, email, access, pw1))
            {
                InfoMessage = "User created";
            }
            else
            {
                ErrorMessage = "Unable to create user";
            }
        }




        public string HiddenEntry()
        {
            string hiddenEntry = "";
            ConsoleKeyInfo ch = Console.ReadKey(true);
            while (ch.Key != ConsoleKey.Enter)
            {
                hiddenEntry += ch.KeyChar;
                Console.Write('*');
                ch = Console.ReadKey(true);
            }
            Console.Write("\n");
            return hiddenEntry;
        }


        public bool GetConfirmation()
        {
            while (true)
            {
                switch (Console.ReadKey(true).KeyChar)
                {
                    case 'y':
                        return true;
                    case 'n':
                        return false;
                }
            }
        }

        public void ShowError()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine(ErrorMessage);
                ErrorMessage = "";
                Console.ResetColor();
            }
        }

        public void ShowInfo()
        {
            if (!String.IsNullOrEmpty(InfoMessage))
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(InfoMessage);
                InfoMessage = "";
                Console.ResetColor();
            }
        }

        public void InputRequest(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
