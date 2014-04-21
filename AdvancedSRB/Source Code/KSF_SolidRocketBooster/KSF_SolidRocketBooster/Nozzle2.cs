using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;

namespace KSF_SolidRocketBooster
{
    public class KSF_SBNozzle : PartModule
    {
        [KSPField]
        public FloatCurve atmosphereCurve;

        [KSPField]
        private string thrustTransform = "thrustTransform";

        [KSPField]
        public float resourceDensity = .00975f;

        [KSPField]
        public string resourceName = "SolidFuel";

        [KSPField]
        public string effectGroupName = "running";

        [KSPField]
        public float fullEffectAtThrust = 300;

        private Transform tThrustTransform;

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


        private Part p;
        private bool bEndofFuelSearch;
        private int i;

        private float fMassFlow = 0;

        //private FXGroup gRunning;
        //private FXGroup gFlameout;

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





        public override void OnAwake()
        {
            tThrustTransform = this.part.FindModelTransform(thrustTransform);

            if (GetResourceDensity(resourceName) == -1.0f)
                Debug.LogError("Problem getting density of " + resourceName);
            else
                resourceDensity = GetResourceDensity(resourceName);

            if (atmosphereCurve == null)
                atmosphereCurve = new FloatCurve();
        }


        private float GetResourceDensity(string ResourceName)
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

        public override void OnStart(StartState state)
        {
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

        }

        #region SRB code which actually does stuff on the physical side

        public override void OnFixedUpdate()
        {
            HeatSegments();
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

                if (notExhausted == false)
                    SRBExtinguish();
            }
        }

        /// <summary>
        /// This runs once per graphics frame, so it should slightly reduce CPU workload on physX frames. It controls the runnning effects of the rocket.
        /// </summary>
        public override void OnUpdate()
        {
            float pwr = 0f;

            if (notExhausted && hasFired)
            {
                pwr = Mathf.Clamp01((9.81f * fCurrentIsp * fFuelFlowMass) / fullEffectAtThrust);
                part.Effect(effectGroupName, pwr);
            }
            else
                part.Effect(effectGroupName, pwr);
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
                notExhausted = (notExhausted | srb.isFuelRemaining(resourceName));
                fMassFlow = Mathf.Max(0, srb.CalcMassFlow(fEngRunTime));
                fFuelFlowMass += (p.RequestResource(resourceName, (fMassFlow / resourceDensity) * TimeWarp.fixedDeltaTime)) * resourceDensity;
            }
        }


        /// <summary>
        /// CalcCurrentIsp() calculates the current Isp of the nozzle, taking into account whether or not the abort sequence has fired
        /// </summary>
        private void CalcCurrentIsp()
        {
            if (hasAborted)
                fCurrentIsp = atmosphereCurve.Evaluate((float)FlightGlobals.getStaticPressure()) / 15f;
            else
                fCurrentIsp = atmosphereCurve.Evaluate((float)FlightGlobals.getStaticPressure());
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
        /// </summary>
        private void SRBIgnite()
        {
            if (hasFired == false)
            {
                hasFired = true;
                this.part.force_activate();

                FuelStackSearcher(FuelSourcesList);
            }
        }


        /// <summary>
        /// SRBExtinguish() is run when the booster stack is out of fuel, and it only controls effects.
        /// </summary>
        private void SRBExtinguish()
        {
            //gRunning.setActive(false);
            //gRunning.audio.Stop();
            //gFlameout.Burst();
            part.Effect(effectGroupName, 0);
        }


        /// <summary>
        /// HeatSegments is an attempt to have failing boosters break by adding heat to the part rapidly
        /// </summary>
        private void HeatSegments()
        {
            if (isExploding)
            {
                FuelSourcesList.RemoveAll(item => item == null);

                if (FuelSourcesList.Contains(this.part))
                {
                    if (FuelSourcesList.Count == 1 && FuelSourcesList.Contains(this.part) || FuelSourcesList.Count == 0)
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

        /// <summary>
        /// This is the combined FuelStackSearch and FuelStackReSearch. It should be a bit more reliable as well as preventing me from updating one segment of code and not the other (which would likley lead to autonomus explosions)
        /// So this sub basically finds all the valid fuel sources for the booster segment, it should work fine.
        /// </summary>
        /// <param name="pl"></param>
        public void FuelStackSearcher(System.Collections.Generic.List<Part> pl)
        {
            pl.Clear();
            i = 0;
            bEndofFuelSearch = false;

            p = this.part;
            AttachNode n = new AttachNode();

            KSF_SolidBoosterSegment SRB = new KSF_SolidBoosterSegment();

            p = this.part;
            n = p.findAttachNode(topNode);

            if (p.Modules.Contains("KSF_SolidBoosterSegment"))
            {
                if (pl.Contains(p) != true)
                    pl.Add(p);

                SRB = p.GetComponent<KSF_SolidBoosterSegment>();

                if (SRB.endOfStack)
                    goto Leave;
            }

            do
            {
                if (n.attachedPart.Modules.Contains("KSF_SolidBoosterSegment"))
                {
                    p = n.attachedPart;

                    if (pl.Contains(p) != true)
                        pl.Add(p);

                    SRB = p.GetComponent<KSF_SolidBoosterSegment>();

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

        #endregion

       
    }
}
