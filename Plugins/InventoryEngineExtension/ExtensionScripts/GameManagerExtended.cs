using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class GameManagerExtended : GameManager
    {
        public override void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            InventoryInputManagerExtended iime = Object.FindObjectOfType<InventoryInputManagerExtended>();
            InputManager im = Object.FindObjectOfType<InputManager>();
            //we check if pause menu is not opened
            if (_pauseMenuOpen == false)
            {
                //we check if ESC button is pressed
                if (im.PauseButton.State.CurrentState.Equals(MMInput.ButtonStates.ButtonDown))
                {
                    //if so, we change it's state to off to nullify it's effect
                    im.PauseButton.State.ChangeState(MMInput.ButtonStates.Off);
                }
                //we check if ESC button is not pressed and we press Toggle inventory button
                if (im.PauseButton.State.CurrentState.Equals(MMInput.ButtonStates.Off) && iime._isPressed)
                {
                    //we unpause the game and close inventory
                    MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
                    Instance.Paused = false;
                    LevelManager.Instance.ToggleCharacterPause();
                    _inventoryOpen = false;
                }
            }
            //if pause menu is opened
            else
            {
                //we unpause game and close pause menu
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
                Instance.Paused = false;
                if ((GUIManager.Instance != null) && (pauseMethod == PauseMethods.PauseMenu))
                {
                    GUIManager.Instance.SetPauseScreen(false);
                    _pauseMenuOpen = false;
                    SetActiveInventoryInputManager(true);
                }
                LevelManager.Instance.ToggleCharacterPause();
            }
        }
    }

}
