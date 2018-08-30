using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Connector;
using RestSharp;

namespace NexBot.Client.Integration.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            //            var subject = "Dear Quinn, \n\n"
            //+ "This is inform you about your member, Oliver(Đặng Hoàng Hải) had a leave request \n\n"
            //+ "_- Date: 24 Aug, 2018 (0.5 day from 08:00 to 12:00)_ \n\n"
            //+ "_- Type: Annual Leave_ \n\n"
            //+ "_- Remark:  Tesing1 _ \n\n"
            //+ "Please process this request at: http://hrm.nexcel.vn/request/member \n\n"
            //            + "Any questions, please contact HR department.";

            //            var botConnector = new BotConnector();

            //            var result1 = botConnector.Send(
            //                subject, "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo");

            var webClient = new RestClient { Encoding = Encoding.UTF8 };
            webClient.BaseUrl = new Uri("http://10.18.200.114:8081/api/");
            var requestUrl = string.Format("Request/ProcessRequest?requestId={0}&command={1}&nickName={2}", 245810, "approve", "Quinn");

            var request = new RestRequest(requestUrl, Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");

            //request.AddParameter("requestId", "245810", ParameterType.UrlSegment);
            //request.AddParameter("command", "approve", ParameterType.UrlSegment);
            //request.AddParameter("nickName", "Quinn", ParameterType.UrlSegment);

            var result = webClient.Execute(request);


            //webClient.BaseUrl = new Uri("http://10.18.200.114:8081/api/");

            //var request = new RestRequest("Request/GetData", Method.GET);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");

            //var result = webClient.Execute(request);

            System.Console.WriteLine(result);
            System.Console.ReadLine();
        }
    }
}