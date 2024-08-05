using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bozo.ModularCharacters
{
    public enum OutfitType { Face, Head, HairFront, HairBack, Top, Bottom, Feet, LowerFace }
    public class OutfitSystem : MonoBehaviour
    {
        SkinnedMeshRenderer CharacterBody;
        private Material CharacterMaterial;
        private Bounds CharacterRenderBounds;
        public Dictionary<OutfitType, Transform> Outfits = new Dictionary<OutfitType, Transform>();

        [Header("BodyShapes")]
        [Range(0, 100)] public float Gender;
        [Range(0, 100)] public float ChestSize;
        [Range(0, 100)] public float FaceShape;

        [Header("FaceShapes")]
        [Range(0, 100)] public float LashLength;
        [Range(0, 100)] public float BrowSize;
        [Range(0, 100)] public float EarTipLength;
        [Space]
        [Range(0, 100)] public float EyeUp;
        [Range(0, 100)] public float EyeDown;
        [Range(0, 100)] public float EyeSquare;
        [Space]
        [Range(0, 100)] public float NoseWidth;
        [Range(0, 100)] public float NoseUp;
        [Range(0, 100)] public float NoseDown;
        [Range(0, 100)] public float NoseBridgeAngle;
        [Space]
        [Range(0, 100)] public float MouthWide;
        [Range(0, 100)] public float MouthThin;

        private void OnValidate()
        {
            SetGender(Gender);
            SetChest(ChestSize);
            SetFace(FaceShape);
            SetLashLength(LashLength);
        }

        private void Awake()
        {
            CharacterBody = GetComponentInChildren<SkinnedMeshRenderer>();
            CharacterRenderBounds = CharacterBody.localBounds;
            CharacterMaterial = CharacterBody.sharedMaterials[0];
            SetGender(Gender);
            SetChest(ChestSize);
            SetFace(FaceShape);
            SetLashLength(LashLength);
            print(CharacterBody.sharedMesh.blendShapeCount);
        }

        private void SetBlendShapes()
        {
            SetGender(Gender);
            SetChest(ChestSize);
            SetFace(FaceShape);
            SetLashLength(LashLength);

        }

        public void AttachSkinnedOutfit(Outfit outfit)
        {

            //check if an outfit is already in that slot and replace it
            if (Outfits.TryGetValue(outfit.Type, out Transform currentOutfitInSlot))
            {
                if (Outfits[outfit.Type]) Destroy(currentOutfitInSlot.gameObject);
                Outfits[outfit.Type] = outfit.transform;
            }
            else
            {
                Outfits.Add(outfit.Type, outfit.transform);
            }


            //Assigning the skin material from the base character
            var renderer = outfit.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer)
            {
                var materialsSort = renderer.material.name.Split("_");
                if (materialsSort[1] == "Skin")
                {
                    renderer.material = CharacterMaterial;
                }
               
            }

            //Merging outfit bones or attaching outfit to specified bone
            if (outfit.AttachPoint == "" && renderer)
            {
                var bones = renderer.bones;
                renderer.bones = CharacterBody.bones;
                renderer.rootBone = CharacterBody.rootBone;
                foreach (var bone in bones) { Destroy(bone.gameObject); }
            }
            else
            {
                Transform bone = null;
                foreach (var item in CharacterBody.bones)
                {
                    if (item.name == outfit.AttachPoint)
                    {
                        bone = item;
                        break;
                    }
                }
                outfit.transform.parent = bone.transform;
                outfit.transform.position = bone.position;
                outfit.transform.rotation = bone.rotation;
            }

            //Adjusting Mesh bounds so the meshes don't unexpectingly disappear.
            if (renderer)
            {
                Bounds outfitBounds = renderer.localBounds;
                CharacterRenderBounds.Encapsulate(outfitBounds);
                UpdateCharacterBounds();
            }

            SetGender(Gender);
            SetChest(ChestSize);

        }

        public void UpdateCharacterBounds()
        {
            CharacterBody.localBounds = CharacterRenderBounds;
            foreach (var item in Outfits.Values)
            {
                var renderer = item.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null) renderer.localBounds = CharacterRenderBounds;
            }
        }

        public void SetGender(float value)
        {
            Gender = value;
            if (!CharacterBody) return;

            CharacterBody.SetBlendShapeWeight(0, Gender);
            foreach (var item in Outfits.Values)
            {
                var outfit = item.GetComponentInChildren<SkinnedMeshRenderer>();
                if (outfit != null)
                {
                    if (outfit.sharedMesh.blendShapeCount >= 1)
                    {
                        outfit.SetBlendShapeWeight(0, Gender);
                    }
                }
            }
        }

        public void SetChest(float value)
        {
            ChestSize = value;
            if (!CharacterBody) return;

            CharacterBody.SetBlendShapeWeight(0, Gender);
            foreach (var item in Outfits.Values)
            {
                var outfit = item.GetComponentInChildren<SkinnedMeshRenderer>();
                if (outfit != null)
                {
                    if (outfit.sharedMesh.blendShapeCount >= 2)
                    {
                        outfit.SetBlendShapeWeight(1, ChestSize);
                    }
                }
            }
        }

        public void SetSkin(Material value)
        {
            if (!CharacterBody) return;
            CharacterBody.material = value;
            CharacterMaterial = value;
            foreach (var item in Outfits.Values)
            {
                var renderer = item.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer)
                {
                    var materialsSort = renderer.material.name.Split("_");
                    if (materialsSort[1] == "Skin")
                    {
                        renderer.material = CharacterMaterial;
                    }
                   
                }
            }
        }

        public void SetEyes(Material value)
        {
            if (!CharacterBody) return;
            var mats = CharacterBody.sharedMaterials;
            mats[1] = value;
            CharacterBody.sharedMaterials = mats;
        }

        #region Blendshapes
        public void SetFace(float value)
        {
            if (!CharacterBody) return;
            FaceShape = value;
            CharacterBody.SetBlendShapeWeight(1, value);
        }

        public void SetLashLength(float value)
        {
            if (!CharacterBody) return;
            LashLength = value;
            CharacterBody.SetBlendShapeWeight(2, value);
        }

        public void SetBrowThickness(float value)
        {
            if (!CharacterBody) return;
            LashLength = value;
            CharacterBody.SetBlendShapeWeight(36, value);
        }

        public void SetEarElf(float value)
        {
            if (!CharacterBody) return;
            EarTipLength = value;
            CharacterBody.SetBlendShapeWeight(46, value);
        }

        #region Eyes
        public void SetEyeDown(float value)
        {
            if (!CharacterBody) return;
            EyeDown = value;
            CharacterBody.SetBlendShapeWeight(43, value);
        }

        public void SetEyeUp(float value)
        {
            if (!CharacterBody) return;
            EyeUp = value;
            CharacterBody.SetBlendShapeWeight(44, value);
        }

        public void SetEyeSquare(float value)
        {
            if (!CharacterBody) return;
            EyeSquare = value;
            CharacterBody.SetBlendShapeWeight(45, value);
        }

        #endregion

        #region Nose
        public void SetNoseWidth(float value)
        {
            if (!CharacterBody) return;
            NoseWidth = value;
            CharacterBody.SetBlendShapeWeight(38, value);
        }

        public void SetNoseUp(float value)
        {
            if (!CharacterBody) return;
            NoseUp = value;
            CharacterBody.SetBlendShapeWeight(40, value);
        }

        public void SetNoseDown(float value)
        {
            if (!CharacterBody) return;
            NoseDown = value;
            CharacterBody.SetBlendShapeWeight(39, value);
        }

        public void SetNoseBridge(float value)
        {
            if (!CharacterBody) return;
            NoseBridgeAngle = value;
            CharacterBody.SetBlendShapeWeight(37, value);
        }

        #endregion

        #region Mouth
        public void SetMouthWide(float value)
        {
            if (!CharacterBody) return;
            MouthWide = value;
            CharacterBody.SetBlendShapeWeight(41, value);
        }

        public void SetMouthThin(float value)
        {
            if (!CharacterBody) return;
            MouthThin = value;
            CharacterBody.SetBlendShapeWeight(42, value);
        }

        #endregion

        public Transform GetOutfit(OutfitType outfitType)
        {
            if (Outfits.TryGetValue(outfitType, out Transform item))
            {
                return item;
            }

            return null;
        }

        #endregion

        public SkinnedMeshRenderer GetCharacterBody() { return CharacterBody; }


    }
}
