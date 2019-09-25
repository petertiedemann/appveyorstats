using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace AppveyorStats
{
    
    public class Project
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public List<Build> Builds { get; set; }

    }

    public class History
    {
        public Project Project { get; set; }
        public List<Build> Builds { get; set; }
    }
    public class Build
    {
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public string AuthorName { get; set; }

        public TimeSpan Elapsed => Finished - Started;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string token = args[1];
            var accountName = args[0];
            
            var client = new RestClient("https://ci.appveyor.com/");

            client.AddDefaultHeader("Authorization", $"Bearer {token}");
            
            var request = new RestRequest("/api/projects", Method.GET);

            var response = client.Execute<List<Project>>(request);

            List<Build> allBuilds = new List<Build>();
            
            foreach (var project in response.Data)
            {
                var historyRequest = new RestRequest($"/api/projects/{accountName}/{project.Slug}/history", Method.GET);

                historyRequest.AddParameter("recordsNumber", 100000);
                
                var history = client.Execute<History>(historyRequest).Data;
                allBuilds.AddRange(history.Builds);
            }

            var timespan = allBuilds.Where(b => b.Started < DateTime.Now - TimeSpan.FromDays(1))
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2.Elapsed);
        }
    }
}