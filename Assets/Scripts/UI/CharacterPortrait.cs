using UnityEngine;

namespace UI
{
    public class CharacterPortrait : ButtonElement
    {
        [SerializeField] private Transform characterTransform;
        [SerializeField] private CameraController cameraController;

        protected override void OnButtonClicked()
        {
            base.OnButtonClicked();
            cameraController.TranslateCamera(characterTransform.position);
        }
    }
}