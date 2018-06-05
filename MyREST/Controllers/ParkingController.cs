using System;
using System.Collections.Generic;
using System.Web.Http;
using MyREST.Models;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MyREST.Controllers
{
    public class ParkingController : ApiController
    {
        User user = new User();
        string Response;
        UserRepository UserRepo;
        ParkRepository ParkRepo;

        List<Employees> Emps = new List<Employees>()
        {
            new Employees {EmpId=1,Name="Katabalwa Steven",Age=23 },
            new Employees {EmpId=2,Name="Kasawuli Allan",Age=27 }
        };

        [HttpGet]
        public List<ParkingYard.Activities> GetActivities([FromUri] int ID)
        {
            ParkRepo = new ParkRepository();
            return ParkRepo.GetActivities(ID);
        }

        [HttpPost]
        public int GetUser([FromBody]string fingerprint)
        {
            ParkRepo = new ParkRepository();
            
            return ParkRepo.GetUser(new ImageConverter().ToBitmap(new ImageConverter().ToByteArr(fingerprint)));
        }

        [HttpGet]
        public bool RegisterDriver([FromUri]int userid)
        {
            
            ParkRepo = new ParkRepository();
            ParkRepo.RegisterDriver(userid);

            return true;
        }
       
        [HttpGet]
        public User Users(int id)
        {
            UserRepo = new UserRepository();

            return UserRepo.GetUser(id);
        }

        [HttpGet]
        public IEnumerable<Employees> Employees()
        {
            return Emps;
        }

        [HttpGet]
        public bool CheckCar([FromUri] string License)
        {
            ParkRepo = new ParkRepository();
            return ParkRepo.CarExists(License);
        }

        [HttpGet]
        public User Users([FromUri] string nin)
        {
            UserRepo = new UserRepository();
           return UserRepo.GetUser(nin);
        }
        
        [HttpGet]
        public ParkTicket GetTicket([FromUri] int ParkID,[FromUri] int CarID)
        {
            ParkRepo = new ParkRepository();
            return ParkRepo.GetTicket(ParkID, CarID);
        }

        [HttpGet]
        public bool ExitParking([FromUri]int UserID,[FromUri] int ParkID, [FromUri] int CarID)
        {
            ParkRepo = new ParkRepository();
            return ParkRepo.ExitParking(UserID, CarID, ParkID);
        }

        [HttpGet]
        public bool GetParking([FromUri] int ParkID,[FromUri] int CarID,[FromUri] int UserID)
        {
            ParkRepo = new ParkRepository();
            return ParkRepo.GetParking(ParkID,CarID,UserID);
        }

        [HttpGet]
        public int RegisterCar([FromUri] string License)
        {
            ParkRepo = new ParkRepository();
            return ParkRepo.GetCarID(License);
        }

        [HttpGet]
        public Employees Employee(int id)
        {

            Employees Emp = new Employees();

            foreach(Employees Employee in Emps)
            {
                if (Employee.EmpId == id)
                {
                    Emp = Employee;
                }

            }

            return Emp;
        }

        [HttpGet]
        public async Task<JObject> Parks([FromUri]double lat,[FromUri] double lng)
        {
            maps mp = new maps();

            Response Response = new Response();
            Response=await mp.GetNearByPlaces(lat,lng, "parking", 10000);
            JObject Error = new JObject();

            if (Response.Code == 200)

            {
                Error.Add("Code", Response.Code);
                Error.Add("Message", "Success");

                ParkingYard Yard = new ParkingYard();
            
                JArray jarr = JArray.Parse(Response.Data);
                
                for (int count = 0; count < jarr.Count; count++)
                {
                    Yard.Image = jarr[count]["place image"].ToString();
                    Yard.Name = jarr[count]["name"].ToString();
                    Yard.Place_ID = jarr[count]["place_id"].ToString();
                    Yard.Reference = jarr[count]["reference"].ToString();

                    ParkingYard.YardLocation YardLocation = new ParkingYard.YardLocation();
                    YardLocation.Name = jarr[count]["vicinity"].ToString();
                    YardLocation.Latitude = Convert.ToDouble(jarr[count]["geometry"]["location"]["lat"]);
                    YardLocation.Longitude = Convert.ToDouble(jarr[count]["geometry"]["location"]["lng"]);

                    Yard.Location = YardLocation;

                    ParkRepo = new ParkRepository();

                    if (await ParkRepo.CreateParkingYard(Yard))
                    {
                        Response.Message = "Parking Created Successfully";
                    }

                    else
                    {
                        Response.Message = "Parking Already exists";
                    }

                    JObject Parking = new JObject();

                    ParkingYard py = ParkRepo.GetParkingYard(Yard.Place_ID);

                    JObject Space = new JObject();

                    Space.Add("space id", py.Spaces.Space_ID);
                    Space.Add("space count", py.Spaces.Count);
                    Space.Add("used spaces", py.Spaces.UsedSpaces);

                    JObject Location = new JObject();
                    Location.Add("id", py.Location.id);
                    Location.Add("name", py.Location.Name);
                    
                    Parking.Add("park_id", py.id);
                    Parking.Add("Space", Space);
                    Parking.Add("Location", Location);
                
                    JObject FinalJarr = (JObject) jarr[count];
                    FinalJarr.Add("Park Data", Parking);

                    jarr[count] = FinalJarr;

                }

               // JArray FinalJSONArray = (JArray)jarr;

                Error.Add("values", jarr);
                
            }

            else
            {
              
                Error.Add("Code", Response.Code);
                Error.Add("Message", Response.Message);
                Error.Add("Values", Response.Data);
                
            }

            return Error;
        }
        
        [HttpPost]
        public async Task<string> Users([FromBody]User user)
        {      
            UserRepo = new UserRepository();

            var response = await UserRepo.CreateUser(user);

            if (response == UserResponse.SUCCESS)
            {
                Response = "User has been successfully created.";
            }

            else if(response == UserResponse.USEREXISTS)
            {
                Response = "Sorry, User Already Exists";
            }

            else
            {
                Response = "Not Registered";
            }

            return Response;

        }
        
        [HttpGet]
        public ParkingYard [] Parks()
        {
            
            ParkRepo = new ParkRepository();

            return ParkRepo.GetParkingYards();
        }

        [HttpGet]
        public ParkingYard Parks(int ID)
        {
            ParkRepo = new ParkRepository();

            return ParkRepo.GetParkingYard(ID);
        }

        [HttpGet]
        public void CreateEmployee([FromUri] int UserID,[FromUri] int ParkID)
        {
            ParkRepo = new ParkRepository();
            ParkRepo.CreateEmployee(UserID, ParkID);
        }
    }
}
