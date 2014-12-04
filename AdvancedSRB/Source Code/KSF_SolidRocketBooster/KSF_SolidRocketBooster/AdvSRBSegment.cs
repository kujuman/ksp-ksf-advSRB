/*
 * Kerbal Science Foundation Advanced Solid Rocket Booster v0.6.1 for Kerbal Space Program
 * Released May 4, 2014 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 * For attribution, please attribute "kujuman"
 */


using System;
using KSP;
using UnityEngine;

namespace KSF_SolidRocketBooster
{
    [KSPModule("SRB Segment")]
    public class AdvSRBSegment : PartModule
    {
        [KSPField]
        public string topNode = "top";

        [KSPField]
        public bool endOfStack = false;

        public VInfoBox heatingBox;

        public float startLoadedFuelMass;

        public override void OnActive()
        {
            VInfoBox box = this.part.stackIcon.DisplayInfo();
            box.SetLength(4f);
            box.SetCaption("Heat");
            box.SetMessage("Heating");
            box.SetMsgBgColor(XKCDColors.PastelRed);
            box.SetMsgTextColor(XKCDColors.Vomit);
            box.SetProgressBarBgColor(XKCDColors.PaleRed);
            box.SetProgressBarColor(XKCDColors.BrightRed);
            box.SetValue(this.part.temperature, 0f, this.part.maxTemp);

            heatingBox = box;
        }

        public void HeatingBoxUpdate(float percent)
        {
            this.heatingBox.SetValue(percent);
        }
        public override void OnFixedUpdate()
        {
            //Debug.Log("AdvSRB: seg set heat bar");

            float percent = this.part.temperature/this.part.maxTemp;

            //Debug.Log("AdvSRB: " + percent);

            this.heatingBox.SetValue(percent);
        }


        public bool isFuelRemaining(string resourceName)
        {
            foreach(PartResource pr in this.part.Resources.list)
            {
                if (pr.info.name == resourceName)
                    if (pr.amount > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
            return false;
        }

        public float calcStartFuelLoadMass(string resourceName)
        {
            float fM = 0;

            foreach (PartResource pr in this.part.Resources.list)
            {
                if (pr.info.name == resourceName)
                    if (pr.amount > 0)
                    {
                        fM = AdvSRBUtils.GetResourceDensity(resourceName) * (float)pr.amount;
                    }
            }
            return fM;
        }
    }
}
