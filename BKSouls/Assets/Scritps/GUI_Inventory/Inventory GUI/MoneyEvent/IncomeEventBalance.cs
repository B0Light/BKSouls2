using System;
using System.Collections.Generic;
using UnityEngine;

namespace BK.Inventory
{

    [CreateAssetMenu(menuName = "ThemePark/Income Event Channel Balance")]
    public class IncomeEventBalance : BaseIncomeEvent
    {
        public override void RaiseIncomeEvent(IncomeData incomeData)
        {
            WorldPlayerInventory.Instance.balance.Value += incomeData.incomeAmount;
            base.RaiseIncomeEvent(incomeData);
        }
    }
}