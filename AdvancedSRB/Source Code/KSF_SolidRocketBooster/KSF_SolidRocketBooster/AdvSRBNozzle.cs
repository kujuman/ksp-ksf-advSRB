using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using UnityEngine;
using KSF_SolidRocketBooster;


namespace KSF_SolidRocketBooster
{
    [KSPModule("aSRB Nozzle")]
    public class AdvSRBNozzle : PartModule
    {
        //moved from segments
        [KSPField(isPersistant = true)]
        public string BurnProfile = "";

        public AnimationCurve MassFlow;

        public float estBurnTime = 0;

        public float fullStackFuelMass;

        [KSPField]
        public int defaultBurnTime = 60;

        public float pressurePulseHertz = 2;

        public float mixtureDensity = 0;

        public System.Collections.Generic.List<Transform> thrustTransforms;

        //public System.Collections.Generic.List<Transform> fxTransforms;

        [KSPField]
        public float heatProductionPerkN = .25f;

        [KSPField]
        public FloatCurve atmosphereCurve;

        [KSPField]
        public string thrustTransform = "thrustTransform";

        [KSPField]
        public string resourceName = "SolidFuel";

        [KSPField]
        public string effectGroupName = "running";

        [KSPField]
        public float fullEffectAtThrust = 300;

        //private Transform tThrustTransform;

        [KSPField(isPersistant = true)]
        public bool hasFired = false;

        [KSPField(isPersistant = true)]
        private bool notExhausted = true;

        [KSPField(guiName = "Isp", guiActive = true, guiFormat = "F2")]
        public float fCurrentIsp = 0f;

        [KSPField(guiActive = true, guiName = "Force", guiUnits = " kN", guiFormat = "F2")]
        public float finalThrust2 = 0f;

        [KSPField(guiActive = true, guiName = "Fuel Mass Flow", guiUnits = " t/s", guiFormat = "F4")]
        private float fFuelFlowMass = 0f;

        [KSPField(isPersistant = true, guiActive = true, guiName = "T+", guiUnits = " s", guiFormat = "F2")]
        public float fEngRunTime = 0f;

        [KSPField(guiActive = false, isPersistant = true)]
        private bool hasAborted = false;

        [KSPField]
        public float maxContThrust = 1000f;

        [KSPField]
        public float maxPeakThrust = 2000f;

        [KSPField]
        public bool useLegacyFX = false;

        string status;
        string statusL2;

        [KSPField(guiActive = false)]
        float realIsp;

        [KSPField(guiActive = false)]
        float fuelFlowGui;

        //[KSPField(guiActiveEditor = false, guiActive = false)]
        //new float thrustPercentage = 1;

        private Part p;
        private bool bEndofFuelSearch;
        private int i;

        private float fMassFlow = 0;

        private FXGroup gRunning;
        private FXGroup gFlameout;

        [KSPField]
        public string topNode = "top";

        public System.Collections.Generic.List<Part> FuelSourcesList = new System.Collections.Generic.List<Part>(); //filled once during OnActivate, is the master list
        public System.Collections.Generic.List<Part> CurrentFuelSourcesList = new System.Collections.Generic.List<Part>(); //filled everytime the vehicle part count changes.

        private int iVesselPartCount;

        private bool isExploding;


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

        public float estimateBurnDuration()
        {
            this.estBurnTime = 0;
            int i = 0;

            float massFracRemain = 1;
            do
            {
                massFracRemain -= this.MassFlow.Evaluate(i);

                i++;
            } while (massFracRemain > 0);
            
            this.estBurnTime = i;
            return i;
        }

        public override void OnFixedUpdate()
        {

            //FixedUpdate();
            if (hasFired && notExhausted)
            {
                if (hasPartCountChanged() && isExploding != true)
                {
                    FuelStackSearcher(CurrentFuelSourcesList);
                    if (FuelSourcesList.Count != CurrentFuelSourcesList.Count)
                    {
                        isExploding = true;
                    }
                }
                RunEngClock();
                CalcCurrentIsp();
                RunFuelFlow();
                DoApplyEngine();

                HeatSegments();

                //if (exhaustDamage)
                //    EngineExhaustDamage();

                if (notExhausted == false)
                    SRBExtinguish();
            }

            fuelFlowGui = fFuelFlowMass / mixtureDensity;
            realIsp = fCurrentIsp;
        }

        //new public void FixedUpdate()
        //{
        //    var ps = this.vessel.parts;

        //    foreach (Part p in ps)
        //    {
        //        foreach (PartModule pm in p.Modules)
        //        {
        //            if (pm.GetType() == typeof(ModuleScienceContainer))
        //            {
        //                ModuleScienceContainer sc = (ModuleScienceContainer)pm;
        //                var sd = sc.GetData();

        //                foreach (ScienceData d in sd)
        //                {
        //                    bool sur = d.title.Contains("surface");

                          

        //                    //GameEvents.on
        //                }
        //                ;
        //            }
        //        }
        //    }



        //    ;
        //}


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
                AdvSRBSegment srb = p.GetComponent<AdvSRBSegment>();
                notExhausted = (notExhausted | srb.isFuelRemaining(resourceName));
                fMassFlow = AdvSRBUtils.CalcMassFlow(fEngRunTime, MassFlow) * srb.startLoadedFuelMass;
                fFuelFlowMass += (p.RequestResource(resourceName, (fMassFlow / mixtureDensity) * TimeWarp.fixedDeltaTime)) * mixtureDensity;
            }
        }


        /// <summary>
        /// CalcCurrentIsp() calculates the current Isp of the nozzle, taking into account whether or not the abort sequence has fired
        /// </summary>
        private void CalcCurrentIsp()
        {
                fCurrentIsp = atmosphereCurve.Evaluate((float)FlightGlobals.getStaticPressure());

                //add a pressure pulse

                //UnityEngine.Random.seed = this.part.GetHashCode();

            //+ UnityEngine.Random.Range(0f,1f))

                fCurrentIsp += 0.05f * fCurrentIsp * Mathf.Sin(fEngRunTime  * Mathf.PI * 2 * pressurePulseHertz);

                if (hasAborted) fCurrentIsp *= .1f;
        }

        /// <summary>
        /// DoApplyEngine() applies a force to the nozzle (simulating the thrust)
        /// Also it converts a few variables into a "per second" format versus "per physics frame" for display in the context menu
        /// </summary>
        private void DoApplyEngine()
        {
            //Debug.Log("Before " + MethodBase.GetCurrentMethod().Name);//__ Requires System.Reflection

            finalThrust2 = 9.80665f * fCurrentIsp * fFuelFlowMass / TimeWarp.fixedDeltaTime;

           // finalThrust = finalThrust2;



           // maxThrust = finalThrust;
           // minThrust = maxThrust;

            //Debug.Log(finalThrust);
            //Debug.Log(tThrustTransform.forward.ToString()); //__ this breaks when useLegacyEffect = true
            //Debug.Log(this.part.rigidbody.position.ToString());

            //Debug.Log("Thrust Transform Count: " + thrustTransforms.Count());

            if(thrustTransforms.Count() == 0)
            {
                GetThrustTransforms();

                //fxTransforms.Clear();

                //foreach (Transform t in thrustTransforms)
                //{
                //    Transform fxTransform = Instantiate(t) as Transform;

                //    fxTransform.parent = t.transform;

                //    fxTransform.name = "fxTransform";

                //    fxTransform.localPosition += Vector3.forward * 1f;

                //    fxTransforms.Add(fxTransform);

                //}

                //part.InitializeEffects();

                //Debug.Log("Transforms found: " + thrustTransforms.Count());
            }

            int i = 0;
            Transform t;
            do
            {
                t = thrustTransforms[i];

                Vector3d offset = new Vector3d(UnityEngine.Random.Range(-.15f,.15f),UnityEngine.Random.Range(-.15f,.15f),0);

                //offset.Normalize();



                Vector3d pos = t.position + offset;

                part.Rigidbody.AddForceAtPosition(thrustTransforms[i].forward * (finalThrust2 / thrustTransforms.Count()) * -1, pos);
                i++;
            }
            while (i < thrustTransforms.Count());




            fFuelFlowMass = fFuelFlowMass / TimeWarp.fixedDeltaTime;
        }

        /// <summary>
        /// This runs once per graphics frame, so it should slightly reduce CPU workload on physX frames. It controls the runnning effects of the rocket.
        /// </summary>
        override public void OnUpdate()
        {
            //Debug.Log("thrustTransforms count = " + thrustTransforms.Count() + "OnUpdate");

            float pwr = 0f;

            if (hasFired && notExhausted)
                //pwr = Mathf.Clamp01((finalThrust) / fullEffectAtThrust);
                pwr = 1;

            if (useLegacyFX)
                gRunning.SetPower(pwr);
            else
                part.Effect(effectGroupName, pwr);

            

            fuelFlowGui = fFuelFlowMass / mixtureDensity;
            realIsp = fCurrentIsp;
        }


        /// <summary>
        /// SRBIgnite() does a number of things
        /// 1. It sets some variables to let the nozzle know that the booster has been fired
        /// </summary>
        public void Ignite()
        {
            if (hasFired == false)
            {
                hasFired = true;
                isEnabled = true;
                //EngineIgnited = true;

                //Debug.Log("thrustTransforms count = " + thrustTransforms.Count() + "Ignite");


                this.part.force_activate();

                StackSearchAndFuel();

                //FuelStackSearcher(FuelSourcesList);

                //foreach(Part p in FuelSourcesList)
                //{
                //    AdvSRBSegment srb = p.GetComponent<AdvSRBSegment>();

                //    srb.startLoadedFuelMass = 0;

                //    srb.startLoadedFuelMass += (float)srb.calcStartFuelLoadMass(resourceName);
                //}


                if (useLegacyFX)
                {
                        gRunning.setActive(true);
                }

                //Debug.Log("thrustTransforms count = " + thrustTransforms.Count() + "IgniteEnd");
            }
        }

        public void DestroyStack()
        {
                    Collider[] colliders;

            foreach (Part p in this.FuelSourcesList)
            {
                if (p != this.part)
                {
                    p.explosionPotential = .5f + p.GetResourceMass()/p.mass;
                    
                    float radius = p.GetResourceMass();

                    colliders = Physics.OverlapSphere(p.GetReferenceTransform().position, radius);

                    foreach (Collider hit in colliders)
                    {
                        if(hit && hit.rigidbody)
                        {
                            hit.rigidbody.AddExplosionForce(1000* radius, p.GetReferenceTransform().position,radius);
                        }
                    }
                    
                    p.explode();
                }
            }
                this.part.explode();
        }

        public void StackSearchAndFuel()
        {
            FuelStackSearcher(FuelSourcesList);

            foreach (Part p in FuelSourcesList)
            {
                AdvSRBSegment srb = p.GetComponent<AdvSRBSegment>();

                srb.startLoadedFuelMass = 0;

                srb.startLoadedFuelMass += (float)srb.calcStartFuelLoadMass(resourceName);
            }
        }

        /// <summary>
        /// SRBExtinguish() is run when the booster stack is out of fuel, and it only controls effects.
        /// </summary>
        private void SRBExtinguish()
        {
            if (useLegacyFX)
            {
                gRunning.setActive(false);
                gRunning.audio.Stop();
                gFlameout.Burst();
            }
            else
                part.Effect(effectGroupName, 0);
        }


        /// <summary>
        /// HeatSegments is an attempt to have failing boosters break by adding heat to the part rapidly
        /// </summary>
        private void HeatSegments()
        {
            int numSegments = 0;

            float heatProduction = 0;

            heatProduction = (this.heatProductionPerkN * this.finalThrust2 * TimeWarp.fixedDeltaTime)/(float)numSegments;

            heatProduction = this.heatProductionPerkN * this.fFuelFlowMass * TimeWarp.fixedDeltaTime / (float)numSegments * 0.001f;

            numSegments = this.FuelSourcesList.Count;

            if (!this.FuelSourcesList.Contains(this.part))
            {
                numSegments++;
            }



            heatProduction = (this.heatProductionPerkN * this.finalThrust2 * TimeWarp.fixedDeltaTime) / (float)numSegments;
            //we apply heat now: we have all the info we need and we save a list search.
            Debug.Log("AdvSRB: " + numSegments + " segs found, " + heatProduction + " heat pro");

            if (!this.FuelSourcesList.Contains(this.part))
            {
                this.part.temperature += heatProduction;
                Debug.Log("AdvSRB: Part Temp " + this.part.name + " " + this.part.temperature + "/" + this.part.maxTemp);

                if(this.part.temperature > this.part.maxTemp)
                {
                    isExploding = true;
                    DestroyStack();
                }
            }

            //DrawGUIHeating(this.part);

            foreach(Part p in this.FuelSourcesList)
            {
                p.temperature += heatProduction;

                Debug.Log("AdvSRB: PartTemp " + p.name + " " + p.temperature + "/" + p.maxTemp);

                if (p.temperature > p.maxTemp)
                {
                    isExploding = true;
                    DestroyStack();
                }

                //p.GetComponent<AdvSRBSegment>().HeatingBoxUpdate(p.temperature/p.maxTemp);

            }

            if (isExploding)
            {
                foreach (Part p in this.FuelSourcesList)
                {
                    p.explosionPotential = 25;
                    p.explode();
                }
                this.part.explode();
            }




            ////if (!isExploding)
            ////{
            ////    ////this.part.temperature += (fForce * 600 / maxContThrust) * TimeWarp.fixedDeltaTime;
            ////    //this.part.temperature += (finalThrust * 600 / maxContThrust) * TimeWarp.fixedDeltaTime;

            ////    //if (this.part.temperature > .75 * this.part.maxTemp) //ensure maxContThrust is abided by
            ////    //{
            ////    //    if (finalThrust < 1.1 * maxContThrust)
            ////    //    {
            ////    //        this.part.temperature -= 200 * TimeWarp.fixedDeltaTime;
            ////    //    }
            ////    //}


            ////    //if (finalThrust > maxPeakThrust * 1.1) //110% of peak thrust rating
            ////    //{
            ////    //    System.Random r = new System.Random();

            ////    //    if (r.NextDouble() > 0.90f)
            ////    //    {
            ////    //        this.part.temperature += 10000;
            ////    //    }
            ////    //}
            ////    //if (this.part.temperature > this.part.maxTemp) //explode stack
            ////    //{
            ////    //    foreach (Part p in FuelSourcesList)
            ////    //    {
            ////    //        p.temperature += 10000;
            ////    //    }
            ////    //}
            ////}

            //if (isExploding)
            //{
            //    FuelSourcesList.RemoveAll(item => item == null);

            //    if (FuelSourcesList.Contains(this.part))
            //    {
            //        if (FuelSourcesList.Count == 1 && FuelSourcesList.Contains(this.part) || FuelSourcesList.Count == 0)
            //        {
            //            this.part.explode();
            //        }

            //        foreach (Part p in FuelSourcesList)
            //        {
            //            p.temperature += UnityEngine.Random.Range(20, 200); //I want to refine this so that parts are even still more randomly heated...they tend to explode within a few frames of one another. I may move this to OnUpdate()
            //        }
            //        this.part.temperature = 100;
            //    }
            //    else
            //    {
            //        if (FuelSourcesList.Count == 0)
            //        {
            //            this.part.explode();
            //        }

            //        foreach (Part p in FuelSourcesList)
            //        {
            //            p.temperature += UnityEngine.Random.Range(20, 200); //I want to refine this so that parts are even still more randomly heated...they tend to explode within a few frames of one another. I may move this to OnUpdate()
            //        }
            //    }
            //}
        }

        public void DrawGUIHeating(AdvSRBSegment s)
        {
            

        }

        /// <summary>
        /// This is the combined FuelStackSearch and FuelStackReSearch. It should be a bit more reliable as well as preventing me from updating one segment of code and not the other (which would likley lead to autonomus explosions)
        /// So this sub basically finds all the valid fuel sources for the booster segment, it should work fine.
        /// </summary>
        /// <param name="pl"></param>
        public void FuelStackSearcher(System.Collections.Generic.List<Part> pl)
        {
            fullStackFuelMass = 0;

            pl.Clear();
            i = 0;
            bEndofFuelSearch = false;

            p = this.part;
            AttachNode n = new AttachNode();

            AdvSRBSegment SRB = new AdvSRBSegment();

            p = this.part;
            n = p.findAttachNode(topNode);

            if (p.Modules.Contains("AdvSRBSegment"))
            {
                if (pl.Contains(p) != true)
                {
                    pl.Add(p);

                    p.GetComponent<AdvSRBSegment>().startLoadedFuelMass = p.GetComponent<AdvSRBSegment>().calcStartFuelLoadMass(resourceName);

                    fullStackFuelMass += p.GetComponent<AdvSRBSegment>().startLoadedFuelMass;
                }

                SRB = p.GetComponent<AdvSRBSegment>();

                if (SRB.endOfStack)
                    goto Leave;
            }

            do
            {
                if (n.attachedPart.Modules.Contains("AdvSRBSegment"))
                {
                    p = n.attachedPart;

                    if (pl.Contains(p) != true)
                    {
                        pl.Add(p);

                        p.GetComponent<AdvSRBSegment>().startLoadedFuelMass = p.GetComponent<AdvSRBSegment>().calcStartFuelLoadMass(resourceName);

                        fullStackFuelMass += p.GetComponent<AdvSRBSegment>().startLoadedFuelMass;
                    }

                    SRB = p.GetComponent<AdvSRBSegment>();

                    if (SRB.endOfStack != true)
                    {
                        foreach (AttachNode k in p.attachNodes)
                        {
                            if (k.id == SRB.topNode)
                            {
                                n = k;
                                goto Finish;
                            }
                        }

                    Finish:

                        if (n.attachedPart == null)
                        {
                            bEndofFuelSearch = true;
                        }
                    }
                    else
                    {
                        bEndofFuelSearch = true;
                    }
                }
                else
                    bEndofFuelSearch = true;

                i += 1;

            } while (bEndofFuelSearch == false && i < 250);

            if (i == 250) //I hope to hell no one builds a rocket such that 250 iterations is restricting an otherwise valid design. Because damn.
            {
                print("SolidBoosterNozzle: Forced out of loop");
                print("SolidBoosterNozzle: Loop executed 250 times");
            }

        Leave:
            ;
        }

        public override void OnActive()
        {
            if (notExhausted) //the if is here to prevent SRBIgnite() being called when loading a scene (ie, if there are booster nozzles scattered around the KSC, they get forceactivated)
            {
                mixtureDensity = AdvSRBUtils.GetResourceDensity(resourceName);

                isExploding = false;
                Ignite();
            }
        }

        private void GetThrustTransforms()
        {
            //Debug.Log("Looking for transforms: " + thrustTransform);

            thrustTransforms = new System.Collections.Generic.List<Transform>();
            thrustTransforms.Clear();
            foreach (Transform t in this.part.FindModelTransforms(thrustTransform))
            {
                thrustTransforms.Add(t);
            }
        }

        public override void OnAwake()
        {
            GetThrustTransforms();

            if (AdvSRBUtils.GetResourceDensity(resourceName) == -1.0f)
                Debug.LogError("Problem getting density of " + resourceName);
            else
                mixtureDensity = AdvSRBUtils.GetResourceDensity(resourceName);

            if (atmosphereCurve == null)
                atmosphereCurve = new FloatCurve();

          


            if (useLegacyFX)
            {
                gRunning = this.part.findFxGroup("running");
                gFlameout = this.part.findFxGroup("flameout");
                gRunning.setActive(false);
                gFlameout.setActive(false);
            }
        }



        public override string GetInfo()
        {
            string displayInfo = "";

            displayInfo += "<b><color=#99ff00ff>Propellant:</color></b>";

            displayInfo += Environment.NewLine;
            displayInfo += "-" + resourceName;

            displayInfo += Environment.NewLine;
            displayInfo += Environment.NewLine;
            displayInfo += "Max Rated Continuous Thrust: " + maxContThrust.ToString("F1") + " kN";
            displayInfo += Environment.NewLine;
            displayInfo += "<color=#E03C31ff>Max Rated Peak Thrust: </color>" + maxPeakThrust.ToString("F1") + " kN";
            displayInfo += Environment.NewLine;


            displayInfo += "Isp: " + atmosphereCurve.Evaluate(1) + "s (Atm) - " + atmosphereCurve.Evaluate(0) + "s (Vac)";

            return displayInfo;
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("Loading AdvSRBNozzle");
            MassFlow = AdvSRBUtils.AnimationCurveFromString(BurnProfile, defaultBurnTime);
        }
    }
}
