using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ececlusters
{
    public class Index
    {
        public string name { get; set; }
        public long compactindexcount { get; set; }
        public long documentcount { get; set; }
    }

    public class Cluster
    {
        public string name { get; set; }
        public string url { get; set; }
        public List<Index> indices { get; set; } = new List<Index>();
        public List<Index> compactindices { get; set; } = new List<Index>();
        public string errormessage { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            PrintClustersAndIndices();
        }

        private static void PrintClustersAndIndices()
        {
            string configfile = "appsettings.json";
            if (!File.Exists(configfile))
            {
                throw new FileNotFoundException(configfile);
            }

            dynamic settings = JObject.Parse(File.ReadAllText(configfile));

            string url = $"{settings.ecebaseurl}/api/v1/clusters/elasticsearch";
            string username = settings.eceusername;
            string password = settings.ecepassword;
            var creds = Encoding.ASCII.GetBytes($"{username}:{password}");

            string result;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(creds));

                result = client.GetStringAsync(url).Result;
            }

            File.WriteAllText("eceresult.json", result);

            dynamic jobject = JObject.Parse(result);
            JArray ececlusters = jobject.elasticsearch_clusters;

            Log($"Got {ececlusters.Count} clusters.");

            string domain = settings.clusterdomain;

            var allclustercredentials = (JArray)settings.clustercredentials;

            List<Cluster> clusters = new List<Cluster>();

            foreach (dynamic ececluster in ececlusters)
            {
                string clusterid = ececluster.cluster_id;
                string clustername = ececluster.cluster_name;

                var clustercredentials = allclustercredentials.Where(c => ((dynamic)c).name == ececluster.cluster_name).ToArray();

                if (clustercredentials.Length == 0)
                {
                    clusters.Add(
                        new Cluster
                        {
                            name = clustername,
                            url = $"https://{clusterid}{domain}:9243"
                        });
                }
                else
                {
                    string clusterusername = ((dynamic)clustercredentials[0]).username;
                    string clusterpassword = ((dynamic)clustercredentials[0]).password;

                    clusters.Add(GetCluster(clusterid, domain, clustername, clusterusername, clusterpassword));
                }
            }

            foreach (var cluster in clusters)
            {
                foreach (var indexgroup in cluster.indices.GroupBy(i => GetShortIndexName(i.name)))
                {
                    cluster.compactindices.Add(
                        new Index
                        {
                            name = indexgroup.Key,
                            compactindexcount = indexgroup.Count(),
                            documentcount = indexgroup.Sum(i => i.documentcount)
                        });
                }
            }

            foreach (var cluster in clusters.OrderBy(c => c.name))
            {
                Log($"'{cluster.name}' {cluster.url} {cluster.indices.Count}");
                foreach (var index in cluster.compactindices.OrderBy(i => i.name))
                {
                    Log($"  {index.name} {index.compactindexcount} {index.documentcount}");
                }
            }

            //if (cluster.associated_kibana_clusters != null && cluster.associated_kibana_clusters.Count > 0 && cluster.associated_kibana_clusters[0].kibana_id != null)            
        }

        private static string GetShortIndexName(string name)
        {
            int length = name.Length;
            if (length > 17 &&
                char.IsDigit(name[length - 1]) && char.IsDigit(name[length - 2]) && char.IsDigit(name[length - 3]) && char.IsDigit(name[length - 4]) && char.IsDigit(name[length - 5]) && char.IsDigit(name[length - 6]) && name[length - 7] == '-' &&
                char.IsDigit(name[length - 8]) && char.IsDigit(name[length - 9]) && name[length - 10] == '.' &&
                char.IsDigit(name[length - 11]) && char.IsDigit(name[length - 12]) && name[length - 13] == '.' &&
                char.IsDigit(name[length - 14]) && char.IsDigit(name[length - 15]) && char.IsDigit(name[length - 16]) && char.IsDigit(name[length - 17]))
            {
                return name.Substring(0, length - 17) + "*";
            }
            else if (length > 14 &&
                char.IsDigit(name[length - 1]) && char.IsDigit(name[length - 2]) && char.IsDigit(name[length - 3]) && char.IsDigit(name[length - 4]) && char.IsDigit(name[length - 5]) && char.IsDigit(name[length - 6]) && name[length - 7] == '-' &&
                char.IsDigit(name[length - 8]) && char.IsDigit(name[length - 9]) && name[length - 10] == '.' &&
                char.IsDigit(name[length - 11]) && char.IsDigit(name[length - 12]) && char.IsDigit(name[length - 13]) && char.IsDigit(name[length - 14]))
            {
                return name.Substring(0, length - 14) + "*";
            }
            else if (length > 10 &&
                char.IsDigit(name[length - 1]) && char.IsDigit(name[length - 2]) && name[length - 3] == '.' &&
                char.IsDigit(name[length - 4]) && char.IsDigit(name[length - 5]) && name[length - 6] == '.' &&
                char.IsDigit(name[length - 7]) && char.IsDigit(name[length - 8]) && char.IsDigit(name[length - 9]) && char.IsDigit(name[length - 10]))
            {
                return name.Substring(0, length - 10) + "*";
            }
            else if (length > 7 &&
                char.IsDigit(name[length - 1]) && char.IsDigit(name[length - 2]) && name[length - 3] == '.' &&
                char.IsDigit(name[length - 4]) && char.IsDigit(name[length - 5]) && char.IsDigit(name[length - 6]) && char.IsDigit(name[length - 7]))
            {
                return name.Substring(0, length - 7) + "*";
            }
            else
            {
                return name;
            }
        }

        private static Cluster GetCluster(string clusterid, string domain, string clustername, string username, string password)
        {
            var cluster = new Cluster()
            {
                name = clustername,
                url = $"https://{clusterid}{domain}:9243"
            };

            var creds = Encoding.ASCII.GetBytes($"{username}:{password}");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(creds));

                string url = $"https://{clusterid}{domain}:9243/_cat/indices";
                string result;

                try
                {
                    result = client.GetStringAsync(url).Result;
                }
                catch (System.Exception ex)
                {
                    cluster.errormessage = ex.Message;
                    return cluster;
                }

                File.WriteAllText($"eceresult_{clusterid}.json", result);

                JArray indices = JArray.Parse(result);
                foreach (dynamic index in indices)
                {
                    string docscount = index["docs.count"];
                    if (!long.TryParse(docscount, out long docs))
                    {
                        docs = 0;
                    }

                    cluster.indices.Add(new Index()
                    {
                        name = index.index,
                        documentcount = docs
                    });
                }
            }

            return cluster;
        }

        private static void Log(string message)
        {
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            Console.WriteLine($"{now}: {message}");
        }

    }
}
