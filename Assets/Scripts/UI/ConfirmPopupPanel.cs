using deVoid.UIFramework;
using deVoid.Utils;
using DevoidUI.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfirmPopupPanel : AWindowController
{
    public Button yesBtn;
    public Button noBtn;
    
    void Start()
    {
        yesBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
        noBtn.onClick.AddListener(() =>
        {
            UIFrameManager.Instance.CloseWindow(ScreenIds.ConfirmPopup);
            Signals.Get<GameManagerCancelExitSignal>().Dispatch();
        });
    }
}
