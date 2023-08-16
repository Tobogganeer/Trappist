using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public static Footsteps instance;
    private void Awake()
    {
        instance = this;
    }

    public Transform footSource;
    public HeadbobModule headbobModule;

    public static Foot foot;
    private const float MIN_AIRTIME = 0.2f;

    private void OnEnable()
    {
        PlayerMovement.OnLand += PlayerMovement_OnLand;
        PlayerMovement.OnJump += PlayerMovement_OnJump;
    }

    private void OnDisable()
    {
        PlayerMovement.OnLand -= PlayerMovement_OnLand;
        PlayerMovement.OnJump -= PlayerMovement_OnJump;
    }

    private void Footstep(Foot foot)//, float magnitude)
    {
        //AudioManager.Play(Sound.From(GetSound(foot)), footSource.position);
        GetSound(foot).Play(footSource.position);
    }

    private void PlayerMovement_OnLand(float airtime)
    {
        if (airtime > MIN_AIRTIME)
        { 
            AudioManager.PlayAudio(Sound.ID.Drop.Override().SetPosition(footSource.position).SetVolume(Mathf.Clamp01(airtime * 0.6f)));
            //if (dropParticles)
            //    Instantiate(dropParticles, footSource.position, Quaternion.identity);
        }
            //AudioManager.Play(AudioArray.Drop, footSource.position, null, 35, AudioCategory.SFX, Mathf.Clamp01(airtime * 0.6f));
    }

    private void PlayerMovement_OnJump(float chargeTime01)
    {
        //AudioManager.Play(Sound.ID.Jump, transform.position); // Could change volume with factor
        Sound.ID.Jump.Play(transform.position);
    }

    private Sound.ID GetSound(Foot foot)
    {
        return foot == Foot.Right ? Sound.ID.RightFootstep : Sound.ID.LeftFootstep;
    }


    private void Update()
    {
        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        if (headbobModule.TimeValue == 0)// || PlayerMovement.Sliding)
        {
            foot = Foot.Right;
        }

        if (headbobModule.SinValue > 0.5f && foot == Foot.Right && PlayerMovement.Grounded)
        {
            Footstep(Foot.Right);//, magnitude);
            foot = Foot.Left;
        }
        else if (headbobModule.SinValue < -0.5f && foot == Foot.Left && PlayerMovement.Grounded)
        {
            Footstep(Foot.Left);//, magnitude);
            foot = Foot.Right;
        }
    }
}
