using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class NetworkRigidbodyCustom : NetworkBehaviour
{
    [Header("Sync Rate")]
    [SerializeField] private float sendRate = 20f;

    [Header("Dead Zone")]
    [SerializeField] private float positionThreshold = 0.005f;
    [SerializeField] private float rotationThreshold = 0.5f;
    [SerializeField] private float velocityThreshold = 0.05f;

    [Header("Interpolation (наблюдатели)")]
    [SerializeField] private float interpolationDelay = 0.1f;
    [SerializeField] private int maxBufferSize = 32;

    [Header("Reconciliation (владелец)")]
    [SerializeField] private float positionSpring = 6f;
    [SerializeField] private float rotationSpring = 6f;
    [SerializeField] private float maxLinearCorrection = 3f;
    [SerializeField] private float maxAngularCorrection = 12f;
    [SerializeField] private float minPositionError = 0.02f;
    [SerializeField] private float minRotationError = 1.5f;
    [SerializeField] private float teleportPositionThreshold = 5.125f;
    [SerializeField] private float teleportRotationThreshold = 90f;

    [Header("Debug")]
    [SerializeField] private bool _log;

    private Rigidbody _rb;

    private float _sendInterval;
    private float _cosHalfRotationThreshold;
    private float _sendTimer;

    [SyncVar(hook = nameof(OnOwnerAuthorityChanged))]
    private bool _ownerAuthority;

    public bool IsOwnerAuthority => _ownerAuthority;

    //  Буфер снапшотов
    private struct Snapshot
    {
        public double Time;
        public Vector3 Pos;
        public Quaternion Rot;
        public Vector3 Vel;
        public Vector3 AngVel;
    }

    private readonly Queue<Snapshot> _buffer = new();

    //  Последний серверный стейт 

    private bool _hasAuthState;
    private Vector3 _authPos;
    private Quaternion _authRot;
    private Vector3 _authVel;
    private Vector3 _authAngVel;
    
    //  Dead-zone

    private Vector3 _lastSentPos;
    private Quaternion _lastSentRot;
    private Vector3 _lastSentVel;

    private bool _wasSleeping;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        RecalculateSendInterval();
        RecalculateRotationThreshold();
    }

    private void RecalculateSendInterval()
    {
        _sendInterval = 1f / Mathf.Max(5f, sendRate);
    }

    private void RecalculateRotationThreshold()
    {
        _cosHalfRotationThreshold = Mathf.Cos(rotationThreshold * Mathf.Deg2Rad * 0.5f);
    }

    [Server]
    public void SetOwnerAuthority(bool on)
    {
        if (_ownerAuthority == on) return;
        _ownerAuthority = on;

        if (on)
        {
            _rb.WakeUp();
        }
    }

    private void OnOwnerAuthorityChanged(bool oldVal, bool newVal)
    {
        _sendTimer = 0f;
        _lastSentPos = _rb.position;
        _lastSentRot = _rb.rotation;
        _lastSentVel = _rb.linearVelocity;
        _hasAuthState = false;

        if (oldVal && !newVal)
        {
            _buffer.Clear();
            _buffer.Enqueue(new Snapshot
            {
                Time = NetworkTime.time,
                Pos = _rb.position,
                Rot = _rb.rotation,
                Vel = _rb.linearVelocity,
                AngVel = _rb.angularVelocity,
            });
        }

        if (_log)
            Debug.Log($"[NetRB] {name}: ownerAuthority {oldVal} → {newVal}, " +
                      $"isServer={isServer}, isOwned={isOwned}");
    }

    //  FixedUpdate
    //  - Сервер: рассылка. Held item — форсированно, free item — по сну.
    //  - Клиент-овнер: reconciliation через AddForce / AddTorque.

    private void FixedUpdate()
    {
        if (isServer)
        {
            if (!_ownerAuthority)
            {
                var sleeping = _rb.IsSleeping();

                if (sleeping)
                {
                    _wasSleeping = true;
                    return;
                }

                if (_wasSleeping)
                {
                    _wasSleeping = false;
                    _sendTimer = _sendInterval;
                }
            }

            _sendTimer += Time.fixedDeltaTime;
            if (_sendTimer < _sendInterval) return;
            _sendTimer = 0f;

            BroadcastState();
            return;
        }

        if (_ownerAuthority && isOwned && _hasAuthState)
            ApplyOwnerReconciliation();
    }

    private void BroadcastState()
    {
        var pos = _rb.position;
        var rot = _rb.rotation;
        var vel = _rb.linearVelocity;
        var angVel = _rb.angularVelocity;

        if (!_ownerAuthority && !NeedsSend(pos, rot, vel))
            return;

        _lastSentPos = pos;
        _lastSentRot = rot;
        _lastSentVel = vel;

        RpcPushState(pos, rot, vel, angVel, NetworkTime.time);
    }

    private bool NeedsSend(Vector3 pos, Quaternion rot, Vector3 vel)
    {
        if ((pos - _lastSentPos).sqrMagnitude > positionThreshold * positionThreshold)
            return true;

        var dot = Mathf.Abs(Quaternion.Dot(rot, _lastSentRot));
        if (dot < _cosHalfRotationThreshold)
            return true;

        return (vel - _lastSentVel).sqrMagnitude > velocityThreshold * velocityThreshold;
    }

    //  Приём серверного стейта

    [ClientRpc(channel = Channels.Unreliable)]
    private void RpcPushState(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, double serverTime)
    {
        if (isServer) return;

        if (_ownerAuthority && isOwned)
        {
            _authPos = pos;
            _authRot = rot;
            _authVel = vel;
            _authAngVel = angVel;
            _hasAuthState = true;
            return;
        }

        _buffer.Enqueue(new Snapshot
        {
            Time = serverTime,
            Pos = pos,
            Rot = rot,
            Vel = vel,
            AngVel = angVel,
        });
        while (_buffer.Count > maxBufferSize) _buffer.Dequeue();
    }

    //  Update — интерполяция наблюдателя

    private void Update()
    {
        if (isServer) return;

        var isOwner = _ownerAuthority && isOwned;
        if (!isOwner && _buffer.Count > 0)
            ApplyInterpolated(NetworkTime.time - interpolationDelay);
    }

    private void ApplyInterpolated(double renderTime)
    {
        Snapshot? from = null;
        Snapshot? to = null;

        foreach (var s in _buffer)
        {
            if (s.Time <= renderTime) from = s;
            else { to = s; break; }
        }

        if (from == null)
        {
            foreach (var s in _buffer) { from = s; break; }
            if (from == null) return;
            ApplySnapshot(from.Value);
            return;
        }

        if (to == null)
        {
            ApplySnapshot(from.Value);
            return;
        }

        var span = to.Value.Time - from.Value.Time;
        var t = span > 1e-6 ? (float)((renderTime - from.Value.Time) / span) : 1f;
        t = Mathf.Clamp01(t);

        _rb.position = Vector3.Lerp(from.Value.Pos, to.Value.Pos, t);
        _rb.rotation = Quaternion.Slerp(from.Value.Rot, to.Value.Rot, t);
        _rb.linearVelocity = Vector3.Lerp(from.Value.Vel, to.Value.Vel, t);
        _rb.angularVelocity = Vector3.Lerp(from.Value.AngVel, to.Value.AngVel, t);

        while (_buffer.Count > 2 && _buffer.Peek().Time < renderTime - 1.0)
            _buffer.Dequeue();
    }

    private void ApplySnapshot(Snapshot s)
    {
        _rb.position = s.Pos;
        _rb.rotation = s.Rot;
        _rb.linearVelocity = s.Vel;
        _rb.angularVelocity = s.AngVel;
    }

    //  Reconciliation — только клиент-овнер

    private void ApplyOwnerReconciliation()
    {
        var posError = _authPos - _rb.position;
        var posErrMag = posError.magnitude;

        var rotDelta = _authRot * Quaternion.Inverse(_rb.rotation);
        rotDelta.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;

        if (posErrMag > teleportPositionThreshold || Mathf.Abs(angleDeg) > teleportRotationThreshold)
        {
            _rb.position = _authPos;
            _rb.rotation = _authRot;
            _rb.linearVelocity = _authVel;
            _rb.angularVelocity = _authAngVel;

            if (_log)
                Debug.LogWarning($"[NetRB] {name} HARD reset: " +
                                 $"posErr={posErrMag:F3}, rotErr={angleDeg:F1}°");
            return;
        }

        if (posErrMag > minPositionError)
        {
            var corr = Vector3.ClampMagnitude(posError * positionSpring, maxLinearCorrection);
            _rb.AddForce(corr * _rb.mass, ForceMode.Force);
        }

        if (Mathf.Abs(angleDeg) > minRotationError)
        {
            var corr = Vector3.ClampMagnitude(
                axis * (angleDeg * Mathf.Deg2Rad * rotationSpring),
                maxAngularCorrection);
            _rb.AddTorque(corr * _rb.mass, ForceMode.Force);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        sendRate = Mathf.Max(5f, sendRate);
        interpolationDelay = Mathf.Clamp(interpolationDelay, 0f, 0.3f);

        if (Application.isPlaying)
        {
            RecalculateSendInterval();
            RecalculateRotationThreshold();
        }
    }
#endif
}