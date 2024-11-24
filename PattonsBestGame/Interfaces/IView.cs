namespace Pattons_Best
{
    public interface IView
    {
        void UpdateView(ref IGameInstance gi, GameAction action);
    }
}
