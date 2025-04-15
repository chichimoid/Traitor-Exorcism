using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public struct FixedIntervalFloat : INetworkSerializable
    {
        public FixedIntervalFloat(float val, float max, float min = 0f)
        {
            if (max <= min) throw new System.ArgumentException("Max must be greater than min");
            
            _value = Mathf.Clamp(val, min, max);
            _maxValue = max;
            _minValue = min;
        }

        private float _value;
        private readonly float _maxValue;
        private readonly float _minValue;
        
        public float Value {
            get => _value; 
            set => _value = Mathf.Clamp(value, _minValue, _maxValue);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _value);
        }
    }
}