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


        public ActionResult Test()
        {
            string dbPath = Server.MapPath("~/App_Data/LineMeSql.sqlite");            
            string cnStr = "data source=" + dbPath;
            List<Models.LineUserSpeak> data = new List<Models.LineUserSpeak>();
            using (var conn = new SQLiteConnection(cnStr))
            {
                conn.Open();
                data = conn.Query<Models.LineUserSpeak>("select * from LineUserSpeak").ToList();
            }
            return View(data);
        }

        public ActionResult Report()
        {
            string dbPath = Server.MapPath("~/App_Data/LineMeSql.sqlite");            
            string cnStr = "data source=" + dbPath;
            string strResult = string.Empty;
            using (var conn = new SQLiteConnection(cnStr))
            {
                conn.Open();
                string GroupId = "C0b89d56006a79e4cd6dfeecfb3f06dcd";
                var etime = DateTime.UtcNow.AddHours(8);
                var stime = etime.Date;
                string sql = "select * from LineUserSpeak where SpeakDate between @stime and @etime and Groupid=@Groupid ";
                List<LineUserSpeak> datas = conn.Query<LineUserSpeak>(sql, param: new { stime = stime, etime = etime, Groupid = GroupId }).ToList();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append($"{datas.FirstOrDefault().SpeakDate.Date.ToString("yyyy-MM-dd")} 共有 {datas.Count} 則對話 \r\n ");
                var speakName = (from c in datas
                                 select c.UserId                                     
                                 ).Distinct();
                List<Speaker> speakers = new List<Speaker>();
                foreach (var item in speakName)
                {
                    if (!speakers.Any(d=>d.Id==item))
                    {
                        speakers.Add(new Speaker() { Id = item, Name = datas.FirstOrDefault(d => d.UserId == item).DisplayName, CountData = datas.Where(d => d.UserId == item).Count() });
                    }
                }
                speakers = speakers.OrderByDescending(d => d.CountData).ToList();
                strResult = Newtonsoft.Json.JsonConvert.SerializeObject(speakers);
               
            }
            return Json(strResult,JsonRequestBehavior.AllowGet);
        }
    }
}
