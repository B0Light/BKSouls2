using UnityEngine;

namespace BK
{
    /// <summary>
    /// 아이스 투사체 프리팹의 런타임 관리자.
    /// IceDamageCollider 에 시전자와 데미지를 주입하고, 충돌 시 파괴 이펙트를 생성합니다.
    /// </summary>
    public class IceSpellManager : SpellManager
    {
        [Header("Collider")]
        public IceDamageCollider damageCollider;

        private bool hasCollided = false;

        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>시전자와 계산된 데미지를 콜라이더에 주입합니다.</summary>
        public void InitializeIceSpell(CharacterManager spellCaster, float calculatedDamage)
        {
            damageCollider.spellCaster = spellCaster;
            damageCollider.magicDamage = calculatedDamage;
        }

        private void OnCollisionEnter(Collision collision)
        {
            //  캐릭터 레이어(6)와 충돌하면 DamageCollider 가 처리하므로 여기서는 무시
            if (collision.gameObject.layer == 6)
                return;

            if (!hasCollided)
            {
                hasCollided = true;
                InstantiateSpellDestructionFX();
            }
        }

        public void InstantiateSpellDestructionFX()
        {
            if (impactParticle != null)
                Instantiate(impactParticle, transform.position, Quaternion.identity);

            WorldSoundFXManager.Instance.AlertNearbyCharactersToSound(transform.position, 6);

            Destroy(gameObject);
        }
    }
}
