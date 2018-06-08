using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Dapper;
using NewLineMessageApi;
using NewLineMessageApi.TemplatesMsg;
using Newtonsoft.Json;

namespace LineBotRookie.Controllers
{
    public class LineMeController : Models.LineController
    {
        public object CarouselColumn { get; private set; }

        [HttpPost]
        public IHttpActionResult Post()
        {
            LineChannel ChannelObj = null;
            LineEvents LineEvent = null;

            if (ChannelObj == null)
            {
                if (LineChannel.VaridateSignature(Request, Channel_secret))
                {
                    ChannelObj = new LineChannel(Channel_Access_Token);
                    LineEvent = JsonConvert.DeserializeObject<LineReceivedMsg>(Request.Content.ReadAsStringAsync().Result).events.FirstOrDefault();
                }
            }

            if (ChannelObj == null)
            {
                return BadRequest();
            }

            if (LineEvent != null)
            {

                Models.LineUserSpeak dbModel = new Models.LineUserSpeak(LineEvent, ChannelObj);
                switch (LineEvent.type)
                {
                    case EventType.message:
                        
                        if (LineEvent.message.type == MessageType.text)
                        {
                            string strLineMsg = LineEvent.message.text;
                            if (strLineMsg=="$統計")
                            {
                                /*GET DATA*/
                               string msg = dbModel.GetReportsDefault(strConn);
                               ChannelObj.SendReplyMessage(LineEvent.replyToken, new NewLineMessageApi.MessageObj.TextMessage(msg));
                               dbModel = null;

                            }else if (strLineMsg == "$功能" || strLineMsg.ToLower() == "$menu")
                            {
                                NewLineMessageApi.TemplatesMsg.CarouselMessage msgObj = GetCarouselMsg(strConn, LineEvent, ChannelObj);
                                ChannelObj.SendReplyMessage(LineEvent.replyToken,new NewLineMessageApi.MessageObj.TemplateMessage() { altText="我要查資料", template=msgObj });
                                dbModel = null;
                            }
                        }
                        if (dbModel != null)
                        {
                            /*save*/
                            dbModel.Save(strConn);

                        }
                        break;
                    case EventType.follow:
                        break;
                    case EventType.unfollow:
                        break;
                    case EventType.join:
                        switch (LineEvent.source.type)
                        {
                            case SourceType.user:
                                break;
                            case SourceType.group:
                                if (LineEvent.source.groupId != "C0b89d56006a79e4cd6dfeecfb3f06dcd")
                                {
                                    ChannelObj.Leave(LineEvent.source.groupId, SourceType.group);
                                }
                                break;
                            case SourceType.room:
                                ChannelObj.Leave(LineEvent.source.roomId, SourceType.room);
                                break;
                            default:
                                break;
                        }
                        break;
                    case EventType.leave:
                        break;
                    case EventType.postback:
                        if (LineEvent.postback.data== "統計資料日期")
                        {
                            if (LineEvent.postback.Params !=null)
                            {
                                var time = LineEvent.postback.Params.date;
                                if (time.HasValue)
                                {
                                    ChannelObj.SendReplyMessage(LineEvent.replyToken, new NewLineMessageApi.MessageObj.TextMessage(dbModel.GetReportsDefault(strConn,time.Value)));
                                }
                            }
                        }
                        
                        break;
                    case EventType.beacon:
                        break;
                    default:
                        break;
                }
            }
            return Ok();
        }

        private CarouselMessage GetCarouselMsg(string strConn, LineEvents lineEvent, LineChannel channelObj)
        {
            var actions = new NewLineMessageApi.LineActions.DatetimeAction("統計資料日期", DateTimePickerType.date);
            using (var conn = new SQLiteConnection(strConn))
            {
                conn.Open();
                string sql = "select SpeakDate from LineUserSpeak order by SpeakDate desc   ";
                var endDate = conn.QueryFirstOrDefault<DateTime>(sql);
                sql = "select SpeakDate from LineUserSpeak order by SpeakDate  ";
                var beginDate = conn.QueryFirstOrDefault<DateTime>(sql);
                actions.min = beginDate.ToString("yyyy-MM-dd");
                actions.max = endDate.ToString("yyyy-MM-dd");
                actions.label = "要查看的日期";

            }
            var lstAction = new List<NewLineMessageApi.LineActions.LineActionsBase>()
            {
                 actions
            };            
            var col = new NewLineMessageApi.TemplatesMsg.ColumnObj.CarouselColumn("選擇完日期後送出", lstAction);
            
            var lstColumns = new List<NewLineMessageApi.TemplatesMsg.ColumnObj.CarouselColumn>()
            {
                col
            };
            return new CarouselMessage(lstColumns);
        }
    }
}
