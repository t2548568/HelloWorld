using Improbable.Entity.Component;
using UnityEngine;
using System.Collections;
using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Core;
using Improbable.Worker;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;

namespace Assets.Gamelogic.Core
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class HeartbeatReceiver : MonoBehaviour
    {
        [Require] private Heartbeat.Writer heartbeat;

        private EntityId entityIdCache;
        private Coroutine heartbeatCoroutine;

        private void OnEnable()
        {
            entityIdCache = gameObject.EntityId();
            heartbeat.CommandReceiver.OnHeartbeat.RegisterResponse(OnHeartbeat);
            heartbeatCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.HeartbeatInterval, HeartbeatIteration));
        }

        private void OnDisable()
        {
            heartbeat.CommandReceiver.OnHeartbeat.DeregisterResponse();
            StopCoroutine(heartbeatCoroutine);
        }

        private void HeartbeatIteration()
        {
            if (heartbeat.Data.timeoutBeats > 0)
            {
                SetHeartbeat(heartbeat.Data.timeoutBeats - 1);
            }
            else
            {
                Cleanup();
            }
        }

        private Nothing OnHeartbeat(Nothing Request, ICommandCallerInfo CallerInfo)
        {
            SetHeartbeat(SimulationSettings.HeartbeatMax);
            return new Nothing();
        }

        private void Cleanup()
        {
            SpatialOS.Commands.DeleteEntity(heartbeat, entityIdCache, OnDeleteResponse);
        }

        private void OnDeleteResponse(ICommandCallbackResponse<EntityId> response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogErrorFormat("failed to delete inactive entity {0} with error message: {1}", entityIdCache, response.ErrorMessage);
            }
        }

        private void SetHeartbeat(uint beats)
        {
            var update = new Heartbeat.Update();
            update.SetTimeoutBeats(beats);
            heartbeat.Send(update);
        }
    }
}