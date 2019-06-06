using Microsoft.EntityFrameworkCore;
using SensorCloud.services.telegram;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.services.sensorcloud
{
    public partial class Service
    {
        private Menu sensorDataMenu;
        public override void InstallTelegramHandlers(telegram.Service telegram)
        {//TODO: softcode this
            var projectorMenu = new Menu(title: "Projector");
            new Menu("Projector screen up", async () => await mqtt.Publish("livingroom/RF/7", "up"), projectorMenu);
            new Menu("Projector screen down", async () => await mqtt.Publish("livingroom/RF/7", "down"), projectorMenu);
            new Menu("Projector screen stop", async () => await mqtt.Publish("livingroom/RF/7", "stop"), projectorMenu);
            telegram.AddRootMenu(projectorMenu);



            string timeSpan = "day";
            int timeOffset = 0;
            string value = "temperature";
            List<datamodel.Node> nodes = new List<datamodel.Node>();


            sensorDataMenu = new Menu(title: "Sensordata", afterMenuText: showSensorData(timeSpan, timeOffset, value, nodes));
            {
                var timespanMenu = new Menu(title: "Timespan: day", parent: sensorDataMenu);
                Func<string, Func<Reply>> timespanCallback = (newSpan) =>
                {
                    return () =>
                    {
                        timeSpan = newSpan;
                        timespanMenu.Title = "Timespan: " + newSpan;
                        var reply = showSensorData(timeSpan, timeOffset, value, nodes)();
                        reply.returnAfterClick = true;
                        return reply;
                    };
                };
                new Menu(title: "Day", parent: timespanMenu, callback: timespanCallback("day"));
                new Menu(title: "Week", parent: timespanMenu, callback: timespanCallback("week"));
                new Menu(title: "Month", parent: timespanMenu, callback: timespanCallback("month"));
                new Menu(title: "Year", parent: timespanMenu, callback: timespanCallback("year"));
            }
            {
                var timeOffsetMenu = new Menu(title: "Time offset: 0", parent: sensorDataMenu);
                Func<int, Func<Reply>> timeOffsetCallback = (offset) =>
                {
                    return () =>
                    {
                        timeOffset += offset;
                        if (timeOffset < 0)
                            timeOffset = 0;

                        timeOffsetMenu.Title = "Time offset: " + timeOffset;
                        var reply = showSensorData(timeSpan, timeOffset, value, nodes)();
                        reply.returnAfterClick = true;
                        return reply;
                    };
                };
                new Menu(title: "1 day back", parent: timeOffsetMenu, callback: timeOffsetCallback(1));
                new Menu(title: "1 day forward", parent: timeOffsetMenu, callback: timeOffsetCallback(-1));
                new Menu(title: "1 week back", parent: timeOffsetMenu, callback: timeOffsetCallback(7));
                new Menu(title: "1 week forward", parent: timeOffsetMenu, callback: timeOffsetCallback(-7));
            }

            {
                var valueMenu = new Menu(title: "Value: temperature", parent: sensorDataMenu);
                Func<string, Func<Reply>> valueCallback = (newValue) =>
                {
                    return () =>
                    {
                        value = newValue;
                        valueMenu.Title = "Value: " + newValue;
                        var reply = showSensorData(timeSpan, timeOffset, value, nodes)();
                        reply.returnAfterClick = true;
                        return reply;
                    };
                };
                new Menu(title: "Temperature", parent: valueMenu, callback: valueCallback("temperature"));
                new Menu(title: "Humidity", parent: valueMenu, callback: valueCallback("humidity"));
                new Menu(title: "Power usage", parent: valueMenu, callback: valueCallback("power usage"));
                new Menu(title: "Gas usage", parent: valueMenu, callback: valueCallback("gas usage"));
            }
            {
                Menu nodeMenu = null;

                Func<datamodel.Node, Func<Reply>> nodeCallback = (node) =>
                {
                    return () =>
                    {
                        if (nodes.Contains(node))
                            nodes.Remove(node);
                        else
                            nodes.Add(node);

                        foreach (var m in nodeMenu.SubMenus)
                            if (m.Title.StartsWith(node.name + ": "))
                                m.Title = node.name + ": " + (nodes.Contains(node) ? "on" : "off");


                        nodeMenu.Title = "Nodes: " + nodes.Aggregate("", (n, next) => n + ", " + next.name);
                        var reply = showSensorData(timeSpan, timeOffset, value, nodes)();
                        return reply;
                    };
                };
                nodeMenu = new Menu(title: "Nodes: ...", parent: sensorDataMenu, () =>
                {
                    nodeMenu.Clear();
                    IQueryable<datamodel.Node> dbNodes;
                    if (value == "temperature" || value == "humidity")
                        dbNodes = db.nodes.Where(n => n.sensors.Any(s => s.type == 1 || s.type == 2));
                    else
                        return "No nodes found!";

                    foreach(var n in dbNodes)
                        new Menu(title: n.name + ": " + (nodes.Contains(n) ? "on" : "off"), parent: nodeMenu, callback: nodeCallback(n));


                    return "Select nodes";
                });
            }

            telegram.AddRootMenu(sensorDataMenu);
        }

        private Func<Reply> showSensorData(string timeSpan, int timeOffset, string value, List<datamodel.Node> nodes)
        {
            return () =>
            {
                var nodesIn = string.Join(",", nodes.Select(n => n.id));

                nodesIn = "6";//
                var between = $"addtime(CURDATE(), '23:59:59') - interval {1+timeOffset} day AND addtime(CURDATE(), '23:59:59') - interval {timeOffset} day";
                var table = "hourly";
                var type = "TEMPERATURE";
                string sql = $"SELECT * FROM `sensordata.{table}` WHERE `type` = '{type}' AND `stamp` BETWEEN {between} AND `nodeid` IN ({nodesIn})";
                var sensorData = db.sensordata.FromSql(sql).ToList();
                if (sensorData.Count == 0)
                    return "No data found";
                float max = (float)sensorData.Max(e => e.value);
                float min = (float)sensorData.Min(e => e.value);

                if (value == "temperature")
                {
                    min = Math.Min(0, min);
                    max = Math.Max(30, max);
                }


                int height = 200,
                    width = 400,
                    markerHeight = 5,
                    bigMarkerHeight = 8,
                    marginBottom = 30,
                    marginLeft = 30,
                    marginRight = 10,
                    marginTop = 10,
                    markerMod = 4;

                float yFactor = (height - marginTop - marginBottom) / (max - min);

                Image<Rgba32> image = new Image<Rgba32>(width, height);
                image.Mutate(ctx => ctx
                    .Fill(Rgba32.Transparent)
//                    .DrawText(timeSpan, SystemFonts.CreateFont("Arial", 39), Rgba32.Black, new PointF(40, 40))
                    .DrawLines(new Pen<Rgba32>(Rgba32.Black, 2), new PointF[]
                        {
                            new PointF(30, 0),          new PointF(30, height-30),
                            new PointF(30, height-30),  new PointF(width, height-30)
                        })
                    );
                if(timeSpan == "day")
                {
                    FontCollection fontCollection = new FontCollection();
                    fontCollection.Install("tahoma.ttf");
                    var fontbottom = fontCollection.CreateFont("Tahoma", 10);
                    var tickWidth = (width - marginLeft - marginRight) / 24f;
                    foreach (var e in Enumerable.Range(0, 25))
                    {
                        image.Mutate(ctx => ctx
                            .DrawLines(new Pen<Rgba32>(Rgba32.Black, 1),
                                new PointF[] { new PointF(marginLeft + tickWidth * e, height - marginBottom), new PointF(marginLeft + tickWidth * e, height - marginBottom + (e% markerMod == 0 ? bigMarkerHeight : markerHeight)) })
                        );

                        if (e % markerMod == 0)
                        {
                            float w = TextMeasurer.Measure(e + ":00", new RendererOptions(fontbottom)).Width;
                            image.Mutate(ctx => ctx.DrawText(e + ":00", fontbottom, Rgba32.Black, new PointF(marginLeft + e * tickWidth - w/2, height - marginBottom + 10)));
                        }
                    }
                    image.Mutate(ctx => ctx
                        .DrawLines(new Pen<Rgba32>(Rgba32.Black, 1),
                            sensorData.Select(d => new PointF(marginLeft + tickWidth * (d.stamp.Hour + d.stamp.Minute / 60.0f), height - marginBottom - (float)(d.value-min) * yFactor)).ToArray())
                        );
                    image.Mutate(ctx => ctx.DrawText(min + "", fontbottom, Rgba32.Black, new PointF(0, height - marginBottom - 10)));
                    image.Mutate(ctx => ctx.DrawText(max + "", fontbottom, Rgba32.Black, new PointF(0, marginTop)));



                }

                return new Reply()
                {
                    message = timeSpan + " graph for ",
                    image = image
                };
            };
        }
    }
}
