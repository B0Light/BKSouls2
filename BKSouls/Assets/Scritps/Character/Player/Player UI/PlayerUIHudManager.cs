using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

namespace BK
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [SerializeField] CanvasGroup[] canvasGroup;

        [Header("Stat Bars")]
        public UI_StatBar healthBar;
        [SerializeField] UI_StatBar staminaBar;
        [SerializeField] UI_StatBar focusPointBar;
        
        [Header("Build Up Bars")]
        [SerializeField] UI_BuildUpBar poisonBuildUpBar;
        [SerializeField] UI_BuildUpBar bleedBuildUpBar;
        [SerializeField] UI_BuildUpBar frostBiteBuildUpBar;
        
        [Header("Runes")]
        [SerializeField] float runeUpdateCountDelayTimer = 2.5f;
        private int pendingRunesToAdd = 0;
        private Coroutine waitThenAddRunesCoroutine;
        [SerializeField] TextMeshProUGUI runesToAddText;
        [SerializeField] TextMeshProUGUI runesCountText;

        [Header("Quick Slots")]
        [SerializeField] Image rightWeaponQuickSlotIcon;
        [SerializeField] Image leftWeaponQuickSlotIcon;
        [SerializeField] Image spellItemQuickSlotIcon;
        [SerializeField] Image quickSlotItemQuickSlotIcon;
        [SerializeField] TextMeshProUGUI quickSlotItemCount;
        [SerializeField] GameObject projectileQuickSlotsGameObject;
        [SerializeField] Image mainProjectileQuickSlotIcon;
        [SerializeField] TextMeshProUGUI mainProjectileCount;
        [SerializeField] Image secondaryProjectileQuickSlotIcon;
        [SerializeField] TextMeshProUGUI secondaryProjectileCount;

        [Header("Spell Quick Slots")]
        [SerializeField] private GameObject spellQuickSlotsGameObject;
        [SerializeField] private Image mainSpellQuickSlotIcon;
        [SerializeField] private Image secondarySpellQuickSlotIcon;

        [Header("Boss Health Bar")]
        public Transform bossHealthBarParent;
        public GameObject bossHealthBarObject;
        [HideInInspector] public UI_Boss_HP_Bar currentBossHealthBar;

        [Header("Crosshair")]
        public GameObject crossHair;

        private void Start()
        {
            if (WorldPlayerInventory.Instance != null)
                WorldPlayerInventory.Instance.OnInventoryChanged += RefreshQuickSlotCount;
        }

        private void OnDestroy()
        {
            if (WorldPlayerInventory.Instance != null)
                WorldPlayerInventory.Instance.OnInventoryChanged -= RefreshQuickSlotCount;
        }

        private void RefreshQuickSlotCount()
        {
            if (GUIController.Instance?.localPlayer == null) return;
            SetQuickSlotItemQuickSlotIcon(GUIController.Instance.localPlayer.playerInventoryManager.currentQuickSlotItem);
        }

        public void ToggleHUD(bool status)
        {
            //  TO DO FADE IN AND OUT OVER TIME

            if (status)
            {
                foreach (var canvas in canvasGroup)
                {
                    canvas.alpha = 1;
                }
            }
            else
            {
                foreach (var canvas in canvasGroup)
                {
                    canvas.alpha = 0;
                }
            }
        }

        public void RefreshHUD()
        {
            healthBar.gameObject.SetActive(false);
            healthBar.gameObject.SetActive(true);
            staminaBar.gameObject.SetActive(false);
            staminaBar.gameObject.SetActive(true);
            focusPointBar.gameObject.SetActive(false);
            focusPointBar.gameObject.SetActive(true);
        }

        public void SetRunesCount(int runesToAdd)
        {
            // 1. ADD THE RUNES WE JUST GOT TO OUR "PENDING" RUNE COUNT (IT CAN GET ADDED ONTO MULTIPLE TIMES)
            pendingRunesToAdd += runesToAdd;

            // 2. WAIT FOR POTENTIALLY MORE RUNES, THEN ADD THEM ALL AFTER X TIME
            if (waitThenAddRunesCoroutine != null)
                StopCoroutine(waitThenAddRunesCoroutine);

            waitThenAddRunesCoroutine = StartCoroutine(WaitThenUpdateRuneCount());
        }

        private IEnumerator WaitThenUpdateRuneCount()
        {
            //  1. WAIT FOR TIMER TO REACH 0 INCASE MORE RUNES ARE QUED UP
            float timer = runeUpdateCountDelayTimer;
            int runesToAdd = pendingRunesToAdd;

            if (runesToAdd >= 0)
            {
                runesToAddText.text = "+ " + runesToAdd.ToString();
            }
            else
            {
                runesToAddText.text = "- " + Mathf.Abs(runesToAdd).ToString();
            }

            runesToAddText.enabled = true;

            while (timer > 0)
            {
                timer -= Time.deltaTime;

                //  2. IF MORE RUNES ARE QUED UP, RE-UPDATE TOTAL NEW RUNE COUNT
                if (runesToAdd != pendingRunesToAdd)
                {
                    runesToAdd = pendingRunesToAdd;
                    runesToAddText.text = "+ " + runesToAdd.ToString();
                }

                yield return null;
            }

            //  3. UPDATE RUNE COUNT, RESET PENDING RUNES AND HIDE PENDING RUNES
            runesToAddText.enabled = false;
            pendingRunesToAdd = 0;
            runesCountText.text = GUIController.Instance.localPlayer.playerStatsManager.runes.ToString();

            yield return null;
        }
        
        public void SetNewPoisonBuildUpAmount(float oldValue, float amount)
        {
            poisonBuildUpBar.SetStat(Mathf.RoundToInt(amount));
        }
        
        public void SetNewBleedBuildUpAmount(float oldValue, float amount)
        {
            bleedBuildUpBar.SetStat(Mathf.RoundToInt(amount));
        }

        public void SetNewFrostBuildUpAmount(float oldValue, float amount)
        {
            frostBiteBuildUpBar.SetStat(Mathf.RoundToInt(amount));
        }

        public void SetMaxBuildUpValue(int buildUpCapacity)
        {
            poisonBuildUpBar.SetMaxStat(buildUpCapacity);
            bleedBuildUpBar.SetMaxStat(buildUpCapacity);
            frostBiteBuildUpBar.SetMaxStat(buildUpCapacity);
        }

        public void SetNewHealthValue(int oldValue, int newValue)
        {
            healthBar.SetStat(newValue);
        }

        public void SetMaxHealthValue(int maxhealth)
        {
            healthBar.SetMaxStat(maxhealth);
        }

        public void SetNewStaminaValue(float oldValue, float newValue)
        {
            staminaBar.SetStat(Mathf.RoundToInt(newValue));
        }

        public void SetMaxStaminaValue(int maxStamina)
        {
            staminaBar.SetMaxStat(maxStamina);
        }

        public void SetNewFocusPointValue(int oldValue, int newValue)
        {
            focusPointBar.SetStat(Mathf.RoundToInt(newValue));
        }

        public void SetMaxFocusPointValue(int maxFocusPoints)
        {
            focusPointBar.SetMaxStat(maxFocusPoints);
        }

        public void SetRightWeaponQuickSlotIcon(int weaponID)
        {
            WeaponItem weapon = WorldItemDatabase.Instance.GetWeaponByID(weaponID);

            if (weapon == null)
            {
                Debug.Log("ITEM IS NULL");
                rightWeaponQuickSlotIcon.enabled = false;
                rightWeaponQuickSlotIcon.sprite = null;
                return;
            }

            if (weapon.itemIcon == null)
            {
                Debug.Log($"ITEM HAS NO ICON : {weapon.name}");
                rightWeaponQuickSlotIcon.enabled = false;
                rightWeaponQuickSlotIcon.sprite = null;
                return;
            }
            
            rightWeaponQuickSlotIcon.sprite = weapon.itemIcon;
            rightWeaponQuickSlotIcon.enabled = true;
        }

        public void SetLeftWeaponQuickSlotIcon(int weaponID)
        {
            WeaponItem weapon = WorldItemDatabase.Instance.GetWeaponByID(weaponID);

            if (weapon == null)
            {
                Debug.Log("ITEM IS NULL");
                leftWeaponQuickSlotIcon.enabled = false;
                leftWeaponQuickSlotIcon.sprite = null;
                return;
            }

            if (weapon.itemIcon == null)
            {
                Debug.Log($"ITEM HAS NO ICON : {weapon.name}");
                leftWeaponQuickSlotIcon.enabled = false;
                leftWeaponQuickSlotIcon.sprite = null;
                return;
            }

            leftWeaponQuickSlotIcon.sprite = weapon.itemIcon;
            leftWeaponQuickSlotIcon.enabled = true;
        }

        public void SetSpellItemQuickSlotIcon(int spellID)
        {
            SpellItem spell = WorldItemDatabase.Instance.GetSpellByID(spellID);

            if (spell == null)
            {
                Debug.Log("ITEM IS NULL");
                spellItemQuickSlotIcon.enabled = false;
                spellItemQuickSlotIcon.sprite = null;
                return;
            }

            if (spell.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                spellItemQuickSlotIcon.enabled = false;
                spellItemQuickSlotIcon.sprite = null;
                return;
            }

            //  THIS IS WHERE YOU WOULD CHECK TO SEE IF YOU MEET THE ITEMS REQUIREMENTS IF YOU WANT TO CREATE THE WARNING FOR NOT BEING ABLE TO WIELD IT IN THE UI

            spellItemQuickSlotIcon.sprite = spell.itemIcon;
            spellItemQuickSlotIcon.enabled = true;
        }

        public void SetSpellSlotIconBySprite(Sprite icon)
        {
            if (icon == null)
            {
                spellItemQuickSlotIcon.enabled = false;
                spellItemQuickSlotIcon.sprite = null;
                return;
            }

            spellItemQuickSlotIcon.sprite = icon;
            spellItemQuickSlotIcon.enabled = true;
        }

        public void SetQuickSlotItemQuickSlotIcon(QuickSlotItem quickSlotItem)
        {
            if (quickSlotItem == null)
            {
                Debug.Log("ITEM IS NULL");
                quickSlotItemQuickSlotIcon.enabled = false;
                quickSlotItemQuickSlotIcon.sprite = null;
                quickSlotItemCount.enabled = false;
                return;
            }

            if (quickSlotItem.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                quickSlotItemQuickSlotIcon.enabled = false;
                quickSlotItemQuickSlotIcon.sprite = null;
                quickSlotItemCount.enabled = false;
                return;
            }

            //  TO DO, UPDATE QUANTITY LEFT, SHOW IN UI
            //  FADE OUT ICON IF NONE REMAINING

            quickSlotItemQuickSlotIcon.sprite = quickSlotItem.itemIcon;
            quickSlotItemQuickSlotIcon.enabled = true;

            if (quickSlotItem.isConsumable)
            {
                quickSlotItemCount.text = quickSlotItem.GetCurrentAmount(GUIController.Instance.localPlayer).ToString();
                quickSlotItemCount.enabled = true;
            }
            else
            {
                quickSlotItemCount.enabled = false;
            }
        }

        public void ToggleProjectileQuickSlotsVisibility(bool status)
        {
            projectileQuickSlotsGameObject.SetActive(status);
        }

        public void ToggleSpellQuickSlotsVisibility(bool status)
        {
            spellQuickSlotsGameObject.SetActive(status);
        }

        public void SetMainSpellQuickSlotIcon(SpellItem spell)
        {
            if (spell == null || spell.itemIcon == null)
            {
                mainSpellQuickSlotIcon.enabled = false;
                mainSpellQuickSlotIcon.sprite = null;
                return;
            }

            mainSpellQuickSlotIcon.sprite = spell.itemIcon;
            mainSpellQuickSlotIcon.enabled = true;
        }

        public void SetSecondarySpellQuickSlotIcon(SpellItem spell)
        {
            if (spell == null || spell.itemIcon == null)
            {
                secondarySpellQuickSlotIcon.enabled = false;
                secondarySpellQuickSlotIcon.sprite = null;
                return;
            }

            secondarySpellQuickSlotIcon.sprite = spell.itemIcon;
            secondarySpellQuickSlotIcon.enabled = true;
        }

        public void SetMainProjectileQuickSlotIcon(RangedProjectileItem projectileItem)
        {
            if (projectileItem == null)
            {
                Debug.Log("ITEM IS NULL");
                mainProjectileQuickSlotIcon.enabled = false;
                mainProjectileQuickSlotIcon.sprite = null;
                mainProjectileCount.enabled = false;
                return;
            }

            if (projectileItem.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                mainProjectileQuickSlotIcon.enabled = false;
                mainProjectileQuickSlotIcon.sprite = null;
                mainProjectileCount.enabled = false;
                return;
            }

            //  TO DO, UPDATE QUANTITY LEFT, SHOW IN UI
            //  FADE OUT ICON IF NONE REMAINING

            mainProjectileQuickSlotIcon.sprite = projectileItem.itemIcon;
            mainProjectileCount.text = projectileItem.currentAmmoAmount.ToString();
            mainProjectileQuickSlotIcon.enabled = true;
            mainProjectileCount.enabled = true;
        }

        public void SetSecondaryProjectileQuickSlotIcon(RangedProjectileItem projectileItem)
        {
            if (projectileItem == null)
            {
                Debug.Log("ITEM IS NULL");
                secondaryProjectileQuickSlotIcon.enabled = false;
                secondaryProjectileQuickSlotIcon.sprite = null;
                secondaryProjectileCount.enabled = false;
                return;
            }

            if (projectileItem.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                secondaryProjectileQuickSlotIcon.enabled = false;
                secondaryProjectileQuickSlotIcon.sprite = null;
                secondaryProjectileCount.enabled = false;
                return;
            }

            //  TO DO, UPDATE QUANTITY LEFT, SHOW IN UI
            //  FADE OUT ICON IF NONE REMAINING

            secondaryProjectileQuickSlotIcon.sprite = projectileItem.itemIcon;
            secondaryProjectileCount.text = projectileItem.currentAmmoAmount.ToString();
            secondaryProjectileQuickSlotIcon.enabled = true;
            secondaryProjectileCount.enabled = true;
        }
    }
}
