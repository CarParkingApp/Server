using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyREST.Models;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Http;
using System.Drawing.Imaging;

namespace MyREST
{
    public class maps
    {
        private const string APIKEY = "AIzaSyBG8KUYh-_Zm7iEjPD5VPrI90fz6QTrAEY";

        public maps()
        {

        }

        private async Task<String> GetRoute(ParkingYard.YardLocation Origin, ParkingYard.YardLocation Destination)
        {

            StringBuilder googleRoutesUrl = new StringBuilder(@"https://maps.googleapis.com/maps/api/directions/json");
            googleRoutesUrl.Append("?origin=" + Origin.Latitude + ',' + Origin.Longitude);
            googleRoutesUrl.Append("&destination=" + Destination.Latitude + ',' + Destination.Longitude);
            googleRoutesUrl.Append("&sensor=true");
            googleRoutesUrl.Append("&key=" + APIKEY);

            string url = googleRoutesUrl.ToString();

            WebRequest request = WebRequest.Create(url);

            WebResponse response = request.GetResponse();

            Stream data = response.GetResponseStream();

            StreamReader reader = new StreamReader(data);

            string responseFromServer = reader.ReadToEnd();

            response.Close();

            return responseFromServer;

        }

        private async Task<string> GetPlaceImage(string photoreference)
        {
           
            StringBuilder PlaceImageURL = new StringBuilder(@"https://maps.googleapis.com/maps/api/place/photo?");
            PlaceImageURL.Append("maxwidth=400");
            PlaceImageURL.Append("&minwidth=400");
            PlaceImageURL.Append("&photoreference=" + photoreference);
            PlaceImageURL.Append("&key=" + APIKEY);

            string url = PlaceImageURL.ToString();
            
            WebClient wc = new WebClient();
          
            string responseFromServer= Convert.ToBase64String(wc.DownloadData(url));

            // byte[] ImageByte = new byte[data.Length];

            // data.Read(ImageByte, 0,ImageByte.Length);
            
            
            return responseFromServer;
        }

        public async Task<Response> GetNearByPlaces(double Lat, double Lng, String Category, double Proximity_Radius)
        {
            
            StringBuilder googlePlacesUrl = new StringBuilder(@"https://maps.googleapis.com/maps/api/place/nearbysearch/json?");
            googlePlacesUrl.Append("location=" + Lat + "," + Lng);
            googlePlacesUrl.Append("&radius=" + Proximity_Radius);
            googlePlacesUrl.Append("&type=" + Category);
            googlePlacesUrl.Append("&sensor=true");
            googlePlacesUrl.Append("&key=" + APIKEY);

            string url = googlePlacesUrl.ToString();
            Response rsp = new Response();

            try
            {

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
                rsp.Code = Convert.ToInt32(response.StatusCode);

                Stream data = response.GetResponseStream();

                StreamReader reader = new StreamReader(data);

                // json-formatted string from maps api
                string responseFromServer = reader.ReadToEnd();

                StringBuilder Response = new StringBuilder();

                JObject JSONObject = JObject.Parse(responseFromServer);

                JArray jarr = new JArray();

                jarr = (JArray)JSONObject["results"];

                response.Close();
                String res = null;

                for (int count = 0; count < jarr.Count; count++)
                {

                    JObject Location = new JObject();
                    Location = (JObject)jarr[count]["geometry"]["location"];

                    res = Location.GetValue("lat").ToString();

                    ParkingYard.YardLocation DestinPark, OriginPark;
                    OriginPark = new ParkingYard.YardLocation();
                    OriginPark.Latitude = Lat;
                    OriginPark.Longitude = Lng;

                    DestinPark = new ParkingYard.YardLocation();
                    DestinPark.Latitude = Convert.ToDouble(Location.GetValue("lat"));
                    DestinPark.Longitude = Convert.ToDouble(Location.GetValue("lng"));

                    string RouteResult = await GetRoute(OriginPark, DestinPark);

                    JArray PhotosArray = (JArray)jarr[count]["photos"];

                    string PlaceImage = null;

                    if (PhotosArray != null)
                    {
                         PlaceImage = await GetPlaceImage((PhotosArray[0]["photo_reference"]).ToString());
                    }
                    else
                    {
                        PlaceImage = null;
                    }
               
                    JObject RoutesObject = JObject.Parse(RouteResult);

                    JObject JarrObject = (JObject)jarr[count];
                    
                    JarrObject.Add("place image", PlaceImage);
                    JarrObject.Add("routes", RoutesObject);
                   
                    jarr[count] = JarrObject;

                }

                rsp.Data = jarr.ToString();
                rsp.Message = response.StatusDescription;
                

            }
            catch (WebException e)
            {

                rsp = new Response();

                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    rsp.Code = Convert.ToInt32(((HttpWebResponse)e.Response).StatusCode);

                }

                rsp.Code = 404;
                rsp.Message = "No Internet Connection";



            }

            return rsp;
        }

    }
}