using UnityEngine;

namespace Jongmin
{
    public class TurnManagerInjector : MonoBehaviour, IInjector
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private TurnRuleDesigner turnRuleDesigner;
        
        public void Inject()
        {
            turnManager.Construct(turnRuleDesigner);
            DIContainer.Register<TurnManager>(turnManager);
        }
    }
}