using UnityEngine;
using UnityEngine.AI;

public class Disembark : Interactable
{
    public float SampleDistance = 1.0f;

    public override void Interact(Transform interactorTransform)
    {
        Player player = GameManager.Get().Player;
        Boat boat = GameManager.Get().Boat;

        Vector3 targetPoint;
        if (player.StandingOn == Player.GroundType.Boat)
        {
            targetPoint = transform.position + transform.forward * (InteractDistance - SampleDistance);
        }
        else
        {
            targetPoint = boat.transform.position + Vector3.up * 0.5f;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPoint, out hit, SampleDistance, NavMesh.AllAreas))
        {
            // In water
            if (hit.position.y < -0.5f)
            {
                return;
            }

            Player.GroundType ground = Player.GroundType.None;
            if (Physics.Raycast(hit.position + Vector3.up, Vector3.down, out RaycastHit rayHit))
            {
                if (rayHit.collider.gameObject.GetComponentInParent<Boat>() != null)
                {
                    ground = Player.GroundType.Boat;
                }
                else
                {
                    ground = Player.GroundType.Land;
                }
            }

            if ((player.StandingOn == Player.GroundType.Boat && ground == Player.GroundType.Land) ||
                (player.StandingOn == Player.GroundType.Land && ground == Player.GroundType.Boat))
            {
                player.transform.position = hit.position;
                boat.Stop();
                player.Reset();
            }
        }
    }
}
