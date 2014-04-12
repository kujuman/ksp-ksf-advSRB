using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

namespace KSF_SolidRocketBooster
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class KSF_AdvSRB_Editor : MonoBehaviour
    {
        #region toolbar integration


        private IButton tbButton;

        internal KSF_AdvSRB_Editor()
        {
            tbButton = ToolbarManager.Instance.add("AdvSRB", "tbButton");
            tbButton.TexturePath = "000_Toolbar/img_buttonTypeMNode";
            tbButton.ToolTip = "Open/Close the AdvSRB thrust profiler";
            tbButton.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);
        }

        internal void OnDestroy()
        {
            tbButton.Destroy();
        }

        #endregion

        #region variable declaration

        List<Part> lNozzles = new List<Part>();
        List<Part> lSegments = new List<Part>();
        List<Part> lShip = new List<Part>();
        Vessel vInEditor;

        #endregion

        internal override void OnAwake()
        {

        }



        internal override void OnFixedUpdate()
        {
            if(HighLogic.LoadedSceneIsEditor)
            {
                vInEditor = EditorLogic.startPod.vessel;
                lNozzles.Clear();

                foreach (Part p in vInEditor.parts)
                {
                    if (p.Modules.Contains("KSF_SBNozzle"))
                    {
                        if (lNozzles.Contains(p) != true)
                            lNozzles.Add(p);
                        //SRB = p.GetComponent<KSF_SolidBoosterSegment>();
                    }
                }

                lNozzles = ToSymmetryGroups(lNozzles);

                ;
            }
        }

        List<Part> ToSymmetryGroups(List<Part> pl)
        {
            List<Part> tpl;
            bool restart = false;

            tpl = pl;

            Restart:

            restart = false;

            foreach(Part p in tpl)
            {
                foreach (Part sp in p.symmetryCounterparts)
                {
                    if (tpl.Contains(sp))
                    {
                        tpl.Remove(sp);
                        restart = true;
                    }
                }

                if (restart)
                    goto Restart;
            }

            return tpl;
        }
    }
}
