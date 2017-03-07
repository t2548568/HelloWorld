using Assets.Gamelogic.Core;
using Assets.Gamelogic.UI;
using Assets.Gamelogic.Utils;
using Improbable.Core;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityClient)]
    class PlayerLifecycle : MonoBehaviour
    {
		[Require] private ClientAuthorityCheck.Writer clientAuthorityCheck;

        private void OnEnable()
        {
            MainCameraController.SetTarget(gameObject);
            UIController.ShowUI();
            StartCoroutine(TimerUtils.WaitAndPerform(1, SplashScreenController.HideSplashScreen));
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void OnApplicationQuit()
        {
            if (SpatialOS.IsConnected)
            {
                ClientPlayerSpawner.DeletePlayer();
            }
        }
    }
}
