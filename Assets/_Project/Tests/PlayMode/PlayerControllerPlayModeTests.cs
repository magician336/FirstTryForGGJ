using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerControllerPlayModeTests : MonoBehaviour
{
    private GameObject player;
    private PlayerController playerController;

    [SetUp]
    public void Setup()
    {
        player = new GameObject("Player");
        playerController = player.AddComponent<PlayerController>();
        // Initialize any required components or settings here
    }

    [UnityTest]
    public IEnumerator PlayerCanJumpWhenGrounded()
    {
        // Arrange
        playerController.moveSpeed = 5f;
        playerController.jumpForce = 10f;

        // Simulate being grounded
        playerController.isGrounded = true;

        // Act
        playerController.Jump();

        // Assert
        yield return null; // Wait for the physics to update
        Assert.IsTrue(playerController.rb.velocity.y > 0, "Player should jump when grounded.");
    }

    [UnityTest]
    public IEnumerator PlayerCannotJumpWhenNotGrounded()
    {
        // Arrange
        playerController.isGrounded = false;

        // Act
        playerController.Jump();

        // Assert
        yield return null; // Wait for the physics to update
        Assert.IsTrue(playerController.rb.velocity.y <= 0, "Player should not jump when not grounded.");
    }

    [UnityTest]
    public IEnumerator PlayerCanInteractWithInteractableObject()
    {
        // Arrange
        GameObject interactableObject = new GameObject("Interactable");
        interactableObject.AddComponent<Interactable>(); // Assuming Interactable has a default implementation
        interactableObject.transform.position = player.transform.position + Vector3.right * playerController.interactRange;

        // Act
        playerController.TryInteract();

        // Assert
        // Check if the interactable's interaction method was called (you may need to implement a way to verify this)
        // For example, you could use a mock or a flag in the Interactable class to verify interaction
        yield return null; // Wait for the interaction to process
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(player);
    }
}