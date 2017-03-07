using Assets.Gamelogic.Core;
using Assets.Gamelogic.EntityTemplate;
using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Collections;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Global;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using Improbable.Worker;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Gamelogic.Global
{
    public class PlayerManagementBehaviour : MonoBehaviour
    {
        [Require] private PlayerLifeCycle.Writer playerLifeCycle;
        
        private uint nextAvailableTeamId;
        private Map<string, EntityId> playerEntityIds;

        private void OnEnable()
        {
            playerLifeCycle.CommandReceiver.OnSpawnPlayer.RegisterResponse(OnSpawnPlayer);
            playerLifeCycle.CommandReceiver.OnDeletePlayer.RegisterResponse(OnDeletePlayer);

            playerEntityIds = new Map<string, EntityId>(playerLifeCycle.Data.playerEntityIds);
        }

        private void OnDisable()
        {
            playerLifeCycle.CommandReceiver.OnSpawnPlayer.DeregisterResponse();
            playerLifeCycle.CommandReceiver.OnDeletePlayer.DeregisterResponse();

            playerEntityIds = null;
        }

        private Nothing OnSpawnPlayer(SpawnPlayerRequest request, ICommandCallerInfo callerinfo)
        {
            // Check if we already have a player, or request for a player
            if (playerEntityIds.ContainsKey(callerinfo.CallerWorkerId))
            {
                return new Nothing();
            }

            // Mark as requested
            playerEntityIds.Add(callerinfo.CallerWorkerId, new EntityId());

            // Request Id
            RequestPlayerEntityId(callerinfo.CallerWorkerId);

            // Respond
            SendMapUpdate();
            return new Nothing();
        }

        private Nothing OnDeletePlayer(DeletePlayerRequest request, ICommandCallerInfo callerinfo)
        {
            if (playerEntityIds.ContainsKey(callerinfo.CallerWorkerId))
            {
                var entityId = playerEntityIds[callerinfo.CallerWorkerId];
                if (entityId.IsValid())
                {

                    SpatialOS.Commands.DeleteEntity(playerLifeCycle, entityId, result =>
                    {
                        if (result.StatusCode != StatusCode.Success)
                        {
                            Debug.LogErrorFormat("failed to delete inactive entity {0} with error message: {1}",
                                entityId, result.ErrorMessage);
                            return;
                        }
                    });
                }
                playerEntityIds.Remove(callerinfo.CallerWorkerId);
                SendMapUpdate();
            }
            return new Nothing();
        }

        private void SendMapUpdate()
        {
            var update = new PlayerLifeCycle.Update();
            update.SetPlayerEntityIds(playerEntityIds);
            playerLifeCycle.Send(update);
        }

        private void RequestPlayerEntityId(string workerId)
        {
            SpatialOS.Commands.ReserveEntityId(playerLifeCycle, (result) =>
            {
                if (result.StatusCode != StatusCode.Success)
                {
                    RequestPlayerEntityId(workerId);
                    Debug.LogError("Failed to reserve entityId for player, retrying");
                    return;
                }

					if (playerEntityIds != null && playerEntityIds.ContainsKey(workerId) && !playerEntityIds[workerId].IsValid())
                {
                    playerEntityIds[workerId] = result.Response.Value;

                    SendMapUpdate();
                    SpawnPlayer(workerId, result.Response.Value);
                }
            });
        }

        private void SpawnPlayer(string workerId, EntityId entityId)
        {
            var assignedTeamId = (nextAvailableTeamId++) % (uint)SimulationSettings.TeamCount;
            var spawningOffset = new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f) * SimulationSettings.PlayerSpawnOffsetFactor;
            var hqPos = SimulationSettings.TeamHQLocations[assignedTeamId].ToVector3();
            var initialPosition = hqPos + spawningOffset;

            var playerEntityTemplate = EntityTemplateFactory.CreatePlayerTemplate(workerId, initialPosition.ToCoordinates(), assignedTeamId);
            SpatialOS.Commands.CreateEntity(playerLifeCycle, entityId, SimulationSettings.PlayerPrefabName, playerEntityTemplate, (response) =>
            {
                if (response.StatusCode != StatusCode.Success)
                {
                    SpawnPlayer(workerId, entityId);
                    Debug.LogError("Failed to create player entity, retrying");
                    return;
                }
            });
        }
    }
}
