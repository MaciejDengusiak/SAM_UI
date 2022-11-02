﻿using Newtonsoft.Json.Linq;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.UI
{
    public class Point3DObject : Point3D, IPoint3DObject
    {
        public PointAppearance PointAppearance { get; set; }

        public Point3D Point3D
        {
            get
            {
                return new Point3D(this);
            }
        }

        public Point3DObject(JObject jObject)
            : base(jObject)
        {

        }

        public Point3DObject(Point3DObject point3DObject)
            : base(point3DObject)
        {
            if (point3DObject?.PointAppearance != null)
            {
                PointAppearance = new PointAppearance(point3DObject?.PointAppearance);
            }
        }

        public Point3DObject(Point3D point3D)
            : base(point3D)
        {

        }

        public Point3DObject(Point3D point3D, PointAppearance pointAppearance)
            : base(point3D)
        {
            if(pointAppearance != null)
            {
                PointAppearance = new PointAppearance(pointAppearance);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("PointAppearance"))
            {
                PointAppearance = new PointAppearance(jObject.Value<JObject>("PointAppearance"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if(PointAppearance != null)
            {
                jObject.Add("PointAppearance", PointAppearance.ToJObject());
            }

            return jObject;
        }
    }
}