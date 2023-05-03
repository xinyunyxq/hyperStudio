using DT.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using System;

public class UIManager : CBC
{
    [SerializeField] private UISettings UISettings = null;
    private UIFrame uiFrame;

    private void Start()
    {

        uiFrame = UISettings.CreateUIInstance();
        uiFrame.ShowPanel(ScreenIds.NavigationPanel);
        //uiFrame.ShowPanel(ScreenIds.mouse);

        Signals.Get<ShowHelpPanel>().AddListener(OnShowHelpPanel);
        Signals.Get<ShowSettingPanel>().AddListener(OnShowSettingPanel);
    }

    private void OnShowSettingPanel()
    {
        if (uiFrame.IsPanelOpen(ScreenIds.SettingsPannel))
        {
            uiFrame.HidePanel(ScreenIds.SettingsPannel);
        }
        else
        {
            hideAllPannel();
            uiFrame.ShowPanel(ScreenIds.SettingsPannel);
        }
    }

    private void OnShowHelpPanel()
    {
        if (uiFrame.IsPanelOpen(ScreenIds.HelpPanel))
        {
            uiFrame.HidePanel(ScreenIds.HelpPanel);
        }
        else
        {
            hideAllPannel();
            uiFrame.ShowPanel(ScreenIds.HelpPanel);
        }
    }

    private void hideAllPannel()
    {
        uiFrame.HidePanel(ScreenIds.HelpPanel);
        uiFrame.HidePanel(ScreenIds.SettingsPannel);
    }

    public void ShowUI(bool value)
    {
        if(value)
        {
            uiFrame.ShowPanel("NavigationPanel");
        }
        else
        {
            hideAllPannel();
            uiFrame.HidePanel("NavigationPanel");
        }
    }
}
