﻿// HeightMapExtensions.cs
//
// Author:
//       Xavier Fischer 
//
// Copyright (c) 2019 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEM.Net.Core
{
    /// <summary>
    /// Various extensions methods for height maps
    /// </summary>
    public static class HeightMapExtensions
    {
        /// <summary>
        /// Centers height map on origin
        /// </summary>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        /// <remarks>This can be used in an height map processing pipeline, as coordinates are changed only on enumeration</remarks>
        public static HeightMap CenterOnOrigin(this HeightMap heightMap)
        {
            //Logger.Info("CenterOnOrigin...");
            var bbox = heightMap.BoundingBox;

            double xOriginOffset = bbox.xMax - (bbox.xMax - bbox.xMin) / 2d;
            double yOriginOffset = bbox.yMax - (bbox.yMax - bbox.yMin) / 2d;
            heightMap.Coordinates = heightMap.Coordinates.Translate(-xOriginOffset, -yOriginOffset);

            heightMap.BoundingBox = new BoundingBox(bbox.xMin - xOriginOffset, bbox.xMax - xOriginOffset
                                                    , bbox.yMin - yOriginOffset, bbox.yMax - yOriginOffset);
            return heightMap;
        }

        /// <summary>
        /// Centers a set of points on origin, when their bbox is not known
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /// <remarks>Warning : The bounding box will be computed (points will be enumerated). Prefer the <see cref="CenterOnOrigin(IEnumerable{GeoPoint}, BoundingBox)"/> method when the bbox is known
        /// </remarks>
        public static IEnumerable<GeoPoint> CenterOnOrigin(this IEnumerable<GeoPoint> points)
        {
            //Logger.Info("CenterOnOrigin...");
            var bbox = points.GetBoundingBox();

            return points.CenterOnOrigin(bbox);
        }

        /// <summary>
        /// Centers a set of points on origin, when their bbox is known
        /// </summary>
        /// <param name="points"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public static IEnumerable<GeoPoint> CenterOnOrigin(this IEnumerable<GeoPoint> points, BoundingBox bbox)
        {
            //Logger.Info("CenterOnOrigin...");
            double xOriginOffset = bbox.xMax - (bbox.xMax - bbox.xMin) / 2d;
            double yOriginOffset = bbox.yMax - (bbox.yMax - bbox.yMin) / 2d;
            points = points.Translate(-xOriginOffset, -yOriginOffset, 0);

            return points;
        }
        /// <summary>
        /// Translate points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static IEnumerable<GeoPoint> Translate(this IEnumerable<GeoPoint> points, double x, double y, double z = 0)
        {
            //Logger.Info("Translate...");
            foreach (var pt in points)
            {
                var p = pt.Clone();
                p.Latitude += y;
                p.Longitude += x;
                p.Elevation += z;
                yield return p;
            }
            //Logger.Info("Translate done...");
        }

        /// <summary>
        /// Helper to get an in memory coordinate list
        /// useful to generate normal maps and let the same height map follow the pipeline (reproj, center, ...)
        /// </summary>
        /// <returns></returns>
        public static HeightMap BakeCoordinates(this HeightMap heightMap)
        {
            heightMap.Coordinates = heightMap.Coordinates.ToList();

            return heightMap;
        }

        /// <summary>
        /// Scale height map verticaly for relief exageration
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="zFactor"></param>
        /// <returns></returns>
        public static HeightMap ZScale(this HeightMap heightMap, float zFactor = 1f)
        {
            heightMap.Coordinates = heightMap.Coordinates.ZScale(zFactor);
            heightMap.Minimum *= zFactor;
            heightMap.Maximum *= zFactor;

            return heightMap;
        }
        /// <summary>
        /// Scale height map
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        /// <remarks>Bounding box will be updated accordingly</remarks>
        public static HeightMap Scale(this HeightMap heightMap, float x = 1f, float y = 1f, float z = 1f)
        {
            heightMap.Coordinates = heightMap.Coordinates.Scale(x, y, z);
            heightMap.BoundingBox = heightMap.BoundingBox.Scale(x, y); // z does not affect bbox
            heightMap.Minimum *= z;
            heightMap.Maximum *= z;

            return heightMap;
        }

        /// <summary>
        /// Rescale the height map in order that it fits into the specified size
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static HeightMap FitInto(this HeightMap heightMap, float maxSize)
        {
            float scale = 1f;
            if (heightMap.BoundingBox.Width > heightMap.BoundingBox.Height)
            {
                scale = (float)(maxSize / heightMap.BoundingBox.Width);
            }
            else
            {
                scale = (float)(maxSize / heightMap.BoundingBox.Height);
            }
            heightMap.Coordinates = heightMap.Coordinates.Scale(scale, scale, scale);
            heightMap.BoundingBox = heightMap.BoundingBox.ScaleAbsolute(scale, scale);
            return heightMap;
        }
        /// <summary>
        /// Scales elevation of give points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="zFactor"></param>
        /// <returns></returns>
        public static IEnumerable<GeoPoint> ZScale(this IEnumerable<GeoPoint> points, float zFactor = 1f)
        {
            return points.Scale(1, 1, zFactor);
        }
        /// <summary>
        /// Scale given points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static IEnumerable<GeoPoint> Scale(this IEnumerable<GeoPoint> points, float x = 1f, float y = 1f, float z = 1f)
        {
            //Logger.Info("Scale...");
            foreach (var pt in points)
            {
                var pout = pt.Clone();
                pout.Longitude *= x;
                pout.Latitude *= y;
                pout.Elevation *= z;
                yield return pout;
            }
            //Logger.Info("Scale done...");

        }

        /// <summary>
        /// Verticaly translates a height map
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static HeightMap ZTranslate(this HeightMap heightMap, float distance)
        {
            heightMap.Coordinates = heightMap.Coordinates.ZTranslate(distance);
            heightMap.Minimum += distance;
            heightMap.Maximum += distance;

            return heightMap;
        }

        /// <summary>
        /// Verticaly translates points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static IEnumerable<GeoPoint> ZTranslate(this IEnumerable<GeoPoint> points, float distance)
        {
            //Logger.Info("ZTranslate...");
            foreach (var pt in points)
            {
                var pout = pt.Clone();
                pout.Elevation += distance;
                yield return pout;
            }
            //Logger.Info("ZTranslate done...");

        }


        /// <summary>
        /// Sort height map coordinates (lat descending, then lon)
        /// </summary>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        public static HeightMap Sort(this HeightMap heightMap)
        {
            heightMap.Coordinates = heightMap.Coordinates.Sort();

            return heightMap;
        }
        /// <summary>
        /// Sort coordinates (lat descending, then lon)
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public static IEnumerable<GeoPoint> Sort(this IEnumerable<GeoPoint> coords)
        {
            coords = coords.OrderByDescending(pt => pt.Latitude)
                           .ThenBy(pt => pt.Longitude);

            return coords;
        }

        /// <summary>
        /// Downsample a given height map
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="step">Must be a factor of two</param>
        /// <returns></returns>
        public static HeightMap Downsample(this HeightMap heightMap, int step)
        {
            if (step == 0 || step % 2 != 0)
                throw new ArgumentOutOfRangeException("step", "Step must be a factor of 2");

            HeightMap hMap = new HeightMap(heightMap.Width / step, heightMap.Height / step);
            hMap.Maximum = heightMap.Maximum;
            hMap.Minimum = heightMap.Minimum;
            hMap.BoundingBox = heightMap.BoundingBox;
            hMap.Coordinates = DownsampleCoordinates(heightMap.Coordinates.ToList(), heightMap.Width, heightMap.Height, step).ToList();

            return hMap;
        }

        private static IEnumerable<GeoPoint> DownsampleCoordinates(List<GeoPoint> input, int w, int h, int step)
        {
            for (int lat = 0; lat <= h; lat += step)
            {
                for (int lon = 0; lon <= w; lon += step)
                {
                    yield return input[lon + lat * h];
                }
            }
        }

    }
}
