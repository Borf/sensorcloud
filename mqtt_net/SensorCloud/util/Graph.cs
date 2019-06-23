using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.util
{
    public class Graph
    {
        public class Config
        {
            public int height = 400;
            public int width = 800;
            public int markerHeight = 5;
            public int bigMarkerHeight = 8;
            public int marginBottom = 30;
            public int marginLeft = 30;
            public int marginRight = 10;
            public int marginTop = 10;

            public int markerCount = 24;
            public int markerMod = 4;
            public List<string> markers = null;
            public Func<int, string> markerFunc = null;

            public float min = 0;
            public float max = 30;

            public Dictionary<Color, List<KeyValuePair<float, float>>> values = new Dictionary<Color, List<KeyValuePair<float, float>>>();
        }


        public static Bitmap buildImage(Config config)
        {
            var min = Math.Min(config.min, config.values.Min(e => e.Value.Min(ee => ee.Value)));
            var max = Math.Max(config.max, config.values.Min(e => e.Value.Min(ee => ee.Value)));
            float yFactor = (config.height - config.marginTop - config.marginBottom) / (max - min);

            Bitmap image = new Bitmap(config.width, config.height);
            using (var fontBottom = new Font(new FontFamily("Tahoma"), 10))
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.FillRectangle(Brushes.Transparent, 0, 0, config.width, config.height);

                //draw bottom axis details
                var tickWidth = (config.width - config.marginLeft - config.marginRight) / (float)config.markerCount;
                foreach (var e in Enumerable.Range(0, config.markerCount + 1))
                {
                    graphics.DrawLines(e % config.markerMod == 0 ? Pens.Gray : Pens.LightGray,
                            new PointF[] { new PointF(config.marginLeft + tickWidth * e, 0), new PointF(config.marginLeft + tickWidth * e, config.height - config.marginBottom) });

                    graphics.DrawLines(Pens.Black,
                        new PointF[] { new PointF(config.marginLeft + tickWidth * e, config.height - config.marginBottom), new PointF(config.marginLeft + tickWidth * e, config.height - config.marginBottom + (e % config.markerMod == 0 ? config.bigMarkerHeight : config.markerHeight)) });


                    if (e % config.markerMod == 0)
                    {
                        string label = "";
                        if (config.markers != null)
                            label = config.markers[e / config.markerMod];
                        else
                            label = config.markerFunc(e / config.markerMod);
                        float w = graphics.MeasureString(label, fontBottom).Width;
                        graphics.DrawString(label, fontBottom, Brushes.Black, new PointF(config.marginLeft + e * tickWidth - w / 2, config.height - config.marginBottom + 10));
                    }
                }
                //draw left details
                graphics.DrawString(min + "", fontBottom, Brushes.Black, new PointF(0, config.height - config.marginBottom - 10));
                graphics.DrawString(max + "", fontBottom, Brushes.Black, new PointF(0, config.marginTop));

                graphics.DrawLines(Pens.Black, new PointF[] //draw axis
                {
                    new PointF(config.marginLeft, 0),                new PointF(config.marginLeft, config.height-config.marginBottom),
                    new PointF(config.marginLeft, config.height-30), new PointF(config.width, config.height-config.marginBottom)
                });

                //draw graph
                foreach (var item in config.values)
                {
                    PointF lastPoint = new PointF(-1,-1);
                    foreach (var d in item.Value)
                    {
                        PointF point = new PointF(config.marginLeft + tickWidth * d.Key, config.height - config.marginBottom - (float)(d.Value - min) * yFactor);
                        if (lastPoint.X != -1)
                            if(point.X - lastPoint.X < tickWidth * 3)
                                graphics.DrawLines(new Pen(item.Key, 1),
                                    new PointF[]
                                    {
                                        lastPoint,
                                        point
                                    }
                                );
                        lastPoint = point;
                    }
                }



            }
            return image;
        }


    }
}
