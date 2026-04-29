using UnityEngine;
using Mirror;

namespace Game.Scripts.GameFiles.Items
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerTestMove : NetworkBehaviour
    {
        public float moveSpeed = 5f;
        public float gravity = -9.81f;

        private CharacterController controller;
        private Vector3 velocity;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            
            if (isLocalPlayer)
            {
                Camera.main.transform.SetParent(this.transform);
                Camera.main.transform.localPosition = new Vector3(0, 0.8f, 0); // Позиция глаз
            }
        }

        void FixedUpdate()
        {
            // КРИТИЧЕСКИ ВАЖНО: управляем только своим персонажем
            if (!isLocalPlayer) return;

            // Получаем ввод от игрока
            var x = -Input.GetAxis("Vertical");
            var z = Input.GetAxis("Horizontal");

            // Движение относительно поворота игрока
            var move = transform.right * x + transform.forward * z;
            controller.Move(move * moveSpeed * Time.deltaTime);

            // Простая гравитация
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}