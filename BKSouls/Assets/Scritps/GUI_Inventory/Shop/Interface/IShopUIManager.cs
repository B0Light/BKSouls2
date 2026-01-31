using System.Collections.Generic;

namespace BK.Inventory
{
    public interface IShopUIManager
    {
        void OpenShop(List<int> itemIds, Interactable interactable = null);

        void SelectItemToBuy(ItemData selectItem);

        void SearchCategory(int itemType);

        void ShowAllItem();

    }
}