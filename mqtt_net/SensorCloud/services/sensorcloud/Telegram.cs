using Microsoft.EntityFrameworkCore;
using SensorCloud.services.telegram;
using SensorCloud.util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
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
                var colors = new Color[]
                {
                    Color.Red,
                    Color.Green,
                    Color.Blue,
                    Color.DarkViolet,
                    Color.Salmon,
                    Color.PowderBlue,
                    Color.GreenYellow,
                    Color.DarkOrange
                };
                var colorLookup = new Dictionary<int, Color>();

                if (nodes.Count == 0)
                    return "No nodes";

                var nodesIn = string.Join(",", nodes.Select(n => n.id));
                var between = $"addtime(CURDATE(), '23:59:59') - interval {1+timeOffset} day AND addtime(CURDATE(), '23:59:59') - interval {timeOffset} day";
                var table = "";
                var type = "TEMPERATURE";
                var config = new Graph.Config();

                switch (value)
                {
                    case "temperature":
                        config.min = 0;
                        config.max = 30;
                        type = "TEMPERATURE";
                        break;
                    case "humidity":
                        config.min = 0;
                        config.max = 100;
                        type = "HUMIDITY";
                        break;
                    case "power":
                        config.min = 0;
                        config.max = 1;
                        break;
                }

                switch(timeSpan)
                {
                    case "day":
                        config.markerCount = 24;
                        config.markerMod = 4;
                        config.markerFunc = (e) => $"{e * 4:02}:00";
//                        between = $"addtime(CURDATE(), '23:59:59') - interval {1 + timeOffset} day AND addtime(CURDATE(), '23:59:59') - interval {timeOffset} day";
                        break;
                    case "week":
                        config.markerCount = 7*4;
                        config.markerMod = 4;
                        config.markerFunc = (e) => e < 7 ? ((DayOfWeek)e).ToString() : "";
                        table = ".hourly";
                        between = $"addtime(SUBDATE(CURDATE(), WEEKDAY(CURDATE())), '23:59:59') - interval {timeOffset} week AND addtime(SUBDATE(CURDATE(), WEEKDAY(CURDATE())), '23:59:59') - interval {(timeOffset-1)} week";
                        break;
                    case "month":
                        config.markerCount = 31; //TODO
                        config.markerMod = 7;
                        config.markerFunc = (e) => $"{e*7+1}";
                        table = ".daily";
                        between = $"addtime(SUBDATE(CURDATE(), DAYOFMONTH(CURDATE())), '23:59:59') - interval {timeOffset} month AND addtime(SUBDATE(CURDATE(), DAYOFMONTH(CURDATE())), '23:59:59') - interval {(timeOffset - 1)} month";
                        break;
                }

                string sql = $"SELECT * FROM `sensordata{table}` WHERE `type` = '{type}' AND `stamp` BETWEEN {between} AND `nodeid` IN ({nodesIn})";
                Console.WriteLine(sql);

                var sensorData = db.sensordata.FromSql(sql).OrderBy(d => d.stamp).ToList();
                if (sensorData.Count == 0)
                    return "No data found";

                foreach (var d in sensorData)
                {
                    if (!colorLookup.ContainsKey(d.nodeid))
                    {
                        colorLookup[d.nodeid] = colors[colorLookup.Count];
                        config.values[colorLookup[d.nodeid]] = new List<KeyValuePair<float, float>>(sensorData.Count / nodes.Count);
                    }
                    if (timeSpan == "day")
                        config.values[colorLookup[d.nodeid]].Add(new KeyValuePair<float, float>(d.stamp.Hour + d.stamp.Minute / 60.0f, (float)d.value));
                    if (timeSpan == "week")
                        config.values[colorLookup[d.nodeid]].Add(new KeyValuePair<float, float>(4*((int)d.stamp.DayOfWeek + d.stamp.Hour / 24.0f), (float)d.value));
                    if (timeSpan == "month")
                        config.values[colorLookup[d.nodeid]].Add(new KeyValuePair<float, float>((int)d.stamp.Day-1, (float)d.value));
                }


                var image = Graph.buildImage(config);

                return new Reply()
                {
                    message = timeSpan + " graph for ",
                    image = image
                };
            };
        }
    }
}
