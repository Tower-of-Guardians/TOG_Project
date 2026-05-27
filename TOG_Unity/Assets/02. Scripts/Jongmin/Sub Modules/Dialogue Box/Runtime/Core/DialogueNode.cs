using System;
using System.Collections.Generic;

namespace JxDialogueBox
{
    public enum NodeType
    {
        Line,
        Choice,
        Jump,
        End
    }

    public abstract class DialogueNode
    {
        public string ID { get; }
        public NodeType Type { get; }

        protected DialogueNode(string id, NodeType type)
        {
            ID = id ?? throw new ArgumentException(nameof(id));
            Type = type;
        }
    }

    public sealed class LineNode : DialogueNode
    {
        public SpeakerRef Speaker { get; }
        public string Text { get; }
        public string PortraitKey { get; }
        public string NextID { get; }

        public LineNode(string id,
                        SpeakerRef speaker,
                        string text,
                        string portraitKey,
                        string nextID)
            : base(id, NodeType.Line)
        {
            Speaker = speaker;
            Text = text;
            PortraitKey = portraitKey;
            NextID = nextID;
        }
    }

    public readonly struct ChoiceOption
    {
        public readonly string Text;
        public readonly string NextID;

        public ChoiceOption(string text, string nextID)
        {
            Text = text ?? string.Empty;
            NextID = nextID ?? string.Empty;
        }
    }

    public sealed class ChoiceNode : DialogueNode
    {
        public string Prompt { get; }
        public IReadOnlyList<ChoiceOption> Options { get; }

        public ChoiceNode(string id,
                          string prompt,
                          IReadOnlyList<ChoiceOption> options)
            : base(id, NodeType.Choice)
        {
            Prompt = prompt ?? string.Empty;
            Options = options ?? Array.Empty<ChoiceOption>();
        }
    }

    public sealed class JumpNode : DialogueNode
    {
        public string TargetID { get; }

        public JumpNode(string id, string targetID)
            : base(id, NodeType.Jump)
        {
            TargetID = targetID ?? string.Empty;
        }
    }

    public sealed class EndNode : DialogueNode
    {
        public EndNode(string id)
            : base(id, NodeType.End)
        {}
    }
}