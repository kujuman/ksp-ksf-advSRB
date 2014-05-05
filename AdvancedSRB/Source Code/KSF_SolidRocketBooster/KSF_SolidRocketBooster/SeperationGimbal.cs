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

