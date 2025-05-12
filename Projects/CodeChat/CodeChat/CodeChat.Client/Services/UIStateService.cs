namespace CodeChat.Client.Services
{
    public class UIStateService
    {
        public event Action? OnShowCreateRoom;

        public void TriggerCreateRoom() {
            OnShowCreateRoom?.Invoke();
        }
    }

}
