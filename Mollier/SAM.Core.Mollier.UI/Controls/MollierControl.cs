﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SAM.Geometry.Planar;

namespace SAM.Core.Mollier.UI.Controls
{
    public partial class MollierControl : UserControl
    {
        public event MollierPointSelectedEventHandler MollierPointSelected;

        public static double MaxPressure = 400000;
        public static double MinPressure = 35000;
        
        private Point mdown = Point.Empty;
        private bool selection = false;
        private MollierControlSettings mollierControlSettings;
        private List<UIMollierPoint> mollierPoints;
        private List<UIMollierProcess> mollierProcesses;
        private List<MollierZone> mollierZones;
        private List<List<UIMollierProcess>> systems;
        private List<Tuple<MollierPoint, string>> created_points;

        private List<Tuple<Series, int>> seriesData = new List<Tuple<Series, int>>();

        public MollierControl()
        {
            InitializeComponent();

            mollierControlSettings = new MollierControlSettings();


        }

        private void create_moved_label(ChartType chartType, double X, double Y, int Mollier_angle, int Psychrometric_angle, double Mollier_X, double Mollier_Y, double Psychrometric_X, double Psychrometric_Y, string LabelContent, ChartDataType chartDataType, ChartParameterType chartParameterType, bool IsDisabled = false, bool fontChange = false, Color? color = null, string tag = null)
        {
            if (IsDisabled)
            {
                return;
            }
            double x = chartType == ChartType.Mollier ? Mollier_X : Psychrometric_X;
            double y = chartType == ChartType.Mollier ? Mollier_Y : Psychrometric_Y;
            //X, Y - coordinates of the point before moveing by x and y

            Series new_label = MollierChart.Series.Add(String.Format(LabelContent + chartDataType.ToString() + Guid.NewGuid().ToString()));
            new_label.IsVisibleInLegend = false;
            new_label.ChartType = SeriesChartType.Spline;
            if (tag == "ColorPointLabel")//in save as pdf we want to move this label(colorpointlabel) so it has to be point not spline
            {
                new_label.ChartType = SeriesChartType.Point;
            }
            new_label.Color = Color.Transparent;
            new_label.SmartLabelStyle.Enabled = false;
            new_label.Points.AddXY(X + x, Y + y);
            new_label.Label = LabelContent;
            new_label.LabelAngle = chartType == ChartType.Mollier ? Mollier_angle % 90 : Psychrometric_angle % 90;
            new_label.LabelForeColor = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.DefaultTemplateName, chartParameterType, chartDataType);
            if (color != null)
            {
                new_label.LabelForeColor = (Color)color;
            }
            if (fontChange)
            {
                new_label.Font = SystemFonts.MenuFont;
            }
            if (tag != null)
            {
                new_label.Tag = tag;
            }
        }

        public void CreateYAxis(Chart chart, ChartArea area, Series series, float axisX, float axisWidth, float labelsSize, bool alignLeft, double P_w_Min, double P_w_Max)
        {

            chart.ApplyPaletteColors();  // (*)
            long x = DateTime.Now.Ticks;

            // Create new chart area for original series
            ChartArea areaSeries = new ChartArea();
            if (MollierChart.ChartAreas.Count != 3)
            {
                areaSeries = chart.ChartAreas.Add("Psychrometric_P_w" + x.ToString());
            }
            else
            {
                areaSeries = chart.ChartAreas[1];
                areaSeries.Name = "Psychrometric_P_w" + x.ToString();
            }
            //ChartArea areaSeries = chart.ChartAreas.Add("Psychrometric_P_w" + x.ToString());
            areaSeries.BackColor = Color.Transparent;
            areaSeries.BorderColor = Color.Transparent;
            areaSeries.Position.FromRectangleF(area.Position.ToRectangleF());
            areaSeries.InnerPlotPosition.FromRectangleF(area.InnerPlotPosition.ToRectangleF());
            areaSeries.AxisX.MajorGrid.Enabled = false;
            areaSeries.AxisX.MajorTickMark.Enabled = false;
            areaSeries.AxisX.LabelStyle.Enabled = false;
            areaSeries.AxisY.MajorGrid.Enabled = false;
            areaSeries.AxisY.MajorTickMark.Enabled = false;
            areaSeries.AxisY.LabelStyle.Enabled = false;
            areaSeries.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
            // associate series with new ca
            series.ChartArea = areaSeries.Name;

            // Create new chart area for axis
            ChartArea areaAxis = new ChartArea();
            if (MollierChart.ChartAreas.Count != 3)
            {
                areaAxis = chart.ChartAreas.Add("Psychrometric_P_w_copy" + x.ToString());
            }
            else
            {
                areaAxis = chart.ChartAreas[2];
                areaAxis.Name = "Psychrometric_P_w_copy" + x.ToString();
            }
            //ChartArea areaAxis = chart.ChartAreas.Add("Psychrometric_P_w_copy" + x.ToString());

            areaAxis.BackColor = Color.Transparent;
            areaAxis.BorderColor = Color.Transparent;
            RectangleF oRect = area.Position.ToRectangleF();
            areaAxis.Position = new ElementPosition(oRect.X, oRect.Y, axisWidth, oRect.Height);
            areaAxis.InnerPlotPosition.FromRectangleF(areaSeries.InnerPlotPosition.ToRectangleF());

            // Create a copy of specified series
            Series seriesCopy = chart.Series.Add("Psychrometric_P_w_copy" + x.ToString());
            seriesCopy.ChartType = series.ChartType;
            seriesCopy.YAxisType = alignLeft ? AxisType.Primary : AxisType.Secondary;  // (**)


            foreach (DataPoint point in series.Points)
            {
                seriesCopy.Points.AddXY(point.XValue, point.YValues[0]);
            }
            // Hide copied series
            seriesCopy.IsVisibleInLegend = false;
            seriesCopy.Color = Color.Transparent;
            seriesCopy.BorderColor = Color.Transparent;
            seriesCopy.ChartArea = areaAxis.Name;

            // Disable grid lines & tickmarks
            areaAxis.AxisX.LineWidth = 0;
            areaAxis.AxisY.LineWidth = 1;
            areaAxis.AxisX.MajorGrid.Enabled = false;
            areaAxis.AxisY.MajorGrid.Enabled = true;
            areaAxis.AxisX.MajorTickMark.Enabled = false;
            areaAxis.AxisY.MajorTickMark.Enabled = true;
            areaAxis.AxisX.LabelStyle.Enabled = false;
            areaAxis.AxisY.LabelStyle.Enabled = true;

            Axis areaAxisAxisY = alignLeft ? areaAxis.AxisY : areaAxis.AxisY2;   // (**)
            areaAxisAxisY.MajorGrid.Enabled = false;
            areaAxisAxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
            areaAxisAxisY.LabelStyle.Font = area.AxisY.LabelStyle.Font;
            areaAxisAxisY.Minimum = System.Math.Round(P_w_Min, 4);
            areaAxisAxisY.Maximum = System.Math.Round(P_w_Max, 4);
            areaAxisAxisY.Interval = mollierControlSettings.P_w_Interval;
            areaAxisAxisY.Title = "Partial Vapour Pressure pW [kPa]";

            areaAxis.AxisX2.Title = "";

            // Adjust area position
            areaAxis.Position.X = axisX;
            areaAxis.InnerPlotPosition.X += labelsSize;

            areaAxisAxisY.MinorTickMark.Enabled = false;
            areaAxisAxisY.MinorGrid.Enabled = false;
            areaAxisAxisY.MinorTickMark.Interval = 0.1;
        }

        public void CreateXAxis(Chart chart, ChartArea area, Series series, float axisY, float axisHeight, float labelsSize, bool alignLeft, double P_w_Min, double P_w_Max)
        {
            long x = DateTime.Now.Ticks;

            chart.ApplyPaletteColors();  // (*)
            // Create new chart area for original series
            ChartArea areaSeries = new ChartArea();
            if (MollierChart.ChartAreas.Count != 3)
            {
                areaSeries = chart.ChartAreas.Add("Mollier P_w" + x.ToString());
            }
            else
            {
                areaSeries = chart.ChartAreas[1];
                areaSeries.Name = "Mollier P_w" + x.ToString();
            }
            //areaSeries = chart.ChartAreas.Add("Mollier P_w" + x.ToString());
            areaSeries.BackColor = Color.Transparent;
            areaSeries.BorderColor = Color.Transparent;
            areaSeries.Position.FromRectangleF(area.Position.ToRectangleF());
            areaSeries.InnerPlotPosition.FromRectangleF(area.InnerPlotPosition.ToRectangleF());
            areaSeries.AxisY.MajorGrid.Enabled = false;
            areaSeries.AxisY.MajorTickMark.Enabled = false;
            areaSeries.AxisY.LabelStyle.Enabled = false;
            areaSeries.AxisX.MajorGrid.Enabled = false;
            areaSeries.AxisX.MajorTickMark.Enabled = false;
            areaSeries.AxisX.LabelStyle.Enabled = false;
            areaSeries.AxisX.IsStartedFromZero = area.AxisX.IsStartedFromZero;
            
            // associate series with new ca
            series.ChartArea = areaSeries.Name;

            // Create new chart area for axis
            ChartArea areaAxis = new ChartArea();
            if (MollierChart.ChartAreas.Count != 3)
            {
                areaAxis = chart.ChartAreas.Add("Mollier P_w_copy" + x.ToString());
            }
            else
            {
                areaAxis = chart.ChartAreas[2];
                areaAxis.Name = "Mollier P_w_Copy" + x.ToString();
            }
            // areaAxis = chart.ChartAreas.Add("Mollier P_w_copy" + x.ToString());
            areaAxis.BackColor = Color.Transparent;
            areaAxis.BorderColor = Color.Transparent;
            RectangleF oRect = area.Position.ToRectangleF();
            areaAxis.Position = new ElementPosition(oRect.X, oRect.Y, oRect.Width, axisHeight);
            areaAxis.InnerPlotPosition.FromRectangleF(areaSeries.InnerPlotPosition.ToRectangleF());

            // Create a copy of specified series
            Series seriesCopy = chart.Series.Add("Mollier P_w_copy" + x.ToString());
            seriesCopy.ChartType = series.ChartType;
            seriesCopy.XAxisType = alignLeft ? AxisType.Primary : AxisType.Secondary;  // (**)

            foreach (DataPoint point in series.Points)
            {
                seriesCopy.Points.AddXY(point.XValue, point.YValues[0]);
            }
            // Hide copied series
            seriesCopy.IsVisibleInLegend = false;
            seriesCopy.Color = Color.Transparent;
            seriesCopy.BorderColor = Color.Transparent;
            seriesCopy.ChartArea = areaAxis.Name;

            // Disable grid lines & tickmarks
            areaAxis.AxisY.LineWidth = 0;
            areaAxis.AxisX.LineWidth = 1;
            areaAxis.AxisY.MajorGrid.Enabled = false;
            areaAxis.AxisX.MajorGrid.Enabled = true;
            areaAxis.AxisY.MajorTickMark.Enabled = false;
            areaAxis.AxisX.MajorTickMark.Enabled = true;
            areaAxis.AxisY.LabelStyle.Enabled = false;
            areaAxis.AxisX.LabelStyle.Enabled = true;

            Axis areaAxisAxisX = alignLeft ? areaAxis.AxisX : areaAxis.AxisX2;   // (**)
            areaAxisAxisX.MajorGrid.Enabled = false;
            areaAxisAxisX.Minimum = System.Math.Round(P_w_Min, 2);
            areaAxisAxisX.Maximum = System.Math.Round(P_w_Max, 2);
            areaAxisAxisX.LabelStyle.Font = area.AxisX.LabelStyle.Font;
            areaAxisAxisX.Interval = mollierControlSettings.P_w_Interval;

            areaAxis.AxisY.Title = "";

            areaAxisAxisX.Title = series.Name;
            //areaAxisAxisX.LineColor = series.Color;    // (*)
            //areaAxisAxisX.TitleForeColor = Color.DarkCyan;  // (*)

            // Adjust area position
            areaAxis.Position.Y = axisY;
            areaAxis.InnerPlotPosition.Y += labelsSize;

            areaAxisAxisX.MinorTickMark.Enabled = true;
            areaAxisAxisX.MinorGrid.Enabled = false;
            areaAxisAxisX.MinorTickMark.Interval = 0.1;
        }


        private void addDivisionArea(ChartType chartType)
        {

            int deltaRelativeHumidity = 10;//RH interval from neighborhoodcount
            int deltaEnthalpy = 3;//enthalpy interval from neighborhoodcount

            //base size
            int RH_size = 100 / deltaRelativeHumidity + 7;
            int Ent_size = 200 / deltaEnthalpy + 7;

            List<MollierPoint>[,] rectangles_points = new List<MollierPoint>[RH_size, Ent_size];//for every rh interval and every enthalpy interval it stores the list of points that belong to this area 
            double maxCount;
            Query.NeighborhoodCount(mollierPoints?.ConvertAll(x => x.MollierPoint), out maxCount, out rectangles_points);

            for (int rh = 0; rh <= 100 - deltaRelativeHumidity; rh += 10)
            {
                for (int e = -39; e <= 140 - deltaEnthalpy; e += 3)
                {
                    int index_1 = rh / deltaRelativeHumidity;
                    int index_2 = e / deltaEnthalpy + 15;
                    if (rectangles_points[index_1, index_2] == null)
                    {
                        continue;
                    }

                    Series series = MollierChart.Series.Add(Guid.NewGuid().ToString());
                    series.IsVisibleInLegend = false;
                    series.Tag = "GradientZone";
                    double pressure = mollierControlSettings.Pressure;
                    series.Points.AddXY(Query.FindDivisionAreaCornerPoints(rh, e, "X", chartType, pressure), Query.FindDivisionAreaCornerPoints(rh, e, "Y", chartType, pressure));//first corner                
                    series.Points.AddXY(Query.FindDivisionAreaCornerPoints(rh, e + deltaEnthalpy, "X", chartType, pressure), Query.FindDivisionAreaCornerPoints(rh, e + deltaEnthalpy, "Y", chartType, pressure));//second corner               
                    series.Points.AddXY(Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity, e + deltaEnthalpy, "X", chartType, pressure), Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity, e + deltaEnthalpy, "Y", chartType, pressure));//third corner
                    series.Points.AddXY(Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity, e, "X", chartType, pressure), Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity, e, "Y", chartType, pressure));//fourth corner
                    series.Points.AddXY(Query.FindDivisionAreaCornerPoints(rh, e, "X", chartType, pressure), Query.FindDivisionAreaCornerPoints(rh, e, "Y", chartType, pressure));//first corner again to close the zone

                    double value = maxCount == 0 ? 0 : System.Convert.ToDouble(System.Convert.ToInt32(System.Math.Log(rectangles_points[index_1, index_2].Count))) / maxCount;
                    series.Color = Core.Query.Lerp(Color.Red, Color.Blue, value);
                    series.ChartType = SeriesChartType.Line;
                    series.BorderWidth = 3;
                    if (mollierControlSettings.DivisionAreaLabels)
                    {
                        Series label = MollierChart.Series.Add(Guid.NewGuid().ToString());
                        label.IsVisibleInLegend = false;
                        label.ChartType = SeriesChartType.Point;
                        if (MollierControlSettings.ChartType == ChartType.Mollier)
                        {
                            label.Points.AddXY(Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity / 2, e + deltaEnthalpy / 2, "X", chartType, pressure), Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity / 2, e + deltaEnthalpy / 2, "Y", chartType, pressure) - 0.5);
                        }
                        else
                        {
                            label.Points.AddXY(Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity / 2, e + deltaEnthalpy / 2, "X", chartType, pressure), Query.FindDivisionAreaCornerPoints(rh + deltaRelativeHumidity / 2, e + deltaEnthalpy / 2, "Y", chartType, pressure));
                        }
                        label.Color = Color.Transparent;
                        label.Label = rectangles_points[index_1, index_2].Count.ToString();
                        label.Tag = "GradientZoneLabel";

                    }
                }
            }
        }
        private void addMollierZones(ChartType chartType)
        {
            foreach (MollierZone mollierZone in mollierZones)
            {
                Series series = MollierChart.Series.Add(Guid.NewGuid().ToString());
                series.IsVisibleInLegend = false;
                series.ChartType = SeriesChartType.Line;
                series.BorderWidth = 2;
                series.Color = Color.Blue;
                string zoneText = "";
                if (mollierZone is MollierControlZone)
                {
                    MollierControlZone mollierControlZone = (MollierControlZone)mollierZone;
                    series.Color = mollierControlZone.Color;
                    zoneText = mollierControlZone.Text;
                }

                List<MollierPoint> mollierPoints = mollierZone.MollierPoints;

                int size = mollierPoints.Count;
                for (int i = 0; i < size; i++)
                {
                    MollierPoint point = mollierPoints[i];
                    if (chartType == ChartType.Mollier)
                    {
                        series.Points.AddXY(point.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(point));
                    }
                    else
                    {
                        series.Points.AddXY(point.DryBulbTemperature, point.HumidityRatio);
                    }
                }
                MollierPoint label = mollierZone.GetCenter();
                double labelX = chartType == ChartType.Mollier ? label.HumidityRatio * 1000 : label.DryBulbTemperature;
                double labelY = chartType == ChartType.Mollier ? Mollier.Query.DiagramTemperature(label) : label.HumidityRatio;
                create_moved_label(chartType, labelX, labelY, 0, 0, 0, 0, 0, 0, zoneText, ChartDataType.Undefined, ChartParameterType.Point, color: Color.Black);
            }
        }
        private void addMollierPoints(ChartType chartType)
        {
            Series series = MollierChart.Series.ToList()?.Find(x => x.Name == "MollierPoints");
            int index_Temp = -1;
            if(series == null)
            {
                series = MollierChart.Series.Add("MollierPoints");
                series.IsVisibleInLegend = false;
                series.ChartType = SeriesChartType.Point;
                index_Temp = series.Points.AddXY(1, 0); //Has to bed added to properly show first point on HumidityRatio = 0
                series.Points[index_Temp].MarkerSize = 0;
            }
            else
            {
                series.Points.Clear();
            }

            series.Tag = mollierPoints;

            Dictionary<MollierPoint, int> dictionary = new Dictionary<MollierPoint, int>();
            double maxCount = 0;
            List<MollierPoint>[,] rectangles_points;
            PointGradientVisibilitySetting pointGradientVisibilitySetting = mollierControlSettings.VisibilitySettings.GetVisibilitySetting("User", ChartParameterType.Point) as PointGradientVisibilitySetting;

            if (pointGradientVisibilitySetting != null)
            {
                dictionary = Query.NeighborhoodCount(mollierPoints.ConvertAll(x => x.MollierPoint), out maxCount, out rectangles_points);
            }
            else
            {
                series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.DefaultTemplateName, ChartParameterType.Point, ChartDataType.Undefined);
            }

            //add points to the chart
            foreach (UIMollierPoint uIMollierPoint in mollierPoints)
            {
                MollierPoint mollierPoint = uIMollierPoint?.MollierPoint;
                if(mollierPoint == null)
                {
                    continue;
                }

                double humidityRatio = mollierPoint.HumidityRatio;
                double dryBulbTemperature = mollierPoint.DryBulbTemperature;
                double diagramTemperature = double.NaN;

                if(chartType == ChartType.Mollier)
                {
                    //diagramTemperature = dryBulbTemperature;

                    diagramTemperature = Mollier.Query.DiagramTemperature(mollierPoint);
                    if(mollierPoint.SaturationHumidityRatio() < humidityRatio)
                    {
                        if(Mollier.Query.TryFindDiagramTemperature(mollierPoint, out double diagramTemperature_Temp))
                        {
                            diagramTemperature = diagramTemperature_Temp;
                        }
                    }
                }

                double x = chartType == ChartType.Mollier ? humidityRatio * 1000 : dryBulbTemperature;
                double y = chartType == ChartType.Mollier ? diagramTemperature : humidityRatio;

                if(series.Contains(x, y, Tolerance.MacroDistance))
                {
                    continue;
                }

                int index = series.Points.AddXY(x, y);

                //if gradient point is on then set a gradient point color with earlier counted intensity
                if (pointGradientVisibilitySetting != null)
                {
                    double value = maxCount == 0 ? 0 : System.Convert.ToDouble(dictionary[mollierPoint]) / maxCount;
                    series.Points[index].Color = Core.Query.Lerp(pointGradientVisibilitySetting.Color, pointGradientVisibilitySetting.GradientColor, value);
                }
                series.Points[index].ToolTip = Query.ToolTipText(mollierPoint, chartType, null);
                series.Points[index].Tag = mollierPoint;
                series.Points[index].MarkerSize = 7; //TODO: Change size of marker
                series.Points[index].MarkerStyle = MarkerStyle.Circle;

                if (uIMollierPoint.UIMollierAppearance != null)
                {
                    if(uIMollierPoint.UIMollierAppearance.Color != Color.Empty)
                    {
                        series.Points[index].Color = uIMollierPoint.UIMollierAppearance.Color;
                    }

                    if(!string.IsNullOrWhiteSpace(uIMollierPoint.UIMollierAppearance.Label))
                    {
                        series.Points[index].Label = uIMollierPoint.UIMollierAppearance.Label;
                    }
                }
            }

            //if(index_Temp != -1)
            //{
            //    series.Points.RemoveAt(index_Temp);
            //}
        }
        private void add_MollierProcesses(ChartType chartType)
        {
            created_points = new List<Tuple<MollierPoint, string>>();//used for labeling in label process new 2

            List<UIMollierProcess> uIMollierProcesses_Temp = mollierProcesses == null ? null : new List<UIMollierProcess>(mollierProcesses.Cast<UIMollierProcess>());
            uIMollierProcesses_Temp?.Sort((x, y) => System.Math.Max(y.Start.HumidityRatio, y.End.HumidityRatio).CompareTo(System.Math.Max(x.Start.HumidityRatio, x.End.HumidityRatio)));
            createProcessesLabels(uIMollierProcesses_Temp, chartType);

            //create series for all points and lines in processes(create circles, lines, tooltips, ADP etc.)
            for (int i = 0; i < mollierProcesses.Count; i++)
            {
                UIMollierProcess uIMollierProcess = mollierProcesses[i];//contains all spcified data of the process like color, start label etc.
                MollierProcess mollierProcess = uIMollierProcess.MollierProcess;//contains the most important data of the process: only start end point, and what type of process is it 

                if (mollierProcess is UndefinedProcess)
                {
                    createSeries_RoomProcess(uIMollierProcess);
                    continue;
                }
                //process series
                Series series = MollierChart.Series.Add(Guid.NewGuid().ToString());
                series.IsVisibleInLegend = false;
                series.ChartType = SeriesChartType.Line;
                series.BorderWidth = 4;
                series.Color = (uIMollierProcess.UIMollierAppearance.Color == Color.Empty) ? mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.DefaultTemplateName, ChartParameterType.Line, mollierProcess) : uIMollierProcess.UIMollierAppearance.Color;
                series.Tag = mollierProcess;

                MollierPoint start = mollierProcess?.Start;
                MollierPoint end = mollierProcess?.End;
                if (start == null || end == null)
                {
                    continue;
                }
                //creating series - processes points pattern
                createSeries_ProcessesPoints(start, uIMollierProcess, chartType, toolTipName: uIMollierProcess.UIMollierAppearance_Start.Label);
                createSeries_ProcessesPoints(end, uIMollierProcess, chartType, toolTipName: uIMollierProcess.UIMollierAppearance_End.Label);
                //add start and end point to the process series
                int index;
                series.ToolTip = Query.ToolTipText(start, end, chartType, Query.FullProcessName(uIMollierProcess));
                index = chartType == ChartType.Mollier ? series.Points.AddXY(start.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(start)) : series.Points.AddXY(start.DryBulbTemperature, start.HumidityRatio);
                series.Points[index].Tag = start;
                index = chartType == ChartType.Mollier ? series.Points.AddXY(end.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(end)) : series.Points.AddXY(end.DryBulbTemperature, end.HumidityRatio);
                series.Points[index].Tag = end;


                //cooling process create one unique process with ADP point
                if (mollierProcess is CoolingProcess)
                {
                    CoolingProcess coolingProcess = (CoolingProcess)mollierProcess;
                    if (start.HumidityRatio == end.HumidityRatio)
                    {
                        continue;
                    }
                    MollierPoint apparatusDewPoint = coolingProcess.ApparatusDewPoint();
                    double X = chartType == ChartType.Mollier ? apparatusDewPoint.HumidityRatio * 1000 : apparatusDewPoint.DryBulbTemperature;
                    double Y = chartType == ChartType.Mollier ? Mollier.Query.DiagramTemperature(apparatusDewPoint) : apparatusDewPoint.HumidityRatio;
                    create_moved_label(chartType, X, Y, 0, 0, 0, -3 * Query.ScaleVector2D(this, MollierControlSettings).Y, -1 * Query.ScaleVector2D(this, MollierControlSettings).X, -0.0007 * Query.ScaleVector2D(this, MollierControlSettings).Y, "ADP", ChartDataType.Undefined, ChartParameterType.Point, color: Color.Gray);

                    MollierPoint ADPPoint_Temp = chartType == ChartType.Mollier ? new MollierPoint(apparatusDewPoint.DryBulbTemperature - 3 * Query.ScaleVector2D(this, MollierControlSettings).Y, apparatusDewPoint.HumidityRatio, mollierControlSettings.Pressure) : new MollierPoint(apparatusDewPoint.DryBulbTemperature - 1 * Query.ScaleVector2D(this, MollierControlSettings).X, apparatusDewPoint.HumidityRatio - 0.0007 * Query.ScaleVector2D(this, MollierControlSettings).Y, mollierControlSettings.Pressure);
                    created_points.Add(new Tuple<MollierPoint, string>(ADPPoint_Temp, "ADP"));

                    MollierPoint secondPoint = coolingProcess.DewPoint();
                    //creating series - processes points pattern
                    createSeries_ProcessesPoints(apparatusDewPoint, uIMollierProcess, chartType, toolTipName: "Dew Point", pointType: "DewPoint");
                    createSeries_ProcessesPoints(secondPoint, uIMollierProcess, chartType, pointType: "SecondPoint");
                    //creating series - special with ADP process pattern
                    createSeries_DewPoint(start, secondPoint, mollierProcess, chartType, Color.LightGray, 2, ChartDashStyle.Dash);
                    createSeries_DewPoint(end, apparatusDewPoint, mollierProcess, chartType, Color.LightGray, 2, ChartDashStyle.Dash);
                    createSeries_DewPoint(end, secondPoint, mollierProcess, chartType, Color.LightGray, 2, ChartDashStyle.Dash);


                    //Additional Lines 2023.06.06
                    List<MollierPoint> mollierPoints = Mollier.Query.ProcessMollierPoints(coolingProcess);
                    if (mollierPoints != null && mollierPoints.Count > 1)
                    {
                        for (int j = 0; j < mollierPoints.Count - 1; j++)
                        {
                            createSeries_DewPoint(mollierPoints[j], mollierPoints[j + 1], mollierProcess, chartType, Color.Gray, 3, ChartDashStyle.Solid);
                        }
                    }
                }

            }
            //labeling all the processes
            createPorcessesLabels_New(uIMollierProcesses_Temp, chartType);
        }

        private void createSeries_DewPoint(MollierPoint mollierPoint_1, MollierPoint mollierPoint_2, IMollierProcess mollierProcess, ChartType chartType, Color color, int borderWidth, ChartDashStyle borderDashStyle)
        {
            Series series = MollierChart.Series.Add(Guid.NewGuid().ToString());
            if (chartType == ChartType.Mollier)
            {
                series.Points.AddXY(mollierPoint_1.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(mollierPoint_1));
                series.Points.AddXY(mollierPoint_2.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(mollierPoint_2));
            }
            else
            {
                series.Points.AddXY(mollierPoint_1.DryBulbTemperature, mollierPoint_1.HumidityRatio);
                series.Points.AddXY(mollierPoint_2.DryBulbTemperature, mollierPoint_2.HumidityRatio);
            }
            series.Color = color;
            series.IsVisibleInLegend = false;
            series.BorderWidth = borderWidth;
            series.ChartType = SeriesChartType.Spline;
            series.BorderDashStyle = borderDashStyle;
            series.Tag = "dashLine";
        }

        private void createSeries_ProcessesPoints(MollierPoint mollierPoint, UIMollierProcess UI_MollierProcess, ChartType chartType, string toolTipName = null, string pointType = "Default")
        {
            Series series = MollierChart.Series.Add(Guid.NewGuid().ToString());
            int index = chartType == ChartType.Mollier ? series.Points.AddXY(mollierPoint.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(mollierPoint)) : series.Points.AddXY(mollierPoint.DryBulbTemperature, mollierPoint.HumidityRatio);
            switch (pointType)
            {
                case "Default":
                    series.MarkerSize = 8;
                    series.MarkerBorderWidth = 2;
                    series.MarkerBorderColor = Color.Orange;
                    break;
                case "DewPoint":
                    series.MarkerSize = 8;
                    break;
                case "SecondPoint":
                    series.MarkerSize = 5;
                    break;
            }
            series.Color = Color.Gray;
            series.IsVisibleInLegend = false;
            series.ChartType = SeriesChartType.Point;
            series.Tag = UI_MollierProcess.MollierProcess;
            if (pointType == "SecondPoint")
            {
                series.Tag = "SecondPoint";
            }
            series.MarkerStyle = MarkerStyle.Circle;
            series.Points[index].ToolTip = Query.ToolTipText(mollierPoint, chartType, toolTipName);
            //seriesDew.mark
        }
        private void createSeries_RoomProcess(UIMollierProcess uIMollierProcess)
        {
            //defines the end label of the process
            MollierProcess mollierProcess = uIMollierProcess.MollierProcess;
            //specified the color of the Room air condition point
            Color color = uIMollierProcess.UIMollierAppearance.Color == Color.Empty ? Color.Gray : uIMollierProcess.UIMollierAppearance.Color;
            //creating series for room process
            Series series = MollierChart.Series.Add(Guid.NewGuid().ToString());
            series.IsVisibleInLegend = false;
            series.ChartType = SeriesChartType.Line;
            series.Color = color;
            series.BorderDashStyle = ChartDashStyle.Dash;
            series.BorderWidth = 3;
            series.Tag = mollierProcess;
            //add start and end point to the process series
            MollierPoint start = mollierProcess.Start;
            MollierPoint end = mollierProcess.End;
            int index;
            index = mollierControlSettings.ChartType == ChartType.Mollier ? series.Points.AddXY(start.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(start)) : series.Points.AddXY(start.DryBulbTemperature, start.HumidityRatio);
            series.Points[index].Tag = start;
            index = mollierControlSettings.ChartType == ChartType.Mollier ? series.Points.AddXY(end.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(end)) : series.Points.AddXY(end.DryBulbTemperature, end.HumidityRatio);
            series.Points[index].Tag = end;
            series.ToolTip = Query.ToolTipText(start, end, mollierControlSettings.ChartType, Query.FullProcessName(uIMollierProcess));

            //creating series for Room air condition point 
            Series seriesRoomPoint = MollierChart.Series.Add(Guid.NewGuid().ToString());
            seriesRoomPoint.IsVisibleInLegend = false;
            seriesRoomPoint.ChartType = SeriesChartType.Point;
            seriesRoomPoint.Color = Color.Gray;
            seriesRoomPoint.MarkerStyle = MarkerStyle.Triangle;
            seriesRoomPoint.MarkerBorderWidth = 2;
            seriesRoomPoint.MarkerBorderColor = color;
            seriesRoomPoint.MarkerSize = 8;
            seriesRoomPoint.Tag = mollierProcess;
            //add Room air condition point to the series and create label for it
            double X = mollierControlSettings.ChartType == ChartType.Mollier ? end.HumidityRatio * 1000 : end.DryBulbTemperature;
            double Y = mollierControlSettings.ChartType == ChartType.Mollier ? Mollier.Query.DiagramTemperature(end) : end.HumidityRatio;
            seriesRoomPoint.Points.AddXY(X, Y);
            seriesRoomPoint.Points[0].ToolTip = Query.ToolTipText(end, mollierControlSettings.ChartType, "ROOM");
            if (!string.IsNullOrWhiteSpace(uIMollierProcess?.UIMollierAppearance_Start?.Label))
            {
                createSeries_ProcessesPoints(start, uIMollierProcess, MollierControlSettings.ChartType);
            }

        }

        private void createProcessesLabels(List<UIMollierProcess> mollierProcesses, ChartType chartType)
        {
            if (systems == null)
            {
                return;
            }
            //Item1 - MollierPoint, Item2 - factor X, Item3 - factor Y
            List<Tuple<MollierPoint, double, double>> tuples = new List<Tuple<MollierPoint, double, double>>();
            char name = 'A';
            for (int i = 0; i < systems.Count; i++)
            {
                for (int j = 0; j < systems[i].Count; j++)
                {
                    UIMollierProcess UI_MollierProcess = systems[i][j];
                    MollierProcess mollierProcess = UI_MollierProcess.MollierProcess;
                    if (UI_MollierProcess.UIMollierAppearance_End?.Label == "SUP")
                    {
                        UI_MollierProcess.UIMollierAppearance_End.Label = null;
                    }


                    if (UI_MollierProcess.UIMollierAppearance_Start.Label == null && systems[i].Count == 1)
                    {
                        UI_MollierProcess.UIMollierAppearance_Start.Label = name + "1";
                    }
                    else if (UI_MollierProcess.UIMollierAppearance_Start.Label == null && j == 0)
                    {
                        UI_MollierProcess.UIMollierAppearance_Start.Label = "OSA";
                    }
                    if (UI_MollierProcess.UIMollierAppearance_End.Label == null && systems[i].Count > 1 && j == systems[i].Count - 2 && systems[i][j + 1].MollierProcess is UndefinedProcess)
                    {
                        UI_MollierProcess.UIMollierAppearance_End.Label = "SUP";
                    }
                    else if (UI_MollierProcess.UIMollierAppearance_End.Label == null && systems[i].Count > 1 && j == systems[i].Count - 1)
                    {
                        UI_MollierProcess.UIMollierAppearance_End.Label = "SUP";
                    }
                    UI_MollierProcess.UIMollierAppearance.Label = UI_MollierProcess.UIMollierAppearance.Label == null ? Query.ProcessName(mollierProcess) : UI_MollierProcess.UIMollierAppearance.Label;
                    UI_MollierProcess.UIMollierAppearance_End.Label = UI_MollierProcess.UIMollierAppearance_End.Label == null ? name + "2" : UI_MollierProcess.UIMollierAppearance_End.Label;

                    name++;
                }
            }

            this.mollierProcesses = mollierProcesses;//used only to remember point name to show it in tooltip
        }
        private void createPorcessesLabels_New(List<UIMollierProcess> mollierProcesses, ChartType chartType)//creates sorted list of points that has to be labaled
        {
            List<Tuple<MollierPoint, string>> points_list = new List<Tuple<MollierPoint, string>>();

            foreach (UIMollierProcess UI_MollierProcess in mollierProcesses)
            {
                if (!string.IsNullOrWhiteSpace(UI_MollierProcess?.UIMollierAppearance_Start?.Label))
                {
                    points_list.Add(new Tuple<MollierPoint, string>(UI_MollierProcess.Start, UI_MollierProcess.UIMollierAppearance_Start.Label));
                }
                if (UI_MollierProcess.UIMollierAppearance_End.Label != null && UI_MollierProcess.UIMollierAppearance_End.Label != "")
                {
                    points_list.Add(new Tuple<MollierPoint, string>(UI_MollierProcess.End, UI_MollierProcess.UIMollierAppearance_End.Label));
                }

            }
            points_list?.Sort((x, y) => x.Item1.HumidityRatio.CompareTo(y.Item1.HumidityRatio));
            foreach (UIMollierProcess UI_MollierProcess in mollierProcesses)
            {
                if (UI_MollierProcess.UIMollierAppearance.Label != null && UI_MollierProcess.UIMollierAppearance.Label != "")
                {
                    double dryBulbTemperature = (UI_MollierProcess.Start.DryBulbTemperature + UI_MollierProcess.End.DryBulbTemperature) / 2;
                    double humdityRatio = (UI_MollierProcess.Start.HumidityRatio + UI_MollierProcess.End.HumidityRatio) / 2;
                    MollierPoint mid = new MollierPoint(dryBulbTemperature, humdityRatio, mollierControlSettings.Pressure);
                    points_list.Add(new Tuple<MollierPoint, string>(mid, UI_MollierProcess.UIMollierAppearance.Label));
                }
            }
            createPorcessesLabels_New_2(points_list);
        }

        private void createPorcessesLabels_New_2(List<Tuple<MollierPoint, string>> points_list)
        {
            if (mollierControlSettings.ChartType == ChartType.Mollier)
            {
                for (int i = 0; i < points_list.Count; i++)
                {
                    MollierPoint mollierPoint = points_list[i].Item1;
                    string label = points_list[i].Item2;
                    Vector2D vector2D = Query.ScaleVector2D(this, MollierControlSettings);
                    //1st option right
                    bool is_space = true;
                    //how much move the label 
                    double moveHumidityRatio = (0.25 + 0.2 * label.Length / 2.0) * vector2D.X;
                    double moveTemperature = -1.4 * vector2D.Y;
                    //mollier point moved  riBght, top, left, down
                    MollierPoint mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio / 1000, mollierControlSettings.Pressure);
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                        continue;
                    }
                    else
                    {
                        is_space = true;
                        moveHumidityRatio = 0;
                        moveTemperature = 0;
                        mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio / 1000, mollierControlSettings.Pressure);
                    }

                    //2nd option top
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                        continue;
                    }
                    else
                    {
                        is_space = true;
                        moveHumidityRatio = -(0.25 + 0.2 * label.Length / 2.0) * vector2D.X;
                        moveTemperature = -1.4 * vector2D.Y;
                        mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio / 1000, mollierControlSettings.Pressure);
                    }
                    //3rd option left
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                        continue;
                    }
                    else
                    {
                        is_space = true;
                        moveHumidityRatio = 0;
                        moveTemperature = -2.5 * vector2D.Y;
                        mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio / 1000, mollierControlSettings.Pressure);
                    }
                    //4th option down   
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                    }
                }
            }
            else
            {
                for (int i = 0; i < points_list.Count; i++)
                {
                    MollierPoint mollierPoint = points_list[i].Item1;
                    string label = points_list[i].Item2;
                    Vector2D vector2D = Query.ScaleVector2D(this, MollierControlSettings);
                    //1st option top
                    bool is_space = true;
                    //how much move the label 
                    double moveHumidityRatio = 0;
                    double moveTemperature = 0;
                    //mollier point moved  right, top, left, down
                    MollierPoint mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio, mollierControlSettings.Pressure);
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                        continue;
                    }
                    else
                    {
                        is_space = true;
                        moveHumidityRatio = -0.0007 * vector2D.Y;
                        moveTemperature = (0.5 + 0.4 * label.Length / 2.0) * vector2D.X;
                        mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio, mollierControlSettings.Pressure);
                    }

                    //2nd option right
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                        continue;
                    }
                    else
                    {
                        is_space = true;
                        moveHumidityRatio = -0.0015 * vector2D.Y;
                        moveTemperature = 0;
                        mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio, mollierControlSettings.Pressure);
                    }
                    //3rd option down
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                        continue;
                    }
                    else
                    {
                        is_space = true;
                        moveHumidityRatio = -0.0007 * vector2D.Y;
                        moveTemperature = -(0.5 + 0.4 * label.Length / 2.0) * vector2D.X;
                        mollierPoint_Moved = new MollierPoint(mollierPoint.DryBulbTemperature + moveTemperature, mollierPoint.HumidityRatio + moveHumidityRatio, mollierControlSettings.Pressure);
                    }
                    //4th option left  
                    for (int j = 0; j < created_points.Count; j++)
                    {
                        if (overlaps(mollierPoint_Moved, created_points[j], label))
                        {
                            is_space = false;
                            break;
                        }
                    }
                    if (is_space == true && !intersect(mollierPoint_Moved, label, mollierProcesses))//we're creating because there is a space
                    {
                        created_points.Add(new Tuple<MollierPoint, string>(mollierPoint_Moved, label));
                    }
                }
            }

            for (int i = 0; i < created_points.Count; i++)
            {
                if (created_points[i].Item2 == "ADP")
                {
                    continue;
                }
                double Y = mollierControlSettings.ChartType == ChartType.Mollier ? Mollier.Query.DiagramTemperature(created_points[i].Item1) : created_points[i].Item1.HumidityRatio;
                double X = mollierControlSettings.ChartType == ChartType.Mollier ? created_points[i].Item1.HumidityRatio * 1000 : created_points[i].Item1.DryBulbTemperature;

                create_moved_label(mollierControlSettings.ChartType, X, Y, 0, 0, 0, 0, 0, 0, created_points[i].Item2, ChartDataType.Undefined, ChartParameterType.Point, color: Color.Black);
            }
        }

        private bool overlaps(MollierPoint mollierPointNew, Tuple<MollierPoint, string> mollierPointLabeled, string label)
        {
            ChartType chartType = mollierControlSettings.ChartType;
            double NewPoint_X = chartType == ChartType.Mollier ? mollierPointNew.HumidityRatio * 1000 : mollierPointNew.DryBulbTemperature;
            double NewPoint_Y = chartType == ChartType.Mollier ? mollierPointNew.DryBulbTemperature : mollierPointNew.HumidityRatio;
            double OldPoint_X = chartType == ChartType.Mollier ? mollierPointLabeled.Item1.HumidityRatio * 1000 : mollierPointLabeled.Item1.DryBulbTemperature;
            double OldPoint_Y = chartType == ChartType.Mollier ? mollierPointLabeled.Item1.DryBulbTemperature : mollierPointLabeled.Item1.HumidityRatio;
            string OldLabel = mollierPointLabeled.Item2;
            Vector2D vector2D = Query.ScaleVector2D(this, MollierControlSettings);
            if (chartType == ChartType.Mollier)
            {
                double y = 0.95 * vector2D.Y;
                double x = 0.2 * vector2D.X;// 0.2 is one letter width in mollier
                x *= label.Length;

                Point2D origin = new Point2D(NewPoint_X - x / 2.0, NewPoint_Y + y);
                Rectangle2D rectangle2Dnew = new Rectangle2D(origin, x, y);


                x = 0.2 * vector2D.X;
                x *= OldLabel.Length;
                origin = new Point2D(OldPoint_X - x / 2.0, OldPoint_Y + y);
                Rectangle2D rectangle2Dold = new Rectangle2D(origin, x, y);

                return rectangle2Dnew.Intersect(rectangle2Dold, 0.001);
            }
            else
            {
                double y = 0.00049 * vector2D.Y;
                double x = 0.49 * vector2D.X;// 0.25 is one letter width in psychro
                x *= label.Length;

                Point2D origin = new Point2D(NewPoint_X - x / 2.0, NewPoint_Y + y);
                Rectangle2D rectangle2Dnew = new Rectangle2D(origin, x, y);


                x = 0.49 * vector2D.X;
                x *= OldLabel.Length;
                origin = new Point2D(OldPoint_X - x / 2.0, OldPoint_Y + y);
                Rectangle2D rectangle2Dold = new Rectangle2D(origin, x, y);

                return rectangle2Dnew.Intersect(rectangle2Dold, 0.0000001);
            }
            return false;
        }

        private bool intersect(MollierPoint mollierPointNew, string label, List<UIMollierProcess> mollierProcesses)
        {
            ChartType chartType = mollierControlSettings.ChartType;
            double NewPoint_X = chartType == ChartType.Mollier ? mollierPointNew.HumidityRatio * 1000 : mollierPointNew.DryBulbTemperature;
            double NewPoint_Y = chartType == ChartType.Mollier ? mollierPointNew.DryBulbTemperature : mollierPointNew.HumidityRatio;
            Vector2D vector2D = Query.ScaleVector2D(this, MollierControlSettings);
            double y = 0.95 * vector2D.Y;
            double x = 0.2 * vector2D.X;// 0.2 is one letter width in mollier
            x *= label.Length;
            if (chartType == ChartType.Psychrometric)
            {
                y = 0.00048 * vector2D.Y;
                x = 0.48 * vector2D.X;// 0.25 is one letter width in psychro
                x *= label.Length;
            }
            Point2D origin = new Point2D(NewPoint_X - x / 2.0, NewPoint_Y + y);

            Rectangle2D rectangle2Dnew = new Rectangle2D(origin, x, y);

            for (int i = 0; i < mollierProcesses.Count; i++)
            {
                IMollierProcess mollierProcess = mollierProcesses[i].MollierProcess;
                Point2D start = chartType == ChartType.Mollier ? new Point2D(mollierProcess.Start.HumidityRatio * 1000, mollierProcess.Start.DryBulbTemperature) : new Point2D(mollierProcess.Start.DryBulbTemperature, mollierProcess.Start.HumidityRatio);
                Point2D end = chartType == ChartType.Mollier ? new Point2D(mollierProcess.End.HumidityRatio * 1000, mollierProcess.End.DryBulbTemperature) : new Point2D(mollierProcess.End.DryBulbTemperature, mollierProcess.End.HumidityRatio);

                Segment2D segment2D = new Segment2D(start, end);

                if (rectangle2Dnew.Intersect(segment2D, Tolerance.MicroDistance))
                {
                    return true;
                }
            }
            return false;
        }


        public void GenerateGraph()
        {
            if (mollierControlSettings == null)
            {
                return;
            }

            if (mollierControlSettings.ChartType == ChartType.Mollier)
            {
                generateGraph_Mollier();
            }
            else if (mollierControlSettings.ChartType == ChartType.Psychrometric)
            {
                generateGraph_Psychrometric();
            }
        }

        private void generateGraph_Mollier()
        {

            if(mollierControlSettings == null || !mollierControlSettings.IsValid())
            {
                mollierControlSettings = new MollierControlSettings();
            }

            MollierChart.ChartAreas[0].AxisY2.MinorTickMark.Enabled = false;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;

            MollierChart.ChartAreas[0].AxisY.MinorTickMark.Enabled = false;
            MollierChart.ChartAreas[0].AxisY.MinorGrid.Enabled = false;

            //INITIAL SIZES
            double pressure = mollierControlSettings.Pressure;
            double humidityRatio_Min = mollierControlSettings.HumidityRatio_Min;
            double humidityRatio_Max = mollierControlSettings.HumidityRatio_Max;
            double humidityRatio_interval = mollierControlSettings.HumidityRatio_Interval;
            double temperature_Min = mollierControlSettings.Temperature_Min;
            double temperature_Max = mollierControlSettings.Temperature_Max;
            double temperature_interval = mollierControlSettings.Temperature_Interval;

            ChartType chartType = mollierControlSettings.ChartType;

            if (MinPressure > pressure || pressure > MaxPressure)
            {
                return;
            }

            //BASE CHART INITIALIZATION
            MollierChart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            MollierChart.Series.Clear();
            ChartArea chartArea = MollierChart.ChartAreas[0];
            ChartArea chartArea_New = MollierChart.ChartAreas["ChartArea1"];
            chartArea_New.Position = new ElementPosition(2, 2, 95, 95);//define sizes of chart
            chartArea_New.InnerPlotPosition = new ElementPosition(7, 6, 88, 88);
            double partialVapourPressure_max = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Max / 1000, temperature_Max, pressure) / 1000;
            double partialVapourPressure_min = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Min / 1000, temperature_Min, pressure) / 1000;

            //AXIS X
            Axis axisX = chartArea.AxisX;
            axisX.Title = "Humidity Ratio  x [g/kg]";
            axisX.Maximum = humidityRatio_Max;
            axisX.Minimum = humidityRatio_Min;
            axisX.Interval = humidityRatio_interval;
            axisX.MajorGrid.LineColor = Color.Gray;
            axisX.MinorGrid.Interval = 1;
            axisX.MinorGrid.Enabled = true;
            axisX.MinorGrid.LineColor = Color.LightGray;
            axisX.IsReversed = false;
            axisX.LabelStyle.Format = "0.##";
            axisX.LabelStyle.Font = chartArea_New.AxisY.LabelStyle.Font;
            //areaAxisAxisY.LabelStyle.Font = area.AxisY.LabelStyle.Font;
            
            //AXIS Y
            MollierChart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.False;
            Axis axisY = chartArea.AxisY;
            axisY.MinorTickMark.Enabled = false;
            axisY.MinorGrid.Enabled = false;
            axisY.Enabled = AxisEnabled.True;
            axisY.Title = "Dry Bulb Temperature t [°C]";
            axisY.TextOrientation = TextOrientation.Rotated270;
            axisY.Maximum = temperature_Max;
            axisY.Minimum = temperature_Min;
            axisY.Interval = temperature_interval;
            axisY.LabelStyle.Format = "0.##";
            axisY.LabelStyle.Font = chartArea_New.AxisY.LabelStyle.Font;
            axisY.MajorTickMark.Enabled = false;
            //axisY.MinorTickMark.Enabled = false;

            List<Series> seriesList = null;

            //Diagram Temperature
            seriesList = Convert.ToChart(ChartDataType.DiagramTemperature, MollierChart, mollierControlSettings);

            //Relative Humidity
            seriesList = Convert.ToChart(ChartDataType.RelativeHumidity, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_RelativeHumidity(series, mollierControlSettings, 5);
                }
            }

            //Density
            seriesList = Convert.ToChart(ChartDataType.Density, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? 2 : -0.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? -0.5 : 0.0005;

                Modify.AddLabel_Label(MollierChart, seriesList[seriesList.Count / 2], mollierControlSettings, "Density ρ [kg/m³]", offset_X, offset_Y);
            }

            //Enthalpy
            seriesList = Convert.ToChart(ChartDataType.Enthalpy, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? -1.2 : 4.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? 3.2 : -0.0018;

                Series series_Temp = seriesList[seriesList.Count / 2];

                Modify.AddLabel_Label(MollierChart, series_Temp, mollierControlSettings, "Enthalpy h [kJ/kg]", offset_X, offset_Y, series_Temp.Points.Count / 2);
            }

            //Wet Bulb Temperature
            seriesList = Convert.ToChart(ChartDataType.WetBulbTemperature, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? -1.2 : 4.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? 3.2 : -0.0018;

                Series series_Temp = seriesList[seriesList.Count / 2];

                Modify.AddLabel_Label(MollierChart, series_Temp, mollierControlSettings, "Wet Bulb Temperature t_wb [°C]", offset_X, offset_Y, series_Temp.Points.Count - 1);
            }

            //Specific Volume
            seriesList = Convert.ToChart(ChartDataType.SpecificVolume, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? -3 : 4.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? 0 : -0.0018;

                Series series_Temp = seriesList[seriesList.Count / 2];

                Modify.AddLabel_Label(MollierChart, series_Temp, mollierControlSettings, "Specific volume v [m³/kg]", offset_X, offset_Y, series_Temp.Points.Count / 2);
            }

            //CREATING P_w AXIS
            Series series1 = MollierChart.Series.Add("Partial Vapour Pressure pW [kPa]");
            series1.Points.AddXY(partialVapourPressure_min, 0);
            series1.Points.AddXY(partialVapourPressure_max, 0);
            series1.ChartType = SeriesChartType.Spline;
            series1.Color = Color.Transparent;
            series1.BorderColor = Color.Transparent;
            series1.IsVisibleInLegend = false;
            CreateXAxis(MollierChart, chartArea_New, series1, 2, 80, 1, false, partialVapourPressure_min, partialVapourPressure_max);

            if (mollierPoints != null && !mollierControlSettings.DivisionArea)
            {
                addMollierPoints(chartType);
            }
            if (mollierProcesses != null)
            {
                add_MollierProcesses(chartType);
            }
            if (mollierZones != null)
            {
                addMollierZones(chartType);
            }
            if (mollierControlSettings.DivisionArea)
            {
                addDivisionArea(chartType);
            }
            ColorPoints(mollierControlSettings.FindPoint, mollierControlSettings.Percent, mollierControlSettings.FindPointType);
        }

        private void generateGraph_Psychrometric()
        {
            //INITIAL SIZES
            ChartType chartType = mollierControlSettings.ChartType;

            double pressure = mollierControlSettings.Pressure;
            double humidityRatio_Min = mollierControlSettings.HumidityRatio_Min;
            double humidityRatio_Max = mollierControlSettings.HumidityRatio_Max;
            double humidityRatio_interval = mollierControlSettings.HumidityRatio_Interval;
            double temperature_Min = mollierControlSettings.Temperature_Min;
            double temperature_Max = mollierControlSettings.Temperature_Max;
            double temperature_interval = mollierControlSettings.Temperature_Interval;

            if (MinPressure > pressure || pressure > MaxPressure)
            {
                return;
            }

            //BASE CHART INITIALIZATION

            MollierChart.ChartAreas[0].AxisY2.MinorTickMark.Enabled = false;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;

            MollierChart.Series?.Clear();
            ChartArea chartArea = MollierChart.ChartAreas[0];
            ChartArea ca = MollierChart.ChartAreas[0];
            ca.Position = new ElementPosition(2, 2, 95, 95);//define sizes of chart
            ca.InnerPlotPosition = new ElementPosition(8, 6, 85, 85);
            MollierChart.ChartAreas[0].AxisX2.Enabled = AxisEnabled.False;
            MollierChart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            MollierChart.ChartAreas[0].AxisY2.Title = "Humidity Ratio  x [g/kg]";
            MollierChart.ChartAreas[0].AxisY2.Maximum = humidityRatio_Max; //divide by /1000 if want to have kg/kg
            MollierChart.ChartAreas[0].AxisY2.Minimum = humidityRatio_Min;
            MollierChart.ChartAreas[0].AxisY2.Interval = humidityRatio_interval;
            MollierChart.ChartAreas[0].AxisY2.MajorGrid.LineColor = Color.Gray;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.Interval = 1;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.Enabled = true;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.LineColor = Color.LightGray;
            MollierChart.ChartAreas[0].AxisY2.LabelStyle.Format = "0.###";
            MollierChart.ChartAreas[0].AxisY2.LabelStyle.Font = ca.AxisY.LabelStyle.Font;
            double P_w_Min = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Min / 1000, temperature_Min, pressure) / 1000;
            double P_w_Max = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Max / 1000, temperature_Max, pressure) / 1000;
            //AXIS X
            Axis axisX = chartArea.AxisX;
            axisX.Title = "Dry Bulb Temperature t [°C]";
            axisX.Maximum = temperature_Max;
            axisX.Minimum = temperature_Min;
            axisX.Interval = temperature_interval;
            axisX.MajorGrid.Enabled = false;
            axisX.MajorGrid.LineColor = Color.Gray;
            axisX.MinorGrid.Interval = 1;
            axisX.MinorGrid.Enabled = false;
            axisX.MinorGrid.LineColor = Color.LightGray;
            axisX.LabelStyle.Font = ca.AxisY.LabelStyle.Font;
            //AXIS Y - P_w AXIS
            Axis axisY = chartArea.AxisY;

            axisY.MinorTickMark.Enabled = false;
            axisY.MinorGrid.Enabled = false;

            axisY.Enabled = AxisEnabled.False;
            axisY.Title = "Partial Vapour Pressure pW [kPa]";
            axisY.TextOrientation = TextOrientation.Rotated270;
            axisY.Maximum = humidityRatio_Max / 1000;
            //axisY.Minimum = humidityRatio_Min > humidityRatio_Max ? 0 : humidityRatio_Min / 1000;
            axisY.Minimum = humidityRatio_Min / 1000; //TODO: Fix Range
            axisY.Interval = humidityRatio_interval;
            axisY.MajorGrid.Enabled = false;
            axisY.MajorGrid.LineColor = Color.Gray;
            axisY.MinorGrid.Interval = 0.1;
            axisY.MinorGrid.Enabled = false;
            axisY.MinorGrid.LineColor = Color.LightGray;
            //axisY.MinorTickMark.Enabled = true;
            axisY.MajorTickMark.Enabled = true;


            Series series1 = MollierChart.Series.Add("Partial Vapour Pressure pW [kPa]");
            series1.Points.AddXY(0, P_w_Min);
            series1.Points.AddXY(0, P_w_Max);
            series1.ChartType = SeriesChartType.Spline;
            series1.Color = Color.Transparent;
            series1.BorderColor = Color.Transparent;
            series1.IsVisibleInLegend = false;
            CreateYAxis(MollierChart, ca, series1, 5, 12, 25, true, P_w_Min, P_w_Max);

            List<Series> seriesList = null;

            //Dry Bulb Temperature 
            seriesList = Convert.ToChart(ChartDataType.DryBulbTemperature, MollierChart, mollierControlSettings);

            //Relative Humidity
            seriesList = Convert.ToChart(ChartDataType.RelativeHumidity, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_RelativeHumidity(series, mollierControlSettings, 5);
                }
            }

            //Density
            seriesList = Convert.ToChart(ChartDataType.Density, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? 2 : -1.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? -0.5 : 0.01;

                Modify.AddLabel_Label(MollierChart, seriesList[seriesList.Count / 2], mollierControlSettings, "Density ρ [kg/m³]", offset_X, offset_Y);
            }

            //Enthalpy
            seriesList = Convert.ToChart(ChartDataType.Enthalpy, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? -1.2 : 4.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? 3.2 : -0.0018;

                Series series_Temp = seriesList[seriesList.Count / 2];

                Modify.AddLabel_Label(MollierChart, series_Temp, mollierControlSettings, "Enthalpy h [kJ/kg]", offset_X, offset_Y, series_Temp.Points.Count / 2);
            }

            //Wet Bulb Temperature
            seriesList = Convert.ToChart(ChartDataType.WetBulbTemperature, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? -1.2 : 4.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? 3.2 : -0.0018;

                Series series_Temp = seriesList[seriesList.Count / 2];

                Modify.AddLabel_Label(MollierChart, series_Temp, mollierControlSettings, "Wet Bulb Temperature t_wb [°C]", offset_X, offset_Y, series_Temp.Points.Count - 1);
            }

            //Specific Volume
            seriesList = Convert.ToChart(ChartDataType.SpecificVolume, MollierChart, mollierControlSettings);
            if (seriesList != null && seriesList.Count != 0)
            {
                foreach (Series series in seriesList)
                {
                    Modify.AddLabel_Unit(MollierChart, series, mollierControlSettings);
                }

                double offset_X = mollierControlSettings.ChartType == ChartType.Mollier ? -3 : 3.5;
                double offset_Y = mollierControlSettings.ChartType == ChartType.Mollier ? 0 : -0.007;

                Series series_Temp = seriesList[seriesList.Count / 2];

                Modify.AddLabel_Label(MollierChart, series_Temp, mollierControlSettings, "Specific volume v [m³/kg]", offset_X, offset_Y, series_Temp.Points.Count / 2);
            }

            if (mollierPoints != null && !mollierControlSettings.DivisionArea)
            {
                addMollierPoints(chartType);
            }

            if (mollierProcesses != null)
            {
                add_MollierProcesses(chartType);
            }
            if (mollierZones != null)
            {
                addMollierZones(chartType);
            }
            if (mollierControlSettings.DivisionArea)
            {
                addDivisionArea(chartType);
            }
            ColorPoints(mollierControlSettings.FindPoint, mollierControlSettings.Percent, mollierControlSettings.FindPointType);
        }


        public List<UIMollierPoint> AddPoints(IEnumerable<IMollierPoint> mollierPoints, bool checkPressure = true)
        {
            if (mollierPoints == null)
            {
                return null;
            }

            if (this.mollierPoints == null)
            {
                this.mollierPoints = new List<UIMollierPoint>();
            }

            List<UIMollierPoint> result = new List<UIMollierPoint>();
            foreach (IMollierPoint mollierPoint in mollierPoints)
            {
                if(mollierPoint == null)
                {
                    continue;
                }

                if(checkPressure && !(mollierPoint.Pressure.AlmostEqual(mollierControlSettings.Pressure)))
                {
                    continue;
                }

                UIMollierPoint uIMollierPoint = mollierPoint as UIMollierPoint;
                if(uIMollierPoint == null)
                {
                    if (mollierPoint is MollierPoint)
                    {
                        UIMollierAppearance uIMollierAppearance = new UIMollierAppearance(Color.Blue);

                        uIMollierPoint = new UIMollierPoint((MollierPoint)mollierPoint, uIMollierAppearance);
                    }
                }

                if(uIMollierPoint == null)
                {
                    continue;
                }

                result.Add(uIMollierPoint);
            }
            this.mollierPoints.AddRange(result);

            GenerateGraph();
            
            return result;
        }

        public List<UIMollierProcess> AddProcesses(IEnumerable<IMollierProcess> mollierProcesses, bool checkPressure = true)
        {
            if (mollierProcesses == null)
            {
                return null;
            }
            if (this.mollierProcesses == null)
            {
                this.mollierProcesses = new List<UIMollierProcess>();
            }
            List<UIMollierProcess> result = new List<UIMollierProcess>();
            foreach (IMollierProcess mollierProcess_iteration in mollierProcesses)
            {
                IMollierProcess mollierProcess = mollierProcess_iteration;
                if (mollierProcess.Start == null || mollierProcess.End == null)
                {
                    continue;
                }

                if (checkPressure && !(mollierProcess.Start.Pressure.AlmostEqual(mollierControlSettings.Pressure)))
                {
                    continue;
                }

                //if (checkPressure && !Core.Query.AlmostEqual(mollierProcess.Pressure, mollierControlSettings.Pressure, Tolerance.MacroDistance))
                //{
                //    return null;
                //}
                if (mollierProcess is MollierProcess)
                {
                    UIMollierProcess mollierProcess_Temp = new UIMollierProcess((MollierProcess)mollierProcess, Color.Empty);
                    mollierProcess = mollierProcess_Temp;
                }
                UIMollierProcess uIMollierProcess = new UIMollierProcess((UIMollierProcess)mollierProcess);
                if (uIMollierProcess.MollierProcess is UndefinedProcess && uIMollierProcess.UIMollierAppearance_End.Label == null)
                {
                    uIMollierProcess.UIMollierAppearance_End.Label = "ROOM";
                }
                this.mollierProcesses.Add(uIMollierProcess);
                result.Add(uIMollierProcess);
            }
            systems = Query.ProcessSortBySystem(this.mollierProcesses);
            GenerateGraph();
            return result;
        }

        public bool AddZone(MollierZone mollierZone)
        {
            if (mollierZone == null)
            {
                //for now to create possibility to disable Zone
                mollierZones = new List<MollierZone>();
                GenerateGraph();
                return false;
            }
            if (mollierZones == null)
            {
                mollierZones = new List<MollierZone>();
            }
            mollierZones.Add(mollierZone);
            GenerateGraph();
            return true;
        }

        public bool Clear()
        {
            mollierPoints?.Clear();
            mollierProcesses?.Clear();
            GenerateGraph();
            return true;
        }

        public bool Save(ChartExportType chartExportType, PageSize pageSize = PageSize.A4, PageOrientation pageOrientation = PageOrientation.Landscape, string path = null)
        {
            string pageType = string.Format("{0}_{1}", pageSize, pageOrientation);

            if (string.IsNullOrEmpty(path))
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    string name = mollierControlSettings.ChartType == ChartType.Mollier ? "Mollier" : "Psychrometric";
                    switch (chartExportType)
                    {
                        case ChartExportType.PDF:
                            saveFileDialog.Filter = "PDF document (*.pdf)|*.pdf|All files (*.*)|*.*";
                            name += "_" + pageType;
                            break;
                        case ChartExportType.JPG:
                            saveFileDialog.Filter = "JPG document (*.jpg)|*.jpg|All files (*.*)|*.*";
                            break;
                        case ChartExportType.EMF:
                            saveFileDialog.Filter = "EMF document (*.emf)|*.emf|All files (*.*)|*.*";
                            break;
                    }
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;
                    saveFileDialog.FileName = name;
                    if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return false;
                    }
                    path = saveFileDialog.FileName;
                }
            }
            if (chartExportType == ChartExportType.JPG)
            {
                MollierChart.SaveImage(path, ChartImageFormat.Jpeg);
                return true;
            }
            if (chartExportType == ChartExportType.EMF)
            {
                MollierChart.SaveImage(path, ChartImageFormat.Emf);
                return true;
            }
            if (chartExportType == ChartExportType.PDF)
            {
                string path_Template = Core.Query.TemplatesDirectory(typeof(Address).Assembly);
                if (!System.IO.Directory.Exists(path_Template))
                {
                    return false;
                }
                string fileName = pageOrientation == PageOrientation.Portrait ? "PDF_Print_AHU_Portrait.xlsx" : "PDF_Print_AHU.xlsx";
                path_Template = System.IO.Path.Combine(path_Template, "AHU", fileName);
                if (!System.IO.File.Exists(path_Template))
                {
                    return false;
                }

                string directory = System.IO.Path.GetDirectoryName(path_Template);
                string worksheetName = pageType;//this should be changed
                if (string.IsNullOrWhiteSpace(path_Template) || !System.IO.File.Exists(path_Template) || string.IsNullOrWhiteSpace(worksheetName))
                {
                    return false;
                }
                string path_Temp = "";
                Func<NetOffice.ExcelApi.Workbook, bool> func = new Func<NetOffice.ExcelApi.Workbook, bool>((NetOffice.ExcelApi.Workbook workbook) =>
                {
                    if (workbook == null)
                    {
                        return false;
                    }
                    string name = "AHU";
                    NetOffice.ExcelApi.Worksheet workseet_Template = Excel.Query.Worksheet(workbook, worksheetName);
                    if (workseet_Template == null)
                    {
                        return false;
                    }

                    NetOffice.ExcelApi.Worksheet worksheet = Excel.Query.Worksheet(workbook, name);

                    if (worksheet != null)
                    {
                        worksheet.Delete();
                    }


                    worksheet = Excel.Modify.Copy(workseet_Template, name);

                    NetOffice.ExcelApi.Range range = null;


                    HashSet<string> uniqueNames = new HashSet<string>();
                    uniqueNames.Add("[ProcessPointName]");
                    uniqueNames.Add("[DryBulbTemperature]");
                    uniqueNames.Add("[HumidityRatio]");
                    uniqueNames.Add("[RelativeHumidity]");
                    uniqueNames.Add("[WetBulbTemperature]");
                    uniqueNames.Add("[SaturationTemperature]");
                    uniqueNames.Add("[Enthalpy]");
                    uniqueNames.Add("[Density]");
                    uniqueNames.Add("[AtmosphericPressure]");
                    uniqueNames.Add("[SpecificVolume]");
                    uniqueNames.Add("[ProcessName]");
                    uniqueNames.Add("[deltaT]");
                    uniqueNames.Add("[deltaX]");
                    uniqueNames.Add("[deltaH]");
                    int numberOfData = 14;
                    int columnIndex_Min = 100;
                    int rowIndex_Min = 100;
                    Dictionary<string, NetOffice.ExcelApi.Range> dictionary = new Dictionary<string, NetOffice.ExcelApi.Range>();
                    object[,] values = worksheet.Range(worksheet.Cells[1, 1], worksheet.Cells[100, 30]).Value as object[,];
                    for (int i = values.GetLowerBound(0); i <= values.GetUpperBound(0); i++)
                    {
                        for (int j = values.GetLowerBound(1); j <= values.GetUpperBound(1); j++)
                        {
                            if (!(values[i, j] is string))
                            {
                                continue;
                            }

                            string rangeValue = values[i, j] as string;
                            if (string.IsNullOrWhiteSpace(rangeValue))
                            {
                                continue;
                            }
                            foreach (string name_Temp in uniqueNames)
                            {
                                if (rangeValue == name_Temp)
                                {
                                    if (j < columnIndex_Min)
                                    {
                                        columnIndex_Min = j;
                                        rowIndex_Min = i;
                                    }
                                    dictionary.Add(name_Temp, worksheet.Cells[i, j]);
                                    break;
                                }
                            }
                        }
                    }

                    if (systems == null || systems.Count == 0)
                    {
                        for (int i = 0; i < numberOfData; i++)
                        {
                            worksheet.Cells[rowIndex_Min, columnIndex_Min + i].Value = string.Empty;
                        }
                    }
                    else
                    {
                        NetOffice.ExcelApi.Range range_1 = worksheet.Range(worksheet.Cells[rowIndex_Min, columnIndex_Min], worksheet.Cells[rowIndex_Min, columnIndex_Min + numberOfData]);
                        int move_Temp = 1;
                        for (int i = 0; i < systems.Count; i++)
                        {
                            range_1.Copy(worksheet.Range(worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min], worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min + numberOfData]));
                            move_Temp++;
                            for (int j = 0; j < systems[i].Count; j++)
                            {
                                UIMollierProcess UI_MollierProcess = systems[i][j];
                                MollierProcess mollierProcess = UI_MollierProcess.MollierProcess;
                                if (UI_MollierProcess.UIMollierAppearance_Start.Label != null && UI_MollierProcess.UIMollierAppearance_Start.Label != "")
                                {
                                    range_1.Copy(worksheet.Range(worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min], worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min + numberOfData]));
                                    move_Temp++;
                                }
                                if (UI_MollierProcess.UIMollierAppearance_End.Label != null && UI_MollierProcess.UIMollierAppearance_End.Label != "")
                                {
                                    range_1.Copy(worksheet.Range(worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min], worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min + numberOfData]));
                                    move_Temp++;
                                }
                            }
                            if (i != systems.Count - 1)
                            {
                                range_1.Copy(worksheet.Range(worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min], worksheet.Cells[rowIndex_Min + move_Temp, columnIndex_Min + numberOfData]));
                            }
                            move_Temp++;
                        }

                        //range_Temp.Copy(worksheet.Cells[rowIndex + id, columnIndex]);
                        foreach (string key_Temp in uniqueNames)
                        {
                            if (!dictionary.ContainsKey(key_Temp))
                            {
                                continue;
                            }

                            NetOffice.ExcelApi.Range range_Temp = dictionary[key_Temp];
                            int columnIndex = range_Temp.Column;
                            int rowIndex = range_Temp.Row;
                            int id = 0;

                            for (int i = 0; i < systems.Count; i++)
                            {
                                //range_Temp.Copy(worksheet.Cells[rowIndex + id, columnIndex]);
                                worksheet.Cells[rowIndex + id, columnIndex].Value = "----";
                                id++;
                                for (int j = 0; j < systems[i].Count; j++)
                                {
                                    UIMollierProcess UI_MollierProcess = systems[i][j];
                                    MollierProcess mollierProcess = UI_MollierProcess.MollierProcess;
                                    MollierPoint start = mollierProcess.Start;
                                    MollierPoint end = mollierProcess.End;
                                    string value_1 = string.Empty;
                                    string value_2 = string.Empty;
                                    switch (key_Temp)
                                    {
                                        case "[ProcessPointName]":
                                            value_1 = UI_MollierProcess.UIMollierAppearance_Start.Label;
                                            value_2 = UI_MollierProcess.UIMollierAppearance_End.Label;
                                            break;
                                        case "[DryBulbTemperature]":
                                            value_1 = System.Math.Round(start.DryBulbTemperature, 2).ToString();
                                            value_2 = System.Math.Round(end.DryBulbTemperature, 2).ToString();
                                            break;
                                        case "[HumidityRatio]":
                                            value_1 = System.Math.Round(start.HumidityRatio * 1000, 2).ToString();
                                            value_2 = System.Math.Round(end.HumidityRatio * 1000, 2).ToString();
                                            break;
                                        case "[RelativeHumidity]":
                                            value_1 = System.Math.Round(start.RelativeHumidity, 1).ToString();
                                            value_2 = System.Math.Round(end.RelativeHumidity, 1).ToString();
                                            break;
                                        case "[WetBulbTemperature]":
                                            value_1 = System.Math.Round(start.WetBulbTemperature(), 2).ToString();
                                            value_2 = System.Math.Round(end.WetBulbTemperature(), 2).ToString();
                                            break;
                                        case "[SaturationTemperature]":
                                            value_1 = System.Math.Round(start.SaturationTemperature(), 2).ToString();
                                            value_2 = System.Math.Round(end.SaturationTemperature(), 2).ToString();
                                            break;
                                        case "[Enthalpy]":
                                            value_1 = System.Math.Round(start.Enthalpy / 1000, 2).ToString();
                                            value_2 = System.Math.Round(end.Enthalpy / 1000, 2).ToString();
                                            break;
                                        case "[SpecificVolume]":
                                            value_1 = System.Math.Round(start.SpecificVolume(), 3).ToString();
                                            value_2 = System.Math.Round(end.SpecificVolume(), 3).ToString();
                                            break;
                                        case "[Density]":
                                            value_1 = System.Math.Round(start.Density(), 3).ToString();
                                            value_2 = System.Math.Round(end.Density(), 3).ToString();
                                            break;
                                        case "[AtmosphericPressure]":
                                            value_1 = System.Math.Round(start.Pressure, 1).ToString();
                                            value_2 = System.Math.Round(end.Pressure, 1).ToString();
                                            break;
                                        case "[ProcessName]":
                                            value_2 = Query.FullProcessName(UI_MollierProcess);
                                            break;
                                        case "[deltaT]":
                                            value_2 = (System.Math.Round(end.DryBulbTemperature, 2) - System.Math.Round(start.DryBulbTemperature, 2)).ToString();
                                            break;
                                        case "[deltaX]":
                                            value_2 = (System.Math.Round(end.HumidityRatio * 1000, 2) - System.Math.Round(start.HumidityRatio * 1000, 2)).ToString();
                                            break;
                                        case "[deltaH]":
                                            value_2 = (System.Math.Round(end.Enthalpy / 1000, 2) - System.Math.Round(start.Enthalpy / 1000, 2)).ToString();
                                            break;
                                    }



                                    if (UI_MollierProcess.UIMollierAppearance_Start.Label != null && UI_MollierProcess.UIMollierAppearance_Start.Label != "")
                                    {
                                        if (value_1 != string.Empty)
                                        {
                                            //range_Temp.Copy(worksheet.Cells[rowIndex + id, columnIndex]);
                                            worksheet.Cells[rowIndex + id, columnIndex].Value = value_1;
                                            id++;
                                        }
                                        else if (key_Temp == "[ProcessName]" || key_Temp == "[deltaT]" || key_Temp == "[deltaX]" || key_Temp == "[deltaH]")
                                        {
                                            //range_Temp.Copy(worksheet.Cells[rowIndex + id, columnIndex]);
                                            worksheet.Cells[rowIndex + id, columnIndex].Value = "-";
                                            id++;
                                        }
                                    }
                                    if (UI_MollierProcess.UIMollierAppearance_End.Label != null && UI_MollierProcess.UIMollierAppearance_End.Label != "")
                                    {
                                        //range_Temp.Copy(worksheet.Cells[rowIndex + id, columnIndex]);
                                        if (value_2 != string.Empty)
                                        {
                                            worksheet.Cells[rowIndex + id, columnIndex].Value = value_2;
                                            id++;
                                        }
                                        else
                                        {
                                            worksheet.Cells[rowIndex + id, columnIndex].Value = "-";
                                            id++;
                                        }
                                    }
                                }
                                //range_Temp.Copy(worksheet.Cells[rowIndex + id, columnIndex]);
                                if (key_Temp == "[ProcessName]")
                                {
                                    int integer = 2;
                                }
                                worksheet.Cells[rowIndex + id, columnIndex].Value = "----";
                                id++;
                            }

                        }
                    }

                    range = Excel.Query.Range(worksheet.UsedRange, pageType);

                    if (range == null)
                    {
                        return false;
                    }
                    float left = (float)(double)range.Left;
                    float top = (float)(double)range.Top;

                    float width = (float)(double)range.Width;
                    float height = (float)(double)range.Height;

                    path_Temp = System.IO.Path.GetTempFileName();

                    Size size_Temp = Size;
                    //Size = new Size(System.Convert.ToInt32(width), System.Convert.ToInt32(height));

                    if (pageSize == PageSize.A3)//a3 pdf
                    {
                        Size = new Size(System.Convert.ToInt32(width * 1.4), System.Convert.ToInt32(height * 1.4));
                    }
                    else//a4 pdf
                    {
                        Size = new Size(System.Convert.ToInt32(width * 2), System.Convert.ToInt32(height * 2));
                    }
                    Save(ChartExportType.EMF, path: path_Temp);

                    Size = size_Temp;


                    NetOffice.ExcelApi.Shape shape = worksheet.Shapes.AddPicture(path_Temp, NetOffice.OfficeApi.Enums.MsoTriState.msoFalse, NetOffice.OfficeApi.Enums.MsoTriState.msoCTrue, left, top, width, height);

                    //double shapeSizeFactor = Query.ShapeSizeFactor(DeviceDpi);

                    shape.PictureFormat.Crop.ShapeHeight = (float)(shape.PictureFormat.Crop.ShapeHeight * Query.ShapeSizeFactor(DeviceDpi, 0.79));
                    shape.PictureFormat.Crop.ShapeWidth = (float)(shape.PictureFormat.Crop.ShapeWidth * Query.ShapeSizeFactor(DeviceDpi, 0.76));
                    shape.Width = width;
                    shape.Height = height;
                    range.Value = string.Empty;

                    workbook.SaveCopyAs(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "TEST.xlsx"));

                    worksheet.ExportAsFixedFormat(NetOffice.ExcelApi.Enums.XlFixedFormatType.xlTypePDF, path);

                    return false;
                });

                Excel.Modify.Edit(path_Template, func);

                System.Threading.Thread.Sleep(1000);

                if (System.IO.File.Exists(path_Temp))
                {
                    if (Core.Query.WaitToUnlock(path_Temp))
                    {
                        System.IO.File.Delete(path_Temp);
                    }
                }
                return true;
            }

            return true;
        }

        public bool Print()
        {
            PrintingManager printingManager = MollierChart?.Printing;
            if (printingManager == null)
            {
                return false;
            }

            printingManager.Print(true);
            return true;
        }

        private void ToolStripMenuItem_ProcessesAndPoints_Click(object sender, EventArgs e)
        {
            if (mollierPoints == null && mollierProcesses == null)
            {
                MessageBox.Show("There is no process or point to zoom");
                return;
            }
            ChartType chartType = mollierControlSettings.ChartType;
            Query.ZoomParameters(MollierChart.Series, chartType, out double x_Min, out double x_Max, out double y_Min, out double y_Max);
            mollierControlSettings.HumidityRatio_Min = chartType == ChartType.Mollier ? x_Min : y_Min;
            mollierControlSettings.HumidityRatio_Max = chartType == ChartType.Mollier ? x_Max : y_Max;
            mollierControlSettings.Temperature_Min = chartType == ChartType.Mollier ? y_Min : x_Min;
            mollierControlSettings.Temperature_Max = chartType == ChartType.Mollier ? y_Max : x_Max;
            GenerateGraph();
        }

        private void ToolStripMenuItem_Selection_Click(object sender, EventArgs e)
        {
            selection = true;
        }

        private void ToolStripMenuItem_Reset_Click(object sender, EventArgs e)
        {
            MollierControlSettings mollierControlSettings_1 = new MollierControlSettings();
            mollierControlSettings.HumidityRatio_Min = mollierControlSettings_1.HumidityRatio_Min;
            mollierControlSettings.HumidityRatio_Max = mollierControlSettings_1.HumidityRatio_Max;
            mollierControlSettings.Temperature_Min = mollierControlSettings_1.Temperature_Min;
            mollierControlSettings.Temperature_Max = mollierControlSettings_1.Temperature_Max;
            GenerateGraph();
        }

        private void MollierChart_MouseDown(object sender, MouseEventArgs e)
        {
            if (!selection)
            {
                return;
            }
            mdown = e.Location;
        }

        private void MollierChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (selection)
            {
                if (e.Button == MouseButtons.Left)
                {
                    MollierChart.Refresh();
                    using (Graphics g = MollierChart.CreateGraphics())
                        g.DrawRectangle(Pens.Red, GetRectangle(mdown, e.Location));
                }

                return;
            }

            foreach (Tuple<Series, int> seriesData_Temp in seriesData)
            {
                seriesData_Temp.Item1.BorderWidth = seriesData_Temp.Item2;
            }

            seriesData.Clear();

            Point point = e.Location;

            HitTestResult[] hitTestResults = MollierChart?.HitTest(point.X, point.Y, false, ChartElementType.DataPoint);
            if (hitTestResults == null)
            {
                return;
            }

            foreach (HitTestResult hitTestResult in hitTestResults)
            {
                Series series = hitTestResult?.Series;
                if (series == null)
                {
                    continue;
                }

                seriesData.Add(new Tuple<Series, int>(series, series.BorderWidth));

                if(series.Tag is MollierProcess)
                {
                    series.BorderWidth += 2;
                }
                else
                {
                    series.BorderWidth += 1;
                }

                //int index = hitTestResult.PointIndex;
                //if(index >= 0)
                //{
                //    DataPoint dataPoint = series.Points[index];
                //    if(dataPoint != null)
                //    {
                //        dataPoint.BorderWidth += 2;
                //    }
                //}
            }
        }

        private void MollierChart_MouseUp(object sender, MouseEventArgs e)
        {
            if (!selection)
            {
                return;
            }
            Axis ax = MollierChart.ChartAreas[0].AxisX;
            Axis ay = MollierChart.ChartAreas[0].AxisY;
            Point mup = e.Location;
            //chart sizes(axis)
            double X_Min = mollierControlSettings.ChartType == ChartType.Mollier ? mollierControlSettings.HumidityRatio_Min : mollierControlSettings.Temperature_Min;
            double Y_Min = mollierControlSettings.ChartType == ChartType.Mollier ? mollierControlSettings.Temperature_Min : mollierControlSettings.HumidityRatio_Min;
            double X_Max = mollierControlSettings.ChartType == ChartType.Mollier ? mollierControlSettings.HumidityRatio_Max : mollierControlSettings.Temperature_Max;
            double Y_Max = mollierControlSettings.ChartType == ChartType.Mollier ? mollierControlSettings.Temperature_Max : mollierControlSettings.HumidityRatio_Max;

            ////new selection area
            if (mup.X < 0 || mup.Y < 0)
            {
                MollierChart.Refresh();
                return;
            }
            double x_Min = System.Math.Min((double)ax.PixelPositionToValue(mup.X), (double)ax.PixelPositionToValue(mdown.X));
            double x_Max = System.Math.Max((double)ax.PixelPositionToValue(mup.X), (double)ax.PixelPositionToValue(mdown.X));
            double y_Min = System.Math.Min((double)ay.PixelPositionToValue(mup.Y), (double)ay.PixelPositionToValue(mdown.Y));
            double y_Max = System.Math.Max((double)ay.PixelPositionToValue(mup.Y), (double)ay.PixelPositionToValue(mdown.Y));

			//Rounding MD 2023-06-26
            //y_Min = mollierControlSettings.ChartType == ChartType.Mollier ? System.Math.Round(y_Min) : System.Math.Round(y_Min * 1000) / 1000;
            //y_Max = mollierControlSettings.ChartType == ChartType.Mollier ? System.Math.Round(y_Max) : System.Math.Round(y_Max * 1000) / 1000;
            //x_Min = System.Math.Round(x_Min);
            //x_Max = System.Math.Round(x_Max);
			//


            double x_Difference = x_Max - x_Min;
            double y_Difference = mollierControlSettings.ChartType == ChartType.Mollier ? y_Max - y_Min : (y_Max - y_Min) * 1000;
            if (x_Difference < 1 || y_Difference < 1)
            {
                MollierChart.Refresh();
                return;
            }

            mollierControlSettings.HumidityRatio_Min = mollierControlSettings.ChartType == ChartType.Mollier ? System.Math.Max(x_Min, X_Min) : System.Math.Max(y_Min * 1000, Y_Min);
            mollierControlSettings.HumidityRatio_Max = mollierControlSettings.ChartType == ChartType.Mollier ? System.Math.Min(x_Max, X_Max) : System.Math.Min(y_Max * 1000, Y_Max);
            mollierControlSettings.Temperature_Min = mollierControlSettings.ChartType == ChartType.Mollier ? System.Math.Max(y_Min, Y_Min) : System.Math.Max(x_Min, X_Min);
            mollierControlSettings.Temperature_Max = mollierControlSettings.ChartType == ChartType.Mollier ? System.Math.Min(y_Max, Y_Max) : System.Math.Min(x_Max, X_Max);

            mollierControlSettings.HumidityRatio_Min = System.Math.Round(mollierControlSettings.HumidityRatio_Min);
            mollierControlSettings.HumidityRatio_Max = System.Math.Round(mollierControlSettings.HumidityRatio_Max);
            mollierControlSettings.Temperature_Min = System.Math.Round(mollierControlSettings.Temperature_Min);
            mollierControlSettings.Temperature_Max = System.Math.Round(mollierControlSettings.Temperature_Max);


            GenerateGraph();

            MollierChart.Refresh();
            selection = false;
        }

        static public Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(System.Math.Min(p1.X, p2.X), System.Math.Min(p1.Y, p2.Y), System.Math.Abs(p1.X - p2.X), System.Math.Abs(p1.Y - p2.Y));
        }

        public MollierControlSettings MollierControlSettings
        {
            get
            {
                if (mollierControlSettings == null)
                {
                    return null;
                }

                return new MollierControlSettings(mollierControlSettings);
            }
            set
            {
                if (value == null)
                {
                    mollierControlSettings = null;
                }

                mollierControlSettings = new MollierControlSettings(value);
                GenerateGraph();
            }
        }

        /// <summary>
        /// Checks if there exist any point
        /// </summary>
        public bool HasMollierPoints
        {
            get
            {
                return mollierPoints != null && mollierPoints.Count != 0;
            }
        }

        public List<UIMollierPoint> UIMollierPoints
        {
            get
            {
                if (mollierPoints == null)
                {
                    return null;
                }

                return mollierPoints.ConvertAll(x => x?.Clone());
            }
        }

        public List<UIMollierProcess> UIMollierProcesses
        {
            get
            {
                if (mollierProcesses == null)
                {
                    return null;
                }

                return mollierProcesses.ConvertAll(x => x?.Clone());
            }
        }

        private void MollierControl_SizeChanged(object sender, EventArgs e)
        {
            if (mollierControlSettings == null)
            {
                return;
            }

            GenerateGraph();
        }

        public void ColorPoints(bool generate, double percent, string chartDataType)
        {
            foreach (Series series_Temp in MollierChart.Series)
            {
                if (series_Temp.Tag == "ColorPoint")
                {
                    series_Temp.Enabled = false;
                }
            }
            if (generate == false || mollierPoints == null || mollierPoints.Count < 4 || percent > 100 || percent < 0)//if too 
            {
                return;
            }
            int index = System.Convert.ToInt32((1 - percent / 100) * mollierPoints.Count) - 1;
            if (index < 0)
            {
                index = 0;
            }

            List<UIMollierPoint> uIMollierPoints = new List<UIMollierPoint>(mollierPoints);//copy of mollierPoints
            Series series = MollierChart.Series.Add(Guid.NewGuid().ToString());
            series.IsVisibleInLegend = false;
            series.ChartType = SeriesChartType.Point;
            series.BorderWidth = 4;
            series.MarkerColor = Color.Red;
            series.MarkerSize = 10;
            series.MarkerStyle = MarkerStyle.Circle;
            series.Tag = "ColorPoint";
            Series series1 = MollierChart.Series.Add(Guid.NewGuid().ToString());
            series1.IsVisibleInLegend = false;
            series1.ChartType = SeriesChartType.Point;
            series1.BorderWidth = 4;
            series1.MarkerColor = Color.Red;
            series1.MarkerSize = 15;
            series1.MarkerStyle = MarkerStyle.Circle;
            series1.Tag = "ColorPointLabelSquare";
            if (mollierControlSettings.ChartType == ChartType.Mollier)
            {
                series1.Points.AddXY((mollierControlSettings.HumidityRatio_Min + mollierControlSettings.HumidityRatio_Max) / 2, (mollierControlSettings.Temperature_Min + mollierControlSettings.Temperature_Max) / 4);
            }
            else
            {
                series1.Points.AddXY((mollierControlSettings.Temperature_Min + mollierControlSettings.Temperature_Max) / 4, (mollierControlSettings.HumidityRatio_Min + mollierControlSettings.HumidityRatio_Max) / 2000);
            }
            switch (chartDataType)
            {
                case "Temperature":
                    uIMollierPoints.Sort((x, y) => x.MollierPoint.DryBulbTemperature.CompareTo(y.MollierPoint.DryBulbTemperature));
                    UIMollierPoint uIMollierPoint_Temperature = uIMollierPoints[index];
                    double X_Temperature = mollierControlSettings.ChartType == ChartType.Mollier ? uIMollierPoint_Temperature.MollierPoint.HumidityRatio * 1000 : uIMollierPoint_Temperature.MollierPoint.DryBulbTemperature;
                    double Y_Temperature = mollierControlSettings.ChartType == ChartType.Mollier ? Mollier.Query.DiagramTemperature(uIMollierPoint_Temperature.MollierPoint) : uIMollierPoint_Temperature.MollierPoint.HumidityRatio;
                    series.Points.AddXY(X_Temperature, Y_Temperature);
                    string name_Temperature = Query.ToolTipText(uIMollierPoint_Temperature.MollierPoint, mollierControlSettings.ChartType, "Temperature " + percent.ToString() + "%") + "\nUnmet hours: " + System.Math.Ceiling(percent / 100 * uIMollierPoints.Count).ToString();
                    create_moved_label(mollierControlSettings.ChartType, series1.Points[0].XValue, series1.Points[0].YValues[0], 0, 0, 0, -16 * Query.ScaleVector2D(this, MollierControlSettings).Y, 0, 0, name_Temperature, ChartDataType.Undefined, ChartParameterType.Point, color: Color.Black, tag: "ColorPointLabel");
                    break;
                case "Enthalpy":
                    uIMollierPoints.Sort((x, y) => x.MollierPoint.Enthalpy.CompareTo(y.MollierPoint.Enthalpy));
                    MollierPoint mollierPoint_Enthalpy = uIMollierPoints[index].MollierPoint;
                    double X_Enthalpy = mollierControlSettings.ChartType == ChartType.Mollier ? mollierPoint_Enthalpy.HumidityRatio * 1000 : mollierPoint_Enthalpy.DryBulbTemperature;
                    double Y_Enthalpy = mollierControlSettings.ChartType == ChartType.Mollier ? Mollier.Query.DiagramTemperature(mollierPoint_Enthalpy) : mollierPoint_Enthalpy.HumidityRatio;
                    series.Points.AddXY(X_Enthalpy, Y_Enthalpy);

                    string name_Enthalpy = Query.ToolTipText(mollierPoint_Enthalpy, mollierControlSettings.ChartType, "Enthalpy " + percent.ToString() + "%") + "\nUnmet hours: " + System.Math.Ceiling(percent / 100 * uIMollierPoints.Count).ToString();
                    create_moved_label(mollierControlSettings.ChartType, series1.Points[0].XValue, series1.Points[0].YValues[0], 0, 0, 0, -16 * Query.ScaleVector2D(this, MollierControlSettings).Y, 0, 0, name_Enthalpy, ChartDataType.Undefined, ChartParameterType.Point, color: Color.Black, tag: "ColorPointLabel");
                    break;
            }
        }

        private void MollierChart_MouseClick(object sender, MouseEventArgs e)
        {
            MollierPoint mollierPoint = GetMollierPoint(e.X, e.Y);

            MollierPointSelected?.Invoke(this, new MollierPointSelectedEventArgs(mollierPoint));
        }

        public MollierPoint GetMollierPoint(int x, int y)
        {
            double chartX = MollierChart.ChartAreas[0].AxisX.PixelPositionToValue(x);
            double chartY = MollierChart.ChartAreas[0].AxisY.PixelPositionToValue(y);

            return Query.MollierPoint(chartX, chartY, mollierControlSettings);
        }

        public void ClearObjects()
        {
            mollierPoints?.Clear();
            mollierProcesses?.Clear();
            mollierZones?.Clear();
            GenerateGraph();
        }
    }
}
