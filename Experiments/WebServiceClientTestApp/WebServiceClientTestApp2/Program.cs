using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServiceClientTestApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ChallengeService2.ChallengeClient();
            client.Open();
            try
            {
                var states = client.login();
                int c = 0;
                foreach (var col in states)
                {
                    int r = 0;
                    Console.WriteLine("column {0}: ", c);
                    foreach (var row in col)
                    {
                        Console.WriteLine("   row {0}: {1}", r, row);
                        r++;
                    }
                    c++;
                }
            }
            finally
            {
                client.Close();
            }
        }
    }
}
