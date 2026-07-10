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
    public class CharacterMetadata
    {
        public string description;
        public CharacterExpressions expressions;
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
