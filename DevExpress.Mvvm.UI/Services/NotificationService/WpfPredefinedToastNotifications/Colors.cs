using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DevExpress.Mvvm.UI.Native {
    class BackgroundCalculator {
        static Color[][] colorTable = new Color[][] {
            new Color[] { Color.FromArgb(76, 153, 255), Color.FromArgb(9, 74, 178) },
            new Color[] { Color.FromArgb(76, 0, 76), Color.FromArgb(140, 0, 149) },
            new Color[] { Color.FromArgb(76, 0, 153), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(76, 0, 0), Color.FromArgb(123, 0, 0) },
            new Color[] { Color.FromArgb(76, 0, 255), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(76, 255, 76), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(76, 255, 153), Color.FromArgb(18, 128, 35) },
            new Color[] { Color.FromArgb(76, 255, 0), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(76, 255, 255), Color.FromArgb(0, 130, 153) },
            new Color[] { Color.FromArgb(153, 76, 76), Color.FromArgb(123, 0, 0) },
            new Color[] { Color.FromArgb(76, 76, 153), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(153, 76, 153), Color.FromArgb(140, 0, 149) },
            new Color[] { Color.FromArgb(153, 76, 0), Color.FromArgb(210, 71, 38) },
            new Color[] { Color.FromArgb(153, 76, 255), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(153, 153, 255), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(153, 0, 76), Color.FromArgb(172, 25, 61) },
            new Color[] { Color.FromArgb(153, 0, 153), Color.FromArgb(140, 0, 149) },
            new Color[] { Color.FromArgb(153, 0, 0), Color.FromArgb(123, 0, 0) },
            new Color[] { Color.FromArgb(153, 0, 255), Color.FromArgb(140, 0, 149) },
            new Color[] { Color.FromArgb(153, 255, 76), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(153, 255, 153), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(153, 255, 255), Color.FromArgb(0, 130, 153) },
            new Color[] { Color.FromArgb(0, 76, 76), Color.FromArgb(0, 130, 153) },
            new Color[] { Color.FromArgb(0, 76, 153), Color.FromArgb(9, 74, 178) },
            new Color[] { Color.FromArgb(0, 76, 0), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(0, 76, 255), Color.FromArgb(9, 74, 178) },
            new Color[] { Color.FromArgb(0, 153, 76), Color.FromArgb(18, 128, 35) },
            new Color[] { Color.FromArgb(76, 76, 255), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(0, 153, 153), Color.FromArgb(0, 130, 153) },
            new Color[] { Color.FromArgb(0, 153, 0), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(0, 153, 255), Color.FromArgb(9, 74, 178) },
            new Color[] { Color.FromArgb(0, 0, 76), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(0, 0, 153), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(0, 0, 255), Color.FromArgb(81, 51, 171) },
            new Color[] { Color.FromArgb(0, 255, 76), Color.FromArgb(18, 128, 35) },
            new Color[] { Color.FromArgb(0, 255, 153), Color.FromArgb(18, 128, 35) },
            new Color[] { Color.FromArgb(0, 255, 0), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(0, 255, 255), Color.FromArgb(0, 130, 153) },
            new Color[] { Color.FromArgb(255, 76, 76), Color.FromArgb(123, 0, 0) },
            new Color[] { Color.FromArgb(255, 76, 153), Color.FromArgb(172, 25, 61) },
            new Color[] { Color.FromArgb(255, 76, 0), Color.FromArgb(210, 71, 38) },
            new Color[] { Color.FromArgb(255, 76, 255), Color.FromArgb(140, 0, 149) },
            new Color[] { Color.FromArgb(255, 153, 76), Color.FromArgb(210, 71, 38) },
            new Color[] { Color.FromArgb(255, 153, 153), Color.FromArgb(123, 0, 0) },
            new Color[] { Color.FromArgb(255, 153, 0), Color.FromArgb(210, 71, 38) },
            new Color[] { Color.FromArgb(255, 153, 255), Color.FromArgb(140, 0, 149) },
            new Color[] { Color.FromArgb(255, 0, 76), Color.FromArgb(172, 25, 61) },
            new Color[] { Color.FromArgb(255, 0, 153), Color.FromArgb(172, 25, 61) },
            new Color[] { Color.FromArgb(255, 0, 0), Color.FromArgb(123, 0, 0) },
            new Color[] { Color.FromArgb(255, 0, 255), Color.FromArgb(140, 0, 149) },
            new Color[] { Color.FromArgb(255, 255, 76), Color.FromArgb(89, 89, 89) },
            new Color[] { Color.FromArgb(255, 255, 153), Color.FromArgb(89, 89, 89) },
            new Color[] { Color.FromArgb(255, 255, 0), Color.FromArgb(89, 89, 89) },
            new Color[] { Color.FromArgb(76, 153, 76), Color.FromArgb(0, 138, 0) },
            new Color[] { Color.FromArgb(76, 153, 153), Color.FromArgb(0, 130, 153) },
            new Color[] { Color.FromArgb(76, 153, 0), Color.FromArgb(0, 138, 0) },
        };

        static Color RemoveGray(Color color) {
            int gray = Math.Min(Math.Min(color.R, color.G), color.B);
            return Color.FromArgb(color.R - gray, color.G - gray, color.B - gray);
        }

        static double LabDistance(Color rgb1, Color rgb2) {
            Lab lab1 = new Lab(), lab2 = new Lab();
            LabConverter.ToColorSpace(new Rgb { R = rgb1.R, G = rgb1.G, B = rgb1.B }, lab1);
            LabConverter.ToColorSpace(new Rgb { R = rgb2.R, G = rgb2.G, B = rgb2.B }, lab2);
            return Math.Pow(lab2.L - lab1.L, 2) + Math.Pow(lab2.A - lab1.A, 2) + Math.Pow(lab2.B - lab1.B, 2);
        }

        static double RgbDistance(Color rgb1, Color rgb2) {
            return Math.Pow(rgb2.R - rgb1.R, 2) + Math.Pow(rgb2.G - rgb1.G, 2) + Math.Pow(rgb2.B - rgb1.B, 2);
        }

        static IEnumerable<Color> EveryPixel(Bitmap bmp) {
            for(int i = 0; i < bmp.Width; ++i) {
                for(int j = 0; j < bmp.Height; ++j) {
                    yield return bmp.GetPixel(i, j);
                }
            }
        }

        static Color Normalize(int r, int g, int b) {
            int max = Math.Max(Math.Max(r, g), b);
            return Color.FromArgb(255 * r / max, 255 * g / max, 255 * b / max);
        }

        internal static System.Windows.Media.Color GetBestMatch(Bitmap bmp) {
            var map = new Dictionary<Color, int>();
            foreach(var pair in colorTable) {
                map[pair[0]] = 0;
            }
            int count = 0;
            foreach(Color px in EveryPixel(bmp)) {
                count++;
                double alpha = px.A / 255.0;
                Color withAlpha = Color.FromArgb(
                    (int)(px.R * alpha),
                    (int)(px.G * alpha),
                    (int)(px.B * alpha)
                );
                if(RgbDistance(RemoveGray(withAlpha), Color.FromArgb(0, 0, 0)) < 10)
                    continue;
                var distances = map.Keys.Select(c => new { d = LabDistance(c, withAlpha), c = c }).OrderBy(i => i.d).ToList();
                Color nearest = map.Keys.OrderBy(c => LabDistance(c, withAlpha)).First();
                map[nearest]++;
            }
            if(!map.Values.Any(v => v > count * 0.05))
                return DefaultGrayColor;
            var test = map.OrderByDescending(p => p.Value).ToList();
            var test2 = test.Select(v => (double)v.Value / test.Count()).ToList();
            double t = test2[0] - test2[1];
            Color left = map.OrderBy(p => p.Value).Last().Key;
            Color res = colorTable.First(p => p[0] == left)[1];
            return System.Windows.Media.Color.FromRgb(res.R, res.G, res.B);
        }

        public static System.Windows.Media.Color DefaultGrayColor {
            get { return System.Windows.Media.Color.FromRgb(89, 89, 89); }
        }

        class Rgb {
            public double R { get; set; }
            public double G { get; set; }
            public double B { get; set; }
            public void Initialize(Rgb color) {
                RgbConverter.ToColorSpace(color, this);
            }
            public Rgb ToRgb() {
                return RgbConverter.ToColor(this);
            }
        }
        class Xyz {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
            public void Initialize(Rgb color) {
                XyzConverter.ToColorSpace(color, this);
            }
            public Rgb ToRgb() {
                return XyzConverter.ToColor(this);
            }
        }
        class Lab {
            public double L;
            public double A;
            public double B;
        }
        static class RgbConverter {
            internal static void ToColorSpace(Rgb color, Rgb item) {
                item.R = color.R;
                item.G = color.G;
                item.B = color.B;
            }

            internal static Rgb ToColor(Rgb item) {
                return item;
            }
        }
        static class LabConverter {
            internal static void ToColorSpace(Rgb color, Lab item) {
                var xyz = new Xyz();
                xyz.Initialize(color);

                var white = XyzConverter.WhiteReference;
                var x = PivotXyz(xyz.X / white.X);
                var y = PivotXyz(xyz.Y / white.Y);
                var z = PivotXyz(xyz.Z / white.Z);

                item.L = Math.Max(0, 116 * y - 16);
                item.A = 500 * (x - y);
                item.B = 200 * (y - z);
            }

            internal static Rgb ToColor(Lab item) {
                var y = (item.L + 16.0) / 116.0;
                var x = item.A / 500.0 + y;
                var z = y - item.B / 200.0;

                var white = XyzConverter.WhiteReference;
                var x3 = x * x * x;
                var z3 = z * z * z;
                var xyz = new Xyz {
                    X = white.X * (x3 > XyzConverter.Epsilon ? x3 : (x - 16.0 / 116.0) / 7.787),
                    Y = white.Y * (item.L > (XyzConverter.Kappa * XyzConverter.Epsilon) ? Math.Pow(((item.L + 16.0) / 116.0), 3) : item.L / XyzConverter.Kappa),
                    Z = white.Z * (z3 > XyzConverter.Epsilon ? z3 : (z - 16.0 / 116.0) / 7.787)
                };

                return xyz.ToRgb();
            }

            private static double PivotXyz(double n) {
                return n > XyzConverter.Epsilon ? CubicRoot(n) : (XyzConverter.Kappa * n + 16) / 116;
            }

            private static double CubicRoot(double n) {
                return Math.Pow(n, 1.0 / 3.0);
            }
        }
        static class XyzConverter {
            internal static Xyz WhiteReference { get; private set; }
            internal const double Epsilon = 0.008856;
            internal const double Kappa = 903.3;
            static XyzConverter() {
                WhiteReference = new Xyz {
                    X = 95.047,
                    Y = 100.000,
                    Z = 108.883
                };
            }

            internal static double CubicRoot(double n) {
                return Math.Pow(n, 1.0 / 3.0);
            }

            internal static void ToColorSpace(Rgb color, Xyz item) {
                var r = PivotRgb(color.R / 255.0);
                var g = PivotRgb(color.G / 255.0);
                var b = PivotRgb(color.B / 255.0);

                item.X = r * 0.4124 + g * 0.3576 + b * 0.1805;
                item.Y = r * 0.2126 + g * 0.7152 + b * 0.0722;
                item.Z = r * 0.0193 + g * 0.1192 + b * 0.9505;
            }

            internal static Rgb ToColor(Xyz item) {
                var x = item.X / 100.0;
                var y = item.Y / 100.0;
                var z = item.Z / 100.0;

                var r = x * 3.2406 + y * -1.5372 + z * -0.4986;
                var g = x * -0.9689 + y * 1.8758 + z * 0.0415;
                var b = x * 0.0557 + y * -0.2040 + z * 1.0570;

                r = r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
                g = g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
                b = b > 0.0031308 ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;

                return new Rgb {
                    R = ToRgb(r),
                    G = ToRgb(g),
                    B = ToRgb(b)
                };
            }

            private static double ToRgb(double n) {
                var result = 255.0 * n;
                if(result < 0) return 0;
                if(result > 255) return 255;
                return result;
            }

            private static double PivotRgb(double n) {
                return (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
            }
        }
    }
}