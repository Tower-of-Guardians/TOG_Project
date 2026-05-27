namespace JxDialogueBox
{
    public enum Speaker
    {
        Player,
        Npc,
    }

    public readonly struct SpeakerRef
    {
        public readonly Speaker Speaker;
        public readonly string CharacterID;

        public SpeakerRef(Speaker speaker, string characterID)
        {
            Speaker = speaker;
            CharacterID = characterID;
        }

        public static SpeakerRef Player(string playerID = "Player")
            => new(Speaker.Player, playerID);

        public static SpeakerRef Npc(string npcID)
            => new(Speaker.Npc, npcID);
    }
}