using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyREST.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.FeatureExtractors;
using PatternRecognition.FingerprintRecognition.Matchers;
using System.Drawing.Drawing2D;

namespace MyREST
{
    public class ParkRepository
    {

        SqlConnection Con;
        SqlCommand Com;
        SqlDataReader Reader;
        GlobalConfigurations GC = new GlobalConfigurations();

        ParkingYard py = new ParkingYard();

        string Query;
        public bool parkingYardExists;
        private int ParkID;

        public int GetParkID
        {
            get
            {
                return ParkID;
            }

        }
        
        private double MatchfingerPrints(Bitmap im1, Bitmap im2)
        {
            // Loading fingerprints
            var fingerprintImg1 = im1;
            var fingerprintImg2 = im2;

            // Building feature extractor and extracting features
            var featExtractor = new MTripletsExtractor() { MtiaExtractor = new Ratha1995MinutiaeExtractor() };
            var features1 = featExtractor.ExtractFeatures(fingerprintImg1);
            var features2 = featExtractor.ExtractFeatures(fingerprintImg2);

            // Building matcher and matching
            var matcher = new M3gl();
            double similarity = matcher.Match(features1, features2);


            return similarity * 100;
        }

        public int GetUser(Bitmap fingerprint)
        {
            int res = 0;

            foreach(User record in GetFingerPrints())
            {
                if(MatchfingerPrints(fingerprint,new ImageConverter().ToBitmap(record.FingerPrint))>80)
                {
                    res = record.ID;
                    break;
                }
                else
                {
                    res = 0;
                }
           
            }

            return res;
        }

        
        private List<User> GetFingerPrints()
        {
            List<User> ls = new List<User>();
            
            using(var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "select ua.fingerprint,ua.user_id from user_account ua";

                using(var com=new SqlCommand(Query, con))
                {
                    Reader = com.ExecuteReader();

                    while (Reader.Read())
                    {
                        if (Reader["fingerprint"] != DBNull.Value)
                        {

                            User d = new User();
                            d.ID = (Convert.ToInt32(Reader["user_id"].ToString()));
                            d.FingerPrint=Reader["fingerprint"].ToString();

                            ls.Add(d);
                        }
                    }

                    com.Dispose();
                }

                con.Close();
            }

            return ls;
        }
        public void RegisterDriver(int UserID)
        {
            using(var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "RegisterDriver";

                using(var com=new SqlCommand(Query, con))
                {

                    com.CommandType = CommandType.StoredProcedure;

                    com.Parameters.Add(new SqlParameter("@UserId", UserID));
                  

                    com.ExecuteNonQuery();

                    com.Dispose();
                }

                con.Close();
            }
        }

        public User GetCarDriver(int CarID,int ParkID)
        {
            User Driver = new User();

            using(var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "Select top 1 * from Users u inner join user_Account ua on u.id=ua.user_id inner join driver d on d.user_id=u.id inner join car_driver cd on cd.Driver_id=d.id inner join car_park cp on cp.car_driver_id=cd.id inner join park_ticket pt on pt.car_park_id=cp.id where cd.car_id='"+CarID+"' and cp.Park_id='"+ParkID+"' and pt.status=0 order by cp.id desc";

                using(var com=new SqlCommand(Query, con))
                {
                    Reader = com.ExecuteReader();

                    Driver.NationalRegInfo = new Citizen();

                    while (Reader.Read())
                    {
                        
                        Driver.ID = Convert.ToInt32(Reader["id"].ToString());
                        Driver.NationalRegInfo.FirstName = Reader["FirstName"].ToString();
                        Driver.NationalRegInfo.LastName = Reader["LastName"].ToString();
                        Driver.NationalRegInfo.MiddleName = Reader["MiddleName"].ToString();
                        Driver.Email = Reader["mail"].ToString();
                        Driver.UserName = Reader["UserName"].ToString();

                        if (Reader["fingerprint"] != DBNull.Value)
                        {
                            Driver.FingerPrint = Reader["fingerprint"].ToString();
                        }
                        Driver.NationalRegInfo.NIN = Reader["nin"].ToString();
                        Driver.PhoneNumber = Reader["PhoneNumber"].ToString();

                    }

                    com.Dispose();
                }

                con.Close();
            }

            return Driver;
        }

        public List<History> GetParkHistory(int ParkID)
        {
            
            List<History> hlist = new List<History>();

            using (var con = new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "select * from dbo.CarParkActivityHistory where Park_Id='"+ParkID+"'";

                using (var com = new SqlCommand(Query, con))
                {
                    Reader = com.ExecuteReader();

                    while (Reader.Read())
                    {
                        History h = new History();
                        
                        h.TicketID = Convert.ToInt32(Reader["Ticket"].ToString());

                        h.From = Convert.ToDateTime(Reader["Entered"].ToString());

                        h.To= Convert.ToDateTime(Reader["Exited"].ToString());

                        hlist.Add(h);

                    }

                    com.Dispose();
                    
                }

                con.Close();
            }

            return hlist;
        }


        
        public ParkTicket GetTicket(int ID)
        {

            ParkTicket pt = new ParkTicket();

            using (var con = new SqlConnection(GC.ConnectionString))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                
                Query = "select * from ParkTicketView where id='"+ID+"'";

                using (var com = new SqlCommand(Query, con))
                {
                    Reader = com.ExecuteReader();

                    while (Reader.Read())
                    {
                        pt.TicketID = Convert.ToInt32(Reader["id"].ToString());
                        pt.DriverName = Reader["FirstName"].ToString() + " " + Reader["LastName"].ToString();
                        pt.License = Reader["license"].ToString();
                        pt.ParkName = Reader["ParkName"].ToString();
                        pt.Date = Convert.ToDateTime(Reader["Date"].ToString());


                    }

                    com.Dispose();
                }

                con.Close();
            }

            return pt;
        }

        private bool AuthenticateDriver(int UserID, int CarID, int ParkID, out int CarDriverID, out int ResultingDriver, out string Error)
        {
            int response = 0;

            int DriverID = 0;

            int CarDriver = 0;

            bool result = false;

            SMS sms = new SMS();


            using (var con = new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "AuthenticateDriverProc";

                using (var com = new SqlCommand(Query, con))
                {

                    com.CommandType = CommandType.StoredProcedure;

                    com.Parameters.Add(new SqlParameter("@UserID", UserID));
                    com.Parameters.Add(new SqlParameter("@CarID", CarID));
                    com.Parameters.Add(new SqlParameter("@ParkID", ParkID));
                    com.Parameters.Add(new SqlParameter("@response", SqlDbType.Int, 20, ParameterDirection.Output, false, 0, 10, null, DataRowVersion.Default, null));
                    com.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int, 20, ParameterDirection.Output, false, 0, 10, null, DataRowVersion.Default, null));
                    com.Parameters.Add(new SqlParameter("@CarDriverID", SqlDbType.Int,20,ParameterDirection.Output,false,0,10,null,DataRowVersion.Default,null));


                    com.UpdatedRowSource = UpdateRowSource.OutputParameters;

                    com.ExecuteNonQuery();

                    response = Convert.ToInt32(com.Parameters["@response"].Value);
                    DriverID = Convert.ToInt32(com.Parameters["@DriverID"].Value);
                    CarDriverID = Convert.ToInt32(com.Parameters["@CarDriverID"].Value);
                    
                    ResultingDriver = DriverID;

                    com.Dispose();
                }

                con.Close();

                if (response != 0)
                {

                    if (CarDriverID > 0)
                    {
                        result = true;
                        Error = "Success";
                    }
                    else
                    {
                        result = false;
                        Error = "Sorry, Failed to Match Car Driver";
                    }

                }
                else
                {
                    User Driver = new User();
                    Driver = GetCarDriver(CarID, ParkID);


                    if (Driver.PhoneNumber != null)
                    {
                        try
                        {
                            
                            sms.OpenPort(GC.SerialPortName, "9600");

                            sms.sendMsg(Driver.PhoneNumber, "Hello '" + Driver.NationalRegInfo.FirstName + " " + Driver.NationalRegInfo.LastName + "'," + System.Environment.NewLine + "Looks like you or someone got it worng on taking your car out of "+GetParkingYard(ParkID).Name);

                        }
                        catch
                        {

                        }
                    }

                    Error = "Sorry, Car has been Suspended from exit. Please Contact Manager";
                    
                }




            }

            return result;
        }

        public bool ExitParking(int DriverID, int CarID, int ParkID,out Response ErrorResponse)
        {
            int CarDriverID = 0;
            int DriverID2 = 0;
            string Error = null;

            Response rsp = new Response();

            if (AuthenticateDriver(DriverID,CarID,ParkID,out CarDriverID,out DriverID2,out Error))
            {
               
                rsp.Message = Error;
                rsp.Data = DriverID2.ToString();

                ErrorResponse = rsp;

                using (var con=new SqlConnection(GC.ConnectionString))
                {
                    con.Open();

                    Query = "ExitParking";

                    using(var com=new SqlCommand(Query, con))
                    {
                        com.CommandType = CommandType.StoredProcedure;

                        com.Parameters.Add(new SqlParameter("@CarDriverID",CarDriverID));
                        com.Parameters.Add(new SqlParameter("@ParkID",ParkID));

                        com.ExecuteNonQuery();
                        com.Dispose();
                    }
                    con.Close();
                }

               

                return true;
            }
            else
            {
                rsp.Message = Error;
                rsp.Data = DriverID2.ToString();

                ErrorResponse = rsp;

                return false;
            }
        }

        public ParkTicket GetTicket(int ParkID,int CarID)
        {
            ParkTicket pt = new ParkTicket();

            using(var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "Select * from ParkTicketView ptv inner join Park_Ticket pt on ptv.id=pt.id inner join car_park cp on pt.car_park_id=cp.id inner join car_driver cd on pt.car_driver_id=cd.id where cp.park_id='"+ParkID+"' and cd.car_id='"+CarID+"'";

                using(var com=new SqlCommand(Query, con))
                {
                    Reader = com.ExecuteReader();

                    while (Reader.Read())
                    {
                        pt.TicketID = Convert.ToInt32(Reader["id"].ToString());
                        pt.DriverName = Reader["FirstName"].ToString() + " " + Reader["LastName"].ToString();
                        pt.License = Reader["license"].ToString();
                        pt.ParkName = Reader["ParkName"].ToString();
                        pt.Date = Convert.ToDateTime(Reader["Date"].ToString());
                    }
                    com.Dispose();
                }
                con.Close();
            }
            return pt;
        }

        public bool GetParking(int ParkID,int CarID,int UserID)
        {
            
            using(var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "GetParking";

                using (var com=new SqlCommand(Query, con))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.Add(new SqlParameter("@ParkId",ParkID));
                    com.Parameters.Add(new SqlParameter("@CarID",CarID));
                    com.Parameters.Add(new SqlParameter("@UserID",UserID));

                    com.ExecuteNonQuery();

                    return true;

                    com.Dispose();
                }

                con.Close();
            }
        }

        public int GetCarID(string License)
        {
            if (CarExists(License))
            {
                using (var con = new SqlConnection(GC.ConnectionString))
                {
                    con.Open();

                    Query = "Select dbo.GetCar(@License)";

                    using (var Com = new SqlCommand(Query, con))
                    {
                        Com.Parameters.Add(new SqlParameter("@License", License));

                        int Result = (int)Com.ExecuteScalar();

                        return Result;
                        Com.Dispose();
                    }


                    con.Close();
                }
            }
            else
            {

                CreateCar(License);

                return GetCarID(License);
                
            }
        }

        private void CreateCar(string license)
        {
            using (var con = new SqlConnection(GC.ConnectionString))
            {

                con.Open();

                Query = "RegisterCar";

                using(var com=new SqlCommand(Query, con))
                {

                    com.CommandType = CommandType.StoredProcedure;

                    com.Parameters.Add(new SqlParameter("@license", license));

                    com.ExecuteNonQuery();

                    com.Dispose();
                }
                con.Close();

            }

        }

        public bool CarExists(string License)
        {
            using (var con = new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "SELECT count(c.id) as res FROM car c where c.License=@license";

                using (var Com = new SqlCommand(Query, con))
                {

                    Com.Parameters.Add(new SqlParameter("@license", License));

                    Reader = Com.ExecuteReader();

                    Reader.Read();

                    if (Convert.ToInt32(Reader["res"].ToString()) >= 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
                con.Close();
            }
            
        }

        public void CreateEmployee(int UserID,int ParkID)
        {
            using(var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "CreateEmployee";

                using(var com=new SqlCommand(Query, con))
                {
                    com.CommandType = CommandType.StoredProcedure;

                    com.Parameters.Add(new SqlParameter("@UserID",UserID));
                    com.Parameters.Add(new SqlParameter("@ParkID", ParkID));
                    com.Parameters.Add(new SqlParameter("@EmployeeType",1));

                    com.ExecuteNonQuery();

                    com.Dispose();

                }

                con.Close();
            }
        }
        
        public List<ParkingYard.Activities> GetActivities(int ParkID)
        {

            List<ParkingYard.Activities> ActivitiesList = new List<ParkingYard.Activities>();
            ParkingYard.Activities activity;

            using (var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "select * from dbo.CarParkActivityHistory where Park_id='"+ParkID+"'";

                using(var com=new SqlCommand(Query, con))
                {
                    Reader = com.ExecuteReader();

                    while (Reader.Read())
                    {
                        activity = new ParkingYard.Activities();
                        activity.License = Reader["license"].ToString();
                        activity.Owner = Reader["Name"].ToString();
                        activity.IN = Convert.ToDateTime(Reader["Entered"].ToString());
                        activity.OUT = Convert.ToDateTime(Reader["Exited"].ToString());

                        ActivitiesList.Add(activity);
                    }
                    com.Dispose();
                }
                con.Close();
            }

            return ActivitiesList;
        }

        public async Task<bool> CreateParkingYard(ParkingYard yard)
        {

            if (ParkingYardExists(yard) == false)
            {
                using (Con = new SqlConnection(GC.ConnectionString))
                {
                    if (Con.State == ConnectionState.Closed)
                    {
                        Con.Open();
                    }
                    
                    Query = "CreatePark";

                    using (Com = new SqlCommand(Query, Con))
                    {
                        Com.CommandType = CommandType.StoredProcedure;

                        //byte[] ImageByte = null;
                        //MemoryStream ms = new MemoryStream();
                        //yard.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        //ImageByte = ms.ToArray();

                       
                        Com.Parameters.Add(new SqlParameter("@name", yard.Name));
                        Com.Parameters.Add(new SqlParameter("@placeid", yard.Place_ID));
                        Com.Parameters.Add(new SqlParameter("@placeref", yard.Reference));
                        Com.Parameters.Add(new SqlParameter("@locationname", yard.Location.Name));
                        Com.Parameters.Add(new SqlParameter("@locationlong", yard.Location.Longitude));
                        Com.Parameters.Add(new SqlParameter("@locationlat", yard.Location.Latitude));
                        Com.Parameters.Add(new SqlParameter("@Image",yard.Image));

                        ParkID =(int)Com.ExecuteScalar();
                        
                        System.Diagnostics.Debug.WriteLine("Park id: "+ParkID);

                        Com.Dispose();
                    }

                    Con.Close();

                   GenerateSpaceData();
                    
                    return true;
                }
                
            }

            else
            {
                return false;
            }
        }

        public bool ParkingYardExists(ParkingYard yard)
        {
        
            using(Con=new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select count(*) as result,p.id from Park p where p.name=@name and p.placeid=@placeid group by p.id";

                using(Com=new SqlCommand(Query, Con))
                {
                    Com.Parameters.Add(new SqlParameter("@name", yard.Name));
                    Com.Parameters.Add(new SqlParameter("@placeid", yard.Place_ID));

                    Reader = Com.ExecuteReader();

                    int Result = 0;

                    while (Reader.Read())
                    {
                        Result = Convert.ToInt32(Reader["result"].ToString());

                        if (Reader["id"] != DBNull.Value)
                        {
                            ParkID = Convert.ToInt32(Reader["id"].ToString());
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("Results:"+Result);

                    Com.Dispose();

                    Con.Close();

                    if (Result > 0)
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

       
        public void CreateEmployee(int UserID,int Type,int ParkID)
        {
            using(var con=new SqlConnection(GC.ConnectionString))
            {
                con.Open();

                Query = "CreateEmployee";

                using(var Com=new SqlCommand(Query, con))
                {
                    Com.CommandType = CommandType.StoredProcedure;

                    Com.Parameters.Add(new SqlParameter("@UserID",UserID));
                    Com.Parameters.Add(new SqlParameter("@EmployeeType",Type));
                    Com.Parameters.Add(new SqlParameter("@ParkID",ParkID));

                    Com.ExecuteNonQuery();

                    Com.Dispose();
                }

                con.Close();

            }
        }
       

        public void GenerateSpaceData()
        {

            using (Con = new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Insert into park_spaces(park_id,Spacecount,usedspace) values('" + ParkID + "','" + new Random().Next(20,50) + "',0)";

                using (Com=new SqlCommand(Query, Con))
                {
                    Com.ExecuteNonQuery();
                }
            }

         }

        public List<Employees> GetEmployees(int ParkID)
        {
            List<Employees> emplist = new List<Employees>();

            using (Con = new SqlConnection(GC.ConnectionString))
            {
                Con.Open();

                Query = "select * from dbo.Employee_View where Park_id='" + ParkID + "'";

                using (var com = new SqlCommand(Query, Con))
                {
                    Reader = com.ExecuteReader();

                    while (Reader.Read())
                    {
                        Employees emp = new Employees();
                        emp.EmpId = Convert.ToInt32(Reader["id"].ToString());
                        emp.Name = Reader["Name"].ToString();
                        emp.Position = Reader["Position"].ToString();
                        emp.Gender = Reader["Sex"].ToString();

                        emplist.Add(emp);
                    }

                    com.Dispose();

                }

                Con.Close();

            }
                return emplist;

            }

        public ParkingYard GetParkingYard(string Place_ID)
        {
            ParkingYard Yard = new ParkingYard();

            using (Con = new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select * from Park p left join park_location pl on p.ID=pl.Park_ID left join Park_spaces on pl.park_id=park_spaces.park_id where p.placeid='" + Place_ID + "'";

                using (Com = new SqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        Yard.id = Convert.ToInt32(Reader["ID"].ToString());
                        Yard.Name = Reader["Name"].ToString();
                        Yard.Reference = Reader["placeref"].ToString();
                        Yard.Place_ID = Reader["placeid"].ToString();
                        Yard.Image = Reader["Image"].ToString();

                        //Get Yard Location
                        ParkingYard.YardLocation yardlocation = new ParkingYard.YardLocation();
                        if (Reader["L_ID"] != DBNull.Value)
                        {
                            yardlocation.id = Convert.ToInt32(Reader["L_ID"].ToString());
                        }
                        if (Reader["long"] != DBNull.Value)
                        {
                            yardlocation.Longitude = Convert.ToDouble(Reader["Long"].ToString());
                        }
                        if (Reader["Lat"] != DBNull.Value)
                        {
                            yardlocation.Latitude = Convert.ToDouble(Reader["Lat"].ToString());
                        }

                        if (Reader["Vicinity"] != DBNull.Value)
                        {
                            yardlocation.Name = Reader["Vicinity"].ToString();
                        }

                        Yard.Location = yardlocation;

                        //Get Yard Spaces
                        ParkingYard.YardSpaces yardspaces = new ParkingYard.YardSpaces();
                        if (Reader["Space_id"] != DBNull.Value)
                        {
                            yardspaces.Space_ID = Convert.ToInt32(Reader["Space_id"].ToString());
                        }
                        if (Reader["SpaceCount"] != DBNull.Value)
                        {
                            yardspaces.Count = Convert.ToInt32(Reader["SpaceCount"].ToString());
                        }
                        if (Reader["UsedSpace"] != DBNull.Value)
                        {
                            yardspaces.UsedSpaces = Convert.ToInt32(Reader["UsedSpace"].ToString());
                        }

                        Yard.Spaces = yardspaces;
                    }

                    Com.Dispose();
                }

                Con.Close();
            }

            return Yard;
        }

        public ParkingYard GetParkingYard(int ParkID)
        {
            ParkingYard Yard = new ParkingYard();

            using(Con=new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select * from Park p left join park_location pl on p.ID=pl.Park_ID left join Park_spaces on pl.park_id=park_spaces.park_id where p.ID='" + ParkID + "'";

                using(Com=new SqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        Yard.id = Convert.ToInt32(Reader["ID"].ToString());
                        Yard.Name = Reader["Name"].ToString();
                        Yard.Reference = Reader["placeref"].ToString();
                        Yard.Place_ID = Reader["Placeid"].ToString();
                        Yard.Image = Reader["Image"].ToString();

                        //Get Yard Location
                        ParkingYard.YardLocation yardlocation = new ParkingYard.YardLocation();
                        if (Reader["L_ID"] != DBNull.Value)
                        {
                            yardlocation.id = Convert.ToInt32(Reader["L_ID"].ToString());
                        }
                        if (Reader["Long"] != DBNull.Value)
                        {
                            yardlocation.Longitude = Convert.ToDouble(Reader["Long"].ToString());
                        }
                        if (Reader["Lat"] != DBNull.Value)
                        {
                            yardlocation.Latitude = Convert.ToDouble(Reader["Lat"].ToString());
                        }
                        if (Reader["Vicinity"] != DBNull.Value)
                        {
                            yardlocation.Name = Reader["Vicinity"].ToString();
                        }

                        Yard.Location = yardlocation;

                        //Get Yard Spaces
                        ParkingYard.YardSpaces yardspaces = new ParkingYard.YardSpaces();
                        if (Reader["Space_id"] != DBNull.Value)
                        {
                            yardspaces.Space_ID = Convert.ToInt32(Reader["Space_id"].ToString());
                        }
                        if (Reader["SpaceCount"] != DBNull.Value)
                        {
                            yardspaces.Count = Convert.ToInt32(Reader["SpaceCount"].ToString());
                        }
                        if (Reader["UsedSpace"] != DBNull.Value)
                        {
                            yardspaces.UsedSpaces = Convert.ToInt32(Reader["UsedSpace"].ToString());
                        }

                        Yard.Spaces = yardspaces;
                    }

                    Com.Dispose();
                }

                Con.Close();
            }

            return Yard;
        }

        public ParkingYard [] GetParkingYards()
        {
           
            List<ParkingYard> YardList = new List<ParkingYard>();

            using (Con = new SqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select * from Park p left join Park_Location pl on p.ID=pl.Park_ID left join Park_spaces on pl.park_id=park_spaces.park_id";

                using (Com = new SqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        ParkingYard Yard = new ParkingYard();

                        Yard.id = Convert.ToInt32(Reader["ID"].ToString());
                        Yard.Name = Reader["Name"].ToString();
                        Yard.Reference = Reader["placeref"].ToString();
                        Yard.Place_ID = Reader["placeid"].ToString();
                        Yard.Image = Reader["Image"].ToString();
                       
                        
                        //Get Yard Location

                        ParkingYard.YardLocation yardlocation = new ParkingYard.YardLocation();
                        if (Reader["L_ID"] != DBNull.Value)
                        {
                            yardlocation.id = Convert.ToInt32(Reader["L_ID"].ToString());
                        }
                        if (Reader["Long"] != DBNull.Value)
                        {
                            yardlocation.Longitude = Convert.ToDouble(Reader["Long"].ToString());
                        }
                        if (Reader["Lat"] != DBNull.Value)
                        {
                            yardlocation.Latitude = Convert.ToDouble(Reader["Lat"].ToString());
                        }
                        if (Reader["Vicinity"] != DBNull.Value)
                        {
                            yardlocation.Name = Reader["Vicinity"].ToString();
                        }
                        Yard.Location = yardlocation;

                        //Get Yard Spaces
                        ParkingYard.YardSpaces yardspaces = new ParkingYard.YardSpaces();

                        if (Reader["Space_id"] !=DBNull.Value)
                        {
                            yardspaces.Space_ID = Convert.ToInt32(Reader["Space_id"].ToString());
                        }
                        if (Reader["SpaceCount"] != DBNull.Value)
                        {
                            yardspaces.Count = Convert.ToInt32(Reader["SpaceCount"].ToString());
                        }
                        if (Reader["UsedSpace"] != DBNull.Value)
                        {
                            yardspaces.UsedSpaces = Convert.ToInt32(Reader["UsedSpace"].ToString());
                        }

                        Yard.Spaces = yardspaces;

                        YardList.Add(Yard);
                    }

                    Com.Dispose();
                }

                Con.Close();
            }

            return YardList.ToArray();
        }


    }
}