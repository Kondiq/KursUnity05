using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPatrolComponent : MonoBehaviour
{
    private List<Vector3> navPointsList;
    private int index = 0;
    private NavMeshAgent agent;
    bool isBeginning = true;

    void Start()
    {
        enabled = false;
        navPointsList = new List<Vector3>();

        agent = GetComponent<NavMeshAgent>();

        if (navPointsList.Count > 0)
            enabled = true;
    }

    void Update()
    {
        GoToNextNavPointRemoveOld();
    }

    //pobranie nastepnego punktu
    Vector3 GetNextNavPoint()
    {
        //pierwszy punkt, poczatek listy na starcie sciezki
        if (isBeginning)
        {
            isBeginning = false;
            return navPointsList[0];
        }
        //kolejny punkt
        if (index + 1 < navPointsList.Count)
        {
            index++;
            return navPointsList[index];
        }
        //jesli doszedl do ostatniego punktu listy
        index = 0;
        return navPointsList[0];
    }

    bool GoToNextNavPointRemoveOld()
    {
        if (navPointsList.Count > 0)
        {
            if (isBeginning)
            {
                isBeginning = false;
                agent.destination = navPointsList[0];
                return true;
            }

            //sprawdzenie czy agent nie oblicza nowej sciezki i dystansu od celu
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                //ustawienie celu na nastepny navpoint
                RemoveNavPoint(0);
                if (navPointsList.Count > 0)
                {
                    agent.destination = navPointsList[0];
                    return true;
                }
            }
        }
        return false;               
    }

    //ustawienie celu na kolejny punkt
    void GoToNextNavPoint()
    {
        if (navPointsList.Count > 0)
            //sprawdzenie czy agent nie oblicza nowej sciezki i dystansu od celu
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                //ustawienie celu na nastepny navpoint
                agent.destination = GetNextNavPoint();    
    }

    public void GoToPoint(Vector3 point)
    {
        ClearNavList();
        AddNavPoint(ref point);
        agent.destination = GetNextNavPoint();
    }

    public void AddNavPoint(ref Vector3 point)
    {
        navPointsList.Add(point);
        enabled = true;
        //Debug.Log(point);
    }

    public void RemoveNavPoint(int index)
    {
        if (index < navPointsList.Count)
            navPointsList.RemoveAt(index);
        if (navPointsList.Count < 1)
            enabled = false;
    }

    public void ClearNavList()
    {
        navPointsList.Clear();
        enabled = false;
    }

    public int GetNavPointsCount()
    {
        return navPointsList.Count;
    }
}
