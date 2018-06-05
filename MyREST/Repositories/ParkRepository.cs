using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyREST.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;
using System.IO;

namespace MyREST
{
    public class ParkRepository
    {

        MySqlConnection Con;
        MySqlCommand Com;
        MySqlDataReader Reader;
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

        public async Task<bool> CreateParkingYard(ParkingYard yard)
        {

            if (ParkingYardExists(yard) == false)
            {
                using (Con = new MySqlConnection(GC.ConnectionString))
                {
                    if (Con.State == ConnectionState.Closed)
                    {
                        Con.Open();
                    }
                    
                    Query = "select CreatePark(@name,@placeid,@placeref,@locationname,@locationlong,@locationlat,@Image)";

                    using (Com = new MySqlCommand(Query, Con))
                    {
                        //byte[] ImageByte = null;
                        //MemoryStream ms = new MemoryStream();
                        //yard.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        //ImageByte = ms.ToArray();

                       
                        Com.Parameters.Add(new MySqlParameter("@name", yard.Name));
                        Com.Parameters.Add(new MySqlParameter("@placeid", yard.Place_ID));
                        Com.Parameters.Add(new MySqlParameter("@placeref", yard.Reference));
                        Com.Parameters.Add(new MySqlParameter("@locationname", yard.Location.Name));
                        Com.Parameters.Add(new MySqlParameter("@locationlong", yard.Location.Longitude));
                        Com.Parameters.Add(new MySqlParameter("@locationlat", yard.Location.Latitude));
                        Com.Parameters.Add(new MySqlParameter("@Image",yard.Image));

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
        
            using(Con=new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select count(*) as result,ID from Parks where Name='"+MySqlHelper.EscapeString(yard.Name)+"' and Place_ID='"+yard.Place_ID+"'";

                using(Com=new MySqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    int Result = 0;

                    while (Reader.Read())
                    {
                        Result = Convert.ToInt32(Reader["result"].ToString());

                        if (Reader["ID"] != DBNull.Value)
                        {
                            ParkID = Convert.ToInt32(Reader["ID"].ToString());
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

        public void GenerateSpaceData()
        {

            using (Con = new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Insert into park_spaces(Park_ID,Space_count,used) values('" + ParkID + "','" + new Random().Next(20,50) + "',0)";

                using (Com=new MySqlCommand(Query, Con))
                {
                    Com.ExecuteNonQuery();
                }
            }

         }

        public ParkingYard GetParkingYard(string Place_ID)
        {
            ParkingYard Yard = new ParkingYard();

            using (Con = new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select * from Parks left join park_geometry on Parks.ID=park_geometry.Park_ID left join Park_spaces on park_geometry.park_id=park_spaces.park_id where Parks.Place_ID='" + Place_ID + "'";

                using (Com = new MySqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        Yard.id = Convert.ToInt32(Reader["ID"].ToString());
                        Yard.Name = Reader["Name"].ToString();
                        Yard.Reference = Reader["Reference"].ToString();
                        Yard.Place_ID = Reader["Place_ID"].ToString();
                        Yard.Image = Reader["Image"].ToString();

                        //Get Yard Location
                        ParkingYard.YardLocation yardlocation = new ParkingYard.YardLocation();
                        if (Reader["L_ID"] != DBNull.Value)
                        {
                            yardlocation.id = Convert.ToInt32(Reader["L_ID"].ToString());
                        }
                        if (Reader["Longitude"] != DBNull.Value)
                        {
                            yardlocation.Longitude = Convert.ToDouble(Reader["Longitude"].ToString());
                        }
                        if (Reader["Latitude"] != DBNull.Value)
                        {
                            yardlocation.Latitude = Convert.ToDouble(Reader["Latitude"].ToString());
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
                        if (Reader["Space_Count"] != DBNull.Value)
                        {
                            yardspaces.Count = Convert.ToInt32(Reader["Space_Count"].ToString());
                        }
                        if (Reader["Used"] != DBNull.Value)
                        {
                            yardspaces.UsedSpaces = Convert.ToInt32(Reader["Used"].ToString());
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

            using(Con=new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select * from Parks left join park_geometry on Parks.ID=park_geometry.Park_ID left join Park_spaces on park_geometry.park_id=park_spaces.park_id where Parks.ID='" + ParkID + "'";

                using(Com=new MySqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        Yard.id = Convert.ToInt32(Reader["ID"].ToString());
                        Yard.Name = Reader["Name"].ToString();
                        Yard.Reference = Reader["Reference"].ToString();
                        Yard.Place_ID = Reader["Place_ID"].ToString();
                        Yard.Image = Reader["Image"].ToString();

                        //Get Yard Location
                        ParkingYard.YardLocation yardlocation = new ParkingYard.YardLocation();
                        if (Reader["L_ID"] != DBNull.Value)
                        {
                            yardlocation.id = Convert.ToInt32(Reader["L_ID"].ToString());
                        }
                        if (Reader["Longitude"] != DBNull.Value)
                        {
                            yardlocation.Longitude = Convert.ToDouble(Reader["Longitude"].ToString());
                        }
                        if (Reader["Latitude"] != DBNull.Value)
                        {
                            yardlocation.Latitude = Convert.ToDouble(Reader["Latitude"].ToString());
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
                        if (Reader["Space_Count"] != DBNull.Value)
                        {
                            yardspaces.Count = Convert.ToInt32(Reader["Space_Count"].ToString());
                        }
                        if (Reader["Used"] != DBNull.Value)
                        {
                            yardspaces.UsedSpaces = Convert.ToInt32(Reader["Used"].ToString());
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

            using (Con = new MySqlConnection(GC.ConnectionString))
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                Query = "Select * from Parks left join Park_geometry on Parks.ID=Park_geometry.Park_ID left join Park_spaces on park_geometry.park_id=park_spaces.park_id";

                using (Com = new MySqlCommand(Query, Con))
                {
                    Reader = Com.ExecuteReader();

                    while (Reader.Read())
                    {
                        ParkingYard Yard = new ParkingYard();

                        Yard.id = Convert.ToInt32(Reader["ID"].ToString());
                        Yard.Name = Reader["Name"].ToString();
                        Yard.Reference = Reader["Reference"].ToString();
                        Yard.Place_ID = Reader["Place_ID"].ToString();
                        Yard.Image = Reader["Image"].ToString();
                       
                        
                        //Get Yard Location

                        ParkingYard.YardLocation yardlocation = new ParkingYard.YardLocation();
                        if (Reader["L_ID"] != DBNull.Value)
                        {
                            yardlocation.id = Convert.ToInt32(Reader["L_ID"].ToString());
                        }
                        if (Reader["Longitude"] != DBNull.Value)
                        {
                            yardlocation.Longitude = Convert.ToDouble(Reader["Longitude"].ToString());
                        }
                        if (Reader["Latitude"] != DBNull.Value)
                        {
                            yardlocation.Latitude = Convert.ToDouble(Reader["Latitude"].ToString());
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
                        if (Reader["Space_Count"] != DBNull.Value)
                        {
                            yardspaces.Count = Convert.ToInt32(Reader["Space_Count"].ToString());
                        }
                        if (Reader["Used"] != DBNull.Value)
                        {
                            yardspaces.UsedSpaces = Convert.ToInt32(Reader["Used"].ToString());
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