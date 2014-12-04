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
    abstract public class KSFAutoSRB
    {
        public virtual string shortName()
        {
            return "undefined";
        }
        public virtual string description()
        {
           return "no description";
        }

        public abstract AnimationCurve computeCurve(FloatCurve Isp, float propellantMass);

        public virtual bool useWithStack()
        {
            return false;
        }

        public virtual bool useWithSegment()
        {
            return true;
        }

        public abstract void drawGUI(Rect baseRect);
    }

    //=======================================================================================================================================================

    public class autoSeg_ThrustForDuration : KSFAutoSRB
    {
        float UIduration = 60;

        public override string shortName()
        {
            return "Thrust for Duration";
        }
        public override string description()
        {
            return "Specify a target burn duration and this mode will set this booster to burn for constant thrust for that duration";
        }

        public override AnimationCurve computeCurve(FloatCurve Isp, float propellantMass)
        {
            AnimationCurve ac = new AnimationCurve();
            float fuelFlow;
            fuelFlow = 1 / UIduration;

            Keyframe k = new Keyframe();
            k.time = 0;
            k.value = fuelFlow;

            ac.AddKey(k);

            return ac;
        }

        public override void drawGUI(Rect baseRect)
        {
            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Burn Time (s)");
            UIduration = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UIduration.ToString()));
        }

        public override bool useWithSegment()
        {
            return true;
        }
        public override bool useWithStack()
        {
            return true;
        }
    }

    //=======================================================================================================================================================

    public class autoSeg_ThrustForThrust : KSFAutoSRB
    {
        float UIthrust = 200;
        float UIAtmDen = 0.5f;

        public override string shortName()
        {
            return "Set Fixed Thrust";
        }
        public override string description()
        {
            return "Set a target thrust at a certain atmospheric density";
        }

        public override AnimationCurve computeCurve(FloatCurve Isp, float propellantMass)
        {
            AnimationCurve ac = new AnimationCurve();

            float fuelFlow;
            fuelFlow = UIthrust / (9.80661f * Isp.Evaluate (UIAtmDen));

            fuelFlow = fuelFlow / propellantMass;

            Keyframe k = new Keyframe();
            k.time = 0;
            k.value = fuelFlow;

            ac.AddKey(k);

            return ac;
        }

        public override void drawGUI(Rect baseRect)
        {
            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Desired Thrust (kN)");
            UIthrust = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UIthrust.ToString()));

            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 90, 240, 20), "Enter Atmospheric Density (0 -> 1)");
            UIAtmDen = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 90, 50, 20), UIAtmDen.ToString()));
        }

        public override bool useWithSegment()
        {
            return true;
        }
        public override bool useWithStack()
        {
            return true;
        }

    }



    //=======================================================================================================================================================
    /* depreciated
    public class autoSeg_ThrustAtTime : KSFAutoSRB
    {
        float UIthrust = 200;
        float UIstart = 60;

        public override string shortName()
        {
            return "Thrust At Time";
        }
        public override string description()
        {
            return "Set a target thrust to begin after a certain period of time, useful for seperation segments";
        }

        public override AnimationCurve computeCurve(FloatCurve Isp, float propellantMass)
        {
            AnimationCurve ac = new AnimationCurve();

            float fuelFlow;
            fuelFlow = UIthrust / (9.81f * Isp.Evaluate(0.1f));

            Keyframe k1 = new Keyframe();
            k1.time = UIstart - 0.01f;
            k1.value = 0;
            k1.inTangent = 0;
            k1.outTangent = fuelFlow * 100;
            ac.AddKey(k1);

            Keyframe k2 = new Keyframe();
            k2.time = UIstart;
            k2.value = fuelFlow;
            k2.inTangent = fuelFlow * 100; //this will be 1/100th of a second
            k2.outTangent = 0;
            ac.AddKey(k2);

            return ac;
        }

        public override void drawGUI(Rect baseRect)
        {
            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Desired Thrust (kN)");
            UIthrust = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UIthrust.ToString()));

            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 90, 240, 20), "Enter Time to Start Burn (s)");
            UIstart = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 90, 50, 20), UIstart.ToString()));
        }

        public override bool useWithSegment()
        {
            return true;
        }
        public override bool useWithStack()
        {
            return true;
        }
    }
     */


    //=======================================================================================================================================================

    public class autoSeg_ExtraThrustForDuration : KSFAutoSRB
    {
        const float g = 9.80665f;

        float UIthrust = 200;
        float UIAtmDen = 0.5f;

        public override string shortName()
        {
            return "Set Fixed (Rel) Thrust";
        }
        public override string description()
        {
            return "Set a target thrust at a certain atmospheric density";
        }

        public override AnimationCurve computeCurve(FloatCurve Isp, float propellantMass)
        {
            AnimationCurve ac = new AnimationCurve();

            float[] fuelFlow = new float[11];

            float deltaThrust;
            float[] deltaDuration = new float[fuelFlow.Length - 1];
            float[] fuelSlope = new float[fuelFlow.Length - 1];
                        Keyframe[] k = new Keyframe[fuelFlow.Length];

            deltaThrust = propellantMass * g;

            int i = 0;
            do
            {
                float ffA = (UIthrust - (((float)i / deltaDuration.Length) * deltaThrust));

                float ffB = (g * Isp.Evaluate(1 - ((float)i / fuelFlow.Length)));

                fuelFlow[i] = (ffA/ffB)/propellantMass;

                i++;
            } while (i< fuelFlow.Length);

            i = 0;

            do
            {
                deltaThrust = fuelFlow[i] - fuelFlow[i+1];
                deltaDuration[i] = 0.1f / (0.5f * deltaThrust + fuelFlow[i]);
                fuelSlope[i] = -deltaThrust / deltaDuration[i];

                i++;
            } while (i < fuelFlow.Length -1);

            i = 0;

            float cumeDuration = 0;

            do
            {
                //Debug.Log("do C: " + i);
                switch (i)
                {
                    case 0:
                        k[i].time = 0;
                        k[i].value = fuelFlow[i];
                        k[i].outTangent = fuelSlope[i];
                        break;

                    default:
                        cumeDuration += deltaDuration[i - 1];

                        k[i].time = cumeDuration;
                        k[i].value = fuelFlow[i];
                        k[i].inTangent =k[i-1].outTangent;

                        if (i < fuelSlope.Length)
                        {
                            Debug.Log("i = " + i + "  fuelSlope[i] " + fuelSlope[i]);
                            k[i].outTangent = fuelSlope[i];
                        }
                        else
                        {
                            k[i].outTangent = 0;
                        }

                        break;
                }
   

                i++;
            } while (i < k.Length);

            foreach (Keyframe K in k)
            {

                ac.AddKey(K);
            }

            return ac;
        }

        public override void drawGUI(Rect baseRect)
        {
            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Desired Thrust (kN)");
            UIthrust = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UIthrust.ToString()));

            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 90, 240, 20), "Enter Atmospheric Density (0 -> 1)");
            UIAtmDen = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 90, 50, 20), UIAtmDen.ToString()));
        }

        public override bool useWithSegment()
        {
            return true;
        }
        public override bool useWithStack()
        {
            return true;
        }

    }

    //=======================================================================================================================================================

    //public class autoSeg_ExtraThrustForExtraThrustAtGee : KSFAutoSRB
    //{
    //    float UImass = 4;
    //    float UIgee = 2;



    //    public override string shortName()
    //    {
    //        return "Lift Mass At G";
    //    }
    //    public override string description()
    //    {
    //        return "Specify a target payload mass to lift at a certain constant(ish) acceleration";
    //    }

    //    public override AnimationCurve computeCurve(FloatCurve Isp, float propellantMass)
    //    {
    //        AnimationCurve ac = new AnimationCurve();
    //        const float g = 9.80665f;
    //        float duration;

    //        float deltaForce = ((segment.GetResourceMass() + segment.mass) * g * UIgee) - (segment.mass * g * UIgee);

    //        Keyframe k1 = new Keyframe();
    //        k1.time = 0;
    //        k1.value = (((UImass + segment.GetResourceMass() + segment.mass) * g * UIgee) / Isp.Evaluate(0)) / g;

    //        Keyframe k2 = new Keyframe();

    //        k2.value = (((UImass + segment.mass) * g * UIgee) / Isp.Evaluate(0)) / g;

    //        duration = segment.GetResourceMass() / (k2.value + 0.5f * (k1.value - k2.value));

    //        k2.time = duration;

    //        k1.outTangent = (k2.value - k1.value) / (k2.time - k1.time);
    //        k2.inTangent = (k2.value - k1.value) / (k2.time - k1.time);

    //        k1.inTangent = 0;
    //        k2.outTangent = 0;

    //        ac.AddKey(k1);
    //        ac.AddKey(k2);

    //        return ac;
    //    }

    //    public override void drawGUI(Rect baseRect)
    //    {
    //        GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Payload Mass(t)");
    //        UImass = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UImass.ToString()));

    //        GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 90, 240, 20), "Enter Acceleration (g)");
    //        UIgee = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 90, 50, 20), UIgee.ToString()));

    //    }

    //    public override bool useWithSegment()
    //    {
    //        return true;
    //    }
    //    public override bool useWithStack()
    //    {
    //        return false;
    //    }
    //}


    //=======================================================================================================================================================

    //public class autoStack_ExtraThrustForExtraThrustAtGee : KSFAutoSRB
    //{
    //    float UImass = 4;
    //    float UIgee = 2;



    //    public override string shortName()
    //    {
    //        return "Stack Mass At G";
    //    }
    //    public override string description()
    //    {
    //        return "Specify a target payload mass to lift at a certain constant(ish) acceleration";
    //    }

    //    public override AnimationCurve computeCurve(FloatCurve Isp, float propellantMass)
    //    {
    //        AnimationCurve ac = new AnimationCurve();
    //        const float g = 9.80665f;
    //        float duration;

    //        //float deltaForce = ((segment.GetResourceMass() + segment.mass) * g * UIgee) - (segment.mass * g * UIgee);

    //        Keyframe k1 = new Keyframe();
    //        k1.time = 0;
    //        k1.value = (((UImass + propellantMass + segment.mass) * g * UIgee) / Isp.Evaluate(0)) / g;

    //        Keyframe k2 = new Keyframe();

    //        k2.value = (((UImass + segment.mass) * g * UIgee) / Isp.Evaluate(0)) / g;

    //        duration = propellantMass / (k2.value + 0.5f * (k1.value - k2.value));

    //        k2.time = duration;

    //        k1.outTangent = (k2.value - k1.value) / (k2.time - k1.time);
    //        k2.inTangent = (k2.value - k1.value) / (k2.time - k1.time);

    //        k1.inTangent = 0;
    //        k2.outTangent = 0;

    //        ac.AddKey(k1);
    //        ac.AddKey(k2);

    //        return ac;
    //    }

    //    public override void drawGUI(Rect baseRect)
    //    {
    //        GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Payload Mass(t)");
    //        UImass = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UImass.ToString()));

    //        GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 90, 240, 20), "Enter Acceleration (g)");
    //        UIgee = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 90, 50, 20), UIgee.ToString()));

    //    }

    //    public override bool useWithSegment()
    //    {
    //        return false;
    //    }
    //    public override bool useWithStack()
    //    {
    //        return true;
    //    }
    }

