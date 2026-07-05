using System.Collections.Generic;
using UnityEngine;

public class SeatManager : KSingleton<SeatManager>
{
    // 만약 손님 경로 탐색 관련 버그 나면 그냥 Translate으로 구현하기
    private readonly List<Seat> seats = new();

    public void Register(Seat seat)
    {
        if (!seats.Contains(seat)) seats.Add(seat);
    }

    public void Unregister(Seat seat)
    {
        seats.Remove(seat);
    }

    public bool HasFreeSeat()
    {
        foreach (Seat seat in seats)
        {
            if (!seat.IsOccupied) return true;
        }
        return false;
    }

    public bool TryReserveNearestSeat(Vector3 fromWorldPosition, out Seat seat)
    {
        Seat nearest = null;
        float nearestSqrDist = float.MaxValue;

        foreach (Seat candidate in seats)
        {
            if (candidate.IsOccupied) continue;

            float sqrDist = (candidate.transform.position - fromWorldPosition).sqrMagnitude;
            if (sqrDist < nearestSqrDist)
            {
                nearestSqrDist = sqrDist;
                nearest = candidate;
            }
        }

        if (nearest == null)
        {
            seat = null;
            return false;
        }

        seat = nearest;
        return nearest.TryReserve();
    }

    // 가까운 순으로 정렬된 빈 좌석 목록 (예약은 안 함 — 호출측이 경로 확인 후 TryReserve).
    public List<Seat> GetFreeSeatsByDistance(Vector3 fromWorldPosition)
    {
        List<Seat> free = new List<Seat>();
        foreach (Seat s in seats)
            if (!s.IsOccupied) free.Add(s);

        free.Sort((a, b) =>
            (a.transform.position - fromWorldPosition).sqrMagnitude
                .CompareTo((b.transform.position - fromWorldPosition).sqrMagnitude));
        return free;
    }
}
