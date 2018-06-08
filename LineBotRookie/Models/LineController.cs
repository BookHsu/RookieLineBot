using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using NewLineMessageApi;
using Newtonsoft.Json;
namespace LineBotRookie.Models
{
    public class LineController : ApiController
    {
        protected const string Channel_secret = "3040177387e7014639f43494d564c2b1";
        protected const string Channel_Id = "1586060764";
        protected const string Channel_Access_Token = "dzKV1la1w+xWXsE6E3egJ/xWpu7ElAF7Gb9DGkWdBigwPERSoSkcWXCIoUFzGa4S53oyrue+Yw7IC51I/WuxcqOOF8WdBTZilvNcENHeO29bokyML2owrv4m4UOBaqHkypAHxF3qaYjQynyQsebvkAdB04t89/1O/w1cDnyilFU=";
        public string strConn { get; set; }
        public LineController():base()
        {
            
            strConn = "data source=" + System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/LineMeSql.sqlite");
        }
    }
}