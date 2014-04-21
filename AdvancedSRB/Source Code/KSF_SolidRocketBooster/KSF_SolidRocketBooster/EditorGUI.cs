﻿
/*
 * Kerbal Science Foundation Advanced Solid Rocket Booster v0.6 for Kerbal Space Program
 * Released September 13, 2013 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 * For attribution, please attribute "Kujuman from the official KSP forums"
 * Portions of this work were based on example code posted at the KSP wiki. If you have something to contribute there, please do!
 */

using KSP;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace KSF_SolidRocketBooster
{
    public class EditorGUI : MonoBehaviour
    {
        private int DebugDetail = -100;

        protected Rect windowPos;
        private Rect GUImainRect = new Rect(10, 60, 660, 530);

        private int GUImodeInt = 1;
        private string[] GUImodeStrings = { "Vessel", "Stack", "Segments" };

        private Vector2 segListVector = new Vector2(0, 0);
        private int segListLength = 0;
        private Part segCurrentGUI;
        private Part segPriorGUI;


        private Vector2 nozListVector = new Vector2(0, 0);
        private int nozListLength = 0;
        private Part nozCurrentGUI;
        private Part nozPriorGUI;

        private Vector2 nodeListVector = new Vector2(0, 0);
        private int nodeNumber = 0;
        private int nodePriorNumber = 0;

        private Vector2 autoListVector = new Vector2(0, 0);
        public System.Collections.Generic.List<KSFAutoSRB> autoTypeList = new System.Collections.Generic.List<KSFAutoSRB>();
        private int autoTypeCurrent;

        private string tbTime;
        private string tbValue;
        private string tbInTan;
        private string tbOutTan;

        private bool isAutoNode = false;

        private string lbMsgBox;

        bool refreshNodeInfo = true;
        bool refreshSegGraph = true;

        private Texture2D segGraph;

        private string klipboard;



        string simDuration;



        List<Part> lHighlightNozzles = new List<Part>();

        List<Part> lNozzles = new List<Part>();
        List<Part> lsNozzles = new List<Part>();
        List<Part> lSegments = new List<Part>();
        List<Part> lShip = new List<Part>();

        int iNozzles = 0;
        int isNozzles = 0;
        int iSegments = 0;

        public void FindAdvSRBNozzles()
        {
            int pc = 1;

            if (DebugDetail == -100)
            Debug.Log("AdvSRB: in FindAdvSRBNozzles");

            lNozzles.Clear();

            if (EditorLogic.startPod)
                RecursePartList(lShip, EditorLogic.startPod); //taken from FAR by Ferram

            if (DebugDetail == -100)
                Debug.Log("AdvSRB: found " + lShip.Count + " parts in this vessel");

            foreach (Part p in lShip)
            {
                if (DebugDetail == -100)
                    Debug.Log("AdvSRB: looking in part #" + pc);

                if (p.Modules.Contains("KSF_SBNozzle"))
                {
                    if (DebugDetail == -100)
                        Debug.Log("AdvSRB: this part contains KSF_SBNozzle");

                    if (lNozzles.Contains(p) != true)
                        lNozzles.Add(p);
                }
                pc++;
            }

            if (DebugDetail == -100)
                Debug.Log("AdvSRB: lNozzles contains " + lNozzles.Count);

            lsNozzles.Clear();

            foreach  (Part p in lNozzles)
            {
                lsNozzles.Add(p);
            }


            lsNozzles = ToSymmetryGroups(lsNozzles);

            if (DebugDetail == -100)
                Debug.Log("AdvSRB: lNozzles contains " + lNozzles.Count);

            if (DebugDetail == -100)
                Debug.Log("AdvSRB: lsNozzles contains " + lsNozzles.Count);
        }

        private void RecursePartList(List<Part> list, Part part) //taken from FAR by Ferram
        {
                        if (DebugDetail == -100)
            Debug.Log("AdvSRB: in RecursePartList");


            list.Add(part);
            foreach (Part p in part.children)
                RecursePartList(list, p);
        }

        List<Part> ToSymmetryGroups(List<Part> inputList)
        {
                        if (DebugDetail == -100)
            Debug.Log("AdvSRB: in ToSymmetryGroups");

            List<Part> outputList = new List<Part>();


            bool restart = false;

            outputList = inputList;

            Restart:

            restart = false;

            foreach(Part p in outputList)
            {
                foreach (Part sp in p.symmetryCounterparts)
                {
                    if (outputList.Contains(sp))
                    {
                        outputList.Remove(sp);
                        restart = true;
                    }
                }

                if (restart)
                    goto Restart;
            }

            return outputList;
        }




        private void WindowGUI(int windowID)
        {
            if (DebugDetail > 0)
                Debug.Log("AdvSRB: in WindowGUI");


            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);

            if (simDuration == null)
                simDuration = "135";

            //top layer of the window GUI
            GUImodeInt = GUI.Toolbar(new Rect(10, 25, 660, 30), GUImodeInt, GUImodeStrings);
            GUI.Box(GUImainRect, GUImodeStrings[GUImodeInt]);
            
            if(GUI.Button(new Rect(10,5,200,20),"Refresh Nozzle List"))
            {
                FindAdvSRBNozzles();
            }


            //select appropriate mode
            if (GUImodeInt == 1)
            {
                stackGUI();
            }

            if (GUImodeInt == 2)
                segmentGUI(lNozzles[iNozzles].GetComponent<KSF_SBNozzle>());



            //DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
            //clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
            //dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
            //it may "cover up" your controls and make them stop responding to the mouse.
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void stackGUI()
        {
            if (DebugDetail > 0)
                Debug.Log("AdvSRB: in stackGUI");

            //this is the "Stack" mode of the GUI, as used in the first iteration of the GUI

            simDuration = GUI.TextField(new Rect(170 + GUImainRect.xMin, 10 + GUImainRect.yMin, 40, 20), simDuration);
            
            GUI.Label(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 10, 150, 20), "Simulation Run Time (s)");



            if (DebugDetail > 1)
                Debug.Log("AdvSRB: 1");

         //FindAdvSRBNozzles();

         //set length of internal text box to be 50 pix per segment
         nozListLength = lNozzles.Count * 50;

         ////automatically select first segment as current GUI
         //if (lNozzles.Count > 0 && nozCurrentGUI == null)
         //{
         //    nozCurrentGUI = nozzle.FuelSourcesList[0];
         //}

         if (DebugDetail > 1)
             Debug.Log("AdvSRB: 2");
            //**********************
            nozListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 10, 170, 450), nozListVector, new Rect(0, 0, 100, nozListLength));

            //GUI.Label(new Rect(15, 0, 100, 15), "Nozzle End");
            for (int i = 0; i < lNozzles.Count; i++)
            {
                if(DebugDetail > 2)
                    Debug.Log("Populating Nozzle list box");

                //SRB = nozzle.FuelSourcesList[i].GetComponent<KSF_SolidBoosterSegment>();
                //if (SRB.GUIshortName != "")
                //    segGUIname = SRB.GUIshortName;
                //else
                //    segGUIname = "Unknown";

                if (iNozzles == i)//next drawn button is the current stack
                {
                    if (GUI.Button(new Rect(5, i * 50 + 5, 145, 40), "* " + lNozzles[i].name)) //argument used to be 5, i * 50 + 20, 95, 40
                    {
                        iNozzles = i;
                        segCurrentGUI = null;


                        lHighlightNozzles.Add(lNozzles[i]);
                        StackColorHandler(lShip, lHighlightNozzles);
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(5, i * 50 + 5, 145, 40), lNozzles[i].name)) //argument used to be 5, i * 50 + 20, 95, 40
                    {
                        iNozzles = i;
                        segCurrentGUI = null;

                        lHighlightNozzles.Add(lNozzles[i]);
                        StackColorHandler(lShip, lHighlightNozzles);
                    }
                }
            }
            //GUI.Label(new Rect(15, segListLength + 15, 100, 10), "Top o' Stack");
            GUI.EndScrollView();

            //************************

            

            //if (GUI.Button(new Rect(GUImainRect.xMin + 440, GUImainRect.yMin + 10, 200, 20), "Generate Thrust Graph"))
            //{
            ////    stackThrustPredictPic(480, 640, Convert.ToInt16(simDuration)); //do not use this line 4/20/14
            //}
            //GUI.Box(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 40, 640, 480), "placeholder");//use this 4/20/14


        }

        private void StackColorHandler(List<Part> allParts, List<Part> colorList)
        {
            foreach (Part p in allParts)
            {
                //if (colorList.Find(p => p.))
                //    ;

                

                p.SetHighlight(false);
            }

            foreach(Part p in colorList)
            {
                p.SetHighlight(true);
            }
            colorList.Clear();
        }
        

        private void segmentGUI(KSF_SBNozzle nozzle)
        {
            if (DebugDetail > 0)
                Debug.Log("AdvSRB: in segmentGUI");

            //this is the "Segment" mode of the GUI, which allows one to customize burn times for each segment
            string segGUIname = "";
            KSF_SolidBoosterSegment SRB;

            //must populate the buttons with the most current fuel sources
            nozzle.FuelStackSearcher(nozzle.FuelSourcesList);

            //set length of internal text box to be 50 pix per segment
            segListLength = nozzle.FuelSourcesList.Count * 50;

            //automatically select first segment as current GUI
            if (nozzle.FuelSourcesList.Count > 0 && segCurrentGUI == null)
            {
                segCurrentGUI = nozzle.FuelSourcesList[0];
                iSegments = 0;
                refreshNodeInfo = true;
            }

            segListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 10, 120, 420), segListVector, new Rect(0, 0, 100, segListLength + 30));

            GUI.Label(new Rect(15, 0, 100, 15), "Nozzle End");
            for (int i = 0; i < nozzle.FuelSourcesList.Count; i++)
            {
                SRB = nozzle.FuelSourcesList[i].GetComponent<KSF_SolidBoosterSegment>();
                if (SRB.GUIshortName != "")
                    segGUIname = SRB.GUIshortName;
                else
                    segGUIname = "Unknown";


                if (i == iSegments)
                {
                    if (GUI.Button(new Rect(5, i * 50 + 20, 95, 40),"* " + segGUIname))
                    {
                        if (nozzle.FuelSourcesList[i] != segCurrentGUI)
                        {
                            segPriorGUI = segCurrentGUI;
                            segCurrentGUI = nozzle.FuelSourcesList[i];
                            refreshSegGraph = true;
                        }
                    }
                }
                else
                {
                    {
                        if (nozzle.FuelSourcesList[i] != segCurrentGUI)
                        {
                            segPriorGUI = segCurrentGUI;
                            segCurrentGUI = nozzle.FuelSourcesList[i];
                            refreshSegGraph = true;
                        }
                    }
                }




            }
            GUI.Label(new Rect(15, segListLength + 15, 100, 10), "Top o' Stack");
            GUI.EndScrollView();


            if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 450, 120, 20), "Apply to Symmetry"))
            {
                KSF_SolidBoosterSegment s;
                KSF_SolidBoosterSegment sb;


                if (segCurrentGUI != null)
                {
                    s = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();

                    

                    foreach(Part p in segCurrentGUI.symmetryCounterparts)
                    {
                        sb = p.GetComponent<KSF_SolidBoosterSegment>();
                        sb.BurnProfile = s.BurnProfile;
                    }
                }
                refreshNodeInfo = true;
            }


            if (isAutoNode)
            {
                if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 470, 120, 20), "Go to Manual Node"))
                {
                    isAutoNode = !isAutoNode;

                    refreshNodeInfo = true;
                }
            }
            else
            {
                if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 470, 120, 20), "Go to Auto Nodes"))
                {
                    isAutoNode = !isAutoNode;

                    refreshNodeInfo = true;
                }
            }

            //copy/paste functionality
            if (segCurrentGUI != null)
            {
                SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();

                if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 500, 55, 20), "Copy"))
                {
                    klipboard = SRB.AnimationCurveToString(SRB.MassFlow);
                }

                if (GUI.Button(new Rect(GUImainRect.xMin + 75, GUImainRect.yMin + 500, 55, 20), "Paste"))
                {
                    SRB.MassFlow = SRB.AnimationCurveFromString(klipboard);
                    SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);

                    refreshNodeInfo = true;
                    refreshSegGraph = true;
                }
            }

            int width = 0;
            if (segCurrentGUI != null)
            {
                SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();
                width = SRB.MassFlow.length * 60;
                if (refreshSegGraph)
                {
                    System.Collections.Generic.List<Part> fSL = new System.Collections.Generic.List<Part>(); //filled once during OnActivate, is the master list
                    fSL.Add(segCurrentGUI);
                    //segGraph = segThrustPredictPic(310, 490, Convert.ToInt16(simDuration), segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>(), segCurrentGUI.GetResourceMass(), segCurrentGUI.mass, 5, 5, fSL);
                    refreshSegGraph = false; ;
                }
                GUI.Box(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 190, 510, 330), segGraph);



                if (isAutoNode)
                {
                    //populate the list box
                    int listLength = 0;

                    listLength = 30 * (autoTypeList.Count + 1);

                    //draw the selection box to select which auto mode to use
                    autoListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 10, 200, 170), autoListVector, new Rect(0, 0, 180, listLength));

                    for (int i = 0; i < autoTypeList.Count; i++)
                    {
                        if (GUI.Button(new Rect(5, i * 30 + 5, 175, 20), autoTypeList[i].shortName()))
                        {
                            autoTypeCurrent = i;
                        }
                    }
                    GUI.EndScrollView();

                    GUI.Label(new Rect(GUImainRect.xMin + 350, GUImainRect.yMin + 10, 300, 40), autoTypeList[autoTypeCurrent].description());

                    autoTypeList[autoTypeCurrent].drawGUI(GUImainRect);


                    //this is the button to compute the segment values
                    if (GUI.Button(new Rect(GUImainRect.xMin + 500, GUImainRect.yMin + 160, 150, 20), "Evaluate"))
                    {
                        SRB.MassFlow = autoTypeList[autoTypeCurrent].computeCurve(nozzle.atmosphereCurve, segCurrentGUI);

                        SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);

                        //refreshSegGraph = true;
                    }

                }
                else
                {
                    nodeListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 10, 510, 40), nodeListVector, new Rect(0, 0, width + 70, 22));
                    if (segCurrentGUI != null)
                    {
                        SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();

                        for (int i = 0; i < SRB.MassFlow.length + 1; i++)
                        {
                            if (i < SRB.MassFlow.length)
                            {
                                if (i == nodeNumber)
                                {
                                    if (GUI.Button(new Rect(3 + i * 60, 4, 54, 18), "*Node " + (i + 1)))
                                    {
                                        nodePriorNumber = nodeNumber;
                                        nodeNumber = i;
                                        refreshNodeInfo = true;
                                    }
                                }
                                else
                                {
                                    if (GUI.Button(new Rect(3 + i * 60, 4, 54, 18), "Node " + (i + 1)))
                                    {
                                        nodePriorNumber = nodeNumber;
                                        nodeNumber = i;
                                        refreshNodeInfo = true;
                                    }
                                }
                            }
                            else
                            {
                                if (GUI.Button(new Rect(3 + i * 60, 4, 64, 18), "Add Node"))
                                {
                                    nodePriorNumber = nodeNumber;
                                    nodeNumber = i;
                                    SRB.MassFlow.AddKey(SRB.MassFlow.keys[i - 1].time + 10, 0);
                                    refreshNodeInfo = true;
                                }
                            }
                        }
                    }
                    GUI.EndScrollView();

                    //set up node editing tools

                    if (segCurrentGUI != null && refreshNodeInfo)
                    {
                        SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();
                        lbMsgBox = "The currently selected segment is " + SRB.GUIshortName + " and the current node is Node " + (nodeNumber + 1);

                        tbTime = SRB.MassFlow.keys[nodeNumber].time.ToString();
                        tbValue = SRB.MassFlow.keys[nodeNumber].value.ToString();
                        tbInTan = SRB.MassFlow.keys[nodeNumber].inTangent.ToString();
                        tbOutTan = SRB.MassFlow.keys[nodeNumber].outTangent.ToString();

                        refreshNodeInfo = false;
                    }
                    else
                    {
                        if (segCurrentGUI == null)
                        {
                            lbMsgBox = "Please select a segment on the left to begin editing";

                            tbTime = "";
                            tbValue = "";
                            tbInTan = "";
                            tbOutTan = "";
                        }
                    }

                    GUI.Label(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 60, 510, 15), lbMsgBox);

                    GUI.Label(new Rect(GUImainRect.xMin + 162, GUImainRect.yMin + 80, 78, 15), "Node Time");
                    GUI.Label(new Rect(GUImainRect.xMin + 292, GUImainRect.yMin + 80, 78, 15), "Node Value");
                    GUI.Label(new Rect(GUImainRect.xMin + 422, GUImainRect.yMin + 80, 78, 15), "In Tangent");
                    GUI.Label(new Rect(GUImainRect.xMin + 552, GUImainRect.yMin + 80, 78, 15), "Out Tangent");

                    tbTime = GUI.TextField(new Rect(140 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbTime);
                    tbValue = GUI.TextField(new Rect(270 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbValue);
                    tbInTan = GUI.TextField(new Rect(400 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbInTan);
                    tbOutTan = GUI.TextField(new Rect(530 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbOutTan);

                    simDuration = GUI.TextField(new Rect(140 + GUImainRect.xMin, 160 + GUImainRect.yMin, 40, 20), simDuration);
                    GUI.Label(new Rect(GUImainRect.xMin + 190, GUImainRect.yMin + 160, 150, 20), "Simulation Run Time (s)");

                    //save node changes
                    if (segCurrentGUI != null)
                    {
                        SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();
                        if (GUI.Button(new Rect(GUImainRect.xMin + 490, GUImainRect.yMin + 160, 160, 20), "Save Changes to Node"))
                        {
                            Keyframe k = new Keyframe();
                            k.time = Convert.ToSingle(tbTime);
                            k.value = Convert.ToSingle(tbValue);
                            k.inTangent = Convert.ToSingle(tbInTan);
                            k.outTangent = Convert.ToSingle(tbOutTan);

                            SRB.MassFlow.RemoveKey(nodeNumber);
                            SRB.MassFlow.AddKey(k);

                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
                            refreshNodeInfo = true;

                            refreshSegGraph = true;
                        }

                        if (GUI.Button(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 130, 160, 20), "Smooth Tangents"))
                        {
                            SRB.MassFlow.SmoothTangents(nodeNumber, 1);

                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
                        }

                        if (GUI.Button(new Rect(GUImainRect.xMin + 320, GUImainRect.yMin + 130, 160, 20), "Flat In Tan"))
                        {
                            float deltaY;
                            float deltaX;

                            deltaY = SRB.MassFlow.keys[nodeNumber].value - SRB.MassFlow.keys[nodeNumber - 1].value;
                            deltaX = SRB.MassFlow.keys[nodeNumber].time - SRB.MassFlow.keys[nodeNumber - 1].time;

                            tbInTan = (deltaY / deltaX).ToString();

                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
                        }

                        if (GUI.Button(new Rect(GUImainRect.xMin + 490, GUImainRect.yMin + 130, 160, 20), "Flat Out Tan"))
                        {
                            float deltaY;
                            float deltaX;

                            deltaY = SRB.MassFlow.keys[nodeNumber + 1].value - SRB.MassFlow.keys[nodeNumber].value;
                            deltaX = SRB.MassFlow.keys[nodeNumber + 1].time - SRB.MassFlow.keys[nodeNumber].time;

                            tbOutTan = (deltaY / deltaX).ToString();

                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
                        }

                        if (nodeNumber != 0)
                        {
                            if (GUI.Button(new Rect(GUImainRect.xMin + 380, GUImainRect.yMin + 160, 100, 20), "Remove Node"))
                            {
                                SRB.MassFlow.RemoveKey(nodeNumber);

                                SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
                                refreshNodeInfo = true;

                                refreshSegGraph = true;
                            }
                        }



                        //highlight the selected booster segment in the editor

                        //segCurrentGUI.SetHighlight(true);
                    }
                }
            }
        }
            




        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUI.Window(1, windowPos, WindowGUI, "AdvSRB Burn Profile Editor");
        }

        public void Activate()  //Called when vessel is placed on the launchpad
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
        }
        public void InitialPos()
        {
            if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                windowPos = new Rect((Screen.width - 680) / 2, (Screen.height - 570) / 2, GUImainRect.width + 2 * GUImainRect.xMin, GUImainRect.height + GUImainRect.yMin + 10);
            }
        }
        public void Deactiveate()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
        }

    }






    #region old code
    //public class EditorGUI : MonoBehaviour
    //{
    //    #region GUI variable declaration
    //    string simDuration;
    //    bool EditorGUIvisible;

    //    private int GUImodeInt = 0;
    //    private string[] GUImodeStrings = { "Stack", "Segments", "Vessel" };

    //    private Rect GUImainRect = new Rect(10, 60, 660, 530);

    //    private Vector2 segListVector = new Vector2(0, 0);
    //    private int segListLength = 0;
    //    private Part segCurrentGUI;
    //    private Part segPriorGUI;

    //    private Vector2 nodeListVector = new Vector2(0, 0);
    //    private int nodeNumber = 0;
    //    private int nodePriorNumber = 0;

    //    private Vector2 autoListVector = new Vector2(0, 0);
    //    private System.Collections.Generic.List<KSFAutoSRB> autoTypeList = new System.Collections.Generic.List<KSFAutoSRB>();
    //    private int autoTypeCurrent;

    //    private string tbTime;
    //    private string tbValue;
    //    private string tbInTan;
    //    private string tbOutTan;

    //    private bool isAutoNode = false;
    //    //float UIduration = 60;
    //    //float UIstart = 1;
    //    //float UIthrust = 0;
    //    //float UIgee = 1;


    //    private string lbMsgBox;

    //    bool refreshNodeInfo = true;
    //    bool refreshSegGraph = true;

    //    private Texture2D segGraph;

    //    private string klipboard;


    //    [KSPField]
    //    public string sResourceType;

    //    [KSPField]
    //    public float graphMassColorR;
    //    [KSPField]
    //    public float graphMassColorG;
    //    [KSPField]
    //    public float graphMassColorB;


    //    [KSPField]
    //    public float graphThrustColorR;
    //    [KSPField]
    //    public float graphThrustColorG;
    //    [KSPField]
    //    public float graphThrustColorB;

    //    [KSPField]
    //    public float graphExtraThrustColorR;
    //    [KSPField]
    //    public float graphExtraThrustColorG;
    //    [KSPField]
    //    public float graphExtraThrustColorB;

    //    #endregion

    //    [KSPField]
    //    public float resourceDensity = .00975f;

    //    private Transform tThrustTransform;

    //    protected Rect EditorGUIPos = new Rect(0, 0, 0, 0);

    //    private Texture2D graphImg = new Texture2D(640, 480);

    //    private Part p;

    //    private int i;


    //    public void Activate()
    //    {
    //        if ((EditorGUIPos.x == 0) && (EditorGUIPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
    //        {
    //            EditorGUIPos = new Rect((Screen.width - 680) / 2, (Screen.height - 570) / 2, GUImainRect.width + 2 * GUImainRect.xMin, GUImainRect.height + GUImainRect.yMin + 10);
    //        }

    //        for (int y = 0; y < graphImg.height; y++)
    //        {
    //            for (int x = 0; x < graphImg.width; x++)
    //            {
    //                graphImg.SetPixel(x, y, Color.black);
    //            }
    //        }


    //        autoTypeList.Clear();
    //        autoTypeList.Add(new autoSeg_ThrustForDuration());
    //        autoTypeList.Add(new autoSeg_ThrustForThrust());
    //        autoTypeList.Add(new autoSeg_ThrustAtTime());
    //        autoTypeList.Add(new autoSeg_ExtraThrustForDuration());
    //        autoTypeList.Add(new autoSeg_ExtraThrustForExtraThrustAtGee());



    //        RenderingManager.AddToPostDrawQueue(3, new Callback(drawEditorGUI));//start the GUI
    //    }

    //    public void Deactivate()
    //    {
    //        RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawEditorGUI)); //close the GUI
    //    }

    //    public void EditorWindowGUI(int windowID, KSF_SBNozzle nozzle)
    //    {
    //        GUIStyle mySty = new GUIStyle(GUI.skin.button);
    //        mySty.normal.textColor = mySty.focused.textColor = Color.white;
    //        mySty.hover.textColor = mySty.active.textColor = Color.yellow;
    //        mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
    //        mySty.padding = new RectOffset(8, 8, 8, 8);
    //        GUI.DragWindow(new Rect(0, 0, 10000, 20));

    //        if (simDuration == null)
    //            simDuration = "135";

    //        //top layer of the window GUI
    //        GUImodeInt = GUI.Toolbar(new Rect(10, 25, 660, 30), GUImodeInt, GUImodeStrings);
    //        GUI.Box(GUImainRect, GUImodeStrings[GUImodeInt]);

    //        //this is the "Stack" mode of the GUI, as used in the first iteration of the GUI
    //        if (GUImodeInt == 0)
    //        {
    //            simDuration = GUI.TextField(new Rect(170 + GUImainRect.xMin, 10 + GUImainRect.yMin, 40, 20), simDuration);
    //            GUI.Label(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 10, 150, 20), "Simulation Run Time (s)");

    //            if (GUI.Button(new Rect(GUImainRect.xMin + 440, GUImainRect.yMin + 10, 200, 20), "Generate Thrust Graph"))
    //            {
    //                //stackThrustPredictPic(480, 640, Convert.ToInt16(simDuration));
    //            }
    //            GUI.Box(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 40, graphImg.width, graphImg.height), graphImg);
    //        }

    //        //this is the "Segment" mode of the GUI, which allows one to customize burn times for each segment
    //        if (GUImodeInt == 1)
    //        {
    //            string segGUIname = "";
    //            KSF_SolidBoosterSegment SRB;

    //            //must populate the buttons with the most current fuel sources
    //            nozzle.FuelStackSearcher(nozzle.FuelSourcesList);

    //            //set length of internal text box to be 50 pix per segment
    //            segListLength = nozzle.FuelSourcesList.Count * 50;

    //            //automatically select first segment as current GUI
    //            if (nozzle.FuelSourcesList.Count > 0 && segCurrentGUI == null)
    //            {
    //                segCurrentGUI = nozzle.FuelSourcesList[0];
    //            }

    //            segListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 10, 120, 450), segListVector, new Rect(0, 0, 100, segListLength + 30));

    //            GUI.Label(new Rect(15, 0, 100, 15), "Nozzle End");
    //            for (int i = 0; i < nozzle.FuelSourcesList.Count; i++)
    //            {
    //                SRB = nozzle.FuelSourcesList[i].GetComponent<KSF_SolidBoosterSegment>();
    //                if (SRB.GUIshortName != "")
    //                    segGUIname = SRB.GUIshortName;
    //                else
    //                    segGUIname = "Unknown";

    //                if (GUI.Button(new Rect(5, i * 50 + 20, 95, 40), segGUIname))
    //                {
    //                    if (nozzle.FuelSourcesList[i] != segCurrentGUI)
    //                    {
    //                        segPriorGUI = segCurrentGUI;
    //                        segCurrentGUI = nozzle.FuelSourcesList[i];
    //                        refreshSegGraph = true;
    //                    }
    //                }
    //            }
    //            GUI.Label(new Rect(15, segListLength + 15, 100, 10), "Top o' Stack");
    //            GUI.EndScrollView();

    //            if (isAutoNode)
    //            {
    //                if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 470, 120, 20), "Go to Manual Node"))
    //                {
    //                    isAutoNode = !isAutoNode;

    //                    refreshNodeInfo = true;
    //                }
    //            }
    //            else
    //            {
    //                if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 470, 120, 20), "Go to Auto Nodes"))
    //                {
    //                    isAutoNode = !isAutoNode;

    //                    refreshNodeInfo = true;
    //                }
    //            }

    //            //copy/paste functionality
    //            if (segCurrentGUI != null)
    //            {
    //                SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();

    //                if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 500, 55, 20), "Copy"))
    //                {
    //                    klipboard = SRB.AnimationCurveToString(SRB.MassFlow);
    //                }

    //                if (GUI.Button(new Rect(GUImainRect.xMin + 75, GUImainRect.yMin + 500, 55, 20), "Paste"))
    //                {
    //                    SRB.MassFlow = SRB.AnimationCurveFromString(klipboard);
    //                    SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);

    //                    refreshNodeInfo = true;
    //                    refreshSegGraph = true;
    //                }
    //            }

    //            int width = 0;
    //            if (segCurrentGUI != null)
    //            {
    //                SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();
    //                width = SRB.MassFlow.length * 60;
    //                if (refreshSegGraph)
    //                {
    //                    System.Collections.Generic.List<Part> fSL = new System.Collections.Generic.List<Part>(); //filled once during OnActivate, is the master list
    //                    fSL.Add(segCurrentGUI);
    //                    //segGraph = segThrustPredictPic(310, 490, Convert.ToInt16(simDuration), segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>(), segCurrentGUI.GetResourceMass(), segCurrentGUI.mass, 5, 5, fSL);
    //                    refreshSegGraph = false; ;
    //                }
    //                GUI.Box(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 190, 510, 330), segGraph);



    //                if (isAutoNode)
    //                {
    //                    //populate the list box
    //                    int listLength = 0;

    //                    listLength = 30 * (autoTypeList.Count + 1);

    //                    //draw the selection box to select which auto mode to use
    //                    autoListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 10, 200, 170), autoListVector, new Rect(0, 0, 180, listLength));

    //                    for (int i = 0; i < autoTypeList.Count; i++)
    //                    {
    //                        if (GUI.Button(new Rect(5, i * 30 + 5, 175, 20), autoTypeList[i].shortName()))
    //                        {
    //                            autoTypeCurrent = i;
    //                        }
    //                    }
    //                    GUI.EndScrollView();

    //                    GUI.Label(new Rect(GUImainRect.xMin + 350, GUImainRect.yMin + 10, 300, 40), autoTypeList[autoTypeCurrent].description());

    //                    autoTypeList[autoTypeCurrent].drawGUI(GUImainRect);


    //                    //this is the button to compute the segment values
    //                    if (GUI.Button(new Rect(GUImainRect.xMin + 500, GUImainRect.yMin + 160, 150, 20), "Evaluate"))
    //                    {
    //                        //SRB.MassFlow = autoTypeList[autoTypeCurrent].computeCurve(this.acISP, segCurrentGUI);

    //                        //SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);

    //                        //refreshSegGraph = true;
    //                    }

    //                }
    //                else
    //                {
    //                    nodeListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 10, 510, 40), nodeListVector, new Rect(0, 0, width + 70, 22));
    //                    if (segCurrentGUI != null)
    //                    {
    //                        SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();

    //                        for (int i = 0; i < SRB.MassFlow.length + 1; i++)
    //                        {
    //                            if (i < SRB.MassFlow.length)
    //                            {
    //                                if (GUI.Button(new Rect(3 + i * 60, 4, 54, 18), "Node " + (i + 1)))
    //                                {
    //                                    nodePriorNumber = nodeNumber;
    //                                    nodeNumber = i;
    //                                    refreshNodeInfo = true;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                if (GUI.Button(new Rect(3 + i * 60, 4, 64, 18), "Add Node"))
    //                                {
    //                                    nodePriorNumber = nodeNumber;
    //                                    nodeNumber = i;
    //                                    SRB.MassFlow.AddKey(SRB.MassFlow.keys[i - 1].time + 10, 0);
    //                                    refreshNodeInfo = true;
    //                                }
    //                            }
    //                        }
    //                    }
    //                    GUI.EndScrollView();

    //                    //set up node editing tools

    //                    if (segCurrentGUI != null && refreshNodeInfo)
    //                    {
    //                        SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();
    //                        lbMsgBox = "The currently selected segment is " + SRB.GUIshortName + " and the current node is Node " + (nodeNumber + 1);

    //                        tbTime = SRB.MassFlow.keys[nodeNumber].time.ToString();
    //                        tbValue = SRB.MassFlow.keys[nodeNumber].value.ToString();
    //                        tbInTan = SRB.MassFlow.keys[nodeNumber].inTangent.ToString();
    //                        tbOutTan = SRB.MassFlow.keys[nodeNumber].outTangent.ToString();

    //                        refreshNodeInfo = false;
    //                    }
    //                    else
    //                    {
    //                        if (segCurrentGUI == null)
    //                        {
    //                            lbMsgBox = "Please select a segment on the left to begin editing";

    //                            tbTime = "";
    //                            tbValue = "";
    //                            tbInTan = "";
    //                            tbOutTan = "";
    //                        }
    //                    }

    //                    GUI.Label(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 60, 510, 15), lbMsgBox);

    //                    GUI.Label(new Rect(GUImainRect.xMin + 162, GUImainRect.yMin + 80, 78, 15), "Node Time");
    //                    GUI.Label(new Rect(GUImainRect.xMin + 292, GUImainRect.yMin + 80, 78, 15), "Node Value");
    //                    GUI.Label(new Rect(GUImainRect.xMin + 422, GUImainRect.yMin + 80, 78, 15), "In Tangent");
    //                    GUI.Label(new Rect(GUImainRect.xMin + 552, GUImainRect.yMin + 80, 78, 15), "Out Tangent");

    //                    tbTime = GUI.TextField(new Rect(140 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbTime);
    //                    tbValue = GUI.TextField(new Rect(270 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbValue);
    //                    tbInTan = GUI.TextField(new Rect(400 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbInTan);
    //                    tbOutTan = GUI.TextField(new Rect(530 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbOutTan);

    //                    simDuration = GUI.TextField(new Rect(140 + GUImainRect.xMin, 160 + GUImainRect.yMin, 40, 20), simDuration);
    //                    GUI.Label(new Rect(GUImainRect.xMin + 190, GUImainRect.yMin + 160, 150, 20), "Simulation Run Time (s)");

    //                    //save node changes
    //                    if (segCurrentGUI != null)
    //                    {
    //                        SRB = segCurrentGUI.GetComponent<KSF_SolidBoosterSegment>();
    //                        if (GUI.Button(new Rect(GUImainRect.xMin + 490, GUImainRect.yMin + 160, 160, 20), "Save Changes to Node"))
    //                        {
    //                            Keyframe k = new Keyframe();
    //                            k.time = Convert.ToSingle(tbTime);
    //                            k.value = Convert.ToSingle(tbValue);
    //                            k.inTangent = Convert.ToSingle(tbInTan);
    //                            k.outTangent = Convert.ToSingle(tbOutTan);

    //                            SRB.MassFlow.RemoveKey(nodeNumber);
    //                            SRB.MassFlow.AddKey(k);

    //                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
    //                            refreshNodeInfo = true;

    //                            refreshSegGraph = true;
    //                        }

    //                        if (GUI.Button(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 130, 160, 20), "Smooth Tangents"))
    //                        {
    //                            SRB.MassFlow.SmoothTangents(nodeNumber, 1);

    //                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
    //                        }

    //                        if (GUI.Button(new Rect(GUImainRect.xMin + 320, GUImainRect.yMin + 130, 160, 20), "Flat In Tan"))
    //                        {
    //                            float deltaY;
    //                            float deltaX;

    //                            deltaY = SRB.MassFlow.keys[nodeNumber].value - SRB.MassFlow.keys[nodeNumber - 1].value;
    //                            deltaX = SRB.MassFlow.keys[nodeNumber].time - SRB.MassFlow.keys[nodeNumber - 1].time;

    //                            tbInTan = (deltaY / deltaX).ToString();

    //                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
    //                        }

    //                        if (GUI.Button(new Rect(GUImainRect.xMin + 490, GUImainRect.yMin + 130, 160, 20), "Flat Out Tan"))
    //                        {
    //                            float deltaY;
    //                            float deltaX;

    //                            deltaY = SRB.MassFlow.keys[nodeNumber + 1].value - SRB.MassFlow.keys[nodeNumber].value;
    //                            deltaX = SRB.MassFlow.keys[nodeNumber + 1].time - SRB.MassFlow.keys[nodeNumber].time;

    //                            tbOutTan = (deltaY / deltaX).ToString();

    //                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
    //                        }

    //                        if (nodeNumber != 0)
    //                        {
    //                            if (GUI.Button(new Rect(GUImainRect.xMin + 380, GUImainRect.yMin + 160, 100, 20), "Remove Node"))
    //                            {
    //                                SRB.MassFlow.RemoveKey(nodeNumber);

    //                                SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);
    //                                refreshNodeInfo = true;

    //                                refreshSegGraph = true;
    //                            }
    //                        }



    //                        //highlight the selected booster segment in the editor

    //                        //segCurrentGUI.SetHighlight(true);
    //                    }
    //                }
    //            }
    //        }
    //    }


    //    public void drawEditorGUI()
    //    {
    //        if (HighLogic.LoadedSceneIsEditor)
    //        {

    //            if (EditorGUIvisible)
    //            {
    //                GUI.skin = HighLogic.Skin;
    //                //EditorGUIPos = GUI.Window(1, EditorGUIPos, EditorWindowGUI, "AdvSRB Analyzer");
    //            }
    //        }
    //    }
    #endregion
}


