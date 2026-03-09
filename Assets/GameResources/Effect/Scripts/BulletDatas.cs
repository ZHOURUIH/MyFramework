using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MasterStylizedProjectile
{
    [CreateAssetMenu(menuName ="MasterStylizedProjectile/BulletDatas")]
    public class BulletDatas : ScriptableObject
    {
        public List<EffectsGroup> Effects = new List<EffectsGroup>();
    }
}
