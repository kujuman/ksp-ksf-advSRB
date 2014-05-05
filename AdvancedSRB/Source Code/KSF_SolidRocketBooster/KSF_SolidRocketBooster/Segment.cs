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
    public class KSF_SolidBoosterSegment : PartModule
    {
        [KSPField(isPersistant = true)]
        public string BurnProfile = "";

        public AnimationCurve MassFlow;

        [KSPField]
        public int defaultBurnTime = 60;

        [KSPField]
        public float thrustVariation = .0f;

        [KSPField(guiName = "ThrustErr", guiActive = true, guiFormat = "F5")]
        public float thrustError = 0;

        [KSPField]
        public string topNode = "top";

        [KSPField]
        public bool endOfStack = false;

        [KSPField]
        public string GUIshortName;

        public override void OnAwake()
        {
            if (MassFlow == null)
            {
                MassFlow = AnimationCurveFromString(BurnProfile);
            }
        }

        public override string GetInfo()
        {
            string displayInfo = "";

            displayInfo += "Default Burn Time: " + defaultBurnTime + "s";

            displayInfo += Environment.NewLine;

            displayInfo += "End of Stack: " + endOfStack.ToString();
            displayInfo += Environment.NewLine;

            displayInfo += "Short Name: " + GUIshortName;
            displayInfo += Environment.NewLine;

            displayInfo += "Thrust Variation: " + (thrustVariation*100f).ToString("F1") + "%";

            return displayInfo;
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

        public float CalcMassFlow(float time)
        {
            float f;
            f = MassFlow.Evaluate(time);

            if (thrustVariation > .0001f)
                f = f * (1 + CalcMassFlux());

            if (f > 0)
                return f;
            else
                return 0;
        }

        public float CalcMassFlux()
        {
            UnityEngine.Random ur = new UnityEngine.Random();

            thrustError = (float)thrustError * .7f + UnityEngine.Random.Range(-1 * thrustVariation, thrustVariation) * .3f;
            return thrustError;
        }

        public AnimationCurve AnimationCurveFromString(string s)
        {
            AnimationCurve ac = new AnimationCurve();
            if (s == "")
            {
                Debug.LogWarning(this.part.name + "No Burn Profile found! Defaulting to constant burn rate.");
                ac.AddKey(0, (float)(this.part.GetResourceMass() / defaultBurnTime));
                return ac;
            }
            print(s);
            s.Trim();
            Char[] delimiters = new Char[] {',', ';'};
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

        public string AnimationCurveToString(AnimationCurve ac)
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

        public override void OnLoad(ConfigNode node)
        {
            MassFlow = AnimationCurveFromString(BurnProfile);
            base.OnLoad(node);
        }
    }
}
