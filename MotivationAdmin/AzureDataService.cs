using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using MotivationAdmin.Models;
using System.Linq;

namespace MotivationAdmin
{
    public class AzureDataService
    {

        SqlConnection connection = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;
        public void Initialize()
        {
            // string conString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
            //db_con = new OleDbConnection(conString);
            connection = new SqlConnection(Constants.AzureSQLConnection);

        }

        public User GetUser(string Info, string Method)
        {
            User _user = new User();
            string query="";
            if (Method == "fbId")
            {
                query = "SELECT Id, Name, FacebookId, AdminBool FROM Users WHERE FacebookId = '" + Info + "'";
             
            } else if(Method == "rId")
            {
                query = "SELECT Id, Name, FacebookId, AdminBool FROM Users WHERE Id = '" + Info + "'";
            } else if(Method == "Name")
            {
                query = "SELECT Id, Name, FacebookId, AdminBool FROM Users WHERE Name = '" + Info + "'";
            }
                         
            _user = (User) AzureConnect(query, "User");
            return _user;
        }
        
        public void RegisterUser(string facebookName, string facebookId)
        {
            string query = "INSERT INTO Users (Name, FacebookId, AdminBool) VALUES ('" + facebookName + "', '" + facebookId + "', 1)";
            AzureConnect(query, "Insert");
        }
        public void AddNewGroup(string cgName, User _user)
        {
            string query = "INSERT INTO ChatGroups (Name) VALUES ('" + cgName + "'); SELECT SCOPE_IDENTITY();";
            int lastId = (int) AzureConnect(query, "CheckId");
            if (lastId != 0)
            {
                AddUserToGroup(_user, lastId);
            }          
        }
        public void AddUserToGroup(User _user, int _id)
        {
            string query1 = "INSERT INTO UserChatGroups (UserId, ChatGroupId) VALUES ('" + _user.Id + "', '" + _id + "')";
            AzureConnect(query1, "Insert");
        }
        public List<ChatGroup> GetGroups(int _userId)
        {
            //List<String> _groupIdList = new List<String>();
            List<ChatGroup> _groupList = new List<ChatGroup>();


            string query = "SELECT cg.Id Id, cg.Name Name FROM ChatGroups cg INNER JOIN UserChatGroups ucg"
                                + " ON ucg.ChatGroupId = cg.Id"
                                + " WHERE ucg.UserId =" + _userId;

            _groupList = (List<ChatGroup>)AzureConnect(query, "ChatGroups");

            if (_groupList != null)
            {

                _groupList = GetGroupUsers(_groupList);
                _groupList = GetGroupToDos(_groupList);
            }


            return _groupList;
        }
        public List<ChatGroup> GetGroupUsers(List<ChatGroup> groupList)
        {
            List<User> _userList = new List<User>();
            User _user = new User();
            string groups = "";
            foreach (var g in groupList)
            {
                if (groups == "")               
                    groups = g.Id + ", ";                
                else               
                    groups = groups + g.Id + ", ";            
            }
            groups = groups.Remove(groups.Length - 2);
            string query = "SELECT u.Id, u.Name, u.FacebookId, u.AdminBool, ucg.ChatGroupId FROM Users u INNER JOIN UserChatGroups ucg"
                                + " ON ucg.UserId = u.Id"
                                + " WHERE ucg.ChatGroupId IN (" + groups + ")";
 
            List<ChatGroup> chatGroupList = (List<ChatGroup>)AzureConnect(query, "GroupUserList");
            foreach(var gl in groupList)
            {
                //gl.UserList = (List<User>)chatGroupList.Where(cg => cg.Id == gl.Id).Select(x => x.UserList);
                Debug.WriteLine("wehave name" + gl.GroupName);
                Debug.WriteLine("we want this in"+chatGroupList.Where(cg => cg.Id == gl.Id).Select(x => x.UserList).FirstOrDefault());

                gl.UserList = (List<User>)chatGroupList.Where(cg => cg.Id == gl.Id).Select(x => x.UserList);
                //gl.UserList = (List<User>) chatGroupList.Where(cg=>cg.Id == gl.Id).Select(x => x.UserList);
            }

            return groupList;
        }
        public List<User> GetSingleGroupUsers(ChatGroup chatGroup)
        {
            List<User> _userList = new List<User>();
            User _user = new User();

            string query = "SELECT u.Id, u.Name, u.FacebookId, u.AdminBool, ucg.ChatGroupId FROM Users u INNER JOIN UserChatGroups ucg"
                                + " ON ucg.UserId = u.Id"
                                + " WHERE ucg.ChatGroupId ='" + chatGroup.Id.ToString() + "'";

            List<User> chatGroupList = (List<User>)AzureConnect(query, "UserList");
  

            return chatGroupList;
        }
        private static readonly int[] RetriableClasses = { 13, 16, 17, 18, 19, 20, 21, 22, 24 };

        public object AzureConnect(string Query, string taskType)
        {
            bool rebuildConnection = true; // First try connection must be open
            object returnValue = null;
            for (int i = 0; i < RetriableClasses[4]; ++i)
            {
                try
                {
                    // (Re)Create connection to SQL Server
                    if (rebuildConnection)
                    {
                        if (connection != null)
                            connection.Dispose();

                        // Create connection and open it...
                        Initialize();
                        cmd.CommandText = Query;
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        connection.Open();

                    }

                    // inserts information
                    if(taskType == "Insert")
                    {
                        int newrows = cmd.ExecuteNonQuery();
                        Console.WriteLine($"Inserted {newrows} row(s).");
                        connection.Close();
                        return newrows;
                    } else //finds information
                    {
                    reader = cmd.ExecuteReader();
                        if(taskType == "User")
                        {
                            returnValue = readUser(reader);
                        } else if(taskType == "ChatGroups")
                        {
                            returnValue = readChatGroups(reader);
                        }  else if (taskType == "CheckId")
                        {
                            returnValue = readLastId(reader);
                        } else if (taskType == "GroupUserList")
                        {
                            returnValue = readGroupUsers(reader);
                        } else if (taskType == "UserList")
                        {
                            returnValue = readSingleGroupUsers(reader);
                        }
                    }
                    

                    // No exceptions, task has been completed
                    return returnValue;
                }
                catch (SqlException e)
                {
                    if (e.Errors.Cast<SqlError>().All(x => CanRetry(x)))
                    {
                        // What to do? Handle that here, also checking Number property.
                        // For Class < 20 you may simply Thread.Sleep(DelayOnError);

                        rebuildConnection = e.Errors
                            .Cast<SqlError>()
                            .Any(x => x.Class >= 20);

                        continue;
                    }

                    throw;
                }
            }
            return null;
        }
        private int readLastId(SqlDataReader _reader)
        {
            int _lastId = 0;          
            if (_reader != null)
            {
                while (reader.Read())
                {
                    _lastId = Convert.ToInt32(reader[0]);
                }
            }

            connection.Close();

            return _lastId;
        }
        private User readUser(SqlDataReader _reader)
        {
            User _user = new User();
            Debug.WriteLine("passed in with reader +"+ _reader);
            if (_reader != null)
            {
                while (reader.Read())
                {
                    _user.Id = Convert.ToInt32(reader[0]);
                    _user.Name = String.Format("{0}", reader[1]);
                    _user.FacebookId = String.Format("{0}", reader[2]);
                    _user.Admin = Convert.ToBoolean(reader[3]);                    
                }
            }

            connection.Close();

            return _user;
        }
        private List<ChatGroup> readGroupUsers(SqlDataReader _reader)
        {
            User _user = new User();
            List<User> _userList = new List<User>();
            ChatGroup _chatGroup = new ChatGroup();
            List<ChatGroup> _chatGroupList = new List<ChatGroup>();
            int groupNo = 0;
            int lastGroupNo = 0;
            Debug.WriteLine("passed in with reader +" + _reader);
            if (_reader != null)
            {
                while (reader.Read())
                {
                    _user.Id = Convert.ToInt32(reader[0]);
                    _user.Name = String.Format("{0}", reader[1]);
                    _user.FacebookId = String.Format("{0}", reader[2]);
                    _user.Admin = Convert.ToBoolean(reader[3]);
                    groupNo = Convert.ToInt32(reader[4]);
                    Debug.WriteLine("FINDING USERS");
                    if ((groupNo != lastGroupNo) && lastGroupNo != 0)
                    {
                        _chatGroup.UserList = _userList;
                        _chatGroup.Id = lastGroupNo;
                        _chatGroupList.Add(_chatGroup);
                    }
                    lastGroupNo = groupNo;
                    _userList.Add(_user);
                }   
            }
            connection.Close();

            return _chatGroupList;
        }
        private List<User> readSingleGroupUsers(SqlDataReader _reader)
        {
            User _user = new User();
            List<User> _userList = new List<User>();

            Debug.WriteLine("passed in with reader +" + _reader);
            if (_reader != null)
            {
                while (reader.Read())
                {
                    _user.Id = Convert.ToInt32(reader[0]);
                    _user.Name = String.Format("{0}", reader[1]);
                    _user.FacebookId = String.Format("{0}", reader[2]);
                    _user.Admin = Convert.ToBoolean(reader[3]);
                    _userList.Add(_user);
                }
            }

            connection.Close();

            return _userList;
        }
        private List<ChatGroup> readChatGroups(SqlDataReader _reader)
        {
            
            List<ChatGroup> _chatGroupList = new List<ChatGroup>();
            Debug.WriteLine("passed in with reader +" + _reader);
            if (_reader != null)
            {
                while (reader.Read())
                {
                    ChatGroup _group = new ChatGroup();
                    _group.Id = Convert.ToInt32(String.Format("{0}", reader[0]));
                    _group.GroupName = String.Format("{0}", reader[1]);                   
                    _chatGroupList.Add(_group);
                }
            }

            connection.Close();

            return _chatGroupList;
        }
        private static bool CanRetry(SqlError error)
        {
            // Use this switch if you want to handle only well-known errors,
            // remove it if you want to always retry. A "blacklist" approach may
            // also work: return false when you're sure you can't recover from one
            // error and rely on Class for anything else.
            switch (error.Number)
            {
                // Handle well-known error codes, 
            }

            // Handle unknown errors with severity 21 or less. 22 or more
            // indicates a serious error that need to be manually fixed.
            // 24 indicates media errors. They're serious errors (that should
            // be also notified) but we may retry...
            return RetriableClasses.Contains(error.Class); // LINQ...
        } 
        
        
        public List<ChatGroup> GetGroupToDos(List<ChatGroup> groupList)
        {
            string groups = "";
            //ChatGroup cg = new ChatGroup();
            //List<TodoItem> toDoList = new List<TodoItem>();
            
            foreach (var g in groupList)
            {
                if (groups == "")
                    groups = "'"+g.Id + "', '";

                groups = groups + g.Id + "', '";
            }
            groups = groups.Remove(groups.Length - 3);
            cmd.CommandText = "SELECT id, text, groupId, complete FROM ToDoItem WHERE groupId IN (" + groups + ")";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            connection.Open();

            //Debug.WriteLine(connection);
            reader = cmd.ExecuteReader();
            if (reader != null)
            {
                while (reader.Read())
                {
                    TodoItem toDo = new TodoItem();
                    toDo.Id = String.Format("{0}", reader[0]);
                    toDo.ToDo = String.Format("{0}", reader[1]);
                    toDo.GroupId = String.Format("{0}", reader[2]);
                    toDo.Done = Convert.ToBoolean( reader[3]);


                   // int groupId = Convert.ToInt32(String.Format("{0}", reader[4]));
                    ChatGroup sGroup = groupList.Single(c => c.Id == Convert.ToInt32(toDo.GroupId));
                    sGroup.ToDoList.Add(toDo);

                }
            }

            //sGroup.ToDoList.Add(toDoList);
            connection.Close();


            //coupons = await GetCouponImages(place);
            return groupList;
        }

        
        public void DeleteFromGroup(int id, ChatGroup chatGroup)
        {
            cmd.CommandText = "DELETE FROM UserChatGroups WHERE UserId = '" + id.ToString() + "' AND ChatGroupId = '" + chatGroup.Id + "'";

            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            connection.Open();

            int oldrows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Deleted {oldrows} row(s).");
            connection.Close();
        }

    }
}
