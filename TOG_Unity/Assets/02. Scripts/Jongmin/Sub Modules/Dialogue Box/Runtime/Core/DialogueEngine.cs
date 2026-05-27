using System;

namespace JxDialogueBox
{
    public sealed class DialogueEngine
    {
        public enum EngineState
        {
            Idle,
            ShowingLine,
            AwaitingChoice,
            Ended,
        }

        public readonly struct LineEvent
        {
            public readonly SpeakerRef Speaker;
            public readonly string Text;
            public readonly string PortraitKey;
            public readonly string NodeID;

            public LineEvent(SpeakerRef speaker, 
                             string text, 
                             string portraitKey, 
                             string nodeID)
            {
                Speaker = speaker;
                Text = text ?? string.Empty;
                PortraitKey = portraitKey ?? string.Empty;
                NodeID = nodeID;
            }
        }

        public readonly struct ChoiceEvent
        {
            public readonly string Prompt;
            public readonly ChoiceOption[] Options;
            public readonly string NodeID;

            public ChoiceEvent(string prompt, 
                               ChoiceOption[] options, 
                               string nodeID)
            {
                Prompt = prompt ?? string.Empty;
                Options = options ?? Array.Empty<ChoiceOption>();
                NodeID = nodeID;
            }
        }

        public event Action<LineEvent> OnLine;
        public event Action<ChoiceEvent> OnChoice;
        public event Action OnEnded;

        public EngineState State { get; private set; } = EngineState.Idle;
        public string CurrentNodeID { get; private set; } = string.Empty;

        private readonly IDialogueDataSource _source;

        public DialogueEngine(IDialogueDataSource source)
        {
            _source = source;
        }

        public void Start(string dialogueID)
        {
            var entryID = _source.GetEntryNodeID(dialogueID);
            if(string.IsNullOrEmpty(entryID))
            {
                EndInternal();
                return;
            } 

            State = EngineState.Idle;
            Goto(entryID);
        }

        public void Advance()
        {
            if(State == EngineState.ShowingLine)
            {
                if(_source.TryGetNode(CurrentNodeID, out var node) && node is LineNode line)
                {
                    if(string.IsNullOrEmpty(line.NextID))
                    {
                        EndInternal();
                        return;
                    }

                    Goto(line.NextID);
                }

                return;
            }

            if(State == EngineState.AwaitingChoice)
                return;
        }

        public void Choose(int optionIndex)
        {
            if (State != EngineState.AwaitingChoice)
            {
                return;
            }

            if (!_source.TryGetNode(CurrentNodeID, out var node) || node is not ChoiceNode choice)
            {
                return;
            }

            if (choice.Options == null || optionIndex < 0 || optionIndex >= choice.Options.Count)
            {
                return;
            }

            var nextID = choice.Options[optionIndex].NextID;
            if(string.IsNullOrEmpty(nextID))
            {
                EndInternal();
                return;
            }

            Goto(nextID);
        }

        private void Goto(string nodeID)
        {
            while(true)
            {
                CurrentNodeID = nodeID;

                if(!_source.TryGetNode(CurrentNodeID, out var node))
                {
                    EndInternal();
                    return;
                }

                switch(node.Type)
                {
                    case NodeType.Line:
                    {
                        var lineNode = node as LineNode;
                        State = EngineState.ShowingLine;
                        OnLine?.Invoke(new LineEvent(lineNode.Speaker, lineNode.Text, lineNode.PortraitKey, lineNode.ID));
                        return;
                    }

                    case NodeType.Choice:
                    {
                        var choiceNode = node as ChoiceNode;
                        State = EngineState.AwaitingChoice;

                        var options = new ChoiceOption[choiceNode.Options.Count];
                        for (var i = 0; i < options.Length; i++)
                        {
                            options[i] = choiceNode.Options[i];
                        }

                        OnChoice?.Invoke(new ChoiceEvent(choiceNode.Prompt, options, choiceNode.ID));
                        return;
                    }

                    case NodeType.Jump:
                    {
                        var jumpNode = node as JumpNode;
                        if(string.IsNullOrEmpty(jumpNode.TargetID))
                        {
                            EndInternal();
                            return;
                        }

                        nodeID = jumpNode.TargetID;
                        break;
                    }

                    case NodeType.End:
                    default:
                        EndInternal();
                        return;
                }
            }
        }

        private void EndInternal()
        {
            State = EngineState.Ended;
            CurrentNodeID = string.Empty;
            OnEnded?.Invoke();
        }
    }
}