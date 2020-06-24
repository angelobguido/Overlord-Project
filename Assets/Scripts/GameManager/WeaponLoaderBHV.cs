using EnemyGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponLoaderBHV : MonoBehaviour
{
    Button button;
    ProjectileTypeSO projectileSO;
    public delegate void LoadWeaponButtonEvent(ProjectileTypeSO projectileSO);
    public static event LoadWeaponButtonEvent loadWeaponButtonEvent;
    public delegate void LoadDifficultySelect();
    public static event LoadDifficultySelect loadDifficultySelect;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnConfirmWeapon);
    }

    protected void OnEnable()
    {
        WeaponSelectionButtonBHV.selectWeaponButtonEvent += PrepareWeapon;
    }

    protected void PrepareWeapon(ProjectileTypeSO projectileSO)
    {
        this.projectileSO = projectileSO;
    }

    void OnConfirmWeapon()
    {
        loadWeaponButtonEvent(projectileSO);
        loadDifficultySelect();
    }
}
