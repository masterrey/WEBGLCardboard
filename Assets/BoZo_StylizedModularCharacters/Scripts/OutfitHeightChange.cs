using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bozo.ModularCharacters
{
    public class OutfitHeightChange : MonoBehaviour
    {
        [SerializeField] float HeightOffset;
        private void Start()
        {
            var System = GetComponentInParent<OutfitSystem>();
            if (System == null) return;

            System.transform.position = new Vector3 (System.transform.position.x, HeightOffset, System.transform.position.z);
        }
    }
}
