using System;
using System.Collections.Generic;
using System.Linq;
// using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Numerics;
using BitMiracle.LibTiff.Classic;

namespace CSharpArrayTest
{
    public class Program
    {
        // main
        static int Main(string[] args)
        {
            //blah
            // objects used in "using" scope will be disposed immediately after exiting the scope
            // Attempt to read an image
            Tiff image = Tiff.Open("Lena_Y.tif", "r");
            // obtain basic tag information of the image
            #region GetTagInfo
            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            byte bits = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToByte();
            byte pixel = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToByte();
            double dpiX = image.GetField(TiffTag.XRESOLUTION)[0].ToDouble();
            double dpiY = image.GetField(TiffTag.YRESOLUTION)[0].ToDouble();
            #endregion
            /*
            // display information
            System.Console.WriteLine("Width is : {0}\nHeight is: {1}\ndpi is: {2}\nThe scanline is {3}\nBits per Sample is: {4}", width, height, dpiX, image.ScanlineSize(), bits);
            Console.WriteLine("Sample per pixel is: {0}", pixel);

            // store the intensity values of the image to 2d array
            byte[,] clean = new byte[height, width];
            clean = TiffFunctions.Tiff2Array(image, height, width);
            */
            byte[,] clean = TiffFunctions.Tiff2Array(image,height,width);
            Tiff noisyImage = Tiff.Open("Lena_Y_gauss_0.1.tif", "r");
            byte[,] noisy = new byte[height, width];
            noisy = TiffFunctions.Tiff2Array(noisyImage,height,width);

            //TiffFunctions.LearnSetPatch(clean, noisy, 9, 400, "SamplePatchGeneration.txt");

            //byte[,] denoised = MLMVN.Activation(noisy, 3, "Lena_Y_gauss_0.1_kernel_3_N_3_54_S_500_SD_2.8.wgt", 384, 3, 54);
            //TiffFunctions.WriteToFile(denoised, width, height, bits, pixel, dpiX, dpiY);

            //int[] networkSize = new int[4] { 511, 511, 511, 121 };
            int[] networkSize = new int[4] { 511, 511, 511, 169 };
            //int[] inputsPerSample = new int[4] { 170, 512, 512, 512 };
            //string fileName = "Lena_Y_gauss_0.1_samples_400_patch_11_rmse_4.0.wgt";
            //byte[,] denoised = MLMVN.fdenoiseNeural2(noisy, 3, fileName, 4, networkSize, 384);
            //TiffFunctions.WriteToFile(denoised, width, height, bits, pixel, dpiX, dpiY);

            string fileNameS = "Lena_Y_Gauss_0.1_Samples_300_Patches_13x13.txt";
            //string fileNameW = "Lena_Y_gauss_0.1_rmse_3.0_session_2.wgt";
            //int[,] output = MLMVN.MLMVN_TEST(fileNameS, 200, fileNameW, 4, networkSize, inputsPerSample, 384);
            //Complex[][,] weights = MLMVN.MLMVN_Learning(fileNameS, 200, fileNameW, 4, networkSize, inputsPerSample, 5.0, 0.0, 384, true);

            string fileName = "testWeightSave.wgt";
            Complex[][,] weights = MLMVN.MLMVN_Learning(fileNameS, 200, " ", 4, networkSize, 300.0, 0.0, 384, true);
            MLMVN.saveMlmvnWeights(fileName, weights, networkSize);
            return 0;
        } // end main
    } // end class
} // end namespace 
