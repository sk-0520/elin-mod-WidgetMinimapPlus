using System.Collections.Generic;

namespace Elin.Plugin.Main.Models
{
    public class CustomHitCache
    {
        #region property

        private Dictionary<Chara, bool> Characters { get; } = new Dictionary<Chara, bool>();
        private Dictionary<Thing, bool> Things { get; } = new Dictionary<Thing, bool>();

        #endregion

        #region function

        public void Clear()
        {
            Characters.Clear();
            Things.Clear();
        }

        public void RegisterCharacter(Chara character, bool value)
        {
            Characters[character] = value;
        }

        public void RegisterThing(Thing thing, bool value)
        {
            Things[thing] = value;
        }

        public bool TryGetCharacter(Chara character, out bool value)
        {
            return Characters.TryGetValue(character, out value);
        }

        public bool TryGetThing(Thing thing, out bool value)
        {
            return Things.TryGetValue(thing, out value);
        }


        #endregion
    }
}
