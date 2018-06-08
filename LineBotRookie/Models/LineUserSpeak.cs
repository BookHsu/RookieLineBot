using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewLineMessageApi;
using System.Data.SQLite;
using Dapper;
namespace LineBotRookie.Models
{
    public class LineUserSpeak
    {
        
        public LineUserSpeak(LineEvents lineEvent, LineChannel channelObj)
        {
            UserProfile userData = new UserProfile();
            switch (lineEvent.source.type)
            {
                case SourceType.user:
                    userData = channelObj.GetUserProfile(lineEvent.source.userId);
                    break;
                case SourceType.group:
                    userData = channelObj.GetGroupOrRoomUserProfile(lineEvent.source.userId, lineEvent.source.groupId, SourceType.group);
                    this.Groupid = lineEvent.source.groupId;
                    break;
                case SourceType.room:
                    userData = channelObj.GetGroupOrRoomUserProfile(lineEvent.source.userId, lineEvent.source.roomId, SourceType.room);
                    this.Groupid = lineEvent.source.roomId;
                    break;
            }
            this.DisplayName = userData.displayName;
            this.UserId = userData.userId;
            if (lineEvent.message!=null)
            {
                this.MessageType = lineEvent.message.type.ToString();
                
            }
            this.SpeakDate = DateTime.UtcNow.AddHours(8);

        }

        public string GetReportsDefault(string strconn)
        {
            var etime = DateTime.UtcNow.AddHours(8);
            var stime = etime.Date;
            return GetTimeData(strconn, etime, stime);
            
        }

        private string GetTimeData(string strconn, DateTime etime, DateTime stime)
        {
            QueryObj query = new QueryObj()
            {
                etime = etime,
                stime = stime
            };
            string sql = "select * from LineUserSpeak where SpeakDate between @stime and @etime ";
            if (string.IsNullOrWhiteSpace(this.Groupid))
            {
                sql += " and UserId=@Groupid ";
                query.Groupid = this.UserId;
            }
            else
            {
                sql += " and Groupid=@Groupid ";
                query.Groupid = this.Groupid;
            }
            List<LineUserSpeak> datas = null;

            using (var conn = new SQLiteConnection(strconn))
            {
                conn.Open();
                datas = conn.Query<LineUserSpeak>(sql, param: new { stime = query.stime, etime = query.etime, Groupid = query.Groupid }).ToList();
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append($"{datas.FirstOrDefault().SpeakDate.Date.ToString("yyyy-MM-dd")} 共有 {datas.Count} 則對話 \r\n ");
            var speakName = (from c in datas
                             select c.UserId
                                  ).Distinct();
            List<Speaker> speakers = new List<Speaker>();
            foreach (var item in speakName)
            {
                if (!speakers.Any(d => d.Id == item))
                {
                    speakers.Add(new Speaker() { Id = item, Name = datas.FirstOrDefault(d => d.UserId == item).DisplayName, CountData = datas.Where(d => d.UserId == item).Count() });
                }
            }
            
            speakers.OrderByDescending(d => d.CountData).ToList().ForEach(d =>
            {
                sb.Append($"{d.Name}：共{d.CountData}則訊息\r\n");
                                
            });
            return sb.ToString();
        }

        public string GetReportsDefault(string strConn, DateTime value)
        {
            return GetTimeData(strConn, value.AddDays(1).AddSeconds(-1), value);
        }
        public class SpeakerWithType
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
            public MessageType MsgType { get; set; }
        }
        public class Speaker
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int CountData { get; set; }
            
        }
       
        public class QueryObj
        {
            public DateTime etime { get; set; }
            public DateTime stime { get; set; }
            public string Groupid { get; set; }
        }
        public void Save(string strConn)
        {
            using(var conn = new SQLiteConnection(strConn))
            {
                string sql = "Insert into LineUserSpeak (Groupid,DisplayName,UserId,MessageType,SpeakDate) values (@Groupid,@DisplayName,@UserId,@MessageType,@SpeakDate)";
                conn.Open();
                conn.Execute(sql, param: this);
            }       
        }

     

        public LineUserSpeak()
        {
          
        }
       
        public int Id { get; set; }
        public string Groupid { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public string MessageType { get; set; }        
        public DateTime SpeakDate { get; set; }

        
    }
}