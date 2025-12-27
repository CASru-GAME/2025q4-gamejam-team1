using UnityEngine;
using System.Collections.Generic;
public class InfoPanelFunctionWrapper : MonoBehaviour
{
    [SerializeField] private TaskTreeLineGenerator taskTreeLineGenerator;

    public List<InfoPanelPair> GetInfoPanelPairs()
    {
        return taskTreeLineGenerator.GetInfoPanelPairs();
    }

    public void SetInfoPanelTexts(int taskID, string taskStatus, string isAlternative, string taskTitle, string taskDescription, string taskNeed, string taskReward)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetInfoPanelTexts(taskStatus, isAlternative, taskTitle, taskDescription, taskNeed, taskReward);
                break;
            }
        }
    }

    public void SetInfoPanelIcon(int taskID, Sprite icon)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetInfoPanelIcon(icon);
                break;
            }
        }
    }

    public void SetFunctionToActivateButton(int taskID, UnityEngine.Events.UnityAction action)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetFunctionToActivateButton(action);
                break;
            }
        }
    }

    public void SetFunctionToCompleteButton(int taskID, UnityEngine.Events.UnityAction action)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetFunctionToCompleteButton(action);
                break;
            }
        }
    }
}

public class InfoPanelPair
{
    public InfoPanelManager infoPanelManager;
    public int taskID;

    public InfoPanelPair(InfoPanelManager manager, int id)
    {
        infoPanelManager = manager;
        taskID = id;
    }
}