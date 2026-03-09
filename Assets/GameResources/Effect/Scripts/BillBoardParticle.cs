using UnityEngine;
using System.Collections;
namespace MasterStylizedProjectile
{
    [ExecuteInEditMode]
    public class BillBoardParticles : MonoBehaviour
    {

        public bool bTurnOver = false;

        void OnWillRenderObject()
        {
            //if in editor mode, transform to camera direction
            if (Application.isEditor && !Application.isPlaying)
            {
                if (Camera.current)
                {
                    if (bTurnOver)
                        transform.forward = Camera.current.transform.forward;
                    else
                        transform.forward = -Camera.current.transform.forward;
                }
            }
            //if in play mode, transform to maincamera direction
            else 
            {
                if (bTurnOver)
                    transform.forward = Camera.main.transform.forward;
                else
                    transform.forward = -Camera.main.transform.forward;
            }
        }
    }
}