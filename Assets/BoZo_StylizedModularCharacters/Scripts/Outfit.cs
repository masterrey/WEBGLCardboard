using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bozo.ModularCharacters
{
    public class Outfit : MonoBehaviour
    {
        [field: SerializeField] public OutfitType Type { get; private set; }
        public string AttachPoint;
        private void Start()
        {
            var System = GetComponentInParent<OutfitSystem>();
            if (System == null) return;

            System.AttachSkinnedOutfit(this);
        }
    }

}
