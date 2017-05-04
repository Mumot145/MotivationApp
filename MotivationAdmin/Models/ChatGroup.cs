using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MotivationAdmin.Models
{
    public class ChatGroup : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        private List<User> users = new List<User>();
        public List<TodoItem> toDos = new List<TodoItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public List<User> UserList
        {
            get { return users; }
            set { users = value; }
        }
        public List<TodoItem> ToDoList
        {
            get { return toDos; }
            set { toDos = value; }
        }
            

    }
}
