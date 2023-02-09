﻿using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.UI.WPF
{
    public class InternalConditionData
    {
        private AnalyticalModel AnalyticalModel { get; set; }
        private object @object;

        public InternalConditionData(AnalyticalModel analyticalModel, Space space)
        {
            AnalyticalModel = analyticalModel;

            @object = space == null ? null : new Space(space);
        }

        public InternalConditionData(AnalyticalModel analyticalModel, InternalCondition internalCondition)
        {
            AnalyticalModel = analyticalModel;

            @object = internalCondition == null ? null : new InternalCondition(internalCondition);
        }

        public InternalConditionData(AnalyticalModel analyticalModel, InternalConditionData internalConditionData)
        {
            AnalyticalModel = analyticalModel;

            @object = internalConditionData?.@object;
        }

        public InternalCondition InternalCondition
        {
            get
            {
                InternalCondition result = @object as InternalCondition;
                if(result != null)
                {
                    return result;
                }

                result = Space?.InternalCondition;

                return result;
            }
            set
            {
                if(@object is Space)
                {
                    ((Space)@object).InternalCondition = value;
                }
                else if(@object is InternalCondition)
                {
                    @object = value;
                }
            }
        }

        public Space Space
        {
            get
            {
                return @object as Space;
            }
            set
            {
                @object = value;
            }
        }

        public double Area
        {
            get
            {
                if (Space == null)
                {
                    return double.NaN;
                }

                if (!Space.TryGetValue(SpaceParameter.Area, out double result))
                {
                    return double.NaN;
                }

                return result;
            }
        }

        public string Name
        {
            get
            {
                return InternalCondition?.Name;
            }
        }

        public double Occupancy
        {
            get
            {
                if(Space == null)
                {
                    return double.NaN;
                }

                if(Space.TryGetValue(SpaceParameter.Occupancy, out double result) && !double.IsNaN(result))
                {
                    return result;
                }

                double area = Area;
                if(double.IsNaN(area))
                {
                    return double.NaN;
                }

                InternalCondition internalCondition = InternalCondition;
                if(internalCondition == null)
                {
                    return double.NaN;
                }

                if(!internalCondition.TryGetValue(InternalConditionParameter.AreaPerPerson, out double areaPerPerson) || double.IsNaN(areaPerPerson) || areaPerPerson <= 0)
                {
                    return double.NaN;
                }

                return area / areaPerPerson;
            }
        }

        public double HeatingDesignTemperature
        {
            get
            {
                return Analytical.Query.HeatingDesignTemperature(InternalCondition, AnalyticalModel?.ProfileLibrary);
            }
        }

        public double CoolingDesignTemperature
        {
            get
            {
                return Analytical.Query.CoolingDesignTemperature(InternalCondition, AnalyticalModel?.ProfileLibrary);
            }
        }

        public double OccupancySensibleGain
        {
            get
            {
                return Analytical.Query.OccupancySensibleGain(Space);
            }
        }

        public double OccupancyLatentGain
        {
            get
            {
                return Analytical.Query.OccupancyLatentGain(Space);
            }
        }

        public double EquipmentSensibleGain
        {
            get
            {
                return Analytical.Query.CalculatedEquipmentSensibleGain(Space);
            }
        }

        public double EquipmentLatentGain
        {
            get
            {
                return Analytical.Query.CalculatedEquipmentLatentGain(Space);
            }
        }

        public double Humidity
        {
            get
            {
                Profile profile = GetProfile(ProfileType.Humidification);
                if(profile == null)
                {
                    return double.NaN;
                }

                return profile.MaxValue;
            }
        }

        public double Dehumidity
        {
            get
            {
                Profile profile = GetProfile(ProfileType.Dehumidification);
                if (profile == null)
                {
                    return double.NaN;
                }

                return profile.MaxValue;
            }
        }

        public double LightingGain
        {
            get
            {
                return Analytical.Query.CalculatedLightingGain(Space);
            }
        }

        public string GetProfileName(ProfileType profileType)
        {
            return InternalCondition?.GetProfileName(profileType);
        }

        public Profile GetProfile(ProfileType profileType)
        {
            return InternalCondition?.GetProfile(profileType, AnalyticalModel?.ProfileLibrary);
        }

        public bool TryGetValue<T>(SpaceParameter spaceParameter, out T value, bool tryConvert = true)
        {
            value = default;
            if(Space == null)
            {
                return false;
            }

            return Space.TryGetValue(spaceParameter, out value, tryConvert);
        }

        public bool TryGetValue<T>(InternalConditionParameter internalConditionParameter, out T value, bool tryConvert = true)
        {
            value = default;
            if (InternalCondition == null)
            {
                return false;
            }

            return InternalCondition.TryGetValue(internalConditionParameter, out value, tryConvert);
        }

        public InternalCondition GetInternalConditionTemplate()
        {
            string name = InternalCondition?.Name;
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

            IEnumerable<InternalCondition> internalConditions = AnalyticalModel?.AdjacencyCluster?.GetInternalConditions(false, true);
            if(internalConditions == null || internalConditions.Count() == 0)
            {
                return null;
            }

            foreach(InternalCondition internalCondition in internalConditions)
            {
                if(internalCondition.Name == name)
                {
                    return internalCondition;
                }
            }

            return null;
        }
    }
}