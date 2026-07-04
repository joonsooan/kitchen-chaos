using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Building))]
public class Table : MonoBehaviour
{
    [SerializeField] private Seat[] linkedSeats;

    public IReadOnlyList<Seat> LinkedSeats => linkedSeats;

    private void Awake()
    {
        for (int i = 0; i < linkedSeats.Length; i++)
        {
            if (linkedSeats[i] != null) linkedSeats[i].AssignTable(i);
        }
    }
}
