﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer
{
    public static class Utilities
    {
        public static Tuple<double, double> Centroid(double x1, double y1, double x2, double y2)  // tinh toa do trong tam
        {
            double x = (x1+x2)/2;
            double y = (y1+y2)/2;

            return Tuple.Create(x, y);
        }
    }
}