using System;

using Xamarin.Forms;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using System.Diagnostics;
using Xamarin.Auth;

namespace MotivationAdmin
{
    public interface IAuthenticate
    {
        Task<bool> Authenticate();
    }

    public class MotiveApp : Application
    {
        static NavigationPage _navPage;
        static string _Token;
        private MobileServiceUser user;
        private string aToken = "";
        public static IAuthenticate Authenticator { get; private set; }

        public static void Init(IAuthenticate authenticator)
        {
            Authenticator = authenticator;

        }

        public MotiveApp(Account _account)
        {
            //Debug.WriteLine("account ->"+_account.Properties["access_token"]);
            if (_account != null)
                aToken = _account.Properties["access_token"];
            // The root page of your application
            _navPage = new NavigationPage(new GroupList(aToken));
            MainPage = _navPage;
        }
        public static bool IsLoggedIn
        {
            get { return !string.IsNullOrWhiteSpace(_Token); }
        }

        public static string Token
        {
            get { return _Token; }
        }

        public static void SaveToken(string token)
        {
            Debug.WriteLine("setting " + token);
            _Token = token;
            Debug.WriteLine("setting _Token" + _Token);
        }

        public static Action SuccessfulLoginAction
        {
            get
            {
                return new Action(() => {
                    Debug.WriteLine("logged in");
                    _navPage.Navigation.PopModalAsync();


                });
            }
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

