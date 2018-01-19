using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MenuItemAudio : MonoBehaviour, IMoveHandler, ISubmitHandler, IPointerEnterHandler, IPointerDownHandler {

	const string MENU_SELECT_SFX = "menu_select";
	const float MENU_SELECT_SFX_VOLUME = 0.2f;
	const string MENU_CONFIRM_SFX = "menu_confirm";

	void Start() {
		AudioHandler.Load (MENU_SELECT_SFX, MENU_CONFIRM_SFX);
	}

	public void OnMove(AxisEventData eventData) {
		AudioHandler.Play(MENU_SELECT_SFX, MENU_SELECT_SFX_VOLUME);
	}

	public void OnPointerEnter( PointerEventData eventData ) {
		AudioHandler.Play(MENU_SELECT_SFX, MENU_SELECT_SFX_VOLUME);
	}

	public void OnPointerDown( PointerEventData eventData ) {
		AudioHandler.Play(MENU_CONFIRM_SFX);
	}

	public void OnSubmit(BaseEventData eventData) {
		AudioHandler.Play(MENU_CONFIRM_SFX);
	}
}