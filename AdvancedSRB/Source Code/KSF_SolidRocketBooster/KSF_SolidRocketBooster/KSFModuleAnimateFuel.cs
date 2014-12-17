using System;
using KSP;
using UnityEngine;
using System.Linq;

namespace KSF_SolidRocketBooster
{
    public delegate void GameObjectVisitor(GameObject go, int indent);

    public static class DebugExtensions
    {
        private static void internal_PrintComponents(GameObject go, int indent)
        {
            Debug.Log((indent > 0 ? new string('-', indent) + ">" : "") + " " + go.name + " has components:");

            var components = go.GetComponents<Component>();
            foreach (var c in components)
                Debug.Log(new string('.', indent + 3) + "c" + ": " + c.GetType().FullName);
        }

        public static void PrintComponents(this UnityEngine.GameObject go)
        {
            go.TraverseHierarchy(internal_PrintComponents);
        }

        public static void TraverseHierarchy(this UnityEngine.GameObject go, GameObjectVisitor visitor, int indent = 0)
        {
            visitor(go, indent);

            for (int i = 0; i < go.transform.childCount; ++i)
                go.transform.GetChild(i).gameObject.TraverseHierarchy(visitor, indent + 3);
        }
    }




    [KSPModule("KSF Fuel Animate")]
    public class KSFModuleAnimateResource : PartModule
    {
        [KSPField]
        public string animName = "";

        [KSPField]
        public int startFrame = 0;

        [KSPField]
        public int endFrame = 100;

        [KSPField (isPersistant = true)]
        public bool useAnimation = true;




        public AnimationState animState;
        public float clipTime;

        public Animation clip;

        public string resourceName = "SolidFuel";
        public string skinnedMeshName = "FuelPlug";
        public bool debug = true;

        SkinnedMeshRenderer skinnedMeshRenderer;
        Mesh skinnedMesh;

        public string blendShapeName = "Empty";
        int skinnedMeshIdx = 0;

        public override void OnAwake()
        {
            if (debug) Debug.Log("KSF FuelAnimate: ");

            //skinnedMeshRenderer = part.GetComponent<SkinnedMeshRenderer>();
            //if(skinnedMeshRenderer == null)
            //if (debug) Debug.Log("KSF FuelAnimate: skinnedMeshRenderer is null");

            DebugExtensions.PrintComponents(part.gameObject);
            

            if (debug) Debug.Log("KSF FuelAnimate: 1");

            skinnedMeshRenderer = part.FindModelComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null)
                if (debug) Debug.Log("KSF FuelAnimate: skinnedMeshRenderer is null");

            skinnedMeshRenderer = part.GetComponentInChildren<SkinnedMeshRenderer>();

            if (debug) Debug.Log("KSF FuelAnimate: 2");


            DebugExtensions.PrintComponents(skinnedMeshRenderer.gameObject);



            //skinnedMeshRenderer = part.FindModelComponent<SkinnedMeshRenderer>(skinnedMeshName);
            //if (skinnedMeshRenderer == null)
            //    if (debug) Debug.Log("KSF FuelAnimate: skinnedMeshRenderer is null");


            if (debug) Debug.Log("KSF FuelAnimate: 3");

            skinnedMesh = skinnedMeshRenderer.sharedMesh;

            getBlendShapeNames(skinnedMeshRenderer.gameObject);


            //skinnedMesh = skinnedMeshRenderer.sharedMesh;

            if(skinnedMesh == null)
                if (debug) Debug.Log("KSF FuelAnimate: skinnedMesh is null");

            if (debug) Debug.Log("KSF FuelAnimate: 2");
            

            skinnedMeshIdx = skinnedMesh.GetBlendShapeIndex(blendShapeName);

            if (debug) Debug.Log("KSF FuelAnimate: Index = " + skinnedMeshIdx);

            if (debug) Debug.Log("KSF FuelAnimate: Count = " + skinnedMesh.blendShapeCount);

            if (debug) Debug.Log("KSF FuelAnimate: 3");
            for (int i = 0; i < skinnedMesh.blendShapeCount; i++)
            {
                if (debug) Debug.Log("KSF FuelAnimate: Found " + skinnedMesh.GetBlendShapeName(i));

                //if (skinnedMesh.GetBlendShapeName(i) == blendShapeName)
                //{
                //    if (debug) Debug.Log("KSF FuelAnimate: Found match " + blendShapeName);
                //}
            }




            ////find the animation
            //if (debug) Debug.Log("KSF FuelAnimate: Looking for Animation..." + animName);
            //clip = part.GetComponentInChildren<Animation>();

            //if (clip == null)
            //{
            //    if (debug) Debug.Log("KSF FuelAnimate: Animation is null");
            //    //useAnimation = false;
            //}

            //Animation[] a = part.FindModelAnimators();

            ////////Animation[] anims2 = part.FindModelComponents<Animation>();

            ////////foreach (Animation a in anims2)
            ////////{
            ////////    if (a.name == animName)
            ////////        clip = a;

            ////////    if (debug) Debug.Log("KSF FuelAnimate: Clip name " + clip.name);
            ////////}

            //clip = part.FindModelAnimators(animName).FirstOrDefault();


            //animState.name = clip.name;
            

            //if (debug) Debug.Log("KSF FuelAnimate: Found "  " animations");

            //Animation[] anims2 = part.FindModelComponents<Animation>();

            //if (debug) Debug.Log("KSF FuelAnimate: Found " + anims2.Length + " animations");

            ////Animator[] anims3 = part.FindModelComponents<Animator>();

            ////if (debug) Debug.Log("KSF FuelAnimate: Found " + anims3.Length + " animators");

            //Animation[] anims4 = part.FindModelComponents<Animation>();

            //if (debug) Debug.Log("KSF FuelAnimate: Found " + anims4.Length + " animations");


            //animState = new AnimationState[animState.Length];


            //for(int i = 0; i<anims.Length; i++)
            //{
            //    Animation anim = anims[i];

            //    if (debug) Debug.Log("KSF FuelAnimate: Animation " + i + ": " + anim.name);

            //    ;
            //}

            //}
        }

         public string [] getBlendShapeNames (GameObject obj)
 {
     if (debug) Debug.Log("KSF FuelAnimate: In getBlendShapeNames()");

     SkinnedMeshRenderer head = obj.GetComponent<SkinnedMeshRenderer>();
     Mesh m = head.sharedMesh;
     string[] arr;
     arr = new string [m.blendShapeCount];
     for (int i= 0; i < m.blendShapeCount; i++)
     {
       string s = m.GetBlendShapeName(i);
    Debug.Log("Blend Shape: " + i + " " + s); // Blend Shape: 4 FightingLlamaStance
       arr[i] = s;
     }
     return arr;
 }


        //public override void OnStart(StartState state)
        //{
        //    base.OnStart(state);


        //    if (debug) Debug.Log("KSF FuelAnimate: 2");


        //    skinnedMeshIdx = skinnedMesh.GetBlendShapeIndex(blendShapeName);

        //    if (debug) Debug.Log("KSF FuelAnimate: Index = " + skinnedMeshIdx);

        //    if (debug) Debug.Log("KSF FuelAnimate: Count = " + skinnedMesh.blendShapeCount);

        //    if (debug) Debug.Log("KSF FuelAnimate: 3");
        //    for (int i = 0; i < skinnedMesh.blendShapeCount; i++)
        //    {
        //        if (debug) Debug.Log("KSF FuelAnimate: Found " + skinnedMesh.GetBlendShapeName(i));

        //        //if (skinnedMesh.GetBlendShapeName(i) == blendShapeName)
        //        //{
        //        //    if (debug) Debug.Log("KSF FuelAnimate: Found match " + blendShapeName);
        //        //}
        //    }
        //}

        public override void OnUpdate()
        {




            if (useAnimation == false)
                return;

            float resourcePercent = 1;
 
            foreach (PartResource pr in this.part.Resources.list)
            {
                if (pr.info.name == resourceName)
                    {
                        //if (debug) Debug.Log("KSF FuelAnimate: Found resource " + resourceName);
                      resourcePercent = (float)(1 - (pr.amount/pr.maxAmount));

                      //if (debug) Debug.Log("KSF FuelAnimate: resourcePercent = " + resourcePercent.ToString());
                
                }
            }

           // if (debug) Debug.Log("KSF FuelAnimate: Apply Animation");
            //clip[animName].normalizedTime = resourcePercent;
            ////animState.normalizedTime = resourcePercent;

            //skinnedMeshRenderer.SetBlendShapeWeight(0, resourcePercent);

            skinnedMeshRenderer.SetBlendShapeWeight(skinnedMeshIdx, resourcePercent);

           
            

        }


    }
}