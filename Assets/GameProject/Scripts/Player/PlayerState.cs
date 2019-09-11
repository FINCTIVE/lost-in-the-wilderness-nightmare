using System;
[System.Serializable]
public class PlayerState
{
    private int _hp = 100;
    public event Action<int> OnPlayerHpChange;
    public int hp
    {
        get => _hp;
        set
        {
            _hp = value;
            if (OnPlayerHpChange != null)
                OnPlayerHpChange(_hp);
        }
    }

    public event Action<float> OnPlayerColdRateChange; 
    private float _coldRate = 0;
    public float coldRate
    {
        get => _coldRate;
        set
        {
            _coldRate = value;
            if (OnPlayerColdRateChange != null)
                OnPlayerColdRateChange(_coldRate);
        }
    }

    public float moveSpeed = 7f;
    public bool isDead = false;
}
