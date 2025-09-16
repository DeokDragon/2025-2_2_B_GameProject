using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DeliveryOrder
{
    public int orderID;
    public string restaurantName;
    public string customerName;
    public Building restaurantBuilding;
    public Building customerBuilding;
    public float orderTime;
    public float timeLimit;
    public float reward;
    public OrderState State;

    public DeliveryOrder(int id, Building restaurant, Building customer, float rewardAmount)
    {
        orderID = id;
        restaurantBuilding = restaurant;
        customerBuilding = customer;
        restaurantName = restaurant.buildingName;
        customerName = customer.buildingName;
        orderTime = Time.time;
        timeLimit = Random.Range(60f, 120f);
        reward = rewardAmount;
        State = OrderState.WaitingPickup;
    }

    public float GetTimeRemaining()
    {
        return Mathf.Max(0, timeLimit - (Time.time - orderTime));
    }

    public bool IsExpired()
    {
        return GetTimeRemaining() <= 0f;
    }
}
