namespace User_management
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UserManager usermanager = new UserManager();
            Ui ui = new Ui(usermanager);
            ui.Run();
            usermanager.ShutdownUserManager();
        }
    }
}