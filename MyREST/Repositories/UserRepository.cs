using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;
using MyREST.Models;
using System.Threading.Tasks;

namespace MyREST
{
    public class UserRepository
    {
        MySqlConnection Con;
        MySqlCommand Com;
        MySqlDataReader Reader;
        string Query;
        bool UserAuthentcated = false;

        GlobalConfigurations GC = new GlobalConfigurations();

        public UserRepository()
        {
            using(Con=new MySqlConnection(GC.ConnectionString))
            {
                try
                {
                    switch (Con.State)
                    {
                        case ConnectionState.Open:
                            break;

                        case ConnectionState.Closed:
                            Con.Open();
                            break;
                    }
                }

                catch(Exception ex)
                {
                    Console.WriteLine(ex.Data);
                }

            }
        }

        public async Task<bool> CreateUser(User user)
        {

            bool UserCreated = false;

            if (await UserExists(user))
            {
                Console.WriteLine("User already exists");
             
            }
            else
            {
                Query = "Insert into Users(ID,First_Name,Last_Name,Middle_Name,UserName,Email,PhoneNumber,pwd) values('" + user.ID + "','" + user.FirstName + "','" + user.LastName + "','" + user.MiddleName + "','" + user.UserName + "','" + user.Email + "','" + user.PhoneNumber + "','" + user.Password + "')";

                using (Con = new MySqlConnection(GC.ConnectionString))
                {
                    if (Con.State == ConnectionState.Closed)
                    {
                        Con.Open();
                    }
                    using (Com = new MySqlCommand(Query, Con))
                    {
                        Com.ExecuteNonQuery();

                        Com.Dispose();

                        UserCreated = true;
                    }
                }
            }

            return UserCreated;
        }

        private async Task<bool> UserExists(User user)
        {  
            using(var Con=new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select count(*) as result from Users where Users.Email='" + user.Email + "' or Users.PhoneNumber='" + user.PhoneNumber + "'";

                using(var Com=new MySqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();
                    int Count=0;

                    while (Reader.Read())
                    {

                        Count = Convert.ToInt32(Reader["result"].ToString());
                        
                    }

                    if (Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public User GetUser(int ID)
        {
            User user = new User();

            Query = "Select * from Users where ID='"+ID+"'";

            using (Con = new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                using (Com = new MySqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        user.ID = Convert.ToInt32(Reader["ID"].ToString());
                        user.FirstName = Reader["First_Name"].ToString();
                        user.LastName = Reader["Last_Name"].ToString();
                        user.MiddleName = Reader["Middle_Name"].ToString();
                        user.Email = Reader["Email"].ToString();
                        user.UserName = Reader["UserName"].ToString();
                        user.PhoneNumber = Reader["PhoneNumber"].ToString();
                        
                    }

                    Com.Dispose();
                }

            }
            return user;
        }

        public bool AuthenticateUser(string Email, string Password)
        {
            Query = "select count(*) as result from Users where Users.Email='" + Email + "' and Users.pwd='" + Password + "'";
            using (Con = new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }
                using (Com = new MySqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    int Count = Convert.ToInt32(Reader["result"].ToString());

                    if (Count > 0)
                    {
                        UserAuthentcated = true;
                    }
                    else
                    {
                        UserAuthentcated = false;
                    }
                }

                return UserAuthentcated;
            }
        }
    }
}