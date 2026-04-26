using UnityEngine;

namespace BK.Inventory
{
    public class MasterInteractableShop : InteractableShop
    {
        protected override void InitializeShop()
        {
            ClearSaleItems();
            foreach (var item in WorldItemDatabase.Instance.GetAllItem())
            {
                if (item.itemID == 0) continue;
                saleItemList.Add(item);
            }
            MarkShopInitialized();
        }

        protected override void EnterShop()
        {
            //InputHandlerManager.Instance.SetInputMode(InputMode.OpenUI);
            GUIController.Instance.OpenShop(saleItemList, this, true);
        }
    }
}
