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

        public abstract AnimationCurve computeCurve(FloatCurve Isp, Part segment);

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

        public override AnimationCurve computeCurve(FloatCurve Isp, Part segment)
        {
            AnimationCurve ac = new AnimationCurve();
            float fuelFlow;
            fuelFlow = segment.GetResourceMass() / UIduration;

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

        public override AnimationCurve computeCurve(FloatCurve Isp, Part segment)
        {
            AnimationCurve ac = new AnimationCurve();

            float fuelFlow;
            fuelFlow = UIthrust / (9.81f * Isp.Evaluate (UIAtmDen));

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
    }

    //=======================================================================================================================================================

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

        public override AnimationCurve computeCurve(FloatCurve Isp, Part segment)
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
    }

    //=======================================================================================================================================================

    public class autoSeg_ExtraThrustForDuration : KSFAutoSRB
    {
        float UIduration = 60;

        public override string shortName()
        {
            return "Extra Thrust for Duration";
        }
        public override string description()
        {
            return "Specify a target burn duration and this mode will set this booster to burn for constant excess thrust for that duration";
        }

        public override AnimationCurve computeCurve(FloatCurve Isp, Part segment)
        {
            AnimationCurve ac = new AnimationCurve();
            float avgFuelFlow = segment.GetResourceMass() / UIduration;
            const float g = 9.80665f;

            float avgThrust = ((Isp.Evaluate(0) + Isp.Evaluate(0)) * g * avgFuelFlow) / 2;
            float deltaForce = ((segment.GetResourceMass() + segment.mass) * g) - (segment.mass * g);

            Keyframe k1 = new Keyframe();
            k1.time = 0;
            k1.value = ((avgThrust + .5f * deltaForce) / Isp.Evaluate(0)) / g;

            Keyframe k2 = new Keyframe();
            k2.time = UIduration;
            k2.value = ((avgThrust - .5f * deltaForce) / Isp.Evaluate(0)) / g; ;

            k1.outTangent = (k2.value - k1.value) / (k2.time - k1.time);
            k2.inTangent = (k2.value - k1.value) / (k2.time - k1.time);

            k1.inTangent = 0;
            k2.outTangent = 0;

            ac.AddKey(k1);
            ac.AddKey(k2);

            return ac;
        }

        public override void drawGUI(Rect baseRect)
        {
            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Burn Time (s)");
            UIduration = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UIduration.ToString()));
        }
    }

    //=======================================================================================================================================================

    public class autoSeg_ExtraThrustForExtraThrustAtGee : KSFAutoSRB
    {
        float UImass = 4;
        float UIgee = 2;



        public override string shortName()
        {
            return "Lift Mass At G";
        }
        public override string description()
        {
            return "Specify a target payload mass to lift at a certain constant(ish) acceleration";
        }

        public override AnimationCurve computeCurve(FloatCurve Isp, Part segment)
        {
            AnimationCurve ac = new AnimationCurve();
            const float g = 9.80665f;
            float duration;

            //float deltaForce = ((segment.GetResourceMass() + segment.mass) * g * UIgee) - (segment.mass * g * UIgee);

            Keyframe k1 = new Keyframe();
            k1.time = 0;
            k1.value = (((UImass + segment.GetResourceMass() + segment.mass) * g * UIgee) / Isp.Evaluate(0)) / g;

            Keyframe k2 = new Keyframe();

            k2.value = (((UImass + segment.mass) * g * UIgee) / Isp.Evaluate(0)) / g;

            duration = segment.GetResourceMass() / (k2.value + 0.5f * (k1.value - k2.value));

            k2.time = duration;

            k1.outTangent = (k2.value - k1.value) / (k2.time - k1.time);
            k2.inTangent = (k2.value - k1.value) / (k2.time - k1.time);

            k1.inTangent = 0;
            k2.outTangent = 0;

            ac.AddKey(k1);
            ac.AddKey(k2);

            return ac;
        }

        public override void drawGUI(Rect baseRect)
        {
            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 60, 240, 20), "Enter Payload Mass(t)");
            UImass = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 60, 50, 20), UImass.ToString()));

            GUI.Label(new Rect(baseRect.xMin + 350, baseRect.yMin + 90, 240, 20), "Enter Acceleration (g)");
            UIgee = Convert.ToSingle(GUI.TextField(new Rect(baseRect.xMin + 560, baseRect.yMin + 90, 50, 20), UIgee.ToString()));

        }
    }

}
