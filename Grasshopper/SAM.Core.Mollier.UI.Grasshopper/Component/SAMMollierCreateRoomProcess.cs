﻿using Grasshopper.Kernel;
using SAM.Core.Mollier.UI.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Mollier;

namespace SAM.Core.Mollier.UI.Grasshopper
{
    public class SAMMollierCreateRoomProcess : MollierDiagramComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("08c03d2c-7e26-4094-a154-2b81255c7462");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.SAM_Mollier;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooMollierPointParam() { Name = "_start", NickName = "_start", Description = "MollierPoint for Start", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number = null;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_airFlow", NickName = "_airflow", Description = "AirFlow [m3/s]", Access = GH_ParamAccess.item, Optional = false };
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_sensibleLoad", NickName = "_sensibleLoad", Description = "Sensible Load [kW]", Access = GH_ParamAccess.item, Optional = false };
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_latentLoad", NickName = "_latentLoad", Description = "Latent Load [kW]", Access = GH_ParamAccess.item, Optional = false };
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooMollierProcessParam() { Name = "roomProcess", NickName = "roomProcess", Description = "Room Process", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooMollierPointParam() { Name = "end", NickName = "end", Description = "End", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "sensibleHeatRatio", NickName = "sensibleHeatRatio", Description = "Sensible Heat Ratio [-]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMMollierCreateRoomProcess()
          : base("SAMMollier.CreateRoomProcess", "SAMMollier.CreateRoomProcess",
              "Creates Room Process",
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

            MollierPoint start = null;
            if (!dataAccess.GetData(index, ref start) || start == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_airFlow");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            double airFlow = double.NaN;
            if (!dataAccess.GetData(index, ref airFlow) || double.IsNaN(airFlow))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_sensibleLoad");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            double sensibleLoad = double.NaN;
            if (!dataAccess.GetData(index, ref sensibleLoad) || double.IsNaN(sensibleLoad))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_latentLoad");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            double latentLoad = double.NaN;
            if (!dataAccess.GetData(index, ref latentLoad) || double.IsNaN(latentLoad))
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

            UndefinedProcess undefinedProcess = Core.Mollier.Create.UndefinedProcess(start, airFlow, sensibleLoad * 1000, latentLoad * 1000);
            index = Params.IndexOfOutputParam("roomProcess");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooMollierProcess(undefinedProcess, color, startLabel, processLabel, endLabel));
            }
            else
            {
                return;
            }

            MollierPoint end = new MollierPoint(undefinedProcess.End);
            index = Params.IndexOfOutputParam("end");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooMollierPoint(end));
            }

            index = Params.IndexOfOutputParam("sensibleHeatRatio");
            if (index != -1)
            {
                dataAccess.SetData(index, Mollier.Query.SensibleHeatRatio(sensibleLoad, latentLoad));
            }
        }

        protected override IEnumerable<IGH_Param> GetMollierDiagramParameters()
        {
            return new IGH_Param[] { Params.Output.Find(x => x.Name == "roomProcess") };
        }
    }
}