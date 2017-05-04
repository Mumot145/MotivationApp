using System;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace MotivationAdmin.Models
{
	public class TodoItem
	{
		string id;
        string group;
        string todo;
		bool done;
       

		[JsonProperty(PropertyName = "id")]
		public string Id
		{
			get { return id; }
			set { id = value;}
		}

		[JsonProperty(PropertyName = "text")]
		public string ToDo
		{
			get { return todo; }
			set { todo = value;}
		}
        [JsonProperty(PropertyName = "groupId")]
        public string GroupId
        {
            get { return group; }
            set { group = value; }
        }

        [JsonProperty(PropertyName = "complete")]
		public bool Done
		{
			get { return done; }
			set { done = value;}
		}

        [Version]
        public string Version { get; set; }
	}
}

