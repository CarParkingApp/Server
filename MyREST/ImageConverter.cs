using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace MyREST
{
    public class ImageConverter
    {

        public byte [] ToByteArr(Bitmap bm)
        {
            
            MemoryStream ms = new MemoryStream();
            ms.Seek(0, SeekOrigin.Begin);
            bm.Save(ms, ImageFormat.Jpeg);

            return ms.ToArray(); 

        }

        public byte[] ToByteArr(string s)
        {
            return Convert.FromBase64String(s);

        }

        public Bitmap ToBitmap(string str)
        {
            return ToBitmap(Convert.FromBase64String(str));
        }


        public Bitmap ToBitmap(byte [] b)
        {
            Bitmap bm;

            using (var ms = new MemoryStream(b))
            {
                bm = new Bitmap(ms);
            }


            return bm;
        }

        
        public string ToBase64String(byte [] b)
        {
            return Convert.ToBase64String(b);
        }

     }
}