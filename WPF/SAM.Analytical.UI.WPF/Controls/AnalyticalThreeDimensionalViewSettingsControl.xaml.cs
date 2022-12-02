﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SAM.Analytical.UI.WPF
{
    /// <summary>
    /// Interaction logic for AnalyticalThreeDimensionalViewSettingsControl.xaml
    /// </summary>
    public partial class AnalyticalThreeDimensionalViewSettingsControl : UserControl
    {
        private AnalyticalThreeDimensionalViewSettings analyticalThreeDimensionalViewSettings;

        public AnalyticalThreeDimensionalViewSettingsControl()
        {
            InitializeComponent();
        }

        public AnalyticalThreeDimensionalViewSettingsControl(AnalyticalThreeDimensionalViewSettings analyticalThreeDimensionalViewSettings)
        {
            InitializeComponent();

            SetAnalyticalThreeDimensionalViewSettings(analyticalThreeDimensionalViewSettings);
        }

        public AnalyticalThreeDimensionalViewSettings AnalyticalThreeDimensionalViewSettings
        {
            get
            {
                return GetAnalyticalThreeDimensionalViewSettings();
            }

            set
            {
                SetAnalyticalThreeDimensionalViewSettings(value);
            }
        }

        private void SetAnalyticalThreeDimensionalViewSettings(AnalyticalThreeDimensionalViewSettings analyticalThreeDimensionalViewSettings)
        {
            this.analyticalThreeDimensionalViewSettings = analyticalThreeDimensionalViewSettings;

            checkBox_Visibilty_Space.IsChecked = analyticalThreeDimensionalViewSettings.ContainsType(typeof(Space));
            checkBox_Visibilty_Panel.IsChecked = analyticalThreeDimensionalViewSettings.ContainsType(typeof(Panel));
            checkBox_Visibilty_Aperture.IsChecked = analyticalThreeDimensionalViewSettings.ContainsType(typeof(Aperture));
        }

        private AnalyticalThreeDimensionalViewSettings GetAnalyticalThreeDimensionalViewSettings()
        {
            AnalyticalThreeDimensionalViewSettings result = new AnalyticalThreeDimensionalViewSettings(analyticalThreeDimensionalViewSettings);

            CheckBox checkBox;

            List<Type> types = new List<Type>();

            checkBox = checkBox_Visibilty_Space;
            if (checkBox.IsChecked != null && checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                types.Add(typeof(Space));
            }

            checkBox = checkBox_Visibilty_Aperture;
            if (checkBox.IsChecked != null && checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                types.Add(typeof(Aperture));
            }

            checkBox = checkBox_Visibilty_Panel;
            if (checkBox.IsChecked != null && checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                types.Add(typeof(Panel));
            }

            result.SetTypes(types);

            return result;
        }
    }
}
