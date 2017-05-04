using Microsoft.WindowsAzure.MobileServices;
using MotivationChat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace MotivationChat
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupList : ContentPage
    {
        bool authenticated = false;
        FacebookUser facebookUser = new FacebookUser();
        private string token;
        public GroupList(string _aToken)
        {           
            InitializeComponent();
            token = _aToken;
            
            BindingContext = new GroupListViewModel();
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            await DisplayAlert("Selected", e.SelectedItem.ToString(), "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Refresh items only when authenticated.
            if (String.IsNullOrEmpty(token))
            {
                await Navigation.PushModalAsync(new LoginPage());
            } else
            {
                await GetFacebookProfileAsync(token);
                //await Register();
            }
            
        }
        public async Task GetFacebookProfileAsync(string accessToken)
        {
            var requestUrl = "https://graph.facebook.com/v2.8/me/"
                             + "?fields=name,picture,cover,age_range,devices,email,gender,is_verified"
                             + "&access_token=" + accessToken;
            var httpClient = new HttpClient();
            var userJson = await httpClient.GetStringAsync(requestUrl);
            facebookUser = JsonConvert.DeserializeObject<FacebookUser>(userJson);
            Debug.WriteLine("graph name ="+facebookUser.Name);
            Debug.WriteLine("graph id ="+facebookUser.Id);

        }
      
    }



    class GroupListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Group> Groups { get; }
        public ObservableCollection<Grouping<string, Group>> ItemsGrouped { get; }

        public GroupListViewModel()
        {
            Groups = new ObservableCollection<Group>(new[]
            {
                new Group { Text = "Group 1", Detail = "Patrick" },
                new Group { Text = "Group 2", Detail = "Kamil" },
                new Group { Text = "Group 3", Detail = "Alex" },
                new Group { Text = "Group 4", Detail = "Albert" },
                new Group { Text = "Group 5", Detail= "Adam" },
                new Group { Text = "Group 6", Detail = "Matthew" },
                new Group { Text = "Group 7", Detail = "David" },
            });

            var sorted = from item in Groups
                         orderby item.Text
                         group item by item.Text[0].ToString() into itemGroup
                         select new Grouping<string, Group>(itemGroup.Key, itemGroup);

            ItemsGrouped = new ObservableCollection<Grouping<string, Group>>(sorted);

            RefreshDataCommand = new Command(
                async () => await RefreshData());
        }

        public ICommand RefreshDataCommand { get; }

        async Task RefreshData()
        {
            IsBusy = true;
            //Load Data Here
            await Task.Delay(2000);

            IsBusy = false;
        }

        bool busy;
        public bool IsBusy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged();
                ((Command)RefreshDataCommand).ChangeCanExecute();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public class Group
        {
            public string Text { get; set; }
            public string Detail { get; set; }

            public override string ToString() => Text;
        }

        public class Grouping<K, T> : ObservableCollection<T>
        {
            public K Key { get; private set; }

            public Grouping(K key, IEnumerable<T> items)
            {
                Key = key;
                foreach (var item in items)
                    this.Items.Add(item);
            }
        }
    }
}
