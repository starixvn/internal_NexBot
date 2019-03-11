namespace NexBot.Client.Integration.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var subject = "Good morning!";

            var botConnector = new BotConnector();

            var result = botConnector.Send(
                subject, "29:1FHnlKj9e2Pnz9eb5NgXGBkemLvJ9L-fqB8x3LKMLpso");

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