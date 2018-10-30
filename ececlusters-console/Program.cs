using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ececlusters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ececlusters
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await PrintClustersAndIndicesAsync();
        }

        static async Task PrintClustersAndIndicesAsync()
        {
            Cluster[] clusters = await ClusterInfo.GetClustersAsync();

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

            long storesize = clusters.Sum(c => c.compactindices.Sum(i => i.storesize));
            string storesizeshort = GetPrettySize(storesize);

            Log($"Total indices: {clusters.Sum(c => c.indices.Count)}");
            Log($"Total documents: {clusters.Sum(c => c.compactindices.Sum(i => i.documentcount))}");
            Log($"Total storesize: {storesize}{storesizeshort}");
        }

        static string GetPrettySize(long size)
        {
            long kb = 1024;
            long mb = 1024 * 1024;
            long gb = 1024 * 1024 * 1024;
            long tb = (long)1024 * 1024 * 1024 * 1024;
            if (size > tb)
            {
                return " (" + (size / (double)tb).ToString("#.0", CultureInfo.InvariantCulture) + " tb)";
            }
            else if (size > gb)
            {
                return " (" + (size / (double)gb).ToString("#.0", CultureInfo.InvariantCulture) + " gb)";
            }
            else if (size > mb)
            {
                return " (" + (size / (double)mb).ToString("#.0", CultureInfo.InvariantCulture) + " mb)";
            }
            else if (size > kb)
            {
                return " (" + (size / (double)kb).ToString("#.0", CultureInfo.InvariantCulture) + " kb)";
            }
            else
            {
                return string.Empty;
            }
        }

        private static void Log(string message)
        {
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            Console.WriteLine($"{now}: {message}");
        }
    }
}
