using System.Collections.Generic;
using System.Linq;
using BaseGame.Utils;
using deVoid.UIFramework;
using deVoid.Utils;
using UnityEngine;

namespace DevoidUI.Core
{
    public class FrameManagerOpenWindowSignal : ASignal<string, IWindowProperties>
    {
    }

    public class FrameManagerCloseWindowSignal : ASignal<string>
    {
    }

    public class FrameManagerOpenPanelSignal : ASignal<string, IPanelProperties>
    {
    }

    public class FrameManagerClosePanelSignal : ASignal<string>
    {
    }
    [System.Serializable]
    public class UILayer
    {
        public UIFrame UIFrame;
        public List<string> ScreenIds = new List<string>();
    }

    public class UIFrameManager : ManualSingletonMono<UIFrameManager>
    {
        [SerializeField] private UISettings[] _layerSettings;
        [SerializeField] private List<UILayer> uiLayers = new List<UILayer>();

        public override void Awake()
        {
            base.Awake();
            uiLayers = new List<UILayer>();
            var frames = FindObjectsOfType<UIFrame>();
            var order = 999;
            if (frames.Length > 0)
            {
                order = frames.Max(f => f.GetComponent<Canvas>()
                    .sortingOrder);
            }

            foreach (var layerSetting in _layerSettings)
            {
                var frame = layerSetting.CreateUIInstance();
                frame.GetComponent<Canvas>()
                    .sortingOrder = ++order;
                uiLayers.Add(new UILayer
                {
                    ScreenIds = layerSetting.ScreenIDs,
                    UIFrame = frame
                });
            }
            Signals.Get<FrameManagerOpenWindowSignal>()
                .AddListener(OpenWindow);
            Signals.Get<FrameManagerCloseWindowSignal>()
                .AddListener(CloseWindow);
            Signals.Get<FrameManagerOpenPanelSignal>()
                .AddListener(OpenPanel);
            Signals.Get<FrameManagerClosePanelSignal>()
                .AddListener(CloseExactPanel);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Signals.Get<FrameManagerCloseWindowSignal>()
                .RemoveListener(CloseWindow);
            Signals.Get<FrameManagerOpenWindowSignal>()
                .RemoveListener(OpenWindow);
            Signals.Get<FrameManagerOpenPanelSignal>()
                .RemoveListener(OpenPanel);
            Signals.Get<FrameManagerClosePanelSignal>()
                .RemoveListener(CloseExactPanel);
        }


        public void CloseWindow(string windowId)
        {
            foreach (var uiLayer in uiLayers)
            {
                if (uiLayer.ScreenIds.Contains(windowId))
                {
                    uiLayer.UIFrame.CloseCurrentWindow();
                    return;
                }
            }

        }

        /// <summary>
        /// Use this for closing window with id
        /// </summary>
        public void CloseExactWindow(string windowId)
        {
            foreach (var uiLayer in uiLayers)
            {
                if (uiLayer.ScreenIds.Contains(windowId))
                {
                    uiLayer.UIFrame.CloseWindow(windowId);
                    return;
                }
            }
        }

        public void OpenPanel(string panelId)
        {
            Debug.Log($"[TestNavigation] -- open panel {panelId}");
            // You usually don't have to do this as the system takes care of everything
            // automatically, but since we're dealing with navigation and the Window layer
            // has a history stack, this way we can make sure we're not just adding
            // entries to the stack indefinitely

            foreach (var uiLayer in uiLayers)
            {
                if (uiLayer.ScreenIds.Contains(panelId))
                {
                    uiLayer.UIFrame.ShowPanel(panelId);
                    return;
                }
            }
        }

        public void OpenPanel(string panelId, IPanelProperties panelProperties = null)
        {
            foreach (var uiLayer in uiLayers)
            {
                if (uiLayer.ScreenIds.Contains(panelId))
                {
                    uiLayer.UIFrame.ShowPanel(panelId, panelProperties);
                    return;
                }
            }
        }

        public void CloseExactPanel(string panelId)
        {
            foreach (var uiLayer in uiLayers)
            {
                if (uiLayer.ScreenIds.Contains(panelId))
                {
                    uiLayer.UIFrame.HidePanel(panelId);
                    return;
                }
            }
        }

        public void OpenWindow(string windowId, IWindowProperties properties = null)
        {

            Debug.Log($"[TestNavigation] -- open window {windowId}");
            // You usually don't have to do this as the system takes care of everything
            // automatically, but since we're dealing with navigation and the Window layer
            // has a history stack, this way we can make sure we're not just adding
            // entries to the stack indefinitely

            foreach (var uiLayer in uiLayers)
            {
                if (uiLayer.ScreenIds.Contains(windowId))
                {
                    uiLayer.UIFrame.CloseCurrentWindow();
                    uiLayer.UIFrame.OpenWindow(windowId, properties);
                    return;
                }
            }

        }
    }
}