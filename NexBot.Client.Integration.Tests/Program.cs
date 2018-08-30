using System;
using System.Text;
using RestSharp;

namespace NexBot.Client.Integration.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            var subject = "Dear Quinn, \n\n"
            + "This is inform you about your member, Oliver(Đặng Hoàng Hải) had a leave request \n\n"
            + "_- Date: 24 Aug, 2018 (0.5 day from 08:00 to 12:00)_ \n\n"
            + "_- Type: Annual Leave_ \n\n"
            + "_- Remark:  Tesing1 _ \n\n"
            + "Please process this request at: http://hrm.nexcel.vn/request/member \n\n"
            + "Any questions, please contact HR department.";

            var botConnector = new BotConnector();

            var result = botConnector.Send(
                subject, "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo");

            //var webClient = new RestClient { Encoding = Encoding.UTF8 };
            //webClient.BaseUrl = new Uri("http://10.18.200.114:8081/api/");

            //var requestUrl = "Request/ProcessRequest?requestId={requestId}&command={command}&nickName={nickName}";
            //var request = new RestRequest(requestUrl, Method.POST);
            //request.AddParameter("requestId", 245810, ParameterType.UrlSegment);
            //request.AddParameter("command", "approve", ParameterType.UrlSegment);
            //request.AddParameter("nickName", "Quinn", ParameterType.UrlSegment);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //var result = webClient.Execute(request);

            //var requestUrl = string.Format("Request/ProcessRequest");
            //var request = new RestRequest(requestUrl, Method.POST);
            //request.AddParameter("requestId", 245810);
            //request.AddParameter("command", "approve");
            //request.AddParameter("nickName", "Quinn");
            //var result = webClient.ExecuteAsPost(request, "POST");

            //var result = webClient.Execute(request);

            System.Console.WriteLine(result);
            System.Console.ReadLine();
        }
    }
}