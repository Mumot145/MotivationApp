using System;
using Gcm.Client;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Auth;
using Xamarin.Forms;
using System.Linq;
using MotivationChat.Droid;
using Java.Security;

namespace MotivationAdmin.Droid
{
	[Activity (Label = "MotivationAdmin.Droid",
		Icon = "@drawable/icon",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		Theme = "@android:style/Theme.Holo.Light")]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        // Define a authenticated user.
        private MobileServiceUser user;
        static MainActivity instance = null;

        public MobileServiceUser _user
        {
            get {
                return user;
            }
            
        }
        public async Task<bool> Authenticate()
        {
            var success = false;
            var message = string.Empty;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                user = await TodoItemManager.DefaultManager.CurrentClient.LoginAsync(this,
                    MobileServiceAuthenticationProvider.Facebook);
                if (user != null)
                {
                   
                    message = string.Format("you are now signed-in as {0}. with info : {0}",
                        user.UserId, user.MobileServiceAuthenticationToken);
                    
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WE HIT AN ERROR BOYSS");
                message = ex.Message;
            }

            // Display the success or failure message.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetTitle("Sign-in result");
            builder.Create().Show();

            return success;
        }
        // Return the current activity instance.
        public static MainActivity CurrentActivity
        {
            get
            {
                return instance;
            }
        }

        protected override void OnCreate (Bundle bundle)
		{
            // Set the current instance of MainActivity.
            instance = this;

            base.OnCreate (bundle);

			// Initialize Azure Mobile Apps
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			// Initialize Xamarin Forms
			global::Xamarin.Forms.Forms.Init (this, bundle);
            IEnumerable<Account> accounts = AccountStore.Create(Forms.Context).FindAccountsForService("Facebook");
            // Initialize the authenticator before loading the app.
            //App.Init((IAuthenticate)this);
            System.Diagnostics.Debug.WriteLine("accounts..."+ accounts.FirstOrDefault());
            // Load the main application
            LoadApplication (new MotiveApp(accounts.FirstOrDefault()));

            PackageInfo info = this.PackageManager.GetPackageInfo("com.xamarin.sample.MotivationAdmin", PackageInfoFlags.Signatures);

            foreach (Android.Content.PM.Signature signature in info.Signatures)
            {
                MessageDigest md = MessageDigest.GetInstance("MD5");
                md.Update(signature.ToByteArray());

                string keyhash = Convert.ToBase64String(md.Digest());
                Console.WriteLine("KeyHash:", keyhash);
            }
            try
            {
                // Check to ensure everything's set up right
                GcmClient.CheckDevice(this);
                GcmClient.CheckManifest(this);

                // Register for push notifications
                System.Diagnostics.Debug.WriteLine("Registering...");
                GcmClient.Register(this, PushHandlerBroadcastReceiver.SENDER_IDS);
            }
            catch (Java.Net.MalformedURLException)
            {
                CreateAndShowDialog("There was an error creating the client. Verify the URL.", "Error");
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e.Message, "Error");
            }
        }
        private void CreateAndShowDialog(String message, String title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}

