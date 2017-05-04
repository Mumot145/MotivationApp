using MotivationAdmin.Models;
using MotivationAdmin.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace MotivationAdmin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupDetails : TabbedPage
    {
        AzureDataService _azure = new AzureDataService();
        User _currentUser = new User();
        public GroupDetails(ChatGroup _chatGroup, User _user)
        {
            TodoList toDo = new TodoList(_chatGroup);
            
            InitializeComponent();
            BindingContext = _chatGroup;
            _currentUser = _user;
            Children.Add(toDo);
            
            var tbi = new ToolbarItem("+", "plus.png", () =>
            {
                
                var todoPage = new NewUser(_chatGroup);
                Navigation.PushAsync(todoPage);
            }, 0, 0);
            tbi.Order = ToolbarItemOrder.Secondary;  // forces it to appear in menu on Android
            ToolbarItems.Add(tbi);
            var tbi2 = new ToolbarItem("?", "chat.png", () =>
            {
                var todoPage = new NewUser((ChatGroup)BindingContext);
                Navigation.PushAsync(todoPage);
            }, 0, 0);
            ToolbarItems.Add(tbi2);
            tbi2.Order = ToolbarItemOrder.Secondary; // forces it to appear in menu on Android

        }
        void OnRefresh(object sender, RefreshEventArgs e)
        {
            refresh();
            userList.IsRefreshing = false;
        }
        void refresh()
        {
            List<ChatGroup> _group = new List<ChatGroup>();
            _group.Add((ChatGroup)BindingContext);
            List<ChatGroup> cgList = _azure.GetGroupUsers(_group);
            List<User> users = cgList.FirstOrDefault().UserList;
            Debug.WriteLine("refreshing rn ==" + users.FirstOrDefault());
            if (userList != null)
            {
                userList.ItemsSource = users;
            }
        }
        async void OnDelete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var u = (User)mi.CommandParameter;
            _azure.Initialize();
            _azure.DeleteFromGroup(u.Id, (ChatGroup)BindingContext);
            //DisplayAlert("Delete Context Action", mi.CommandParameter + " delete context action", "OK");
            Debug.WriteLine("deleted");
        }
    } 
}
