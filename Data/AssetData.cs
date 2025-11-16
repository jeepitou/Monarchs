using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
    //Default fx and audio, some can be overrided on each individual card

    [CreateAssetMenu(fileName = "AssetData", menuName = "TcgEngine/AssetData", order = 0)]
    public class AssetData : ScriptableObject
    {
        [Header("FX")]
        public GameObject card_spawn_fx;
        public GameObject card_destroy_fx;
        public GameObject card_attack_fx;
        public GameObject card_damage_fx;
        public GameObject card_exhausted_fx;
        public GameObject player_damage_fx;
        public GameObject damage_fx;
        public GameObject play_card_fx;
        public GameObject play_card_other_fx;
        public GameObject play_secret_fx;
        public GameObject play_secret_other_fx;
        public GameObject dice_roll_fx;
        public GameObject hover_text_box;
        public GameObject new_turn_fx;
        public GameObject win_fx;
        public GameObject lose_fx;
        public GameObject tied_fx;
        public GameObject trap_triggered_fx;

        [Header("Audio")]
        public AudioClip card_spawn_audio;
        public AudioClip card_destroy_audio;
        public AudioClip card_attack_audio;
        public AudioClip card_move_audio;
        public AudioClip card_damage_audio;
        public AudioClip player_damage_audio;
        public AudioClip hand_card_click_audio;
        public AudioClip new_turn_audio;
        public AudioClip win_audio;
        public AudioClip defeat_audio;
        public AudioClip win_music;
        public AudioClip defeat_music;
        public AudioClip trap_played_audio;
        public AudioClip card_about_to_be_played_audio;

        public static AssetData Get()
        {
            return DataLoader.Get().assets;
        }
    }
}
