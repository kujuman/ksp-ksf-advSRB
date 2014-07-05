
/*
 * Kerbal Science Foundation Advanced Solid Rocket Booster v0.6.1 for Kerbal Space Program
 * Released May 4, 2014 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 * For attribution, please attribute "kujuman"
 */

using KSP;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;



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

        private bool isAutoNode = true;

        private string lbMsgBox;

        bool refreshNodeInfo = true;
        bool refreshSegGraph = true;

        private Texture2D segGraph;

        private Texture2D stackGraph;

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

            lShip.Add(EditorLogic.startPod);

            foreach (Part p in EditorLogic.startPod.FindChildParts<Part>(true))
            {
                lShip.Add(p);
            }



                Debug.Log("AdvSRB: found " + lShip.Count + " parts in this vessel");

            foreach (Part p in lShip)
            {
                if (p.Modules.Contains("AdvSRBNozzle"))
                {
                    if (lNozzles.Contains(p) != true)
                        lNozzles.Add(p);
                }
                pc++;
            }
            lsNozzles.Clear();

            foreach  (Part p in lNozzles)
            {
                lsNozzles.Add(p);
            }

            lsNozzles = ToSymmetryGroups(lsNozzles);
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
            {
                segmentGUI(lNozzles[iNozzles].GetComponent<AdvSRBNozzle>());
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void stackGUI()
        {
            //this is the "Stack" mode of the GUI, as used in the first iteration of the GUI

            simDuration = GUI.TextField(new Rect(170 + GUImainRect.xMin, 10 + GUImainRect.yMin, 40, 20), simDuration);
            
            GUI.Label(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 10, 150, 20), "Simulation Run Time (s)");


         //set length of internal text box to be 50 pix per segment
         nozListLength = lNozzles.Count * 50;

            //**********************
            nozListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 10, 170, 450), nozListVector, new Rect(0, 0, 100, nozListLength));

            //GUI.Label(new Rect(15, 0, 100, 15), "Nozzle End");
            for (int i = 0; i < lNozzles.Count; i++)
            {
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



            if (GUI.Button(new Rect(GUImainRect.xMin + 440, GUImainRect.yMin + 10, 200, 20), "Generate Thrust Graph"))
            {
                AdvSRBNozzle nozzle;
                nozzle = lNozzles[iNozzles].GetComponent<AdvSRBNozzle>();

                stackGraph = nozzle.stackThrustPredictPic(480, 640, Convert.ToInt16(simDuration), nozzle.atmosphereCurve);
            }


            //GUI.Box(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 40, 640, 480), stackGraph);//use this 4/20/14


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
        

        public void segmentGUI(AdvSRBNozzle nozzle)
        {
            if (DebugDetail > 0)
                Debug.Log("AdvSRB: in segmentGUI");

            //this is the "Segment" mode of the GUI, which allows one to customize burn times for each segment
            string segGUIname = "";
            AdvSRBSegment SRB;

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
                SRB = nozzle.FuelSourcesList[i].GetComponent<AdvSRBSegment>();
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
                    if (GUI.Button(new Rect(5, i * 50 + 20, 95, 40), segGUIname))
                    {
                        if (nozzle.FuelSourcesList[i] != segCurrentGUI)
                        {
                            segPriorGUI = segCurrentGUI;
                            segCurrentGUI = nozzle.FuelSourcesList[i];
                            iSegments = i;
                            refreshSegGraph = true;
                        }
                    }
                }




            }
            GUI.Label(new Rect(15, segListLength + 15, 100, 10), "Top o' Stack");
            GUI.EndScrollView();


            if (GUI.Button(new Rect(GUImainRect.xMin + 10, GUImainRect.yMin + 445, 120, 20), "Apply to Symmetry"))
            {
                AdvSRBSegment s;
                AdvSRBSegment sb;


                if (segCurrentGUI != null)
                {
                    s = segCurrentGUI.GetComponent<AdvSRBSegment>();

                    

                    foreach(Part p in segCurrentGUI.symmetryCounterparts)
                    {
                        Debug.Log("Symmetry Found! " + p.partName);

                        sb = p.GetComponent<AdvSRBSegment>();

                        sb.MassFlow = s.MassFlow;

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
                SRB = segCurrentGUI.GetComponent<AdvSRBSegment>();

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
                SRB = segCurrentGUI.GetComponent<AdvSRBSegment>();
                width = SRB.MassFlow.length * 60;
                if (refreshSegGraph)
                {
                    System.Collections.Generic.List<Part> fSL = new System.Collections.Generic.List<Part>(); //filled once during OnActivate, is the master list
                    fSL.Add(segCurrentGUI);
                    segGraph = nozzle.segThrustPredictPic(310, 490, Convert.ToInt16(simDuration), segCurrentGUI.GetComponent<AdvSRBSegment>(), segCurrentGUI.GetResourceMass(), segCurrentGUI.mass, 5, 5, fSL);
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

                    if (autoTypeList[autoTypeCurrent].useWithSegment())
                    {
                        //this is the button to compute the segment values
                        if (GUI.Button(new Rect(GUImainRect.xMin + 500, GUImainRect.yMin + 140, 150, 20), "Evaluate for Segment"))
                        {
                            SRB.MassFlow = autoTypeList[autoTypeCurrent].computeCurve(nozzle.atmosphereCurve, segCurrentGUI);

                            SRB.BurnProfile = SRB.AnimationCurveToString(SRB.MassFlow);

                            refreshSegGraph = true;
                        }
                    }

                    if (autoTypeList[autoTypeCurrent].useWithStack())
                    {
                        //this is the button to compute stack values
                        if (GUI.Button(new Rect(GUImainRect.xMin + 500, GUImainRect.yMin + 170, 150, 20), "Evaluate for Stack"))
                        {
                            AdvSRBSegment s;

                            foreach (Part p in nozzle.FuelSourcesList)
                            {
                                s = p.Modules.OfType<AdvSRBSegment>().FirstOrDefault();

                                s.MassFlow = autoTypeList[autoTypeCurrent].computeCurve(nozzle.atmosphereCurve, p);
                                s.BurnProfile = s.AnimationCurveToString(s.MassFlow);
                            }

                            refreshSegGraph = true;
                        }
                    }

                }
                else
                {
                    nodeListVector = GUI.BeginScrollView(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 10, 510, 40), nodeListVector, new Rect(0, 0, width + 70, 22));
                    if (segCurrentGUI != null)
                    {
                        SRB = segCurrentGUI.GetComponent<AdvSRBSegment>();

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
                        SRB = segCurrentGUI.GetComponent<AdvSRBSegment>();
                        //lbMsgBox = "The currently selected segment is " + SRB.GUIshortName + " and the current node is Node " + (nodeNumber + 1); //removed May 4, 2014: asterisks make redundant

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

                    

                    GUI.Label(new Rect(GUImainRect.xMin + 162, GUImainRect.yMin + 80, 78, 15), "Node Time (s)");
                    GUI.Label(new Rect(GUImainRect.xMin + 292, GUImainRect.yMin + 80, 78, 15), "Node Value");
                    GUI.Label(new Rect(GUImainRect.xMin + 422, GUImainRect.yMin + 80, 78, 15), "In Tangent"); 
                    GUI.Label(new Rect(GUImainRect.xMin + 552, GUImainRect.yMin + 80, 78, 15), "Out Tangent");

                    tbTime = GUI.TextField(new Rect(140 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbTime);
                    tbValue = GUI.TextField(new Rect(270 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbValue);
                    tbInTan = GUI.TextField(new Rect(400 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbInTan);
                    tbOutTan = GUI.TextField(new Rect(530 + GUImainRect.xMin, 100 + GUImainRect.yMin, 120, 20), tbOutTan);

                    

                    GUI.Label(new Rect(GUImainRect.xMin + 140, GUImainRect.yMin + 60, 510, 15), convertMassFlowToThrust(Convert.ToSingle(tbValue), nozzle.atmosphereCurve, lbMsgBox));


                    simDuration = GUI.TextField(new Rect(140 + GUImainRect.xMin, 160 + GUImainRect.yMin, 40, 20), simDuration);
                    GUI.Label(new Rect(GUImainRect.xMin + 190, GUImainRect.yMin + 160, 150, 20), "Simulation Run Time (s)");

                    //save node changes
                    if (segCurrentGUI != null)
                    {
                        SRB = segCurrentGUI.GetComponent<AdvSRBSegment>();
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
            
        private string convertMassFlowToThrust(float MassFlow, FloatCurve atmosphereCurve, string msgOut)
        {
            msgOut = "";
            msgOut = "Atmosphere thrust at this node is " + (9.80665f * atmosphereCurve.Evaluate(1) * MassFlow) + "kN, vacuum thrust is " + (9.80665f * atmosphereCurve.Evaluate(0) * MassFlow) + "kN";
            return msgOut;
            ;
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
}



