[CreateAssetMenu(fileName = "TaskTree", menuName = "Task/TaskTree")]
public class TaskTree : ScriptableObject
{
    [SerializeField] private TaskNode[] nodes;

    public TaskNode[] Nodes => nodes;
    public TaskNode GetNodeByID(int id)
    {
        foreach (var node in nodes)
        {
            if (node.ID == id)
                return node;
        }
        return null;
    }
    public List<TaskNode> GetActiveNodes()
    {
        List<TaskNode> activeNodes = new List<TaskNode>();
        foreach (var node in nodes)
        {
            if (node.isActive)
                activeNodes.Add(node);
        }
        return activeNodes;
    }

    public List<TaskNode> GetCompletedNodes()
    {
        List<TaskNode> completedNodes = new List<TaskNode>();
        foreach (var node in nodes)
        {
            if (node.isCompleted)
                completedNodes.Add(node);
        }
        return completedNodes;
    }

    public int GetTotalNodes()
    {
        return nodes.Length;
    }
}
