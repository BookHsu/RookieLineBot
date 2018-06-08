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
        protected const string Channel_secret = "Channel_secret";
        protected const string Channel_Id = "Channel_Id";
        protected const string Channel_Access_Token = "Channel_Access_Token";
        public string strConn { get; set; }
        public LineController():base()
        {
            
            strConn = "data source=" + System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/LineMeSql.sqlite");
        }
    }
}