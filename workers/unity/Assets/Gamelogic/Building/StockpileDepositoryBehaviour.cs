using Assets.Gamelogic.ComponentExtensions;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Life;
using Improbable.Building;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Life;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Building
{
    public class StockpileDepositoryBehaviour : MonoBehaviour
    {
        [Require] private StockpileDepository.Writer stockpileDepository;
        [Require] private Health.Writer health;

        private void OnEnable ()
        { 
            stockpileDepository.CommandReceiver.OnAddResource.RegisterResponse(OnAddResource);
        }

        private void OnDisable()
        {
            stockpileDepository.CommandReceiver.OnAddResource.DeregisterResponse();
        }

        private Nothing OnAddResource(AddResource Request, ICommandCallerInfo CallerInfo)
        {
            if (stockpileDepository.Data.canAcceptResources)
            {
                health.AddCurrentHealthDelta(Request.quantity);
            }
            return new Nothing();
        }
    }
}
