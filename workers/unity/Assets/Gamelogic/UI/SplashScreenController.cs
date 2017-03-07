using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.UI
{
    public class SplashScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject NotReadyWarning;
        [SerializeField] private Button ConnectButton;

        private Bootstrap bootstrap;

        private static SplashScreenController instance;

        private void Awake()
        {
            instance = this;
        }

        public static void HideSplashScreen()
        {
            instance.NotReadyWarning.SetActive(false);
            instance.gameObject.SetActive(false);
        }

        public void AttemptToConnect()
        {
            DisableConnectButton();
            instance.bootstrap = GameObject.Find("GameEntry").GetComponent<Bootstrap>();
            if (instance.bootstrap.IsReadyToConnectClient())
            {
                instance.AttemptConnection();
            }
            else
            {
                instance.DisplayNotReadyWarning();
                StartCoroutine(TimerUtils.WaitAndPerform(7, EnableConnectButton));
            }
        }

        private void EnableConnectButton()
        {
            ConnectButton.interactable = true;
        }

        private void DisableConnectButton()
        {
            ConnectButton.interactable = false;
            ConnectButton.GetComponent<CursorHoverEffect>().ShowDefaultCursor();
        }

        private void AttemptConnection()
        {
            instance.bootstrap.AttemptClientConnect();
            StartCoroutine(TimerUtils.WaitAndPerform(7, DisplayNotReadyWarning));
        }

        private void DisplayNotReadyWarning()
        {
            if (gameObject.activeSelf)
            {
                instance.NotReadyWarning.SetActive(true);
                EnableConnectButton();
            } 
        }
    }
}

