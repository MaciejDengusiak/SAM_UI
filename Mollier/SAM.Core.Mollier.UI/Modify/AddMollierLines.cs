﻿using SAM.Geometry.Planar;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using System;
using System.Drawing;

namespace SAM.Core.Mollier.UI
{
    public static partial class Modify
    {
        public static List<Series> AddMollierLines(this Chart chart, IEnumerable<UIMollierCurve> uIMollierCurves, MollierControlSettings mollierControlSettings)
        {
            if(uIMollierCurves == null)
            {
                return null;
            }

            List<Series> result = new List<Series>();

            ChartType chartType = mollierControlSettings.ChartType;


            foreach (UIMollierCurve uIMollierCurve in uIMollierCurves)
            {            
                MollierLine mollierLine = uIMollierCurve?.MollierCurve as MollierLine;
                if(mollierLine == null)
                {
                    continue;
                }

                MollierSensibleHeatRatioLine mollierSensibleHeatRatioLine = mollierLine as MollierSensibleHeatRatioLine;
                if(mollierSensibleHeatRatioLine == null)
                {
                    continue;
                }

                Color color = uIMollierCurve.UIMollierAppearance.Color == Color.Empty ? Color.Gray : uIMollierCurve.UIMollierAppearance.Color;

                Series series = chart.Series.Add("Mollier Sensible Heat Ratio Line " + Guid.NewGuid().ToString());
                series.IsVisibleInLegend = false;
                series.ChartType = SeriesChartType.Line;
                series.Color = color;
                series.BorderDashStyle = ChartDashStyle.Dash;
                series.BorderWidth = mollierControlSettings.ProccessLineThickness != -1 ? mollierControlSettings.ProccessLineThickness : 3;
                series.Tag = uIMollierCurve;

                double sensibleHeatRatio = mollierSensibleHeatRatioLine.SensibleHeatRatio; 

                MollierPoint mollierPoint = mollierSensibleHeatRatioLine.MollierPoints[0];

                double sensibleLoad = 1;

                double latentLoad = sensibleLoad * ((1 - sensibleHeatRatio) / sensibleHeatRatio);

                RoomProcess roomProcess_Temp = Mollier.Create.RoomProcess_ByEnd(mollierPoint, 1, sensibleLoad  * 1000, latentLoad * 1000);

                Point2D point_1 = Convert.ToSAM(roomProcess_Temp.Start, chartType);
                Point2D point_2 = Convert.ToSAM(roomProcess_Temp.End, chartType);

                Line2D line2D = new Line2D(point_1, point_2);

                ChartArea chartArea = chart.ChartAreas[series.ChartArea];

                BoundingBox2D boundingBox2D = new BoundingBox2D(new Point2D(chartArea.AxisX.Minimum, chartArea.AxisY.Minimum), new Point2D(chartArea.AxisX.Maximum, chartArea.AxisY.Maximum));

                List<Point2D> point2Ds = boundingBox2D.Intersections(line2D);
                if (point2Ds != null)
                {
                    foreach(Point2D point2D in point2Ds)
                    {
                        series.Points.AddXY(point2D.X, point2D.Y);
                    }
                }

                result.Add(series);
            }

            return result;
        }

        public static List<Series> AddMollierLines(this Chart chart, MollierModel mollierModel, MollierControlSettings mollierControlSettings)
        {
            if (mollierModel == null)
            {
                return null;
            }

            List<UIMollierCurve> uIMollierCurves = mollierModel.GetMollierObjects<UIMollierCurve>();

            uIMollierCurves = uIMollierCurves?.FindAll(x => x.UIMollierAppearance.Visible == true);
            return chart.AddMollierLines(uIMollierCurves, mollierControlSettings);
        }
    }
}