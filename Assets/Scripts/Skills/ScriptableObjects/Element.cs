using UnityEngine;

namespace Skills
{
    [CreateAssetMenu(fileName = "newElementData", menuName = "Data/Element Data/Element")]
    public class Element : ScriptableObject
    {
        public Sprite icon;
        public Effectiveness[] effectivenesses;
    }
}




   
