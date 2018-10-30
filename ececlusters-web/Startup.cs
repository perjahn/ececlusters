using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ececlusters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ececlusters_web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                string content = await GetClusterTableAsync();
                await context.Response.WriteAsync(content);
            });
        }

        public async Task<string> GetClusterTableAsync()
        {
            Cluster[] clusters = await ClusterInfo.GetClustersAsync();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
                "<tr><th>Cluster name</th>" +
                "<th>Elasticsearch url/Index</th>" +
                "<th class='number'>Indices</th>" +
                "<th class='number'>Documents</th>" +
                "<th class='number'>Size</th>" +
                "<th>Kibana url</th></tr>");

            foreach (var cluster in clusters.OrderBy(c => c.name))
            {
                long clustersize = cluster.indices.Sum(i => i.storesize);
                string prettysizetooltip = GetPrettyTooltip(clustersize);
                if (cluster.kibanaurl != null)
                {
                    sb.AppendLine(
                        $"<tr><td>{cluster.name}</td>" +
                        $"<td><a href='{cluster.url}' target='_blank'>{cluster.url}</a></td>" +
                        $"<td class='number'>{cluster.indices.Count}</td>" +
                        $"<td class='number'>{cluster.indices.Sum(i => i.documentcount)}</td>" +
                        $"<td class='number'{prettysizetooltip}>{clustersize}</td>" +
                        $"<td><a href='{cluster.kibanaurl}' target='_blank'>{cluster.kibanaurl}</a></td></tr>");
                }
                else
                {
                    sb.AppendLine(
                        $"<tr><td>{cluster.name}</td>" +
                        $"<td><a href='{cluster.url}' target='_blank'>{cluster.url}</a></td>" +
                        $"<td class='number'>{cluster.indices.Count}</td>" +
                        $"<td class='number'>{cluster.indices.Sum(i => i.documentcount)}</td>" +
                        $"<td class='number'{prettysizetooltip}>{clustersize}</td></tr>");
                }

                foreach (var index in cluster.compactindices.OrderBy(i => i.name))
                {
                    sb.AppendLine(
                        $"<tr><td></td>" +
                        $"<td>{index.name}</td>" +
                        $"<td class='number'>{index.realindices.Count}</td>" +
                        $"<td class='number'>{index.documentcount}</td>" +
                        $"<td class='number'{GetPrettyTooltip(index.storesize)}>{index.storesize}</td></tr>");
                }
            }

            long storesize = clusters.Sum(c => c.compactindices.Sum(i => i.storesize));
            string storesizeshort = GetPrettySize(storesize);
            if (storesizeshort != string.Empty)
            {
                storesizeshort = $" ({storesizeshort})";
            }

            string content =
                "<html><body>" + Environment.NewLine +
                "<style>" + Environment.NewLine +
                "table { border-collapse: collapse; }" + Environment.NewLine +
                "p { font-family: sans-serif; }" + Environment.NewLine +
                "td, th { font-family: sans-serif; text-align: left; white-space: nowrap; }" + Environment.NewLine +
                "td.number, th.number { text-align: right; }" + Environment.NewLine +
                "</style>" + Environment.NewLine +
                "<table border='1'>" + Environment.NewLine +
                sb.ToString() +
                "</table>" + Environment.NewLine +
                $"<p>Total indices: {clusters.Sum(c => c.indices.Count)}</p>" + Environment.NewLine +
                $"<p>Total documents: {clusters.Sum(c => c.compactindices.Sum(i => i.documentcount))}</p>" + Environment.NewLine +
                $"<p>Total storesize: {storesize}{storesizeshort}</p>" + Environment.NewLine +
                "</html></body>" + Environment.NewLine;

            return content;
        }

        string GetPrettyTooltip(long size)
        {
            string pretty = GetPrettySize(size);
            return pretty == string.Empty ? string.Empty : $" title='{pretty}'";
        }

        string GetPrettySize(long size)
        {
            long kb = 1024;
            long mb = 1024 * 1024;
            long gb = 1024 * 1024 * 1024;
            long tb = (long)1024 * 1024 * 1024 * 1024;
            if (size > tb)
            {
                return (size / (double)tb).ToString("#.0", CultureInfo.InvariantCulture) + " tb";
            }
            else if (size > gb)
            {
                return (size / (double)gb).ToString("#.0", CultureInfo.InvariantCulture) + " gb";
            }
            else if (size > mb)
            {
                return (size / (double)mb).ToString("#.0", CultureInfo.InvariantCulture) + " mb";
            }
            else if (size > kb)
            {
                return (size / (double)kb).ToString("#.0", CultureInfo.InvariantCulture) + " kb";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
