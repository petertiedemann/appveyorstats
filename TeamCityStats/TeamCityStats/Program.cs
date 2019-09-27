using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RestSharp;

namespace TeamCityStats
{

    public class Builds {
        public List<Build> Build { get; set; }
    }

    public class Build
    {
        public string Id { get; set; }
        public string StartDate { get; set; }
        public string FinishDate { get; set; }

        public DateTime Start => Parse( StartDate );
        public DateTime Finish => Parse( FinishDate );

        private DateTime Parse( string date ) {
            return DateTime.ParseExact( date, "yyyyMMddTHHmmss+0000", null );
        }

        public TimeSpan Elapsed => Finish - Start;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string token = "eyJ0eXAiOiAiVENWMiJ9.bnZ3V3VKYy1VZktJRndjS0RqM1pvR3NaUUFN.NzUzNzk1NmMtZTdjYy00N2VhLTk4YjYtNzU4NmMzNjNhODBl";

            var client = new RestClient("http://build.configit.com/");

            client.AddDefaultHeader("Authorization", $"Bearer {token}");
            client.AddDefaultHeader( "Accept", "application/json" );


            var request = new RestRequest("/app/rest/builds?fields=build(id,startDate,finishDate)&count=100000", Method.GET);

            Stopwatch timer = Stopwatch.StartNew();
            var response = client.Execute<Builds>( request );
            Console.WriteLine( $"Request took {timer.Elapsed} to retrieve {response.Data.Build.Count} builds" );

             var buildsLastMonth = response.Data.Build.Where( b => b.Start >= DateTime.UtcNow - TimeSpan.FromDays( 30 ) );

            var timespan = buildsLastMonth.Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2.Elapsed);
            Console.WriteLine( $"Spent {timespan.TotalMinutes} minutes over last 30 days" );
        }
    }
}