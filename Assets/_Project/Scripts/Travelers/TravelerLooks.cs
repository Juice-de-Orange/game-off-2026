using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// P108: how each traveler looks. A name always gets the same look: one of six skins per species and, for humans, a hairstyle or hat.
    /// The passport photos are rendered from exactly these combinations (GameOff/Build Traveler Photos), so photo and person always match.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Traveler Looks", fileName = "TravelerLooks")]
    public class TravelerLooks : ScriptableObject
    {
        [Serializable]
        public class SpeciesLooks
        {
            public string species;
            [Tooltip("Six textures: the original first, then the delivered variants.")]
            public Texture2D[] skins;
        }

        public readonly struct Look
        {
            public readonly int Skin, Hair;
            public Look(int skin, int hair) { Skin = skin; Hair = hair; }
        }

        private static readonly Color[] HairColors =
        {
            new Color(0.12f, 0.1f, 0.09f), new Color(0.45f, 0.28f, 0.14f), new Color(0.72f, 0.58f, 0.32f),
            new Color(0.55f, 0.16f, 0.1f), new Color(0.72f, 0.72f, 0.74f), new Color(0.2f, 0.3f, 0.4f),
        };
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private const string HairObject = "TravelerHair";

        [SerializeField] private List<SpeciesLooks> species = new List<SpeciesLooks>();
        [Tooltip("Humans only: hair and hats, set on the head bone. Hair value 0 = none, 1.. = entry 0..")]
        [SerializeField] private GameObject[] hairs = new GameObject[0];
        [Tooltip("Height of each hair on the head bone (m): the models have their pivots in different places.")]
        [SerializeField] private float[] hairHeights = new float[0];
        [SerializeField] private List<Texture2D> photos = new List<Texture2D>();

        public int HairCount => hairs.Length;
        public IReadOnlyList<SpeciesLooks> Species => species;
        public static string PhotoName(string speciesName, Look look) => $"T_Doc_Photo_{speciesName}_{look.Skin}_{look.Hair}";

        /// <summary>The same name always gives the same look, in every run and on every platform.</summary>
        public static Look For(string name, string speciesName, int skins, int hairCount)
        {
            uint hash = 2166136261;
            foreach (char c in name ?? "")
            {
                hash ^= c;
                hash *= 16777619;
            }
            bool human = string.Equals(speciesName, "Human", StringComparison.OrdinalIgnoreCase);
            return new Look(skins > 0 ? (int)(hash % (uint)skins) : 0, human && hairCount > 0 ? (int)((hash >> 8) % (uint)(hairCount + 1)) : 0);
        }

        public Look LookOf(string name, string speciesName)
        {
            var entry = species.Find(s => string.Equals(s.species, speciesName, StringComparison.OrdinalIgnoreCase));
            return For(name, speciesName, entry != null && entry.skins != null ? entry.skins.Length : 0, hairs.Length);
        }

        /// <summary>The passport photo of this person; null if it was not rendered.</summary>
        public Texture2D PhotoFor(string name, string speciesName)
        {
            string wanted = PhotoName(speciesName, LookOf(name, speciesName));
            foreach (var photo in photos)
                if (photo != null && photo.name == wanted) return photo;
            return null;
        }

        public void Apply(GameObject figure, string name, string speciesName) => Apply(figure, speciesName, LookOf(name, speciesName));

        /// <summary>Dresses a figure of the species. Reused figures can be dressed again: the old hair goes.</summary>
        public void Apply(GameObject figure, string speciesName, Look look)
        {
            var entry = species.Find(s => string.Equals(s.species, speciesName, StringComparison.OrdinalIgnoreCase));
            if (entry != null && entry.skins != null && look.Skin < entry.skins.Length && entry.skins[look.Skin] != null)
            {
                var block = new MaterialPropertyBlock();
                foreach (var r in figure.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    r.GetPropertyBlock(block);
                    block.SetTexture(BaseMap, entry.skins[look.Skin]);
                    r.SetPropertyBlock(block);
                }
            }
            var animator = figure.GetComponent<Animator>();
            var head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            if (head == null) return;
            var old = head.Find(HairObject);
            if (old != null)
            {
                if (Application.isPlaying) Destroy(old.gameObject);
                else DestroyImmediate(old.gameObject);
            }
            if (look.Hair <= 0 || look.Hair > hairs.Length || hairs[look.Hair - 1] == null) return;
            var hair = Instantiate(hairs[look.Hair - 1], head, false);
            hair.name = HairObject;
            if (look.Hair - 1 < hairHeights.Length) hair.transform.localPosition = Vector3.up * hairHeights[look.Hair - 1];
            var hairBlock = new MaterialPropertyBlock();
            foreach (var r in hair.GetComponentsInChildren<Renderer>(true))
            {
                r.GetPropertyBlock(hairBlock);
                hairBlock.SetColor(BaseColor, HairColors[look.Skin % HairColors.Length]);
                r.SetPropertyBlock(hairBlock);
            }
        }
    }
}
