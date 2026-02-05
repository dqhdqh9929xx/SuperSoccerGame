using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script debug đơn giản để test xem Input có hoạt động không
/// </summary>
public class SimpleInputTest : MonoBehaviour {
    void Start() {
        Debug.Log("==========================================");
        Debug.Log("[SimpleInputTest] ✅ SCRIPT IS ACTIVE!");
        Debug.Log("[SimpleInputTest] Press ANY KEY to see it logged");
        Debug.Log("[SimpleInputTest] Special test keys: T, Y, U, I, 1, 2");
        Debug.Log("==========================================");
    }
    
    void Update() {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Test bất kỳ phím nào
        if (keyboard.anyKey.wasPressedThisFrame) {
            Debug.Log("[SimpleInputTest] ⚡⚡⚡ SOME KEY WAS PRESSED!");
        }
        
        // Test các phím cụ thể
        if (keyboard.tKey.wasPressedThisFrame) {
            Debug.Log("[SimpleInputTest] ⚡ T KEY PRESSED!");
        }
        
        if (keyboard.yKey.wasPressedThisFrame) {
            Debug.Log("[SimpleInputTest] ⚡ Y KEY PRESSED!");
        }
        
        if (keyboard.uKey.wasPressedThisFrame) {
            Debug.Log("[SimpleInputTest] ⚡ U KEY PRESSED!");
        }
        
        if (keyboard.iKey.wasPressedThisFrame) {
            Debug.Log("[SimpleInputTest] ⚡ I KEY PRESSED!");
        }
        
        if (keyboard.digit1Key.wasPressedThisFrame) {
            Debug.Log("[SimpleInputTest] ⚡ 1 KEY PRESSED!");
        }
        
        if (keyboard.digit2Key.wasPressedThisFrame) {
            Debug.Log("[SimpleInputTest] ⚡ 2 KEY PRESSED!");
        }
    }
}
