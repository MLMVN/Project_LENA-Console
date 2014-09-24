using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;

namespace CSharpArrayTest
{
    public class TiffFunctions
    {
        // convert tiff image to 2d byte array
        public static byte[,] Tiff2Array(Tiff image, int height, int width)
        {
            // store the image information in 2d byte array
            // reserve memory for storing the size of 1 line
            byte[] scanline = new byte[image.ScanlineSize()];
            // reserve memory for the size of image
            byte[,] im = new byte[height, width];
            // grab the intensity values
            for (int i = 0; i < height; i++)
            {
                image.ReadScanline(scanline, i);
                for (int j = 0; j < width; j++)
                    im[i, j] = scanline[j];
            } // end grabbing intensity values
            return im;
        } // end method

        // write the image to file
        public static void WriteToFile(byte[,] im, int width, int height, byte bits, byte pixel, double dpiX, double dpiY)
        {
            // Attempt to recreate the image from 2d byte array im
            using (Tiff output = Tiff.Open("processed.tif", "w"))
            {
                // simple error check
                if (output == null)
                {
                    Console.WriteLine("Can not read the image.");
                    return;
                }
                // set tag information
                #region SetTagInfo
                output.SetField(TiffTag.IMAGEWIDTH, width);
                output.SetField(TiffTag.IMAGELENGTH, height);
                output.SetField(TiffTag.BITSPERSAMPLE, bits);
                output.SetField(TiffTag.SAMPLESPERPIXEL, pixel);
                output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.ROWSPERSTRIP, height);
                output.SetField(TiffTag.XRESOLUTION, dpiX);
                output.SetField(TiffTag.YRESOLUTION, dpiY);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                #endregion

                // reserve buffer
                byte[] buffer = new byte[width * sizeof(byte /*can be changed depending on the format of the image*/)];
                // obtain each line of the final byte arrays and write them to a file
                for (int i = 0; i < height; i++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        buffer[k] = im[i, k];
                    }
                    // write
                    output.WriteScanline(buffer, i);
                }
                // write to file
                output.WriteDirectory();
            } // end inner using
        }

        // create surrounding borders
        public static byte[,] MirrorImage(byte[,] im, int height, int width, int offset)
        {
            // write code here... someday
            // reserve 2darray with extended sizes
            int newHeight = height + (offset * 2);
            int newWidth = width + (offset * 2);
            byte[,] image = new byte[newHeight, newWidth];
            Console.WriteLine("Calling MirrorImage...\nSize of new matrix is {0} by {1}", newWidth, newHeight);
            // copy original image
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    image[i + offset, j + offset] = im[i, j];
                }
            }
            // copy columns - confirmed to work
            for (int i = 0; i < offset; i++) // 0~2
            {
                for (int row = offset; row < offset + height; row++) // 3~514
                {
                    // copy left columns
                    image[row, i] = im[row - offset, offset - i];
                    // copy right columns
                    image[row, i + width + offset] = im[row - offset, width - i - 2];
                } // end for
            } // end for

            for (int i = 0; i < offset; i++)
            {
                for (int col = 0; col < width + (offset * 2); col++)
                {
                    // copy top rows
                    image[i, col] = image[(offset * 2) - i, col];
                    // copy bottom rows
                    image[i + height + offset, col] = image[height + offset - 2 - i, col];
                }
            }
            return image;
        }

        // create kernel window
        public static byte[,] CreateWindow(byte[,] im, int row, int col, int kernel, int offset)
        {
            byte[,] image = new byte[kernel, kernel];
            for (int i = 0; i < kernel; i++)
            {
                for (int j = 0; j < kernel; j++)
                {
                    image[i, j] = im[row - offset + i, col - offset + j];
                }
            }
            return image;
        }

        // create kernel window
        public static byte[,] CreatePatch(byte[,] im, int row, int col, int kernel)
        {
            byte[,] array = new byte[kernel, kernel];
            for (int i = 0; i < kernel; i++)
            {
                for (int j = 0; j < kernel; j++)
                {
                    array[i, j] = im[row + i, col + j];
                }
            }
            return array;
        }
        public static byte[] CreatePatchAsArray(byte[,] im, int row, int col, int kernel)
        {
            byte[] array = new byte[kernel * kernel];
            for (int i = 0; i < kernel; i++)
            {
                for (int j = 0; j < kernel; j++)
                {
                    array[(i * kernel) + j] = im[row + i, col + j];
                }
            }
            return array;
        }
        // create learning samples
        public static void LearnSet(byte[,] clean, byte[,] noised, int kernel, int offset, int sSize)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("Sample_1.txt", true))
            {
                // get height and width
                int cleanHeight = clean.GetLength(0);
                int cleanWidth = clean.GetLength(1);
                // write the number of samples to file
                file.WriteLine(sSize.ToString());
                // initialize random number generator
                Random random = new Random();
                // begin generating samples
                for (int v = 0; v < sSize; v++)
                {
                    // generate random coordinate
                    int randomRow = random.Next(0, 256);
                    int randomCol = random.Next(0, 256);
                    // fetch kernel
                    byte[,] inputArray = TiffFunctions.CreateWindow(noised, randomRow + offset, randomCol + offset, kernel, offset);
                    // get optimal intensity value from clean image
                    int pixel = clean[randomRow, randomCol];
                    // reserve byte array
                    byte[] S = new byte[kernel * kernel];
                    // transform multi dimensional inputArray to 1d array
                    for (int i = 0; i < kernel; i++)
                    {
                        for (int j = 0; j < kernel; j++)
                        {
                            S[kernel * i + j] = inputArray[i, j];
                        } // end for loop
                    } // end for loop

                    // Create custome code for GUI version to save it under specified user directory with preferred name
                    // write to file
                    for (int i = 0; i < S.Length; i++)
                    {
                        file.Write(S[i] + " ");
                    }
                    file.Write(pixel);
                    file.WriteLine();
                } // end for loop
            } // end using scope
        }

        public static void LearnSetPatch(byte[,] clean, byte[,] noised, int kernel, int Size, string FileName)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileName, true)) // Samples.txt
            {
                // get height and width
                int patchLength = kernel * kernel;
                int twoPatchLength = patchLength * 2;
                int rowSize = clean.GetLength(0);  //rowSize
                int colSize = clean.GetLength(1);   //colSize
                double temp = rowSize / kernel;
                int sSizeRow = (int)Math.Floor(temp) - 1;
                temp = colSize / kernel;
                int sSizeCol = (int)Math.Floor(temp) - 1;
                int sSize = sSizeRow * sSizeCol;
                int sSizeRowIndex = sSizeRow * kernel;
                int sSizeColIndex = sSizeCol * kernel;

                // initialize variables to store patches
                byte[] noisyVector = new byte[kernel * kernel];
                byte[] cleanVector = new byte[kernel * kernel];
                byte[,] vectorMAP = new byte[sSize, twoPatchLength];
                Random random = new Random();

                // generate all possible patches first
                for (int row = 0; row < sSizeRow; row++)
                {
                    for (int col = 0; col < sSizeCol; col++)
                    {
                        //create patch from noisy image
                        noisyVector = CreatePatchAsArray(noised, row * kernel, col * kernel, kernel);
                        cleanVector = CreatePatchAsArray(clean, row * kernel, col * kernel, kernel);
                        // store in vectorMap
                        // noisy first
                        for (int v = 0; v < patchLength; v++)
                            vectorMAP[row * sSizeRow + col, v] = noisyVector[v];
                        // then clean
                        for (int v = patchLength; v < twoPatchLength; v++)
                            vectorMAP[row * sSizeRow + col, v] = cleanVector[v - patchLength];
                    }
                }
                byte[] S = new byte[twoPatchLength];
                // done generating all possible patches.
                // Now we need to randomly select patches
                for (int v = 0; v < Size; v++)
                {
                    //generate random coordinate
                    int randomRow = random.Next(0, sSize);
                    //Create custome code for GUI version to save it under specified user directory with preferred name
                    //write to file
                    for (int i = 0; i < twoPatchLength; i++)
                        S[i] = vectorMAP[randomRow, i];
                    for (int i = 0; i < twoPatchLength; i++)
                    {
                        //file.Write(vectorMAP[randomRow, i]);
                        file.Write(S[i] + " ");
                    }
                    file.WriteLine();
                } // end for loop
            } // end using scope
        }
    } // end class
} // end namespace
