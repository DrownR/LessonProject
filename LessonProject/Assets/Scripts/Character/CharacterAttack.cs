using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRate = 1f;     // seconds between attacks
    [SerializeField] private float attackDuration = 0.2f; // damage zone active time
    [SerializeField] private GameObject damageZone;

    private void Start()
    {
        if (damageZone != null)
            damageZone.SetActive(false);

        InvokeRepeating(nameof(PerformAttack), 0f, attackRate);
    }

    private void PerformAttack()
    {
        if (damageZone == null) return;

        // Enable DamageZone
        damageZone.SetActive(true);

        // Disable after attackDuration
        StartCoroutine(DisableDamageZoneAfterTime(attackDuration));
    }
    private System.Collections.IEnumerator DisableDamageZoneAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        damageZone.SetActive(false);
    }

}
