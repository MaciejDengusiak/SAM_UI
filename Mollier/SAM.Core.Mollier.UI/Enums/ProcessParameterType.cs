﻿using System.ComponentModel;

namespace SAM.Core.Mollier.UI
{
    public enum ProcessParameterType
    {
        [Description("Undefined")] Undefined,
        [Description("Dry Bulb Temperature")] DryBulbTemperature,
        [Description("Flow Temperature")] FlowTemperature,
        [Description("Return Temperature")] ReturnTemperature,
        [Description("Dry Bulb Temperature Difference")] DryBulbTemperatureDifference,
        [Description("Enthalpy Difference")] EnthalpyDifference,
        [Description("Humidity Ratio Difference")] HumidityRatioDifference,
        [Description("Relative Humidity")] RelativeHumidity,
        [Description("Sensible Heat Recovery Efficiency")] SensibleHeatRecoveryEfficiency,
        [Description("Latent Heat Recovery Efficiency")] LatentHeatRecoveryEfficiency,
        [Description("Airflow")] Airflow,
        [Description("Efficiency")] Efficiency,
    }
}