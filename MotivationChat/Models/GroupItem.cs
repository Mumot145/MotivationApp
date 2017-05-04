using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotivationChat.Models
{
    public class GroupItem
    {
        string id;
        string group;
        string groupName;
        string admin;
        List<string> groupUsers;

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "groupName")]
        public string Name
        {
            get { return groupName; }
            set { groupName = value; }
        }

        [JsonProperty(PropertyName = "groupId")]
        public string GroupId
        {
            get { return group; }
            set { group = value; }
        }

    }
}
