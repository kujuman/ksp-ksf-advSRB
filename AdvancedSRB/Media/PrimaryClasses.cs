
/*
 * Kerbal Science Foundation Advanced Solid Rocket Booster v0.5 for Kerbal Space Program
 * Released September 13, 2013 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 * For attribution, please attribute "Kujuman from the official KSP forums"
 * Portions of this work were based on example code posted at the KSP wiki. If you have something to contribute there, please do!
 */

using KSP;
using UnityEngine;
using System;

namespace KSF_SolidRocketBooster
{
    public class KSF_SolidBoosterSegment : PartModule
    {
        [KSPField]
        public FloatCurve MassFlow;

        [KSPField]
        public string topNode = "SRBtop";

        [KSPField]
        public bool endOfStack = false;

        public override void OnAwake()
        {
            if (MassFlow == null)
                MassFlow = new FloatCurve();
        }

        public bool isFuelRemaining()
        {
            if (this.part.GetResourceMass() > 0)
                return true;
            else
                return false;
        }

        public float CalcMassFlow(float time)
        {
            return MassFlow.Evaluate(time);
        }
    }

    public class KSF_SolidBooster_Analyze : PartModule
    {
        private Single i = 0;
        private string sOutput = "";

        [KSPField]
        private Single stepSize = .1f;

        [KSPEvent(guiActive = true, guiName = "Analyze burn", active = true)]
        private void AnalyzeBurn()
        {
            KSF_SolidBoosterSegment srb = this.part.GetComponent<KSF_SolidBoosterSegment>();
            {
                i = 0;
                sOutput += this.part.ToString() + "," + this.part.GetResourceMass() + Environment.NewLine;
                do
                {
                    sOutput += i + "," + Mathf.Max(0,srb.CalcMassFlow(i)) + Environment.NewLine;
                    i += stepSize;
                } while (i < 200.0f);
                WriteFile();
                return;
            }
        }

        private void WriteFile()
        {
            KSP.IO.File.WriteAllText<KSF_SolidBooster_Analyze>(sOutput, this.part.ToString() + ".txt");
        }
    }

    public class KSF_SolidBoosterNozzle : PartModule
    {
        #region Character arrays declaration
            KSF_CharArray Spacech = new KSF_CharArray();
            KSF_CharArray Sch = new KSF_CharArray();
            KSF_CharArray Tch = new KSF_CharArray();
            KSF_CharArray Ach = new KSF_CharArray();
            KSF_CharArray Cch = new KSF_CharArray();
            KSF_CharArray ch1 = new KSF_CharArray();
            KSF_CharArray ch2 = new KSF_CharArray();
            KSF_CharArray ch3 = new KSF_CharArray();
            KSF_CharArray ch4 = new KSF_CharArray();
            KSF_CharArray ch5 = new KSF_CharArray();
            KSF_CharArray ch6 = new KSF_CharArray();
            KSF_CharArray ch7 = new KSF_CharArray();
            KSF_CharArray ch8 = new KSF_CharArray();
            KSF_CharArray ch9 = new KSF_CharArray();
            KSF_CharArray ch0 = new KSF_CharArray();
            KSF_CharArray chs = new KSF_CharArray();
            KSF_CharArray chk = new KSF_CharArray();
            KSF_CharArray cht = new KSF_CharArray();
        #endregion

        string simDuration;
        bool EditorGUIvisible;



        [KSPField]
        public string sResourceType;

        [KSPField]
        public FloatCurve acISP;

        [KSPField]
        private string thrustTransform = "thrustTransform";

        [KSPField]
        public float resourceDensity = .00975f;

        private Transform tThrustTransform;

        protected Rect EditorGUIPos = new Rect(0,0,0 ,0);

        private Texture2D graphImg = new Texture2D(640,480);

        [KSPField(isPersistant = true)]
        private bool hasFired = false;

        [KSPField(isPersistant = true)]
        private bool notExhausted = true;

        [KSPField(guiName = "Isp", guiActive = true, guiFormat = "F2")]
        public float fCurrentIsp = 0f;

        [KSPField(guiActive = true, guiName = "Force", guiUnits = " kN", guiFormat = "F2")]
        public float fForce = 0f;

        [KSPField(guiActive = true, guiName = "Fuel Mass Flow", guiUnits = " t/s", guiFormat = "F4")]
        private float fFuelFlowMass = 0f;

        [KSPField(isPersistant = true, guiActive = true, guiName = "T+", guiUnits = " s", guiFormat = "F2")]
        public float fEngRunTime = 0f;

        [KSPField(guiActive = false, isPersistant = true)]
        private bool hasAborted = false;

        [KSPField]
        public Vector3 fxOffset = Vector3.zero;

        [KSPAction("Ignite")]
        private void TryIgnite(KSPActionParam a)
        {
            if (hasFired)
                return;
            else
                this.part.force_activate();
        }

        [KSPAction("Abort")]
        private void Abort(KSPActionParam a)
        {
            hasAborted = true;
        }

        private Part p;
        private bool bEndofFuelSearch;
        private int i;

        private float fMassFlow = 0;

        private FXGroup gRunning;
        private FXGroup gFlameout;

        [KSPField]
        public string topNode = "SRBtop";

        System.Collections.Generic.List<Part> FuelSourcesList = new System.Collections.Generic.List<Part>(); //filled once during OnActivate, is the master list
        System.Collections.Generic.List<Part> CurrentFuelSourcesList = new System.Collections.Generic.List<Part>(); //filled everytime the vehicle part count changes.

        private int iVesselPartCount;

        private bool isExploding;

        public override void OnAwake()
        {
            gRunning = this.part.findFxGroup("running");
            gFlameout = this.part.findFxGroup("flameout");

            tThrustTransform = this.part.FindModelTransform(thrustTransform);

            if (acISP == null)
                acISP = new FloatCurve();
        }

        public override void OnStart(StartState state)
        {
            if ((EditorGUIPos.x == 0) && (EditorGUIPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                EditorGUIPos = new Rect(Screen.width / 2, Screen.height / 2, 10, 10);
            }

            for (int y = 0; y < graphImg.height; y++)
            {
                for (int x = 0; x < graphImg.width; x++)
                {
                    graphImg.SetPixel(x, y, Color.black);
                }
            }


            RenderingManager.AddToPostDrawQueue(3, new Callback(drawEditorGUI));//start the GUI
        }

        public override void OnActive()
        {
            if (notExhausted) //the if is here to prevent SRBIgnite() being called when loading a scene (ie, if there are booster nozzles scattered around the KSC, they get forceactivated)
            {
                isExploding = false;
                SRBIgnite();
            }
        }

        public override void OnInactive()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawEditorGUI)); //close the GUI
        }

        public override void OnFixedUpdate()
        {
            HeatSegments();
            if (hasFired && notExhausted)
            {

                //This block of code is used to evaluate if the booster has had an unplanned autonomous dissassembly event (it broke apart)
                if (hasPartCountChanged() && isExploding != true)
                {
                    FuelStackSearcher(CurrentFuelSourcesList);
                    if (FuelSourcesList.Count != CurrentFuelSourcesList.Count)
                    {
                        //These are debugging lines. I'm going to leave them here until final release I think.
                        //print("New fuel sources are different. I think the booster broke");

                        //foreach (Part fcp in FuelSourcesList)
                        //{
                        //    print("FuelSourcesList contains: " + fcp.name + fcp.GetInstanceID());
                        //}
                        //foreach (Part fcp in CurrentFuelSourcesList)
                        //{
                        //    print("CurrentFuelSourcesList contains: " + fcp.name + fcp.GetInstanceID());
                        //}

                        isExploding = true;
                    }
                }
                RunEngClock();
                CalcCurrentIsp();
                RunFuelFlow();
                DoApplyEngine();

                gRunning.SetPower(Mathf.Min(1.5f, fFuelFlowMass * 10f)); //fx, will move to OnUpdate eventually

                if (notExhausted == false)
                    SRBExtinguish();


            }
        }


        /// <summary>
        /// RunEngClock ticks the engine run clock forward
        /// </summary>
        private void RunEngClock()
        {
            fEngRunTime += TimeWarp.fixedDeltaTime;
        }


        /// <summary>
        /// hasPartCountChanged basically determines if the part count of the vessel has changed.
        /// </summary>
        /// <returns>True or false depending upon if the part count of the vessel has changed</returns>
        private bool hasPartCountChanged()
        {
            if (iVesselPartCount == 0)
            {
                iVesselPartCount = vessel.parts.Count;
                print("Initial Part count = " + iVesselPartCount);
                return false;
            }

            if (vessel.parts.Count != iVesselPartCount)
            {
                iVesselPartCount = vessel.parts.Count;
                return true;
            }

            return false;
        }


        /// <summary>
        /// RunFuelFlow gets the fuel from the connected SRB segments and adds their mass flow to the nozzle. It also determines if there is fuel in the stack.
        /// </summary>
        private void RunFuelFlow()
        {
            notExhausted = false;
            fFuelFlowMass = 0f;
            foreach (Part p in FuelSourcesList)
            {
                KSF_SolidBoosterSegment srb = p.GetComponent<KSF_SolidBoosterSegment>();
                notExhausted = (notExhausted | srb.isFuelRemaining());
                fMassFlow = Mathf.Max(0, srb.CalcMassFlow(fEngRunTime));
                fFuelFlowMass += (p.RequestResource(sResourceType, (fMassFlow / resourceDensity) * TimeWarp.fixedDeltaTime)) * resourceDensity;
            }
        }


        /// <summary>
        /// CalcCurrentIsp() calculates the current Isp of the nozzle, taking into account whether or not the abort sequence has fired
        /// </summary>
        private void CalcCurrentIsp()
        {
            if (hasAborted)
                fCurrentIsp = acISP.Evaluate((float)FlightGlobals.getStaticPressure()) / 15f;
            else
                fCurrentIsp = acISP.Evaluate((float)FlightGlobals.getStaticPressure());
        }


        /// <summary>
        /// DoApplyEngine() applies a force to the nozzle (simulating the thrust)
        /// Also it converts a few variables into a "per second" format versus "per physics frame" for display in the context menu
        /// </summary>
        private void DoApplyEngine()
        {
            fForce = 9.81f * fCurrentIsp * fFuelFlowMass / TimeWarp.fixedDeltaTime;

            part.Rigidbody.AddForceAtPosition(tThrustTransform.forward * fForce * -1, this.part.rigidbody.position);

            fFuelFlowMass = fFuelFlowMass / TimeWarp.fixedDeltaTime;
        }


        /// <summary>
        /// SRBIgnite() does a number of things
        /// 1. It sets some variables to let the nozzle know that the booster has been fired
        /// 2. It sets up the effects, turning them on and positioning them in the proper location
        /// </summary>
        private void SRBIgnite()
        {
            hasFired = true;

            foreach (FXGroup e in this.part.fxGroups)
            {
                foreach (GameObject o in e.fxEmitters)
                {
                    o.transform.SetParent(tThrustTransform);

                    //I might want to flip the following two lines, as effects seem to be offset in directions they should not be. But that's what the next version is for.
                    o.transform.position += fxOffset;
                    o.transform.Rotate(-90, 0, 0);
                }
            }
            gRunning.setActive(true);
            gRunning.audio.Play();

            FuelStackSearcher(FuelSourcesList);
        }


        /// <summary>
        /// SRBExtinguish() is run when the booster stack is out of fuel, and it only controls effects.
        /// </summary>
        private void SRBExtinguish()
        {
            gRunning.setActive(false);
            gRunning.audio.Stop();
            gFlameout.Burst();
        }


        /// <summary>
        /// HeatSegments is an attempt to have failing boosters break by adding heat to the part rapidly
        /// </summary>
        private void HeatSegments()
        {
            if (isExploding)
            {
                //print("Heating Up segements");

                FuelSourcesList.RemoveAll(item => item == null);


                if (FuelSourcesList.Contains(this.part))
                {
                    if (FuelSourcesList.Count == 1)
                    {
                        this.part.explode();
                    }

                    foreach (Part p in FuelSourcesList)
                    {
                        p.temperature += UnityEngine.Random.Range(20, 200); //I want to refine this so that parts are even still more randomly heated...they tend to explode within a few frames of one another. I may move this to OnUpdate()
                    }

                    this.part.temperature = 100;
                }
                else
                {
                    if (FuelSourcesList.Count == 0)
                    {
                        this.part.explode();
                    }

                    foreach (Part p in FuelSourcesList)
                    {
                        p.temperature += UnityEngine.Random.Range(20, 200); //I want to refine this so that parts are even still more randomly heated...they tend to explode within a few frames of one another. I may move this to OnUpdate()
                    }
                }
            }
        }

        ///// <summary>
        ///// Detonate is an attempt to make SRB segment seperation more...realistic. If LFTs seperate, no big deal as the tanks themselves are not containing combustion, but
        ///// SRB segments are actually containing rapid burning, so if they seperate bad times are a'come'n. So Detonate is activated when the SRB stack loses parts and engulfs itself in flame
        ///// Ideally, this would have a slight time delay between booster segment detonations, but I just want to get it working first. *Update, this sub is now obsolete, by HeatSegments()
        ///// </summary>
        //private void DetonateStack()
        //{
        //    //print("Number of Parts to be detonated: " + FuelSourcesList.Count);

        //    do
        //    {
        //        p = FuelSourcesList[FuelSourcesList.Count - 1];

        //        FuelSourcesList.Remove(p);

        //        p.explode();
        //    } while (FuelSourcesList.Count > 0);

        //    this.part.explode();
        //}


        ///// <summary>
        ///// FuelStackSearch() will be the new method of determining which SRB segments are connected to the nozzle
        ///// It will run at the start of each flight, each load, and also whenever the vessel loses parts
        ///// The intent of changing the location of this from being run OnUpdate() is both performance and to allow a list of connected parts\
        /////     which will be useful for when there is a rapid disassembly event (which will eventually lead to the disconnected parts exploding)
        ///// It is possible that running this in the editors will enable an in-editor display of the thrust curve of the motors
        ///// </summary>
        //private void FuelStackSearch()
        //{
        //    print("Begin Stack Search");
        //    FuelSourcesList.Clear();
        //    i = 0;

        //    p = this.part;
        //    AttachNode n = new AttachNode();

        //    KSF_SolidBoosterSegment SRB = new KSF_SolidBoosterSegment();

        //    p = this.part;
        //    n = p.findAttachNode(topNode);

        //    print("1");
        //    do
        //    {
        //        print("2");
        //        if (n.attachedPart.Modules.Contains("KSF_SolidBoosterSegment"))
        //        {
        //            print("3");
        //            p = n.attachedPart;

        //            if (FuelSourcesList.Contains(p) != true)
        //                FuelSourcesList.Add(p);


        //            print(p.ToString());

        //            print("4");
        //            SRB = p.GetComponent<KSF_SolidBoosterSegment>();

        //            if (SRB.endOfStack != true)
        //            {
        //                print("4.5");
        //                print(SRB.topNode);
        //                foreach (AttachNode k in p.attachNodes)
        //                {
        //                    print(k.id);
        //                }

        //                //n = p.findAttachNode("SRBtop");

        //                foreach (AttachNode k in p.attachNodes)
        //                {
        //                    print("Looper");
        //                    if (k.id == SRB.topNode)
        //                    {
        //                        print("I haven't died yet");

        //                        n = k;
        //                        print("I think i am dead");
        //                        goto Finish;
        //                    }
        //                }

        //            Finish:

        //                if (n.attachedPart == null)
        //                    bEndofFuelSearch = true;

        //            }
        //            else
        //            {
        //                bEndofFuelSearch = true;
        //                print("6");
        //            }
        //        }
        //        else
        //            bEndofFuelSearch = true;

        //        i += 1;

        //    } while (bEndofFuelSearch == false && i < 250);
        //    print("7");

        //    if (i == 250)
        //    {
        //        print("SolidBoosterNozzle: Forced out of loop");
        //        print("SolidBoosterNozzle: Loop executed 250 times");
        //    }

        //    foreach (Part fcp in FuelSourcesList)
        //    {
        //        print("FuelSourcesList contains: " + fcp.name + fcp.GetInstanceID());
        //    }
        //}

        /// <summary>
        /// This is the combined FuelStackSearch and FuelStackReSearch. It should be a bit more reliable as well as preventing me from updating one segment of code and not the other (which would likley lead to autonomus explosions)
        /// So this sub basically finds all the valid fuel sources for the booster segment, it should work fine. I've left debugging prints commented out because I'm sure I'll need them again.
        /// </summary>
        /// <param name="pl"></param>
        private void FuelStackSearcher(System.Collections.Generic.List<Part> pl)
        {
            print("Begin Stack Search (Generic)");
            pl.Clear();
            i = 0;
            bEndofFuelSearch = false;

            p = this.part;
            AttachNode n = new AttachNode();

            KSF_SolidBoosterSegment SRB = new KSF_SolidBoosterSegment();

            p = this.part;
            n = p.findAttachNode(topNode);

            //new code to allow one part boosters
            if (p.Modules.Contains("KSF_SolidBoosterSegment"))
            {
                if (pl.Contains(p) != true)
                    pl.Add(p);

                SRB = p.GetComponent<KSF_SolidBoosterSegment>();

                if (SRB.endOfStack)
                    goto Leave;
            }
            //end of new code for one part boosters

            //print("1");

            do
            {
                //print("2");
                if (n.attachedPart.Modules.Contains("KSF_SolidBoosterSegment"))
                {
                    //print("3");
                    p = n.attachedPart;

                    if (pl.Contains(p) != true)
                        pl.Add(p);

                    //print("4");

                    SRB = p.GetComponent<KSF_SolidBoosterSegment>();

                    if (SRB.endOfStack != true)
                    {
                        //print("5");
                        //n = p.findAttachNode("SRBtop"); This line is better than the following foreach loop, but it was giving me some weird behavior...if I can be sure it works I'll probably reinstate it.

                        foreach (AttachNode k in p.attachNodes)
                        {
                            //print("6");
                            if (k.id == SRB.topNode)
                            {
                                //print("7");
                                n = k;

                                goto Finish;
                            }
                        }

                    Finish:

                        if (n.attachedPart == null)
                        {
                            //print("null");
                            bEndofFuelSearch = true;
                        }
                    }
                    else
                    {
                        //print("I am here");
                        bEndofFuelSearch = true;
                    }
                }
                else
                    bEndofFuelSearch = true;

                i += 1;

            } while (bEndofFuelSearch == false && i < 250);

            //foreach (Part fcp in pl)
            //{
            //    print("pl contains: " + fcp.name + fcp.GetInstanceID());
            //}

            if (i == 250) //I hope to hell no one builds a rocket such that 250 iterations is restricting an otherwise valid design. Because damn.
            {
                print("SolidBoosterNozzle: Forced out of loop");
                print("SolidBoosterNozzle: Loop executed 250 times");
            }

        Leave:
            ;
        }

        ///// <summary>
        ///// FuelStackReSearch() is basically the same as FuelStackSearch, except that it is called when the vehicle changes part count and assigns found parts into
        ///// CurrentFuelSourcesList. The code may be more efficent if I combine the two subs (it'd be super simple to do), but I'd bet the user is willing to sacrifice a few kB for my sanity ;)
        ///// </summary>
        //private void FuelStackReSearch()
        //{
        //    print("Begin Stack RESearch");
        //    CurrentFuelSourcesList.Clear();
        //    i = 0;

        //    p = this.part;
        //    AttachNode n = new AttachNode();

        //    KSF_SolidBoosterSegment SRB = new KSF_SolidBoosterSegment();

        //    p = this.part;
        //    n = p.findAttachNode(topNode);

        //    print("1re");
        //    do
        //    {
        //        print("2re");
        //        if (n.attachedPart.Modules.Contains("KSF_SolidBoosterSegment"))
        //        {
        //            print("3re");
        //            p = n.attachedPart;

        //            if (CurrentFuelSourcesList.Contains(p) != true)
        //                CurrentFuelSourcesList.Add(p);

        //            print("4re");
        //            SRB = p.GetComponent<KSF_SolidBoosterSegment>();

        //            if (SRB.endOfStack != true)
        //            {
        //                n = p.findAttachNode(SRB.topNode);
        //                print("5re");
        //            }
        //            else
        //            {
        //                bEndofFuelSearch = true;
        //                print("6re");
        //            }

        //            if (n.attachedPart == null)
        //                bEndofFuelSearch = true;

        //        }
        //        else
        //            bEndofFuelSearch = true;

        //        i += 1;

        //    } while (bEndofFuelSearch == false && i < 250);
        //    print("7re");

        //    if (i == 250)
        //    {
        //        print("SolidBoosterNozzle: Forced out of loop");
        //        print("SolidBoosterNozzle: Loop executed 250 times");
        //    }

        //    foreach (Part fcp in FuelSourcesList)
        //    {
        //        print("CurrentFuelSourcesList contains: " + fcp.name + fcp.GetInstanceID());
        //    }
        //}


        #region GUI graph maker
        /// <summary>
        /// This will return an image of the specified size with the thrust graph for this nozzle
        /// Totally jacked the image code from the wiki http://wiki.kerbalspaceprogram.com/wiki/Module_code_examples, with some modifications
        /// </summary>
        /// <param name="imgH">Hight of the desired output, in pixels</param>
        /// <param name="imgW">Width of the desired output, in pixels</param>
        /// <param name="burnTime">Chart for burn time in seconds</param>
        /// <returns></returns>
        private void thrustPredictPic(int imgH, int imgW, int burnTime)
        {
            //First we set up the analyzer
            //Step 1: Figure out fuel sources
            FuelStackSearcher(FuelSourcesList);
            //Step 2: Set up mass variables
            float stackTotalMass = 0f;
            stackTotalMass = CalcStackWetMass(FuelSourcesList, stackTotalMass);

            float[] segmentFuelArray = new float[FuelSourcesList.Count];

            int i = 0;
            foreach (Part p in FuelSourcesList)
            {
                segmentFuelArray[i] = p.GetResourceMass();
                i++;
            }

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
            //Step 4: Define text arrays
            populateCharArrays();
            //Step 5a: Define time markings (every 10 seconds gets a verticle line)
            System.Collections.Generic.List<int> timeLines = new System.Collections.Generic.List<int>();
            double xScale = (imgW - 40) / (double)burnTime;
            //print("xScale: " + xScale);
            calcTimeLines((float)burnTime, 10,timeLines, (float)xScale);
            //Step 5b: Define vertical line markings (9 total to give 10 sections)
            System.Collections.Generic.List<int> horzLines = new System.Collections.Generic.List<int>();
            calcHorizLines(imgH - graphYmin, horzLines, 9);
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
            int graphArraySize = Convert.ToInt16(Convert.ToDouble(simDuration) / simStep);
            double[,] graphArray = new double[graphArraySize, 4];


            //one time setups
            graphArray[i, 0] = stackMassFlow(FuelSourcesList, (float)(i * simStep),segmentFuelArray, simStep);
            graphArray[i, 1] = stackTotalMass;
            graphArray[i, 2] = 9.81 * acISP.Evaluate(1) * graphArray[i,0];
            graphArray[i, 3] = graphArray[i, 2] - (graphArray[i, 1] * 9.81);


            //fForce = 9.81f * fCurrentIsp * fFuelFlowMass / TimeWarp.fixedDeltaTime;
            do
            {
                i++;
                graphArray[i, 0] = stackMassFlow(FuelSourcesList, (float)(i * simStep), segmentFuelArray, simStep);
                graphArray[i, 1] = graphArray[i-1,1]-(graphArray[i-1,0] * simStep);
                graphArray[i, 2] = 9.81 * acISP.Evaluate(1) * graphArray[i, 0];
                graphArray[i, 3] = graphArray[i, 2] - (graphArray[i, 1] * 9.81);

                if (graphArray[i, 2] > peakThrustAmt)
                {
                    peakThrustAmt = graphArray[i, 2];
                    //peakThrustTime = i;
                }
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

            print("graphed scales");

            //Step 8c: Graph the mass
            int lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale,x - 20,yScaleMass,graphArray, simStep, graphArraySize,1);

                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, brightG);
                }
            }

            //Step 8d: Graph the thrust
            lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - 20, yScaleThrust, graphArray, simStep, graphArraySize,2);
                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, brightG);
                }
            }

            //Step 8e: Graph the thrust extra
            lineWidth = 3;
            for (int x = graphXmin; x < graphXmax; x++)
            {
                int fx = fGraph(xScale, x - 20, yScaleThrust, graphArray, simStep, graphArraySize, 3);
                for (int y = fx; y < fx + lineWidth; y++)
                {
                    image.SetPixel(x, y, brightG);
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
                s = i*10 + " s";

                //print("composite string: " + s);

                length = calcStringPixLength(convertStringToCharArray(s));
                //print("length: " + length);

                startpos = Mathf.FloorToInt((float)pos - 0.5f * (float)length);

                Color[] region = image.GetPixels(startpos, 23, length, 11);

                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;   
                }

                image.SetPixels(startpos, 23, length, 11, region);

                tex = convertCharArrayToTex(convertStringToCharArray(s), length, brightG);

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
                length = calcStringPixLength(convertStringToCharArray(s));
                startpos = Mathf.FloorToInt((float)pos - 5f);
                Color[] region = image.GetPixels(23, startpos, length, 11);
                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }
                image.SetPixels(23, startpos, length, 11, region);
                tex = convertCharArrayToTex(convertStringToCharArray(s), length, brightG);
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
                length = calcStringPixLength(convertStringToCharArray(s));
                startpos = Mathf.FloorToInt((float)pos - 5f);
                Color[] region = image.GetPixels(imgW-60, startpos, length, 11);
                for (int c = 0; c < region.Length; c++)
                {
                    region[c] = brightG;
                }
                image.SetPixels(imgW - 60, startpos, length, 11, region);
                tex = convertCharArrayToTex(convertStringToCharArray(s), length, brightG);
                Color[] region2 = tex.GetPixels();
                image.SetPixels(imgW - 58, startpos + 2, length - 4, 7, region2);
                length = 0;
            } while (i < horzLines.Count);



            //Step 9a: Boxes for labels

            //Step 9b: labels

            image.Apply();
            graphImg = image;
        }

        private int fGraph(double xScale, int xPos, double yScale, double[,] array, double step, int arraySize, int column)
        {
            double t = 0;
            int i = 0;
            t = xPos / xScale;
            t = Mathf.Floor((float)(t / step));
            i = Convert.ToInt16(t);
            i = Mathf.Clamp(i, 0, arraySize - 1);
            //print(yScale + " : " + array[i,column]);
            return (Convert.ToInt16(yScale * array[i, column]) + 20);
        }


        private int calcStringPixLength(System.Collections.Generic.List<KSF_CharArray> list)
        {
            int i = 2;

            foreach (KSF_CharArray c in list)
            {
                i += c.charWidth;
            }
            i++;
            return i;
        }

        private System.Collections.Generic.List<KSF_CharArray> convertStringToCharArray(string s)
        {
            System.Collections.Generic.List<KSF_CharArray> chArray = new System.Collections.Generic.List<KSF_CharArray>();
            chArray.Clear();

            foreach (char c in s)
            {
                if (c == '1')
                {
                    chArray.Add(ch1);
                    continue;
                }

                if (c == '2')
                {
                    chArray.Add(ch2);
                    continue;
                }

                if (c == '3')
                {
                    chArray.Add(ch3);
                    continue;
                }

                if (c == '4')
                {
                    chArray.Add(ch4);
                    continue;
                }

                if (c == '5')
                {
                    chArray.Add(ch5);
                    continue;
                }

                if (c == '6')
                {
                    chArray.Add(ch6);
                    continue;
                }

                if (c == '7')
                {
                    chArray.Add(ch7);
                    continue;
                }

                if (c == '8')
                {
                    chArray.Add(ch8);
                    continue;
                }

                if (c == '9')
                {
                    chArray.Add(ch9);
                    continue;
                }

                if (c == '0')
                {
                    chArray.Add(ch0);
                    continue;
                }

                if (c == 's')
                {
                    chArray.Add(chs);
                    continue;
                }

                if (c == 'A')
                {
                    chArray.Add(Ach);
                    continue;
                }

                if (c == ' ')
                {
                    chArray.Add(Spacech);
                    continue;
                }

                if (c == 'k')
                {
                    chArray.Add(chk);
                    continue;
                }

                if (c == 't')
                {
                    chArray.Add(cht);
                    continue;
                }
            }

            return chArray;
        }

        private Texture2D convertCharArrayToTex(System.Collections.Generic.List<KSF_CharArray> chArray, int length, Color color)
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
                    if (pxMap.Contains(new Vector2(x,y)))
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

        private void calcTimeLines(float duration, float spacing, System.Collections.Generic.List<int> list, float xscale)
        {
            list.Clear();
            float f = 0f;
            int result = 0;
            int i = 1;
            do
            {
                f = (i * spacing * xscale);
                result = Mathf.RoundToInt(f)+20;
                list.Add(result);
                print("Added Vertical line at: " + result);
                i++;
            } while (i < duration/spacing);
        }

        private void calcHorizLines(float yHeight, System.Collections.Generic.List<int> list, int numLines)
        {
            list.Clear();
            float f = 0f;
            int result = 0;
            int i = 1;
            do
            {
                f = (i * yHeight)/(numLines+1);
                result = Mathf.RoundToInt(f)+20;
                list.Add(result);
                //print("Added horizontal line at: " + result);
                i++;
            } while (i<= numLines);
        }

        private float stackMassFlow(System.Collections.Generic.List<Part> pl, float t, float[] a, double step)
        {
            int i = 0;
            float totalFlow = 0f;
            foreach (Part p in pl)
            {
                KSF_SolidBoosterSegment srb = p.GetComponent<KSF_SolidBoosterSegment>();

                if(a[i] > 0 && srb.CalcMassFlow(t) > 0)
                {
                    totalFlow += srb.CalcMassFlow(t);
                    a[i] -= (float)(srb.CalcMassFlow(t)*step);
                }
                i++;
            }
            return totalFlow;
        }

        private float CalcStackWetMass(System.Collections.Generic.List<Part> pl, float totalMass)
        {
            totalMass = 0f;
            foreach (Part p in pl)
            {
                totalMass += p.mass + p.GetResourceMass();
            }
            totalMass += part.mass;
            return totalMass;
        }

        public void populateCharArrays()
        {
            Spacech.pixelList.Clear();
            Spacech.charName = "Space";
            Spacech.charWidth = 2;

            Sch.pixelList.Clear();
            Sch.charName = "S";
            Sch.charWidth = 5;
            Sch.pixelList.Add(new Vector2(0, 5));
            Sch.pixelList.Add(new Vector2(0, 4));
            Sch.pixelList.Add(new Vector2(0, 1));
            Sch.pixelList.Add(new Vector2(1, 6));
            Sch.pixelList.Add(new Vector2(1, 3));
            Sch.pixelList.Add(new Vector2(1, 0));
            Sch.pixelList.Add(new Vector2(2, 6));
            Sch.pixelList.Add(new Vector2(2, 3));
            Sch.pixelList.Add(new Vector2(2, 0));
            Sch.pixelList.Add(new Vector2(3, 5));
            Sch.pixelList.Add(new Vector2(3, 4));
            Sch.pixelList.Add(new Vector2(3, 1));

            Tch.pixelList.Clear();
            Tch.charName = "T";
            Tch.charWidth = 6;
            Tch.pixelList.Add(new Vector2(0, 6));
            Tch.pixelList.Add(new Vector2(1, 6));
            Tch.pixelList.Add(new Vector2(2, 6));
            Tch.pixelList.Add(new Vector2(3, 6));
            Tch.pixelList.Add(new Vector2(4, 6));
            Tch.pixelList.Add(new Vector2(2, 1));
            Tch.pixelList.Add(new Vector2(2, 2));
            Tch.pixelList.Add(new Vector2(2, 3));
            Tch.pixelList.Add(new Vector2(2, 4));
            Tch.pixelList.Add(new Vector2(2, 5));
            Tch.pixelList.Add(new Vector2(2, 0));

            Ach.pixelList.Clear();
            Ach.charName = "A";
            Ach.charWidth = 6;
            Ach.pixelList.Add(new Vector2(0, 3));
            Ach.pixelList.Add(new Vector2(0, 2));
            Ach.pixelList.Add(new Vector2(0, 1));
            Ach.pixelList.Add(new Vector2(0, 0));
            Ach.pixelList.Add(new Vector2(4, 3));
            Ach.pixelList.Add(new Vector2(4, 2));
            Ach.pixelList.Add(new Vector2(4, 1));
            Ach.pixelList.Add(new Vector2(4, 0));
            Ach.pixelList.Add(new Vector2(1, 5));
            Ach.pixelList.Add(new Vector2(1, 4));
            Ach.pixelList.Add(new Vector2(1, 2));
            Ach.pixelList.Add(new Vector2(3, 5));
            Ach.pixelList.Add(new Vector2(3, 4));
            Ach.pixelList.Add(new Vector2(3, 2));
            Ach.pixelList.Add(new Vector2(2, 6));
            Ach.pixelList.Add(new Vector2(2, 2));

            Cch.pixelList.Clear();
            Cch.charName = "C";
            Cch.charWidth = 6;
            Cch.pixelList.Add(new Vector2(0, 4));
            Cch.pixelList.Add(new Vector2(0, 3));
            Cch.pixelList.Add(new Vector2(0, 2));
            Cch.pixelList.Add(new Vector2(1, 5));
            Cch.pixelList.Add(new Vector2(1, 1));
            Cch.pixelList.Add(new Vector2(2, 6));
            Cch.pixelList.Add(new Vector2(2, 0));
            Cch.pixelList.Add(new Vector2(3, 6));
            Cch.pixelList.Add(new Vector2(3, 0));
            Cch.pixelList.Add(new Vector2(4, 5));
            Cch.pixelList.Add(new Vector2(4, 1));



            ch1.pixelList.Clear();
            ch1.charName = "1";
            ch1.charWidth = 5;
            ch1.pixelList.Add(new Vector2(0, 0));
            ch1.pixelList.Add(new Vector2(0, 4));
            ch1.pixelList.Add(new Vector2(1, 0));
            ch1.pixelList.Add(new Vector2(1, 1));
            ch1.pixelList.Add(new Vector2(1, 2));
            ch1.pixelList.Add(new Vector2(1, 3));
            ch1.pixelList.Add(new Vector2(1, 4));
            ch1.pixelList.Add(new Vector2(1, 5));
            ch1.pixelList.Add(new Vector2(2, 0));
            ch1.pixelList.Add(new Vector2(2, 1));
            ch1.pixelList.Add(new Vector2(2, 2));
            ch1.pixelList.Add(new Vector2(2, 3));
            ch1.pixelList.Add(new Vector2(2, 4));
            ch1.pixelList.Add(new Vector2(2, 5));
            ch1.pixelList.Add(new Vector2(2, 6));
            ch1.pixelList.Add(new Vector2(3, 0));
            
            ch2.pixelList.Clear();
            ch2.charName = "2";
            ch2.charWidth = 6;
            ch2.pixelList.Add(new Vector2(0, 0));
            ch2.pixelList.Add(new Vector2(0, 4));
            ch2.pixelList.Add(new Vector2(0, 5));
            ch2.pixelList.Add(new Vector2(1, 0));
            ch2.pixelList.Add(new Vector2(1, 1));
            ch2.pixelList.Add(new Vector2(1, 5));
            ch2.pixelList.Add(new Vector2(1, 6));
            ch2.pixelList.Add(new Vector2(2, 0));
            ch2.pixelList.Add(new Vector2(2, 1));
            ch2.pixelList.Add(new Vector2(2, 2));
            ch2.pixelList.Add(new Vector2(2, 6));
            ch2.pixelList.Add(new Vector2(3, 0));
            ch2.pixelList.Add(new Vector2(3, 2));
            ch2.pixelList.Add(new Vector2(3, 3));
            ch2.pixelList.Add(new Vector2(3, 5));
            ch2.pixelList.Add(new Vector2(3, 6));
            ch2.pixelList.Add(new Vector2(4, 0));
            ch2.pixelList.Add(new Vector2(4, 3));
            ch2.pixelList.Add(new Vector2(4, 4));
            ch2.pixelList.Add(new Vector2(4, 5));

            ch3.pixelList.Clear();
            ch3.charName = "3";
            ch3.charWidth = 6;
            ch3.pixelList.Add(new Vector2(0, 1));
            ch3.pixelList.Add(new Vector2(0, 2));
            ch3.pixelList.Add(new Vector2(0, 4));
            ch3.pixelList.Add(new Vector2(0, 5));
            ch3.pixelList.Add(new Vector2(1, 0));
            ch3.pixelList.Add(new Vector2(1, 1));
            ch3.pixelList.Add(new Vector2(1, 5));
            ch3.pixelList.Add(new Vector2(1, 6));
            ch3.pixelList.Add(new Vector2(2, 0));
            ch3.pixelList.Add(new Vector2(2, 3));
            ch3.pixelList.Add(new Vector2(2, 6));
            ch3.pixelList.Add(new Vector2(3, 0));
            ch3.pixelList.Add(new Vector2(3, 1));
            ch3.pixelList.Add(new Vector2(3, 2));
            ch3.pixelList.Add(new Vector2(3, 3));
            ch3.pixelList.Add(new Vector2(3, 4));
            ch3.pixelList.Add(new Vector2(3, 5));
            ch3.pixelList.Add(new Vector2(3, 6));
            ch3.pixelList.Add(new Vector2(4, 1));
            ch3.pixelList.Add(new Vector2(4, 2));
            ch3.pixelList.Add(new Vector2(4, 4));
            ch3.pixelList.Add(new Vector2(4, 5));

            ch4.pixelList.Clear();
            ch4.charName = "4";
            ch4.charWidth = 6;
            ch4.pixelList.Add(new Vector2(0, 2));
            ch4.pixelList.Add(new Vector2(0, 3));
            ch4.pixelList.Add(new Vector2(0, 4));
            ch4.pixelList.Add(new Vector2(1, 2));
            ch4.pixelList.Add(new Vector2(1, 4));
            ch4.pixelList.Add(new Vector2(1, 5));
            ch4.pixelList.Add(new Vector2(2, 2));
            ch4.pixelList.Add(new Vector2(3, 0));
            ch4.pixelList.Add(new Vector2(3, 1));
            ch4.pixelList.Add(new Vector2(3, 2));
            ch4.pixelList.Add(new Vector2(3, 3));
            ch4.pixelList.Add(new Vector2(3, 4));
            ch4.pixelList.Add(new Vector2(3, 5));
            ch4.pixelList.Add(new Vector2(3, 6));
            ch4.pixelList.Add(new Vector2(4, 0));
            ch4.pixelList.Add(new Vector2(4, 1));
            ch4.pixelList.Add(new Vector2(4, 2));
            ch4.pixelList.Add(new Vector2(4, 3));
            ch4.pixelList.Add(new Vector2(4, 4));
            ch4.pixelList.Add(new Vector2(4, 5));
            ch4.pixelList.Add(new Vector2(4, 6));

            ch5.pixelList.Clear();
            ch5.charName = "5";
            ch5.charWidth = 5;
            ch5.pixelList.Add(new Vector2(3, 1));
            ch5.pixelList.Add(new Vector2(3, 2));
            ch5.pixelList.Add(new Vector2(3, 6));
            ch5.pixelList.Add(new Vector2(1, 0));
            ch5.pixelList.Add(new Vector2(1, 3));
            ch5.pixelList.Add(new Vector2(1, 6));
            ch5.pixelList.Add(new Vector2(2, 0));
            ch5.pixelList.Add(new Vector2(2, 1));
            ch5.pixelList.Add(new Vector2(2, 2));
            ch5.pixelList.Add(new Vector2(2, 3));
            ch5.pixelList.Add(new Vector2(2, 6));
            ch5.pixelList.Add(new Vector2(0, 0));
            ch5.pixelList.Add(new Vector2(0, 3));
            ch5.pixelList.Add(new Vector2(0, 4));
            ch5.pixelList.Add(new Vector2(0, 5));
            ch5.pixelList.Add(new Vector2(0, 6));

            ch6.pixelList.Clear();
            ch6.charName = "6";
            ch6.charWidth = 6;
            ch6.pixelList.Add(new Vector2(0, 1));
            ch6.pixelList.Add(new Vector2(0, 2));
            ch6.pixelList.Add(new Vector2(0, 3));
            ch6.pixelList.Add(new Vector2(1, 0));
            ch6.pixelList.Add(new Vector2(1, 3));
            ch6.pixelList.Add(new Vector2(1, 4));
            ch6.pixelList.Add(new Vector2(1, 5));
            ch6.pixelList.Add(new Vector2(2, 0));
            ch6.pixelList.Add(new Vector2(2, 3));
            ch6.pixelList.Add(new Vector2(2, 5));
            ch6.pixelList.Add(new Vector2(2, 6));
            ch6.pixelList.Add(new Vector2(3, 0));
            ch6.pixelList.Add(new Vector2(3, 1));
            ch6.pixelList.Add(new Vector2(3, 2));
            ch6.pixelList.Add(new Vector2(3, 3));
            ch6.pixelList.Add(new Vector2(3, 6));
            ch6.pixelList.Add(new Vector2(4, 1));
            ch6.pixelList.Add(new Vector2(4, 2));
            ch6.pixelList.Add(new Vector2(4, 6));

            ch7.pixelList.Clear();
            ch7.charName = "7";
            ch7.charWidth = 5;
            ch7.pixelList.Add(new Vector2(0, 5));
            ch7.pixelList.Add(new Vector2(0, 6));
            ch7.pixelList.Add(new Vector2(1, 0));
            ch7.pixelList.Add(new Vector2(1, 1));
            ch7.pixelList.Add(new Vector2(1, 5));
            ch7.pixelList.Add(new Vector2(1, 6));
            ch7.pixelList.Add(new Vector2(2, 1));
            ch7.pixelList.Add(new Vector2(2, 2));
            ch7.pixelList.Add(new Vector2(2, 3));
            ch7.pixelList.Add(new Vector2(2, 5));
            ch7.pixelList.Add(new Vector2(2, 6));
            ch7.pixelList.Add(new Vector2(3, 3));
            ch7.pixelList.Add(new Vector2(3, 4));
            ch7.pixelList.Add(new Vector2(3, 5));
            ch7.pixelList.Add(new Vector2(3, 6));

            ch8.pixelList.Clear();
            ch8.charName = "8";
            ch8.charWidth = 6;
            ch8.pixelList.Add(new Vector2(0, 1));
            ch8.pixelList.Add(new Vector2(0, 2));
            ch8.pixelList.Add(new Vector2(0, 4));
            ch8.pixelList.Add(new Vector2(0, 5));
            ch8.pixelList.Add(new Vector2(1, 0));
            ch8.pixelList.Add(new Vector2(1, 3));
            ch8.pixelList.Add(new Vector2(1, 6));
            ch8.pixelList.Add(new Vector2(2, 0));
            ch8.pixelList.Add(new Vector2(2, 3));
            ch8.pixelList.Add(new Vector2(2, 6));
            ch8.pixelList.Add(new Vector2(3, 0));
            ch8.pixelList.Add(new Vector2(3, 1));
            ch8.pixelList.Add(new Vector2(3, 2));
            ch8.pixelList.Add(new Vector2(3, 3));
            ch8.pixelList.Add(new Vector2(3, 4));
            ch8.pixelList.Add(new Vector2(3, 5));
            ch8.pixelList.Add(new Vector2(3, 6));
            ch8.pixelList.Add(new Vector2(4, 1));
            ch8.pixelList.Add(new Vector2(4, 2));
            ch8.pixelList.Add(new Vector2(4, 5));
            ch8.pixelList.Add(new Vector2(4, 6));

            ch9.pixelList.Clear();
            ch9.charName = "9";
            ch9.charWidth = 6;
            ch9.pixelList.Add(new Vector2(0, 3));
            ch9.pixelList.Add(new Vector2(0, 4));
            ch9.pixelList.Add(new Vector2(0, 5));
            ch9.pixelList.Add(new Vector2(1, 2));
            ch9.pixelList.Add(new Vector2(1, 3));
            ch9.pixelList.Add(new Vector2(1, 5));
            ch9.pixelList.Add(new Vector2(1, 6));
            ch9.pixelList.Add(new Vector2(2, 2));
            ch9.pixelList.Add(new Vector2(2, 6));
            ch9.pixelList.Add(new Vector2(3, 0));
            ch9.pixelList.Add(new Vector2(3, 1));
            ch9.pixelList.Add(new Vector2(3, 2));
            ch9.pixelList.Add(new Vector2(3, 3));
            ch9.pixelList.Add(new Vector2(3, 4));
            ch9.pixelList.Add(new Vector2(3, 5));
            ch9.pixelList.Add(new Vector2(3, 6));
            ch9.pixelList.Add(new Vector2(4, 0));
            ch9.pixelList.Add(new Vector2(4, 1));
            ch9.pixelList.Add(new Vector2(4, 2));
            ch9.pixelList.Add(new Vector2(4, 3));
            ch9.pixelList.Add(new Vector2(4, 4));
            ch9.pixelList.Add(new Vector2(4, 5));

            ch0.pixelList.Clear();
            ch0.charName = "0";
            ch0.charWidth = 6;
            ch0.pixelList.Add(new Vector2(1, 1));
            ch0.pixelList.Add(new Vector2(1, 1));
            ch0.pixelList.Add(new Vector2(1, 2));
            ch0.pixelList.Add(new Vector2(1, 5));
            ch0.pixelList.Add(new Vector2(1, 6));
            ch0.pixelList.Add(new Vector2(2, 0));
            ch0.pixelList.Add(new Vector2(2, 3));
            ch0.pixelList.Add(new Vector2(2, 6));
            ch0.pixelList.Add(new Vector2(3, 0));
            ch0.pixelList.Add(new Vector2(3, 1));
            ch0.pixelList.Add(new Vector2(3, 4));
            ch0.pixelList.Add(new Vector2(3, 5));
            ch0.pixelList.Add(new Vector2(3, 6));
            ch0.pixelList.Add(new Vector2(4, 1));
            ch0.pixelList.Add(new Vector2(4, 2));
            ch0.pixelList.Add(new Vector2(4, 3));
            ch0.pixelList.Add(new Vector2(4, 4));
            ch0.pixelList.Add(new Vector2(4, 5));
            ch0.pixelList.Add(new Vector2(0, 1));
            ch0.pixelList.Add(new Vector2(0, 2));
            ch0.pixelList.Add(new Vector2(0, 3));
            ch0.pixelList.Add(new Vector2(0, 4));
            ch0.pixelList.Add(new Vector2(0, 5));

            chs.pixelList.Clear();
            chs.charName = "sec";
            chs.charWidth = 4;
            chs.pixelList.Add(new Vector2(0, 0));
            chs.pixelList.Add(new Vector2(0, 2));
            chs.pixelList.Add(new Vector2(0, 3));
            chs.pixelList.Add(new Vector2(0, 4));
            chs.pixelList.Add(new Vector2(2, 0));
            chs.pixelList.Add(new Vector2(2, 2));
            chs.pixelList.Add(new Vector2(2, 1));
            chs.pixelList.Add(new Vector2(2, 4));
            chs.pixelList.Add(new Vector2(1, 0));
            chs.pixelList.Add(new Vector2(1, 2));
            chs.pixelList.Add(new Vector2(1, 4));

            chk.pixelList.Clear();
            chk.charName = "kN";
            chk.charWidth = 10;
            chk.pixelList.Add(new Vector2(0, 0));
            chk.pixelList.Add(new Vector2(0, 1));
            chk.pixelList.Add(new Vector2(0, 2));
            chk.pixelList.Add(new Vector2(0, 3));
            chk.pixelList.Add(new Vector2(0, 4));
            chk.pixelList.Add(new Vector2(1, 1));
            chk.pixelList.Add(new Vector2(2, 0));
            chk.pixelList.Add(new Vector2(2, 2));
            chk.pixelList.Add(new Vector2(4, 0));
            chk.pixelList.Add(new Vector2(4, 1));
            chk.pixelList.Add(new Vector2(4, 2));
            chk.pixelList.Add(new Vector2(4, 3));
            chk.pixelList.Add(new Vector2(4, 4));
            chk.pixelList.Add(new Vector2(4, 5));
            chk.pixelList.Add(new Vector2(5, 4));
            chk.pixelList.Add(new Vector2(5, 5));
            chk.pixelList.Add(new Vector2(6, 2));
            chk.pixelList.Add(new Vector2(6, 3));
            chk.pixelList.Add(new Vector2(7, 1));
            chk.pixelList.Add(new Vector2(7, 0));
            chk.pixelList.Add(new Vector2(8, 0));
            chk.pixelList.Add(new Vector2(8, 1));
            chk.pixelList.Add(new Vector2(8, 2));
            chk.pixelList.Add(new Vector2(8, 3));
            chk.pixelList.Add(new Vector2(8, 4));
            chk.pixelList.Add(new Vector2(8, 5));

            cht.pixelList.Clear();
            cht.charName = "t";
            cht.charWidth = 4;
            cht.pixelList.Add(new Vector2(0, 3));
            cht.pixelList.Add(new Vector2(1, 0));
            cht.pixelList.Add(new Vector2(1, 1));
            cht.pixelList.Add(new Vector2(1, 2));
            cht.pixelList.Add(new Vector2(1, 3));
            cht.pixelList.Add(new Vector2(1, 4));
            cht.pixelList.Add(new Vector2(2, 0));
            cht.pixelList.Add(new Vector2(2, 3));
        }

        private void EditorWindowGUI(int windowID)
        {



                GUIStyle mySty = new GUIStyle(GUI.skin.button);
                mySty.normal.textColor = mySty.focused.textColor = Color.white;
                mySty.hover.textColor = mySty.active.textColor = Color.yellow;
                mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
                mySty.padding = new RectOffset(8, 8, 8, 8);

                if (simDuration == null)
                    simDuration = "200";


                GUILayout.BeginVertical();
                simDuration = GUI.TextField(new Rect(10, 10, 100, 20), simDuration);
                if (GUILayout.Button("Generate", mySty, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))//GUILayout.Button is "true" when clicked
                {
                    thrustPredictPic(480, 640, Convert.ToInt16(simDuration));
                }
                //GUILayout.Space(480);

                //simDuration = GUI.TextField(new Rect(20, 20, 100, 20), simDuration);

                GUILayout.Box(graphImg, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUILayout.EndVertical();



                //string textFieldString = "200";
                //textFieldString = GUI.TextField(new Rect(220, 510, 100, 20), textFieldString);

                //GUI.Box(new Rect(20,20,640,480),graphImg);

                //textFieldString = GUI.TextField(new Rect(220, 510, 100, 20), textFieldString);

                //if (GUI.Button(new Rect(20, 510, 180, 20), "Generate"))
                //{
                //    thrustPredictPic(480, 640, Convert.ToInt16(textFieldString));
                //}




                GUI.DragWindow(new Rect(0, 0, 10000, 20));
            
        }

        private void drawEditorGUI()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (EditorGUIvisible == null)
                {
                    EditorGUIvisible = false;
                }

                if (Input.GetKeyDown("home") && this.part.stackIcon.highlightIcon)
                {
                    EditorGUIvisible = true;
                }

                if (Input.GetKeyDown("end") && this.part.stackIcon.highlightIcon)
                {
                    EditorGUIvisible = false;
                }



                if (EditorGUIvisible)
                {
                    GUI.skin = HighLogic.Skin;
                    EditorGUIPos = GUILayout.Window(1, EditorGUIPos, EditorWindowGUI, "AdvSRB Analyzer", GUILayout.MinWidth(680), GUILayout.MinHeight(520));
                }
            }

        }


        #endregion
    }

    public class KSF_CharArray
    {
        public System.Collections.Generic.List<Vector2> pixelList = new System.Collections.Generic.List<Vector2>();
        public string charName;
        public int charWidth;
    }



    /// <summary>
    /// KSF_SeperationGimbal is an attempt to provide additional help in removing spent boosters from a stack by gimbaling the engine in a certain direction upon activation
    /// Currently, this is probably only useful on KSF AdvSRBs as they provide some thrust while being removed; a LFE would be full throttle and probably push waaaay past, blowing things up.
    /// However, a potential solution to this is to force the engine it's a part of to some lower, configurable level of thrust, maybe 10% of current throttle setting? Of course
    /// then the engine would be difficult to orient correctly in the VAB, as most engines are not designed to be clear which direction is which. (perhaps a line in the VAB?)
    /// </summary>
    public class KSF_SeperationGimbal : PartModule
    {
        [KSPField]
        private string gimbalTransformName = "thrustTransform";

        private Transform tTransformProxy;

        [KSPField]
        public float gimbalX = 0;

        [KSPField]
        public float gimbalY = 0;

        public Vector3 gimbalDirection = Vector3.zero;

        [KSPField(isPersistant = true)]
        public bool hasGimballed = false;

        [KSPAction("Seperation Gimbal")]
        private void SeperationGimbal(KSPActionParam a)
        {
            if (hasGimballed == false)
            {
                hasGimballed = true;

                foreach (PartModule m in this.part.Modules)
                {
                    if (m.ClassName == "ModuleGimbal")
                    {
                        ModuleGimbal g = (ModuleGimbal)m;
                        g.LockGimbal();
                    }
                }

                gimbalDirection.x = gimbalX;
                gimbalDirection.y = gimbalY;
            }
        }

        public override void OnAwake()
        {
            tTransformProxy = this.part.FindModelTransform(gimbalTransformName);
        }

        public override void OnFixedUpdate()
        {
            if (hasGimballed)
                Gimbal();
        }

        private void Gimbal()
        {  
            {
                tTransformProxy.Rotate(gimbalDirection);
                print("Seperation Gimbal");
            }
        }
    }
}
