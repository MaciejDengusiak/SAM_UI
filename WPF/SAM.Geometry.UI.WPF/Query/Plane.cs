﻿using System.Windows.Media.Media3D;

namespace SAM.Geometry.UI.WPF
{
    public static partial class Query
    {
        public static Spatial.Plane Plane(this PerspectiveCamera perspectiveCamera)
        {
            if(perspectiveCamera == null)
            {
                return null;
            }

            if (Core.UI.WPF.Query.IsNaN(perspectiveCamera.Position) || Core.UI.WPF.Query.IsNaN(perspectiveCamera.UpDirection) || Core.UI.WPF.Query.IsNaN(perspectiveCamera.LookDirection))
            {
                return null;
            }

            Spatial.Vector3D upDirection = Convert.ToSAM(perspectiveCamera.UpDirection);
            Spatial.Vector3D lookDirection = Convert.ToSAM(perspectiveCamera.LookDirection);

            //Geometry.Spatial.Plane result = new Geometry.Spatial.Plane(Convert.ToSAM(perspectiveCamera.Position), -lookDirection.CrossProduct(upDirection), upDirection);

            Spatial.Plane result = new Spatial.Plane(Convert.ToSAM(perspectiveCamera.Position), lookDirection);

            upDirection = Spatial.Query.Project(result, upDirection);

            result = new Spatial.Plane(Convert.ToSAM(perspectiveCamera.Position), -lookDirection.CrossProduct(upDirection), upDirection);

            return result;
        }
    }
}
