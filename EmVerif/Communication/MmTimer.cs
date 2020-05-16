using System;
using System.Runtime.InteropServices;

namespace EmVerif.Communication
{
    class MmTimer
    {
        public static MmTimer Instance = new MmTimer();
        public event EventHandler OnTimer;

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeSetEvent(UInt32 msDelay, UInt32 msResolution, TimerEventHandler handler, UIntPtr userCtx, UInt32 eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeKillEvent(UInt32 timerEventId);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeBeginPeriod(UInt32 uMilliseconds);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeEndPeriod(UInt32 uMilliseconds);

        private delegate void TimerEventHandler(UInt32 id, UInt32 msg, UIntPtr userCtx, UIntPtr rsv1, UIntPtr rsv2);
        private UInt32 _TimerID;
        private TimerEventHandler _TimerHandler;

        public MmTimer()
        {
            _TimerID = 0;
        }

        public void Start()
        {
            if (timeBeginPeriod(1) == 0)
            {
                _TimerHandler = TimerProc;
                _TimerID = timeSetEvent(1, 0, _TimerHandler, UIntPtr.Zero, 1);
            }
        }

        public void Stop()
        {
            if (_TimerID != 0)
            {
                timeKillEvent(_TimerID);
                timeEndPeriod(1);
                _TimerID = 0;
            }
        }

        private void TimerProc(UInt32 id, UInt32 msg, UIntPtr userCtx, UIntPtr rsv1, UIntPtr rsv2)
        {
            OnTimer?.Invoke(this, new EventArgs());
        }
    }
}
