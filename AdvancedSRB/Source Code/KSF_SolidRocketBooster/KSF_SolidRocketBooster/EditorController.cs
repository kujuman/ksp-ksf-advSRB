/*
 * Kerbal Science Foundation Advanced Solid Rocket Booster v0.6.1 for Kerbal Space Program
 * Released May 4, 2014 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 * For attribution, please attribute "kujuman"
 */

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
            tbButton.TexturePath = "KerbalScienceFoundation/AdvSRB/tool_btn";
            tbButton.ToolTip = "Open/Close the AdvSRB thrust profiler";
            tbButton.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);
            tbButton.OnClick += (e) => ToggleController();
        }

        internal void OnDestroy()
        {
            tbButton.Destroy();
        }

        #endregion

        public void ToggleController()
        {
            Debug.Log("AdvSRB: in ToggleController");

            bEditorVisible = !bEditorVisible;

            tbButton.TexturePath = bEditorVisible ? "KerbalScienceFoundation/AdvSRB/tool_btnX" : "KerbalScienceFoundation/AdvSRB/tool_btn";




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
                //eGUI.autoTypeList.Add(new autoSeg_ThrustAtTime());
                eGUI.autoTypeList.Add(new autoSeg_ExtraThrustForDuration());
                //eGUI.autoTypeList.Add(new autoSeg_ExtraThrustForExtraThrustAtGee());
            }

            if (!bEditorVisible)
                eGUI.Deactiveate();

            bFirstRun = false;
        }

        public void Awake()
        {
            Debug.Log("Rise and shine: AdvSRB Editor");
        }
    }
}
