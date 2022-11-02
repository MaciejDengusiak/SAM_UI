﻿using System.Collections.Generic;

namespace SAM.Geometry.UI.WPF
{
    public static partial class Convert
    {
        public static VisualGeometryObjectModel ToMedia3D(this GeometryObjectModel geometryObjectModel)
        {
            if(geometryObjectModel == null)
            {
                return null;
            }

            VisualGeometryObjectModel result = new VisualGeometryObjectModel(geometryObjectModel);

            List<ISAMGeometryObject> sAMGeometryObjects = geometryObjectModel?.GetSAMGeometryObjects<ISAMGeometryObject>();
            if (sAMGeometryObjects != null)
            {
                foreach (ISAMGeometryObject sAMGeometryObject in sAMGeometryObjects)
                {
                    if(sAMGeometryObject == null)
                    {
                        continue;
                    }

                    IVisualGeometryObject visualGeometryObject = Create.IVisualGeometryObject(sAMGeometryObject);
                    if (visualGeometryObject == null)
                    {
                        continue;
                    }

                    result.Children.Add(visualGeometryObject as dynamic);
                }
            }

            return result;
        }
    }
}