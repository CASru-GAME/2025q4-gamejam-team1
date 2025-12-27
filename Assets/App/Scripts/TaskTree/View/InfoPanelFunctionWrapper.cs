using UnityEngine;
using System.Collections.Generic;
public class InfoPanelFunctionWrapper : MonoBehaviour
{
    [SerializeField] private TaskTreeLineGenerator taskTreeLineGenerator;

    public List<InfoPanelPair> GetInfoPanelPairs()
    {
        return taskTreeLineGenerator.GetInfoPanelPairs();
    }

    public void SetInfoPanelTexts(int taskID, string taskStatus, string isAlternative, string taskTitle, string taskType, string taskDescription, string taskNeed, string taskReward)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetInfoPanelTexts(taskStatus, isAlternative, taskTitle, taskType, taskDescription, taskNeed, taskReward);
                break;
            }
        }
    }

    public void UpdateInfoPanelStatus(int taskID, string taskStatus)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.UpdateInfoPanelStatus(taskStatus);
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

    public void SetFunctionToActivateButton(int taskID, List<UnityEngine.Events.UnityAction> actions)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetFunctionToActivateButton(actions);
                break;
            }
        }
    }

    public void SetFunctionToCompleteButton(int taskID, List<UnityEngine.Events.UnityAction> actions)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetFunctionToCompleteButton(actions);
                break;
            }
        }
    }

    public void SetFunctionToDeliverButton(int taskID, List<UnityEngine.Events.UnityAction> actions)
    {
        var infoPanelPairs = taskTreeLineGenerator.GetInfoPanelPairs();
        foreach (var pair in infoPanelPairs)
        {
            if (pair.taskID == taskID)
            {
                pair.infoPanelManager.SetFunctionToDeliverButton(actions);
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