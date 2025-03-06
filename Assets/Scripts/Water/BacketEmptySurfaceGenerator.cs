using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacketEmptySurfaceGenerator : MonoBehaviour
{
    [SerializeField] private PitcherWaterController _pitcherWaterController;
    [SerializeField] private BacketEmptyWaterController _backetEmptyWaterController;

    private MeshRenderer _meshRenderer;
    private float fillingRate;
    private int pouredWaterLevel = 0;
    private float reducedWaterLevel = 0;
    public float maxPitcherFillingRate = 0.7f;
    public int maxPouredWaterLevel;  // ���̗��̍ő僌�x��
    public float maxFillingRate = 0.7f;  // fillingRate�̍ő�l

    // Start is called before the first frame update
    void Start()
    {
        fillingRate = _backetEmptyWaterController.fillingRate;  // �����l���擾
        pouredWaterLevel = 0;

        // MeshRenderer ���擾
        _meshRenderer = _backetEmptyWaterController.GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            Debug.LogError("MeshRenderer �� BacketEmptyWaterController �ɃA�^�b�`����Ă��܂���B");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ���݂� pouredWaterLevel �ɉ����� fillingRate ���X�V
        UpdateFillingRate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterGrain(Clone)")
        {
            pouredWaterLevel++;
            pouredWaterLevel = Mathf.Clamp(pouredWaterLevel, 0, maxPouredWaterLevel);  // �ő�l�𒴂��Ȃ��悤�ɐ���
            Debug.Log(pouredWaterLevel);
        }
        else if (other.gameObject.name == "PitcherWaterGenerator")
        {
            reducedWaterLevel = ((maxPitcherFillingRate - _pitcherWaterController.fillingRate) / maxPitcherFillingRate) * 60; //Pitcher��fillingrate�ɉ����Đ������炷
        }
    }

    private void UpdateFillingRate()
    {
        // pouredWaterLevel �Ɋ�Â��� fillingRate ���X�V
        fillingRate = Mathf.Lerp(0, maxFillingRate, (float)pouredWaterLevel / maxPouredWaterLevel);
        fillingRate = Mathf.Clamp(fillingRate, 0, maxFillingRate);  // fillingRate �̏���𐧌�

        // BowlWaterController �ɒl�𔽉f
        _backetEmptyWaterController.fillingRate = fillingRate;

        // fillingRate �� 0 �ȉ��Ȃ� MeshRenderer ���I�t�A�����łȂ���΃I��
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = fillingRate > 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "PitcherWaterGenerator")
        {
            pouredWaterLevel -= (int)reducedWaterLevel;
            UpdateFillingRate();
        }
    }
}
