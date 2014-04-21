using System;
using System.Collections.Generic;
using UnityEngine;
using Toolbar;

namespace KSF_SolidRocketBooster
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Editor_Controller : MonoBehaviour
    {
        #region toolbar integration

        private EditorGUI eGUI = new EditorGUI();

        private IButton tbButton;

        public bool bEditorVisible = false;
        public bool bFirstRun = true;

        private Editor_Controller()
        {
            tbButton = ToolbarManager.Instance.add("AdvSRB", "tbButton");
            tbButton.TexturePath = "AdvSRB/tool_btn";
            tbButton.ToolTip = "Open/Close the AdvSRB thrust profiler";
            tbButton.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);
            tbButton.OnClick += (e) => ToggleController();
        }

        internal void OnDestroy()
        {
            tbButton.Destroy();
        }

        #endregion

        #region variable declaration

        //List<Part> lNozzles = new List<Part>();
        //List<Part> lSegments = new List<Part>();
        //List<Part> lShip = new List<Part>();
        Vessel vInEditor;

        #endregion

        public void ToggleController()
        {
            Debug.Log("AdvSRB: in ToggleController");

            //FindAdvSRBNozzles();

            bEditorVisible = !bEditorVisible;

            tbButton.TexturePath = bEditorVisible ? "AdvSRB/tool_btnX" : "AdvSRB/tool_btn";




            if (bEditorVisible)
            {
                eGUI.FindAdvSRBNozzles();
                eGUI.Activate();
            }

            if (bFirstRun)
            {
                eGUI.InitialPos();

                eGUI.autoTypeList.Clear();
                eGUI.autoTypeList.Add(new autoSeg_ThrustForDuration());
                eGUI.autoTypeList.Add(new autoSeg_ThrustForThrust());
                eGUI.autoTypeList.Add(new autoSeg_ThrustAtTime());
                eGUI.autoTypeList.Add(new autoSeg_ExtraThrustForDuration());
                eGUI.autoTypeList.Add(new autoSeg_ExtraThrustForExtraThrustAtGee());
            }

            if (!bEditorVisible)
                eGUI.Deactiveate();

            bFirstRun = false;
        }

        public void Awake()
        {
            Debug.Log("Rise and shine: AdvSRB Editor");
            //KSF_AdvSRB_Editor();
        }

        //void FindAdvSRBNozzles()
        //{
        //    lNozzles.Clear();

        //    if (EditorLogic.startPod)
        //        RecursePartList(lShip, EditorLogic.startPod);

        //    foreach (Part p in lShip)
        //    {
        //        if (p.Modules.Contains("KSF_SBNozzle"))
        //        {
        //            if (lNozzles.Contains(p) != true)
        //                lNozzles.Add(p);
        //        }
        //    }
        //    lNozzles = ToSymmetryGroups(lNozzles);
        //}

        //private static void RecursePartList(List<Part> list, Part part) //taken from FAR by Ferram
        //{
        //    list.Add(part);
        //    foreach (Part p in part.children)
        //        RecursePartList(list, p);
        //}

        //List<Part> ToSymmetryGroups(List<Part> pl)
        //{
        //    List<Part> tpl;
        //    bool restart = false;

        //    tpl = pl;

        //    Restart:

        //    restart = false;

        //    foreach(Part p in tpl)
        //    {
        //        foreach (Part sp in p.symmetryCounterparts)
        //        {
        //            if (tpl.Contains(sp))
        //            {
        //                tpl.Remove(sp);
        //                restart = true;
        //            }
        //        }

        //        if (restart)
        //            goto Restart;
        //    }

        //    return tpl;
        //}
    }
}
