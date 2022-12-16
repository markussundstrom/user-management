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
                    
                    string usernameInput = InputRequest("Enter username");
                    string passwordInput = HiddenInputRequest("Enter password");
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
                    if (GetConfirmation("Do you want to quit? (y/n)"))
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
                        if (int.TryParse(InputRequest("Enter id of row to edit"), out int id))
                        {
                            if (0 > id || id > _userList.Count)
                            {
                                ErrorMessage = "Invalid id to edit";
                            }
                            else
                            {
                                EditUser(_userList[id]);
                            }
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
                        user.FullName = InputRequest("Enter new Full name:");
                        InfoMessage = "Full name changed for account.";
                        break;

                    case 'e':
                        if (user.SetEmail(InputRequest("Enter new e-mail")))
                        {
                            InfoMessage = "Email changed";
                        }
                        else
                        {
                            ErrorMessage = "Invalid email format";
                        }
                        break;

                    case 'p':
                        string pw1 = HiddenInputRequest("Enter new password");
                        string pw2 = HiddenInputRequest("Enter password again");
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
                        if (GetConfirmation("Really delete user? (y/n)"))
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

            string username = InputRequest("Enter username for new user");
            
            string fullname = InputRequest("Enter full name for new user");
            
            string email = InputRequest("Enter e-mail for new user (optional)");

            int access = (int)User.Permissions.User;
            if (_usermanager.CurrentUser != null)
            {
                foreach (User.Permissions p in Enum.GetValues(typeof(User.Permissions)))
                {
                    Console.WriteLine($"{(int)p}: {p}");
                }
                access = int.TryParse(InputRequest("Select access level for new user (default: User)"), out access) ? access : (int)User.Permissions.User;
            }

            string pw1;
            string pw2;
            do
            {
                ShowError();
                pw1 = HiddenInputRequest("Enter password for new user");
                pw2 = HiddenInputRequest("Enter password again");
                if (pw1 != pw2)
                {
                    ErrorMessage = "Passwords do not match";
                }
            } while (pw1 != pw2);

            if (_usermanager.AddUser(username, fullname, email, access, pw1, out string createUserError))
            {
                InfoMessage = "User created";
            }
            else
            {
                ErrorMessage = createUserError;
            }
        }




        public string HiddenInputRequest(string message)
        {
            ShowPrompt(message);
            string hiddenEntry = "";
            //This is not optimal, function keys etc. are accepted as input.
            ConsoleKeyInfo ch = Console.ReadKey(true);
            while (ch.Key != ConsoleKey.Enter)
            {
                if (ch.Key != ConsoleKey.Backspace)
                {
                    hiddenEntry += ch.KeyChar;
                    Console.Write('*');
                }
                else if (ch.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(hiddenEntry))
                    {
                        hiddenEntry = hiddenEntry.Substring(0, hiddenEntry.Length - 1);
                        int pos = Console.CursorLeft;
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                ch = Console.ReadKey(true);
            }
            Console.Write("\n");
            return hiddenEntry;
        }


        public bool GetConfirmation(string message)
        {
            ShowPrompt(message);
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

        public void ShowPrompt(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
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

        public string InputRequest(string message)
        {
            ShowPrompt(message);
            return Console.ReadLine();
        }
    }
}
