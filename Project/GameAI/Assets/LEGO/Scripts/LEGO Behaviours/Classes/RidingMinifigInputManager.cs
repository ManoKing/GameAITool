using UnityEngine;
using Unity.LEGO.Game;
using Unity.LEGO.Minifig;

namespace Unity.LEGO.Behaviours
{
    public class RidingMinifigInputManager : MinifigInputManager
    {
        protected override void OnGameOver(GameOverEvent evt) 
        {
            var ridingMinifigController = m_MinifigController as RidingMinifigController;

            // Disable input when the game is over.
            ridingMinifigController.SetInputEnabled(false);

            // If we have won, turn to the camera and do a little celebration!
            if (evt.Win)
            {
                ridingMinifigController.TurnTo(Camera.main.transform.position);

                var randomCelebration = Random.Range(0, 1);
                switch (randomCelebration)
                {
                    case 0:
                        {
                            ridingMinifigController.PlaySpecialAnimation(RidingMinifigController.SpecialAnimation.Dance);
                            break;
                        }
                }
            }
        }
    }
}
