using System;
using System.Drawing;

namespace CoreBrush
{
    public static class ImageProcessor
    {
        // Grayscale via luminance
        public static Bitmap Grayscale(Bitmap source)
        {
            int w = source.Width; int h = source.Height;
            Bitmap bmp = new Bitmap(w, h);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Color p = source.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    bmp.SetPixel(x, y, Color.FromArgb(p.A, gray, gray, gray));
                }
            }
            return bmp;
        }

        // Brightness/Contrast adjustment
        public static Bitmap BrightnessContrast(Bitmap source, int brightness, float contrast)
        {
            int w = source.Width; int h = source.Height;
            Bitmap bmp = new Bitmap(w, h);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Color p = source.GetPixel(x, y);
                    int r = (int)(((p.R / 255.0 - 0.5) * contrast + 0.5) * 255.0 + brightness);
                    int g = (int)(((p.G / 255.0 - 0.5) * contrast + 0.5) * 255.0 + brightness);
                    int b = (int)(((p.B / 255.0 - 0.5) * contrast + 0.5) * 255.0 + brightness);
                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));
                    bmp.SetPixel(x, y, Color.FromArgb(p.A, r, g, b));
                }
            }
            return bmp;
        }

        // Convolution (RGB) with options
        public static Bitmap ApplyConvolution(Bitmap source, double[,] kernel, double factor = 1.0, double bias = 0.0, bool clampToGrayScale = false, bool absoluteValue = false)
        {
            int width = source.Width;
            int height = source.Height;
            Bitmap result = new Bitmap(width, height);

            int kH = kernel.GetLength(0); // rows
            int kW = kernel.GetLength(1); // cols
            int kHalfW = kW / 2;
            int kHalfH = kH / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double r = 0, g = 0, b = 0;
                    int a = source.GetPixel(x, y).A;
                    for (int ky = 0; ky < kH; ky++)
                    {
                        int sy = Math.Clamp(y + ky - kHalfH, 0, height - 1);
                        for (int kx = 0; kx < kW; kx++)
                        {
                            int sx = Math.Clamp(x + kx - kHalfW, 0, width - 1);
                            Color sc = source.GetPixel(sx, sy);
                            double kval = kernel[ky, kx];
                            r += sc.R * kval;
                            g += sc.G * kval;
                            b += sc.B * kval;
                        }
                    }
                    if (absoluteValue)
                    {
                        r = Math.Abs(r); g = Math.Abs(g); b = Math.Abs(b);
                    }
                    r = r * factor + bias;
                    g = g * factor + bias;
                    b = b * factor + bias;
                    int ri = (int)Math.Round(Math.Clamp(r, 0, 255));
                    int gi = (int)Math.Round(Math.Clamp(g, 0, 255));
                    int bi = (int)Math.Round(Math.Clamp(b, 0, 255));
                    if (clampToGrayScale)
                    {
                        int gray = (int)Math.Round(0.299 * ri + 0.587 * gi + 0.114 * bi);
                        gray = Math.Max(0, Math.Min(255, gray));
                        result.SetPixel(x, y, Color.FromArgb(a, gray, gray, gray));
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.FromArgb(a, ri, gi, bi));
                    }
                }
            }
            return result;
        }

        // Unsharp Mask
        public static Bitmap ApplyUnsharp(Bitmap original, Bitmap blurred, double amount)
        {
            int width = original.Width;
            int height = original.Height;
            Bitmap? resized = null;
            Bitmap blurRef = blurred;
            if (blurred.Width != width || blurred.Height != height)
            {
                resized = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.DrawImage(blurred, 0, 0, width, height);
                }
                blurRef = resized;
            }
            Bitmap result = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color o = original.GetPixel(x, y);
                    Color b = blurRef.GetPixel(x, y);
                    double r = o.R + amount * (o.R - b.R);
                    double g = o.G + amount * (o.G - b.G);
                    double bl = o.B + amount * (o.B - b.B);
                    int ri = (int)Math.Round(Math.Clamp(r, 0, 255));
                    int gi = (int)Math.Round(Math.Clamp(g, 0, 255));
                    int bi = (int)Math.Round(Math.Clamp(bl, 0, 255));
                    result.SetPixel(x, y, Color.FromArgb(o.A, ri, gi, bi));
                }
            }
            resized?.Dispose();
            return result;
        }

        // Threshold manual
        public static Bitmap ThresholdManual(Bitmap source, int threshold)
        {
            int w = source.Width; int h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = source.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    int bw = gray >= threshold ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(p.A, bw, bw, bw));
                }
            }
            return result;
        }

        // Otsu
        public static int ComputeOtsuThreshold(Bitmap source)
        {
            int[] hist = new int[256];
            int w = source.Width; int h = source.Height;
            int total = w * h;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = source.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    hist[gray]++;
                }
            }
            double sum = 0; for (int i = 0; i < 256; i++) sum += i * hist[i];
            double sumB = 0; int wB = 0; int wF = 0; double maxVar = -1; int threshold = 0;
            for (int t = 0; t < 256; t++)
            {
                wB += hist[t]; if (wB == 0) continue; wF = total - wB; if (wF == 0) break;
                sumB += t * hist[t];
                double mB = sumB / wB; double mF = (sum - sumB) / wF;
                double between = wB * wF * (mB - mF) * (mB - mF);
                if (between > maxVar) { maxVar = between; threshold = t; }
            }
            return threshold;
        }

        // Adaptive Gaussian threshold (5x5) using local mean - C
        public static Bitmap ThresholdAdaptiveGaussian(Bitmap source, double C)
        {
            double[,] g5 = new double[,]
            {
                { 1,  4,  6,  4, 1 },
                { 4, 16, 24, 16, 4 },
                { 6, 24, 36, 24, 6 },
                { 4, 16, 24, 16, 4 },
                { 1,  4,  6,  4, 1 }
            };
            using var localMean = ApplyConvolution(source, g5, factor: 1.0 / 256.0, bias: 0.0, clampToGrayScale: true);
            int w = source.Width; int h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = source.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    int tLocal = (int)Math.Round(localMean.GetPixel(x, y).R - C);
                    tLocal = Math.Max(0, Math.Min(255, tLocal));
                    int bw = gray >= tLocal ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(p.A, bw, bw, bw));
                }
            }
            return result;
        }

        // ============================
        // Morphology (3x3 structuring element)
        // ============================

        // Binary erosion (foreground=white=255). Pixel stays white only if ALL neighbors under SE are white
        public static Bitmap ErodeBinary(Bitmap source)
        {
            int w = source.Width, h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool allWhite = true;
                    for (int dy = -1; dy <= 1 && allWhite; dy++)
                    {
                        int yy = Math.Clamp(y + dy, 0, h - 1);
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int xx = Math.Clamp(x + dx, 0, w - 1);
                            var p = source.GetPixel(xx, yy);
                            int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                            if (gray < 255)
                            {
                                allWhite = false;
                                break;
                            }
                        }
                    }
                    int val = allWhite ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(255, val, val, val));
                }
            }
            return result;
        }

        // Binary dilation (foreground=white=255). Pixel becomes white if ANY neighbor under SE is white
        public static Bitmap DilateBinary(Bitmap source)
        {
            int w = source.Width, h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool anyWhite = false;
                    for (int dy = -1; dy <= 1 && !anyWhite; dy++)
                    {
                        int yy = Math.Clamp(y + dy, 0, h - 1);
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int xx = Math.Clamp(x + dx, 0, w - 1);
                            var p = source.GetPixel(xx, yy);
                            int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                            if (gray > 0)
                            {
                                anyWhite = true;
                                break;
                            }
                        }
                    }
                    int val = anyWhite ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(255, val, val, val));
                }
            }
            return result;
        }

        // Grayscale erosion (minimum in 3x3 neighborhood)
        public static Bitmap ErodeGrayscale(Bitmap source)
        {
            int w = source.Width, h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int minVal = 255;
                    int a = source.GetPixel(x, y).A;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int yy = Math.Clamp(y + dy, 0, h - 1);
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int xx = Math.Clamp(x + dx, 0, w - 1);
                            var p = source.GetPixel(xx, yy);
                            int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                            if (gray < minVal) minVal = gray;
                        }
                    }
                    result.SetPixel(x, y, Color.FromArgb(a, minVal, minVal, minVal));
                }
            }
            return result;
        }

        // Grayscale dilation (maximum in 3x3 neighborhood)
        public static Bitmap DilateGrayscale(Bitmap source)
        {
            int w = source.Width, h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int maxVal = 0;
                    int a = source.GetPixel(x, y).A;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int yy = Math.Clamp(y + dy, 0, h - 1);
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int xx = Math.Clamp(x + dx, 0, w - 1);
                            var p = source.GetPixel(xx, yy);
                            int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                            if (gray > maxVal) maxVal = gray;
                        }
                    }
                    result.SetPixel(x, y, Color.FromArgb(a, maxVal, maxVal, maxVal));
                }
            }
            return result;
        }
    }
}
