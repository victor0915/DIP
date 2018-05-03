using System;

using System.Runtime.InteropServices;
using System.Collections.Generic;

using Emgu.CV;
using Emgu.CV.Structure;


namespace DigitalImageProcessing
{
    public static class imageProcessing
    {
        public static Image<Bgr, Byte> ConvertToGray(Image<Bgr, Byte> source)  //灰階 function //Gray = R*0.299 + G*0.587 + B*0.114
        {
            Image<Bgr, Byte> result = new Image<Bgr, Byte>(source.Width, source.Height);

            int rows = source.Height;
            int cols = source.Width;

            byte r, g, b;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    b = (byte)source.Data[y, x, 0];  //B
                    g = (byte)source.Data[y, x, 1];  //G
                    r = (byte)source.Data[y, x, 2];  //R

                    byte grayColor = (byte)(b * 0.114 + g * 0.587 + r * 0.299);
 
                    result.Data[y, x, 0] = grayColor;
                    result.Data[y, x, 1] = grayColor;
                    result.Data[y, x, 2] = grayColor;
                }
            }

            return result;
        }

        public static Image<Bgr, Byte> ConvertToMirror(Image<Bgr, Byte> source)
        {
            Image<Bgr, Byte> result = new Image<Bgr, Byte>(source.Width, source.Height);

            int rows = source.Height;
            int cols = source.Width;

            byte r, g, b;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    b = (byte)source.Data[y, x, 0];  //B
                    g = (byte)source.Data[y, x, 1];  //G
                    r = (byte)source.Data[y, x, 2];  //R

                    result.Data[y, (cols - x - 1), 0] = b;
                    result.Data[y, (cols - x - 1), 1] = g;
                    result.Data[y, (cols - x - 1), 2] = r;
                }
            }
            return result;
        }

        public static Image<Bgr, Byte> Rotating(Image<Bgr, Byte> source, double theta) 
        {
            Image<Bgr, Byte> result = new Image<Bgr, Byte>(source.Width, source.Height);

            int rows = source.Height;
            int cols = source.Width;
            
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    double radian = -theta * Math.PI / 180;

                    int centerX = x - cols / 2;
                    int centerY = y - rows / 2;
                    int newX = (int)(Math.Cos(radian) * centerX - Math.Sin(radian) * centerY) + cols / 2;
                    int newY = (int)(Math.Sin(radian) * centerX + Math.Cos(radian) * centerY) + rows / 2;

                    if (newY >= 0 && newY < rows && newX >= 0 && newX < cols)
                    {
                        result.Data[y, x, 0] = source.Data[newY, newX, 0];
                        result.Data[y, x, 1] = source.Data[newY, newX, 1];
                        result.Data[y, x, 2] = source.Data[newY, newX, 2];
                    }
                }

            }

            

            return result;
        }
        
        public static Image<Bgr, Byte> imageBlending(Image<Bgr, Byte> source, Image<Bgr, Byte> source2, double Threshold)
        {
            Image<Bgr, Byte> result = new Image<Bgr, Byte>(source.Width, source.Height);

            int cols = source.Width;
            int rows = source.Height;
            
            byte r, g, b;
            byte r2, g2, b2;
            for(int y = 0; y < rows; y++)
            {
                for(int x = 0; x < cols; x++)
                {
                    b = source.Data[y, x, 0];
                    g = source.Data[y, x, 1];
                    r = source.Data[y, x, 2];

                    b2 = source2.Data[y, x, 0];
                    g2 = source2.Data[y, x, 1];
                    r2 = source2.Data[y, x, 2];

                    result.Data[y, x, 0] = (byte)(b * Threshold + b2 * (1 - Threshold));
                    result.Data[y, x, 1] = (byte)(g * Threshold + g2 * (1 - Threshold));
                    result.Data[y, x, 2] = (byte)(r * Threshold + r2 * (1 - Threshold));
                }

            }



            return result;
        }

        public static Image<Bgr, Byte> ConvertToOtsu(Image<Bgr, Byte> source)
        {
            Image<Bgr, Byte> result = new Image<Bgr, Byte>(source.Width, source.Height);

            int cols = source.Width;
            int rows = source.Height;
            int [ , , ] grayConvert = new int[rows, cols, 3];
            int [] grayHistogram = new int[256];

            int grayOneLocalTotall = 0;
            int oneLocalPixelTotall = 0;
            float oneLocalMean = 0;
            float varianceOneLocal = 0;

            int grayTwoLocalTotall = 0;
            int twoLocalPixelTotall = 0;
            float twoLocalMean = 0;
            float varianceTwoLocal = 0;

            float minVariance = 0;

            byte r, g, b;
            for(int y = 0; y < rows; y++)
            {
                for(int x = 0; x < cols; x++)
                {
                    b = source.Data[y, x, 0];
                    g = source.Data[y, x, 1];
                    r = source.Data[y, x, 2];
                    
                    byte grayColor = (byte)(b * 0.114 + g * 0.587 + r * 0.299);

                    grayConvert[y, x, 0] = grayColor;
                    grayConvert[y, x, 1] = grayColor;
                    grayConvert[y, x, 2] = grayColor;

                    grayHistogram[grayColor] += 1;
                }
            }

            int Threshold = 0;
            for(int T = 0; T < 256; T++)
            {
                grayOneLocalTotall = 0;
                oneLocalPixelTotall = 0;
                oneLocalMean = 0;
                for (int oneLocal = 0; oneLocal <= T; oneLocal++)
                {
                    grayOneLocalTotall += grayHistogram[oneLocal] * oneLocal;
                    oneLocalPixelTotall += grayHistogram[oneLocal];
                }

                if (oneLocalPixelTotall == 0) continue;  //分母不能為零

                oneLocalMean = (float)grayOneLocalTotall / oneLocalPixelTotall; //平均灰階1
                             //oneLocalMean = (float)Math.Round((decimal)grayOneLocalTotall / oneLocalPixelTotall, 6);  //平均灰階1
 
                varianceOneLocal = 0;
                for (int oneLocal = 0; oneLocal <= T; oneLocal++)
                {
                   varianceOneLocal += (float)Math.Pow(oneLocalMean - oneLocal, 2) * grayHistogram[oneLocal];  //變異量1
                }

                grayTwoLocalTotall = 0;
                twoLocalPixelTotall = 0;
                twoLocalMean = 0;
                for (int twoLocal = T + 1; twoLocal < 256; twoLocal++)
                {
                    grayTwoLocalTotall += grayHistogram[twoLocal] * twoLocal;
                    twoLocalPixelTotall += grayHistogram[twoLocal];
                }

                if (twoLocalPixelTotall == 0) continue;  //分母不能為零

                twoLocalMean = (float)grayTwoLocalTotall / twoLocalPixelTotall; //平均灰階2
                             //twoLocalMean = (float)Math.Round((decimal)grayTwoLocalTotall / twoLocalPixelTotall, 6);  //平均灰階2

                varianceTwoLocal = 0;
                for (int twoLocal = T + 1; twoLocal < 256; twoLocal++)
                {
                    varianceTwoLocal += (float)Math.Pow(twoLocalMean - twoLocal, 2) * grayHistogram[twoLocal];  //變異量2
                }

                if((minVariance > varianceOneLocal + varianceTwoLocal) || minVariance == 0)
                {
                    minVariance = varianceOneLocal + varianceTwoLocal;  //找到最小的群內變異數總和
                    Threshold = T;
                }   
            }

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if(grayConvert[y, x, 0] < Threshold)
                    {
                        grayConvert[y, x, 0] = 0;
                        grayConvert[y, x, 1] = 0;
                        grayConvert[y, x, 2] = 0;
                    }
                    else
                    {
                        grayConvert[y, x, 0] = 255;
                        grayConvert[y, x, 1] = 255;
                        grayConvert[y, x, 2] = 255;
                    }

                    result.Data[y, x, 0] = (byte)grayConvert[y, x, 0];
                    result.Data[y, x, 1] = (byte)grayConvert[y, x, 1];
                    result.Data[y, x, 2] = (byte)grayConvert[y, x, 2];
                }
            }

            return result;
        }
                
        public static Image<Bgr, Byte> HistogramEqualization(Image<Bgr, Byte> source)
        {
            Image<Bgr, Byte> result = new Image<Bgr, Byte>(source.Width, source.Height);

            int cols = source.Width;
            int rows = source.Height;
            int[,] histogram = new int[256, 3];
            float bCdfEqualized = 0, gCdfEqualized = 0, rCdfEqualized = 0;
            int[,] arrayEqualized = new int[256, 3];

            byte b, g, r;
            for(int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    b = source.Data[y, x, 0];
                    g = source.Data[y, x, 1];
                    r = source.Data[y, x, 2];

                    histogram[b, 0] += 1;
                    histogram[g, 1] += 1;
                    histogram[r, 2] += 1;
                }
            }

            for (int grayScale = 0; grayScale < 256; grayScale++)
            {
                bCdfEqualized += (float)Math.Round((decimal)histogram[grayScale, 0] / (rows * cols), 6) * 255;
                arrayEqualized[grayScale, 0] = (int)Math.Round(bCdfEqualized, 0);

                gCdfEqualized += (float)Math.Round((decimal)histogram[grayScale, 1] / (rows * cols), 6) * 255;
                arrayEqualized[grayScale, 1] = (int)Math.Round(gCdfEqualized, 0);

                rCdfEqualized += (float)Math.Round((decimal)histogram[grayScale, 2] / (rows * cols), 6) * 255;
                arrayEqualized[grayScale, 2] = (int)Math.Round(rCdfEqualized, 0);
            }

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    b = source.Data[y, x, 0];
                    g = source.Data[y, x, 1];
                    r = source.Data[y, x, 2];

                    result.Data[y, x, 0] = (byte)arrayEqualized[b, 0];
                    result.Data[y, x, 1] = (byte)arrayEqualized[g, 1];
                    result.Data[y, x, 2] = (byte)arrayEqualized[r, 2];
                }
            }

            return result;
        }
    }
}
