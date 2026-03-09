using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MasterStylizedProjectile
{
    public class Bullet:MonoBehaviour
    {
        public float Speed = 5;
        public ParticleSystem OnHitEffect;
        public AudioClip bulletClip;
        public AudioClip onHitClip;

        public bool isTargeting;
        public Transform target;
        public float rotSpeed = 0;
        private void Start()
        {
            if (bulletClip != null)
            {
                var audio = gameObject.AddComponent<AudioSource>();
                audio.clip = bulletClip;
                audio.Play();
            }
        }
        private void Update()
        {
            Vector3 forward = Vector3.forward;
            if (isTargeting == true && target != null)
            {
                transform.forward = Vector3.RotateTowards(transform.forward, target.position - transform.position, rotSpeed * Time.deltaTime, 0.0f);
            }
            transform.Translate(forward * Speed * Time.deltaTime, Space.Self);
        }
        private void OnTriggerEnter(Collider other)
        {

            if (OnHitEffect != null)
            {
                var onHitObj = Instantiate(OnHitEffect, transform.position, Quaternion.identity);
                var onHit = onHitObj.gameObject.AddComponent<AudioTrigger>();
                if (onHitClip != null)
                {
                    onHit.onClip = onHitClip;
                }
                
            }
            Destroy(gameObject);
        }

    }
}
