using System;
using UnityEngine;


namespace LifeHMA.Utilities
{
    public abstract class Timer
    {
        protected float initialTime;
        protected float Time { get; set; }

        public float progress => Time / initialTime;
        public bool isRunning { get; protected set; }

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        protected Timer(float value)
        {
            initialTime = value;
            isRunning = false;
        }

        public void Start()
        {
            Time = initialTime;
            if(!isRunning)
            {
                isRunning = true;
                OnTimerStart.Invoke();
            }
        }
        public void Stop()
        {
            if (isRunning)
            {
                isRunning = false;
                OnTimerStop.Invoke();
            }
        }

        public void Resume() => isRunning = true;
        public void Pause() => isRunning = false;

        public abstract void Tick(float deltaTime);
    }

    public class CountdownTimer : Timer
    {
        public CountdownTimer(float value) : base(value) { }

        public override void Tick(float deltaTime)
        {
            if(isRunning && Time > 0)
            {
                Time -= deltaTime;
            }
            if (isRunning && Time <= 0)
            {
                Stop();
            }
        }

        public bool IsFinished => Time <= 0;

        public void Reset() => Time = initialTime;

        public void Reset(float newTime)
        {
            initialTime = newTime;
            Reset();
        }
    }

    public class StopwatchTimer : Timer
    {
        public StopwatchTimer() : base(0) { }

        public override void Tick(float deltaTime)
        {
            if(isRunning)
            {
                Time += deltaTime;
            }
        }

        public void Reset() => Time = 0;

        public float GetTime() => Time;
    }

    public class HMA_Math
    {
        public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }
            else return a * Quaternion.Inverse(b);
        }

        public static Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}