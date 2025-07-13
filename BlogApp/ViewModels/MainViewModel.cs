namespace BlogApp.ViewModels
{
    public class MainViewModel
    {
        public LoginViewModel LoginViewModel { get; set; }
        public RegisterViewModel RegisterViewModel { get; set; }

        public MainViewModel()
        {
            LoginViewModel = new LoginViewModel();
            RegisterViewModel = new RegisterViewModel();
        }
    }
}
