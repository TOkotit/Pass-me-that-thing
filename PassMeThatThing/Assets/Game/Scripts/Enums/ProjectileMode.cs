public enum ProjectileMode
{
    Ballistic,                    // начальная скорость + гравитация, без дальнейшего управления
    ConstantSpeedNoGravity,       // постоянная скорость без гравитации
    ConstantThrustWithGravity,    // постоянная тяга + гравитация (поддерживает заданную скорость)
    DeceleratingBallistic         // начальная скорость + трение + гравитация
}