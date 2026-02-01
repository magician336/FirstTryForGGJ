using UnityEngine;

public class FinalMask : Interactable
{
    [Header("终极皮肤配置")]
    [SerializeField] private PlayerFormType targetForm = PlayerFormType.NormalHead;
    [SerializeField] private int finalSkinIndex = 1; 
    [SerializeField] private bool switchImmediately = true;

    [Header("表现特效")]
    [SerializeField] private GameObject winVfxPrefab;

    // 修改点：去掉参数 (PlayerController player)，保持与基类一致
    public override void TriggerInteract()
    {
        // 通过 GameManager 获取玩家引用
        var player = GameManager.Instance?.Player;
        
        if (player == null)
        {
            Debug.LogError("FinalMask: 找不到玩家引用！");
            return;
        }

        // 1. 解锁这个终极皮肤
        player.ForceUnlockSkin(targetForm, finalSkinIndex);
        
        if (switchImmediately)
        {
            player.SwitchFormWithSkin(targetForm, finalSkinIndex);
        }

        // 2. 播放特效
        if (winVfxPrefab != null)
        {
            Instantiate(winVfxPrefab, transform.position, Quaternion.identity);
        }

        // 3. 判定胜利
        if (Win.Instance != null)
        {
            Win.Instance.Victory();
        }
        else
        {
            Debug.LogError("场景中缺少 [Win] 脚本实例！无法显示胜利界面。");
        }

        // 4. 面具消失
        gameObject.SetActive(false);
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
}