using System;
using System.Collections.Generic;

namespace CozyAR.Data
{
    [Serializable]
    public class CharacterSummary
    {
        public int id;
        public string name;
        public string thumbnail_url;
    }

    [Serializable]
    public class CharacterExpressions
    {
        public string idle;
        public string happy;
        public string sad;
        public string wave;
    }

    [Serializable]
    public class CharacterConfig
    {
        public float idleBobAmplitude;
        public float idleBobSpeed;
        public float breathingScale;
        public float blinkIntervalMin;
        public float blinkIntervalMax;
        public float headRotationRange;
        public float hairSpring;
        public float hairDamping;
    }

    [Serializable]
    public class CharacterLayers
    {
        public string body;
        public string head;
        public string left_arm;
        public string right_arm;
        public string hair;
        public string eyes_open;
        public string eyes_closed;
        public string mouth;
        public string mouth_happy;
        public string mouth_sad;
        public string shadow;
        public string accessory; // Optional
    }

    [Serializable]
    public class CharacterMetadata
    {
        public string description;
        public CharacterExpressions expressions; // Kept for backward compatibility
        public CharacterConfig config;
        public CharacterLayers layers;
    }

    [Serializable]
    public class CharacterDetails
    {
        public int id;
        public string name;
        public string thumbnail_url;
        public string image_url;
        public CharacterMetadata metadata;
    }
}
