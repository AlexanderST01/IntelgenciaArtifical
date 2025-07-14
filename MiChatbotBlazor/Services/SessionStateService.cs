namespace MiChatbotBlazor.Services
{
    public class SessionStateService
    {
        private int _currentSessionId;
        
        public event Action<int>? SessionChanged;
        
        public int CurrentSessionId 
        { 
            get => _currentSessionId;
            private set 
            {
                if (_currentSessionId != value)
                {
                    _currentSessionId = value;
                    SessionChanged?.Invoke(value);
                }
            }
        }
        
        public void SetCurrentSession(int sessionId)
        {
            CurrentSessionId = sessionId;
        }
    }
}
