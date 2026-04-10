using TMPro;
using UnityEngine;
namespace ArrowGame
{
    public class CoinPanel : MonoBehaviour
    {
        public TextMeshProUGUI totalCoinText;

        void UpdateTotalCoins(int coin)
        {
            totalCoinText.text = coin.ToString(); ;
        }
    }
}
