using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class MultiplayerSetup : MonoBehaviour
{
    [MenuItem("GameObject/Setup Second Player")]
    static void SetupSecondPlayer()
    {
        // Find the first player to use as a reference
        GameObject firstPlayer = GameObject.FindObjectOfType<KeyboardController>()?.gameObject;
        
        if (firstPlayer == null)
        {
            Debug.LogError("No first player found with KeyboardController. Please set up the first player first.");
            return;
        }
        
        // Create the second player
        GameObject secondPlayer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        secondPlayer.name = "Player2";
        
        // Position it away from the first player
        secondPlayer.transform.position = firstPlayer.transform.position + new Vector3(3f, 0f, 0f);
        
        // Add the WASD controller
        secondPlayer.AddComponent<WASDController>();
        
        // Add the concentric rings
        ConcentricRings rings = secondPlayer.AddComponent<ConcentricRings>();
        rings.targetSphere = secondPlayer;
        
        // Add the player 2 ring game controller
        Player2RingGameController controller = secondPlayer.AddComponent<Player2RingGameController>();
        controller.concentricRings = rings;
        
        // Create a cube for this player
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Player2Cube";
        cube.transform.position = new Vector3(0, -10, 0); // Hide it initially
        cube.SetActive(false);
        controller.cubeObject = cube;
        
        // Add UI for the second player
        RingGameUI ui = secondPlayer.AddComponent<RingGameUI>();
        
        // Ensure there's a PlayerManager in the scene
        if (FindObjectOfType<PlayerManager>() == null)
        {
            GameObject managerObj = new GameObject("PlayerManager");
            managerObj.AddComponent<PlayerManager>();
        }
        
        Debug.Log("Second player has been set up! Use WASD to move and F to play the ring game.");
    }
}
#endif