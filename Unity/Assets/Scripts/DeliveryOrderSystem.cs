using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("주문 설정")]
    public float ordergenerationInterval = 15f;
    public int maxActiveOrders = 8;

    [Header("게임 상태")]
    public int totalOrdersGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    //주문 리스트
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    [System.Serializable]
    public class OrderSystemEvents
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickedUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }

    public OrderSystemEvents orderEvents;
    public DeliveryDriver driver;


    // Start is called before the first frame update
    void Start()
    {
        driver = FindObjectOfType<DeliveryDriver>();
        FindAllBuilding();

        //초기 주문 생성
        StartCoroutine(GenerateInitialOrders());
        //주기적 주문 생성
        StartCoroutine(orderGenerator());
        //만료 체크
        StartCoroutine(ExpiredOrderChecker());
    }

    void FindAllBuilding()
    {
        Building[] allbuildings = FindObjectsOfType<Building>();

        foreach (Building building in allbuildings)
        {
            if (building.BuildingType == BuildingType.Restaurant)
            {
                restaurants.Add(building);
            }
            else if (building.BuildingType == BuildingType.Coustomer)
            {
                customers.Add(building);
            }
        }
        Debug.Log($"음식점 {restaurants.Count}개, 고객 {customers.Count}개 찾음");
    }

    void CreateNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;

        //랜덤 음식점 고객 선택
        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCustomer = customers[Random.Range(0, customers.Count)];

        //같은 건물이면 다시선택
        if(randomRestaurant == randomCustomer)
        {
            randomCustomer = customers[Random.Range(0, customers.Count)];
        }

        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOrder = new DeliveryOrder(++totalOrdersGenerated, randomRestaurant, randomCustomer, reward);

        currentOrders.Add(newOrder);
        orderEvents.OnNewOrderAdded?.Invoke(newOrder);
    }

    void PickupOrder(DeliveryOrder order)
    {
        order.State = OrderState.PickedUp;
        orderEvents.OnOrderPickedUp?.Invoke(order);
    }

    void CompleteOrder(DeliveryOrder order)
    {
        order.State = OrderState.Completed;
        completedOrders++;

        if(driver != null)
        {
            driver.AddMoney(order.reward);
            
        }
        currentOrders.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }

    void ExpireOrder(DeliveryOrder order)
    {
        order.State = OrderState.Expired;
        expiredOrders++;
        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.State == OrderState.WaitingPickup)
            {
                count++;
            }
        }
        return count;
    }
    public int GetDeliveryWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.State == OrderState.PickedUp)
            {
                count++;
            }
        }
        return count;
    }

    DeliveryOrder FindOrderForPickup(Building restaurant)
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.State == OrderState.WaitingPickup && order.restaurantBuilding == restaurant)
            {
                return order;
            }
        }
        return null;
    }
    DeliveryOrder FindOrderForDelivery(Building customer)
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.customerBuilding == customer && order.State == OrderState.PickedUp)
            {
                return order;
            }
        }
        return null;
    }

    public void OnDriverEnteredRestarant(Building restaurant)
    {
        DeliveryOrder orderToPickup = FindOrderForPickup(restaurant);

        if(orderToPickup != null)
        {
            PickupOrder(orderToPickup);
        }

        
    }

    public void OnDriverEnteredCustomer(Building customer)
    {
        DeliveryOrder orderToDeliver = FindOrderForDelivery(customer);
        if (orderToDeliver != null)
        {
            CompleteOrder(orderToDeliver);
        }
    }

    IEnumerator GenerateInitialOrders()
    {
        yield return new WaitForSeconds(1f);

        for(int i = 0; i < 3; i++)
        {
            CreateNewOrder();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator orderGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(ordergenerationInterval);
            if (currentOrders.Count < maxActiveOrders)
            {
                CreateNewOrder();
            }
        }
    }

    IEnumerator ExpiredOrderChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            List<DeliveryOrder> ordersToExpire = new List<DeliveryOrder>();

            foreach (DeliveryOrder order in currentOrders)
            {
                if (order.IsExpired() && order.State != OrderState.Completed)
                {
                    ordersToExpire.Add(order);
                }
            }
            foreach(DeliveryOrder expired in ordersToExpire)
            {
                ExpireOrder(expired);
            }
        }
    }
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 1300));

        GUILayout.Label("===배달 주문===");
        GUILayout.Label($"활성 주문: {currentOrders.Count}개");
        GUILayout.Label($"픽업 대기: {GetPickWaitingCount()}개");
        GUILayout.Label($"배달 대기: {GetDeliveryWaitingCount()}개");
        GUILayout.Label($"완료 : {completedOrders}개 | 만료 : {expiredOrders}");

        GUILayout.Space(10);

        foreach (DeliveryOrder order in currentOrders)
        {
            string status = order.State == OrderState.WaitingPickup ? "픽업 대기" : "배달대기";
            float timeleft = order.GetTimeRemaining();

            GUILayout.Label($"[#{order.orderID}] {order.restaurantName} -> {order.customerName}");
            GUILayout.Label($"{status} | {timeleft:F0}초 남음");
        }
        GUILayout.EndArea();
    }
}
