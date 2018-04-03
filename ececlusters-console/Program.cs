using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using ececlusters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ececlusters
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintClustersAndIndices();
        }

        private static void PrintClustersAndIndices()
        {
            List<Cluster> clusters = ClusterInfo.GetClusters();

            foreach (var cluster in clusters.OrderBy(c => c.name))
            {
                if (cluster.kibanaurl != null)
                {
                    Log($"'{cluster.name}' {cluster.url} {cluster.indices.Count} {cluster.indices.Sum(i => i.documentcount)} {cluster.kibanaurl}");
                }
                else
                {
                    Log($"'{cluster.name}' {cluster.url} {cluster.indices.Count} {cluster.indices.Sum(i => i.documentcount)}");
                }

                foreach (var index in cluster.compactindices.OrderBy(i => i.name))
                {
                    Log($"  {index.name} {index.realindices.Count} {index.documentcount}");
                }
            }
        }

        private static void Log(string message)
        {
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            Console.WriteLine($"{now}: {message}");
        }

    }
}
