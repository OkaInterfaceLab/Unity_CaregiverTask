using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BowlSmallWaterSurfaceGenerator : MonoBehaviour
{
    [SerializeField] private BowlSmallWaterController _bowlSmallWaterController;
    [SerializeField] private TextMeshProUGUI _waterTemp;

    private MeshRenderer _meshRenderer;
    private float fillingRate;
    public int pouredWaterLevel = 0;
    public int maxPouredWaterLevel; // ���̗��̍ő僌�x��
    public float maxFillingRate = 0.7f; // fillingRate�̍ő�l

    public float currentBowlWaterTemperature = 20.0f; // �������� (�ێ�)
    public float ambientTemperature = 20.0f; // �����x (�ێ�)
    public float coolingRate = 0.1f; // ���R��p�� (1�b������̉��x����)

    private void Start()
    {
        pouredWaterLevel = 0;

        // MeshRenderer ���擾
        _meshRenderer = _bowlSmallWaterController.GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            Debug.LogError("MeshRenderer �� BowlWaterController �ɃA�^�b�`����Ă��܂���B");
        }
    }

    private void Update()
    {
        // BowlWaterController �� fillingRate ����Ɏ擾
        fillingRate = _bowlSmallWaterController.fillingRate;

        // MeshRenderer �̏�Ԃ��X�V
        UpdateMeshRenderer();

        // fillingRate ���X�V
        UpdateFillingRateFromPouredWaterLevel();

        // ���R��p��K�p
        ApplyNaturalCooling();

        if (fillingRate <= 0)
        {
            _waterTemp.text = "Bowl Water Temp : Empty";
        }
        else
        {
            _waterTemp.text = "Water Temp : " + currentBowlWaterTemperature + "C";
        }
    }

    private void UpdateMeshRenderer()
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = fillingRate > 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterGrain(Clone)")
        {
            pouredWaterLevel++;
            pouredWaterLevel = Mathf.Clamp(pouredWaterLevel, 0, maxPouredWaterLevel);

            WaterGrainController _waterGrainController = other.GetComponent<WaterGrainController>();

            if (_waterGrainController != null)
            {
                // �����q�̉��x���擾���ă{�E���̐������X�V
                float waterGrainTemperature = _waterGrainController.waterTemperature;
                UpdateBowlWaterTemperature(waterGrainTemperature);
            }

            Destroy(other.gameObject); // �����q���폜
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PitcherWaterGenerator")
        {
        }
    }

    private void UpdateFillingRateFromPouredWaterLevel()
    {
        fillingRate = Mathf.Lerp(0, maxFillingRate, (float)pouredWaterLevel / maxPouredWaterLevel);
        fillingRate = Mathf.Clamp(fillingRate, 0, maxFillingRate);

        _bowlSmallWaterController.fillingRate = fillingRate;

        // Debug.Log(fillingRate);
    }

    private void UpdateBowlWaterTemperature(float waterGrainTemperature)
    {
        // �{�E�����̐��̑��� (pouredWaterLevel)
        float bowlWaterMass = pouredWaterLevel;

        // �����q�̗� (1����)
        float grainMass = 1.0f;

        // �{�E�����̐��ʂ��[���̏ꍇ�A�P���ɗ��q�̉��x���̗p
        if (bowlWaterMass <= 0)
        {
            currentBowlWaterTemperature = waterGrainTemperature;
            return;
        }

        // �V���������ʂ��v�Z
        float newTotalMass = pouredWaterLevel + grainMass;

        // ���ʉ��d���ςŐV�������x���v�Z
        currentBowlWaterTemperature = (pouredWaterLevel * currentBowlWaterTemperature + grainMass * waterGrainTemperature) / newTotalMass;

    }

    private void ApplyNaturalCooling()
    {
        // ���Ԍo�߂Ɋ�Â����R��p
        if (currentBowlWaterTemperature > ambientTemperature)
        {
            currentBowlWaterTemperature -= coolingRate * Time.deltaTime;
            currentBowlWaterTemperature = Mathf.Max(currentBowlWaterTemperature, ambientTemperature);
        }
    }
}
