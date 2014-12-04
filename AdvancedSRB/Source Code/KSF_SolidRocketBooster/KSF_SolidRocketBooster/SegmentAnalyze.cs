///*
// * Kerbal Science Foundation Advanced Solid Rocket Booster v0.6.1 for Kerbal Space Program
// * Released May 4, 2014 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
// * For attribution, please attribute "kujuman"
// */


//using System;
//using KSP;
//using UnityEngine;

//namespace KSF_SolidRocketBooster
//{
//    public class KSF_SolidBooster_Analyze : PartModule
//    {
//        private Single i = 0;
//        private string sOutput = "";

//        [KSPField]
//        private Single stepSize = .1f;

//        [KSPField]
//        private Single Duration = 200;

//        [KSPEvent(guiActive = true, guiName = "Analyze burn", active = true)]
//        private void AnalyzeBurn()
//        {
//            AdvSRBSegment srb = this.part.GetComponent<AdvSRBSegment>();
//            {
//                i = 0;
//                sOutput += this.part.ToString() + "," + this.part.GetResourceMass() + Environment.NewLine;
//                do
//                {
//                    sOutput += i + "," + Mathf.Max(0, srb.CalcMassFlow(i)) + Environment.NewLine;
//                    i += stepSize;
//                } while (i < Duration);
//                WriteFile();
//                return;
//            }
//        }

//        private void WriteFile()
//        {
//            KSP.IO.File.WriteAllText<KSF_SolidBooster_Analyze>(sOutput, this.part.ToString() + ".txt");
//        }
//    }
//}
