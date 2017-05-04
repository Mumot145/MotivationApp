using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using MotivationChat;
using MotivationChat.Models;


namespace MotivationChat
{
    public class AzureDataService
    {
        public MobileServiceClient MobileService { get; set; }
        //IMobileServiceSyncTable coffeeTable;
        List<String> coupons;

        SqlConnection connection = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader reader;
        public async Task Initialize()
        {
            // string conString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
            //db_con = new OleDbConnection(conString);
            connection = new SqlConnection(Constants.AzureSQLConnection);

        }

        public string FindUserFB(string fbName, string fbId)
        {
            string userName = "";


            cmd.CommandText = "SELECT Name FROM Users WHERE FacebookId = " + fbId;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            connection.Open();

            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                userName = String.Format("{0}", reader[0]);
                Console.WriteLine(String.Format("{0}", reader[0]));
            }
            connection.Close();

            if (!String.IsNullOrEmpty(fbId))
            {
                if (String.IsNullOrEmpty(userName))
                {
                    //register user 
                    RegisterUser(fbName, fbId);
                }
            }

            Debug.WriteLine("user that is found -" + userName);
            //coupons = await GetCouponImages(place);
            return userName;
        }

        private void RegisterUser(string facebookName, string facebookId)
        {
            cmd.CommandText = "INSERT INTO Users (Name, FacebookId) VALUES ('" + facebookName + "', '" + facebookId + "')";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            connection.Open();

            int newrows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Inserted {newrows} row(s).");
            connection.Close();
        }

        public User GetUserInfo(String FacebookId)
        {
            User user = new User();
            cmd.CommandText = "SELECT * FROM Users WHERE FacebookId = '" + FacebookId + "'";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            connection.Open();

            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                user.Id = String.Format("{0}", reader[0]);
                user.name = String.Format("{0}", reader[1]);
            }
            connection.Close();
            user.FacebookId = FacebookId;

            return user;
        }
    
        public Task<List<Coupon>> GetBackpack(User _user)
        {

            List<String> _couponId = new List<String>();
            List<Coupon> _coupons = new List<Coupon>();


            cmd.CommandText = "SELECT * FROM UserBackpack WHERE UserId = '" + _user.Id + "'";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            connection.Open();

            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _couponId.Add(String.Format("{0}", reader[0]));
                //Console.WriteLine(String.Format("{0}", reader[0]));
            }
            connection.Close();
            //backpack.couponList = ;
            // _coupons = GetCouponInfo(_couponId);
            //coupons = await GetCouponImages(place);

            return null;
        }

        //private List<Coupon> GetCouponInfo(List<Coupon> _couponId)
        //{

        //    return null;
        //}
        public void AddToBackpack(Coupon _coupon, User _user)
        {
            cmd.CommandText = "INSERT INTO UserBackpack (UserId, CouponId) VALUES ('" + _user.Id + "', '" + _coupon.id + "')";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
            connection.Open();

            int newrows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Inserted {newrows} row(s).");
            connection.Close();
        }
        public async Task<List<Coupon>> GetCoupons(List<String> placeId)
        {
            List<String> place = new List<String>();
            List<Coupon> coupons = new List<Coupon>();
            foreach (var p in placeId)
            {

                cmd.CommandText = "SELECT StoreId FROM StoreLocations WHERE PlaceId = '" + p + "'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = connection;
                connection.Open();

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    place.Add(String.Format("{0}", reader[0]));
                    Console.WriteLine(String.Format("{0}", reader[0]));
                }
                connection.Close();
            }

            coupons = await GetCouponImages(place);

            return coupons;
        }
        public async Task<List<Coupon>> GetCouponImages(List<String> places)
        {
            List<Coupon> couponImages = new List<Coupon>();
            int i = 0;
            // Coupon cpn = new Coupon();
            foreach (var place in places)
            {
                cmd.CommandText = "SELECT Id, CouponId FROM Coupons WHERE Id = '" + place + "'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = connection;
                connection.Open();
                Coupon cpn = new Coupon();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // cpn = null;

                    cpn.StoreId = String.Format("{0}", reader[0]);
                    cpn.ImageUrl = String.Format("{0}", reader[1]);

                    cpn.id = i;

                    couponImages.Add(cpn);


                    i++;
                }
                connection.Close();
            }

            var coupons = await GetCouponTypes(couponImages);

            return coupons;
        }
        public async Task<List<Coupon>> GetCouponTypes(List<Coupon> _coupons)
        {
            List<Coupon> finalCoupons = new List<Coupon>();
            int i = 0;
            // Coupon cpn = new Coupon();
            foreach (var c in _coupons)
            {
                cmd.CommandText = "SELECT LocationType FROM Stores WHERE Id = '" + c.StoreId + "'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = connection;
                connection.Open();

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // cpn = null;

                    c.CouponType = String.Format("{0}", reader[0]);

                    finalCoupons.Add(c);
                    Console.WriteLine(String.Format("coupong type is ---->>> {0}", c.CouponType));

                    i++;
                }
                connection.Close();
            }



            return finalCoupons;
        }

    }
}
