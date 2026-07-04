using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Building))]
public class Table : MonoBehaviour
{
    [SerializeField] private Seat[] linkedSeats;

    public IReadOnlyList<Seat> LinkedSeats => linkedSeats;
}
