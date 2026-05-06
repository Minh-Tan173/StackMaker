using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpriteSwapButton : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList;

    private Button button;

    private int currentIndex = 0;

    private void Awake() {

        button = GetComponent<Button>();

        // On Init
        SetSpriteTo(spriteList[currentIndex]);

        button.onClick.AddListener(SwapSprite);
    }

    private void SetSpriteTo(Sprite sprite) {
        button.image.sprite = sprite;
    }

    public void SwapSprite() {

        currentIndex = (currentIndex + 1) % spriteList.Count;
        SetSpriteTo(spriteList[currentIndex]);
    }

}
