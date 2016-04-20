using System;
using UnityEngine;


namespace KSF_SolidRocketBooster
{
    public static class AdvSRBGraphUtils
    {
        
        /// <summary>
        /// This will return an image of the specified size with the thrust graph for this nozzle
        /// Totally jacked the image code from the wiki http://wiki.kerbalspaceprogram.com/wiki/Module_code_examples, with some modifications
        /// 
        /// 
        /// </summary>
        /// <param name="imgH">Hight of the desired output, in pixels</param>
        /// <param name="imgW">Width of the desired output, in pixels</param>
        /// <param name="burnTime">Chart for burn time in seconds</param>
        /// <returns></returns>
        public static Texture2D stackThrustPredictPic(int imgH, int imgW, int burnTime, FloatCurve atmoCurve, AdvSRBNozzle nozzle)
        {
            //First we set up the analyzer
            //Step 1: Figure out fuel sources
            nozzle.FuelStackSearcher(nozzle.FuelSourcesList);
            //Step 2: Set up mass variables
            float stackTotalMass = 0f;
            stackTotalMass = nozzle.fullStackFuelMass;

            float remStackFuel = nozzle.fullStackFuelMass;

            //stackTotalMass = CalcStackWetMass(FuelSourcesList, stackTotalMass);

            //float[] segmentFuelArray = new float[nozzle.FuelSourcesList.Count];

            int i = 0;
            //foreach (Part p in nozzle.FuelSourcesList)
            //{
            //    segmentFuelArray[i] = p.GetResourceMass();
            //    i++;
            //}

            //float stackCurrentMass = stackTotalMass;
            //Now we set up the image maker
            Texture2D image = new Texture2D(imgW, imgH);
            int graphXmin = 19;
            int graphXmax = imgW - 20;
            int graphYmin = 19;
            //Step 3: Set Up color variables
            Color brightG = Color.black;
            brightG.r = .3372549f;
            brightG.g = 1;
            Color mediumG = Color.black;
            mediumG.r = 0.16862745f;
            mediumG.g = .5f;
            Color lowG = Color.black;
            lowG.r = 0.042156863f;
            lowG.g = .125f;

            Color MassColor = Color.blue;
            Color ThrustColor = Color.cyan;
            Color ExtraThrustColor = Color.yellow;


            //Step 4: Define text arrays
            KSF_CharArrayUtils.populateCharArrays();
            //Step 5a: Define time markings (every 10 seconds gets a verticle line)
            System.Collections.Generic.List<int> timeLines = new System.Collections.Generic.List<int>();
            double xScale = (imgW - 40) / (double)burnTime;
            //print("xScale: " + xScale);
            calcTimeLines((float)burnTime, 10, timeLines, (float)xScale, 20);
            //Step 5b: Define vertical line markings (9 total to give 10 sections)
            System.Collections.Generic.List<int> horzLines = new System.Collections.Generic.List<int>();
            calcHorizLines(imgH - graphYmin, horzLines, 9, 20);
            //Step 6: Clear the background

            //Set all the pixels to black. If you don't do this the image contains random junk.
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    image.SetPixel(x, y, Color.black);
                }
            }

            //Step 7a: Draw Time Lines
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    if (timeLines.Contains(x) && y > graphYmin)
                        image.SetPixel(x, y, lowG);
                }
            }

            //Step 7b: Draw Vert Lines
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    if (horzLines.Contains(y) && x < graphXmax && x > graphXmin)
                        image.SetPixel(x, y, lowG);
                }
            }


            //Step 7c: Draw Bounding Lines
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    if ((x == graphXmin | x == graphXmax) && (y > graphYmin | y == graphYmin))
                        image.SetPixel(x, y, mediumG);

                    if (y == graphYmin && graphXmax > x && graphXmin < x)
                        image.SetPixel(x, y, mediumG);
                }
            }

            //Step 8a: Populate graphArray
            double simStep = .2;
            i = 0;
            //double peakThrustTime = 0;
            double peakThrustAmt = 0;

            //set up the array for the graphs
            int graphArraySize = Convert.ToInt16(Convert.ToDouble(burnTime) / simStep);
            double[,] graphArray = new double[graphArraySize, 4];


            //one time setups
            //graphArray[i, 0] = stackMassFlow(FuelSourcesList, (float)(i * simStep), segmentFuelArray, simStep);

            graphArray[i, 0] = nozzle.MassFlow.Evaluate((float)(i * simStep)) * nozzle.fullStackFuelMass;
            graphArray[i, 1] = stackTotalMass;
            graphArray[i, 2] = 9.80665 * atmoCurve.Evaluate(1) * graphArray[i, 0];
            graphArray[i, 3] = graphArray[i, 2] - (graphArray[i, 1] * 9.80665);

            remStackFuel -= (float)graphArray[i, 0];


            //fForce = 9.81f * fCurrentIsp * fFuelFlowMass / TimeWarp.fixedDeltaTime;
            do
            {
                i++;
                //
                //graphArray[i, 0] = stackMassFlow(FuelSourcesList, (float)(i * simStep), segmentFuelArray, simStep);

                graphArray[i, 0] = nozzle.MassFlow.Evaluate((float)(i * simStep)) * nozzle.fullStackFuelMass;
                
                
                
                graphArray[i, 1] = graphArray[i - 1, 1] - (graphArray[i - 1, 0] * simStep);

                if (remStackFuel > 0)
                    graphArray[i, 2] = 9.80665 * atmoCurve.Evaluate(1) * graphArray[i, 0];
                else
                    graphArray[i, 2] = 0;


                if (graphArray[i, 2] > 0)
                    graphArray[i, 3] = graphArray[i, 2] - (graphArray[i, 1] * 9.80665);
                else
                    graphArray[i, 3] = 0;

                if (graphArray[i, 2] > peakThrustAmt)
                {
                    peakThrustAmt = graphArray[i, 2];
                    //peakThrustTime = i;
                }


                remStackFuel -= (float)graphArray[i, 0] * (float)simStep;

                //print("generating params i=" + i + " peak at " + Convert.ToInt16(Convert.ToDouble(simDuration) / simStep));
            } while (i + 1 < graphArraySize);

            //Step 8b: Make scales for the y axis
            double yScaleMass = 1;
            double yScaleThrust = 1;
            int usableY;
            usableY = imgH - 20;

            yScaleMass = usableY / (Mathf.CeilToInt((float)(graphArray[0, 1] / 10)) * 10);
            float inter;
            inter = (float)peakThrustAmt / 100;
            //print("1: " + inter);
            inter = Mathf.CeilToInt(inter);
            //print("22: " + inter);
            inter = inter * 100;
            //print("3: " + inter);
            inter = usableY / inter;
            //print("4: " + inter);

            yScaleThrust = inter;

            //print(yScaleThrust + ":" + peakThrustAmt + ":" + usableY);

            Debug.Log("graphed scales");

            //Step 8c: Graph the mass
            int lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - 20, yScaleMass, graphArray, simStep, graphArraySize, 1, 20);

                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, MassColor);
                }
            }

            //Step 8d: Graph the thrust
            lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - 20, yScaleThrust, graphArray, simStep, graphArraySize, 2, 20);
                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, ThrustColor);
                }
            }

            //Step 8e: Graph the thrust extra
            lineWidth = 2;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - 20, yScaleThrust, graphArray, simStep, graphArraySize, 3, 20);
                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, ExtraThrustColor);
                }
            }




            //Step 9: Set up boxes for time

            //int i = 0;
            string s;
            i = 0;
            int length = 0;
            int pos = 0;
            int startpos = 0;
            Texture2D tex;

            do
            {
                s = "";

                pos = timeLines[i];
                i++;
                s = i * 10 + " s";

                //print("composite string: " + s);

                length = calcStringPixLength(KSF_CharArrayUtils.convertStringToCharArray(s));
                //print("length: " + length);

                startpos = Mathf.FloorToInt((float)pos - 0.5f * (float)length);

                Color[] region = image.GetPixels(startpos, 23, length, 11);

                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }

                image.SetPixels(startpos, 23, length, 11, region);

                tex = convertCharArrayToTex(KSF_CharArrayUtils.convertStringToCharArray(s), length, brightG);

                Color[] region2 = tex.GetPixels();

                image.SetPixels(startpos + 2, 25, length - 4, 7, region2);

                length = 0;

            } while (i < timeLines.Count);


            //set up boxes for horizontal lines, mass first
            startpos = 0;
            length = 0;
            pos = 0;
            i = 0;
            do
            {
                s = "";
                pos = horzLines[i];
                i++;
                s = ((Mathf.CeilToInt((float)(graphArray[0, 1] / 10)) * i)).ToString() + " t";
                length = calcStringPixLength(KSF_CharArrayUtils.convertStringToCharArray(s));
                startpos = Mathf.FloorToInt((float)pos - 5f);
                Color[] region = image.GetPixels(23, startpos, length, 11);
                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }
                image.SetPixels(23, startpos, length, 11, region);
                tex = convertCharArrayToTex(KSF_CharArrayUtils.convertStringToCharArray(s), length, brightG);
                Color[] region2 = tex.GetPixels();
                image.SetPixels(25, startpos + 2, length - 4, 7, region2);
                length = 0;
            } while (i < horzLines.Count);

            //set up boxes for horizontal lines,  thrust
            startpos = 0;
            length = 0;
            pos = 0;
            i = 0;
            do
            {
                s = "";
                pos = horzLines[i];
                i++;
                s = ((Mathf.CeilToInt((float)(peakThrustAmt / 100)) * (i * 10))).ToString() + " k";
                length = calcStringPixLength(KSF_CharArrayUtils.convertStringToCharArray(s));
                startpos = Mathf.FloorToInt((float)pos - 5f);
                Color[] region = image.GetPixels(imgW - 60, startpos, length, 11);
                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }
                image.SetPixels(imgW - 60, startpos, length, 11, region);
                tex = convertCharArrayToTex(KSF_CharArrayUtils.convertStringToCharArray(s), length, brightG);
                Color[] region2 = tex.GetPixels();
                image.SetPixels(imgW - 58, startpos + 2, length - 4, 7, region2);
                length = 0;
            } while (i < horzLines.Count);

            image.Apply();
            return image;
        }

        /*
        /// <summary>
        /// This is a modified version of stackThrustPredictPic to take more arguments and to fit a single segment
        /// </summary>
        /// <param name="imgH">Desired hieght of the image, in pixels</param>
        /// <param name="imgW">Desired width of the image, in pixels</param>
        /// <param name="burnTime">Length of time to simulate burn, in seconds</param>
        /// <param name="AdvSRB_Module">the partmodule AdvSRBSegment</param>
        /// <param name="fuelMass">mass of the SRB fuel on this segment, in KSP units</param>
        /// <param name="xOffset">margin on x axis (beginning and end)</param>
        /// <param name="yOffset">margin on y axis (bottom)</param>
        public Texture2D segThrustPredictPic(int imgH, int imgW, int burnTime, AdvSRBSegment AdvSRB_Module, float fuelMass, float dryMass, int xOffset, int yOffset, System.Collections.Generic.List<Part> fSL)
        {
            //Now we set up the image maker
            Texture2D image = new Texture2D(imgW, imgH);
            int graphXmin = xOffset - 1;
            int graphXmax = imgW - xOffset;
            int graphYmin = yOffset - 1;
            //Set Up color variables

            Color brightG = Color.black;
            brightG.r = .3372549f;
            brightG.g = 1;

            Color lowG = Color.black;
            lowG.r = 0.042156863f;
            lowG.g = .125f;

            Color mediumG = Color.black;
            mediumG.r = 0.16862745f;
            mediumG.g = .5f;

            Color MassColor = Color.blue;
            Color ThrustColor = Color.cyan;
            Color ExtraThrustColor = Color.yellow;


            //fuel arrays
            float[] segmentFuelArray = new float[1];
            segmentFuelArray[0] = fSL[0].GetResourceMass();


            //Step 4: Define text arrays
            populateCharArrays();

            //Step 5a: Define time markings (every 10 seconds gets a verticle line)
            System.Collections.Generic.List<int> timeLines = new System.Collections.Generic.List<int>();
            double xScale = (imgW - (2 * xOffset)) / (double)burnTime;
            calcTimeLines((float)burnTime, 10, timeLines, (float)xScale, xOffset);

            //Step 5b: Define vertical line markings (9 total to give 10 sections)
            System.Collections.Generic.List<int> horzLines = new System.Collections.Generic.List<int>();
            calcHorizLines(imgH - graphYmin, horzLines, 9, yOffset);

            //Step 6: Clear the background
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    image.SetPixel(x, y, Color.black);
                }
            }

            //Step 7a: Draw Time Lines
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    if (timeLines.Contains(x) && y > graphYmin)
                        image.SetPixel(x, y, lowG);
                }
            }

            //Step 7b: Draw Vert Lines
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    if (horzLines.Contains(y) && x < graphXmax && x > graphXmin)
                        image.SetPixel(x, y, lowG);
                }
            }


            //Step 7c: Draw Bounding Lines
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    if ((x == graphXmin | x == graphXmax) && (y > graphYmin | y == graphYmin))
                        image.SetPixel(x, y, mediumG);

                    if (y == graphYmin && graphXmax > x && graphXmin < x)
                        image.SetPixel(x, y, mediumG);
                }
            }

            //Step 8a: Populate graphArray
            double simStep = .1;
            i = 0;
            //double peakThrustTime = 0;
            double peakThrustAmt = 0;

            //set up the array for the graphs
            int graphArraySize = Convert.ToInt16(Convert.ToDouble(burnTime) / simStep);
            double[,] graphArray = new double[graphArraySize, 4];

            //one time setups
            graphArray[i, 0] = graphArray[i, 0] = stackMassFlow(fSL, (float)(i * simStep), segmentFuelArray, simStep);
            graphArray[i, 1] = fuelMass + dryMass;
            graphArray[i, 2] = 9.81 * atmosphereCurve.Evaluate(1) * graphArray[i, 0];
            graphArray[i, 3] = graphArray[i, 2] - (graphArray[i, 1] * 9.81);

            //run the full simulation to generate the lookup table for the graph
            do
            {
                i++;
                graphArray[i, 0] = graphArray[i, 0] = stackMassFlow(fSL, (float)(i * simStep), segmentFuelArray, simStep);
                graphArray[i, 1] = graphArray[i - 1, 1] - (graphArray[i - 1, 0] * simStep);
                graphArray[i, 2] = 9.81 * atmosphereCurve.Evaluate(1) * graphArray[i, 0];
                graphArray[i, 3] = graphArray[i, 2] - (graphArray[i, 1] * 9.81);

                if (graphArray[i, 2] > peakThrustAmt)
                {
                    peakThrustAmt = graphArray[i, 2];
                }
            } while (i + 1 < graphArraySize);

            //Step 8b: Make scales for the y axis
            double yScaleMass = 1;
            double yScaleThrust = 1;
            int usableY;
            usableY = imgH - yOffset;

            yScaleMass = usableY / (Mathf.CeilToInt((float)(graphArray[0, 1] / 10)) * 10);
            float inter;
            inter = (float)peakThrustAmt / 100;
            inter = Mathf.CeilToInt(inter);
            inter = inter * 100;
            inter = usableY / inter;
            yScaleThrust = inter;

            //Step 8c: Graph the mass
            int lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - xOffset, yScaleMass, graphArray, simStep, graphArraySize, 1, yOffset);

                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, MassColor);
                }
            }

            //Step 8d: Graph the thrust
            lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - xOffset, yScaleThrust, graphArray, simStep, graphArraySize, 2, yOffset);
                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, ThrustColor);
                }
            }

            //Step 8e: Graph the thrust extra
            lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - xOffset, yScaleThrust, graphArray, simStep, graphArraySize, 3, yOffset);
                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, ExtraThrustColor);
                }
            }

            //Step 9: Set up boxes for time
            string s;
            i = 0;
            int length = 0;
            int pos = 0;
            int startpos = 0;
            Texture2D tex;

            do
            {
                s = "";
                pos = timeLines[i];
                i++;
                s = i * 10 + " s";
                length = calcStringPixLength(convertStringToCharArray(s));
                startpos = Mathf.FloorToInt((float)pos - 0.5f * (float)length);
                Color[] region = image.GetPixels(startpos, 3 + yOffset, length, 11);
                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }
                image.SetPixels(startpos, 3 + yOffset, length, 11, region);
                tex = convertCharArrayToTex(convertStringToCharArray(s), length, brightG);
                Color[] region2 = tex.GetPixels();
                image.SetPixels(startpos + 2, 5 + yOffset, length - 4, 7, region2);
                length = 0;
            } while (i < timeLines.Count);


            //set up boxes for horizontal lines, mass first
            startpos = 0;
            length = 0;
            pos = 0;
            i = 0;
            do
            {
                s = "";
                pos = horzLines[i];
                i++;
                s = ((Mathf.CeilToInt((float)(graphArray[0, 1] / 10)) * i)).ToString() + " t";
                length = calcStringPixLength(convertStringToCharArray(s));
                startpos = Mathf.FloorToInt((float)pos - 5f);
                Color[] region = image.GetPixels(3 + xOffset, startpos, length, 11);
                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }
                image.SetPixels(3 + xOffset, startpos, length, 11, region);
                tex = convertCharArrayToTex(convertStringToCharArray(s), length, brightG);
                Color[] region2 = tex.GetPixels();
                image.SetPixels(5 + xOffset, startpos + 2, length - 4, 7, region2);
                length = 0;
            } while (i < horzLines.Count);

            //set up boxes for horizontal lines,  thrust
            startpos = 0;
            length = 0;
            pos = 0;
            i = 0;
            do
            {
                s = "";
                pos = horzLines[i];
                i++;
                s = ((Mathf.CeilToInt((float)(peakThrustAmt / 100)) * (i * 10))).ToString() + " k";
                length = calcStringPixLength(convertStringToCharArray(s));
                startpos = Mathf.FloorToInt((float)pos - 5f);
                Color[] region = image.GetPixels(imgW - (40 + xOffset), startpos, length, 11);
                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }
                image.SetPixels(imgW - (40 + xOffset), startpos, length, 11, region);
                tex = convertCharArrayToTex(convertStringToCharArray(s), length, brightG);
                Color[] region2 = tex.GetPixels();
                image.SetPixels(imgW - (38 + xOffset), startpos + 2, length - 4, 7, region2);
                length = 0;
            } while (i < horzLines.Count);

            image.Apply();
            return image;
        }

         */

        private static int fGraph(double xScale, int xPos, double yScale, double[,] array, double step, int arraySize, int column, int yOffset)
        {
            double t = 0;
            int i = 0;
            t = xPos / xScale;
            t = Mathf.Floor((float)(t / step));
            i = Convert.ToInt16(t);
            i = Mathf.Clamp(i, 0, arraySize - 1);
            if (Convert.ToInt16(yScale * array[i, column]) > 0)
                return (Convert.ToInt16(yScale * array[i, column]) + yOffset);
            else
                return yOffset;
        }

        
        private static int calcStringPixLength(System.Collections.Generic.List<KSF_CharArray> list)
        {
            int i = 2;

            foreach (KSF_CharArray c in list)
            {
                i += c.charWidth;
            }
            i++;
            return i;
        }



        private static  Texture2D convertCharArrayToTex(System.Collections.Generic.List<KSF_CharArray> chArray, int length, Color color)
        {
            Texture2D tex = new Texture2D(length - 4, 7);
            int offSet = 0;
            System.Collections.Generic.List<Vector2> pxMap = new System.Collections.Generic.List<Vector2>();
            pxMap.Clear();

            foreach (KSF_CharArray ka in chArray)
            {
                foreach (Vector2 px in ka.pixelList)
                {
                    pxMap.Add(new Vector2(px.x + offSet, px.y));
                }
                offSet += ka.charWidth;
            }

            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    if (pxMap.Contains(new Vector2(x, y)))
                    {
                        tex.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
            return tex;
        }

        private static void calcTimeLines(float duration, float spacing, System.Collections.Generic.List<int> list, float xscale, int xOffset)
        {
            list.Clear();
            float f = 0f;
            int result = 0;
            int i = 1;
            do
            {
                f = (i * spacing * xscale);
                result = Mathf.RoundToInt(f) + xOffset;
                list.Add(result);
                i++;
            } while (i < duration / spacing);
        }

        private static void calcHorizLines(float yHeight, System.Collections.Generic.List<int> list, int numLines, int yOffset)
        {
            list.Clear();
            float f = 0f;
            int result = 0;
            int i = 1;
            do
            {
                f = (i * yHeight) / (numLines + 1);
                result = Mathf.RoundToInt(f) + yOffset;
                list.Add(result);
                //print("Added horizontal line at: " + result);
                i++;
            } while (i <= numLines);
        }





        /*


        public static float stackMassFlow(System.Collections.Generic.List<Part> stackPartList, AdvSRBNozzle nozzle, float time, float[] a, double step)
        {
            int i = 0;
            float totalFlow = 0f;
            foreach (Part p in stackPartList)
            {
                AdvSRBSegment srb = p.GetComponent<AdvSRBSegment>();

                if (a[i] > 0 && srb.CalcMassFlow(time) > 0)
                {
                    totalFlow += srb.CalcMassFlow(time);
                    a[i] -= (float)(srb.CalcMassFlow(time) * step);
                }
                i++;
            }
            return totalFlow;
        }

        public static float CalcStackWetMass(System.Collections.Generic.List<Part> pl, float totalMass)
        {
            totalMass = 0f;
            foreach (Part p in pl)
            {
                totalMass += p.mass + p.GetResourceMass();
            }
            totalMass += part.mass;
            return totalMass;
        }
    }
    */

    }
        public static class AdvSRBUtils
        {
            public static float GetResourceDensity(string ResourceName)
            {
                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("RESOURCE_DEFINITION"))
                {
                    if (ResourceName == node.GetValues("name")[0])
                    {
                        return float.Parse(node.GetValues("density")[0]);
                    }
                }

                return -1.0f;
            }


            public static float CalcMassFlow(float time, AnimationCurve massFlow)
            {
                float f;
                f = massFlow.Evaluate(time);

                //if (!HighLogic.LoadedSceneIsEditor)
                //{
                //    if (thrustVariation > .0001f)
                //        f = f * (1 + CalcMassFlux());
                //}

                if (f > 0)
                    return f;
                else
                    return 0;
            }

            public static AnimationCurve AnimationCurveFromString(string s, float defaultBurnTime)
            {
                AnimationCurve ac = new AnimationCurve();
                if (s == "")
                {
                    Debug.LogWarning("No Burn Profile found! Defaulting to constant burn rate.");
                    ac.AddKey(0, (float)(1 / defaultBurnTime));
                    return ac;
                }
                //Debug.Log(s);
                s.Trim();
                Char[] delimiters = new Char[] { ',', ';' };
                String[] result;
                String time;
                String value;
                String inTang;
                String outTang;
                Keyframe k = new Keyframe();
                bool cont = true;
                int i = 0;
                do
                {
                    result = s.Split(delimiters, 2);
                    time = result[0];
                    s = result[1];

                    result = s.Split(delimiters, 2);
                    value = result[0];
                    s = result[1];

                    result = s.Split(delimiters, 2);
                    inTang = result[0];
                    s = result[1];

                    result = s.Split(delimiters, 2);
                    outTang = result[0];

                    if (result.Length > 1)
                    {
                        s = result[1];
                        cont = true;
                    }
                    else
                        cont = false;

                    k.time = Convert.ToSingle(time);
                    k.value = Convert.ToSingle(value);
                    k.inTangent = Convert.ToSingle(inTang);
                    k.outTangent = Convert.ToSingle(outTang);

                    ac.AddKey(k);

                    i++;

                } while (cont);
                return ac;
            }

            public static string AnimationCurveToString(AnimationCurve ac)
            {
                string s = "";
                for (int i = 0; i < ac.length; i++)
                {
                    if (s != "")
                    {
                        s += ';';
                    }
                    s += (ac.keys[i].time + ";" + ac.keys[i].value + ";" + ac.keys[i].inTangent + ";" + ac.keys[i].outTangent);
                }
                return s;
            }
        }
    }

