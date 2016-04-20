/*
 * Kerbal Science Foundation Advanced Solid Rocket Booster v0.6.1 for Kerbal Space Program
 * Released May 4, 2014 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 * For attribution, please attribute "kujuman"
 */

using UnityEngine;
using KSP;
using KSP.UI.Screens;


namespace KSF_SolidRocketBooster
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Editor_Controller : MonoBehaviour
    {
        /*
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

         * 
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
         * 
         * 
         * 
         */

        #region stock app launcher

        Editor_Controller app;
        ApplicationLauncherButton appButton;

        public bool bEditorVisible = false;
        public bool bFirstRun = true;

        private EditorGUI eGUI = new EditorGUI();

        void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIReady);

            GameEvents.onGameSceneLoadRequested.Add(dEstroy);
        }

        public void dEstroy(GameScenes g)
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIReady);

            if (appButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
            }
        }

        void OnGUIReady()
        {
            if (ApplicationLauncher.Ready)
            {
                appButton = ApplicationLauncher.Instance.AddModApplication(
                    onAppLaunchToggleOn,
                    onAppLaunchToggleOff,
                    onAppLaunchHoverOn,
                    onAppLaunchHoverOff,
                    onAppLaunchEnable,
                    onAppLaunchDisable,
                    ApplicationLauncher.AppScenes.VAB,
                    (Texture)GameDatabase.Instance.GetTexture("KerbalScienceFoundation/AdvSRB/Icon", false)
                  );

  //              appButton = ApplicationLauncher.Instance.AddModApplication(
  //  onAppLaunchToggleOn,
  //  onAppLaunchToggleOff,
  //  onAppLaunchHoverOn,
  //  onAppLaunchHoverOff,
  //  onAppLaunchEnable,
  //  onAppLaunchDisable,
  //  ApplicationLauncher.AppScenes.SPH,
  //  (Texture)GameDatabase.Instance.GetTexture("KerbalScienceFoundation/AdvancedSRB/Icon", false)
  //);
                ;
            }
            app = this;
        }

        void onAppLaunchToggleOn()
        {
            bEditorVisible = false;
            ToggleController();
        }
        void onAppLaunchToggleOff()
        {
            bEditorVisible = true;
            ToggleController();
        }
        void onAppLaunchHoverOn()
        {
        }
        void onAppLaunchHoverOff()
        {
        }
        void onAppLaunchEnable()
        {
            ;
        }
        void onAppLaunchDisable()
        {
            ;
        }

        bool isApplicationTrue()
        {
            return false;
        }

        #endregion

        public void ToggleController()
        {
            Debug.Log("AdvSRB: in ToggleController");

            bEditorVisible = !bEditorVisible;

            //tbButton.TexturePath = bEditorVisible ? "KerbalScienceFoundation/AdvSRB/tool_btnX" : "KerbalScienceFoundation/AdvSRB/tool_btn";




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



    }
}
