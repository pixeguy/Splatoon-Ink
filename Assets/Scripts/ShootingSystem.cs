using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class ShootingSystem : MonoBehaviour
{
    private CameraViewer input;

    [SerializeField] private ParticleSystem inkParticle;
    [SerializeField] private Transform parentController;
    [SerializeField] private Transform splatGunNozzle;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Aim Pitch")]
    [SerializeField] private float visualPitchMultiplier = 0.35f;
    [SerializeField] private float visualPitchSmooth = 0.3f;
    [SerializeField] private float minVisualPitch = -25f;
    [SerializeField] private float maxVisualPitch = 25f;

    private void Start()
    {
        input = GetComponent<CameraViewer>();

        if (impulseSource == null)
            impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
    }

    private void Update()
    {
        bool pressing = Input.GetMouseButton(0);

        if (pressing)
            VisualPolish();

        if (Input.GetMouseButtonDown(0))
            inkParticle.Play();
        else if (Input.GetMouseButtonUp(0))
            inkParticle.Stop();

        UpdateAimVisual();
    }

    private void UpdateAimVisual()
    {
        Vector3 angle = parentController.localEulerAngles;

        float targetPitch = Mathf.Clamp(
            input.GetPitch() * visualPitchMultiplier,
            minVisualPitch,
            maxVisualPitch
        );

        float newX = Mathf.LerpAngle(
            parentController.localEulerAngles.x,
            targetPitch,
            visualPitchSmooth
        );

        parentController.localEulerAngles = new Vector3(newX, angle.y, angle.z);
    }

    private void VisualPolish()
    {
        if (!DOTween.IsTweening(parentController))
        {
            parentController.DOComplete();

            Vector3 localPos = parentController.localPosition;

            parentController.DOLocalMove(localPos - new Vector3(0, 0, 0.2f), 0.03f)
                .OnComplete(() =>
                    parentController.DOLocalMove(localPos, 0.1f).SetEase(Ease.OutSine)
                );

            if (impulseSource != null)
                impulseSource.GenerateImpulse();
        }

        if (!DOTween.IsTweening(splatGunNozzle))
        {
            splatGunNozzle.DOComplete();
            splatGunNozzle.DOPunchScale(new Vector3(0, 1, 1) / 1.5f, 0.15f, 10, 1);
        }
    }
}