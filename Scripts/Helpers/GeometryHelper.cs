using System;
using GravityZond.Measurements;
using VRageMath;

namespace SpaceEngineers.UWBlockPrograms.Helpers.Geometry
{
    public class GeometryHelper {
        public static Vector3D GetGravVectorsIntersection(GravityPointData point1Data, GravityPointData point2Data) {
            return GetLinesIntersection(point1Data.Gravity, point1Data.Position, point2Data.Gravity, point2Data.Position);
        }

        public static Vector3D GetLinesIntersection(Vector3D line1GuideVector, Vector3D line1StartPoint, Vector3D line2GuideVector, Vector3D line2StartPoint){
            double n1x = line1GuideVector.X;
            double n1y = line1GuideVector.Y;
            double n1z = line1GuideVector.Z;

            double p1x = line1StartPoint.X;
            double p1y = line1StartPoint.Y;
            double p1z = line1StartPoint.Z;

            double n2x = line2GuideVector.X;
            double n2y = line2GuideVector.Y;
            double n2z = line2GuideVector.Z;

            double p2x = line2StartPoint.X;
            double p2y = line2StartPoint.Y;
            double p2z = line2StartPoint.Z;

            double denominator = n1x*n2y - n1y*n2x;

            if(denominator == 0)
                throw new Exception("Lines do not intersect");

            // here is solution of equations system for lines intersection
            double intersectionX = n1x*n2y*p2x - n1y*n2x*p1x + n1x*n2x * (p1y - p2y);
            intersectionX /= denominator;

            double intersectionY = n1x*n2y*p1y + n1y*n2y*p2x - n1y*n2x*p2y - n1y*n2y*p1x;
            intersectionX /= denominator;

            double intersectionZ = n2z*(n1y*(p2x - p1x) + n1x*(p1y - p2y))/denominator + p2z;

            return new Vector3D(intersectionX, intersectionY, intersectionZ);
        }
    }
}