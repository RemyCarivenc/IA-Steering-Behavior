using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationVL_Steering : Steering
{
    [SerializeField]
    private ObjectAI leader;
    [SerializeField]
    private bool useLeaderOrientation = false;
    [SerializeField]
    private float distanceMax;
    [SerializeField]
    private float slowingDistance;
    [SerializeField, Range(0, 360)]
    private float angle;

    private static Dictionary<ObjectAI, List<FormationVL_Steering>> dictFormation = new Dictionary<ObjectAI, List<FormationVL_Steering>>();
    private int nbInLine = 10;

    protected override void Start()
    {
        if (!dictFormation.ContainsKey(leader))
            dictFormation.Add(leader, new List<FormationVL_Steering>());

        dictFormation[leader].Add(this);
    }

    protected override Vector3 CalculateForce()
    {
        Vector3 steering = Vector3.zero;
        Vector2 forward ,right;
        /*forward = new Vector2(leader.Velocity.normalized.x,leader.Velocity.normalized.y);
        right = new Vector2(forward.y, -forward.x);*/
        int id = dictFormation[leader].IndexOf(this);

        if (leader == null)
            return steering;

        // Add 1 unit per line to have an odd number 
        if (nbInLine % 2 == 0)
            nbInLine++;
        // Use minimum between nbInLine and max units
        if (nbInLine > dictFormation[leader].Count)
            nbInLine = dictFormation[leader].Count;

        int idRight = (id % nbInLine) - nbInLine / 2;
        int idBack = id / nbInLine;
        float fX = idRight * distanceMax;
        float fY = Mathf.Abs(fX) * Mathf.Tan(angle) + idBack * distanceMax;
        Vector2 arrivalOffset = /*right*/Vector2.up * fX - /*forward*/Vector2.right * fY;
        Vector2 arrivalPos = new Vector2(leader.Position.x, leader.Position.y) + arrivalOffset;

        //Arrival
        Vector2 targetOffset = arrivalPos - new Vector2(ObjectAI.Position.x, ObjectAI.Position.y);
        float distance = targetOffset.magnitude;
        float rampedSpeed = ObjectAI.MaxSpeed * distance / slowingDistance;
        float clippedSpeed = (rampedSpeed < ObjectAI.MaxSpeed) ? rampedSpeed : ObjectAI.MaxSpeed;
        Vector2 desiredVelocity = targetOffset * clippedSpeed / distance;
        Vector2 AAA = desiredVelocity - new Vector2(ObjectAI.Velocity.x, ObjectAI.Velocity.y);

        return new Vector3(AAA.x, AAA.y, 0);
    }
}
