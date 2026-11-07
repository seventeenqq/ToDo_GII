using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Todo.Services;
using Todo.ViewModels;

namespace Todo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            string path = Path.Combine(FileSystem.AppDataDirectory, "tasks_nullable.db3");
            TodoDatabase database = new TodoDatabase(path);
            BindingContext = new TodoViewModel(database);
        }
    }
}
