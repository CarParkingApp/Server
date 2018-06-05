using MyREST.Models;
using PatternRecognition.FingerprintRecognition.FeatureExtractors;
using PatternRecognition.FingerprintRecognition.Matchers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MyREST
{

  
    public class NIRA
    {
        
        SqlConnection Con;
        SqlCommand Com;
        SqlDataReader Reader;
        string Query;

        GlobalConfigurations GC = new GlobalConfigurations();
        public bool CheckPerson(string nin)
        {
            using (var con = new SqlConnection(GC.ConnectionString))

            {
                con.Open();
                Query = "select dbo.CheckNIRAPerson(@nin)";

                using (var com=new SqlCommand(Query, con))
                {
                    com.Parameters.Add(new SqlParameter("@NIN", nin));

                    var value = (int)com.ExecuteScalar();
                    com.Dispose();
                    con.Close();

                    if (value == 1)
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
        public int GetPerson(string nin)
        {
            int PersonID = 0;

            using (var con = new SqlConnection(GC.ConnectionString))

            {
                con.Open();
                Query = "select dbo.GetNIRAPerson(@nin)";

                using (var com = new SqlCommand(Query, con))
                {
                    com.Parameters.Add(new SqlParameter("@NIN", nin));

                    PersonID = (int)com.ExecuteScalar();
                    com.Dispose();
                }
                con.Close();
            }

            return PersonID;
        }
        public bool CheckPerson(Bitmap FingerPrint)
        {
            bool res = false;
            
            foreach (Citizen person in GetPeoplePrints())
            {
                if (person.FingerPrint != null)
                {
                    if (MatchfingerPrints(FingerPrint, new ImageConverter().ToBitmap(person.FingerPrint)) > 80)
                    {
                        res = true;
                    }
                    else
                    {
                        res = false;
                    }
                }
            }
            return res;
        }

        public int GetPerson(Bitmap FingerPrint)
        {
            int res = 0;

            foreach (Citizen person in GetPeoplePrints())
            {

                if (person.FingerPrint != null)
                {
                    if (MatchfingerPrints(FingerPrint, new ImageConverter().ToBitmap(person.FingerPrint)) > 80)
                    {
                        res = person.ID;
                        break;
                    }
                    else
                    {
                        res = 0;
                    }
                }
            }
            return res;
        }

        private List<Citizen> GetPeoplePrints()
        {
            
            Citizen person=null;
            List<Citizen> personlist = new List<Citizen>();

            using (var con=new SqlConnection(GC.NIRAConnectionString))
            {
                con.Open();

                Query = "select p.* from Person p";

                using (var com=new SqlCommand(Query, con))
                {
                    Reader = com.ExecuteReader();

                    while (Reader.Read())
                    {
                        person = new Citizen();
                        person.ID = Convert.ToInt32(Reader["id"].ToString());

                        if (Reader["fingerprint"]!=DBNull.Value)
                        {

                        person.FingerPrint = Reader["fingerprint"].ToString();

                        }
                        else
                        {
                            person.FingerPrint = null;
                        }

                        personlist.Add(person);


                    }

                    com.Dispose();
                }
                con.Close();
            }
            return personlist;
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
        
      

    }
}