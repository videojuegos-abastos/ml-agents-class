using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Actor : Agent
{

    [SerializeField]
    Transform target;
    [SerializeField]
    float speed;

    void Start()
    {
        target = transform.parent.GetChild(2).GetChild(0);
        StartCoroutine(nameof(SlowUpdate));
    }

    IEnumerator SlowUpdate()
    {
        while (isActiveAndEnabled)
        {
            float distance = (target.position - transform.position).magnitude;
            AddReward( 100 / Mathf.Pow(distance, 2));
            yield return new WaitForSeconds(.1f);
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        target.parent.GetComponent<RandomPosition>().SetRandomPositions();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 targetPos = new Vector2(target.localPosition.x, target.localPosition.z);
        Vector2 myPos = new Vector2(transform.localPosition.x, transform.localPosition.z);


        sensor.AddObservation(targetPos);
        sensor.AddObservation(myPos);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 direction = new Vector3(moveX, 0, moveZ);

        transform.localPosition += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider collider)
    {
        SetReward(+1f);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;

        continousActions[0] = Input.GetAxis("Horizontal");
        continousActions[1] = Input.GetAxis("Vertical");
    }


}
