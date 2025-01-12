using Mirage.Logging;
using UnityEngine;

namespace Mirage.Examples.MultipleAdditiveScenes
{
    [RequireComponent(typeof(RandomColor))]
    public class Reward : NetworkBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(Reward));

        public bool available = true;
        public Spawner spawner;
        public RandomColor randomColor;

        void OnValidate()
        {
            if (randomColor == null)
                randomColor = GetComponent<RandomColor>();
        }

        [Server(error = false)]
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ClaimPrize(other.gameObject);
            }
        }

        // This is called from PlayerController.CmdClaimPrize which is invoked by PlayerController.OnControllerColliderHit
        // This only runs on the server
        public void ClaimPrize(GameObject player)
        {
            if (available)
            {
                // This is a fast switch to prevent two players claiming the prize in a bang-bang close contest for it.
                // First hit turns it off, pending the object being destroyed a few frames later.
                available = false;

                Color prizeColor = randomColor.color;

                // calculate the points from the color ... lighter scores higher as the average approaches 255
                // UnityEngine.Color RGB values are float fractions of 255
                uint points = (uint)(((prizeColor.r * 255) + (prizeColor.g * 255) + (prizeColor.b * 255)) / 3);
                if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "Scored {0} points R:{1} G:{2} B:{3}", points, prizeColor.r, prizeColor.g, prizeColor.b);

                // award the points via SyncVar on the PlayerController
                player.GetComponent<PlayerScore>().score += points;

                // spawn a replacement
                spawner.SpawnPrize();

                // destroy this one
                ServerObjectManager.Destroy(gameObject);
            }
        }
    }
}
