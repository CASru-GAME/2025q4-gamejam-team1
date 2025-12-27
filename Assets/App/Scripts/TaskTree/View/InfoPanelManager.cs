using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private List<InfoPanelText> infoPanelTexts;
    [SerializeField] private Image TaskIcon;
    [SerializeField] private Button ActivateButton;
    [SerializeField] private Button ConpleteButton;

    [SerializeField][Header("自動割当・書き込み禁止")] private List<InfoPanelManager> infoPanels;
    public void Initialize(List<InfoPanelManager> allInfoPanels)
    {
        infoPanels = allInfoPanels;
    }
    public void SetInfoPanelTexts(string taskStatus, string isAlternative, string taskTitle, string taskDescription, string taskNeed, string taskReward)
    {
        foreach (var panelText in infoPanelTexts)
        {
            switch (panelText.textType)
            {
                case InfoPanelTextType.TaskStatus:
                    panelText.uiText.text = taskStatus;
                    break;
                case InfoPanelTextType.IsAlternative:
                    panelText.uiText.text = isAlternative;
                    break;
                case InfoPanelTextType.TaskTitle:
                    panelText.uiText.text = taskTitle;
                    break;
                case InfoPanelTextType.TaskDescription:
                    panelText.uiText.text = taskDescription;
                    break;
                case InfoPanelTextType.TaskNeed:
                    panelText.uiText.text = taskNeed;
                    break;
                case InfoPanelTextType.TaskReward:
                    panelText.uiText.text = taskReward;
                    break;
            }
        }
    }

    public void SetInfoPanelIcon(Sprite icon)
    {
        TaskIcon.sprite = icon;
    }

    public void SetFunctionToActivateButton(UnityEngine.Events.UnityAction action)
    {
        ActivateButton.onClick.RemoveAllListeners();
        ActivateButton.onClick.AddListener(action);
    }
    public void SetFunctionToCompleteButton(UnityEngine.Events.UnityAction action)
    {
        ConpleteButton.onClick.RemoveAllListeners();
        ConpleteButton.onClick.AddListener(action);
    }

    public void ToggleInfoPanel()
    {
        if (infoPanel.activeSelf)
        {
            HideInfoPanel(infoPanel);
        }
        else
        {
            GetComponent<RectTransform>().SetAsLastSibling();
            ShowInfoPanel(infoPanel);
            foreach (var panel in infoPanels)
            {
                if (panel != this)
                {
                    panel.HideInfoPanel(panel.infoPanel);
                }
            }
        }
    }
    public bool IsInfoPanelActive()
    {
        return infoPanel.activeSelf;
    }
    private void ShowInfoPanel(GameObject panel)
    {
        panel.SetActive(true);
    }
    private void HideInfoPanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    [System.Serializable]
    private class InfoPanelText
    {
        public InfoPanelTextType textType;
        public Text uiText;
    }

    private enum InfoPanelTextType
    {
        TaskStatus,
        IsAlternative,
        TaskTitle,
        TaskType,
        TaskDescription,
        TaskNeed,
        TaskReward
    }
}
