using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SQLite;
using Dapper;
using LineBotRookie.Models;
using static LineBotRookie.Models.LineUserSpeak;

namespace LineBotRookie.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            string dbPath = Server.MapPath("~/App_Data/LineMeSql.sqlite");
            string path = Server.MapPath("~/App_Data");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string cnStr = "data source=" + dbPath;
            if (!System.IO.File.Exists(dbPath))
            {
                using(var conn =new SQLiteConnection(cnStr))
                {
                    conn.Open();
                    string sql = @"create table LineUserSpeak ( 
                        Id INTEGER  PRIMARY KEY AUTOINCREMENT,
                        Groupid text,
                        DisplayName text not null,
                       UserId text not null,
                       MessageType text not null,
                        SpeakDate DATETIME not null
                          )";

                    conn.Execute(sql);
                }
            }           
            return View();
        }

    }
}
