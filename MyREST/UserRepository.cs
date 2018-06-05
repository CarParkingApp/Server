using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;
using MyREST.Models;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Drawing;

namespace MyREST
{
    public enum UserResponse
    {
        USEREXISTS,SUCCESS,NOTREGISTERED
    }

    public class UserRepository
    {
        SqlConnection Con;
        SqlCommand Com;
        SqlDataReader Reader;
        string Query;
        bool UserAuthentcated = false;

        GlobalConfigurations GC = new GlobalConfigurations();

        public UserRepository()
        {
            using (Con = new SqlConnection(GC.ConnectionString))
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

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Data);
                }

            }
        }

        public async Task<UserResponse> CreateUser(User user)
        {
            bool UserCreated = false;
            NIRA nira = new NIRA();

            if (user.NationalRegInfo.NIN != null)
            {

                if (nira.CheckPerson(user.NationalRegInfo.NIN))
                {

                    user.NationalRegInfo.ID = nira.GetPerson(user.NationalRegInfo.NIN);

                    if (await UserExists(user))
                    {
                        return UserResponse.USEREXISTS;
                    }

                    else
                    {

                        Query = "CreateUser";

                        using (Con = new SqlConnection(GC.ConnectionString))
                        {
                            if (Con.State == ConnectionState.Closed)
                            {
                                Con.Open();
                            }
                            using (Com = new SqlCommand(Query, Con))
                            {

                                Com.CommandType = CommandType.StoredProcedure;

                                Com.Parameters.Add(new SqlParameter("@personid", user.NationalRegInfo.ID));
                                Com.Parameters.Add(new SqlParameter("@mail", user.Email));
                                Com.Parameters.Add(new SqlParameter("@phonenumber", user.PhoneNumber));
                                Com.Parameters.Add(new SqlParameter("@pswd", user.Password));

                                Com.ExecuteNonQuery();

                                Com.Dispose();

                                UserCreated = true;
                            }
                        }

                        return UserResponse.SUCCESS;
                    }
                }

            
            else
            {
                UserCreated = false;
                return UserResponse.NOTREGISTERED;
            }

            }
            else
            {


                if (nira.CheckPerson(new ImageConverter().ToBitmap(new ImageConverter().ToByteArr(user.NationalRegInfo.FingerPrint))))
                {

                    user.NationalRegInfo.ID = nira.GetPerson(new ImageConverter().ToBitmap(new ImageConverter().ToByteArr(user.NationalRegInfo.FingerPrint)));

                    if (await UserExists(user))
                    {
                        return UserResponse.USEREXISTS;
                    }

                    else
                    {

                        Query = "CreateUser";

                        using (Con = new SqlConnection(GC.ConnectionString))
                        {
                            if (Con.State == ConnectionState.Closed)
                            {
                                Con.Open();
                            }
                            using (Com = new SqlCommand(Query, Con))
                            {

                                Com.CommandType = CommandType.StoredProcedure;

                                Com.Parameters.Add(new SqlParameter("@personid", user.NationalRegInfo.ID.ToString()));
                                Com.Parameters.Add(new SqlParameter("@mail", user.Email));
                                Com.Parameters.Add(new SqlParameter("@phonenumber", user.PhoneNumber));
                                Com.Parameters.Add(new SqlParameter("@pswd", user.Password));

                                Com.ExecuteNonQuery();

                                Com.Dispose();

                                UserCreated = true;
                            }
                        }

                        return UserResponse.SUCCESS;
                    }
                }
                else
                {
                    UserCreated = false;
                    return UserResponse.NOTREGISTERED;
                }
            }

           
        }

        public async Task<bool> Login(User user,int Type,int ParkID)
        {
            using (var con = new SqlConnection(GC.ConnectionString))
            {

                con.Open();
          
                Query = "select count(*) as result from Users u inner join user_account ua on u.ID=ua.user_id inner join employee e on u.id=e.user_id inner join  park_employee pe on pe.employee_id=e.id where ua.mail='" + user.Email + "' and ua.pswd='" + user.Password + "' and pe.park_id='"+ParkID+"' and e.Employee_Type='"+Type+"'";

                using (var command = new SqlCommand(Query, con))
                {
                    Reader = command.ExecuteReader();

                    int count = 0;

                    while (Reader.Read())
                    {

                        if (Reader["result"] != DBNull.Value)
                        {
                            count = Convert.ToInt32(Reader["result"].ToString());
                        }
                        else
                        {
                            count = 0;
                            
                        }
                    }

                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    Com.Dispose();

                }

                con.Close();
            }

        }

      
        public User GetUser(string NIN)
        {
            User user = new User();

            using (var Con = new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select *  from users u inner join user_account ua on u.id=ua.user_id where u.nin='" + NIN + "'";

                using (var Com = new SqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();
    
                    while (Reader.Read())
                    {

                        user.NationalRegInfo = new Citizen();

                        user.ID = Convert.ToInt32(Reader["id"].ToString());
                        user.NationalRegInfo.FirstName = Reader["FirstName"].ToString();
                        user.NationalRegInfo.LastName = Reader["LastName"].ToString();
                        user.NationalRegInfo.MiddleName = Reader["MiddleName"].ToString();
                        user.Email = Reader["mail"].ToString();

                        if (Reader["fingerprint"] != DBNull.Value)
                        {
                            user.FingerPrint = Reader["fingerprint"].ToString();
                        }
                        user.NationalRegInfo.NIN = Reader["nin"].ToString();
                        user.PhoneNumber = Reader["PhoneNumber"].ToString();

                    }

                    Com.Dispose();

                }

                Con.Close();
            }
            return user;
          }
    
        private async Task<bool> CitizenExists(string NIN)
        {
            using (var Con = new SqlConnection(GC.NIRAConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select count(*) as result from person p inner join person_nin pn on p.id=pn.person_id where pn.nin='"+NIN+"'";

                using (var Com = new SqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();
                    int Count = 0;

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

        private async Task<bool> UserExists(User user)
        {  
            using(var Con=new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }
                
                Query = "Select count(*) as result from Users u inner join User_Account ua on u.id=ua.user_id  where ua.mail='" + user.Email + "' or ua.phonenumber='" + user.PhoneNumber + "'";

                using(var Com=new SqlCommand(Query, Con))
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

            Query = "Select * from Users u left join User_Account ua on u.id=ua.user_id where u.id='"+ID+"'";

            using (Con = new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                using (Com = new SqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        user.NationalRegInfo = new Citizen();

                        user.ID = Convert.ToInt32(Reader["id"].ToString());
                        user.NationalRegInfo.FirstName = Reader["FirstName"].ToString();
                        user.NationalRegInfo.LastName = Reader["LastName"].ToString();
                        user.NationalRegInfo.MiddleName = Reader["MiddleName"].ToString();
                        user.Email = Reader["mail"].ToString();
                        //user.UserName = Reader["UserName"].ToString();
                        user.PhoneNumber = Reader["PhoneNumber"].ToString();
                        
                    }

                    Com.Dispose();
                }

            }
            return user;
        }

       
    }
}