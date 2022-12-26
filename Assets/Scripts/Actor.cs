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
		// Forma 'rápida' de obtener MI objetivo (porque ahora tenemos más de un agent y target)
        target = transform.parent.GetChild(2).GetChild(0);
        StartCoroutine(nameof(SlowUpdate));
    }

    IEnumerator SlowUpdate()
    {
		// Prueba de cómo puede afectar recompensar nuestro agente dependiendo de la distancia al objetivo
        while (isActiveAndEnabled)
        {
            float distance = (target.position - transform.position).magnitude;
            AddReward( 100 / Mathf.Pow(distance, 2));
            yield return new WaitForSeconds(.1f);
        }
    }

    public override void OnEpisodeBegin()
    {
		// Reseteamos mi posición y la del objetivo
        transform.localPosition = Vector3.zero;
        target.parent.GetComponent<RandomPosition>().SetRandomPositions();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
		// Observaciones / Inputs de la red
		// Suministramos a la red los parámetros que creemos que son relevantes
        Vector2 targetPos = new Vector2(target.localPosition.x, target.localPosition.z);
        Vector2 myPos = new Vector2(transform.localPosition.x, transform.localPosition.z);

        sensor.AddObservation(targetPos);
        sensor.AddObservation(myPos);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
		// Hacemos las acciones (moverse) con el output de la red
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 direction = new Vector3(moveX, 0, moveZ);

        transform.localPosition += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider collider)
    {
		// Esta es la primera forma que tomamos de recompensa, cuando el agente toca el objetivo, lo recompensamos
        SetReward(+1f);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
		// Heurística, sobreescribimos los outputs de la red para comprobar que las acciones que puede hacer el agente funcionan
		// *Para manejar con heurística tenemos que activarla en los Behaviour Parameters de la red. (Heuristic)
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;

        continousActions[0] = Input.GetAxis("Horizontal");
        continousActions[1] = Input.GetAxis("Vertical");
    }


}
