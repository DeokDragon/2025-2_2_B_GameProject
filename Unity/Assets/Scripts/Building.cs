using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    [Header("건물 정보")]
    public BuildingType BuildingType;
    public string buildingName = "건물";

    [System.Serializable]

    public class BuildingEvents
    {
        public UnityEvent<string> OnDriverEntered;
        public UnityEvent<string> OnDriverExited;
        public UnityEvent<BuildingType> OnServiceUsed;
    }

    public BuildingEvents buildingEvents;

    private DeliveryOrderSystem orderSystem;

    void HandleDriverService(DeliveryDriver driver)
    {
        switch (BuildingType)
            {
            case BuildingType.Restaurant:
                if(orderSystem != null)
                {
                    orderSystem.OnDriverEnteredRestarant(this);
                }
                
                break;
            case BuildingType.Coustomer:
                if(orderSystem != null)
                {
                    orderSystem.OnDriverEnteredCustomer(this);
                }
                
                driver.CompleteDelivery();
                break;
            case BuildingType.ChargingStation:
                
                driver.ChargeBattery();
                break;

            }
        buildingEvents.OnServiceUsed?.Invoke(BuildingType);
    }

    void OnTriggerEnter(Collider other)
    {
        DeliveryDriver driver = other.GetComponent<DeliveryDriver>();
        if (driver != null)
        {
            buildingEvents.OnDriverEntered?.Invoke(buildingName);
            HandleDriverService(driver);
        }
    }

    void OnTriggerExit(Collider other)
    {
        DeliveryDriver driver = other.GetComponent<DeliveryDriver>();
        if(driver != null)
        {
            buildingEvents.OnDriverExited?.Invoke(buildingName);
            Debug.Log($"{buildingName}을 떠났습니다.");
        }
    }

    void CreateNameTag()
    {
        GameObject nameTag = new GameObject("NameTag");
        nameTag.transform.SetParent(this.transform);
        nameTag.transform.localPosition = Vector3.up * 1.5f;
        
        TextMesh textMesh = nameTag.AddComponent<TextMesh>();
        textMesh.text = buildingName;
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.2f;
        
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;
        // 항상 카메라를 향하도록 설정
        nameTag.AddComponent<Bildboard>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupBuilding();
        orderSystem = FindObjectOfType<DeliveryOrderSystem>();
        CreateNameTag();
    }

    void SetupBuilding()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            switch (BuildingType)
            {
                case BuildingType.Restaurant:
                    mat.color = Color.red;
                    
                    break;
                case BuildingType.Coustomer:
                    mat.color = Color.green;
                    
                    break;
                case BuildingType.ChargingStation:
                    mat.color = Color.yellow;
                    
                    break;
            }
        }
        Collider col = GetComponent<Collider>();
        if (col != null) { col.isTrigger = true; }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
