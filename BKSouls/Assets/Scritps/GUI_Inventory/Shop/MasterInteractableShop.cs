using UnityEngine;

namespace BK.Inventory
{
    public class MasterInteractableShop : InteractableShop
    {
        protected override void InitializeShop()
        {
            saleItemList.Clear();
            foreach (var item in WorldDatabase_Item.Instance.GetAllItem())
            {
                if (item.itemCode == 0) continue;
                item.purChaseWithItem = true;
                saleItemList.Add(item);
            }
        }

        protected override void EnterShop()
        {
            //InputHandlerManager.Instance.SetInputMode(InputMode.OpenUI);
            GUIController.Instance.OpenShop(saleItemList, this, true);
        }
    }
}