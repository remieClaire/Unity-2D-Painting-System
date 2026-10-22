using UnityEngine;
using System.Collections.Generic;

/*
* Base class for all other commands.
*
*/
public class CanvasCommand 
{
    protected List<CanvasCommand> CommandList = new(); 
    public CanvasCommand recentCommand => CommandList[^1];

    protected Stack<CanvasCommand> RedoStack = new();

    public static CanvasCommand Instance { get; } = new CanvasCommand(); // instantiate like this, since this is not MonoBehaviour

    public CanvasCommand()
    {}

    public void AddCommand(CanvasCommand cmd)
    {
        RedoStack.Clear();

        ExecuteCommand(cmd);
        CommandList.Add(cmd);

    }


    public void ExecuteCommand(CanvasCommand cmd)
    {
        if (cmd is ClearCanvas)
        {
            (cmd as ClearCanvas).Execute();
        }
        else if (cmd is DrawCommand)
        {
            (cmd as DrawCommand).Execute();
        }
    }

    public void UndoCommand()
    {

        if (recentCommand is ClearCanvas)
            return;

        // retrieve last command and add to redo list
        RedoStack.Push(recentCommand);

        // remove the last draw command (i.e. the last brush stroke)
        CommandList.RemoveAt(CommandList.Count - 1);

        // clears the render texture 
        GL.Clear(true, true, Color.clear);

        // replay all commands using the shader
        ReplayAllCommands();

    } 

    // ---helper functions---
    private void ReplayAllCommands()
    {

        foreach (var cmd in CommandList)
        {
            if (cmd is ClearCanvas)
            {
                (cmd as ClearCanvas).Undo();
            }
            else if (cmd is DrawCommand)
            {
                (cmd as DrawCommand).Undo();
            }
            
        }
    }

    public void RedoCommand()
    {
        if (RedoStack.Count == 0)
            return;

        // get & remove last command that was undone
        CanvasCommand cmdToRedo = RedoStack.Pop();
        
        // mark command as extendable 
        if (cmdToRedo is DrawCommand)
        {
            (cmdToRedo as DrawCommand).SetExtendable(false);
        }

        // execute command and add back to command list
        ExecuteCommand(cmdToRedo);
        CommandList.Add(cmdToRedo);
    }

}
