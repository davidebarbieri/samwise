// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public partial class DialogueMachine
    {
        class DialogueContext : IDialogueContext, IExternalCodeContext
        {
            public DialogueStatus Status { get; private set; }
            public long Uid { get; internal set; }
            public string BranchName { get; private set; }
            public IDialogueNode Current { get; private set; }

            public DialogueMachine Machine { get; private set; }
            public IDialogueContext Parent => parent;
            public object ExternalContext { get; private set; }
            public bool IsEnded => Current == null && children.Count == 0;

            public IDataRoot DataRoot => Machine.dataRoot;
            public IDataContext DataContext => localDataContext;

            internal IReadOnlyList<DialogueContext> Children => children;

            public DialogueContext(DialogueMachine machine, long uid)
            {
                Machine = machine;
                Uid = uid;
            }

            public void Stop()
            {
                if (Status == DialogueStatus.Stopping || Status == DialogueStatus.Stopped)
                    return;

                Status = DialogueStatus.Stopping;
                for (int i=children.Count-1; i>=0; --i)
                {
                    children[i].Stop();
                }

                if (Current != null)
                    OnLeaveNode(Current);
                    
                Current = null;

                parent?.OnChildStopped(this);
                Clear();
                Machine.OnDialogueContextStop(this);
            }

            public bool Advance()
            {
                if (Status == DialogueStatus.Running && Current is IAdvanceableNode)
                {
                    SwitchNode(((IAdvanceableNode)Current).Advance(Machine.Dialogues));
                    return true;
                }
                else if (Status == DialogueStatus.ShowingSpeechOption)
                {
                    var option = waitingOption;
                    waitingOption = null;
                    ((Option)option).OnVisited(this);

                    var nextNode = reenterNode;
                    reenterNode = null;
                    Status = DialogueStatus.Running;
                    SwitchNode(nextNode);
                }
                else if (Status == DialogueStatus.Waiting && Current is WaitExpressionNode waitExpressionCurrent) // waiting on expression
                {
                    if (waitExpressionCurrent.Expression.EvaluateBool(this))
                    {
                        Status = DialogueStatus.Running;
                        waitExpressionCurrent.Expression.OnVisited(this);
                        SwitchNode(Current.FindNextSibling());
                        return true;
                    }
                }
#if DEBUG
                else if (Status == DialogueStatus.Stopped || Status == DialogueStatus.Stopping)
                {
                    Machine.Log("You are trying to advance on a stopped context. This is most probably a bug.");
                }
#endif
                
                return false;
            }

            public bool TryResolveMissingDialogues()
            {
                if (Status == DialogueStatus.Resolving)
                {
                    if ((reenterNode is IReferencingNode referencingNode) && Machine.Resolve(referencingNode.DestinationDialogueId) != null)
                    {
                        reenterNode = null;
                        Status = DialogueStatus.Running;
                        Machine.onWaitForMissingDialogueEnd?.Invoke(this, referencingNode.DestinationDialogueId);
                        OnEnterNode(reenterNode, true);
                        return true;
                    }
                    return false;
                }

                return true;
            }

            public bool Choose(IOption option)
            {
                if (Status == DialogueStatus.Running)
                {
                    if (option != null && Current == option.Parent)
                    {
                        // Check if option needs challenge
                        if (option.HasCheck(out _, out var checkName))
                        {
                            waitingOption = option;
                            Status = DialogueStatus.Challenging;
                            Machine.onChallengeStart(this, option, checkName);
                            return true;
                        }

                        if (option.MuteOption)
                        {
                            ((Option)option).OnVisited(this);
                            SwitchNode(((ChoiceNode)Current).Next(option, this));
                        }
                        else
                        {
                            waitingOption = option;
                            Status = DialogueStatus.ShowingSpeechOption;
                            reenterNode = ((ChoiceNode)Current).Next(option, this);
                            Machine.onSpeechOptionStart?.Invoke(this, option);
                        }
                        return true;
                    }
                    else if (option == null && Current is IChoosableNode)
                    {
                        // Skip option
                        SwitchNode(((ChoiceNode)Current).Next(null, this));
                        return true;
                    }
                }
                
                return false;
            }

            public void CompleteChallenge(bool passed)
            {
                if (Status == DialogueStatus.Challenging)
                {
                    Status = DialogueStatus.Running;

                    // Set bPass, bFail
                    DataContext.SetValueBool("bPass", passed);
                    DataContext.SetValueBool("bFail", !passed);

                    // it was a challenged choice
                    if (Current is CheckNode)
                    {
                        CheckNode c = Current as CheckNode;
                        SwitchNode(c.Next(this));
                    }
                    else if (reenterNode != null) // it was a challenged node
                    {
                        var node = reenterNode;
                        reenterNode = null;

                        if (passed)
                            OnEnterNode(node, true);
                        else
                        {
                            // skip node (it's necessarily a pre-check)
                            SwitchNode(node.FindNextSibling(), true);
                        }
                    }
                    else if (waitingOption != null)
                    {
                        var option = (Option)waitingOption;
                        var isPreChallenge = option.IsPreCheck;
                        waitingOption = null;

                        if (isPreChallenge && !passed)
                        {
                            SwitchNode(Current.FindNextSibling());
                        }
                        else
                        {
                            if (option.MuteOption)
                            {
                                option.OnVisited(this);
                                SwitchNode(((ChoiceNode)Current).Next(option, this));
                            }
                            else
                            {
                                waitingOption = option;
                                Status = DialogueStatus.ShowingSpeechOption;
                                reenterNode = ((ChoiceNode)Current).Next(option, this);
                                Machine.onSpeechOptionStart?.Invoke(this, option);
                            }
                        }
                    }
                }
            }

            internal void Clear()
            {
                if (localDataContext != null)
                {  
                    DataRoot.ReleaseData(localDataContext);
                    localDataContext = null;
                }

                waitingForJoin = null;
                reenterNode = null;
                Status = DialogueStatus.Stopped;

                children.Clear();
                branchesMap.Clear();
                parent = null;
                ExternalContext = null;
                forkedFromNode = null;
            }

            internal void Start(string branchName, IDialogueNode node, DialogueContext parent, object externalContext = null)
            {
                Machine.runningContexes.Add(this);

                if (node == null)
                   throw new System.ArgumentException("Can't start a null node");

                if (parent != null)
                {
                    parent.children.Add(this);
                    if (!string.IsNullOrEmpty(branchName))
                        parent.branchesMap[branchName] = this;
                }

                localDataContext = DataRoot.CreateData(this);

                BranchName = branchName;
                this.parent = parent;

                if (parent == null)
                    depth = 0;
                else
                    depth = parent.depth + 1;

                if (depth > 1000)
                {
                    // probably there's a bug
                    throw new System.StackOverflowException("The Samwise machine forked reached a depth level of 1000. This mostly means there's an endless forking loop in your samwise dialogue.");
                } 

                ExternalContext = externalContext ?? (parent != null ?  parent.ExternalContext : null);
                Status = DialogueStatus.Running;

                SwitchNode(node);
            }

            void Start(string branchName, IAsyncCode code, DialogueContext parent)
            {
                if (parent != null)
                {
                    parent.children.Add(this);
                    if (!string.IsNullOrEmpty(branchName))
                        parent.branchesMap[branchName] = this;
                }

                // no data context
                localDataContext = null;
                Current = null;
               
                BranchName = branchName;
                this.parent = parent;

                if (parent == null)
                    depth = 0;
                else
                    depth = parent.depth + 1;

                if (depth > 1000)
                {
                    // probably there's a bug
                    throw new System.StackOverflowException("The Samwise machine forked reached a depth level of 1000. This mostly means there's an endless forking loop in your samwise dialogue.");
                }

                Status = DialogueStatus.Running;
                
                if (Machine.externalCodeMachine == null)
                {
                    Machine.Log("Error: No External Code Machine configured but external code nodes are present in the dialogue");
                    OnEnd(); // auto-end (skip)
                }
                else
                {
                    Machine.externalCodeMachine.Dispatch(this, code); 
                }
            }

            internal void InitializeStarted(string branchName, IDialogueNode node, DialogueContext parent, object externalContext = null)
            {
                BranchName = branchName;
                this.parent = parent;

                if (parent != null)
                {
                    parent.children.Add(this);
                    if (!string.IsNullOrEmpty(branchName))
                        parent.branchesMap[branchName] = this;
                }

                if (node != null)
                {
                    Machine.runningContexes.Add(this);
                    localDataContext = DataRoot.CreateData(this);
                }
                else
                    localDataContext = null; // no data context, if no node

                if (parent == null)
                    depth = 0;
                else
                    depth = parent.depth + 1;

                ExternalContext = externalContext ?? (parent != null ?  parent.ExternalContext : null);
                Status = DialogueStatus.Running;

                Current = node;
            }

            // Serialization/Deserialization
            internal void GetInternalState(out DialogueStatus status, out IDialogueNode reenterNode, out IDialogueNode forkedFromNode, out IOption waitingOption, out DialogueContext waitingForJoin)
            {
                status = this.Status;
                reenterNode = this.reenterNode;
                waitingForJoin = this.waitingForJoin;
                forkedFromNode = this.forkedFromNode;
                waitingOption = this.waitingOption;
            }

            internal void SetInternalState(DialogueStatus status, IDialogueNode reenterNode, IDialogueNode forkedFromNode, IOption waitingOption, DialogueContext waitingForJoin)
            {
                this.Status = status;
                this.reenterNode = reenterNode;
                this.waitingForJoin = waitingForJoin;
                this.forkedFromNode = forkedFromNode;
                this.waitingOption = waitingOption;
            }

            void SwitchNode(IDialogueNode node, bool isSkippingPreviousNode = false)
            {
                if (!isSkippingPreviousNode)
                {
                    if (Current == node)
                        return;

                    if (Current != null)
                        OnLeaveNode(Current);
                    else
                    {
                        // null -> Current (it's starting)
                        Current = node; // set it before callback
                        Machine.onDialogueContextStart?.Invoke(this);
                    }
                }

                Current = node;

                if (node != null)
                    OnEnterNode(node);
                else
                {
                    // Current -> null (it's ending)
                    if (children.Count == 0)
                    {
                        OnEnd();
                        Machine.OnDialogueContextEnd(this);
                    }   
                }
            }

            void OnEnterNode(IDialogueNode node, bool skipPreamble = false)
            {
                if (!skipPreamble)
                {
                    // Check if any interrupt is triggered
                    if (node.InnermostInterruptibleNode != null)
                    {
                        // The current node is interrupted?
                        var triggeredInterrupt = node.InnermostInterruptibleNode.FindTriggeredParent(this);

                        if (triggeredInterrupt != null)
                        {
                            // Interrupt this context subtree
                            SwitchNode(triggeredInterrupt.FindNextSibling(), true);
                            return;
                        }
                    }
                    
                    // Check if any interrupt is triggered (parent fork)
                    if (forkedFromNode != null && forkedFromNode.InnermostInterruptibleNode != null)
                    {
                        // Was a parent node triggered?
                        var triggeredInterrupt = forkedFromNode.InnermostInterruptibleNode.FindTriggeredParent(this);

                        if (triggeredInterrupt != null)
                        {
                            // Interrupt this context
                            Stop();
                            return;
                        }
                    }

                    // Check node condition
                    if (node.Condition != null)
                    {
                        if (!node.Condition.EvaluateBool(this))
                        {
                            // Condition failed, skip this node

                            if (node is ConditionNode conditionNode && conditionNode.ElseCondition != null)
                            {
                                SwitchNode(conditionNode.ElseCondition, true);
                            }
                            else
                                SwitchNode(node.FindNextSibling(), true);
                            return;
                        }
                        else
                        {
                            node.Condition.OnVisited(this);
                        }
                    }

                    if (node.PreCheck != null)
                    {
                        reenterNode = node;
                        Status = DialogueStatus.Challenging;
                        Machine.onChallengeStart(this, node, node.PreCheck);
                        return;
                    }
                }

                try
                {
                    switch (node)
                    {
                        case CaptionNode actionNode:
                            Machine.onCaptionStart?.Invoke(this, actionNode);
                            break;
                        case SpeechNode talkNode:
                            Machine.onSpeechStart?.Invoke(this, talkNode);
                            break;
                        case IChoosableNode choiceNode:
                            Machine.onChoiceStart?.Invoke(this, choiceNode);
                            break;
                        case WaitTimeNode waitNode:
                            Machine.onWaitTimeStart?.Invoke(this, waitNode);
                            break;
                        case CheckNode checkNode:
                            Status = DialogueStatus.Challenging;
                            Machine.onChallengeStart?.Invoke(this, checkNode, checkNode.Name);
                            break;
                        case GotoNode gotoNode:
                            // Check if changed dialogue
                            var nextNode = gotoNode.Next(Machine.Dialogues, this);

                            var prevDialogue = gotoNode.GetDialogue();
                            var newDialogue = nextNode != null ? nextNode.GetDialogue() : null;

                            if (prevDialogue != newDialogue)
                                Machine.onDialogueChange?.Invoke(this, prevDialogue, newDialogue);
                                
                            SwitchNode(nextNode);
                            break;
                        case WaitExpressionNode waitExpressionNode:
                        {
                            if (waitExpressionNode.Expression.EvaluateBool(this))
                            {
                                waitExpressionNode.Expression.OnVisited(this);
                                SwitchNode(Current.FindNextSibling());
                                break;
                            }
                            else
                            {
                                Status = DialogueStatus.Waiting;
                                Machine.OnLocked(this);
                                break;
                            }
                        }
                        case ForkNode forkNode:
                        {
                            var targetDialogue = Machine.Resolve(forkNode.DestinationDialogueId);

                            if (targetDialogue == null)
                                throw new DialogueNotFoundException(this, forkNode);

                            var targetNode = FindLabel(targetDialogue, forkNode.DestinationLabel);

                            if (targetNode == null)
                                throw new DialogueNodeNotFoundException(this, forkNode.DestinationDialogueId, forkNode.DestinationLabel);

                            OnFork(forkNode, forkNode.BranchName, targetNode);
                            SwitchNode(forkNode.Next(Machine.Dialogues, this));
                            break;
                        }
                        case LocalForkNode forkNode:
                            OnFork(forkNode, forkNode.BranchName, forkNode.Destination);
                            SwitchNode(forkNode.Next(Machine.Dialogues, this));
                            break;
                        case ExternalForkNode forkNode:
                            OnFork(forkNode, forkNode.BranchName, forkNode.Code);
                            SwitchNode(forkNode.Next(Machine.Dialogues, this));
                            break;
                        case AnonymousForkNode forkNode:
                            OnFork(forkNode, forkNode.BranchName, forkNode.AnonymousBlock.GetChild(0));
                            SwitchNode(forkNode.Next(Machine.Dialogues, this));
                            break;
                        case AwaitNode awaitNode:
                        {
                            var targetDialogue = Machine.Resolve(awaitNode.DestinationDialogueId);

                            if (targetDialogue == null)
                                throw new DialogueNotFoundException(this, awaitNode);

                            var targetNode = FindLabel(targetDialogue, awaitNode.DestinationLabel);

                            if (targetNode == null)
                                throw new DialogueNodeNotFoundException(this, awaitNode.DestinationDialogueId, awaitNode.DestinationLabel);

                            OnAwait(awaitNode, targetNode, awaitNode.Next(Machine.Dialogues, this));
                            break;
                        }
                        case LocalAwaitNode awaitNode:
                            OnAwait(awaitNode, awaitNode.Destination, awaitNode.Next(Machine.Dialogues, this));
                            break;
                        case ExternalAwaitNode awaitNode:
                            OnAwait(awaitNode, awaitNode.Code, awaitNode.Next(Machine.Dialogues, this));
                            break;
                        case JoinNode joinNode:
                            OnJoin(joinNode.BranchName, joinNode.Next(Machine.Dialogues, this));
                            break;
                        case CancelNode cancelNode:
                            OnCancel(cancelNode.BranchName, cancelNode.Next(Machine.Dialogues, this));
                            break;
                        case IAutoNode autoNode:
                            SwitchNode(autoNode.Next(Machine.Dialogues, this));
                            break;
                    }
                }
                catch (DialogueNotFoundException e)
                {
                    Status = DialogueStatus.Resolving;
                    reenterNode = e.CausingNode;
                    Machine.onWaitForMissingDialogueStart?.Invoke(this, e.CausingNode.DestinationDialogueId, e.CausingNode);
                }
                
            }

            void OnLeaveNode(IDialogueNode node)
            {
                switch (node)
                {
                    case CaptionNode actionNode:
                        Machine.onCaptionEnd?.Invoke(this, actionNode);
                        break;
                    case SpeechNode talkNode:
                        Machine.onSpeechEnd?.Invoke(this, talkNode);
                        break;
                    case IChoosableNode choiceNode:
                        Machine.onChoiceEnd?.Invoke(this, choiceNode);
                        break;
                    case WaitTimeNode waitNode:
                        Machine.onWaitTimeEnd?.Invoke(this, waitNode);
                        break;
                }
            }
            
            void IExternalCodeContext.OnEnd()
            {
                OnEnd();
            }
            
            void OnEnd()
            {
                parent?.OnChildEnded(this);
                Clear();
            }

            void OnChildEnded(DialogueContext child)
            {
                bool currentWasNull = Current == null;
                children.Remove(child);

                if (!string.IsNullOrEmpty(child.BranchName))
                    branchesMap.Remove(child.BranchName);

                if (waitingForJoin == child)
                {
                    waitingForJoin = null;
                    var node = reenterNode;
                    reenterNode = null;
                    Status = DialogueStatus.Running;
                    SwitchNode(node);
                }
                else if (waitingForJoin == this && children.Count == 0)
                {
                    // all children ended
                    waitingForJoin = null;
                    var node = reenterNode;
                    reenterNode = null;
                    Status = DialogueStatus.Running;
                    SwitchNode(node);
                }

                if (currentWasNull && children.Count == 0)
                {
                    OnEnd();
                    Machine.OnDialogueContextEnd(this);
                }
            }

            void OnChildStopped(DialogueContext child)
            {
                if (Status != DialogueStatus.Stopping)
                    OnChildEnded(child);
            }

            void OnFork(IDialogueNode forkNode, string branchName, IDialogueNode destinationNode)
            {
                if (destinationNode == null)
                    throw new System.ArgumentException("Cannot fork dialogue using an invalid destination node");

                var newContext = Machine.CreateContext();

                newContext.Start(branchName, destinationNode, this);
                newContext.forkedFromNode = forkNode;
            }

            void OnFork(IDialogueNode forkNode, string branchName, IAsyncCode code)
            {
                if (code == null)
                    throw new System.ArgumentException("Cannot fork dialogue using an invalid code node");

                var newContext = Machine.CreateContext();

                newContext.forkedFromNode = forkNode;
                newContext.Start(branchName, code, this);
            }

            void OnJoin(string branchName, IDialogueNode nextNode)
            {
                if (string.IsNullOrEmpty(branchName))
                {
                    if (children.Count == 0)
                    {
                        SwitchNode(nextNode);
                        return;
                    }

                    // wait for all children
                    Status = DialogueStatus.Waiting;
                    waitingForJoin = this;
                    reenterNode = nextNode;

                    return;
                }

                if (!branchesMap.TryGetValue(branchName, out var child))
                    return;

                Status = DialogueStatus.Waiting;
                waitingForJoin = child;
                reenterNode = nextNode;
            }

            void OnCancel(string branchName, IDialogueNode nextNode)
            {
                OnJoin(branchName, nextNode);
                
                if (string.IsNullOrEmpty(branchName))
                {
                    for (int i=children.Count-1; i>=0; --i)
                    {
                        children[i].Stop();
                    }
                }
                else
                {
                    if (branchesMap.TryGetValue(branchName, out var child))
                    {
                        child.Stop();
                    }
                }
            }

            void OnAwait(IDialogueNode awaitNode, IDialogueNode destinationNode, IDialogueNode nextNode)
            {
                if (destinationNode == null)
                    throw new System.ArgumentException("Cannot await dialogue using an invalid destination node");

                var newContext = Machine.CreateContext();

                Status = DialogueStatus.Waiting;
                newContext.Start(null, destinationNode, this);
                newContext.forkedFromNode = awaitNode;
                waitingForJoin = newContext;
                reenterNode = nextNode;
            }

            void OnAwait(IDialogueNode awaitNode, IAsyncCode code, IDialogueNode nextNode)
            {
                // nextNode is null when this is the last line in the dialogue

                var newContext = Machine.CreateContext();

                Status = DialogueStatus.Waiting;
                newContext.forkedFromNode = awaitNode;
                waitingForJoin = newContext;
                reenterNode = nextNode;
                newContext.Start(null, code, this);
            }

            IDialogueNode FindLabel(Dialogue dialogue, string label)
            {
                if (!string.IsNullOrEmpty(label))
                    return dialogue.GetNodeFromLabel(label);

                if (dialogue.ChildrenCount == 0)
                    return null;

                return dialogue.GetChild(0);
            }

            // Data state
            IDataContext localDataContext;

            // General state
            IDialogueNode reenterNode; // next node after join/awake, choice spoken line (normal or challenge), unresolved and challenged nodes
            DialogueContext waitingForJoin;
            IDialogueNode forkedFromNode;  // which fork/await node created this context
            IOption waitingOption; // waiting on this option (e.g. for a challenge or because it's showing a speech option)

            //// No need to serialize
            DialogueContext parent;
            List<DialogueContext> children = new List<DialogueContext>();
            Dictionary<string, DialogueContext> branchesMap = new Dictionary<string, DialogueContext>();

            int depth;
        }
    }
}