using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Todo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new MainPage());
            window.Title = "Список задач";
            return window;
        }
    }
}
