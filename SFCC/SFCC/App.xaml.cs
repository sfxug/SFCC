using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SFCC.Views;
using SFCC.Common.Managers;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SFCC
{
    public partial class App : Application
    {
        public static NavigationPage Navigation { get; internal set; }

        public App()
        {
            InitializeComponent();

            DependencyService.Register<TodoItemDataStore>();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
