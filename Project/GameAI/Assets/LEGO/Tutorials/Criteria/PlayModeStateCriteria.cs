using System.Linq;
using UnityEngine;

namespace Unity.LEGO.Tutorials
{
    /// <summary>
    /// Contains all the callbacks needed for the tutorial steps that check for special Play mode toggling
    /// </summary>
    [CreateAssetMenu(fileName = "PlayModeStateCriteria", menuName = "Tutorials/Microgame/PlayModeStateCriteria")]
    class PlayModeStateCriteria : ScriptableObject
    {
        bool playerEnteredPlayMode = false;
        bool playerExitedPlayMode = false;

        public void ResetPlayModeToggles()
        {
            playerEnteredPlayMode = false;
            playerExitedPlayMode = false;
        }

        public bool AutoComplete()
        {
            playerEnteredPlayMode = true;
            playerExitedPlayMode = true;
            return true;
        }

        public bool UserEnteredAndExitedPlayMode()
        {
            if (Application.isPlaying)
            {
                playerEnteredPlayMode = true;
            }
            else if (playerEnteredPlayMode)
            {
                playerExitedPlayMode = true;
            }
            return playerEnteredPlayMode && playerExitedPlayMode;
        }
    }
}
