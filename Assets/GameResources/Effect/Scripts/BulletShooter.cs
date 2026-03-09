using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MasterStylizedProjectile
{
    [System.Serializable]
    public class EffectsGroup
    {
        public string EffectName;
        public float Speed = 20;
        public ParticleSystem ChargeParticles;
        public float ChargeParticleTime;
        public AudioClip ChargeClip;
        public ParticleSystem StartParticles;
        public ParticleSystem BulletParticles;
        public ParticleSystem HitParticles;
        public AudioClip startClip;
        public AudioClip bulletClip;
        public AudioClip hitClip;
        public bool isTargeting;
        public float RotSpeed;
    }
    public class BulletShooter : MonoBehaviour
    {
        //public List<EffectsGroup> Effects = new List<EffectsGroup>();

        public BulletDatas datas;
        public int Index = 0;
        public EffectsGroup CurEffect => datas.Effects[Index];
        public Transform StartNodeTrans;

        public float Speed;
        public float ShootInterval = 0.2f;
        float LastShootTime = 0;
         // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            if(Input.GetMouseButton(0))
            {
                if (Time.time - LastShootTime > ShootInterval)
                {
                    Shoot();
                }
            }
        }
        public void Shoot()
        {
            StartCoroutine(ShootIE());
        }
        public IEnumerator ShootIE()
        {
            LastShootTime = Time.time;
            yield return Charge();
            DoShoot();
        }
        public IEnumerator Charge()
        {
            if (CurEffect.ChargeParticles != null)
            {
                var ChargePar = Instantiate(CurEffect.ChargeParticles, StartNodeTrans.position, Quaternion.identity);
                //var onStart = gameObject.AddComponent<AudioTrigger>();
                //if (CurEffect.ChargeClip != null)
                //{
                //    onStart.onClip = CurEffect.startClip;
                //}

            
                if (CurEffect.ChargeClip != null)
                {
                    GameObject AudioObj = new GameObject();
                    var audiosource = AudioObj.AddComponent<AudioSource>();
                    audiosource.clip = CurEffect.ChargeClip;
                    audiosource.Play();
                }
                yield return new WaitForSeconds(CurEffect.ChargeParticleTime);
                Destroy(ChargePar.gameObject);
            }
           
        }
        public void DoShoot()
        {
            var targetPos = GetMouseTargetPos();
            var targetDir = targetPos - StartNodeTrans.position;
            targetDir = targetDir.normalized;
            if (CurEffect.StartParticles != null)
            {
                var StartPar = Instantiate(CurEffect.StartParticles, StartNodeTrans.position, Quaternion.identity);
                StartPar.transform.forward = targetDir;

                var onStart = StartPar.gameObject.AddComponent<AudioTrigger>();
                if (CurEffect.startClip != null)
                {
                    onStart.onClip = CurEffect.startClip;
                }

            }
            if (CurEffect.BulletParticles != null)
            {
                var bulletObj = Instantiate(CurEffect.BulletParticles, StartNodeTrans.position, Quaternion.identity);
                bulletObj.transform.forward = targetDir;

                var bullet = bulletObj.gameObject.AddComponent<Bullet>();
                bullet.OnHitEffect = CurEffect.HitParticles;
                bullet.Speed = CurEffect.Speed;
                bullet.isTargeting = CurEffect.isTargeting;
                if (CurEffect.isTargeting)
                {
                    var target = FindNearestTarget("Respawn");
                    if (target != null)
                    {
                        bullet.rotSpeed = CurEffect.RotSpeed;
                        bullet.target = target.transform;
                    }
                }

                   
                if (CurEffect.hitClip != null)
                {
                    bullet.onHitClip = CurEffect.hitClip;
                }
                if (CurEffect.bulletClip != null)
                {
                    bullet.bulletClip = CurEffect.bulletClip;
                }


                var collider = bulletObj.gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 0.6f;
            }
      
        }


        public Vector3 GetMouseTargetPos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray,out hit,100))
            {
                return hit.point;
            }
            return Vector3.zero;
        }

        public GameObject FindNearestTarget(string tag)
        {
            var gameObjects = GameObject.FindGameObjectsWithTag(tag).ToList().OrderBy(
                (x) => Vector3.Distance(transform.position, x.transform.position));
            return gameObjects.FirstOrDefault();
        }
    }

}
