using System;
using UnityEngine;

public class MapUtils
{
    public struct ProjectedPos
    {
        public int X, Y, ZoomLevel;
        public double LocalX, LocalY;
        public ProjectedPos(int x, int y, int zoomLevel, double localX = 0, double localY = 0)
        {
            X = x;
            Y = y;
            ZoomLevel = zoomLevel;
            LocalX = localX;
            LocalY = localY;
        }

        public ProjectedPos(double x, double y, int zoomLevel)
        {
            X = (int)Math.Floor(x);
            Y = (int)Math.Floor(y);
            ZoomLevel = zoomLevel;
            LocalX = x - X;
            LocalY = y - Y;
        }

        public float Magnitude
        {
            get { return ((Vector2)this).magnitude; }
        }

        public static ProjectedPos Lerp(ProjectedPos from, ProjectedPos to, float t)
        {
            Vector2 lerped = Vector2.Lerp(from, to, t);
            return new ProjectedPos(lerped.x, lerped.y, from.ZoomLevel);
        }

        public static implicit operator Vector2(ProjectedPos pos)
        {
            return new Vector2(pos.X + (float)pos.LocalX, pos.Y + (float)pos.LocalY);
        }


        public static bool operator ==(ProjectedPos p1, ProjectedPos p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y && p1.ZoomLevel == p2.ZoomLevel;
        }

        public static bool operator !=(ProjectedPos p1, ProjectedPos p2)
        {
            return !(p1 == p2);
        }

        public static ProjectedPos operator +(ProjectedPos p1, ProjectedPos p2)
        {
            double x = p1.X + p1.LocalX + p2.X + p2.LocalX;
            double y = p1.Y + p1.LocalY + p2.Y + p2.LocalY;
            return new ProjectedPos(x, y, p1.ZoomLevel);
        }

        public static ProjectedPos operator -(ProjectedPos p1, ProjectedPos p2)
        {
            double x = (p1.X + p1.LocalX) - (p2.X + p2.LocalX);
            double y = (p1.Y + p1.LocalY) - (p2.Y + p2.LocalY);
            return new ProjectedPos(x, y, p1.ZoomLevel);
        }
    }

    /// <summary>
    /// Forward mapping equation to project geographical coordinates to Cartesian ones.
    /// </summary>
    /// <param name="geo">Geographical Coordinates</param>
    /// <param name="zoomLevel">Intended zoom level</param>
    /// <returns>Cartesian coordinates</returns>
    public static ProjectedPos GeographicToProjection(Vector2 geo, int zoomLevel)
    {


        double tilesPerEdge = TilesPerEdge(zoomLevel);

        //Mercator Projection:
        double x = (geo.x + 180.0) * tilesPerEdge / 360.0;
        double y = (1.0 - (Math.Log(Math.Tan(Math.PI / 4 + (geo.y * Mathf.Deg2Rad) / 2.0)) / Math.PI)) / 2.0 * tilesPerEdge;

        return new ProjectedPos(x, y, zoomLevel);
    }

    /// <summary>
    /// Inverse mapping equation to get geographical coordinates from Cartesian coordinates
    /// </summary>
    /// <param name="proj">Cartesian coordinates</param>
    /// <returns>Geographical Coordinates</returns>
    public static Vector2 ProjectionToGeographic(ProjectedPos proj)
    {
        double tilesPerEdge = TilesPerEdge(proj.ZoomLevel);

        //Mercator Projection:
        double longitude = (proj.X * (360 / tilesPerEdge)) - 180;
        double latitude = Mathf.Rad2Deg * (Math.Atan(Math.Sinh((1 - proj.Y * (2 / tilesPerEdge)) * Math.PI)));

        return new Vector2((float)longitude, (float)latitude);
    }

    /// <summary>
    /// Amount of tiles on each edge which represents the world.
    /// </summary>
    /// <param name="zoomLevel">A zoomlevel of 1 means that the world map consist of 2x2 tiles.</param>
    /// <returns></returns>
    public static double TilesPerEdge(int zoomLevel)
    {
        return Math.Pow(2.0, zoomLevel);
    }
}
