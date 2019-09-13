using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUI : MonoBehaviour
{
    public Slider hpSlider;
    public Image hpFill;
       
    public Color warmBloodColor;
    public Color coldBloodColor;

    void Start()
    {
        PlayerController.player.playerInfo.OnPlayerHpChange += _OnPlayerHpChange;
        PlayerController.player.playerInfo.OnPlayerColdRateChange += _OnPlayerColdRateChange;
    }

    private void _OnPlayerHpChange(int hp)
    {
        hpSlider.value = (float)hp / 100f;
    }

    private void _OnPlayerColdRateChange(float coldRate)
    {
        hpFill.color = Color.Lerp(coldBloodColor, warmBloodColor, coldRate);
    }
}
