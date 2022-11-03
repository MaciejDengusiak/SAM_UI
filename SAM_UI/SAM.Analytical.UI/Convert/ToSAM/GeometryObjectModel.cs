﻿using SAM.Core;
using SAM.Geometry;
using SAM.Geometry.Spatial;
using SAM.Geometry.UI;
using System.Collections.Generic;
using System.Windows.Media;

namespace SAM.Analytical.UI
{
    public static partial class Create
    {
        public static GeometryObjectModel ToSAM_GeometryObjectModel(this AnalyticalModel analyticalModel, Mode mode, Plane plane = null)
        {
            if(analyticalModel == null)
            {
                return null;
            }

            switch(mode)
            {
                case Mode.ThreeDimensional:
                    return ToSAM_GeometryObjectModel_3D(analyticalModel, plane);

                case Mode.TwoDimensional:
                    return ToSAM_GeometryObjectModel_2D(analyticalModel, plane);
            }

            return null;
        }

        private static GeometryObjectModel ToSAM_GeometryObjectModel_3D(this AnalyticalModel analyticalModel, Plane plane = null)
        {
            if (analyticalModel == null)
            {
                return null;
            }

            AnalyticalModel analyticalModel_Temp = new AnalyticalModel(analyticalModel);
            analyticalModel_Temp.Normalize();

            List<Panel> panels = analyticalModel_Temp.GetPanels();

            GeometryObjectModel result = new GeometryObjectModel();
            foreach (Panel panel in panels)
            {
                VisualPanel visualPanel = new VisualPanel(panel);
                visualPanel.Add(new Face3DObject(panel.GetFace3D(true), Query.SurfaceAppearance(panel)));

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null && apertures.Count != 0)
                {
                    foreach (Aperture aperture in apertures)
                    {
                        VisualAperture visualAperture = new VisualAperture(aperture);

                        AperturePart aperturePart = AperturePart.Undefined;
                        List<Face3D> face3Ds = null;

                        aperturePart = AperturePart.Frame;
                        face3Ds = aperture.GetFace3Ds(aperturePart);
                        if (face3Ds != null && face3Ds.Count != 0)
                        {
                            face3Ds.ForEach(x => visualAperture.Add(new Face3DObject(x, Query.SurfaceAppearance(aperture, aperturePart))));
                        }

                        aperturePart = AperturePart.Pane;
                        face3Ds = aperture.GetFace3Ds(aperturePart);
                        if (face3Ds != null && face3Ds.Count != 0)
                        {
                            face3Ds.ForEach(x => visualAperture.Add(new Face3DObject(x, Query.SurfaceAppearance(aperture, aperturePart))));
                        }

                        visualPanel.Add(visualAperture);
                    }
                }

                result.Add(visualPanel);
            }


            return result;
        }

        private static GeometryObjectModel ToSAM_GeometryObjectModel_2D(this AnalyticalModel analyticalModel, Plane plane = null)
        {
            if (analyticalModel == null)
            {
                return null;
            }

            AnalyticalModel analyticalModel_Temp = new AnalyticalModel(analyticalModel);

            Dictionary<Panel, List<ISegmentable3D>> dictionary = Analytical.Query.SectionDictionary<ISegmentable3D>(analyticalModel_Temp.GetPanels(), plane);
            if(dictionary == null)
            {
                return null;
            }

            GeometryObjectModel result = new GeometryObjectModel();
            foreach (KeyValuePair<Panel, List<ISegmentable3D>> keyValuePair in dictionary)
            {
                if(keyValuePair.Key == null)
                {
                    continue;
                }

                if(keyValuePair.Value == null)
                {
                    continue;
                }

                VisualPanel visualPanel = new VisualPanel(keyValuePair.Key);
                foreach(ISegmentable3D segmentable3D in keyValuePair.Value)
                {
                    List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                    if(segment3Ds == null)
                    {
                        continue;
                    }

                    segment3Ds.ForEach(x => visualPanel.Add(new Segment3DObject(x, new CurveAppearance(Color.FromRgb(255, 255, 255), 0.01)) { Tag = keyValuePair.Key }));
                }
                result.Add(visualPanel);

            }

            return result;

        }
    }
}
