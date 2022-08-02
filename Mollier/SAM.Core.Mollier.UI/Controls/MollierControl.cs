﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace SAM.Core.Mollier.UI.Controls
{
    public partial class MollierControl : UserControl
    {
        public static double MaxPressure = 108400, MinPressure = 90000;
        private Point mdown = Point.Empty;
        private bool selection = false;
        private MollierControlSettings mollierControlSettings;
        private List<MollierPoint> mollierPoints;
        private List<IMollierProcess> mollierProcesses;
        
        public MollierControl()
        {
            InitializeComponent();

            mollierControlSettings = new MollierControlSettings();
        }
        private void create_relative_humidity_line_Mollier(int temperature_Min, int temperature_Max, double relative_humidity, double pressure)
        {
            List<List<Geometry.Planar.Point2D>> humidity_ratio_points = new List<List<Geometry.Planar.Point2D>>();
            for (int i = temperature_Min; i <= temperature_Max; i++)
            {
                humidity_ratio_points.Add(new List<Geometry.Planar.Point2D>());
            }
            int index = temperature_Min;
            for (int i = 1; i <= 10; i++)
            {
                bool adjust_RH = temperature_Min == -20 ? true : false;
                if (adjust_RH == true && i % 2 == 1)
                    temperature_Min = -10;
                string unit = (i * 10).ToString() + '%';
                Series series = MollierChart.Series.Add(unit);
                series.IsVisibleInLegend = false;
                series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Line, ChartDataType.RelativeHumidity);
                series.ChartType = SeriesChartType.Spline;
                List<Geometry.Planar.Point2D> relative_humidity_points = new List<Geometry.Planar.Point2D>();
                for (int j = temperature_Min; j <= temperature_Max; j++)
                {
                    double humidity_ratio = Mollier.Query.HumidityRatio(j, relative_humidity, pressure);
                    double diagram_temperature = Mollier.Query.DiagramTemperature(j, humidity_ratio);
                    if (humidity_ratio_points[j - index].Count == 0)
                        humidity_ratio_points[j - index].Add(new Geometry.Planar.Point2D(0, j));
                    relative_humidity_points.Add(new Geometry.Planar.Point2D(humidity_ratio * 1000, diagram_temperature));
                    humidity_ratio_points[j - index].Add(new Geometry.Planar.Point2D(humidity_ratio * 1000, diagram_temperature));
                }
                foreach (Geometry.Planar.Point2D point2D in relative_humidity_points)
                {
                    series.Points.AddXY(point2D.X, point2D.Y);
                    if (i == 10)
                    {
                        series.BorderWidth = 3;
                    }
                }
                //rotate relative humidity label
                int index_Point = 5;
                int count = relative_humidity_points.Count;
                if ((count - (index_Point + 1) - i < 0) || (count - (index_Point - 1) - i < 0))
                {
                    if (adjust_RH == true)
                        temperature_Min = -20;
                    relative_humidity += 10;
                    continue;
                }
                double range_difference = (mollierControlSettings.Temperature_Max - mollierControlSettings.Temperature_Min) / (mollierControlSettings.HumidityRatio_Max - mollierControlSettings.HumidityRatio_Min) * 2;
                Geometry.Planar.Point2D point2D_1 = relative_humidity_points[count - (index_Point - 1) - i];
                Geometry.Planar.Point2D point2D_2 = relative_humidity_points[count - (index_Point + 1) - i];
                point2D_1.X *= range_difference; 
                point2D_2.X *= range_difference;
                Geometry.Planar.Vector2D vector2D = new Geometry.Planar.Vector2D(point2D_2, point2D_1);
                int angle = System.Convert.ToInt32((vector2D.Angle(Geometry.Planar.Vector2D.WorldX)) * 180 / System.Math.PI);
                string label = " Relative Humidity φ";
                series.SmartLabelStyle.Enabled = false;
                if (i == 5)
                {
                    string newLabel = "";
                    if (!mollierControlSettings.DisableUnits)
                    {
                        newLabel += unit;
                    }
                    if (!mollierControlSettings.DisableLabels)
                    {
                        newLabel += label;
                    }
                    series.Points[count - index_Point - i].Label = newLabel;
                }
                else if (!mollierControlSettings.DisableUnits) 
                {
                    series.Points[count - index_Point - i].Label = unit;
                }
                series.Points[count - index_Point - i].LabelAngle = -angle;
                series.Points[count - index_Point - i].LabelForeColor = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Unit, ChartDataType.RelativeHumidity);
                //series.Points[count - index_Point - i].LabelAngle = -(System.Convert.ToInt32(angle * 180 / System.Math.PI) - 22);
                if (adjust_RH == true)
                    temperature_Min = -20;
                relative_humidity += 10;
            }
            int list_size = humidity_ratio_points.Count;
            for (int i = 0; i < list_size; i++)
            {
                string unit_1 = (i - 25).ToString();
                Series series_1 = MollierChart.Series.Add(unit_1);
                series_1.IsVisibleInLegend = false;
                series_1.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color,  ChartParameterType.Line, ChartDataType.DiagramTemperature);
                series_1.ChartType = SeriesChartType.Spline;
                List<Geometry.Planar.Point2D> point2Ds_humidity = new List<Geometry.Planar.Point2D>();
                point2Ds_humidity = humidity_ratio_points[i];
                for (int j = 0; j < point2Ds_humidity.Count; j++)
                {
                    series_1.Points.AddXY(point2Ds_humidity[j].X, point2Ds_humidity[j].Y);
                }
                if (i % 5 == 0)//bolds every 5th line 
                {
                    series_1.BorderWidth = 2;
                    series_1.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.BoldLine, ChartDataType.DiagramTemperature);
                }
            }
        }
        private void create_relative_humidity_line_Psychrometric(int temperature_Min, int temperature_Max, double relative_humidity, double pressure)
        {
            for (int i = 1; i <= 10; i++)
            {
                string unit = (i * 10).ToString() + '%';
                Series series = MollierChart.Series.Add(unit);
                series.IsVisibleInLegend = false;
                series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Line, ChartDataType.RelativeHumidity);
                series.ChartType = SeriesChartType.Spline;
                List<Geometry.Planar.Point2D> relative_humidity_points = new List<Geometry.Planar.Point2D>();
                for (int j = temperature_Min; j <= temperature_Max; j++)
                {
                    double humidity_ratio = Mollier.Query.HumidityRatio(j, relative_humidity, pressure);
                    relative_humidity_points.Add(new Geometry.Planar.Point2D(j, humidity_ratio));

                }
                foreach (Geometry.Planar.Point2D point2D in relative_humidity_points) 
                {
                    series.Points.AddXY(point2D.X, point2D.Y);
                    if (i == 10)
                    {
                        series.BorderWidth = 3;
                    }
                }
                int index_Point = 5;
                int count = relative_humidity_points.Count;
                if ((count - (index_Point + 1) - i < 0) || (count - (index_Point - 1) - i < 0))
                {
                    relative_humidity += 10;
                    continue;
                }
                double range_difference = (mollierControlSettings.Temperature_Max - mollierControlSettings.Temperature_Min) / (mollierControlSettings.HumidityRatio_Max - mollierControlSettings.HumidityRatio_Min);
                Geometry.Planar.Point2D point2D_1 = relative_humidity_points[count - (index_Point - 1) - i];
                Geometry.Planar.Point2D point2D_2 = relative_humidity_points[count - (index_Point + 1) - i];
                point2D_2.X = 2 * point2D_2.X - point2D_1.X;
                point2D_2.Y *= 1000 * range_difference;
                point2D_1.Y *= 1000 * range_difference;
                Geometry.Planar.Vector2D vector2D = new Geometry.Planar.Vector2D(point2D_1, point2D_2);

                int angle = System.Convert.ToInt32(vector2D.Angle(SAM.Geometry.Planar.Vector2D.WorldX) * 180/System.Math.PI);
                string label = " Relative Humidity φ";
                series.SmartLabelStyle.Enabled = false;
                if (i == 5)
                {
                    string newLabel = "";
                    if (!mollierControlSettings.DisableUnits)
                    {
                        newLabel += unit;
                    }
                    if (!mollierControlSettings.DisableLabels)
                    {
                        newLabel += label;
                    }
                    series.Points[count - index_Point - i].Label = newLabel;
                }
                else if (!mollierControlSettings.DisableUnits)
                {
                    series.Points[count - index_Point - i].Label = unit;
                }
                series.Points[count - index_Point - i].LabelForeColor = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Unit, ChartDataType.RelativeHumidity);
                series.Points[count - index_Point - i].LabelAngle = angle - 180;
                relative_humidity += 10;
            }
        }

        private void create_density_line(ChartType chartType, double density_Min, double density_Max, double pressure)
        {
            Dictionary<double, List<MollierPoint>> dictionary = GetMollierPoints_Density(density_Min, density_Max, pressure);
            if(dictionary == null)
            {
                return;
            }
            List<Series> series = CreateSeries(dictionary, chartType, ChartDataType.Density, "kg / m³", "Density");

            Series series_Temp = series?.Find(x => x.Name.Contains((1.2).ToString()));
            if (series_Temp != null)
            {
                double X = series_Temp.Points[0].XValue;
                double Y = series_Temp.Points[0].YValues[0];
                int angle = findAngle(series_Temp, chartType);
                create_moved_label(chartType, X, Y, angle, angle, 2, -0.5, -0.5, 0.005, "Density ρ [kg/m³]", ChartDataType.Density, ChartParameterType.Label, mollierControlSettings.DisableLabels);
            }
        }
        private Dictionary<double, List<MollierPoint>> GetMollierPoints_Density(double density_Min, double density_Max, double pressure)
        {
            Dictionary<double, List<MollierPoint>> result = new Dictionary<double, List<MollierPoint>>();
            while (density_Min <= density_Max)
            {
                result[density_Min] = new List<MollierPoint>();
                double temperature_1 = Mollier.Query.DryBulbTemperature_ByDensityAndRelativeHumidity(density_Min, 0, pressure);
                double humidityRatio_1 = Mollier.Query.HumidityRatio(temperature_1, 0, pressure);
                MollierPoint mollierPoint_1 = new MollierPoint(temperature_1, humidityRatio_1, pressure);
                result[density_Min].Add(mollierPoint_1);

                double temperature_2 = Mollier.Query.DryBulbTemperature_ByDensityAndRelativeHumidity(density_Min, 100, pressure);
                double humidityRatio_2 = Mollier.Query.HumidityRatio(temperature_2, 100, pressure);
                MollierPoint mollierPoint_2 = new MollierPoint(temperature_2, humidityRatio_2, pressure);
                result[density_Min].Add(mollierPoint_2);
                double a = (temperature_1 - temperature_2)/(humidityRatio_1 - humidityRatio_2);
                density_Min += 0.02;
            }
            return result;
        }

        private void create_enthalpy_line(ChartType chartType, double enthalpy_Min, double enthalpy_Max, double pressure)
        {
            Dictionary<double, List<MollierPoint>> dictionary = GetMollierPoints_Enthalpy(enthalpy_Min, enthalpy_Max, chartType, pressure);
            if(dictionary == null)
            {
                return;
            }
            List<Series> series = CreateSeries(dictionary, chartType, ChartDataType.Enthalpy, "kJ / kg", "Enthalpy");

            Series series_Temp = series?.Find(x => x.Name.Contains((50).ToString()));
            if (series_Temp != null)
            {
                double X = series_Temp.Points.Last().XValue;
                double Y = series_Temp.Points.Last().YValues[0];
                create_moved_label(chartType, X, Y, 0, 0, 1.75, -2.5, -2, 0.0007, "Enthalpy  h [kJ/kg]", ChartDataType.Enthalpy, ChartParameterType.Label, mollierControlSettings.DisableLabels);
            }
        }
        private Dictionary<double, List<MollierPoint>> GetMollierPoints_Enthalpy(double enthalpy_Min, double enthalpy_Max, ChartType chartType, double pressure)
        {   
            Dictionary<double, List<MollierPoint>> result = new Dictionary<double, List<MollierPoint>>();

            while (enthalpy_Min <= enthalpy_Max)
            {
                result[enthalpy_Min] = new List<MollierPoint>();
                double humidityRatio_Min = Mollier.Query.HumidityRatio_ByEnthalpy(-20, enthalpy_Min * 1000);

                double humidityRatio_1 = Mollier.Query.HumidityRatio_ByEnthalpy(100, enthalpy_Min * 1000);
                double temperature_1 = Mollier.Query.DryBulbTemperature(enthalpy_Min * 1000, humidityRatio_1);
                double temperature_2 = Mollier.Query.DryBulbTemperature_ByEnthalpy(enthalpy_Min * 1000, 100, pressure);
                double humidityRatio_2 = Mollier.Query.HumidityRatio(temperature_2, 100, pressure);

                MollierPoint mollierPoint_1 = new MollierPoint(temperature_1, humidityRatio_1, pressure);
                result[enthalpy_Min].Add(mollierPoint_1);

                if(enthalpy_Min % 10 == 0)
                {
                    Geometry.Planar.Point2D Point_1 = new Geometry.Planar.Point2D(humidityRatio_1,temperature_1);
                    Geometry.Planar.Point2D Point_2 = new Geometry.Planar.Point2D(humidityRatio_2,temperature_2);
                    Math.PolynomialEquation polynomialEquation = SAM.Geometry.Create.PolynomialEquation(new Geometry.Planar.Point2D[] { Point_2, Point_1 });
                    double a = chartType == ChartType.Mollier ? 0.0015 : 0.0006;
                    temperature_2 = polynomialEquation.Evaluate(Point_2.X + a);

                    MollierPoint mollierPoint_2 = new MollierPoint(temperature_2, Point_2.X + a, pressure);
                    result[enthalpy_Min].Add(mollierPoint_2);
                }
                else
                {
                    MollierPoint mollierPoint_2 = new MollierPoint(temperature_2, humidityRatio_2, pressure);
                    result[enthalpy_Min].Add(mollierPoint_2);
                }

                enthalpy_Min += 1;
            }
            return result;
        }  

        private void create_Wet_Bulb_Temperature_line(ChartType chartType, double temperature_Max, double wetBulbTemperature_Min, double wetBulbTemperature_Max, double pressure)
        {
            Dictionary<double, List<MollierPoint>> dictionary = GetMollierPoints_WetBulbTemperature(wetBulbTemperature_Min, wetBulbTemperature_Max, temperature_Max, pressure);
            if(dictionary == null)
            {
                return;
            }
            List<Series> series = CreateSeries(dictionary, chartType, ChartDataType.WetBulbTemperature, "°C", "Wet Bulb Temperature");

            Series series_Temp = series?.Find(x => x.Name.Contains((15).ToString()));
            if (series_Temp != null)
            {
                double X = series_Temp.Points.Last().XValue;
                double Y = series_Temp.Points.Last().YValues[0];
                int angle = findAngle(series_Temp, chartType);
                create_moved_label(chartType, X, Y, angle, angle, -1.2, 3.2, 4.5, -0.0018, "Wet Bulb Temperature t_wb [°C]", ChartDataType.WetBulbTemperature, ChartParameterType.Label, mollierControlSettings.DisableLabels);
            }
        }
        private Dictionary<double, List<MollierPoint>> GetMollierPoints_WetBulbTemperature(double wetBulbTemperature_Min, double wetBulbTemperature_Max, double temperature_Max, double pressure)
        {
            Dictionary<double, List<MollierPoint>> result = new Dictionary<double, List<MollierPoint>>(); 
            while(wetBulbTemperature_Min <= wetBulbTemperature_Max)


            {
                result[wetBulbTemperature_Min] = new List<MollierPoint>();
                double temperature_1 = Mollier.Query.DryBulbTemperature_ByWetBulbTemperature(wetBulbTemperature_Min, 0, pressure);
                double humidityRatio_1 = Mollier.Query.HumidityRatio(temperature_1, 0, pressure);
                MollierPoint mollierPoint_1 = new MollierPoint(temperature_1, humidityRatio_1, pressure);
                result[wetBulbTemperature_Min].Add(mollierPoint_1);

                double temperature_2 = Mollier.Query.DryBulbTemperature_ByWetBulbTemperature(wetBulbTemperature_Min, 100, pressure);
                double humidityRatio_2 = Mollier.Query.HumidityRatio(temperature_2, 100, pressure);
                MollierPoint mollierPoint_2 = new MollierPoint(temperature_2, humidityRatio_2, pressure);
                result[wetBulbTemperature_Min].Add(mollierPoint_2);

                wetBulbTemperature_Min += 5;
            }
            return result;
        }
   
        private void create_specific_volume_line(ChartType chartType, double specific_volume_Min, double specific_volume_Max, double pressure)
        {
            Dictionary<double, List<MollierPoint>> dictionary = GetMollierPoints_SpecificVolume(specific_volume_Min, specific_volume_Max, pressure);
            if(dictionary == null)
            {
                return;
            }

            List<Series> series = CreateSeries(dictionary, chartType, ChartDataType.SpecificVolume, "kg/m³", "specific volume");

            Series series_Temp = series?.Find(x => x.Name.Contains((0.9).ToString()));
            if (series_Temp != null)
            {
                double X = series_Temp.Points.Last().XValue;
                double Y = series_Temp.Points.Last().YValues[0];
                int angle = findAngle(series_Temp, chartType);
                create_moved_label(chartType, X, Y, angle, angle, -2, 0.6, 2.5, -0.005, "Specific volume v [kg/m³]", ChartDataType.SpecificVolume, ChartParameterType.Label, mollierControlSettings.DisableLabels);
            }
        }
        private Dictionary<double, List<MollierPoint>> GetMollierPoints_SpecificVolume(double specific_volume_Min, double specific_volume_Max, double pressure)
        {
            Dictionary<double, List<MollierPoint>> result = new Dictionary<double, List<MollierPoint>>();

            while (specific_volume_Min <= specific_volume_Max)
            {
                result[specific_volume_Min] = new List<MollierPoint>();

                MollierPoint mollierPoint_1 = Create.MollierPoint_ByRelativeHumidityAndSpecificVolume(0, specific_volume_Min, pressure);
                result[specific_volume_Min].Add(mollierPoint_1);
                MollierPoint mollierPoint_2 = Create.MollierPoint_ByRelativeHumidityAndSpecificVolume(100, specific_volume_Min, pressure);
                result[specific_volume_Min].Add(mollierPoint_2);

                specific_volume_Min += 0.05;
            }

            return result;
        }
      

        private int findAngle(Series series, ChartType chartType)
        {
            //takes series (line must be straight) and chartType and returns angle of label along the line

            double range_difference = (mollierControlSettings.Temperature_Max - mollierControlSettings.Temperature_Min) / (mollierControlSettings.HumidityRatio_Max - mollierControlSettings.HumidityRatio_Min);
            Geometry.Planar.Point2D a = new Geometry.Planar.Point2D(); a.X = series.Points[0].XValue; a.Y = series.Points[0].YValues[0];
            Geometry.Planar.Point2D b = new Geometry.Planar.Point2D(); b.X = series.Points[1].XValue * range_difference; b.Y = series.Points[1].YValues[0];
            
            a.X = series.Points[0].XValue;
            a.Y = chartType == ChartType.Mollier ? series.Points[0].YValues[0] : series.Points[0].YValues[0] * 1000;
            b.X = chartType == ChartType.Mollier ? series.Points[1].XValue * range_difference * 2 : 2 * series.Points[1].XValue - a.X;
            b.Y = chartType == ChartType.Mollier ? series.Points[1].YValues[0] : series.Points[1].YValues[0] * 1000 * range_difference;
            
            Geometry.Planar.Vector2D vector = chartType == ChartType.Mollier ? new Geometry.Planar.Vector2D(a, b) : new Geometry.Planar.Vector2D(a,b);
            int result = System.Convert.ToInt32(vector.Angle(Geometry.Planar.Vector2D.WorldX) * 180 / System.Math.PI);

            return chartType == ChartType.Mollier ? result : 180 - result;

        }
        private void create_moved_label(ChartType chartType, double X, double Y, int Mollier_angle, int Psychrometric_angle, double Mollier_X, double Mollier_Y, double Psychrometric_X, double Psychrometric_Y, string LabelContent, ChartDataType chartDataType, ChartParameterType chartParameterType, bool IsDisabled = false)
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
            new_label.SmartLabelStyle.Enabled = false;
            new_label.Points.AddXY(X + x, Y + y);
            new_label.Label = LabelContent;
            new_label.LabelAngle = chartType == ChartType.Mollier ? Mollier_angle : Psychrometric_angle;
            //if (color == null)
            //    return;
            new_label.LabelForeColor = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, chartParameterType, chartDataType);
        }
        private void createLabels(ChartType chartType, string name, ChartDataType chartDataType, Series series, double value)
        {
            double X, Y;
            int angle = 0;
            switch (name)
            {
                case "specific volume":
                    X = series.Points[1].XValue;
                    Y = series.Points[1].YValues[0];
                    series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Line, chartDataType);
                    angle = findAngle(series, chartType);
                    create_moved_label(chartType, X, Y, angle, angle, -0.5, 0, 0.7, -0.0015, value.ToString(), chartDataType, ChartParameterType.Unit, mollierControlSettings.DisableUnits);
                    break;

                case "Wet Bulb Temperature":
                    X = series.Points[1].XValue;
                    Y = series.Points[1].YValues[0];
                    series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Line, chartDataType);
                    create_moved_label(chartType, X, Y, 0, 0, 0, -1.8, -0.45, -0.00035, value.ToString(), chartDataType, ChartParameterType.Unit, mollierControlSettings.DisableUnits);
                    break;
                case "Density":
                    X = series.Points[0].XValue;
                    Y = series.Points[0].YValues[0];
                    series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Line, chartDataType);
                    series.BorderDashStyle = ChartDashStyle.DashDotDot;
                    angle = findAngle(series, chartType);
                    create_moved_label(chartType, X, Y, angle, angle, 0.3, -0.2, 0.2, 0.0002, value.ToString(), chartDataType, ChartParameterType.Unit, mollierControlSettings.DisableUnits);
                    break;
                case "Enthalpy":
                    X = series.Points[1].XValue;
                    Y = series.Points[1].YValues[0];
                    series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Line, chartDataType);
                    if (value % 10 == 0)
                    {
                        series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.BoldLine, chartDataType);
                        if (chartType == ChartType.Mollier)
                            create_moved_label(chartType, X, Y, 0, 0, 0, -1.8, 0, 0, value.ToString(), chartDataType, ChartParameterType.Unit, mollierControlSettings.DisableUnits);
                        else
                            create_moved_label(chartType, X, Y, 0, 0, 0, 0, 0, -0.0002, value.ToString(), chartDataType, ChartParameterType.Unit, mollierControlSettings.DisableUnits);
                    }
                    break;
            }
        }
        private List<Series> CreateSeries(Dictionary<double, List<MollierPoint>> dictionary, ChartType chartType, ChartDataType chartDataType, string unit, string prefix)
        {
            List<Series> result = new List<Series>();
            foreach (KeyValuePair<double, List<MollierPoint>> keyValuePair in dictionary)
            {
                Series series = CreateSeries(keyValuePair.Value, chartType, chartDataType, keyValuePair.Key, unit, prefix);
                if(series != null)
                {
                    result.Add(series);
                }
            }

            return result;
        }
        private Series CreateSeries(List<MollierPoint> mollierPoints, ChartType chartType, ChartDataType chartDataType, double value, string unit, string prefix)
        {
            Series result = MollierChart.Series.Add(String.Format("{0} {1} {2}", prefix, value, unit));
            result.ChartType = SeriesChartType.Spline;
            result.IsVisibleInLegend = false;

            foreach(MollierPoint mollierPoint in mollierPoints)
            {
                double temperature = mollierPoint.DryBulbTemperature;
                double humidityRatio = mollierPoint.HumidityRatio;

                if(chartType == ChartType.Mollier)
                {
                    temperature = Mollier.Query.DiagramTemperature(mollierPoint);
                    humidityRatio = humidityRatio * 1000;
                    result.Points.AddXY(humidityRatio, temperature);
                }
                else
                {
                    result.Points.AddXY(temperature, humidityRatio);
                }
            }
            createLabels(chartType,prefix, chartDataType, result, value);
            return result;
        }
        private void add_MollierPoints(ChartType chartType)
        {
            Series series = MollierChart.Series.Add(System.Guid.NewGuid().ToString());
            series.IsVisibleInLegend = false;
            series.ChartType = SeriesChartType.Point;
            series.Tag = mollierPoints;

            Dictionary<MollierPoint, int> dictionary = new Dictionary<MollierPoint, int>();
            double MaxCount = 0;
            //if gradient point is on then color by their intensity, otherwise color with normal color saved in visibilitysettings
            if (mollierControlSettings.GradientPoint)
            {
                dictionary = Query.NeighborhoodCount(mollierPoints, out MaxCount);
            }
            else
            {
                series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Point, ChartDataType.Undefined, mollierControlSettings.GradientPoint);
            }
            //add points to the chart
            foreach (MollierPoint point in mollierPoints)
            {
                double humidity_ratio = point.HumidityRatio;
                double DryBulbTemperature = point.DryBulbTemperature;
                double diagram_temperature = Mollier.Query.DiagramTemperature(point);
                int index = chartType == ChartType.Mollier ? series.Points.AddXY(humidity_ratio * 1000, diagram_temperature) : series.Points.AddXY(DryBulbTemperature, humidity_ratio);
                //if gradient point is on then set a gradient point color with earlier counted intensity
                if (mollierControlSettings.GradientPoint)
                {
                    double value = MaxCount == 0 ? 0 : System.Convert.ToDouble(dictionary[point]) / MaxCount;
                    series.Points[index].Color = Core.Query.Lerp(mollierControlSettings.GradientColors.Color, mollierControlSettings.GradientColors.GradientColor, value);
                }
                series.Points[index].ToolTip = ToolTip(point, chartType);
                series.Points[index].Tag = point;
            }
        }
        private void add_Processes(ChartType chartType)
        {
            foreach (IMollierProcess mollierProcess in mollierProcesses)
            {

                Series series = MollierChart.Series.Add(System.Guid.NewGuid().ToString());
                series.IsVisibleInLegend = false;
                series.ChartType = SeriesChartType.Line;
                series.BorderWidth = 5;
                series.Color = mollierControlSettings.VisibilitySettings.GetColor(mollierControlSettings.Color, ChartParameterType.Line, mollierProcess);

                MollierPoint start = mollierProcess?.Start;
                MollierPoint end = mollierProcess?.End;
                if (start == null || end == null)
                {
                    continue;
                }

                int index;

                index = chartType == ChartType.Mollier ? series.Points.AddXY(start.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(start)) : series.Points.AddXY(start.DryBulbTemperature, start.HumidityRatio);
                series.Points[index].ToolTip = ToolTip(start, chartType);
                series.Points[index].Tag = start;
                index = chartType == ChartType.Mollier ? series.Points.AddXY(end.HumidityRatio * 1000, Mollier.Query.DiagramTemperature(end)) : series.Points.AddXY(end.DryBulbTemperature, end.HumidityRatio); ;
                series.Points[index].ToolTip = ToolTip(end, chartType);
                series.Points[index].Tag = end;

                MollierPoint mid = new MollierPoint((start.DryBulbTemperature + end.DryBulbTemperature) / 2, (start.HumidityRatio + end.HumidityRatio) / 2, mollierControlSettings.Pressure);
                string process = "";
                if (mollierProcess is HeatingProcess)
                {
                    process = "Heating";
                }
                if(mollierProcess is CoolingProcess)
                {
                    process = "Cooling";
                }
                if(mollierProcess is MixingProcess)
                {
                    process = "Mixing";
                }
                if(mollierProcess is SteamHumidificationProcess)
                {
                    process = "Humidify";
                }
                if(mollierProcess is HeatRecoveryProcess)
                {
                    process = "Heat Recovery";
                }
                double distance = process == "Heat Recovery" ? 1.5 : 1;//heat recovery is longer so it needs more space
                create_moved_label(chartType, start.HumidityRatio * 1000, start.DryBulbTemperature, 0, 0, 0.5, 0, 0, 0, "P1", ChartDataType.Undefined, ChartParameterType.Point);
                create_moved_label(chartType, mid.HumidityRatio * 1000, mid.DryBulbTemperature, 0, 0, distance, 0, 0, 0, process, ChartDataType.Undefined, ChartParameterType.Point);
                create_moved_label(chartType, end.HumidityRatio * 1000, end.DryBulbTemperature, 0, 0, 0.5, 0, 0, 0, "P2", ChartDataType.Undefined, ChartParameterType.Point);
                series.Tag = mollierProcess;
                series.ToolTip = ToolTip(mollierProcess);
            }
        }
        public void CreateYAxis(Chart chart, ChartArea area, Series series, float axisX, float axisWidth, float labelsSize, bool alignLeft, double P_w_Min, double P_w_Max)
        {

            chart.ApplyPaletteColors();  // (*)
            long x = System.DateTime.Now.Ticks;
  
            // Create new chart area for original series
            ChartArea areaSeries = chart.ChartAreas.Add("Psychrometric_P_w" + x.ToString());
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
            ChartArea areaAxis = chart.ChartAreas.Add("Psychrometric_P_w_copy" + x.ToString());

            areaAxis.BackColor = Color.Transparent;
            areaAxis.BorderColor = Color.Transparent;
            RectangleF oRect = area.Position.ToRectangleF();
            areaAxis.Position = new ElementPosition(oRect.X, oRect.Y, axisWidth, oRect.Height);
            areaAxis.InnerPlotPosition
                    .FromRectangleF(areaSeries.InnerPlotPosition.ToRectangleF());

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
            areaAxis.AxisX.MajorGrid.Enabled = false;
            areaAxis.AxisX.MajorTickMark.Enabled = false;
            areaAxis.AxisX.LabelStyle.Enabled = false;

            Axis areaAxisAxisY = alignLeft ? areaAxis.AxisY : areaAxis.AxisY2;   // (**)
            areaAxisAxisY.MajorGrid.Enabled = false;
            areaAxisAxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
            areaAxisAxisY.LabelStyle.Font = area.AxisY.LabelStyle.Font;
            areaAxisAxisY.Minimum = System.Math.Round(P_w_Min, 4);
            areaAxisAxisY.Maximum = System.Math.Round(P_w_Max, 4);
            areaAxisAxisY.Interval = mollierControlSettings.P_w_Interval / 1000;
            areaAxisAxisY.Title = "P_w  x [kPa]";

            // Adjust area position
            areaAxis.Position.X = axisX;
            areaAxis.InnerPlotPosition.X += labelsSize;
        }
        public void CreateXAxis(Chart chart, ChartArea area, Series series, float axisY, float axisHeight, float labelsSize, bool alignLeft, double P_w_Min, double P_w_Max)
        {
            long x = System.DateTime.Now.Ticks;

            chart.ApplyPaletteColors();  // (*)
            // Create new chart area for original series
            ChartArea areaSeries = chart.ChartAreas.Add("Mollier P_w" + x.ToString());
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
            ChartArea areaAxis = chart.ChartAreas.Add("Mollier P_w_copy" + x.ToString());

            areaAxis.BackColor = Color.Transparent;
            areaAxis.BorderColor = Color.Transparent;
            RectangleF oRect = area.Position.ToRectangleF();
            areaAxis.Position = new ElementPosition(oRect.X, oRect.Y, oRect.Width, axisHeight);
            areaAxis.InnerPlotPosition
                    .FromRectangleF(areaSeries.InnerPlotPosition.ToRectangleF());

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
            areaAxis.AxisY.MajorGrid.Enabled = false;
            areaAxis.AxisY.MajorTickMark.Enabled = false;
            areaAxis.AxisY.LabelStyle.Enabled = false;
            Axis areaAxisAxisX = alignLeft ? areaAxis.AxisX : areaAxis.AxisX2;   // (**)
            areaAxisAxisX.MajorGrid.Enabled = false;
            areaAxisAxisX.Minimum = System.Math.Round(P_w_Min,2);
            areaAxisAxisX.Maximum = System.Math.Round(P_w_Max,2);
            areaAxisAxisX.LabelStyle.Font = area.AxisX.LabelStyle.Font;
            areaAxisAxisX.Interval = mollierControlSettings.P_w_Interval;


            areaAxisAxisX.Title = series.Name;
            //areaAxisAxisX.LineColor = series.Color;    // (*)
            //areaAxisAxisX.TitleForeColor = Color.DarkCyan;  // (*)

            // Adjust area position
            areaAxis.Position.Y = axisY;
            areaAxis.InnerPlotPosition.Y += labelsSize;
        }
        private void generate_graph()
        {
            if(mollierControlSettings.ChartType == ChartType.Mollier)
            {
                generate_graph_mollier();
            }
            else if (mollierControlSettings.ChartType == ChartType.Psychrometric)
            {
                generate_graph_psychrometric();
            }
        }
        private void generate_graph_mollier()
        {
            
            //INITIAL SIZES
            double pressure = mollierControlSettings.Pressure;
            double humidityRatio_Min = mollierControlSettings.HumidityRatio_Min;
            double humidityRatio_Max = mollierControlSettings.HumidityRatio_Max;
            double humidityRatio_interval = mollierControlSettings.HumidityRatio_Interval;
            double temperature_Min = mollierControlSettings.Temperature_Min;
            double temperature_Max = mollierControlSettings.Temperature_Max;
            double temperature_interval = mollierControlSettings.Temperature_Interval;
            bool density_line = mollierControlSettings.Density_line;
            bool enthalpy_line = mollierControlSettings.Enthalpy_line;
            bool specific_volume_line = mollierControlSettings.SpecificVolume_line;
            bool wet_bulb_temperature_line = mollierControlSettings.WetBulbTemperature_line;
            ChartType chartType = mollierControlSettings.ChartType;
            int wetBulbTemperature_Min = -10;
            int wetBulbTemperature_Max = 30;
            double density_Min = 0.96;
            double density_Max = 1.41;
            int enthalpy_Min = -20;
            int enthalpy_Max = 140;
            double specific_volume_Min = 0.75;
            double specific_volume_Max = 0.95;
            double relative_humidity = 10;
            //checking whether creating a new graph has sense with this pressure
            if (MinPressure > pressure  || pressure > MaxPressure)
            {
                return;
            }
            //BASE CHART INITIALIZATION
            MollierChart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            MollierChart.Series.Clear();
            ChartArea chartArea = MollierChart.ChartAreas[0];
            ChartArea ca = MollierChart.ChartAreas["ChartArea1"];
            ca.Position = new ElementPosition(2, 2, 95, 95);//define sizes of chart
            ca.InnerPlotPosition = new ElementPosition(7, 6, 88, 88);
            double P_w_max = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Max / 1000, pressure) / 1000;
            double P_w_min = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Min / 1000, pressure) / 1000;

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
            //AXIS Y
            MollierChart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.False;
            Axis axisY = chartArea.AxisY;
            axisY.Enabled = AxisEnabled.True;
            axisY.Title = "Dry Bulb Temperature t [°C]";
            axisY.TextOrientation = TextOrientation.Rotated270;
            axisY.Maximum = temperature_Max;
            axisY.Minimum = temperature_Min;
            axisY.Interval = temperature_interval;
            axisY.LabelStyle.Format = "0.##";
            //CREATING RELATIVE HUMIDITY AND HUMIDITY RATIO LINES
            create_relative_humidity_line_Mollier(System.Convert.ToInt32(temperature_Min), System.Convert.ToInt32(temperature_Max), relative_humidity, pressure);
            //CREATING DENSITY LINE
            if (density_line)
                create_density_line(ChartType.Mollier, density_Min, density_Max, pressure);
            //CREATING ENTHALPY LINE
            if (enthalpy_line)
                create_enthalpy_line(ChartType.Mollier, enthalpy_Min, enthalpy_Max, pressure);
            //CREATETING WET BULB TEMPERATURE LINE
            if (wet_bulb_temperature_line)
                create_Wet_Bulb_Temperature_line(ChartType.Mollier, temperature_Max, wetBulbTemperature_Min, wetBulbTemperature_Max, pressure);
            //CREATING SPECIFIC VOLUME LINE
            if (specific_volume_line)
                create_specific_volume_line(ChartType.Mollier, specific_volume_Min, specific_volume_Max, pressure);
            //CREATING P_w AXIS
            Series series1 = MollierChart.Series.Add("P_w  x [kPa]");
            series1.Points.AddXY(P_w_min, 0);
            series1.Points.AddXY(P_w_max, 0);
            series1.ChartType = SeriesChartType.Spline;
            series1.Color = Color.Transparent;
            series1.BorderColor = Color.Transparent;
            series1.IsVisibleInLegend = false;
            CreateXAxis(MollierChart, ca, series1, 2, 80, 1, false, P_w_min, P_w_max);

            if (mollierPoints != null)
            {
                add_MollierPoints(chartType);
            }
            if (mollierProcesses != null)
            {
                add_Processes(chartType);
            }

        }
        private void generate_graph_psychrometric()
        {
            //INITIAL SIZES
            int wetBulbTemperature_Min = -10;
            int wetBulbTemperature_Max = 30;
            double density_Min = 0.96;
            double density_Max = 1.41;
            int enthalpy_Min = -20;
            int enthalpy_Max = 140;
            double specific_volume_Min = 0.75;
            double specific_volume_Max = 0.95;
            double relative_humidity = 10;
            double pressure = mollierControlSettings.Pressure;
            double humidityRatio_Min = mollierControlSettings.HumidityRatio_Min;
            double humidityRatio_Max = mollierControlSettings.HumidityRatio_Max;
            double humidityRatio_interval = mollierControlSettings.HumidityRatio_Interval;
            double temperature_Min = mollierControlSettings.Temperature_Min;
            double temperature_Max = mollierControlSettings.Temperature_Max;
            double temperature_interval = mollierControlSettings.Temperature_Interval;
            bool density_line = mollierControlSettings.Density_line;
            bool enthalpy_line = mollierControlSettings.Enthalpy_line;
            bool specific_volume_line = mollierControlSettings.SpecificVolume_line;
            bool wet_bulb_temperature_line = mollierControlSettings.WetBulbTemperature_line;
            //checking whether creating a new graph has sense with this pressure
            if (MinPressure > pressure || pressure > MaxPressure)
            {
                return;
            }
            ChartType chartType = mollierControlSettings.ChartType;

            //BASE CHART INITIALIZATION
            MollierChart.Series.Clear();
            ChartArea chartArea = MollierChart.ChartAreas[0];
            MollierChart.ChartAreas[0].AxisX2.Enabled = AxisEnabled.False;
            MollierChart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            MollierChart.ChartAreas[0].AxisY2.Title = "Humidity Ratio  x [kg/kg]";
            MollierChart.ChartAreas[0].AxisY2.Maximum = humidityRatio_Max / 1000;
            MollierChart.ChartAreas[0].AxisY2.Minimum = humidityRatio_Min / 1000;
            MollierChart.ChartAreas[0].AxisY2.Interval = humidityRatio_interval / 1000;
            MollierChart.ChartAreas[0].AxisY2.MajorGrid.LineColor = Color.Gray;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.Interval = 0.001;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.Enabled = true;
            MollierChart.ChartAreas[0].AxisY2.MinorGrid.LineColor = Color.LightGray;
            MollierChart.ChartAreas[0].AxisY2.LabelStyle.Format = "0.###";
            double P_w_Min = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Min / 1000, pressure) / 1000000;
            double P_w_Max = Mollier.Query.PartialVapourPressure_ByHumidityRatio(humidityRatio_Max / 1000, pressure) / 1000000;
            //AXIS X
            Axis axisX = chartArea.AxisX;
            axisX.Title = "Dry Bulb Temperature t [°C]";
            axisX.Maximum = temperature_Max;
            axisX.Minimum = temperature_Min;
            axisX.Interval = temperature_interval;
            axisX.MajorGrid.LineColor = Color.Gray;
            axisX.MinorGrid.Interval = 1;
            axisX.MinorGrid.Enabled = true;
            axisX.MinorGrid.LineColor = Color.LightGray;
            //AXIS Y - P_w AXIS
            Axis axisY = chartArea.AxisY;
            axisY.Enabled = AxisEnabled.False;
            axisY.Title = "P_w  x [ kPa ]";
            axisY.TextOrientation = TextOrientation.Rotated270;
            axisY.Maximum = humidityRatio_Max/1000;
            axisY.Minimum = humidityRatio_Min/ 1000;
            axisY.Interval = humidityRatio_interval/1000;
            axisY.MajorGrid.Enabled = false;
            axisY.MajorGrid.LineColor = Color.Gray;
            axisY.MinorGrid.Interval = 0.001;
            axisY.MinorGrid.Enabled = false;
            axisY.MinorGrid.LineColor = Color.LightGray;

            ChartArea ca = MollierChart.ChartAreas[0];
            ca.Position = new ElementPosition(2, 2, 95, 95);//define sizes of chart
            ca.InnerPlotPosition = new ElementPosition(8, 6, 85, 85);

            Series series1 = MollierChart.Series.Add("P_w_Psychro x [kPa]");
            series1.Points.AddXY(0,P_w_Min);
            series1.Points.AddXY(0,P_w_Max);
            series1.ChartType = SeriesChartType.Spline;
            series1.Color = Color.Transparent;
            series1.BorderColor = Color.Transparent;
            series1.IsVisibleInLegend = false;
            CreateYAxis(MollierChart, ca, series1, 5, 12, 25, true, P_w_Min, P_w_Max);


            //CREATING RELATIVE HUMIDITY LINES
            create_relative_humidity_line_Psychrometric(System.Convert.ToInt32(temperature_Min), System.Convert.ToInt32(temperature_Max), relative_humidity, pressure);
            //CREATING DENSITY LINE
            if (density_line)
                create_density_line(ChartType.Psychrometric, density_Min, density_Max, pressure);
            //CREATING ENTHALPY LINE
            if (enthalpy_line)
                create_enthalpy_line(ChartType.Psychrometric, enthalpy_Min, enthalpy_Max, pressure);
            //CREATING WET BULB TEMPERATURE LINE
            if (wet_bulb_temperature_line)
                create_Wet_Bulb_Temperature_line(ChartType.Psychrometric, temperature_Max, wetBulbTemperature_Min, wetBulbTemperature_Max, pressure);
            //CREATING SPECIFIC VOLUME LINE
            if (specific_volume_line)
                create_specific_volume_line(ChartType.Psychrometric, specific_volume_Min, specific_volume_Max, pressure);
            
            if (mollierPoints != null)
            {
                add_MollierPoints(chartType);
            }

            if (mollierProcesses != null)
            {
                add_Processes(chartType);
            }
        }

        private string ToolTip(MollierPoint mollierPoint, ChartType chartType)
        {
            //Returns data that should be displayed after hover over the point
            if(mollierPoint == null)
            {
                return null;
            }

            string mask = "t = {3} °C\nx = {1}{2}\nφ = {0} %\nt_wb = {6} °C\nh = {5} kJ/kg\nρ = {8} kg/m³\np = {4} Pa\n𝜈 = {7} kg/m³";
            switch (chartType)
            {
                case ChartType.Psychrometric:
                    return String.Format(mask, Core.Query.Round(mollierPoint.RelativeHumidity, 0.01), Core.Query.Round(mollierPoint.HumidityRatio, Tolerance.MacroDistance), " kg/kg", Core.Query.Round(mollierPoint.DryBulbTemperature, 0.01), mollierPoint.Pressure, Core.Query.Round(mollierPoint.Enthalpy,0.01), Core.Query.Round(mollierPoint.WetBulbTemperature(), 0.01), Core.Query.Round(mollierPoint.SpecificVolume(),0.01), Core.Query.Round(mollierPoint.Density(),0.01));

                case ChartType.Mollier:
                    return String.Format(mask, Core.Query.Round(mollierPoint.RelativeHumidity, 0.01), Core.Query.Round(mollierPoint.HumidityRatio * 1000, 0.1), " g/kg", Core.Query.Round(Mollier.Query.DiagramTemperature(mollierPoint), 0.01), mollierPoint.Pressure, Core.Query.Round(mollierPoint.Enthalpy, 0.01), Core.Query.Round(mollierPoint.WetBulbTemperature(), 0.01), Core.Query.Round(mollierPoint.SpecificVolume(), 0.01), Core.Query.Round(mollierPoint.Density(),0.01));
            }
            return null;
            
        }

        private string ToolTip(IMollierProcess mollierProcess)
        {
            if (mollierProcess is HeatingProcess)
            {
                return "Heating";
            }

            if (mollierProcess is CoolingProcess)
            {
                return "Cooling";
            }

            if (mollierProcess is MixingProcess)
            {
                return "Mixing";
            }

            if (mollierProcess is HeatRecoveryProcess)
            {
                return "Heat Recovery";
            }

            if (mollierProcess is HumidificationProcess)
            {
                return "Humidification";
            }

            return null;
        }
        public List<MollierPoint> AddPoints(IEnumerable<MollierPoint> mollierPoints, bool checkPressure = true)
        {
            if (mollierPoints == null)
                return null;
            if(this.mollierPoints == null)
            {
                this.mollierPoints = new List<MollierPoint>();
            }

            List<MollierPoint> mollierPointsResult = new List<MollierPoint>();
            foreach(MollierPoint point in mollierPoints)
            {
                if (!checkPressure || Core.Query.AlmostEqual(point.Pressure, mollierControlSettings.Pressure, Tolerance.MacroDistance))
                {
                    mollierPointsResult.Add(point);
                }
            }
            this.mollierPoints.AddRange(mollierPointsResult);
            generate_graph();
            return mollierPointsResult;
        }

        public bool AddProcess(IMollierProcess mollierProcess, bool checkPressure = true)
        {
            if(mollierProcess == null)
            {
                return false;
            }
            if(mollierProcesses == null)
            {
                mollierProcesses = new List<IMollierProcess>();
            }
            if(checkPressure && !Core.Query.AlmostEqual(mollierProcess.Pressure, mollierControlSettings.Pressure, Tolerance.MacroDistance))
            {
                return false;
            }
            mollierProcesses.Add(mollierProcess);
            generate_graph();//TODO: optimalise
            return true;
        }
        public bool Clear()
        {
            mollierPoints?.Clear();
            mollierProcesses?.Clear();
            generate_graph();
            return true;
        }
        public bool Save()
        {
            string path = null;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "emf files (*.emf)|*.emf|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }
                path = saveFileDialog.FileName;
            }

            MollierChart.SaveImage(path, ImageFormat.Emf);
            return true;
        }

        public List<Series> Series
        {
            get
            {
                return MollierChart.Series?.ToList();
            }
        }

        private void ToolStripMenuItem_ProcessesAndPoints_Click(object sender, EventArgs e)
        {
            Query.ZoomParameters(MollierChart.Series, out double humidityRatio_Min, out double humidityRatio_Max, out double temperature_Min, out double temperature_Max);
            mollierControlSettings.HumidityRatio_Min = humidityRatio_Min;
            mollierControlSettings.HumidityRatio_Max = humidityRatio_Max;
            mollierControlSettings.Temperature_Min = temperature_Min;
            mollierControlSettings.Temperature_Max = temperature_Max;
            generate_graph();
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
            generate_graph();
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
            if (!selection)
            {
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                MollierChart.Refresh();
                using (Graphics g = MollierChart.CreateGraphics())
                    g.DrawRectangle(Pens.Red, GetRectangle(mdown, e.Location));
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

            double x_Min = System.Math.Min((double)ax.PixelPositionToValue(mup.X), (double)ax.PixelPositionToValue(mdown.X));
            double x_Max = System.Math.Max((double)ax.PixelPositionToValue(mup.X), (double)ax.PixelPositionToValue(mdown.X));
            double y_Min = System.Math.Min((double)ay.PixelPositionToValue(mup.Y), (double)ay.PixelPositionToValue(mdown.Y));
            double y_Max = System.Math.Max((double)ay.PixelPositionToValue(mup.Y), (double)ay.PixelPositionToValue(mdown.Y));
            double x_Difference = x_Max - x_Min;
            double y_Difference = mollierControlSettings.ChartType == ChartType.Mollier ? y_Max - y_Min : (y_Max - y_Min) *1000;
            if (x_Difference < 1 || y_Difference < 1) { 
                MollierChart.Refresh();
                return;
            }
            mollierControlSettings.HumidityRatio_Min = mollierControlSettings.ChartType == ChartType.Mollier ? x_Min : y_Min * 1000;
            mollierControlSettings.HumidityRatio_Max = mollierControlSettings.ChartType == ChartType.Mollier ? x_Max : y_Max * 1000;
            mollierControlSettings.Temperature_Min = mollierControlSettings.ChartType == ChartType.Mollier ? y_Min : x_Min;
            mollierControlSettings.Temperature_Max = mollierControlSettings.ChartType == ChartType.Mollier ? y_Max : x_Max;
            generate_graph();

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
                if(mollierControlSettings == null)
                {
                    return null;
                }

                return new MollierControlSettings(mollierControlSettings);
            }
            set
            {
                if(value == null)
                {
                    mollierControlSettings = null;
                }

                mollierControlSettings = new MollierControlSettings(value);
                generate_graph();
            }
        }


    }
}
