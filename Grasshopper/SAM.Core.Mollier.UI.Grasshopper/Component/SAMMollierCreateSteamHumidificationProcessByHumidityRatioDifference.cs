﻿using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Mollier;
using SAM.Core.Mollier.UI.Grasshopper.Properties;

namespace SAM.Core.Mollier.UI.Grasshopper
{
    [Obsolete("Obsolete since 2023.12.21")]
    public class SAMMollierCreateSteamHumidificationProcessByHumidityRatioDifference : MollierDiagramComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4f1010c4-e6a2-41c5-8fec-e729d843ddc7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.SAM_Mollier;

        //public override GH_Exposure Exposure => GH_Exposure.primary;
        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.hidden;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooMollierPointParam() { Name = "_start", NickName = "_start", Description = "Start Mollier Point for MollierProcess", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_humidityRatioDifference", NickName = "_humidityRatioDifference", Description = "Humidity Ratio Difference x [g/kg]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Colour param_Colour = null;
                param_Colour = new global::Grasshopper.Kernel.Parameters.Param_Colour() { Name = "_color_", NickName = "_color_", Description = "Colour RGB", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_Colour, ParamVisibility.Voluntary));
                global::Grasshopper.Kernel.Parameters.Param_String param_Label = null;
                param_Label = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "startLabel_", NickName = "startLabel_", Description = "Start Label", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_Label, ParamVisibility.Voluntary));

                param_Label = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "processLabel_", NickName = "processLabel_", Description = "Process Label", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_Label, ParamVisibility.Voluntary));

                param_Label = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "endLabel_", NickName = "endLabel_", Description = "End Label", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_Label, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooMollierProcessParam() { Name = "steamHumidificationProcess", NickName = "steamHumidificationProcess", Description = "SteamHumidificationProcess", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooMollierPointParam() { Name = "end", NickName = "end", Description = "End", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Colour() { Name = "color", NickName = "color", Description = "Color", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "epsilon", NickName = "epsilon", Description = "Epsilon", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMMollierCreateSteamHumidificationProcessByHumidityRatioDifference()
          : base("SAMMollier.CreateSteamHumidificationProcessByHumidityRatioDifference", "SAMMollier.CreateSteamHumidificationProcessByHumidityRatioDifference",
              "Creates SteamHumidificationProcess",
              "SAM", "Mollier")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_start");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            MollierPoint mollierPoint = null;
            if (!dataAccess.GetData(index, ref mollierPoint) || mollierPoint == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_humidityRatioDifference");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            double humidityRatioDifference = double.NaN;
            if (!dataAccess.GetData(index, ref humidityRatioDifference) || double.IsNaN(humidityRatioDifference))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Color color = Color.Empty;

            index = Params.IndexOfInputParam("_color_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref color);
            }

            string startLabel = null;
            index = Params.IndexOfInputParam("startLabel_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref startLabel);
            }
            string processLabel = null;
            index = Params.IndexOfInputParam("processLabel_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref processLabel);
            }
            string endLabel = null;
            index = Params.IndexOfInputParam("endLabel_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref endLabel);
            }

            SteamHumidificationProcess steamHumidificationProcess = Mollier.Create.SteamHumidificationProcess_ByHumidityRatioDifference(mollierPoint, humidityRatioDifference / 1000);

            if (steamHumidificationProcess != null && double.IsNaN(steamHumidificationProcess.End.RelativeHumidity))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Your target RH is above 100%, range for Relative Humidity is 0 - 100%");
            }

            index = Params.IndexOfOutputParam("steamHumidificationProcess");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooMollierProcess(steamHumidificationProcess, color, startLabel, processLabel, endLabel));
            }
            else
            {
                return;
            }
            MollierPoint end = new MollierPoint(steamHumidificationProcess.End);
            index = Params.IndexOfOutputParam("end");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooMollierPoint(end));
            }
            
            index = Params.IndexOfOutputParam("color");
            if (index != -1)
            {
                dataAccess.SetData(index, color);
            }

            index = Params.IndexOfOutputParam("epsilon");
            if (index != -1)
            {
                dataAccess.SetData(index, steamHumidificationProcess.Epsilon());
            }
        }

        protected override IEnumerable<IGH_Param> GetMollierDiagramParameters()
        {
            return new IGH_Param[] { Params.Output.Find(x => x.Name == "steamHumidificationProcess") };
        }
    }
}
