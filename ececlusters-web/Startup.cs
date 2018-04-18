﻿using System;
using System.Collections.Generic;
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
                string content = GetClusterTable();
                await context.Response.WriteAsync(content);
            });
        }

        public string GetClusterTable()
        {
            List<Cluster> clusters = ClusterInfo.GetClusters();

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
                if (cluster.kibanaurl != null)
                {
                    sb.AppendLine(
                        $"<tr><td>{cluster.name}</td>" +
                        $"<td><a href='{cluster.url}' target='_blank'>{cluster.url}</a></td>" +
                        $"<td class='number'>{cluster.indices.Count}</td>" +
                        $"<td class='number'>{cluster.indices.Sum(i => i.documentcount)}</td>" +
                        $"<td class='number'>{cluster.indices.Sum(i => i.storesize)}</td>" +
                        $"<td><a href='{cluster.kibanaurl}' target='_blank'>{cluster.kibanaurl}</a></td></tr>");
                }
                else
                {
                    sb.AppendLine(
                        $"<tr><td>{cluster.name}</td>" +
                        $"<td><a href='{cluster.url}' target='_blank'>{cluster.url}</a></td>" +
                        $"<td class='number'>{cluster.indices.Count}</td>" +
                        $"<td class='number'>{cluster.indices.Sum(i => i.documentcount)}</td>" +
                        $"<td class='number'>{cluster.indices.Sum(i => i.storesize)}</td></tr>");
                }

                foreach (var index in cluster.compactindices.OrderBy(i => i.name))
                {
                    sb.AppendLine(
                        $"<tr><td></td>" +
                        $"<td>{index.name}</td>" +
                        $"<td class='number'>{index.realindices.Count}</td>" +
                        $"<td class='number'>{index.documentcount}</td>" +
                        $"<td class='number'>{index.storesize}</td></tr>");
                }
            }

            string content =
                "<html><body>" + Environment.NewLine +
                "<style>" + Environment.NewLine +
                "table { border-collapse: collapse; }" + Environment.NewLine +
                "td, th { font-family: sans-serif; text-align: left; }" + Environment.NewLine +
                "td.number, th.number { text-align: right; }" + Environment.NewLine +
                "</style>" + Environment.NewLine +
                "<table border='1'>" + Environment.NewLine +
                sb.ToString() +
                "</table></html></body>" + Environment.NewLine;

            return content;
        }
    }
}
